using System;

namespace ProcessorEmulator
{
    /// <summary>
    /// Represents an error that occurs during firmware unpacking.
    /// </summary>
    public class FirmwareUnpackException : Exception
    {
        public FirmwareUnpackException()
        {
        }

        public FirmwareUnpackException(string message)
            : base(message)
        {
        }

        public FirmwareUnpackException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
