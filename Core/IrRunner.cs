using System;
using System.Text;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Decoders;
using ProcessorEmulator.Core.Backends;

namespace ProcessorEmulator.Core
{
    public class IrRunner
    {
        // MIPS Exception Cause Codes (simplified)
        private const int EXCEPTION_INTERRUPT = 0;
        private const int EXCEPTION_ALIGNMENT = 4;
        private const int EXCEPTION_ILLEGAL_INSTRUCTION = 10;

        private readonly ICpuState _state;
        private readonly IMemoryManager _memory;
        private readonly IInstructionDecoder _decoder;
        private readonly IExecutionEngine _executor;

        public IrRunner(ICpuState state, IMemoryManager memory)
        {
            _state = state;
            _memory = memory;
            _decoder = new MipsIrDecoder();
            _executor = new BaseIrExecutor();
        }

        private void TriggerException(ulong pc, int causeCode, string faultType)
        {
            Console.WriteLine($"[FAULT] {faultType} at 0x{pc:X8}. Triggering exception.");
            
            _state.SetRegister("cp0_epc", pc, BitWidth.Bits32);
            _state.SetRegister("cp0_cause", (ulong)causeCode << 2, BitWidth.Bits32);
            _state.PC = 0x80000180; // Standard MIPS General Exception Vector
        }

        public void Step()
        {
            ulong pcBefore = _state.PC;
            uint rawInstruction = 0;

            try
            {
                rawInstruction = _memory.ReadMemory32(pcBefore);
                var irStatements = _decoder.Decode(pcBefore, _memory).ToList();

                foreach (var statement in irStatements)
                {
                    var log = new StringBuilder();
                    log.Append($"[TRACE] PC: 0x{pcBefore:X8} | Raw: 0x{rawInstruction:X8} | {statement.Op,-10} | ");

                    string destReg = statement.Destination.RegisterName;
                    ulong oldVal = 0;
                    if (!string.IsNullOrEmpty(destReg))
                    {
                        oldVal = _state.GetRegister(destReg, statement.Destination.Width);
                    }

                    // Execute the single statement
                    _executor.ExecuteStatement(statement, _state, _memory);

                    // Format the log based on the operation
                    switch(statement.Op)
                    {
                        case IrOpCode.Store:
                        case IrOpCode.StoreConditional:
                            var addr = _state.GetRegister(statement.SourceA.RegisterName, statement.SourceA.Width) + statement.SourceB.Value;
                            var val = _state.GetRegister(statement.SourceC.RegisterName, statement.SourceC.Width);
                            log.Append($"MEM[0x{addr:X}] <- {statement.SourceC.RegisterName}(0x{val:X})");
                            if(statement.Op == IrOpCode.StoreConditional)
                            {
                                ulong success = _state.GetRegister(destReg, statement.Destination.Width);
                                log.Append($" | Success: {success}");
                            }
                            break;
                        
                        case IrOpCode.Load:
                        case IrOpCode.LoadLinked:
                             ulong newVal = _state.GetRegister(destReg, statement.Destination.Width);
                             log.Append($"{destReg} <- 0x{newVal:X8} | old: 0x{oldVal:X8}");
                             break;
                        
                        case IrOpCode.BranchIfEqual:
                            log.Append($"if {_state.GetRegister(statement.SourceA.RegisterName, statement.SourceA.Width)} == {_state.GetRegister(statement.SourceB.RegisterName, statement.SourceB.Width)} -> PC=0x{statement.Destination.Value:X}");
                            break;

                        default: // Covers Add, Sub, And, Or, Copy
                            if (!string.IsNullOrEmpty(destReg))
                            {
                                ulong finalVal = _state.GetRegister(destReg, statement.Destination.Width);
                                log.Append($"{destReg} -> 0x{finalVal:X8} | old: 0x{oldVal:X8}");
                            }
                            else
                            {
                                log.Append("No destination register.");
                            }
                            break;
                    }
                    Console.WriteLine(log.ToString());
                }
            
                if (_state.PC == pcBefore)
                {
                    _state.PC += 4;
                }
            }
            catch (CpuAlignmentException)
            {
                TriggerException(pcBefore, EXCEPTION_ALIGNMENT, "Alignment Error");
                return;
            }
            catch (IllegalInstructionException)
            {
                TriggerException(pcBefore, EXCEPTION_ILLEGAL_INSTRUCTION, "Illegal Instruction");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL] At PC 0x{pcBefore:X8} (Raw: 0x{rawInstruction:X8}): {ex.Message}");
                throw;
            }

            // System Heartbeat
            ulong count = _state.GetRegister("cp0_count", BitWidth.Bits32);
            ulong compare = _state.GetRegister("cp0_compare", BitWidth.Bits32);
            count += 100;
            _state.SetRegister("cp0_count", count, BitWidth.Bits32);

            ulong status = _state.GetRegister("cp0_status", BitWidth.Bits32);
            bool interruptsEnabled = (status & 1) == 1;

            if (interruptsEnabled && compare != 0 && count >= compare)
            {
                TriggerException(pcBefore, EXCEPTION_INTERRUPT, "Timer Interrupt");
            }
        }
    }
}
