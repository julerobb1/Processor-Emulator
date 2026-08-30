using System;
using ProcessorEmulator.Core;

namespace ProcessorEmulator.Emulation
{
    /// <summary>
    /// Manages the MIPS address space, routing memory accesses to the correct devices.
    /// This implementation uses a fast lookup table for O(1) device discovery.
    /// </summary>
    public class MipsBus
    {
        // A lookup table mapping the upper 16 bits of the address space to a device.
        // This gives 65,536 entries, each covering a 64KB chunk of the 4GB address space.
        private readonly IBusDevice[] _lookupTable = new IBusDevice[1 << 16];
        private readonly System.Collections.Generic.List<IBusDevice> _devices = new System.Collections.Generic.List<IBusDevice>();

        private readonly CP0 _cp0;
        public bool IsBigEndian { get; set; } = true; // Default to Big Endian for MIPS set-top boxes

        public MipsBus(CP0 cp0)
        {
            _cp0 = cp0;
        }

        /// <summary>
        /// Maps a device to a specific range of the address space.
        /// </summary>
        public void AddDevice(IBusDevice device)
        {
            // Determine the start and end indices in the lookup table
            uint startIdx = device.StartAddress >> 16;
            uint endIdx = (device.StartAddress + device.Size - 1) >> 16;

            for (uint i = startIdx; i <= endIdx; i++)
            {
                if (_lookupTable[i] != null)
                {
                    throw new InvalidOperationException($"Address space conflict. Block 0x{i:X4}0000 is already mapped.");
                }
                _lookupTable[i] = device;
            }
            _devices.Add(device);
        }

        public void Tick(int cycles)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] is BcmSysControlRegs sysctl)
                    sysctl.Tick(cycles);
            }
        }
        
        public uint Translate(uint vaddr)
        {
            return Translate(vaddr, isStore: false);
        }

        public uint Translate(uint vaddr, bool isStore)
        {
            // kseg0 (0x80000000 - 0x9FFFFFFF) and kseg1 (0xA0000000 - 0xBFFFFFFF)
            // are unmapped (direct mapped) physical memory regions.
            if ((vaddr & 0xE0000000) == 0x80000000 || (vaddr & 0xE0000000) == 0xA0000000)
            {
                return vaddr & 0x1FFFFFFF; // Direct map to lower 512MB physical
            }

            return PerformTlbLookup(vaddr, isStore);
        }

        private uint PerformTlbLookup(uint vaddr, bool isStore)
        {
            CP0.TlbTranslateStatus status = _cp0.TryTranslate(vaddr, out uint paddr);
            if (status == CP0.TlbTranslateStatus.Hit)
                return paddr;

            bool invalid = status == CP0.TlbTranslateStatus.Invalid;
            string kind = invalid ? "TLB Invalid" : "TLB Miss";
            throw new TlbMissException($"{kind} for virtual address 0x{vaddr:X8}", vaddr, isStore, invalid);
        }

        private static uint Swap(uint value)
        {
            return ((value & 0xFF000000) >> 24) |
                   ((value & 0x00FF0000) >> 8) |
                   ((value & 0x0000FF00) << 8) |
                   ((value & 0x000000FF) << 24);
        }

        public uint Read32(uint vaddr)
        {
            vaddr = CeRomTocFiles.MapDdiNopDestVa(vaddr);
            vaddr = CeRomTocFiles.MapProcessHeapSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapCoredllSharedVa(this, vaddr);
            vaddr = CeRomTocFiles.MapFirmwareSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapVallocHostVa(vaddr);
            vaddr = CeRomTocFiles.MapExeXipVa(this, vaddr);
            uint paddr = Translate(vaddr, isStore: false);
            IBusDevice device = _lookupTable[paddr >> 16];

            if (device != null)
            {
                uint val = device.Read32(paddr - device.StartAddress);
                return IsBigEndian ? Swap(val) : val;
            }
            throw new AddressErrorException($"Read from unmapped physical address 0x{paddr:X8}");
        }

        public void Write32(uint vaddr, uint value)
        {
            HostHardDisk.NoteDispC8Write(vaddr, value, this);
            vaddr = CeRomTocFiles.MapDdiNopDestVa(vaddr);
            vaddr = CeRomTocFiles.MapProcessHeapSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapCoredllSharedVa(this, vaddr);
            vaddr = CeRomTocFiles.MapFirmwareSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapVallocHostVa(vaddr);
            uint paddr = Translate(vaddr, isStore: true);
            IBusDevice device = _lookupTable[paddr >> 16];

            if (device != null)
            {
                uint valueToStore = IsBigEndian ? Swap(value) : value;
                device.Write32(paddr - device.StartAddress, valueToStore);
                return;
            }
            throw new AddressErrorException($"Write to unmapped physical address 0x{paddr:X8}");
        }
        
        public byte Read8(uint vaddr)
        {
            vaddr = CeRomTocFiles.MapDdiNopDestVa(vaddr);
            vaddr = CeRomTocFiles.MapProcessHeapSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapCoredllSharedVa(this, vaddr);
            vaddr = CeRomTocFiles.MapFirmwareSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapVallocHostVa(vaddr);
            vaddr = CeRomTocFiles.MapExeXipVa(this, vaddr);
            uint paddr = Translate(vaddr, isStore: false);
            IBusDevice device = _lookupTable[paddr >> 16];

            if (device != null)
            {
                // For byte access, endianness of the byte itself doesn't matter,
                // but the address translation must be correct.
                return device.Read8(paddr - device.StartAddress);
            }
            throw new AddressErrorException($"Read from unmapped physical address 0x{paddr:X8}");
        }

        public void Write8(uint vaddr, byte value)
        {
            HostHardDisk.NoteDispC8Write(vaddr, value, this);
            vaddr = CeRomTocFiles.MapDdiNopDestVa(vaddr);
            vaddr = CeRomTocFiles.MapProcessHeapSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapCoredllSharedVa(this, vaddr);
            vaddr = CeRomTocFiles.MapFirmwareSlotVa(this, vaddr);
            vaddr = CeRomTocFiles.MapVallocHostVa(vaddr);
            uint paddr = Translate(vaddr, isStore: true);
            IBusDevice device = _lookupTable[paddr >> 16];

            if (device != null)
            {
                device.Write8(paddr - device.StartAddress, value);
                return;
            }
            throw new AddressErrorException($"Write to unmapped physical address 0x{paddr:X8}");
        }

        public void WriteBytes(uint vaddr, byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            for (uint i = 0; i < data.Length; i++)
            {
                Write8(vaddr + i, data[i]);
            }
        }
    }
}