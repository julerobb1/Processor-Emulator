using System;

namespace ProcessorEmulator
{
    public class SparcEmulator : IEmulator
    {
        public SparcEmulator() { }

        public void LoadBinary(byte[] binary, uint loadAddress) { /* stub */ }
        public void Run() { /* stub */ }
        public void Step() { /* stub */ }
        
        public uint ProgramCounter { get; set; } = 0;
        public uint StackPointer { get; set; } = 0;
        public int InstructionCount { get; private set; } = 0;
        public uint CurrentInstruction { get; private set; } = 0;
        public uint[] RegisterState { get; private set; } = new uint[32];
        public byte[] MemoryState { get; private set; } = new byte[1024];
        public void MapMemory(uint address, byte[] data) { /* stub */ }
        public void RegisterDevice(IDeviceEmulator device) { /* stub */ }
    }
}
