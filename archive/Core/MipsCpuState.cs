namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Holds the complete state of a MIPS32 CPU core.
    /// </summary>
    public class MipsCpuState
    {
        /// <summary>
        /// General Purpose Registers (r0-r31). r0 is hardwired to zero.
        /// </summary>
        public uint[] GPR { get; } = new uint[32];

        /// <summary>
        /// The Program Counter. Points to the next instruction to be executed.
        /// </summary>
        public uint PC { get; set; }

        /// <summary>
        /// The HI register, used for the upper 32 bits of multiplication results and remainder of division.
        /// </summary>
        public uint HI { get; set; }

        /// <summary>
        /// The LO register, used for the lower 32 bits of multiplication results and quotient of division.
        /// </summary>
        public uint LO { get; set; }

        /// <summary>
        /// Address for Load-Linked instructions.
        /// </summary>
        public uint LLAddr { get; set; }
        
        /// <summary>
        /// Flag for Load-Linked/Store-Conditional instructions.
        /// </summary>
        public bool LLBit { get; set; }

        public MipsCpuState(uint startPC = 0)
        {
            PC = startPC;
        }

        /// <summary>
        /// Read from a general-purpose register. Reading r0 always returns 0.
        /// </summary>
        public uint GetRegister(int index)
        {
            // r0 is hardwired to 0
            return (index == 0) ? 0 : GPR[index];
        }

        /// <summary>
        /// Write to a general-purpose register. Writing to r0 is ignored.
        /// </summary>
        public void SetRegister(int index, uint value)
        {
            if (index != 0)
            {
                GPR[index] = value;
            }
        }
    }
}
