using System;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Memory;

namespace ProcessorEmulator.Core.Backends
{
    public class BaseIrExecutor : IExecutionEngine
    {
        // Primary implementation invoked by callers and overridable by backends
        public virtual void ExecuteStatement(IrStatement statement, ICpuState state, IMemoryManager memory)
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

                case IrOpCode.Copy:
                    result = valA & mask; // Get value from SourceA and apply mask
                    SetDestinationValue(statement.Destination, result, state);
                    break;

                case IrOpCode.Load:
                    {
                        ulong virtualAddr = valA + valB;
                        ulong physicalAddr = VirtualMmu.TranslateAddress(virtualAddr);
                        if ((physicalAddr % 4) != 0 && statement.Destination.Width == BitWidth.Bits32)
                        {
                            throw new CpuAlignmentException($"Unaligned 32-bit read at vAddr 0x{virtualAddr:X} (pAddr 0x{physicalAddr:X})");
                        }
                        result = memory.ReadMemory32(physicalAddr);
                        SetDestinationValue(statement.Destination, result, state);
                    }
                    break;

                case IrOpCode.Store:
                    {
                        ulong virtualAddr = valA + valB;
                        ulong physicalAddr = VirtualMmu.TranslateAddress(virtualAddr);
                        if ((physicalAddr % 4) != 0 && statement.SourceC.Width == BitWidth.Bits32)
                        {
                            throw new CpuAlignmentException($"Unaligned 32-bit write at vAddr 0x{virtualAddr:X} (pAddr 0x{physicalAddr:X})");
                        }
                        memory.WriteMemory32(physicalAddr, (uint)valC);
                    }
                    break;
                
                case IrOpCode.LoadLinked:
                    {
                        ulong virtualAddr = valA + valB;
                        ulong physicalAddr = VirtualMmu.TranslateAddress(virtualAddr);
                        if ((physicalAddr % 4) != 0) throw new CpuAlignmentException($"Unaligned LL at vAddr 0x{virtualAddr:X} (pAddr 0x{physicalAddr:X})");
                        
                        result = memory.ReadMemory32(physicalAddr);
                        SetDestinationValue(statement.Destination, result, state);
                        state.LinkedAddress = physicalAddr;
                    }
                    break;

                case IrOpCode.StoreConditional:
                    {
                        ulong virtualAddr = valA + valB;
                        ulong physicalAddr = VirtualMmu.TranslateAddress(virtualAddr);
                        if ((physicalAddr % 4) != 0) throw new CpuAlignmentException($"Unaligned SC at vAddr 0x{virtualAddr:X} (pAddr 0x{physicalAddr:X})");

                        if (state.LinkedAddress == physicalAddr)
                        {
                            memory.WriteMemory32(physicalAddr, (uint)valC);
                            SetDestinationValue(statement.Destination, 1, state);
                            state.LinkedAddress = null;
                        }
                        else
                        {
                            SetDestinationValue(statement.Destination, 0, state);
                        }
                    }
                    break;

                case IrOpCode.BranchIfEqual:
                    if (valA == valB)
                    {
                        state.PC = statement.Destination.Value;
                    }
                    // Note: MIPS Branch Delay Slot logic is handled by the Decoder.
                    break;

                default:
                    throw new NotImplementedException($"Canonical IR Op {statement.Op} is not yet implemented.");
            }
        }

        // Explicit interface implementation to ensure the class satisfies the IExecutionEngine contract
        void IExecutionEngine.ExecuteStatement(IrStatement statement, ICpuState state, IMemoryManager memory)
        {
            ExecuteStatement(statement, state, memory);
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