using System;

namespace ProcessorEmulator.Emulation
{
    public class CP0
    {
        private uint[] registers = new uint[32];
        private uint _entryHi;
        private uint _entryLo0;
        private uint _entryLo1;

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
        public uint EntryHi { get => _entryHi; set => _entryHi = value; }
        public uint EntryLo0 { get => _entryLo0; set => _entryLo0 = value; }
        public uint EntryLo1 { get => _entryLo1; set => _entryLo1 = value; }
        public uint BadVAddr { get => registers[BadVAddrReg]; set => registers[BadVAddrReg] = value; }
        public uint Context { get => registers[ContextReg]; set => registers[ContextReg] = value; }


        public uint PRId { get; set; }

        // TLB Entry structure
        public struct TLBEntry
        {
            public uint EntryHi;    // Virtual Page Number, ASID, VPN2
            public uint EntryLo0;   // PFN, C, D, V, G for even page
            public uint EntryLo1;   // PFN, C, D, V, G for odd page
            public uint PageMask;   // Page size
        }

        private const int TLB_ENTRIES = 32;
        private TLBEntry[] _tlb = new TLBEntry[TLB_ENTRIES];

        public enum TlbTranslateStatus
        {
            Hit,
            Miss,
            Invalid
        }

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

            // Random is read-only; tlbwr consumes it and counts down to Wired.
            registers[RandomReg] = TLB_ENTRIES - 1;
        }

