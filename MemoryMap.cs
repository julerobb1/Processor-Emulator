using System;
using System.Collections.Generic;

namespace ProcessorEmulator
{
    /// <summary>
    /// Memory Map implementation for realistic firmware emulation
    /// Provides RAM, ROM, and memory-mapped I/O simulation
    /// </summary>
    public class MemoryMap
    {
        public byte[] RAM { get; private set; }
        public byte[] ROM { get; private set; }
        
        // Memory regions
        public const uint RAM_BASE = 0x80000000;
        public const uint ROM_BASE = 0x00000000;
        public const uint IO_BASE = 0xF0000000;
        
        private Dictionary<uint, string> memoryRegions;
        
        public MemoryMap(byte[] rom, int ramSizeMB = 2)
        {
            ROM = rom ?? throw new ArgumentNullException(nameof(rom));
            RAM = new byte[ramSizeMB * 1024 * 1024]; // Default 2MB RAM
            
            InitializeMemoryRegions();
            Console.WriteLine($"📍 Memory map initialized:");
            Console.WriteLine($"   ROM: 0x{ROM_BASE:X8} - 0x{ROM_BASE + ROM.Length:X8} ({ROM.Length / 1024:N0}KB)");
            Console.WriteLine($"   RAM: 0x{RAM_BASE:X8} - 0x{RAM_BASE + RAM.Length:X8} ({RAM.Length / 1024:N0}KB)");
            Console.WriteLine($"   I/O: 0x{IO_BASE:X8} - 0x{IO_BASE + 0x1000000:X8} (16MB region)");
        }
        
        private void InitializeMemoryRegions()
        {
            memoryRegions = new Dictionary<uint, string>
            {
                [0x00000000] = "ROM/Flash",
                [0x80000000] = "System RAM",
                [0xF0000000] = "Memory-mapped I/O",
                [0xF0001000] = "UART Controller",
                [0xF0002000] = "Timer Controller", 
                [0xF0003000] = "Interrupt Controller",
                [0xF0004000] = "GPIO Controller",
                [0xF0005000] = "Network Controller",
                [0xF0006000] = "Video Controller"
            };
        }
        
        public byte ReadByte(uint address)
        {
            if (address >= ROM_BASE && address < ROM_BASE + ROM.Length)
            {
                return ROM[address - ROM_BASE];
            }
            else if (address >= RAM_BASE && address < RAM_BASE + RAM.Length)
            {
                return RAM[address - RAM_BASE];
            }
            else if (address >= IO_BASE)
            {
                return ReadIO(address);
            }
            else
            {
                Console.WriteLine($"⚠️ Memory read from unmapped address: 0x{address:X8}");
                return 0xFF; // Bus error simulation
            }
        }
        
        public void WriteByte(uint address, byte value)
        {
            if (address >= RAM_BASE && address < RAM_BASE + RAM.Length)
            {
                RAM[address - RAM_BASE] = value;
            }
            else if (address >= IO_BASE)
            {
                WriteIO(address, value);
            }
            else if (address >= ROM_BASE && address < ROM_BASE + ROM.Length)
            {
                Console.WriteLine($"⚠️ Attempt to write to ROM at 0x{address:X8} (value: 0x{value:X2})");
            }
            else
            {
                Console.WriteLine($"⚠️ Memory write to unmapped address: 0x{address:X8} = 0x{value:X2}");
            }
        }
        
        private byte ReadIO(uint address)
        {
            // Simulate memory-mapped I/O reads
            return address switch
            {
                0xF0001000 => 0x80, // UART status (ready)
                0xF0002000 => (byte)(DateTime.Now.Millisecond & 0xFF), // Timer low
                0xF0003000 => 0x00, // No pending interrupts
                _ => 0x00
            };
        }
        
        private void WriteIO(uint address, byte value)
        {
            // Simulate memory-mapped I/O writes
            switch (address)
            {
                case 0xF0001004: // UART data register
                    Console.WriteLine($"📡 UART output: '{(char)value}' (0x{value:X2})");
                    break;
                case 0xF0002004: // Timer control
                    Console.WriteLine($"⏰ Timer control: 0x{value:X2}");
                    break;
                case 0xF0003004: // Interrupt mask
                    Console.WriteLine($"🔔 Interrupt mask: 0x{value:X2}");
                    break;
                default:
                    Console.WriteLine($"🔧 I/O write: 0x{address:X8} = 0x{value:X2}");
                    break;
            }
        }
        
        public uint ReadWord(uint address)
        {
            return (uint)(ReadByte(address) | 
                         (ReadByte(address + 1) << 8) |
                         (ReadByte(address + 2) << 16) |
                         (ReadByte(address + 3) << 24));
        }
        
        public void WriteWord(uint address, uint value)
        {
            WriteByte(address, (byte)(value & 0xFF));
            WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
            WriteByte(address + 2, (byte)((value >> 16) & 0xFF));
            WriteByte(address + 3, (byte)((value >> 24) & 0xFF));
        }
        
        public string GetRegionName(uint address)
        {
            foreach (var region in memoryRegions)
            {
                if (address >= region.Key && address < region.Key + 0x1000)
                {
                    return region.Value;
                }
            }
            return "Unknown";
        }
        
        public void DumpMemory(uint address, int length)
        {
            Console.WriteLine($"📋 Memory dump at 0x{address:X8} ({GetRegionName(address)}):");
            
            for (int i = 0; i < length; i += 16)
            {
                var line = $"0x{address + i:X8}: ";
                var ascii = "";
                
                for (int j = 0; j < 16 && i + j < length; j++)
                {
                    byte b = ReadByte(address + (uint)(i + j));
                    line += $"{b:X2} ";
                    ascii += (b >= 32 && b <= 126) ? (char)b : '.';
                }
                
                line = line.PadRight(60) + ascii;
                Console.WriteLine(line);
            }
        }
    }
}
