using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProcessorEmulator.Emulation.SoC
{
    /// <summary>
    /// Base class for emulating BCM7449 SoC peripherals found in ARRIS XG1V4 RDK-V devices.
    /// Provides MMIO address mapping and read/write handling for various on-chip peripherals.
    /// </summary>
    public abstract class Bcm7449PeripheralStub
    {
        protected readonly string PeripheralName;
        public bool VerboseLogging { get; set; } = true;
        
        // BCM7449 known MMIO peripheral addresses (based on Broadcom documentation)
        protected static readonly Dictionary<uint, string> KnownMmioAddresses = new Dictionary<uint, string>
        {
            // Core system peripherals
            { 0x10400000, "System Control" },
            { 0x10410000, "Clock Control" },
            { 0x10420000, "Reset Control" },
            { 0x10430000, "Power Management" },
            
            // Security and boot
            { 0x10440000, "Secure Boot Controller" },
            { 0x10441000, "Hardware Security Module" },
            { 0x10442000, "OTP Controller" },
            { 0x10443000, "Random Number Generator" },
            
            // Crypto engines
            { 0x10500000, "AES Crypto Engine" },
            { 0x10501000, "DES/3DES Engine" },
            { 0x10502000, "SHA Hash Engine" },
            { 0x10503000, "RSA Public Key Engine" },
            { 0x10504000, "PKE (Public Key Engine)" },
            
            // Network and connectivity
            { 0x10600000, "MoCA 2.0 Controller" },
            { 0x10601000, "MoCA PHY" },
            { 0x10602000, "MoCA MAC" },
            { 0x10610000, "Ethernet Controller" },
            { 0x10620000, "USB 3.0 Controller" },
            { 0x10630000, "PCIe Controller" },
            
            // Video and display
            { 0x10700000, "HDMI TX Controller" },
            { 0x10701000, "HDMI RX Controller" },
            { 0x10702000, "HDMI CEC" },
            { 0x10703000, "HDMI HDCP 2.2" },
            { 0x10710000, "Video Decoder 0" },
            { 0x10711000, "Video Decoder 1" },
            { 0x10720000, "Video Encoder" },
            { 0x10730000, "Graphics 3D Engine" },
            { 0x10740000, "Display Controller" },
            
            // Audio
            { 0x10800000, "Audio DSP" },
            { 0x10801000, "Audio Mixer" },
            { 0x10802000, "Audio I2S" },
            { 0x10803000, "Audio SPDIF" },
            
            // Cable/Satellite tuning
            { 0x10900000, "Frontend Tuner 0" },
            { 0x10901000, "Frontend Tuner 1" },
            { 0x10902000, "Frontend Tuner 2" },
            { 0x10903000, "Frontend Tuner 3" },
            { 0x10910000, "Demodulator QAM" },
            { 0x10920000, "Transport Stream Processor" },
            
            // CableCARD and conditional access
            { 0x10A00000, "CableCARD Interface" },
            { 0x10A01000, "CableCARD Security" },
            { 0x10A02000, "Conditional Access" },
            { 0x10A03000, "DRM Controller" },
            
            // Memory controllers
            { 0x10B00000, "DDR3 Memory Controller" },
            { 0x10B01000, "Memory Scheduler" },
            { 0x10B02000, "Memory QoS" },
            
            // Storage
            { 0x10C00000, "SATA Controller" },
            { 0x10C10000, "NAND Flash Controller" },
            { 0x10C20000, "SPI Flash Controller" },
            
            // General purpose I/O
            { 0x10D00000, "GPIO Controller 0" },
            { 0x10D01000, "GPIO Controller 1" },
            { 0x10D10000, "I2C Controller 0" },
            { 0x10D11000, "I2C Controller 1" },
            { 0x10D20000, "SPI Controller" },
            { 0x10D30000, "UART 0" },
            { 0x10D31000, "UART 1" },
            { 0x10D32000, "UART 2" },
            
            // Timers and interrupts
            { 0x10E00000, "Timer Controller" },
            { 0x10E10000, "Watchdog Timer" },
            { 0x10E20000, "Interrupt Controller" },
            
            // Debug and trace
            { 0x10F00000, "Debug UART" },
            { 0x10F10000, "JTAG Controller" },
            { 0x10F20000, "Performance Counters" }
        };
        
        protected Bcm7449PeripheralStub(string peripheralName)
        {
            PeripheralName = peripheralName;
        }
        
        /// <summary>
        /// Handle read from MMIO address. Override in derived classes for specific behavior.
        /// </summary>
        /// <param name="address">32-bit MMIO address</param>
        /// <returns>32-bit value read from address</returns>
        public virtual uint HandleRead(uint address)
        {
            LogAccess("READ", address, 0);
            
            // Default behavior: return 0 for unknown addresses
            return 0x00000000;
        }
        
        /// <summary>
        /// Handle write to MMIO address. Override in derived classes for specific behavior.
        /// </summary>
        /// <param name="address">32-bit MMIO address</param>
        /// <param name="value">32-bit value to write</param>
        public virtual void HandleWrite(uint address, uint value)
        {
            LogAccess("WRITE", address, value);
            
            // Default behavior: no-op for writes
        }
        
        /// <summary>
        /// Check if an address is within this peripheral's range.
        /// Override in derived classes to define address ranges.
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <returns>True if address is handled by this peripheral</returns>
        public virtual bool HandlesAddress(uint address)
        {
            return KnownMmioAddresses.ContainsKey(address);
        }
        
        /// <summary>
        /// Log memory access with optional verbose details.
        /// </summary>
        protected void LogAccess(string operation, uint address, uint value)
        {
            string peripheralInfo = GetPeripheralInfo(address);
            
            if (VerboseLogging)
            {
                if (operation == "READ")
                {
                    Debug.WriteLine($"[{PeripheralName}] READ  0x{address:X8} = 0x{value:X8} ({peripheralInfo})");
                }
                else
                {
                    Debug.WriteLine($"[{PeripheralName}] WRITE 0x{address:X8} = 0x{value:X8} ({peripheralInfo})");
                }
            }
            
            // Log unknown addresses separately
            if (peripheralInfo.StartsWith("Unknown"))
            {
                Debug.WriteLine($"[BCM7449] WARNING: {operation} to unknown address 0x{address:X8} in {PeripheralName}");
            }
        }
        
        /// <summary>
        /// Get human-readable information about an MMIO address.
        /// </summary>
        protected string GetPeripheralInfo(uint address)
        {
            // Check exact match first
            if (KnownMmioAddresses.TryGetValue(address, out string exactMatch))
            {
                return exactMatch;
            }
            
            // Check for base address match (within 4KB of known peripheral)
            foreach (var kvp in KnownMmioAddresses)
            {
                uint baseAddr = kvp.Key;
                if (address >= baseAddr && address < (baseAddr + 0x1000))
                {
                    uint offset = address - baseAddr;
                    return $"{kvp.Value} + 0x{offset:X3}";
                }
            }
            
            return $"Unknown MMIO region";
        }
        
        /// <summary>
        /// Get a status register value indicating the peripheral is ready/initialized.
        /// Common pattern for BCM7449 peripherals.
        /// </summary>
        protected uint GetStatusRegister(bool ready = true, bool error = false, bool busy = false)
        {
            uint status = 0;
            if (ready) status |= 0x00000001; // Ready bit
            if (error) status |= 0x00000002; // Error bit  
            if (busy)  status |= 0x00000004; // Busy bit
            
            // Add some realistic additional status bits
            status |= 0x00000100; // Version field
            status |= 0x00001000; // Feature support
            
            return status;
        }
        
        /// <summary>
        /// Get a capability register indicating supported features.
        /// </summary>
        protected uint GetCapabilityRegister(params string[] features)
        {
            uint caps = 0x12345678; // Base capability signature
            
            // Set feature bits based on supported features
            foreach (string feature in features)
            {
                switch (feature?.ToUpper())
                {
                    case "DRM": caps |= 0x00000001; break;
                    case "CRYPTO": caps |= 0x00000002; break;
                    case "HDCP": caps |= 0x00000004; break;
                    case "SECURE": caps |= 0x00000008; break;
                    case "NETWORK": caps |= 0x00000010; break;
                    case "VIDEO": caps |= 0x00000020; break;
                    case "AUDIO": caps |= 0x00000040; break;
                }
            }
            
            return caps;
        }
    }
}
