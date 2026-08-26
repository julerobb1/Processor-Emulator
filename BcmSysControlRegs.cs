using System;

namespace ProcessorEmulator.Emulation
{
    public class BcmSysControlRegs : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size { get; }
        private readonly uint[] _regs;
        private readonly CP0 _cp0;
        private bool _irqDelivered;

        public BcmSysControlRegs(CP0 cp0 = null, uint startAddress = 0x10400000, uint size = 0x10000)
        {
            _cp0 = cp0;
            StartAddress = startAddress;
            Size = size;
            _regs = new uint[size / 4];
        }

        public uint Read32(uint offset)
        {
            if (offset + 4 > Size)
                return 0;
            uint value = _regs[offset / 4];
            Console.WriteLine($"[SYSCTL] Read 0x{StartAddress + offset:X8} = 0x{value:X8}");
            return value;
        }

        public void Write32(uint offset, uint value)
        {
            if (offset + 4 > Size)
                return;
            _regs[offset / 4] = value;
            Console.WriteLine($"[SYSCTL] Write 0x{StartAddress + offset:X8} = 0x{value:X8}");
            if (offset == 0x60C8)
                UpdateArmedTimer(value);
        }

        private void UpdateArmedTimer(uint enable)
        {
            if (enable == 0)
                return;

            // Firmware arms UPG timer 0x104060C8=1 and later reads
            // 0x104060D4 bit0 / 0x1040A000 bit0 from the PIC path (IRQ 23).
            _regs[0x60D4 / 4] |= 1u;
            if (0xA000 + 4 <= Size)
                _regs[0xA000 / 4] |= 1u;

            if (_cp0 == null)
                return;

            if (!_irqDelivered)
            {
                _cp0.SetExternalIrq(true);
                _irqDelivered = true;
            }
            else
            {
                // Re-enable from the ISR is an ack, not a new arm.
                _cp0.SetExternalIrq(false);
            }
        }

        public byte Read8(uint offset)
        {
            uint aligned = offset & ~3u;
            uint word = Read32(aligned);
            return (byte)(word >> (int)((offset & 3) * 8));
        }

        public void Write8(uint offset, byte value)
        {
            uint aligned = offset & ~3u;
            uint shift = (offset & 3) * 8;
            uint word = (aligned + 4 <= Size) ? _regs[aligned / 4] : 0;
            word = (word & ~(0xFFu << (int)shift)) | ((uint)value << (int)shift);
            Write32(aligned, word);
        }
    }
}
