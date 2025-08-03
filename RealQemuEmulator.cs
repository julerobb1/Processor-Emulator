using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator
{
    /// <summary>
    /// Real QEMU-based firmware emulator for actual ARM/MIPS firmware booting
    /// This actually boots real firmware using QEMU, not fake simulation
    /// </summary>
    public class RealQemuEmulator
    {
        private Process qemuProcess;
        private string qemuPath;
        private bool isRunning = false;

        public RealQemuEmulator()
        {
            // Use the QemuInstaller to find QEMU
            qemuPath = QemuInstaller.FindQemuPath();
        }

        private async Task<bool> IsMipsArchitecture(string filePath)
        {
            try
            {
                // Read first 200 bytes to check PE header
                var bytes = new byte[200];
                using (var fs = File.OpenRead(filePath))
                {
                    await fs.ReadAsync(bytes, 0, bytes.Length);
                }
                
                // Check if it's a PE file (MZ header)
                if (bytes[0] != 0x4D || bytes[1] != 0x5A) // "MZ"
                    return false;
                
                // Get PE offset
                int peOffset = BitConverter.ToInt32(bytes, 60);
                if (peOffset >= bytes.Length - 6)
                    return false;
                
                // Read machine type from PE header
                ushort machineType = BitConverter.ToUInt16(bytes, peOffset + 4);
                
                // 0x166 = MIPS, 0x1c2 = ARM
                return machineType == 0x166;
            }
            catch
            {
                return false;
            }
        }

        private void FindQemuInstallation()
        {
            // Removed - now using QemuInstaller.FindQemuPath()
        }

        public async Task<bool> BootWinCEFirmware(string nkBinPath, string registryPath = null)
        {
            // Determine if this is MIPS or ARM based on file path and content
            bool isMips = nkBinPath.Contains("Uverse", StringComparison.OrdinalIgnoreCase) || 
                         await IsMipsArchitecture(nkBinPath);
            
            // Find appropriate QEMU executable
            string qemuExe = isMips ? "qemu-system-mips.exe" : "qemu-system-arm.exe";
            string actualQemuPath = qemuPath?.Replace("qemu-system-arm.exe", qemuExe).Replace("qemu-system-mips.exe", qemuExe);
            
            if (string.IsNullOrEmpty(actualQemuPath) || !File.Exists(actualQemuPath))
            {
                MessageBox.Show($"{qemuExe} not found!\n\nPlease install QEMU with {(isMips ? "MIPS" : "ARM")} support.", 
                    "QEMU Required", MessageBoxButton.OK, MessageBoxImage.Error);
                await QemuInstaller.TryAutoInstallViaChocolatey();
                return false;
            }

            if (!File.Exists(nkBinPath))
            {
                MessageBox.Show($"Firmware file not found: {nkBinPath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                // Create QEMU arguments based on architecture
                var args = new List<string>();
                
                if (isMips)
                {
                    // MIPS configuration for U-verse and similar devices
                    args.AddRange(new[]
                    {
                        "-M", "malta",        // MIPS Malta platform board
                        "-cpu", "24Kf",       // MIPS 24Kf CPU
                        "-m", "256",          // 256MB RAM
                        "-kernel", $"\"{nkBinPath}\"",  // WinCE kernel
                        "-serial", "stdio",   // Serial output to console
                        "-display", "sdl",    // SDL graphics display
                        "-no-reboot"          // Don't reboot on crash
                    });
                }
                else
                {
                    // ARM configuration for other devices
                    args.AddRange(new[]
                    {
                        "-M", "versatilepb",  // ARM versatile platform board
                        "-cpu", "arm1176",    // ARM11 CPU
                        "-m", "256",          // 256MB RAM
                        "-kernel", $"\"{nkBinPath}\"",  // WinCE kernel
                        "-serial", "stdio",   // Serial output to console
                        "-display", "sdl",    // SDL graphics display
                        "-no-reboot"          // Don't reboot on crash
                    });
                }

                // If we have registry/filesystem files, add them
                if (!string.IsNullOrEmpty(registryPath) && File.Exists(registryPath))
                {
                    args.AddRange(new[] { "-drive", $"file={registryPath},format=raw,if=sd" });
                }

                var qemuArgs = string.Join(" ", args);
                
                Console.WriteLine($"Starting QEMU with: {qemuPath} {qemuArgs}");

                qemuProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = actualQemuPath,  // Use architecture-specific QEMU
                        Arguments = qemuArgs,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                qemuProcess.OutputDataReceived += (s, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"QEMU: {e.Data}");
                };

                qemuProcess.ErrorDataReceived += (s, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"QEMU ERR: {e.Data}");
                };

                bool started = qemuProcess.Start();
                if (started)
                {
                    qemuProcess.BeginOutputReadLine();
                    qemuProcess.BeginErrorReadLine();
                    isRunning = true;
                    
                    MessageBox.Show($"QEMU started successfully!\n\nBooting WinCE firmware: {Path.GetFileName(nkBinPath)}\n\nQEMU window should open showing the boot process.\nWatch the console for boot messages.", 
                        "QEMU Boot Started", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    return true;
                }
                else
                {
                    MessageBox.Show("Failed to start QEMU process", "QEMU Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting QEMU: {ex.Message}\n\nStack: {ex.StackTrace}", 
                    "QEMU Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> BootMipsFirmware(string firmwarePath)
        {
            if (string.IsNullOrEmpty(qemuPath))
            {
                await QemuInstaller.TryAutoInstallViaChocolatey();
                return false;
            }

            try
            {
                // Use MIPS QEMU for MIPS firmware
                var mipsQemu = qemuPath.Replace("qemu-system-arm", "qemu-system-mips");
                
                var args = new List<string>
                {
                    "-M", "malta",        // MIPS Malta board
                    "-cpu", "24Kf",       // MIPS 24K CPU
                    "-m", "256",          // 256MB RAM
                    "-kernel", $"\"{firmwarePath}\"",
                    "-serial", "stdio",
                    "-display", "sdl",
                    "-no-reboot"
                };

                var qemuArgs = string.Join(" ", args);
                
                qemuProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = mipsQemu,
                        Arguments = qemuArgs,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                bool started = qemuProcess.Start();
                if (started)
                {
                    isRunning = true;
                    MessageBox.Show($"QEMU MIPS started!\n\nBooting firmware: {Path.GetFileName(firmwarePath)}", 
                        "QEMU Boot Started", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QEMU MIPS Error: {ex.Message}", "QEMU Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                if (qemuProcess != null && !qemuProcess.HasExited)
                {
                    qemuProcess.Kill();
                    qemuProcess.WaitForExit(5000);
                }
                isRunning = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping QEMU: {ex.Message}");
            }
        }

        public bool IsRunning => isRunning && qemuProcess != null && !qemuProcess.HasExited;

        public string GetQemuPath() => qemuPath;
    }
}
