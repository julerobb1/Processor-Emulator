namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Represents a decoded MIPS instruction, providing easy access to its component parts.
    /// </summary>
    public readonly struct MipsInstruction
    {
        public readonly uint Raw;

        // Common fields for all instruction types
        public readonly uint Opcode; // Bits 31-26

        // R-Type fields
        public readonly uint Rs;     // Bits 25-21
        public readonly uint Rt;     // Bits 20-16
        public readonly uint Rd;     // Bits 15-11
        public readonly uint Shamt;  // Bits 10-6
        public readonly uint Funct;  // Bits 5-0

        // I-Type fields
        public readonly ushort Imm;   // Bits 15-0 (unsigned)
        public readonly int ImmSigned; // Bits 15-0 (signed)

        // J-Type fields
        public readonly uint Addr;   // Bits 25-0

        public MipsInstruction(uint instruction)
        {
            Raw = instruction;

            Opcode = instruction >> 26;
            Rs = (instruction >> 21) & 0x1F;
            Rt = (instruction >> 16) & 0x1F;
            Rd = (instruction >> 11) & 0x1F;
            Shamt = (instruction >> 6) & 0x1F;
            Funct = instruction & 0x3F;
            Imm = (ushort)(instruction & 0xFFFF);
            ImmSigned = (short)Imm;
            Addr = instruction & 0x03FFFFFF;
        }

        public override string ToString()
        {
            return $"Op: {Opcode:X2}, Rs: {Rs}, Rt: {Rt}, Rd: {Rd}, Funct: {Funct:X2}, Imm: {Imm:X4}";
        }
    }
}