        public void WriteRegister(int reg, uint value)
        {
            if (reg >= 0 && reg < 32)
            {
                if (reg == RandomReg)
                    return;

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
                
                switch(reg)
                {
                    case EntryHiReg:
                        _entryHi = value;
                        Console.WriteLine($"CP0 Write: EntryHi = 0x{value:X8}");
                        break;
                    case EntryLo0Reg:
                        _entryLo0 = value;
                        Console.WriteLine($"CP0 Write: EntryLo0 = 0x{value:X8}");
                        break;
                    case EntryLo1Reg:
                        _entryLo1 = value;
                        Console.WriteLine($"CP0 Write: EntryLo1 = 0x{value:X8}");
                        break;
                    default:
                        Console.WriteLine($"CP0 Write: Reg {reg} = 0x{value:X8}");
                        registers[reg] = value;
                        break;
                }
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
                uint value;
                switch(reg)
                {
                    case EntryHiReg:
                        value = _entryHi;
                        break;
                    case EntryLo0Reg:
                        value = _entryLo0;
                        break;
                    case EntryLo1Reg:
                        value = _entryLo1;
                        break;
                    default:
                        value = registers[reg];
                        break;
                }
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

        public TlbTranslateStatus TryTranslate(uint vaddr, out uint paddr)
        {
            paddr = 0;
            uint vpn2 = (vaddr >> 13) & 0x7FFFF;
            bool odd = (vaddr & 0x1000) != 0;
            uint pageOffset = vaddr & 0x0FFF;

            for (int i = 0; i < TLB_ENTRIES; i++)
            {
                uint tlbVpn2 = (_tlb[i].EntryHi >> 13) & 0x7FFFF;
                if (tlbVpn2 != vpn2)
                    continue;

                uint lo = odd ? _tlb[i].EntryLo1 : _tlb[i].EntryLo0;
                if ((lo & 2) == 0)
                    return TlbTranslateStatus.Invalid;

                paddr = ((lo & 0x3FFFFC0) << 6) | pageOffset;
                return TlbTranslateStatus.Hit;
            }

            return TlbTranslateStatus.Miss;
        }

        public void PrepareTlbException(uint vaddr)
        {
            BadVAddr = vaddr;
            EntryHi = (vaddr & 0xFFFFE000) | (EntryHi & 0xFF);
            Context = (Context & 0xFF800000) | ((vaddr >> 13) << 4);
        }

        // MIPS TLB operations

        /// <summary>
        /// Reads the TLB entry specified by IndexReg into EntryHi, EntryLo0, EntryLo1, and PageMask.
        /// </summary>
        public void ReadTLBEntry()
        {
            int index = (int)(registers[IndexReg] & 0x1F); // Index is 5 bits
            if (index < TLB_ENTRIES)
            {
                EntryHi = _tlb[index].EntryHi;
                EntryLo0 = _tlb[index].EntryLo0;
                EntryLo1 = _tlb[index].EntryLo1;
                registers[PageMaskReg] = _tlb[index].PageMask;
                Console.WriteLine($"CP0 TLBR: Read TLB entry {index}");
            }
            else
            {
                Console.WriteLine($"CP0 TLBR: Invalid TLB index {index}");
                // In a real CPU, this might cause an exception or return garbage.
            }
        }

        /// <summary>
        /// Writes EntryHi, EntryLo0, EntryLo1, and PageMask to the TLB entry specified by IndexReg.
        /// </summary>
        public void WriteTLBEntryIndexed()
        {
            int index = (int)(registers[IndexReg] & 0x1F); // Index is 5 bits
            if (index < TLB_ENTRIES)
            {
                _tlb[index].EntryHi = EntryHi;
                _tlb[index].EntryLo0 = EntryLo0;
                _tlb[index].EntryLo1 = EntryLo1;
                _tlb[index].PageMask = registers[PageMaskReg];
                Console.WriteLine($"CP0 TLBWI: Wrote TLB entry {index}");
            }
            else
            {
                Console.WriteLine($"CP0 TLBWI: Invalid TLB index {index}");
            }
        }

        /// <summary>
        /// Writes EntryHi, EntryLo0, EntryLo1, and PageMask to the TLB entry
        /// selected by Random (clamped to [Wired, 31]). Random then counts
        /// down and wraps at Wired so consecutive tlbwr hits different slots.
        /// </summary>
        public void WriteTLBEntryRandom()
        {
            uint wired = registers[WiredReg] & 0x1F;
            if (wired >= TLB_ENTRIES)
                wired = 0;
            uint random = registers[RandomReg] & 0x1F;
            if (random < wired || random >= TLB_ENTRIES)
                random = (uint)(TLB_ENTRIES - 1);
            int index = (int)random;
            _tlb[index].EntryHi = EntryHi;
            _tlb[index].EntryLo0 = EntryLo0;
            _tlb[index].EntryLo1 = EntryLo1;
            _tlb[index].PageMask = registers[PageMaskReg];
            // Advance after tlbwr. Per-instruction decrement locks to one slot
            // when the refill path length equals the Random range (Wired..31).
            if (random <= wired)
                random = (uint)(TLB_ENTRIES - 1);
            else
                random--;
            registers[RandomReg] = random;
            Console.WriteLine($"CP0 TLBWR: Wrote TLB entry randomly at {index}");
        }

        /// <summary>
        /// Searches the TLB for an entry matching EntryHi. If found, its index is written to IndexReg.
        /// </summary>
        public void ProbeTLB()
        {
            // MIPS TLBP instruction uses EntryHi to find a matching entry in the TLB.
            // If found, Index register is updated with the entry's index.
            // If not found, the P bit (bit 31) of Index register is set.

            uint vpn2 = (EntryHi >> 13) & 0x7FFFF; // VPN2 from EntryHi (bits 13-31)
            uint asid = EntryHi & 0xFF; // ASID from EntryHi (bits 0-7)

            for (int i = 0; i < TLB_ENTRIES; i++)
            {
                // Extract VPN2 and ASID from stored TLB entry
                uint tlbVpn2 = (_tlb[i].EntryHi >> 13) & 0x7FFFF;
                uint tlbAsid = _tlb[i].EntryHi & 0xFF;

                // For simplicity, ignore ASID for now (assume all entries are global or ASID matches)
                // A real implementation would check the G bit and ASID match
                if (tlbVpn2 == vpn2) // && (tlbAsid == asid || (_tlb[i].EntryLo0 & 1) != 0 || (_tlb[i].EntryLo1 & 1) != 0)) // Check G bit
                {
                    registers[IndexReg] = (uint)i;
                    Console.WriteLine($"CP0 TLBP: Found match at index {i}");
                    return;
                }
            }

            // No match found, set P bit (bit 31) in IndexReg
            registers[IndexReg] = 0x80000000;
            Console.WriteLine("CP0 TLBP: No match found");
        }
    }
}