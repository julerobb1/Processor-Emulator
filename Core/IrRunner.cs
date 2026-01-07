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

        public void RunHex(string hexInstruction)
        {
            // Convert hex string to bytes and write to memory at the current PC
            uint raw = uint.Parse(hexInstruction.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            _memory.WriteMemory32(_state.PC, raw);
            
            // For branch tests, we need to also write the delay slot instruction
            if (hexInstruction.StartsWith("0x04") || hexInstruction.StartsWith("0x14")) // BEQ, BNE
            {
                // Let's use a NOP for the delay slot for now. MIPS NOP is all zeroes.
                 _memory.WriteMemory32(_state.PC + 4, 0x00000000);
            }

            Console.WriteLine($"--- Executing instruction at 0x{_state.PC:X} ---");
            
            ulong pcBefore = _state.PC;

            // 1. Decode to IR
            var irStatements = _decoder.Decode(_state.PC, _memory).ToList();

            // 2. Execute IR
            foreach (var statement in irStatements)
            {
                // Capture old value for the diff
                ulong oldVal = 0;
                if (statement.Op != IrOpCode.BranchIfEqual && !string.IsNullOrEmpty(statement.Destination.RegisterName))
                    oldVal = _state.GetRegister(statement.Destination.RegisterName, statement.Destination.Width);

                _executor.ExecuteStatement(statement, _state, _memory);

                // 3. Print Diff
                if (statement.Op != IrOpCode.BranchIfEqual && !string.IsNullOrEmpty(statement.Destination.RegisterName))
                {
                    ulong newVal = _state.GetRegister(statement.Destination.RegisterName, statement.Destination.Width);
                    Console.WriteLine($"  {statement.Destination.RegisterName}: 0x{oldVal:X8} -> 0x{newVal:X8}");
                }
            }
            
            // 4. PC Truth Rule: Only increment PC if it wasn't modified by a branch.
            if (_state.PC == pcBefore)
            {
                _state.PC += (ulong)(irStatements.Count > 1 ? 8 : 4); // Account for delay slot
            }
            else
            {
                 Console.WriteLine($"  BRANCH TAKEN -> New PC: 0x{_state.PC:X}");
            }
        }
    }
}
