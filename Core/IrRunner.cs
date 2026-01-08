using System;
using System.Linq;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Decoders;
using ProcessorEmulator.Core.Backends;

namespace ProcessorEmulator.Core
{
    public class IrRunner
    {
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

        public void Step()
        {
            ulong pcBefore = _state.PC;

            // 1. Decode to IR from memory at the current PC
            var irStatements = _decoder.Decode(_state.PC, _memory).ToList();

            // 2. Execute IR
            foreach (var statement in irStatements)
            {
                _executor.ExecuteStatement(statement, _state, _memory);
            }
            
            // 3. PC Truth Rule: Only increment PC if it wasn't modified by a branch.
            if (_state.PC == pcBefore)
            {
                // The number of instructions in the delay slot is handled by the decoder.
                // We advance the PC by 4 for the main instruction. The delay slot PC handling is implicit.
                _state.PC += 4;
            }

            // 4. System Heartbeat: Increment CP0 Count register
            ulong count = _state.GetRegister("cp0_count", BitWidth.Bits32);
            ulong compare = _state.GetRegister("cp0_compare", BitWidth.Bits32);
            
            count += 100; // Increment by a fixed value per instruction
            _state.SetRegister("cp0_count", count, BitWidth.Bits32);

            if (compare != 0 && count >= compare)
            {
                Console.WriteLine($"[TIMER INTERRUPT] CP0 Count (0x{count:X}) >= Compare (0x{compare:X})");
            }
        }
    }
}
