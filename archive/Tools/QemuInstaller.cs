using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// QEMU installation manager and setup helper
    /// Helps users install and configure QEMU for real firmware emulation
    /// </summary>
    public static class QemuInstaller
    {
        private static readonly string[] QemuPaths = 
        {
            @"C:\Program Files\qemu\qemu-system-mips.exe",  // MIPS for U-verse
            @"C:\Program Files\qemu\qemu-system-arm.exe",
            @"C:\qemu\qemu-system-mips.exe",
            @"C:\qemu\qemu-system-arm.exe",
            @"C:\msys64\mingw64\bin\qemu-system-mips.exe",
            @"C:\msys64\mingw64\bin\qemu-system-arm.exe",
            @"C:\tools\qemu\qemu-system-mips.exe",
            @"C:\tools\qemu\qemu-system-arm.exe"
        };

        public static bool IsQemuInstalled()
        {
            // Check common installation paths
            foreach (var path in QemuPaths)
            {
                if (File.Exists(path))
                    return true;
            }

            // Check if in PATH
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "qemu-system-arm",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static string FindQemuPath()
        {
            foreach (var path in QemuPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "qemu-system-arm",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    return process.StandardOutput.ReadToEnd().Trim();
                }
            }
            catch { }

            return null;
        }

        public static void ShowInstallationInstructions()
        {
            var message = @"QEMU Required for Real Firmware Emulation

To boot real ARM/MIPS firmware, you need QEMU installed:

📥 EASY INSTALLATION:
1. Using Chocolatey (Recommended):
   • Open PowerShell as Administrator
   • Run: choco install qemu
   
2. Manual Download:
   • Visit: https://qemu.weilnetz.de/w64/
   • Download latest QEMU for Windows
   • Install to C:\Program Files\qemu\

3. Alternative - MSYS2:
   • Install MSYS2 from https://msys2.org/
   • Run: pacman -S mingw-w64-x86_64-qemu

WHAT THIS ENABLES:
Boot real ARM firmware (U-verse, DirectTV)
Boot real MIPS firmware (Set-top boxes)
Graphics display of actual boot process
Real hardware emulation, not simulation

⚡ AFTER INSTALLATION:
• Restart this application
• Select real firmware files (.bin, .exe)
• Watch actual firmware boot in QEMU window!";

            MessageBox.Show(message, "Install QEMU for Real Emulation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static Task<bool> TryAutoInstallViaChocolatey()
        {
            try
            {
                var result = MessageBox.Show(
                    "Would you like to automatically install QEMU using Chocolatey?\n\n" +
                    "This requires Administrator privileges and Chocolatey to be installed.\n\n" +
                    "Click Yes to attempt automatic installation, or No for manual instructions.",
                    "Auto-Install QEMU", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Start-Process powershell -ArgumentList 'choco install qemu -y' -Verb RunAs\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    MessageBox.Show(
                        "QEMU installation started!\n\n" +
                        "• A PowerShell window will open for installation\n" +
                        "• Wait for installation to complete\n" +
                        "• Restart this application after installation\n" +
                        "• You'll then be able to boot real firmware!",
                        "Installation Started", MessageBoxButton.OK, MessageBoxImage.Information);

                    return Task.FromResult(true);
                }
                else
                {
                    ShowInstallationInstructions();
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Auto-installation failed: {ex.Message}\n\nPlease install manually.", 
                    "Installation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowInstallationInstructions();
                return Task.FromResult(false);
            }
        }

        public static string GetQemuStatus()
        {
            if (IsQemuInstalled())
            {
                var path = FindQemuPath();
                return $"QEMU Installed: {path}";
            }
            else
            {
                return "QEMU Not Found - Real firmware emulation disabled";
            }
        }
    }
}
