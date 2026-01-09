using System;

namespace ProcessorEmulator.Emulation
{
    public class CP0
    {
        private uint[] registers = new uint[32];

        // MIPS CP0 Register Constants
        private const int IndexReg = 0;
        private const int RandomReg = 1;
        private const int EntryLo0Reg = 2;
        private const int EntryLo1Reg = 3;
        private const int ContextReg = 4;
        private const int PageMaskReg = 5;
        private const int WiredReg = 6;
        private const int BadVAddrReg = 8;
        private const int CountReg = 9;
        private const int EntryHiReg = 10;
        private const int CompareReg = 11;
        private const int StatusReg = 12;
        private const int CauseReg = 13;
        private const int EPCReg = 14;
        private const int PRIdReg = 15;
        private const int ConfigReg = 16;
        
        // Public properties for easier register access
        public uint Status { get => registers[StatusReg]; set => registers[StatusReg] = value; }
        public uint Cause { get => registers[CauseReg]; set => registers[CauseReg] = value; }
        public uint EPC { get => registers[EPCReg]; set => registers[EPCReg] = value; }
        public uint Count { get => registers[CountReg]; set => registers[CountReg] = value; }
        public uint Compare { get => registers[CompareReg]; set => registers[CompareReg] = value; }

        public uint PRId { get; set; }

        // Status Register bits
        private const uint STATUS_IE_BIT = 1 << 0;  // Interrupt Enable
        private const uint STATUS_EXL_BIT = 1 << 1; // Exception Level
        private const uint BEV_BIT = 1 << 22;       // Boot Exception Vector
        
        // Cause Register bits
        private const uint CAUSE_IP7_BIT = 1 << 15; // Timer Interrupt Pending

        public CP0()
        {
            // Initialize PRId register (Processor Revision Identifier) to a generic default
            PRId = 0x00018000; // Generic MIPS 4Kc

            // Initialize Status register
            // Set BEV bit for boot sequence.
            Status = BEV_BIT;

            // Initialize Config register for nk.bin
            // Set to little-endian
            registers[ConfigReg] = 0;
        }

        public void WriteRegister(int reg, uint value)
        {
            if (reg >= 0 && reg < 32)
            {
                // When the guest OS writes to the Compare register, it clears the timer interrupt.
                if (reg == CompareReg)
                {
                    Cause &= ~CAUSE_IP7_BIT;
                }

                // Writing to the Cause register can only clear interrupt pending bits, not set them.
                if (reg == CauseReg)
                {
                   uint clearMask = 0xFFFF00FF; // Only allow writes to lower bits, not IP bits
                   registers[CauseReg] = (registers[CauseReg] & ~clearMask) | (value & clearMask);
                   return;
                }

                Console.WriteLine($"CP0 Write: Reg {reg} = 0x{value:X8}");
                registers[reg] = value;
            }
        }

        public uint ReadRegister(int reg)
        {
            if (reg == PRIdReg)
            {
                Console.WriteLine($"CP0 Read: Reg {reg} (PRId) returns 0x{PRId:X8}");
                return PRId;
            }

            if (reg >= 0 && reg < 32)
            {
                uint value = registers[reg];
                // Console.WriteLine($"CP0 Read: Reg {reg} returns 0x{value:X8}"); // Too noisy for timer
                return value;
            }
            return 0;
        }

        /// <summary>
        /// Updates the internal timer count and triggers an interrupt if Compare is reached.
        /// </summary>
        /// <param name="cycles">Number of CPU cycles to advance the timer by.</param>
        public void UpdateTimer(int cycles)
        {
            uint old_count = Count;
            Count += (uint)cycles;
            
            // The guest OS sets the Compare register to schedule an interrupt.
            // When Count matches Compare, we set the interrupt pending bit.
            if (Count == Compare && old_count != Compare)
            {
                Cause |= CAUSE_IP7_BIT;
            }
        }

        /// <summary>
        /// Checks if a hardware interrupt should be triggered by the CPU.
        /// </summary>
        public bool ShouldTriggerInterrupt()
        {
            // An interrupt can only occur if:
            // 1. Global interrupts are enabled (IE bit in Status is 1)
            // 2. The CPU is not in an exception level (EXL bit in Status is 0)
            // 3. An interrupt is pending (IP bits in Cause) and not masked (IM bits in Status)
            bool interruptsEnabled = (Status & STATUS_IE_BIT) != 0;
            bool inException = (Status & STATUS_EXL_BIT) != 0;
            uint interruptMask = (Status >> 8) & 0xFF;
            uint interruptPending = (Cause >> 8) & 0xFF;

            return interruptsEnabled && !inException && (interruptPending & interruptMask) != 0;
        }
    }
}