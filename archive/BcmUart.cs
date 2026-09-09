using System;

namespace ProcessorEmulator.Emulation
{
    public class BcmUart : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size => 0x1000; // Typical UART register block size

        public BcmUart(uint startAddress)
        {
            StartAddress = startAddress;
        }

        public uint Read32(uint offset)
        {
            Console.WriteLine($"[UART Read] @ 0x{StartAddress + offset:X8}");
            return 0;
        }

        public void Write32(uint offset, uint value)
        {
            Console.WriteLine($"[UART Write] @ 0x{StartAddress + offset:X8} = 0x{value:X8}");
        }

        public byte Read8(uint offset)
        {
            Console.WriteLine($"[UART Read B] @ 0x{StartAddress + offset:X8}");
            return 0;
        }
        
        public void Write8(uint offset, byte value)
        {
            Console.WriteLine($"[UART Write B] @ 0x{StartAddress + offset:X8} = 0x{value:X2}");
        }
    }
}
