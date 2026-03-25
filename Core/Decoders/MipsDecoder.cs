using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Decoders
{
    /// <summary>
    /// IR decoder wrapper for the existing MIPS decode pipeline.
    /// </summary>
    public class MipsDecoder : IInstructionDecoder
    {
        public IEnumerable<IrStatement> Decode(ulong address, IMemoryManager memory)
        {
            // Use existing MIPS instruction decoder for raw instruction decoding.
            uint instructionWord = memory.ReadMemory32(address);
            var instr = ProcessorEmulator.Core.MipsDecoder.Decode(instructionWord);

            // A simple placeholder conversion from instruction to IR statement.
            // This can be expanded over time with full IR translation rules.
            var ir = new IrStatement
            {
                Op = IrOpCode.Copy,
                Destination = new IrOperand { RegisterName = "r0", Width = BitWidth.Bits32, IsImmediate = false },
                SourceA = new IrOperand { RegisterName = "r0", Width = BitWidth.Bits32, IsImmediate = false },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = 0, IsImmediate = true },
                SourceC = new IrOperand { Width = BitWidth.Bits32, Value = 0, IsImmediate = true },
                Metadata = 0
            };

            return new[] { ir };
        }
    }
}
