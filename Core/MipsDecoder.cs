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
                
                case 0x10: // COP0
                    // This opcode is for all CP0 instructions, differentiated by the 'rs' field
                    foreach (var stmt in DecodeCop0(instruction))
                    {
                        yield return stmt;
                    }
                    break;

                case 0x30: // LL
                    yield return DecodeLoadLinked(instruction);
                    break;

                case 0x38: // SC
                    yield return DecodeStoreConditional(instruction);
                    break;

                default:
                    // Rule: No silent failures. No fake execution. (Instruction 7)
                    throw new IllegalInstructionException($"Illegal Instruction: Opcode 0x{opcode:X2} at 0x{address:X16} (Raw: 0x{instruction:X8})");
            }
        }

        private IEnumerable<IrStatement> DecodeCop0(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            uint rd = (instr >> 11) & 0x1F;

            switch (rs)
            {
                case 0b00100: // MTC0 (Move To Coprocessor 0)
                    yield return new IrStatement
                    {
                        Op = IrOpCode.Copy,
                        // Destination is the CP0 register
                        Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = GetCp0RegisterName(rd) },
                        // Source is the GPR
                        SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" }
                    };
                    break;
                
                case 0b00000: // MFC0 (Move From Coprocessor 0)
                     yield return new IrStatement
                    {
                        Op = IrOpCode.Copy,
                        // Destination is the GPR
                        Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                        // Source is the CP0 register
                        SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = GetCp0RegisterName(rd) }
                    };
                    break;

                default:
                    throw new IllegalInstructionException($"Unsupported COP0 instruction with rs=0b{Convert.ToString(rs, 2).PadLeft(5, '0')}");
            }
        }

        private string GetCp0RegisterName(uint index) => index switch
        {
            9 => "cp0_count",
            11 => "cp0_compare",
            12 => "cp0_status",
            13 => "cp0_cause",
            14 => "cp0_epc",
            _ => throw new NotSupportedException($"CP0 Register {index} is not implemented.")
        };


        private IrStatement DecodeLoadLinked(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F; // base
            uint rt = (instr >> 16) & 0x1F; // destination
            short imm = (short)(instr & 0xFFFF); // offset

            return new IrStatement
            {
                Op = IrOpCode.LoadLinked,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = (ulong)(long)imm, IsImmediate = true }
            };
        }

        private IrStatement DecodeStoreConditional(uint instr)
        {
            uint rs = (instr >> 21) & 0x1F; // base
            uint rt = (instr >> 16) & 0x1F; // value to store is in this register
            short imm = (short)(instr & 0xFFFF); // offset
            
            // In the IR, the destination register (rt) is also where we write the success/fail result.
            return new IrStatement
            {
                Op = IrOpCode.StoreConditional,
                Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" },
                SourceA = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rs}" },
                SourceB = new IrOperand { Width = BitWidth.Bits32, Value = (ulong)(long)imm, IsImmediate = true },
                SourceC = new IrOperand { Width = BitWidth.Bits32, RegisterName = $"r{rt}" } // value to store
            };
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
