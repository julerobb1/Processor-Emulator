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
    /// Thrown when the TLB does not contain a valid mapping for a virtual address.
    /// This is a specific type of memory exception that the MIPS kernel is expected to handle.
    /// </summary>
    public class TlbMissException : Exception
    {
        public uint FaultingAddress { get; }
        public bool IsStore { get; }
        public bool IsInvalid { get; }
        public TlbMissException(string message, uint faultingAddress, bool isStore = false, bool isInvalid = false) : base(message)
        {
            FaultingAddress = faultingAddress;
            IsStore = isStore;
            IsInvalid = isInvalid;
        }
    }

    /// <summary>
    /// Thrown when a physical address does not map to any known device on the bus.
    /// </summary>
    public class AddressErrorException : Exception
    {
        public AddressErrorException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when the decoder encounters an unknown or unsupported instruction opcode.
    /// </summary>
    public class IllegalInstructionException : Exception
    {
        public IllegalInstructionException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a bus access triggers a memory mapping violation / alignment issue.
    /// </summary>
    public class BusErrorException : Exception
    {
        public BusErrorException(string message) : base(message) { }
    }
}