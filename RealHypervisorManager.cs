using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator
{
    public class RealHypervisorManager
    {
        private readonly string _qemuPath = @"C:\Program Files\qemu";
        private readonly string _tempDir;
        
        public RealHypervisorManager()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "ProcessorEmulator");
            Directory.CreateDirectory(_tempDir);
        }

        public async Task<bool> BootFirmwareFile(string firmwarePath)
        {
            try
            {
                var firmware = new FileInfo(firmwarePath);
                var platformInfo = DetectPlatformFromFilename(firmware.Name);
                
                MessageBox.Show($"Real Hypervisor Boot Analysis\n\n" +
                    $"=== REAL HYPERVISOR BOOT SEQUENCE ===\n" +
                    $"Firmware File: {firmware.Name}\n" +
                    $"Size: {firmware.Length:N0} bytes\n" +
                    $"Detected Platform: {platformInfo.Platform}\n" +
                    $"Architecture: {platformInfo.Architecture}\n" +
                    $"CPU: {platformInfo.CPU}\n\n" +
                    $"=== UNPACKING FIRMWARE ===");

                // Step 1: Try to unpack firmware container (ARRIS PACK1 format)
                var sections = await UnpackFirmware(firmwarePath);
                
                ExtractedFiles extractedFiles;
                if (sections == null || sections.Count == 0)
                {
                    // Not a PACK1 container - treat as raw firmware binary
                    MessageBox.Show($"Firmware is not PACK1 format - treating as raw binary\n" +
                                  $"Will boot directly with QEMU using raw firmware image", "Raw Firmware Boot");
                    
                    extractedFiles = new ExtractedFiles
                    {
                        KernelPath = firmwarePath, // Use raw firmware as kernel
                        RootfsPath = null,
                        DtbPath = null
                    };
                }
                else
                {
                    // Step 2: Extract kernel and rootfs from PACK1 sections
                var extractedFiles = await ExtractBootableComponents(sections, platformInfo);
                if (extractedFiles.KernelPath == null)
                {
                    MessageBox.Show("No bootable kernel found in firmware", "Hypervisor Error");
                    return false;
                }

                // Step 3: Launch real QEMU hypervisor
                await LaunchQemuHypervisor(extractedFiles, platformInfo);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Boot failed: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Hypervisor Error");
                return false;
            }
        }

        private async Task<System.Collections.Generic.List<FirmwareUnpacker.FirmwareSection>> UnpackFirmware(string firmwarePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return FirmwareUnpacker.UnpackARRISFirmware(firmwarePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to unpack firmware: {ex.Message}", "Unpacking Error");
                    return null;
                }
            });
        }

        private async Task<ExtractedFiles> ExtractBootableComponents(
            System.Collections.Generic.List<FirmwareUnpacker.FirmwareSection> sections, 
            PlatformInfo platformInfo)
        {
            return await Task.Run(() =>
            {
                var extracted = new ExtractedFiles();
                var outputLog = "=== FIRMWARE SECTION ANALYSIS ===\n";

                foreach (var section in sections)
                {
                    outputLog += $"\nSection: {section.Name}\n";
                    outputLog += $"  Size: {section.Size:N0} bytes\n";
                    outputLog += $"  Platform: {section.Platform}\n";
                    
                    // Detect section type
                    var sectionType = AnalyzeSectionType(section);
                    outputLog += $"  Type: {sectionType}\n";

                    // Extract based on type
                    var outputPath = Path.Combine(_tempDir, $"{section.Name}_{section.Platform}");
                    
                    if (sectionType.Contains("Kernel") && extracted.KernelPath == null)
                    {
                        var kernelPath = outputPath + "_kernel.bin";
                        FirmwareUnpacker.ExtractKernelFromSection(section, kernelPath);
                        extracted.KernelPath = kernelPath;
                        outputLog += $"  Extracted kernel to: {kernelPath}\n";
                    }
                    else if (sectionType.Contains("Filesystem"))
                    {
                        var fsPath = outputPath + "_rootfs.img";
                        File.WriteAllBytes(fsPath, section.Data);
                        extracted.RootfsPath = fsPath;
                        outputLog += $"  Extracted filesystem to: {fsPath}\n";
                    }
                    else
                    {
                        var dataPath = outputPath + ".bin";
                        File.WriteAllBytes(dataPath, section.Data);
                        outputLog += $"  Extracted data to: {dataPath}\n";
                    }
                }

                MessageBox.Show(outputLog, "Firmware Extraction Results");

                return extracted;
            });
        }

        private string AnalyzeSectionType(FirmwareUnpacker.FirmwareSection section)
        {
            var data = section.Data;
            if (data.Length < 64) return "Unknown (too small)";

            // Check for uImage header
            if (data[0] == 0x27 && data[1] == 0x05 && data[2] == 0x19 && data[3] == 0x56)
                return "U-Boot Kernel Image";

            // Check for ELF header
            if (data[0] == 0x7F && data[1] == 0x45 && data[2] == 0x4C && data[3] == 0x46)
                return "ELF Executable/Kernel";

            // Check for Linux kernel signature
            for (int i = 0; i < Math.Min(data.Length - 5, 1024); i++)
            {
                if (data[i] == 0x4C && data[i + 1] == 0x69 && data[i + 2] == 0x6E && 
                    data[i + 3] == 0x75 && data[i + 4] == 0x78) // "Linux"
                    return "Linux Kernel";
            }

            // Check for common filesystem signatures
            if (CheckFilesystemSignature(data, "hsqs")) return "SquashFS Filesystem";
            if (CheckFilesystemSignature(data, "YAFFS")) return "YAFFS2 Filesystem";
            if (CheckFilesystemSignature(data, "\x53\xEF")) return "ext2/ext3/ext4 Filesystem";

            // Check for bootloader signatures
            if (section.Name.ToLowerInvariant().Contains("boot")) return "Bootloader";
            if (section.Name.ToLowerInvariant().Contains("kernel")) return "Kernel Image";
            if (section.Name.ToLowerInvariant().Contains("rootfs")) return "Root Filesystem";

            return "Unknown Binary Data";
        }

        private bool CheckFilesystemSignature(byte[] data, string signature)
        {
            var sigBytes = System.Text.Encoding.ASCII.GetBytes(signature);
            for (int i = 0; i <= data.Length - sigBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sigBytes.Length; j++)
                {
                    if (data[i + j] != sigBytes[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
            return false;
        }

        private async Task LaunchQemuHypervisor(ExtractedFiles extracted, PlatformInfo platformInfo)
        {
            var qemuExecutable = GetQemuExecutable(platformInfo.Architecture);
            var qemuArgs = BuildQemuArguments(extracted, platformInfo);

            var bootLog = $"=== LAUNCHING REAL HYPERVISOR ===\n" +
                         $"QEMU Executable: {qemuExecutable}\n" +
                         $"Architecture: {platformInfo.Architecture}\n" +
                         $"Platform: {platformInfo.Platform}\n" +
                         $"Kernel: {extracted.KernelPath}\n" +
                         $"Root FS: {extracted.RootfsPath ?? "None"}\n\n" +
                         $"QEMU Command Line:\n{qemuExecutable} {qemuArgs}\n\n" +
                         $"=== HYPERVISOR STARTING ===\n" +
                         $"The firmware will boot in a separate QEMU window.\n" +
                         $"Watch for:\n" +
                         $"• Boot messages and kernel output\n" +
                         $"• Device initialization\n" +
                         $"• Filesystem mounting\n" +
                         $"• Application startup\n\n" +
                         $"Use Ctrl+Alt+2 in QEMU for monitor console\n" +
                         $"Use Ctrl+Alt+1 to return to main console";

            MessageBox.Show(bootLog, "🚀 LAUNCHING REAL QEMU HYPERVISOR 🚀", MessageBoxButton.OK, MessageBoxImage.Information);

            await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = qemuExecutable,
                        Arguments = qemuArgs,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WorkingDirectory = _tempDir
                    }
                };

                process.Start();
                
                // Give QEMU time to start
                System.Threading.Thread.Sleep(2000);
                
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show("QEMU hypervisor launched successfully!\n\n" +
                        "If the QEMU window doesn't appear, check:\n" +
                        "1. Firewall settings\n" +
                        "2. Graphics drivers\n" +
                        "3. QEMU installation\n\n" +
                        "The firmware is now booting in real hardware emulation.", "✅ HYPERVISOR LAUNCHED!"));
            });
        }

        private string GetQemuExecutable(string architecture)
        {
            var executable = architecture.ToUpperInvariant() switch
            {
                "ARM" => "qemu-system-arm.exe",
                "ARM64" => "qemu-system-aarch64.exe", 
                "MIPS" => "qemu-system-mips.exe",
                "MIPS64" => "qemu-system-mips64.exe",
                "MIPSEL" => "qemu-system-mipsel.exe",
                "POWERPC" => "qemu-system-ppc.exe",
                "X86" => "qemu-system-i386.exe",
                "X86_64" => "qemu-system-x86_64.exe",
                _ => "qemu-system-arm.exe" // Default to ARM
            };

            return Path.Combine(_qemuPath, executable);
        }

        private string BuildQemuArguments(ExtractedFiles extracted, PlatformInfo platformInfo)
        {
            var args = platformInfo.Architecture.ToUpperInvariant() switch
            {
                "ARM" => BuildArmArguments(extracted, platformInfo),
                "MIPS" or "MIPSEL" => BuildMipsArguments(extracted, platformInfo),
                "POWERPC" => BuildPowerPCArguments(extracted, platformInfo),
                _ => BuildArmArguments(extracted, platformInfo) // Default
            };

            // Add common debugging and monitoring options
            args += " -monitor stdio -serial vc:800x600";
            
            return args;
        }

        private string BuildArmArguments(ExtractedFiles extracted, PlatformInfo platformInfo)
        {
            var machine = platformInfo.Platform.Contains("BCM7445") ? "virt" : "vexpress-a15";
            var cpu = platformInfo.CPU ?? "cortex-a15";
            
            var args = $"-M {machine} -cpu {cpu} -m 512M";
            
            if (File.Exists(extracted.KernelPath))
            {
                args += $" -kernel \"{extracted.KernelPath}\"";
                args += " -append \"console=ttyAMA0,115200 root=/dev/ram0 rw\"";
            }
            
            if (!string.IsNullOrEmpty(extracted.RootfsPath) && File.Exists(extracted.RootfsPath))
            {
                args += $" -drive file=\"{extracted.RootfsPath}\",if=sd,format=raw";
            }
            
            return args;
        }

        private string BuildMipsArguments(ExtractedFiles extracted, PlatformInfo platformInfo)
        {
            var args = "-M malta -cpu 24Kf -m 256M";
            
            if (File.Exists(extracted.KernelPath))
            {
                args += $" -kernel \"{extracted.KernelPath}\"";
                args += " -append \"console=ttyS0,115200 root=/dev/hda1 rw\"";
            }
            
            if (!string.IsNullOrEmpty(extracted.RootfsPath) && File.Exists(extracted.RootfsPath))
            {
                args += $" -drive file=\"{extracted.RootfsPath}\",if=ide,format=raw";
            }
            
            return args;
        }

        private string BuildPowerPCArguments(ExtractedFiles extracted, PlatformInfo platformInfo)
        {
            var args = "-M g3beige -cpu G3 -m 256M";
            
            if (File.Exists(extracted.KernelPath))
            {
                args += $" -kernel \"{extracted.KernelPath}\"";
                args += " -append \"console=ttyS0,115200 root=/dev/hda1 rw\"";
            }
            
            if (!string.IsNullOrEmpty(extracted.RootfsPath) && File.Exists(extracted.RootfsPath))
            {
                args += $" -drive file=\"{extracted.RootfsPath}\",if=ide,format=raw";
            }
            
            return args;
        }

        private PlatformInfo DetectPlatformFromFilename(string filename)
        {
            var info = new PlatformInfo();
            
            if (filename.Contains("AX014AN"))
            {
                info.Platform = "Arris XG1v4";
                info.Architecture = "ARM";
                info.CPU = "BCM7445 (Cortex-A15)";
            }
            else if (filename.Contains("CXD01ANI"))
            {
                info.Platform = "Cisco XiD-P";
                info.Architecture = "ARM";
                info.CPU = "BCM7445 (Cortex-A15)";
            }
            else if (filename.Contains("PX013AN"))
            {
                info.Platform = "Pace XG1v3";
                info.Architecture = "ARM";
                info.CPU = "BCM7445 (Cortex-A15)";
            }
            else
            {
                info.Platform = "Unknown STB";
                info.Architecture = "ARM";
                info.CPU = "Cortex-A15";
            }
            
            return info;
        }

        private class ExtractedFiles
        {
            public string KernelPath { get; set; }
            public string RootfsPath { get; set; }
        }

        private class PlatformInfo
        {
            public string Platform { get; set; }
            public string Architecture { get; set; }
            public string CPU { get; set; }
        }
    }
}
