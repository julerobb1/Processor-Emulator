using System;
using System.Collections.Generic;
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
        }
        
        public uint Translate(uint vaddr)
        {
            // kseg0 (0x80000000 - 0x9FFFFFFF) and kseg1 (0xA0000000 - 0xBFFFFFFF)
            // are unmapped (direct mapped) physical memory regions.
            if ((vaddr & 0xE0000000) == 0x80000000 || (vaddr & 0xE0000000) == 0xA0000000)
            {
                return vaddr & 0x1FFFFFFF; // Direct map to lower 512MB physical
            }

            // For any other segment (kuseg, kseg2), we need to use the TLB.
            // This is a placeholder for a full TLB search.
            return PerformTlbLookup(vaddr);
        }

        private uint PerformTlbLookup(uint vaddr)
        {
            // In a real MIPS CPU, this would search all TLB entries.
            // For now, we simulate a very basic single-entry TLB check.
            const uint PageMask4KB = 0xFFFFF000;
            uint vpn2 = (vaddr >> 13); // Virtual Page Number (for 8KB pages)
            
            // Simplified check against EntryHi
            if ((_cp0.EntryHi & PageMask4KB) == (vaddr & PageMask4KB))
            {
                 // Simplified check against EntryLo0/1 based on page
                 uint pageOffset = vaddr & 0x0FFF;
                 uint pfn; // Physical Frame Number
                 if ((vaddr & 0x1000) == 0) // Check if it's the even page of a pair
                 {
                     pfn = (_cp0.EntryLo0 & 0x3FFFFC0) << 6;
                 }
                 else
                 {
                     pfn = (_cp0.EntryLo1 & 0x3FFFFC0) << 6;
                 }
                 return pfn | pageOffset;
            }

            // If we get here, no valid translation was found in the TLB.
            // This should trigger a TLB Miss exception for the CPU to handle.
            throw new TlbMissException($"TLB Miss for virtual address 0x{vaddr:X8}", vaddr);
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
            uint paddr = Translate(vaddr);
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
            uint paddr = Translate(vaddr);
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
            uint paddr = Translate(vaddr);
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
            uint paddr = Translate(vaddr);
            IBusDevice device = _lookupTable[paddr >> 16];

            if (device != null)
            {
                device.Write8(paddr - device.StartAddress, value);
                return;
            }
            throw new AddressErrorException($"Write to unmapped physical address 0x{paddr:X8}");
        }
    }
}