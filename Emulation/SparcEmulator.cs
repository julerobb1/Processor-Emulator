namespace ProcessorEmulator.Emulation
{
    public class SparcEmulator : IEmulator
    {
        public SparcEmulator() { }

        public void LoadBinary(byte[] binary) { /* stub */ }
        public void Run() { /* stub */ }
        public void Step() { /* stub */ }
        public void Decompile() { /* stub */ }
        public void Recompile(string code) { /* stub */ }
        
        // IEmulator properties
        public uint ProgramCounter { get; private set; } = 0;
        public int InstructionCount { get; private set; } = 0;
        public uint CurrentInstruction { get; private set; } = 0;
        public uint[] RegisterState { get; private set; } = new uint[32];
        public byte[] MemoryState { get; private set; } = new byte[1024];
    }
}
