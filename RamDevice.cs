using System;

namespace ProcessorEmulator.Emulation
{
    public class RamDevice : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size { get; }
        private readonly byte[] _memory;

        public RamDevice(uint startAddress, uint size)
        {
            StartAddress = startAddress;
            Size = size;
            _memory = new byte[size];
        }

        public uint Read32(uint offset)
        {
            if (offset + 4 > Size) return 0; // Or throw
            return (uint)(_memory[offset] |
                          (_memory[offset + 1] << 8) |
                          (_memory[offset + 2] << 16) |
                          (_memory[offset + 3] << 24));
        }

        public void Write32(uint offset, uint value)
        {
            if (offset + 4 > Size) return; // Or throw
            _memory[offset] = (byte)(value & 0xFF);
            _memory[offset + 1] = (byte)((value >> 8) & 0xFF);
            _memory[offset + 2] = (byte)((value >> 16) & 0xFF);
            _memory[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        
        public byte Read8(uint offset)
        {
            if (offset >= Size) return 0;
            return _memory[offset];
        }

        public void Write8(uint offset, byte value)
        {
            if (offset >= Size) return;
            _memory[offset] = value;
        }

        public void LoadData(uint offset, byte[] data)
        {
            Array.Copy(data, 0, _memory, offset, data.Length);
        }
    }
}
