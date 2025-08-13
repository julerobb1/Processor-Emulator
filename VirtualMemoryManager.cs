using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    /// <summary>
    /// Virtual Memory Manager for Windows CE process emulation
    /// </summary>
    public class VirtualMemoryManager
    {
        private readonly Dictionary<uint, MemoryRegion> regions;
        private readonly byte[] mainMemory;
        private const uint MaxMemorySize = 0x80000000; // 2GB address space

        public VirtualMemoryManager()
        {
            regions = new Dictionary<uint, MemoryRegion>();
            mainMemory = new byte[MaxMemorySize];
        }

        public void MapRegion(uint baseAddress, uint size, MemoryProtection protection)
        {
            var region = new MemoryRegion
            {
                BaseAddress = baseAddress,
                Size = size,
                Protection = protection,
                IsMapped = true
            };

            regions[baseAddress] = region;
            Console.WriteLine($"  🗺️ Mapped 0x{baseAddress:X8}-0x{baseAddress + size:X8} ({size:N0} bytes, {protection})");
        }

        public void UnmapRegion(uint baseAddress)
        {
            if (regions.ContainsKey(baseAddress))
            {
                regions.Remove(baseAddress);
                Console.WriteLine($"  🚫 Unmapped region at 0x{baseAddress:X8}");
            }
        }

        private MemoryRegion FindRegion(uint address)
        {
            foreach (var region in regions.Values)
            {
                if (address >= region.BaseAddress && address < region.BaseAddress + region.Size)
                    return region;
            }
            return null;
        }

        private void ValidateAccess(uint address, MemoryProtection requiredProtection)
        {
            var region = FindRegion(address);
            if (region == null || !region.IsMapped)
                throw new MemoryAccessException(address, "Access to unmapped memory");

            if ((region.Protection & requiredProtection) == 0)
                throw new MemoryAccessException(address, $"Access violation: required {requiredProtection}, have {region.Protection}");
        }

        public byte ReadByte(uint address)
        {
            ValidateAccess(address, MemoryProtection.Read);
            if (address >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            return mainMemory[address];
        }

        public void WriteByte(uint address, byte value)
        {
            ValidateAccess(address, MemoryProtection.Write);
            if (address >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            mainMemory[address] = value;
        }

        public ushort ReadUInt16(uint address)
        {
            ValidateAccess(address, MemoryProtection.Read);
            if (address + 1 >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            return BitConverter.ToUInt16(mainMemory, (int)address);
        }

        public void WriteUInt16(uint address, ushort value)
        {
            ValidateAccess(address, MemoryProtection.Write);
            if (address + 1 >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, mainMemory, address, 2);
        }

        public uint ReadUInt32(uint address)
        {
            ValidateAccess(address, MemoryProtection.Read);
            if (address + 3 >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            return BitConverter.ToUInt32(mainMemory, (int)address);
        }

        public void WriteUInt32(uint address, uint value)
        {
            ValidateAccess(address, MemoryProtection.Write);
            if (address + 3 >= MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, mainMemory, address, 4);
        }

        public byte[] ReadBytes(uint address, uint count)
        {
            ValidateAccess(address, MemoryProtection.Read);
            if (address + count > MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            
            var buffer = new byte[count];
            Array.Copy(mainMemory, address, buffer, 0, count);
            return buffer;
        }

        public void WriteBytes(uint address, byte[] data)
        {
            if (data == null || data.Length == 0) return;
            
            ValidateAccess(address, MemoryProtection.Write);
            if (address + data.Length > MaxMemorySize)
                throw new MemoryAccessException(address, "Address out of range");
            
            Array.Copy(data, 0, mainMemory, address, data.Length);
        }

        public void WriteString(uint address, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(text + '\0');
            WriteBytes(address, bytes);
        }

        public string ReadString(uint address, uint maxLength = 256)
        {
            var bytes = new List<byte>();
            for (uint i = 0; i < maxLength; i++)
            {
                var b = ReadByte(address + i);
                if (b == 0) { break; }
                bytes.Add(b);
            }
            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }

        public void DumpMemory(uint address, uint count)
        {
            Console.WriteLine($"\n🔍 Memory dump at 0x{address:X8}:");
            for (uint i = 0; i < count; i += 16)
            {
                var lineAddr = address + i;
                var hex = "";
                var ascii = "";
                
                for (uint j = 0; j < 16 && i + j < count; j++)
                {
                    try
                    {
                        var b = ReadByte(lineAddr + j);
                        hex += $"{b:X2} ";
                        ascii += (b >= 32 && b <= 126) ? (char)b : '.';
                    }
                    catch
                    {
                        hex += "?? ";
                        ascii += '?';
                    }
                }
                
                Console.WriteLine($"  {lineAddr:X8}: {hex.PadRight(48)} {ascii}");
            }
        }

        public uint AllocateMemory(uint size, MemoryProtection protection = MemoryProtection.ReadWrite)
        {
            // Simple allocator - find free space
            uint baseAddr = 0x20000000; // Start allocation from this address
            
            while (baseAddr + size < MaxMemorySize)
            {
                if (FindRegion(baseAddr) == null)
                {
                    MapRegion(baseAddr, size, protection);
                    return baseAddr;
                }
                baseAddr += 0x10000; // Try next 64KB boundary
            }
            
            throw new OutOfMemoryException("Cannot allocate virtual memory");
        }

        public void FreeMemory(uint address)
        {
            if (address != 0) UnmapRegion(address);
        }
    }

    public class MemoryRegion
    {
        public uint BaseAddress { get; set; }
        public uint Size { get; set; }
        public MemoryProtection Protection { get; set; }
        public bool IsMapped { get; set; }
    }
}
