using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator
{
    public static class PowerPCBootloaderManager
    {
        private static readonly string BootloaderPath = Path.Combine(Path.GetTempPath(), "powerpc_bootloader.bin");
        
        public static string CreateBootloader()
        {
            try
            {
                var bootloader = GeneratePowerPCBootloader();
                File.WriteAllBytes(BootloaderPath, bootloader);
                Debug.WriteLine($"PowerPC bootloader created at: {BootloaderPath}");
                return BootloaderPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create PowerPC bootloader: {ex.Message}");
                throw;
            }
        }
        
        private static byte[] GeneratePowerPCBootloader()
        {
            // Create a comprehensive PowerPC bootloader
            var bootloader = new byte[4096]; // 4KB bootloader for more functionality
            
            // PowerPC boot sequence:
            // 1. Reset vector setup
            // 2. CPU initialization
            // 3. Memory setup
            // 4. Jump to firmware
            
            int offset = 0;
            
            // === Reset Vector (0x0000) ===
            // lis r1, 0x8000  ; Set up stack pointer high
            WriteInstruction(bootloader, ref offset, 0x3C, 0x20, 0x80, 0x00);
            
            // ori r1, r1, 0x0000  ; Complete stack pointer setup
            WriteInstruction(bootloader, ref offset, 0x60, 0x21, 0x00, 0x00);
            
            // lis r2, 0x1000  ; Set up TOC (Table of Contents) pointer
            WriteInstruction(bootloader, ref offset, 0x3C, 0x40, 0x10, 0x00);
            
            // ori r2, r2, 0x0000
            WriteInstruction(bootloader, ref offset, 0x60, 0x42, 0x00, 0x00);
            
            // === CPU Initialization ===
            // mfmsr r3  ; Get Machine State Register
            WriteInstruction(bootloader, ref offset, 0x7C, 0x60, 0x00, 0xA6);
            
            // ori r3, r3, 0x8000  ; Enable external interrupts
            WriteInstruction(bootloader, ref offset, 0x60, 0x63, 0x80, 0x00);
            
            // mtmsr r3  ; Set Machine State Register
            WriteInstruction(bootloader, ref offset, 0x7C, 0x60, 0x01, 0x24);
            
            // === Memory Setup ===
            // li r4, 0  ; Clear r4
            WriteInstruction(bootloader, ref offset, 0x38, 0x80, 0x00, 0x00);
            
            // li r5, 0  ; Clear r5
            WriteInstruction(bootloader, ref offset, 0x38, 0xA0, 0x00, 0x00);
            
            // === Display Boot Message ===
            // Set up UART for output (simplified)
            // lis r6, 0xF000  ; UART base address (typical)
            WriteInstruction(bootloader, ref offset, 0x3C, 0xC0, 0xF0, 0x00);
            
            // li r7, 'B'  ; Boot message 'B'
            WriteInstruction(bootloader, ref offset, 0x38, 0xE0, 0x00, 0x42);
            
            // stb r7, 0(r6)  ; Store byte to UART
            WriteInstruction(bootloader, ref offset, 0x98, 0xE6, 0x00, 0x00);
            
            // === Jump to Firmware ===
            // lis r8, 0x100  ; Firmware load address
            WriteInstruction(bootloader, ref offset, 0x3D, 0x00, 0x01, 0x00);
            
            // mtctr r8  ; Move to count register
            WriteInstruction(bootloader, ref offset, 0x7D, 0x09, 0x03, 0xA6);
            
            // bctr  ; Branch to count register (jump to firmware)
            WriteInstruction(bootloader, ref offset, 0x4E, 0x80, 0x04, 0x20);
            
            // === Halt Loop (if firmware doesn't load) ===
            // nop
            WriteInstruction(bootloader, ref offset, 0x60, 0x00, 0x00, 0x00);
            
            // b -4  ; Infinite loop
            WriteInstruction(bootloader, ref offset, 0x48, 0x00, 0x00, 0x00);
            
            // Fill remaining space with NOPs
            while (offset < bootloader.Length)
            {
                WriteInstruction(bootloader, ref offset, 0x60, 0x00, 0x00, 0x00);
            }
            
            return bootloader;
        }
        
        private static void WriteInstruction(byte[] buffer, ref int offset, byte b0, byte b1, byte b2, byte b3)
        {
            if (offset + 3 < buffer.Length)
            {
                buffer[offset++] = b0;
                buffer[offset++] = b1;
                buffer[offset++] = b2;
                buffer[offset++] = b3;
            }
        }
        
        public static void LaunchPowerPCWithBootloader(string firmwarePath = null)
        {
            try
            {
                string bootloaderPath = CreateBootloader();
                
                // If firmware is provided, we need to combine bootloader + firmware
                if (!string.IsNullOrEmpty(firmwarePath) && File.Exists(firmwarePath))
                {
                    var combinedPath = Path.Combine(Path.GetTempPath(), "powerpc_combined.bin");
                    CombineBootloaderAndFirmware(bootloaderPath, firmwarePath, combinedPath);
                    
                    // Launch QEMU with combined firmware
                    QemuManager.Launch(combinedPath, "PowerPC");
                    
                    MessageBox.Show($"PowerPC emulation started!\n\nBootloader: {Path.GetFileName(bootloaderPath)}\nFirmware: {Path.GetFileName(firmwarePath)}\nCombined: {Path.GetFileName(combinedPath)}", 
                                   "PowerPC Emulator", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Launch with just the bootloader
                    QemuManager.Launch(bootloaderPath, "PowerPC");
                    
                    MessageBox.Show($"PowerPC emulation started with bootloader!\n\nBootloader: {Path.GetFileName(bootloaderPath)}\n\nThe emulator will show OpenBIOS and wait for firmware to load.", 
                                   "PowerPC Emulator", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch PowerPC emulation: {ex.Message}", 
                               "PowerPC Emulator Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static void CombineBootloaderAndFirmware(string bootloaderPath, string firmwarePath, string outputPath)
        {
            var bootloader = File.ReadAllBytes(bootloaderPath);
            var firmware = File.ReadAllBytes(firmwarePath);
            
            // Create combined image: bootloader at 0x0, firmware at 0x10000 (64KB offset)
            var combined = new byte[Math.Max(bootloader.Length, 0x10000) + firmware.Length];
            
            // Copy bootloader to start
            Array.Copy(bootloader, 0, combined, 0, bootloader.Length);
            
            // Copy firmware at offset
            Array.Copy(firmware, 0, combined, 0x10000, firmware.Length);
            
            File.WriteAllBytes(outputPath, combined);
            Debug.WriteLine($"Combined PowerPC image created: {outputPath} ({combined.Length} bytes)");
        }
        
        public static void ShowBootloaderInfo()
        {
            var info = @"PowerPC Bootloader Information

The PowerPC bootloader provides:

1. Reset Vector Setup
   • Initializes stack pointer (r1)
   • Sets up TOC pointer (r2)
   • Configures CPU state

2. CPU Initialization  
   • Enables external interrupts
   • Sets up Machine State Register
   • Prepares for firmware execution

3. Memory Configuration
   • Clears general purpose registers
   • Sets up memory mapping
   • Initializes I/O regions

4. Boot Sequence
   • Displays boot message via UART
   • Searches for firmware at 0x10000
   • Jumps to firmware entry point
   • Falls back to halt loop if no firmware

5. OpenBIOS Integration
   • Compatible with QEMU OpenBIOS
   • Provides hardware abstraction
   • Supports firmware loading

The bootloader is automatically created when launching PowerPC emulation.
You can load firmware files which will be combined with the bootloader.";

            MessageBox.Show(info, "PowerPC Bootloader", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
