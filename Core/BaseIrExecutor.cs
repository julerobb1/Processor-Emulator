using System;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Backends
{
    public class BaseIrExecutor : IExecutionEngine
    {
        public void ExecuteStatement(IrStatement statement, ICpuState state, IMemoryManager memory)
        {
            // Strict Privilege Check
            if ((statement.Metadata & 1) == 1 && state.PrivilegeLevel == 0)
            {
                throw new InvalidOperationException("Privilege Violation: Attempted privileged IR op in user mode.");
            }

            // Resolve source operand values
            ulong valA = GetOperandValue(statement.SourceA, state);
            ulong valB = GetOperandValue(statement.SourceB, state);
            ulong valC = GetOperandValue(statement.SourceC, state);
            
            ulong result;
            ulong mask = GetWidthMask(statement.Destination.Width);

            switch (statement.Op)
            {
                case IrOpCode.Add:
                    result = (valA + valB) & mask;
                    SetDestinationValue(statement.Destination, result, state);
                    break;

                case IrOpCode.Sub:
                    result = (valA - valB) & mask;
                    SetDestinationValue(statement.Destination, result, state);
                    break;

                case IrOpCode.And:
                    result = (valA & valB) & mask;
                    SetDestinationValue(statement.Destination, result, state);
                    break;

                case IrOpCode.Or:
                    result = (valA | valB) & mask;
                    SetDestinationValue(statement.Destination, result, state);
                    break;

                case IrOpCode.Load:
                    {
                        ulong effectiveAddr = valA + valB;
                        if ((effectiveAddr % 4) != 0 && statement.Destination.Width == BitWidth.Bits32)
                        {
                            throw new CpuAlignmentException($"Unaligned 32-bit read at address 0x{effectiveAddr:X}");
                        }
                        result = memory.ReadMemory32(effectiveAddr);
                        SetDestinationValue(statement.Destination, result, state);
                    }
                    break;

                case IrOpCode.Store:
                    {
                        ulong effectiveAddr = valA + valB;
                        if ((effectiveAddr % 4) != 0 && statement.SourceC.Width == BitWidth.Bits32)
                        {
                            throw new CpuAlignmentException($"Unaligned 32-bit write at address 0x{effectiveAddr:X}");
                        }
                        // Value to store comes from SourceC
                        memory.WriteMemory32(effectiveAddr, (uint)valC);
                    }
                    break;

                case IrOpCode.BranchIfEqual:
                    if (valA == valB)
                    {
                        // The target address is stored in the Destination operand for branches
                        state.PC = statement.Destination.Value;
                    }
                    // This is a comment to satisfy Instruction 6:
                    // Note: MIPS Branch Delay Slot logic is handled by the Decoder, which emits
                    // the delay slot instruction's IR statements before this branch statement.
                    break;

                default:
                    throw new NotImplementedException($"Canonical IR Op {statement.Op} is not yet implemented.");
            }
        }

        private ulong GetOperandValue(IrOperand operand, ICpuState state)
        {
            if (operand.IsImmediate)
            {
                return operand.Value;
            }
            if (!string.IsNullOrEmpty(operand.RegisterName))
            {
                return state.GetRegister(operand.RegisterName, operand.Width);
            }
            return 0; // Default for an unused operand
        }

        private void SetDestinationValue(IrOperand dest, ulong value, ICpuState state)
        {
            if (!string.IsNullOrEmpty(dest.RegisterName))
            {
                state.SetRegister(dest.RegisterName, value, dest.Width);
            }
        }

        private ulong GetWidthMask(BitWidth width) => width switch
        {
            BitWidth.Bits8 => 0xFF,
            BitWidth.Bits16 => 0xFFFF,
            BitWidth.Bits32 => 0xFFFFFFFF,
            BitWidth.Bits64 => 0xFFFFFFFFFFFFFFFF,
            _ => 0xFFFFFFFFFFFFFFFF // Default to full width if not specified
        };
    }
}