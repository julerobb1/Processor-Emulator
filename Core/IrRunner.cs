using System;
using System.Linq;
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
            _decoder = new MipsDecoder();
            _executor = new BaseIrExecutor();
        }

        private void TriggerException(ulong pc, int causeCode)
        {
            Console.WriteLine($"[EXCEPTION] Triggered. Cause: {causeCode}. Storing PC 0x{pc:X} in EPC.");
            
            // 1. Save current PC into the EPC (Exception Program Counter) register
            _state.SetRegister("cp0_epc", pc, BitWidth.Bits32);

            // 2. Set the Cause register with the reason for the exception
            _state.SetRegister("cp0_cause", (ulong)causeCode << 2, BitWidth.Bits32); // Shift code into ExcCode field

            // 3. Set the PC to the general exception vector
            _state.PC = 0x80000180; // Standard MIPS General Exception Vector for BEV=0
        }

        public void Step()
        {
            ulong pcBefore = _state.PC;

            try
            {
                // 1. Decode and Execute
                var irStatements = _decoder.Decode(pcBefore, _memory).ToList();
                foreach (var statement in irStatements)
                {
                    _executor.ExecuteStatement(statement, _state, _memory);
                }
            
                // 2. PC Truth Rule: Only increment PC if it wasn't modified by a branch.
                if (_state.PC == pcBefore)
                {
                    _state.PC += 4;
                }
            }
            catch (CpuAlignmentException ex)
            {
                Console.WriteLine($"[FAULT] Alignment Error: {ex.Message}");
                TriggerException(pcBefore, EXCEPTION_ALIGNMENT);
                return; // End this step, next step will be at exception vector
            }
            catch (IllegalInstructionException ex)
            {
                Console.WriteLine($"[FAULT] Illegal Instruction: {ex.Message}");
                TriggerException(pcBefore, EXCEPTION_ILLEGAL_INSTRUCTION);
                return;
            }
            catch (Exception ex)
            {
                // Catch any other emulator-breaking error
                Console.WriteLine($"[FATAL EMULATOR ERROR] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // Halt execution by re-throwing
                throw;
            }

            // 3. System Heartbeat: Increment CP0 Count and check for timer interrupt
            ulong count = _state.GetRegister("cp0_count", BitWidth.Bits32);
            ulong compare = _state.GetRegister("cp0_compare", BitWidth.Bits32);
            
            count += 100;
            _state.SetRegister("cp0_count", count, BitWidth.Bits32);

            // Check if interrupts are enabled in the Status register (IE bit 0)
            ulong status = _state.GetRegister("cp0_status", BitWidth.Bits32);
            bool interruptsEnabled = (status & 1) == 1;

            if (interruptsEnabled && compare != 0 && count >= compare)
            {
                Console.WriteLine($"[INTERRUPT] Timer interrupt triggered.");
                TriggerException(pcBefore, EXCEPTION_INTERRUPT);
            }
        }
    }
}