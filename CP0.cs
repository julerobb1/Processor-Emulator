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
        
        // Status Register bits
        private const uint BEV_BIT = 1 << 22; // Boot Exception Vector

        public CP0()
        {
            // Initialize PRId register (Processor Revision Identifier)
            registers[PRIdReg] = 0x00018000; // Example value for a MIPS32 processor

            // Initialize Status register
            // Set BEV bit for boot sequence.
            registers[StatusReg] = BEV_BIT;

            // Initialize Config register for nk.bin
            // Set to little-endian
            registers[ConfigReg] = 0;
        }

        public void WriteRegister(int reg, uint value)
        {
            if (reg >= 0 && reg < 32)
            {
                // Simple write for now. Add special handling for registers if needed.
                Console.WriteLine($"CP0 Write: Reg {reg} = 0x{value:X8}");
                registers[reg] = value;
            }
        }

        public uint ReadRegister(int reg)
        {
            if (reg >= 0 && reg < 32)
            {
                uint value = registers[reg];
                Console.WriteLine($"CP0 Read: Reg {reg} returns 0x{value:X8}");
                return value;
            }
            return 0;
        }
    }
}
