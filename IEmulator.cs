using System;

namespace ProcessorEmulator.Emulation
{
    public interface IEmulator
    {
        // Core emulation methods
        void LoadBinary(byte[] binary);
        void Run();
        void Step();
        
        // State access for debugging/display
        uint ProgramCounter { get; }
        int InstructionCount { get; }
        uint CurrentInstruction { get; }
        uint[] RegisterState { get; }
        byte[] MemoryState { get; }
    }
}
