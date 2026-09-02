using System;

namespace ProcessorEmulator
{
    public class MocaTunersStub : IDeviceEmulator
    {
        public uint BaseAddress { get; }
        public uint Size => 0x1000; // 4KB size for the device

        public MocaTunersStub(uint baseAddress)
        {
            BaseAddress = baseAddress;
            Console.WriteLine($"MoCA Tuner Stub initialized at 0x{BaseAddress:X8}");
        }

        public uint Read(uint address)
        {
            uint offset = address - BaseAddress;
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
            uint offset = address - BaseAddress;
            Console.WriteLine($"MoCA Tuner Stub: Write to 0x{address:X8} (offset 0x{offset:X}) with value 0x{value:X}");
            // Handle writes if necessary
        }
    }
}
