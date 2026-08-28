using System;

namespace ProcessorEmulator.Emulation
{
    public class BcmSysControlRegs : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size { get; }
        private readonly uint[] _regs;
        private readonly CP0 _cp0;
        private readonly BcmStickyMmio _pic;
        private uint _remaining;
        private bool _running;
        private bool _calibRunning;
        private uint _calibAnchor;

        private const uint OffPeriod = 0x600C;
        private const uint OffEnable = 0x60C8;
        private const uint OffStatus = 0x60D4;
        private const uint OffPicPend = 0xA000;
        // OEMInit calib at 0x80057054: sw 1 -> 0xB04007C0, sw 0xC0337F98 ->
        // 0xB04007C8, mtc0 Count=0, spin on bit0, return Count<<4. That 07C8
        // word is not a Count-scale compare (3.2e9 ticks). Honor 07C8 only
        // when it fits the cap; otherwise wait this many Count ticks.
        private const uint OffCalibCtrl = 0x07C0;
        private const uint OffCalibData = 0x07C8;
        private const uint CalibWaitCap = 0x10000;
        // Walker at 0x80056500: (~*0x1000140C) & *0x10001400. IRQ 23 = bit 23.
        private const uint PicStatusOff = 0x400;
        private const uint Irq23Bit = 1u << 23;

        // 64KB matches the bus slot already claimed for 0x1040xxxx.
        public BcmSysControlRegs(CP0 cp0 = null, BcmStickyMmio pic = null, uint startAddress = 0x10400000, uint size = 0x10000)
        {
            _cp0 = cp0;
            _pic = pic;
            StartAddress = startAddress;
            Size = size;
            _regs = new uint[size / 4];
        }

        public uint Read32(uint offset)
        {
            if (offset + 4 > Size)
                return 0;
            if (offset == OffCalibCtrl)
            {
                CompleteCalibIfReady();
                return _regs[offset / 4];
            }
            uint value = _regs[offset / 4];
            Console.WriteLine($"[SYSCTL] Read 0x{StartAddress + offset:X8} = 0x{value:X8}");
            return value;
        }

        public void Write32(uint offset, uint value)
        {
            if (offset + 4 > Size)
                return;
            if (offset == OffCalibCtrl)
            {
                _regs[offset / 4] = value & ~1u;
                _calibRunning = true;
                _calibAnchor = _cp0 != null ? _cp0.Count : 0;
                Console.WriteLine($"[SYSCTL] Write 0x{StartAddress + offset:X8} = 0x{value:X8} (bit0 cleared)");
                return;
            }
            bool hadPending = (_regs[OffStatus / 4] & 1u) != 0;
            _regs[offset / 4] = value;
            Console.WriteLine($"[SYSCTL] Write 0x{StartAddress + offset:X8} = 0x{value:X8}");

            if (offset == OffPeriod && value != 0 && (_regs[OffEnable / 4] != 0))
            {
                if (_remaining == 0)
                    _remaining = value;
                _running = true;
            }

            if (offset == OffEnable)
            {
                if (value == 0)
                {
                    _running = false;
                    return;
                }

                // ISR tail at 0x800574A8 writes 1 here after handling. That re-arm
                // is the only evidenced pending clear (no store to 0x60D4 / 0xA000).
                if (hadPending)
                    ClearPending();

                uint period = _regs[OffPeriod / 4];
                if (period != 0)
                    _remaining = period;
                _running = period != 0;
            }
        }

        public void Tick(int cycles)
        {
            CompleteCalibIfReady();
            if (!_running || _remaining == 0 || cycles <= 0)
                return;
            if (_remaining > (uint)cycles)
            {
                _remaining -= (uint)cycles;
                return;
            }
            _remaining = 0;
            _running = false;
            _regs[OffStatus / 4] |= 1u;
            if (OffPicPend + 4 <= Size)
                _regs[OffPicPend / 4] |= 1u;
            // 0x140C stays 0 (mask/NOR side). Setting it would hide IRQ 23.
            _pic?.Or32(PicStatusOff, Irq23Bit);
            _cp0?.SetExternalIrq(true);
            Console.WriteLine($"[SYSCTL] UPG timer expired pending=0x{_regs[OffStatus / 4]:X8} pic23=0x{Irq23Bit:X8}");
        }

        private void CompleteCalibIfReady()
        {
            if (!_calibRunning || _cp0 == null)
                return;

            uint count = _cp0.Count;
            // 0x80057054 zeros Count after the start write. Track the low
            // water so elapsed is measured from that reset, not the pre-mtc0
            // snapshot (which would wrap and complete immediately).
            if (count < _calibAnchor)
                _calibAnchor = count;

            uint elapsed = count - _calibAnchor;
            uint programmed = _regs[OffCalibData / 4];
            uint wait = (programmed > 0 && programmed <= CalibWaitCap)
                ? programmed
                : CalibWaitCap;
            if (elapsed < wait)
                return;

            _regs[OffCalibCtrl / 4] |= 1u;
            _calibRunning = false;
            Console.WriteLine($"[SYSCTL] Calib 0x{StartAddress + OffCalibCtrl:X8} bit0 after {elapsed} Count ticks (07C8=0x{programmed:X8})");
        }

        private void ClearPending()
        {
            _regs[OffStatus / 4] &= ~1u;
            if (OffPicPend + 4 <= Size)
                _regs[OffPicPend / 4] &= ~1u;
            _cp0?.SetExternalIrq(false);
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
