using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Emulation
{
    public class UniversalUart : IBusDevice
    {
        public uint StartAddress { get; set; }
        public uint Size => 0x1000;

        private Queue<byte> _inputBuffer = new Queue<byte>();
        public event Action<char> OnCharReceived;

        // Call this from your WinForms KeyDown handler
        public void SendKey(char c)
        {
            _inputBuffer.Enqueue((byte)c);
        }

        public uint Read32(uint offset)
        {
            // Offset 0x00: Receiver Buffer Register (RBR)
            if (offset == 0x00)
            {
                return _inputBuffer.Count > 0 ? _inputBuffer.Dequeue() : 0u;
            }

            // Offset 0x14 (5*4): Line Status Register (LSR)
            if (offset == 0x14)
            {
                uint status = 0x20; // Bit 5: Transmitter is always empty/ready
                if (_inputBuffer.Count > 0) status |= 0x01; // Bit 0: Data Ready
                return status;
            }
            return 0;
        }

        public void Write32(uint offset, uint value)
        {
            if (offset == 0x00) // Transmit
            {
                OnCharReceived?.Invoke((char)(value & 0xFF));
            }
        }
        
        public byte Read8(uint offset)
        {
            // Offset 0x00: Receiver Buffer Register (RBR)
            if (offset == 0x00)
            {
                return _inputBuffer.Count > 0 ? _inputBuffer.Dequeue() : (byte)0;
            }

            // Offset 0x05: Line Status Register (LSR)
            if (offset == 0x05)
            {
                byte status = 0x20; // Bit 5: Transmitter is always empty/ready
                if (_inputBuffer.Count > 0) status |= 0x01; // Bit 0: Data Ready
                return status;
            }
            return 0;
        }

        public void Write8(uint offset, byte value)
        {
            if (offset == 0x00) // Transmit
            {
                OnCharReceived?.Invoke((char)value);
            }
        }
    }
}