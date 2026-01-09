using System;

namespace ProcessorEmulator.Emulation
{
    public class MipsBus
    {
        private readonly byte[] _physicalMemory;
        private readonly uint _memorySize;

        public MipsBus(uint sizeMb)
        {
            _memorySize = sizeMb * 1024 * 1024;
            _physicalMemory = new byte[_memorySize];
        }

        // This is the core logic we discussed
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

        public uint Read32(uint vaddr)
        {
            uint paddr = Translate(vaddr);
            if (paddr + 4 > _memorySize) return 0; // Or throw Bus Error

            // Little-Endian Read
            return (uint)(_physicalMemory[paddr] |
                         (_physicalMemory[paddr + 1] << 8) |
                         (_physicalMemory[paddr + 2] << 16) |
                         (_physicalMemory[paddr + 3] << 24));
        }
        
        public byte Read8(uint vaddr)
        {
            uint paddr = Translate(vaddr);
            if (paddr + 1 > _memorySize) return 0; // Or throw Bus Error
            return _physicalMemory[paddr];
        }


        public void Write32(uint vaddr, uint value)
        {
            uint paddr = Translate(vaddr);
            if (paddr + 4 > _memorySize) return;

            // Little-Endian Write
            _physicalMemory[paddr] = (byte)(value & 0xFF);
            _physicalMemory[paddr + 1] = (byte)((value >> 8) & 0xFF);
            _physicalMemory[paddr + 2] = (byte)((value >> 16) & 0xFF);
            _physicalMemory[paddr + 3] = (byte)((value >> 24) & 0xFF);
        }

        // Helper for loading your nk.bin
        public void LoadRawBinary(uint paddr, byte[] data)
        {
            Array.Copy(data, 0, _physicalMemory, paddr, data.Length);
        }
    }
}
