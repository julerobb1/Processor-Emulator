using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ProcessorEmulator
{
    /// <summary>
    /// Integrates BOLT bootloader with the main emulator system
    /// Provides a bridge between BOLT simulation and HomebrewEmulator
    /// </summary>
    public class BoltEmulatorBridge
    {
        private Emulation.BoltBootloader bolt;
        private Emulation.HomebrewEmulator emulator;
        private bool bootInProgress;

        public BoltEmulatorBridge()
        {
            bolt = new Emulation.BoltBootloader();
            emulator = new Emulation.HomebrewEmulator();
            bootInProgress = false;
        }

        /// <summary>
        /// Initialize BOLT and prepare for firmware loading
        /// </summary>
        public async Task<bool> InitializeBolt()
        {
            try
            {
                await Task.Run(() => bolt.InitializeSoC());
                return bolt.IsInitialized;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BOLT initialization failed: {ex.Message}", "BOLT Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Boot firmware through BOLT -> HomebrewEmulator chain
        /// </summary>
        public async Task<bool> BootFirmware(string firmwarePath, string architecture = "ARM")
        {
            if (!bolt.IsInitialized)
            {
                ShowTextWindow("BOLT not initialized", "BOLT Error");
                return false;
            }

            if (bootInProgress)
            {
                ShowTextWindow("Boot already in progress", "BOLT Status");
                return false;
            }

            try
            {
                bootInProgress = true;
                ShowTextWindow($"BOLT: Starting boot sequence for {Path.GetFileName(firmwarePath)}", "BOLT Boot");

                // Step 1: Load firmware through BOLT
                bool loaded = await Task.Run(() => bolt.LoadELF(firmwarePath));
                if (!loaded)
                {
                    ShowTextWindow("BOLT: Firmware load failed", "BOLT Error");
                    return false;
                }

                // Step 2: Execute BOLT boot command
                string bootResult = bolt.ExecuteCommand($"boot -elf {firmwarePath}");
                ShowTextWindow($"BOLT: {bootResult}", "BOLT Boot");

                if (!bootResult.StartsWith("BOOT_SUCCESS"))
                {
                    return false;
                }

                // Step 3: Parse boot result and extract addresses
                string[] bootData = bootResult.Split(':');
                if (bootData.Length >= 3)
                {
                    uint entryPoint = Convert.ToUInt32(bootData[1], 16);
                    uint dtbAddress = Convert.ToUInt32(bootData[2], 16);

                    ShowTextWindow($"BOLT: Handoff to emulator - Entry: 0x{entryPoint:X8}, DTB: 0x{dtbAddress:X8}", "BOLT Handoff");

                    // Step 4: Transfer control to HomebrewEmulator
                    return await HandoffToEmulator(entryPoint, dtbAddress, architecture);
                }

                return false;
            }
            catch (Exception ex)
            {
                ShowTextWindow($"BOLT boot failed: {ex.Message}", "BOLT Error");
                return false;
            }
            finally
            {
                bootInProgress = false;
            }
        }

        /// <summary>
        /// Hand off from BOLT to the main emulator
        /// </summary>
        private async Task<bool> HandoffToEmulator(uint entryPoint, uint dtbAddress, string architecture)
        {
            try
            {
                // Get memory map from BOLT
                var memoryMap = bolt.GetMemoryMap();
                ShowTextWindow($"BOLT: Transferring {memoryMap.Count} memory entries to emulator", "BOLT Handoff");

                // Initialize emulator with BOLT's memory state
                await Task.Run(() => 
                {
                    ShowTextWindow($"Emulator: Received memory map from BOLT", "Emulator");
                    ShowTextWindow($"Emulator: Starting {architecture} emulation at 0x{entryPoint:X8}", "Emulator");
                    ShowTextWindow($"Emulator: Device tree available at 0x{dtbAddress:X8}", "Emulator");
                    
                    // 🔥 CRITICAL: Transfer BOLT's loaded firmware to HomebrewEmulator
                    TransferFirmwareToEmulator(memoryMap, entryPoint);
                    
                    // Start the actual emulation
                    emulator.StartEmulation(architecture);
                });

                return true;
            }
            catch (Exception ex)
            {
                ShowTextWindow($"Emulator handoff failed: {ex.Message}", "Emulator Error");
                return false;
            }
        }
        
        /// <summary>
        /// Transfer firmware memory from BOLT to HomebrewEmulator
        /// </summary>
        private void TransferFirmwareToEmulator(Dictionary<uint, uint> memoryMap, uint entryPoint)
        {
            try
            {
                // Convert BOLT's memory map to byte array for HomebrewEmulator
                const uint maxMemorySize = 0x8000000; // 128MB
                byte[] firmwareMemory = new byte[maxMemorySize];
                
                int transferredBytes = 0;
                
                foreach (var kvp in memoryMap)
                {
                    uint address = kvp.Key;
                    uint value = kvp.Value;
                    
                    // Only transfer memory that contains actual firmware (non-zero)
                    if (value != 0 && address < maxMemorySize - 4)
                    {
                        // Convert uint to little-endian bytes
                        byte[] valueBytes = BitConverter.GetBytes(value);
                        Array.Copy(valueBytes, 0, firmwareMemory, address, 4);
                        transferredBytes += 4;
                    }
                }
                
                ShowTextWindow($"Memory Transfer: {transferredBytes} bytes of firmware data", "BOLT->Emulator");
                ShowTextWindow($"Entry Point: 0x{entryPoint:X8}", "Firmware Info");
                
                // Load the firmware into HomebrewEmulator
                emulator.LoadBinary(firmwareMemory);
                
                ShowTextWindow("✅ Firmware transfer complete - emulator ready!", "BOLT Handoff");
            }
            catch (Exception ex)
            {
                ShowTextWindow($"Firmware transfer failed: {ex.Message}", "Transfer Error");
                throw;
            }
        }

        /// <summary>
        /// Execute BOLT CLI command
        /// </summary>
        public string ExecuteBoltCommand(string command)
        {
            if (!bolt.IsInitialized)
            {
                return "BOLT: Not initialized";
            }

            return bolt.ExecuteCommand(command);
        }

        /// <summary>
        /// Show BOLT CLI interface
        /// </summary>
        public void ShowBoltCLI()
        {
            if (!bolt.IsInitialized)
            {
                ShowTextWindow("BOLT not initialized. Run InitializeBolt() first.", "BOLT CLI");
                return;
            }

            var cliOutput = @"BOLT Command Line Interface
Available commands:
- boot -elf <path>     : Boot firmware image
- dump <addr> <len>    : Dump memory
- memtest              : Run memory test  
- dt show              : Show device tree
- dt add <path> <prop> <val> : Add DT property
- help                 : Show help

Enter 'exit' to close CLI.";

            ShowTextWindow(cliOutput, "BOLT CLI");
        }

        /// <summary>
        /// Get current BOLT status
        /// </summary>
        public string GetBoltStatus()
        {
            if (!bolt.IsInitialized)
            {
                return "BOLT Status: Not initialized";
            }

            return $@"BOLT Status: Initialized
Entry Point: 0x{bolt.EntryPoint:X8}  
DTB Address: 0x{bolt.DeviceTreeAddress:X8}
Boot Status: {(bootInProgress ? "In Progress" : "Ready")}";
        }

        /// <summary>
        /// Helper method to show text window (matches existing pattern)
        /// </summary>
        private void ShowTextWindow(string text, string title)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var window = new Window
                {
                    Title = title,
                    Width = 600,
                    Height = 400,
                    Content = new System.Windows.Controls.ScrollViewer
                    {
                        Content = new System.Windows.Controls.TextBlock
                        {
                            Text = text,
                            Margin = new Thickness(10),
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                };
                window.Show();
            });
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            bootInProgress = false;
            bolt = null;
            emulator = null;
        }
    }
}
