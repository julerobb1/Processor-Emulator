namespace ProcessorEmulator.Emulation
{
    public static class PlatformFactory
    {
        public static void ApplyConfiguration(string platform, MipsBus bus, CP0 cp0)
        {
            switch (platform.ToLower())
            {
                case "u-verse":
                    bus.IsBigEndian = true;
                    cp0.PRId = 0x0002A000; // BCM7405
                    bus.AddDevice(new RamDevice(0x00000000, 128 * 1024 * 1024)); // 128MB RAM
                    bus.AddDevice(new BcmUart(0x10400000)); // Standard BCM UART
                    bus.AddDevice(new BcmInterruptController(0x10000000));
                    bus.AddDevice(new DiscoveryDevice());
                    break;

                case "iguide":
                    bus.IsBigEndian = true;
                    cp0.PRId = 0x00020000; // BCM7401
                    bus.AddDevice(new NvRamDevice(0x1FD00000, 0x10000));
                    bus.AddDevice(new BcmUart(0xfffe0000)); // Different offset for older chips
                    break;

                case "wince_generic":
                    bus.IsBigEndian = false;
                    cp0.PRId = 0x00018000; // Generic 4Kc
                    break;
            }
        }
    }
}
