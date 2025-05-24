using System;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator.Emulation
{
    public static class EmulatorLauncher
    {
        public static void Launch(string binaryPath, string architecture, string platform = null, bool requireHardware = false)
        {
            // Use QEMU for most architectures, fallback to custom emulator if implemented
            if (requireHardware || platform == "RDK-B" || platform == "RDK-V")
            {
                // QEMU with hardware passthrough (example: USB, network)
                var qemu = new QemuManager();
                string extraArgs = "";
                if (requireHardware)
                {
                    extraArgs = "-usb -device usb-host -net user -net nic"; // Example, customize as needed
                }
                qemu.LaunchWithArgs(binaryPath, architecture, extraArgs);
            }
            else
            {
                switch (architecture)
                {
                    case "MIPS32": new Mips32Emulator().Run(); break;
                    case "ARM": new ArmEmulator().Run(); break;
                    case "ARM64": new Arm64Emulator().Run(); break;
                    case "PowerPC": new PowerPcEmulator().Run(); break;
                    case "x86": new X86Emulator().Run(); break;
                    case "x86-64": new X64Emulator().Run(); break;
                    default:
                        // Fallback to QEMU if custom emulator not implemented
                        var qemu = new QemuManager();
                        qemu.Launch(binaryPath, architecture);
                        break;
                }
            }
        }
    }
}
