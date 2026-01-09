using System;

namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Thrown when a memory access violates hardware alignment rules (e.g., a 32-bit read from an unaligned address).
    /// </summary>
    public class CpuAlignmentException : Exception
    {
        public CpuAlignmentException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a memory access is to an invalid or unmapped address.
    /// </summary>
    public class BusErrorException : Exception
    {
        public BusErrorException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when the decoder encounters an unknown or unsupported instruction opcode.
    /// </summary>
    public class IllegalInstructionException : Exception
    {
        public IllegalInstructionException(string message) : base(message) { }
    }
}