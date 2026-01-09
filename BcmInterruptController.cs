using System;

namespace ProcessorEmulator.Emulation
{
    public class BcmInterruptController : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size => 0x1000; // Example size

        public BcmInterruptController(uint startAddress)
        {
            StartAddress = startAddress;
        }

        public uint Read32(uint offset)
        {
            Console.WriteLine($"[Interrupt Controller Read] @ 0x{StartAddress + offset:X8}");
            return 0;
        }

        public void Write32(uint offset, uint value)
        {
            Console.WriteLine($"[Interrupt Controller Write] @ 0x{StartAddress + offset:X8} = 0x{value:X8}");
        }

        public byte Read8(uint offset)
        {
            Console.WriteLine($"[Interrupt Controller Read B] @ 0x{StartAddress + offset:X8}");
            return 0;
        }

        public void Write8(uint offset, byte value)
        {
            Console.WriteLine($"[Interrupt Controller Write B] @ 0x{StartAddress + offset:X8} = 0x{value:X2}");
        }
    }
}
