using System;

namespace ProcessorEmulator.Emulation
{
    public class BcmSysControlRegs : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size { get; }
        private readonly uint[] _regs;

        public BcmSysControlRegs(uint startAddress = 0x10400000, uint size = 0x1000)
        {
            StartAddress = startAddress;
            Size = size;
            _regs = new uint[size / 4];
        }

        public uint Read32(uint offset)
        {
            if (offset + 4 > Size)
                return 0;
            uint value = _regs[offset / 4];
            Console.WriteLine($"[SYSCTL] Read 0x{StartAddress + offset:X8} = 0x{value:X8}");
            return value;
        }

        public void Write32(uint offset, uint value)
        {
            if (offset + 4 > Size)
                return;
            _regs[offset / 4] = value;
            Console.WriteLine($"[SYSCTL] Write 0x{StartAddress + offset:X8} = 0x{value:X8}");
        }

        public byte Read8(uint offset)
        {
            uint aligned = offset & ~3u;
            uint word = Read32(aligned);
            return (byte)(word >> (int)((offset & 3) * 8));
        }

        public void Write8(uint offset, byte value)
        {
            uint aligned = offset & ~3u;
            uint shift = (offset & 3) * 8;
            uint word = (aligned + 4 <= Size) ? _regs[aligned / 4] : 0;
            word = (word & ~(0xFFu << (int)shift)) | ((uint)value << (int)shift);
            Write32(aligned, word);
        }
    }
}
