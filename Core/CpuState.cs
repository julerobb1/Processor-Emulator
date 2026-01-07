using System.Collections.Generic;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core
{
    /// <summary>
    /// A concrete implementation of the ICpuState interface for the new IR pipeline.
    /// </summary>
    public class CpuState : ICpuState
    {
        private readonly Dictionary<string, ulong> _registers = new Dictionary<string, ulong>(System.StringComparer.OrdinalIgnoreCase);

        public int PrivilegeLevel { get; set; } = 0;
        public ulong PC { get; set; } = 0;

        public ulong GetRegister(string name, BitWidth width)
        {
            _registers.TryGetValue(name, out ulong value);
            // The executor is responsible for masking, so we return the raw ulong value.
            return value;
        }

        public void SetRegister(string name, ulong value, BitWidth width)
        {
            _registers[name] = value;
        }

        public void WriteMemory8(ulong address, byte value)
        {
            // For now, memory is not implemented as it's not needed for the ADDI test.
            throw new System.NotImplementedException();
        }
    }
}