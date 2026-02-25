using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading;
using ProcessorEmulator.Emulation;
using System.Windows.Forms;

namespace ProcessorEmulator
{
    public partial class App : System.Windows.Application
    {
        private static string StartupLogPath => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ProcessorEmulator_startup.log");
        private static void Log(string line)
        {
            try { System.IO.File.AppendAllText(StartupLogPath, DateTime.Now.ToString("o") + " " + line + Environment.NewLine); } catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // The original CLI argument handling can remain if needed.
            if (e.Args.Length > 0)
            {
                // ... (CLI logic as before) ...
                Environment.Exit(0);
                return;
            }

            // --- WinForms Pivot ---
            // The WPF Application object still runs, but we don't show a WPF window.
            // Instead, we launch our WinForms console.
            
            var console = new EmulatorConsole();
            console.Show();

            console.FormClosed += (s, args) => {
                System.Windows.Application.Current.Shutdown();
            };

            // --- Emulator Setup ---
            var bus = new MipsBus();
            var cp0 = new CP0();
            var uart = new UniversalUart();
            var cpu = new MipsCpuEmulator(bus, cp0);
            
            // Configure the platform
            PlatformFactory.ApplyConfiguration("u-verse", bus, cp0);
            
            // The PlatformFactory in this example adds a UART, but if it didn't, we would add it here.
            // For this example, let's assume the U-Verse config in the factory adds the UART.
            // We still need to subscribe to its output.
            var uartDevice = bus.Devices.OfType<UniversalUart>().FirstOrDefault();
            Program.CurrentUart = uartDevice; // Make it globally accessible for the console
            if (uartDevice != null)
            {
                uartDevice.OnCharReceived += console.AppendChar;
            }

            // --- Determine firmware path ---
            // 1. look at config file created by the user
            // 2. fallback to environment variable FIRMWARE_PATH
            // 3. as a last resort try scanning common folders
            string nkBinPath = ConfigManager.Config.FirmwarePath;
            if (string.IsNullOrEmpty(nkBinPath))
            {
                nkBinPath = Environment.GetEnvironmentVariable("FIRMWARE_PATH") ?? string.Empty;
            }

            if (string.IsNullOrEmpty(nkBinPath) ||
                !(File.Exists(nkBinPath) || Directory.Exists(nkBinPath)))
            {
                // perform a one‑time gentle search in Downloads and Documents
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                nkBinPath = FirmwareScanner.FindKernelGently(Path.Combine(home, "Downloads"))
                            ?? FirmwareScanner.FindKernelGently(Path.Combine(home, "Documents"));

                if (!string.IsNullOrEmpty(nkBinPath))
                {
                    Log($"Auto‑discovered firmware at {nkBinPath}, saving config");
                    ConfigManager.Config.FirmwarePath = nkBinPath;
                    ConfigManager.Save();
                }
            }

            // directory‑vs‑file detection (previous logic)
            if (Directory.Exists(nkBinPath))
            {
                Log($"Startup: '{nkBinPath}' is a directory, searching for kernel exe");
                var candidate = Directory.EnumerateFiles(nkBinPath, "nk.exe",
                    SearchOption.AllDirectories).FirstOrDefault();
                if (candidate != null)
                {
                    Log($"Found kernel image at {candidate}");
                    nkBinPath = candidate;
                }
                else
                {
                    Log("Warning: directory provided but no nk.exe found – will fall back to dummy");
                }
            }

            if (!File.Exists(nkBinPath))
            {
                // fallback to local copy (dummy program)
                nkBinPath = "nk.bin";
                if (!File.Exists(nkBinPath))
                {
                    // Create a simple MIPS program:
                    // 0x3c01bfc0  lui at, 0xbfc0
                    // 0x24220000  addiu v0, at, 0
                    // loop:
                    // 0x10400001  beq v0, zero, loop
                    byte[] dummyBin = {
                        0x3c, 0x01, 0xbf, 0xc0,
                        0x24, 0x22, 0x00, 0x00,
                        0x10, 0x40, 0x00, 0x01 
                    };
                    File.WriteAllBytes(nkBinPath, dummyBin);
                }
            }
            
            byte[] osImage = File.ReadAllBytes(nkBinPath);
            // Load it at the physical address corresponding to the reset vector
            bus.LoadData(0x1FC00000, osImage);

            // --- Start Emulation ---
            Thread emulationThread = new Thread(() => {
                try
                {
                    cpu.Run();
                }
                catch (Exception ex)
                {
                    console.AppendText($"\n--- FATAL EMULATOR ERROR ---\n{ex.Message}\n{ex.StackTrace}");
                }
            });
            emulationThread.IsBackground = true; // Ensure thread exits when app closes
            emulationThread.Start();
        }
    }
}