using System;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Backends
{
    public class BaseIrExecutor : IExecutionEngine
    {
        public void ExecuteStatement(IrStatement statement, ICpuState state)
        {
            // Strict Privilege Check
            // Metadata bit 0 = IsPrivileged
            if ((statement.Metadata & 1) == 1 && state.PrivilegeLevel == 0)
            {
                throw new InvalidOperationException("Privilege Violation: Attempted privileged IR op in user mode.");
            }

            ulong result = 0;
            ulong mask = GetWidthMask(statement.Destination.Width);

            switch (statement.Op)
            {
                case IrOpCode.Add:
                    // Perform addition and wrap based on destination bit-width
                    result = (statement.SourceA.Value + statement.SourceB.Value) & mask;
                    break;

                case IrOpCode.Sub:
                    result = (statement.SourceA.Value - statement.SourceB.Value) & mask;
                    break;

                case IrOpCode.And:
                    result = (statement.SourceA.Value & statement.SourceB.Value) & mask;
                    break;

                case IrOpCode.Store:
                    // Simple memory write (assuming 8-bit increments for this IR)
                    state.WriteMemory8(statement.Destination.Value, (byte)(statement.SourceA.Value & 0xFF));
                    return;

                default:
                    throw new NotImplementedException($"Canonical IR Op {statement.Op} is not yet implemented.");
            }

            // Commit result back to state (Registers)
            if (!string.IsNullOrEmpty(statement.Destination.RegisterName))
            {
                state.SetRegister(statement.Destination.RegisterName, result, statement.Destination.Width);
            }
        }

        private ulong GetWidthMask(BitWidth width) => width switch
        {
            BitWidth.Bits8 => 0xFF,
            BitWidth.Bits16 => 0xFFFF,
            BitWidth.Bits32 => 0xFFFFFFFF,
            BitWidth.Bits64 => 0xFFFFFFFFFFFFFFFF,
            _ => throw new ArgumentOutOfRangeException(nameof(width))
        };
    }
}