using System;

namespace ProcessorEmulator.Core.Memory
{
    /// <summary>
    /// Implements MIPS R4000-style virtual memory address translation.
    /// </summary>
    public static class VirtualMmu
    {
        // MIPS Memory Segment Definitions
        public const ulong KUSEG_START = 0x00000000;
        public const ulong KUSEG_END = 0x7FFFFFFF;

        public const ulong KSEG0_START = 0x80000000;
        public const ulong KSEG0_END = 0x9FFFFFFF;

        public const ulong KSEG1_START = 0xA0000000;
        public const ulong KSEG1_END = 0xBFFFFFFF;

        /// <summary>
        /// Translates a virtual address to a physical address based on MIPS memory segments.
        /// </summary>
        /// <param name="virtualAddress">The virtual address to translate.</param>
        /// <returns>The corresponding physical address.</returns>
        public static ulong TranslateAddress(ulong virtualAddress)
        {
            // KSEG0: Unmapped, cached. Physical = Virtual & 0x1FFFFFFF
            if (virtualAddress >= KSEG0_START && virtualAddress <= KSEG0_END)
            {
                return virtualAddress & 0x1FFFFFFF;
            }

            // KSEG1: Unmapped, uncached. Physical = Virtual & 0x1FFFFFFF
            if (virtualAddress >= KSEG1_START && virtualAddress <= KSEG1_END)
            {
                return virtualAddress & 0x1FFFFFFF;
            }

            // KUSEG: Mapped via TLB. For now, we use identity mapping.
            if (virtualAddress >= KUSEG_START && virtualAddress <= KUSEG_END)
            {
                // Task: For now, use identity mapping. Later, this will involve a TLB lookup.
                return virtualAddress;
            }

            // If the address is outside the main segments, it might be in KSEG2 or other
            // implementation-defined areas. For now, we'll assume identity mapping for those too.
            // A more complete implementation would handle TLB-mapped KSEG2 addresses.
            return virtualAddress;
        }
    }
}
