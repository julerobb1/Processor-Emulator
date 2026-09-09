using System;
using System.Diagnostics;
using ProcessorEmulator.Core;

namespace ProcessorEmulator.Emulation
{
    /// <summary>
    /// Simulates a basic MIPS UART (Universal Asynchronous Receiver/Transmitter)
    /// for console output and basic input.
    /// Implements IBusDevice to be memory-mapped on the MIPS bus.
    /// </summary>
    public class MipsUart : IBusDevice
    {
        public uint StartAddress { get; private set; }
        public uint Size { get; private set; }

        // Simplified UART registers
        private const uint UART_DR = 0x0; // Data Register (Read/Write)
        private const uint UART_SR = 0x4; // Status Register (Read-only for now)

        private const uint STATUS_RX_READY = 0x1;
        private const uint STATUS_TX_EMPTY = 0x2;

        public MipsUart(uint startAddress, uint size)
        {
            StartAddress = startAddress;
            Size = size;
            Debug.WriteLine($"[MipsUart] Initialized at 0x{StartAddress:X8} with size 0x{Size:X8}");
        }

        public uint Read32(uint offset)
        {
            switch (offset)
            {
                case UART_SR:
                    // Always return TX_EMPTY and RX_READY for simplicity for now
                    return STATUS_RX_READY | STATUS_TX_EMPTY;
                case UART_DR:
                    // Simulate receiving a character
                    // For now, no actual input. Returns 0.
                    return 0;
                default:
                    Debug.WriteLine($"[MipsUart] Read from unknown register offset 0x{offset:X} at 0x{StartAddress + offset:X8}");
                    return 0;
            }
        }

        public void Write32(uint offset, uint value)
        {
            switch (offset)
            {
                case UART_DR:
                    // WinExe hides Console.Write. Same boot.log as
                    // ExtraROM OpenFile. Do not invent a second UART.
                    BootLog.UartTx((byte)value);
                    break;
                default:
                    Debug.WriteLine($"[MipsUart] Write to unknown register offset 0x{offset:X} at 0x{StartAddress + offset:X8} with value 0x{value:X}");
                    break;
            }
        }

        public byte Read8(uint offset)
        {
            return (byte)(Read32(offset) & 0xFF);
        }

        public void Write8(uint offset, byte value)
        {
            Write32(offset, value);
        }

        public ushort Read16(uint offset)
        {
            return (ushort)(Read32(offset) & 0xFFFF);
        }

        public void Write16(uint offset, ushort value)
        {
            Write32(offset, value);
        }
    }
}
