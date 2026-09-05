using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator
{
    public class RealQemuEmulator
    {
        private Process qemuProcess;
        private string qemuPath;
        private bool isRunning = false;

        public RealQemuEmulator()
        {
            qemuPath = QemuInstaller.FindQemuPath();
        }

        private async Task<bool> IsMipsArchitecture(string filePath)
        {
            try
            {
                var bytes = new byte[200];
                using (var fs = File.OpenRead(filePath))
                {
                    await fs.ReadAsync(bytes, 0, bytes.Length);
                }
                
                if (bytes[0] != 0x4D || bytes[1] != 0x5A)
                    return false;
                
                int peOffset = BitConverter.ToInt32(bytes, 60);
                if (peOffset >= bytes.Length - 6)
                    return false;
                
                ushort machineType = BitConverter.ToUInt16(bytes, peOffset + 4);
                return machineType == 0x166; // MIPS
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BootWinCEFirmware(string nkBinPath, string registryPath = null)
        {
            bool isMips = nkBinPath.Contains("Uverse", StringComparison.OrdinalIgnoreCase) || 
                         await IsMipsArchitecture(nkBinPath);
            
            string qemuExe = isMips ? "qemu-system-mips.exe" : "qemu-system-arm.exe";
            string actualQemuPath = qemuPath?.Replace("qemu-system-arm.exe", qemuExe).Replace("qemu-system-mips.exe", qemuExe);
            
            if (string.IsNullOrEmpty(actualQemuPath) || !File.Exists(actualQemuPath))
            {
                MessageBox.Show($"{qemuExe} not found!\n\nInstall QEMU with {(isMips ? "MIPS" : "ARM")} support.", 
                    "QEMU Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(nkBinPath))
            {
                MessageBox.Show($"Firmware file not found: {nkBinPath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                var args = new List<string>();
                
                if (isMips)
                {
                    args.AddRange(new[]
                    {
                        "-M", "malta",
                        "-cpu", "24Kf",
                        "-m", "256",
                        "-kernel", $"\"{nkBinPath}\"",
                        "-serial", "stdio",
                        "-display", "sdl",
                        "-no-reboot"
                    });
                }
                else
                {
                    args.AddRange(new[]
                    {
                        "-M", "versatilepb",
                        "-cpu", "arm1176",
                        "-m", "256",
                        "-kernel", $"\"{nkBinPath}\"",
                        "-serial", "stdio",
                        "-display", "sdl",
                        "-no-reboot"
                    });
                }

                if (!string.IsNullOrEmpty(registryPath) && File.Exists(registryPath))
                {
                    args.AddRange(new[] { "-drive", $"file={registryPath},format=raw,if=sd" });
                }

                var qemuArgs = string.Join(" ", args);
                Console.WriteLine($"QEMU Command: {actualQemuPath} {qemuArgs}");

                qemuProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = actualQemuPath,
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
                    
                    Console.WriteLine("QEMU process started successfully!");
                    MessageBox.Show($"QEMU {(isMips ? "MIPS" : "ARM")} Started!\n\n" +
                                  $"Kernel: {Path.GetFileName(nkBinPath)}\n" +
                                  $"Architecture: {(isMips ? "MIPS" : "ARM")}\n" +
                                  $"QEMU Path: {actualQemuPath}\n\n" +
                                  "QEMU window should open showing boot process.", 
                        "Real Firmware Boot", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    return true;
                }
                else
                {
                    MessageBox.Show($"Failed to start QEMU process\n\nCommand: {actualQemuPath}\nArgs: {qemuArgs}", 
                        "QEMU Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting QEMU: {ex.Message}", "QEMU Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public string GetQemuPath()
        {
            return qemuPath;
        }
    }
}