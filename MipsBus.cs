using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessorEmulator.Emulation
{
    public class MipsBus
    {
        private readonly List<IBusDevice> _devices = new List<IBusDevice>();
        private readonly CP0 _cp0;
        public bool IsBigEndian { get; set; } = false;

        // Parameterless ctor for legacy callers that expect `new MipsBus()`
        public MipsBus()
        {
            var cp0 = new CP0();
            _cp0 = cp0;
        }

        public MipsBus(CP0 cp0)
        {
            _cp0 = cp0;
        }

        // Convenience constructor used by older code paths that pass a RAM size.
        public MipsBus(uint ramSize)
        {
            var cp0 = new CP0();
            _cp0 = cp0;
            // Create a RAM region starting at physical 0 of the provided size
            AddDevice(new RamDevice(0, ramSize));
        }

        public void AddDevice(IBusDevice device) => _devices.Add(device);

        // Expose devices collection for legacy callers
        public IEnumerable<IBusDevice> Devices => _devices;

        public uint Translate(uint vaddr)
        {
            // kseg0 (0x80000000 - 0x9FFFFFFF) and kseg1 (0xA0000000 - 0xBFFFFFFF)
            // are unmapped (direct mapped) physical memory regions.
            // No TLB lookup for these segments.
            if ((vaddr >= 0x80000000 && vaddr <= 0x9FFFFFFF) || // kseg0
                (vaddr >= 0xA0000000 && vaddr <= 0xBFFFFFFF))  // kseg1
            {
                return vaddr & 0x1FFFFFFF; // Direct map to lower 512MB physical
            }

            // kuseg (0x00000000 - 0x7FFFFFFF) - requires TLB lookup
            // kseg2 (0xC0000000 - 0xDFFFFFFF) - requires TLB lookup
            // kseg3 (0xE0000000 - 0xFFFFFFFF) - requires TLB lookup

            // For now, let's focus on kuseg and a very simplified TLB lookup
            // This is a placeholder for a proper multi-entry TLB
            // We assume EntryHi holds the VPN and EntryLo0/1 hold the PPNs

            // Simplified: Assume 4KB page size for now
            const uint PageSize = 4 * 1024; // 4KB
            const uint PageMask = ~(PageSize - 1); // Mask for page address

            uint vpn = (vaddr & PageMask); // Virtual Page Number
            uint pageOffset = (vaddr & (PageSize - 1)); // Offset within page

            // Check if the current vaddr matches the EntryHi VPN
            // In a real TLB, we'd search through multiple entries.
            // For simplicity, we are assuming EntryHi/Lo registers define the *current* active translation.
            if ((_cp0.EntryHi & PageMask) == vpn)
            {
                // Check if it's an even or odd page
                // This is simplified and assumes a 2-page entry as defined by EntryLo0/EntryLo1
                // based on MIPS architecture which typically uses odd/even pages for a single TLB entry
                if ((vaddr & PageSize) == 0) // Even page
                {
                    // Check valid and dirty bits if necessary
                    // For now, assume valid. PPN is bits 6-31 of EntryLo0
                    uint ppn = (_cp0.EntryLo0 & PageMask);
                    return ppn | pageOffset;
                }
                else // Odd page
                {
                    uint ppn = (_cp0.EntryLo1 & PageMask);
                    return ppn | pageOffset;
                }
            }
            
            // If no TLB match, or for other segments, for now, treat as direct map.
            // In a full implementation, this would trigger a TLB Miss exception.
            return vaddr;
        }

        private uint Swap(uint value)
        {
            return ((value & 0xFF000000) >> 24) |
                   ((value & 0x00FF0000) >> 8) |
                   ((value & 0x0000FF00) << 8) |
                   ((value & 0x000000FF) << 24);
        }

        public uint Read32(uint vaddr)
        {
            uint paddr = Translate(vaddr);
            var device = _devices.FirstOrDefault(d => paddr >= d.StartAddress && paddr < d.StartAddress + d.Size);

            if (device != null)
            {
                uint val = device.Read32(paddr - device.StartAddress);
                // The value is read from the device in little-endian format (as C# memory is LE).
                // If the GUEST is big-endian, we need to swap the bytes to present it correctly to the CPU.
                return IsBigEndian ? Swap(val) : val;
            }
            return 0; // Bus Error / Silent Fail
        }

        public void Write32(uint vaddr, uint value)
        {
            uint paddr = Translate(vaddr);
            var device = _devices.FirstOrDefault(d => paddr >= d.StartAddress && paddr < d.StartAddress + d.Size);

            if (device != null)
            {
                // The value from the CPU is in the guest's endianness.
                // If the guest is big-endian, we need to swap it to little-endian before writing to our C# memory.
                uint valueToStore = IsBigEndian ? Swap(value) : value;
                device.Write32(paddr - device.StartAddress, valueToStore);
            }
        }
        
        public byte Read8(uint vaddr)
        {
            uint paddr = Translate(vaddr);
            var device = _devices.FirstOrDefault(d => paddr >= d.StartAddress && paddr < d.StartAddress + d.Size);
            
            if (device != null)
            {
                return device.Read8(paddr - device.StartAddress);
            }
            return 0;
        }

        public void Write8(uint vaddr, byte value)
        {
            uint paddr = Translate(vaddr);
            var device = _devices.FirstOrDefault(d => paddr >= d.StartAddress && paddr < d.StartAddress + d.Size);

            if (device != null)
            {
                device.Write8(paddr - device.StartAddress, value);
            }
        }

        // This is a helper to load data into a specific device, e.g. loading a ROM into a RAM device.
        public void LoadData(uint paddr, byte[] data)
        {
            var device = _devices.FirstOrDefault(d => paddr >= d.StartAddress && paddr < d.StartAddress + d.Size);
            if (device is RamDevice ram)
            {
                ram.LoadData(paddr - ram.StartAddress, data);
            }
            else
            {
                // Or handle other device types that can be loaded
                throw new Exception($"Cannot load data into device at address 0x{paddr:X8}");
            }
        }

        // Compatibility helper: write a sequence of bytes to a virtual address.
        // Some callers (boot managers) expect a WriteBytes method to bulk-load images.
        public void WriteBytes(uint vaddr, byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            for (int i = 0; i < data.Length; i++)
            {
                Write8(vaddr + (uint)i, data[i]);
            }
        }
    }
}