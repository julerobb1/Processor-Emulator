using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // Hive Autoload child BINBlk (Profile=BINBlk, Dll=binblk.dll,
    // IClass={A4E7EDDA-E575-4252-9D6B-4195D48BB865}) is BootPhase=2,
    // so Autoload(0)/(1) skip it. The raw nk.bin B000FF bytes are the
    // BINBlk media. Advertise that existing hive name as BLOCK_DRIVER
    // and serve DISK IOCTL from the image. No SetEvent of the filesys
    // store gate, h2, or BootPhase; no invented volume name.
    public static class BinBlkMedia
    {
        public const uint WfmoJalr = 0x03E88DF8;
        public const uint WfmoRet = 0x03E88E00;
        public const uint ReadMsgJal = 0x03E88FAC;
        public const uint ReadMsgRet = 0x03E88FB4;
        public const uint CreateFile1 = 0x03E8BCF0;
        public const uint CreateFile2 = 0x03E8BD44;
        public const uint DiskIoctl = 0x03E8BAE0;
        // binfs FSD_MountDisk talks through FSDMGR_DiskIoControl
        // (IAT thunk 0x03EA4140), not the store wrapper at DiskIoctl.
        public const uint FsdmgrDiskIoctl = 0x03EA4140;
        public const uint Handle = 0xB1B10C01;
        public const uint DiskIoctlGetInfo = 1;
        public const uint DiskIoctlRead = 2;
        public const uint DiskIoctlWrite = 3;
        public const uint IoctlDiskGetName = 0x00071800;
        public const uint SectorSize = 512;
        public const string HiveName = "BINBlk";

        private static readonly uint[] BlockDriverGuid =
        {
            0xA4E7EDDA, 0x4252E575, 0x95416B9D, 0x65B88BD4
        };

        private static byte[]? _image;
        private static bool _notified;
        private static bool _detailFilled;
        private static bool _opened;
        private static bool _readServed;

        public static bool IsPresent => _image != null && _image.Length > 0;
        public static bool IsOpen => _opened;
        public static bool HasServedRead => _readServed;

        public static void Attach(byte[] image)
        {
            _image = image != null && image.Length > 0 ? image : null;
            _notified = false;
            _detailFilled = false;
            _opened = false;
            _readServed = false;
            if (IsPresent)
                System.Console.WriteLine($"[BINBlk] media {_image.Length} bytes name={HiveName}");
        }

        public static bool TryStep(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!IsPresent || registers == null || bus == null)
                return false;

            uint pc = programCounter;
            if (pc == WfmoJalr)
                return TrySatisfyWfmo(registers, ref programCounter);
            if (pc == ReadMsgJal)
                return TryFillDevDetail(registers, bus, ref programCounter);
            if (pc == CreateFile1 || pc == CreateFile2)
                return TryCreateFile(registers, bus, pc, ref programCounter);
            if (pc == DiskIoctl)
                return TryIoctl(registers, bus, ref programCounter);
            if (pc == FsdmgrDiskIoctl)
                return TryFsdmgrDiskIoctl(registers, bus, ref programCounter);
            return false;
        }

        private static bool TrySatisfyWfmo(uint[] registers, ref uint programCounter)
        {
            if (_notified)
                return false;
            _notified = true;
            registers[2] = 0;
            registers[4] = 3;
            programCounter = WfmoRet;
            System.Console.WriteLine("[BINBlk] notify BLOCK_DRIVER");
            return true;
        }

        private static bool TryFillDevDetail(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (_detailFilled || !_notified)
                return false;
            uint buf = registers[18];
            if (buf == 0)
                buf = registers[5];
            if (buf == 0)
                return false;
            try
            {
                bus.Write32(registers[29] + 16, registers[2]);
                for (uint i = 0; i < 232; i += 4)
                    bus.Write32(buf + i, 0);
                bus.Write32(buf + 0, BlockDriverGuid[0]);
                bus.Write32(buf + 4, BlockDriverGuid[1]);
                bus.Write32(buf + 8, BlockDriverGuid[2]);
                bus.Write32(buf + 12, BlockDriverGuid[3]);
                bus.Write32(buf + 16, 0);
                bus.Write32(buf + 20, 1);
                bus.Write32(buf + 24, (uint)((HiveName.Length + 1) * 2));
                WriteUtf16(bus, buf + 28, HiveName);
            }
            catch
            {
                return false;
            }
            registers[2] = 1;
            programCounter = ReadMsgRet;
            _detailFilled = true;
            System.Console.WriteLine("[BINBlk] DEVDETAIL");
            return true;
        }

        private static bool TryCreateFile(uint[] registers, MipsBus bus, uint pc, ref uint programCounter)
        {
            if (!NameIsHive(bus, registers[4]))
                return false;
            try
            {
                bus.Write32(registers[29] + 16, registers[20]);
            }
            catch
            {
            }
            registers[2] = Handle;
            programCounter = pc + 8;
            _opened = true;
            System.Console.WriteLine("[BINBlk] CreateFile");
            return true;
        }

        private static bool TryIoctl(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            uint store = registers[4];
            if (!_opened || store == 0 || !NameIsHive(bus, store + 16))
                return false;

            uint code = registers[6];
            uint buf = registers[7];
            uint size = 0;
            try { size = bus.Read32(registers[29] + 16); }
            catch { }

            uint err = ServeIoctl(bus, code, buf, size);
            registers[2] = err;
            programCounter = registers[31];
            System.Console.WriteLine($"[BINBlk] IOCTL 0x{code:X} err={err}");
            return true;
        }

        // FSDMGR_DiskIoControl(HDSK, code, buf, inlen, ...): BOOL, not Win32 err.
        // MountDisk (0x03EA1E50 / 0x03EA291C / 0x03EA2C54) uses this thunk.
        private static bool TryFsdmgrDiskIoctl(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened)
                return false;
            uint hdsk = registers[4];
            if (HostHardDisk.OwnsHdsk(bus, hdsk))
                return false;

            uint code = registers[5];
            uint buf = registers[6];
            uint size = registers[7];
            if (size == 0)
            {
                try { size = bus.Read32(registers[29] + 20); }
                catch { }
            }

            uint err = ServeIoctl(bus, code, buf, size);
            registers[2] = err == 0 ? 1u : 0u;
            programCounter = registers[31];
            System.Console.WriteLine($"[BINBlk] FSDIOCTL 0x{code:X} err={err} v0={registers[2]}");
            return true;
        }

        private static uint ServeIoctl(MipsBus bus, uint code, uint buf, uint size)
        {
            try
            {
                if (code == DiskIoctlGetInfo && buf != 0 && size >= 24)
                {
                    uint sectors = SectorCount();
                    bus.Write32(buf + 0, sectors);
                    bus.Write32(buf + 4, SectorSize);
                    bus.Write32(buf + 8, 0);
                    bus.Write32(buf + 12, 0);
                    bus.Write32(buf + 16, 0);
                    bus.Write32(buf + 20, 0);
                    return 0;
                }
                if (code == IoctlDiskGetName && buf != 0 && size >= 20)
                {
                    // FSDMGR keeps a DWORD at store+2460 and the
                    // profile tail at +2464 (ROEX showed Profiles\NBlk
                    // when the name started at offset 0).
                    bus.Write32(buf, 0);
                    WriteUtf16(bus, buf + 4, HiveName);
                    return 0;
                }
                if (code == DiskIoctlRead && buf != 0)
                {
                    uint err = TryReadSg(bus, buf);
                    if (err == 0)
                        _readServed = true;
                    return err;
                }
                if (code == DiskIoctlWrite)
                    return 19;
            }
            catch
            {
                return 87;
            }
            return 50;
        }

        private static uint TryReadSg(MipsBus bus, uint sg)
        {
            uint start = bus.Read32(sg + 0);
            uint num = bus.Read32(sg + 4);
            uint nsg = bus.Read32(sg + 8);
            if (nsg == 0 || num == 0)
                return 87;
            uint dest = bus.Read32(sg + 20);
            uint len = bus.Read32(sg + 24);
            uint want = num * SectorSize;
            if (dest == 0 || len < want)
                return 122;
            ulong off = (ulong)start * SectorSize;
            if (off >= (ulong)_image.Length)
                return 87;
            for (uint i = 0; i < want; i++)
            {
                byte b = 0;
                ulong src = off + i;
                if (src < (ulong)_image.Length)
                    b = _image[(int)src];
                Write8(bus, dest + i, b);
            }
            bus.Write32(sg + 12, 0);
            return 0;
        }

        private static uint SectorCount()
        {
            return (uint)((_image.Length + (int)SectorSize - 1) / (int)SectorSize);
        }

        private static bool NameIsHive(MipsBus bus, uint addr)
        {
            if (addr == 0)
                return false;
            try
            {
                return NamesEqual(ReadUtf16(bus, addr), HiveName);
            }
            catch
            {
                return false;
            }
        }

        private static string ReadUtf16(MipsBus bus, uint addr)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                uint p = addr + (uint)(i * 2);
                uint word = bus.Read32(p & ~3u);
                uint ch = ((p & 2) == 0) ? (word & 0xFFFF) : (word >> 16);
                if (ch == 0)
                    break;
                if (ch < 0x20 || ch > 0x7E)
                    return "";
                sb.Append((char)ch);
            }
            return sb.ToString();
        }

        private static void WriteUtf16(MipsBus bus, uint addr, string text)
        {
            for (int i = 0; i < text.Length; i++)
                Write16(bus, addr + (uint)(i * 2), text[i]);
            Write16(bus, addr + (uint)(text.Length * 2), 0);
        }

        private static void Write16(MipsBus bus, uint addr, int val)
        {
            uint aligned = addr & ~3u;
            uint word = bus.Read32(aligned);
            if ((addr & 2) == 0)
                word = (word & 0xFFFF0000u) | (ushort)val;
            else
                word = (word & 0x0000FFFFu) | ((uint)(ushort)val << 16);
            bus.Write32(aligned, word);
        }

        private static void Write8(MipsBus bus, uint addr, byte val)
        {
            uint aligned = addr & ~3u;
            int shift = 8 * (int)(addr & 3);
            uint word = bus.Read32(aligned);
            word = (word & ~(0xFFu << shift)) | ((uint)val << shift);
            bus.Write32(aligned, word);
        }

        private static bool NamesEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                char ca = a[i];
                char cb = b[i];
                if (ca >= 'A' && ca <= 'Z') ca = (char)(ca + 32);
                if (cb >= 'A' && cb <= 'Z') cb = (char)(cb + 32);
                if (ca != cb)
                    return false;
            }
            return true;
        }
    }
}
