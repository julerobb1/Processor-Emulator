using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessorEmulator.Emulation
{
    public class MipsBus
    {
        private readonly List<IBusDevice> _devices = new List<IBusDevice>();
        public bool IsBigEndian { get; set; } = false;

        public void AddDevice(IBusDevice device) => _devices.Add(device);

        public uint Translate(uint vaddr)
        {
            // kseg0 & kseg1 both map to the first 512MB of physical memory
            if (vaddr >= 0x80000000 && vaddr <= 0xBFFFFFFF)
            {
                return vaddr & 0x1FFFFFFF;
            }

            // kuseg (0x00000000 - 0x7FFFFFFF)
            // For now, we treat this as a direct map until we finish the TLB
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
    }
}