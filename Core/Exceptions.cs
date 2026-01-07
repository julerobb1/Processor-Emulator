using System;

namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Thrown when an attempt is made to execute a privileged instruction
    /// without the required CPU privilege level.
    /// </summary>
    public class PrivilegeViolationException : Exception
    {
        public PrivilegeViolationException() { }

        public PrivilegeViolationException(string message) : base(message) { }

        public PrivilegeViolationException(string message, Exception inner) : base(message, inner) { }
    }
}
