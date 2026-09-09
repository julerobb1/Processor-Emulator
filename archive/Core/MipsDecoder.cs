namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Decodes raw 32-bit MIPS instructions into a structured format.
    /// </summary>
    public static class MipsDecoder
    {
        /// <summary>
        /// Decodes a 32-bit instruction word.
        /// </summary>
        /// <param name="instruction">The 32-bit instruction word.</param>
        /// <returns>A MipsInstruction struct containing the decoded fields.</returns>
        public static MipsInstruction Decode(uint instruction)
        {
            return new MipsInstruction(instruction);
        }
    }
}
