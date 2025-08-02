using System;

namespace ProcessorEmulator
{
    public interface IEmulator
    {
        // Core emulation methods
        void LoadBinary(byte[] binary, uint loadAddress);
        void Run();
        void Step();
        
        // State access for debugging/display
        uint ProgramCounter { get; set; }
        uint StackPointer { get; set; }
        int InstructionCount { get; }
        uint CurrentInstruction { get; }
        uint[] RegisterState { get; }
        byte[] MemoryState { get; }

        // Memory and device management
        void MapMemory(uint address, byte[] data);
        void RegisterDevice(IDeviceEmulator device);
    }

    public interface IDeviceEmulator
    {
        uint BaseAddress { get; }
        uint Size { get; }
        uint Read(uint address);
        void Write(uint address, uint value);
    }
}
