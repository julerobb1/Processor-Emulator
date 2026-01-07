using System;
using System.Collections.Generic;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Decoders
{
    public class MipsDecoder : IInstructionDecoder
    {
        public IEnumerable<IrStatement> Decode(ulong address, ReadOnlySpan<byte> code)
        {
            // MIPS instructions are always 4 bytes (32-bit)
            if (code.Length < 4)
            {
                throw new ArgumentException("MIPS instruction stream requires at least 4 bytes.", nameof(code));
            }
            
            uint instruction = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(code);
            uint opcode = (instruction >> 26) & 0x3F;

            var statements = new List<IrStatement>();

            switch (opcode)
            {
                case 0x08: // ADDI
                    statements.Add(DecodeAddImmediate(instruction));
                    break;
                
                default:
                    // Rule: No silent failures. No fake execution.
                    throw new Exception($"Illegal Instruction: Opcode 0x{opcode:X2} at 0x{address:X16}");
            }

            return statements;
        }

        private IrStatement DecodeAddImmediate(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            short imm = (short)(instr & 0xFFFF); // Use 'short' to force C# to handle the sign bit

            // Correctly use object initializers for the IrOperand struct
            return new IrStatement
            {
                Op = IrOpCode.Add,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = (ulong)(long)imm, IsImmediate = true },
                Metadata = 0 // User-mode safe
            };
        }
    }
}
