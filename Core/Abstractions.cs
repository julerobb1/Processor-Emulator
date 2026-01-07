namespace ProcessorEmulator.Core.Emulation
{
    /// <summary>
    /// Defines the bit-width for an operand or operation.
    /// </summary>
    public enum BitWidth
    {
        Bits8,
        Bits16,
        Bits32,
        Bits64
    }

    /// <summary>
    /// Defines the canonical, ISA-agnostic operations for the IR.
    /// </summary>
    public enum IrOpCode
    {
        Add,
        Sub,
        And,
        Or,
        Store
    }

    /// <summary>
    /// Represents an operand in an IR statement.
    /// </summary>
    public struct IrOperand
    {
        public BitWidth Width { get; set; }
        public ulong Value { get; set; }
        public string RegisterName { get; set; }
        public bool IsImmediate { get; set; }
    }

    /// <summary>
    /// Represents a single canonical instruction statement.
    /// </summary>
    public struct IrStatement
    {
        public IrOpCode Op;
        public ulong Metadata; // Bit 0 = IsPrivileged
        public IrOperand Destination;
        public IrOperand SourceA;
        public IrOperand SourceB;
    }

    /// <summary>
    /// Defines the interface for the emulated CPU state.
    /// </summary>
    public interface ICpuState
    {
        int PrivilegeLevel { get; }
        ulong PC { get; set; }
        void WriteMemory8(ulong address, byte value);
        ulong GetRegister(string name, BitWidth width);
        void SetRegister(string name, ulong value, BitWidth width);
    }

    /// <summary>
    /// Defines the interface for an execution backend.
    /// </summary>
    public interface IExecutionEngine
    {
        void ExecuteStatement(IrStatement statement, ICpuState state);
    }

    /// <summary>
    /// Defines the interface for an instruction decoder.
    /// </summary>
    public interface IInstructionDecoder
    {
        System.Collections.Generic.IEnumerable<IrStatement> Decode(ulong address, System.ReadOnlySpan<byte> code);
    }
}