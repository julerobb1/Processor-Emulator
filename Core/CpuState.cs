using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Core
{
    /// <summary>
    /// A concrete implementation of the ICpuState interface. It uses a dictionary
    /// to store register values, allowing for flexibility with different ISAs.
    /// </summary>
    public class CpuState : ICpuState
    {
        private readonly Dictionary<string, IrValue> _registers;

        /// <summary>
        /// Gets or sets the current privilege level of the CPU.
        /// </summary>
        public int PrivilegeLevel { get; set; }

        /// <summary>
        /// Gets the name of the register used as the program counter for the current architecture.
        /// </summary>
        public string ProgramCounterRegisterName { get; }

        /// <summary>
        /// Initializes a new instance of the CpuState class.
        /// </summary>
        /// <param name="pcRegisterName">The canonical name of the program counter register.</param>
        /// <param name="initialRegisters">An optional dictionary of registers to initialize the state with.</param>
        /// <param name="initialPrivilegeLevel">The starting privilege level. Defaults to 0 (user mode).</param>
        public CpuState(string pcRegisterName, Dictionary<string, IrValue> initialRegisters = null, int initialPrivilegeLevel = 0)
        {
            ProgramCounterRegisterName = pcRegisterName ?? throw new ArgumentNullException(nameof(pcRegisterName));
            PrivilegeLevel = initialPrivilegeLevel;

            if (initialRegisters != null)
            {
                // Use a case-insensitive comparer for register names.
                _registers = new Dictionary<string, IrValue>(initialRegisters, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _registers = new Dictionary<string, IrValue>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Retrieves the value of a register by its canonical name.
        /// Throws an exception if the register does not exist.
        /// </summary>
        /// <param name="name">The canonical name of the register (e.g., "R0", "EAX", "PC").</param>
        /// <returns>The register's value with its bit width.</returns>
        public IrValue GetRegister(string name)
        {
            if (_registers.TryGetValue(name, out var value))
            {
                return value;
            }
            // A register that has not been written to is assumed to be 0.
            // This is a common architectural behavior. For strictness, throwing an exception
            // could be an alternative, but this is a more pragmatic default.
            // A truly unknown bit-width is an issue, so we must handle that.
            // For now, we will return a 0-value with a default width of 32,
            // but a more robust system might require explicit register definition.
            return new IrValue(0, 32); 
        }

        /// <summary>
        /// Updates the value of a register. If the register has a different bit width,
        /// it will be updated. The new value must match the register's expected bit width.
        /// </summary>
        /// <param name="name">The canonical name of the register.</param>
        /// <param name="value">The new value for the register.</param>
        public void SetRegister(string name, IrValue value)
        {
            if (_registers.TryGetValue(name, out var existingValue))
            {
                if (existingValue.BitWidth != value.BitWidth)
                {
                    // This could be a feature or a bug. For now, enforce same bit width.
                    // A more advanced model could handle register aliasing (like EAX/RAX).
                    throw new InvalidOperationException(
                        $"Attempted to write value with bit width {value.BitWidth} " +
                        $"to register '{name}' with bit width {existingValue.BitWidth}.");
                }
            }
            _registers[name] = value;
        }
    }
}
