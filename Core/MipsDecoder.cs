using System;
using System.Collections.Generic;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Decoders
{
    public class MipsDecoder : IInstructionDecoder
    {
        public IEnumerable<IrStatement> Decode(ulong address, IMemoryManager memory)
        {
            uint instruction = memory.ReadMemory32(address);
            uint opcode = (instruction >> 26) & 0x3F;

            switch (opcode)
            {
                case 0x08: // ADDI
                    yield return DecodeAddImmediate(instruction);
                    break;
                
                case 0x0C: // ANDI
                    yield return DecodeAndImmediate(instruction);
                    break;

                case 0x0D: // ORI
                    yield return DecodeOrImmediate(instruction);
                    break;

                case 0x04: // BEQ
                    // Per Instruction 10, we first emit the delay slot's statements, then the branch statement.
                    var delaySlotStatements = Decode(address + 4, memory);
                    foreach(var stmt in delaySlotStatements)
                    {
                        yield return stmt;
                    }
                    yield return DecodeBranchEqual(address, instruction);
                    break;

                default:
                    // Rule: No silent failures. No fake execution. (Instruction 7)
                    throw new IllegalInstructionException($"Illegal Instruction: Opcode 0x{opcode:X2} at 0x{address:X16} (Raw: 0x{instruction:X8})");
            }
        }

        private IrStatement DecodeAddImmediate(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            short imm = (short)(instr & 0xFFFF); // Use 'short' to force C# to handle the sign bit

            return new IrStatement
            {
                Op = IrOpCode.Add,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = (ulong)(long)imm, IsImmediate = true }
            };
        }

        private IrStatement DecodeAndImmediate(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            ushort imm = (ushort)(instr & 0xFFFF); // Zero-extended as per MIPS spec

            return new IrStatement
            {
                Op = IrOpCode.And,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = imm, IsImmediate = true }
            };
        }

        private IrStatement DecodeOrImmediate(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            ushort imm = (ushort)(instr & 0xFFFF); // Zero-extended as per MIPS spec

            return new IrStatement
            {
                Op = IrOpCode.Or,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = imm, IsImmediate = true }
            };
        }

        private IrStatement DecodeBranchEqual(ulong currentPC, uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            short offset = (short)(instr & 0xFFFF);
            
            // The MIPS decoder is responsible for computing the absolute branch target
            ulong targetAddress = (currentPC + 4) + (ulong)((long)offset << 2);

            return new IrStatement
            {
                Op = IrOpCode.BranchIfEqual,
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                // Destination holds the target address for the branch
                Destination = new IrOperand { Width = BitWidth.Bits64, Value = targetAddress, IsImmediate = true }
            };
        }
    }
}
