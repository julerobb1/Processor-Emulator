using System;

namespace ProcessorEmulator
{
    public class MocaTunersStub 
    {
        private readonly uint _baseAddress;

        public MocaTunersStub(uint baseAddress)
        {
            _baseAddress = baseAddress;
            Console.WriteLine($"MoCA Tuner Stub initialized at 0x{_baseAddress:X8}");
        }

        public uint Read(uint address)
        {
            uint offset = address - _baseAddress;
            Console.WriteLine($"MoCA Tuner Stub: Read from 0x{address:X8} (offset 0x{offset:X})");
            // Return a vendor/device ID or other meaningful value
            if (offset == 0x0)
            {
                return 0x2C0514F1; // Vendor ID 0x14F1, Device ID 0x2C05
            }
            return 0;
        }

        public void Write(uint address, uint value)
        {
            uint offset = address - _baseAddress;
            Console.WriteLine($"MoCA Tuner Stub: Write to 0x{address:X8} (offset 0x{offset:X}) with value 0x{value:X}");
            // Handle writes if necessary
        }
    }
}
