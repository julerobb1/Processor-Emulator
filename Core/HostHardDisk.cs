using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // Existing Uverse Drive E tree as the Hard Disk FAT volume.
    // Read-only host directory. Never writes, deletes, or renames
    // dump files. Not a BINBlk/BINFS/ExtraROM object.
    //
    // FSDMGR WFMO #2 (after BINBlk) is already waiting on the
    // BLOCK_DRIVER queue. Deliver HDProf there (7-char CE name).
    // GETNAME is HDProfile so Profiles\HDProfile / Folder Hard Disk
    // apply. mspart calls FSDMGR at FsdmgrIoImpl, not the binfs IAT.
    // No SetEvent of store/BootPhase/Autoload/pump.
    public static class HostHardDisk
    {
        public const string EnvName = "UVERSE_HARD_DISK";
        public const string EnvNameAlt = "PROCESSOR_EMULATOR_HARD_DISK";
        // CE device names are 7 chars max; HDProfile does not fit
        // DEVDETAIL and becomes "HDProfi". Advertise HDProf, then
        // GETNAME HDProfile so Profiles\HDProfile / Folder Hard Disk.
        public const string DeviceName = "HDProf";
        public const string ProfileName = "HDProfile";
        public const string FolderName = "Hard Disk";
        public const uint Handle = 0xA15C0D15;
        public const uint KernelCreateFile = 0x8001D3A0;
        // mspart PD_OpenStore calls this FSDMGR export, not binfs IAT 0x03EA4140.
        public const uint FsdmgrIoImpl = 0x03E83C08;
        // mspart GetDiskInfo / OpenStore uses these FSDMGR
        // internals, not DiskIoctl 0x03E8BAE0.
        public const uint FsdmgrGetDiskInfo = 0x03E8332C;
        public const uint FsdmgrStoreIoctl2 = 0x03E8B618;
        public const uint IoctlDiskGetInfo = 0x00071C00;
        public const uint IoctlDiskReadEx = 0x00075C08;
        public const uint IoctlDiskGetStorageId = 0x00071C24;
        public const uint SectorSize = 512;

        private static readonly uint[] BlockDriverGuid =
        {
            0xA4E7EDDA, 0x4252E575, 0x95416B9D, 0x65B88BD4
        };

        private static string _root = "";
        private static byte[] _image = Array.Empty<byte>();
        private static bool _notified;
        private static bool _detailFilled;
        private static bool _opened;
        private static readonly HashSet<string> _logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static bool IsPresent => _image != null && _image.Length > 0;
        public static bool IsOpen => _opened;
        public static bool DetailFilled => _detailFilled;
        public static string Root => _root;

        public static void Attach()
        {
            _root = "";
            _image = Array.Empty<byte>();
            _notified = false;
            _detailFilled = false;
            _opened = false;
            _logged.Clear();
            string dir = ResolveRoot();
            if (string.IsNullOrEmpty(dir))
            {
                System.Console.WriteLine("[HardDisk] no host dir (set " + EnvName + " to Uverse Drive E)");
                return;
            }
            try
            {
                _image = Fat16.Build(dir);
                _root = dir;
                System.Console.WriteLine($"[HardDisk] FAT {_image.Length} bytes root={dir} name={FolderName}");
            }
            catch (Exception ex)
            {
                _image = Array.Empty<byte>();
                System.Console.WriteLine("[HardDisk] FAT build failed: " + ex.Message);
            }
        }

        public static bool TryStep(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (registers == null || bus == null)
                return false;

            uint pc = programCounter;
            if (pc == KernelCreateFile)
            {
                string kn = ReadUtf16(bus, registers[4]);
                if ((_notified || IsHardDiskPath(kn)) && _logged.Add("k:" + kn))
                    System.Console.WriteLine($"[HardDisk] kCreateFile \"{kn}\"");
                LogKernelCreateFile(bus, registers[4]);
                return false;
            }

            if (!IsPresent)
                return false;

            if (pc == BinBlkMedia.WfmoJalr)
                return TrySatisfyWfmo(registers, ref programCounter);
            if (pc == BinBlkMedia.ReadMsgJal)
                return TryFillDevDetail(registers, bus, ref programCounter);
            if (pc == BinBlkMedia.CreateFile1 || pc == BinBlkMedia.CreateFile2)
            {
                string fn = ReadUtf16(bus, registers[4]);
                if (_notified && _logged.Add("f:" + fn))
                    System.Console.WriteLine($"[HardDisk] fsdCreateFile \"{fn}\"");
                return TryCreateFile(registers, bus, pc, ref programCounter);
            }
            if (pc == BinBlkMedia.DiskIoctl)
                return TryIoctl(registers, bus, ref programCounter);
            if (pc == BinBlkMedia.FsdmgrDiskIoctl)
                return TryFsdmgrDiskIoctl(registers, bus, ref programCounter);
            if (pc == FsdmgrIoImpl)
                return TryFsdmgrIoImpl(registers, bus, ref programCounter);
            if (pc == FsdmgrStoreIoctl2)
                return TryStoreIoctl2(registers, bus, ref programCounter);
            if (pc == FsdmgrGetDiskInfo)
                return TryGetDiskInfo(registers, bus, ref programCounter);
            return false;
        }

        public static bool OwnsHdsk(MipsBus bus, uint hdsk)
        {
            if (!_opened || hdsk == 0 || bus == null)
                return false;
            if (hdsk == Handle)
                return true;
            try
            {
                return NameIsOurs(bus, hdsk) || NameIsOurs(bus, hdsk + 16);
            }
            catch
            {
                return false;
            }
        }

        private static void LogKernelCreateFile(MipsBus bus, uint path)
        {
            string name = ReadUtf16(bus, path);
            if (string.IsNullOrEmpty(name) || !IsHardDiskPath(name))
                return;
            if (!_logged.Add(name))
                return;
            string host = MapHost(_root, name);
            bool hit = !string.IsNullOrEmpty(host) && File.Exists(host);
            System.Console.WriteLine($"[HardDisk] CreateFile \"{name}\" host={(hit ? host : "miss")} fat={(IsPresent ? "yes" : "no")}");
        }

        private static bool TrySatisfyWfmo(uint[] registers, ref uint programCounter)
        {
            if (_notified)
                return false;
            // WFMO #2 is the HD slot. Do not fire before BINBlk
            // CreateFile; the ioctl burst that follows is still
            // BINBlk. The next WaitForMultipleObjects at this
            // jalr is the second BLOCK_DRIVER wait. No READ
            // requirement: BINFS MountDisk may never DISK_READ.
            if (BinBlkMedia.IsPresent && !BinBlkMedia.IsOpen)
                return false;
            _notified = true;
            registers[2] = 0;
            registers[4] = 3;
            programCounter = BinBlkMedia.WfmoRet;
            System.Console.WriteLine("[HardDisk] notify BLOCK_DRIVER HDProfile");
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
                bus.Write32(buf + 24, (uint)((DeviceName.Length + 1) * 2));
                WriteUtf16(bus, buf + 28, DeviceName);
            }
            catch
            {
                return false;
            }
            registers[2] = 1;
            programCounter = BinBlkMedia.ReadMsgRet;
            _detailFilled = true;
            System.Console.WriteLine("[HardDisk] DEVDETAIL " + DeviceName);
            return true;
        }

        private static bool TryCreateFile(uint[] registers, MipsBus bus, uint pc, ref uint programCounter)
        {
            if (!NameIsOurs(ReadUtf16(bus, registers[4])))
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
            System.Console.WriteLine("[HardDisk] CreateFile " + DeviceName);
            return true;
        }

        private static bool TryIoctl(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            uint store = registers[4];
            if (!_opened || store == 0 || !NameIsOurs(bus, store + 16))
                return false;
            uint code = registers[6];
            uint buf = registers[7];
            uint size = 0;
            try { size = bus.Read32(registers[29] + 16); }
            catch { }
            uint err = ServeIoctl(bus, code, buf, size, registers[29]);
            registers[2] = err;
            programCounter = registers[31];
            System.Console.WriteLine($"[HardDisk] IOCTL 0x{code:X} err={err}");
            return true;
        }

        private static bool TryStoreIoctl2(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            uint store = registers[4];
            if (!_opened || store == 0)
                return false;
            bool ours = store == Handle || NameIsOurs(bus, store) || NameIsOurs(bus, store + 16);
            uint a1 = registers[5];
            uint a2 = registers[6];
            uint a3 = registers[7];
            if (!ours)
            {
                if (_logged.Add("i2:" + store.ToString("X")))
                    System.Console.WriteLine($"[HardDisk] StoreIoctl2 miss a0=0x{store:X8} a1=0x{a1:X} a2=0x{a2:X} a3=0x{a3:X}");
                return false;
            }
            uint code = a2;
            uint buf = a3;
            if (!IsIoctlCode(code) && IsIoctlCode(a1))
            {
                code = a1;
                buf = a2;
            }
            uint size = 0;
            try { size = bus.Read32(registers[29] + 16); }
            catch { }
            if (size == 0)
            {
                try { size = bus.Read32(registers[29] + 20); }
                catch { }
            }
            uint err = ServeIoctl(bus, code, buf, size, registers[29]);
            registers[2] = err;
            programCounter = registers[31];
            System.Console.WriteLine($"[HardDisk] IOCTL2 0x{code:X} err={err} buf=0x{buf:X8}");
            return true;
        }

        private static bool TryGetDiskInfo(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened)
                return false;
            uint a0 = registers[4];
            uint a1 = registers[5];
            uint info = 0;
            if (OwnsHdsk(bus, a0) && LooksLikePtr(a1))
                info = a1;
            else if (OwnsHdsk(bus, a1) && LooksLikePtr(a0))
                info = a0;
            else if ((NameIsOurs(bus, a0) || NameIsOurs(bus, a0 + 16)) && LooksLikePtr(a1))
                info = a1;
            if (info == 0)
            {
                if (_logged.Add("gdi"))
                    System.Console.WriteLine($"[HardDisk] GetDiskInfo miss a0=0x{a0:X8} a1=0x{a1:X8}");
                return false;
            }
            uint err = ServeIoctl(bus, BinBlkMedia.DiskIoctlGetInfo, info, 24, registers[29]);
            registers[2] = err == 0 ? 1u : 0u;
            programCounter = registers[31];
            System.Console.WriteLine($"[HardDisk] GetDiskInfo err={err} info=0x{info:X8}");
            return true;
        }

        private static bool IsIoctlCode(uint c)
        {
            return c == 1 || c == 2 || c == 3
                || c == BinBlkMedia.IoctlDiskGetName
                || c == IoctlDiskGetInfo
                || c == IoctlDiskGetStorageId
                || c == IoctlDiskReadEx
                || (c >= 0x71000 && c < 0x80000);
        }

        private static bool LooksLikePtr(uint p)
        {
            return p > 0x1000 && p < 0x80000000 && (p < 0x71000 || p >= 0x80000);
        }

        private static bool TryFsdmgrIoImpl(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened)
                return false;
            uint hdsk = registers[4];
            if (!OwnsHdsk(bus, hdsk))
                return false;
            uint a1 = registers[5];
            // mspart hits this with a1=0 (not an ioctl). Do not steal it.
            if (a1 == 0)
                return false;
            return TryFsdmgrDiskIoctl(registers, bus, ref programCounter);
        }

        private static bool TryFsdmgrDiskIoctl(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened)
                return false;
            uint hdsk = registers[4];
            if (!OwnsHdsk(bus, hdsk))
                return false;
            uint a1 = registers[5];
            if (a1 == 0)
                return false;
            // FSDMGR_GetDiskInfo(hDsk, pInfo): a1 is a user pointer.
            if (a1 > 0x10000 && (a1 < 0x71000 || a1 >= 0x80000))
            {
                uint errInfo = ServeIoctl(bus, BinBlkMedia.DiskIoctlGetInfo, a1, 24, registers[29]);
                registers[2] = errInfo == 0 ? 1u : 0u;
                programCounter = registers[31];
                System.Console.WriteLine($"[HardDisk] GetDiskInfo err={errInfo} v0={registers[2]}");
                return true;
            }
            uint code = a1;
            uint buf = registers[6];
            uint size = registers[7];
            if (size == 0)
            {
                try { size = bus.Read32(registers[29] + 20); }
                catch { }
            }
            uint err = ServeIoctl(bus, code, buf, size, registers[29]);
            registers[2] = err == 0 ? 1u : 0u;
            programCounter = registers[31];
            System.Console.WriteLine($"[HardDisk] FSDIOCTL 0x{code:X} err={err} v0={registers[2]}");
            return true;
        }

        private static uint ServeIoctl(MipsBus bus, uint code, uint buf, uint size, uint sp)
        {
            try
            {
                if ((code == BinBlkMedia.DiskIoctlGetInfo || code == IoctlDiskGetInfo)
                    && buf != 0)
                {
                    uint sectors = (uint)((_image.Length + (int)SectorSize - 1) / (int)SectorSize);
                    bus.Write32(buf + 0, sectors);
                    bus.Write32(buf + 4, SectorSize);
                    bus.Write32(buf + 8, 0);
                    bus.Write32(buf + 12, 0);
                    bus.Write32(buf + 16, 0);
                    bus.Write32(buf + 20, 0);
                    return 0;
                }
                if (code == BinBlkMedia.IoctlDiskGetName && buf != 0 && size >= 20)
                {
                    bus.Write32(buf, 0);
                    WriteUtf16(bus, buf + 4, ProfileName);
                    return 0;
                }
                if (code == IoctlDiskGetStorageId)
                {
                    uint outBuf = buf;
                    uint outLen = size;
                    if (outBuf == 0 || outLen < 16)
                    {
                        uint s16 = 0, s20 = 0, s24 = 0;
                        try { s16 = bus.Read32(sp + 16); } catch { }
                        try { s20 = bus.Read32(sp + 20); } catch { }
                        try { s24 = bus.Read32(sp + 24); } catch { }
                        if (s16 > 0x1000 && s16 < 0x80000000 && outBuf == 0)
                        {
                            outBuf = s16;
                            outLen = s20;
                        }
                        else if (s20 > 0x1000 && s20 < 0x80000000)
                        {
                            outBuf = s20;
                            outLen = s24;
                        }
                    }
                    if (outBuf == 0)
                        return 87;
                    if (outLen < 16)
                        return 122;
                    bus.Write32(outBuf + 0, 16);
                    bus.Write32(outBuf + 4, 3);
                    bus.Write32(outBuf + 8, 0);
                    bus.Write32(outBuf + 12, 0);
                    return 0;
                }
                if ((code == BinBlkMedia.DiskIoctlRead || code == IoctlDiskReadEx) && buf != 0)
                    return TryReadSg(bus, buf);
                if (code == BinBlkMedia.DiskIoctlWrite)
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
            if (_logged.Add("read:" + start))
                System.Console.WriteLine($"[HardDisk] DISK_READ lba={start} n={num}");
            return 0;
        }

        internal static string ResolveRoot()
        {
            foreach (string raw in CandidateRoots())
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                string dir = raw.Trim();
                try
                {
                    if (Directory.Exists(dir) && LooksLikeVolume(dir))
                        return Path.GetFullPath(dir);
                }
                catch
                {
                }
            }
            return "";
        }

        private static IEnumerable<string> CandidateRoots()
        {
            yield return Environment.GetEnvironmentVariable(EnvName);
            yield return Environment.GetEnvironmentVariable(EnvNameAlt);
            string cwd = Environment.CurrentDirectory;
            string bas = AppDomain.CurrentDomain.BaseDirectory;
            yield return Path.Combine(cwd, "UverseDriveE");
            yield return Path.Combine(bas ?? "", "UverseDriveE");
            yield return "/workspace/UverseDriveE";
            yield return Path.Combine(cwd, "Uverse Drive E");
            yield return Path.Combine(bas ?? "", "Uverse Drive E");
            yield return @"E:\EVO backup 2026 august 26\DVR Stuff\UVERSE STUFF\Uverse Drive E";
        }

        private static bool LooksLikeVolume(string dir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    string n = Path.GetFileName(f);
                    if (n.Equals("nk.bin", StringComparison.OrdinalIgnoreCase)
                        || n.Equals("etc.bin", StringComparison.OrdinalIgnoreCase)
                        || n.Equals("BOOT.PRF", StringComparison.OrdinalIgnoreCase)
                        || n.Equals("sec.bin", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        internal static bool IsHardDiskPath(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            string n = name.Replace('/', '\\');
            if (n.Length >= 1 && n[0] == '\\')
                n = n.TrimStart('\\');
            if (StartsWithIgnore(n, "Hard Disk\\") || EqualsIgnore(n, "Hard Disk"))
                return true;
            if (EqualsIgnore(n, "ETC.bin") || StartsWithIgnore(n, "ETC.bin"))
                return true;
            if (EqualsIgnore(n, "SEC.bin") || StartsWithIgnore(n, "Application\\")
                || StartsWithIgnore(n, "TV2ClientCE\\")
                || EqualsIgnore(n, "BOOT.PRF") || EqualsIgnore(n, "BOOTPRF.BAK"))
                return true;
            return false;
        }

        internal static string MapHost(string root, string cePath)
        {
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(cePath))
                return "";
            string n = cePath.Replace('/', '\\').TrimStart('\\');
            if (StartsWithIgnore(n, "Hard Disk\\"))
                n = n.Substring("Hard Disk\\".Length);
            if (n.IndexOf("..", StringComparison.Ordinal) >= 0)
                return "";
            string host = root;
            foreach (string part in n.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string next = FindChild(host, part);
                if (string.IsNullOrEmpty(next))
                    return "";
                host = next;
            }
            return host;
        }

        private static string FindChild(string dir, string name)
        {
            try
            {
                foreach (string p in Directory.GetFileSystemEntries(dir))
                {
                    if (Path.GetFileName(p).Equals(name, StringComparison.OrdinalIgnoreCase))
                        return p;
                }
            }
            catch
            {
            }
            return "";
        }

        private static bool NameIsOurs(MipsBus bus, uint addr)
        {
            return NameIsOurs(ReadUtf16(bus, addr));
        }

        private static bool NameIsOurs(string n)
        {
            if (string.IsNullOrEmpty(n))
                return false;
            if (n.Length >= 1 && (n[0] == '\\' || n[0] == '/'))
                n = n.TrimStart('\\', '/');
            if (StartsWithIgnore(n, "Profiles\\"))
                n = n.Substring("Profiles\\".Length);
            int colon = n.IndexOf(':');
            if (colon > 0)
                n = n.Substring(0, colon);
            return EqualsIgnore(n, DeviceName) || EqualsIgnore(n, ProfileName)
                || EqualsIgnore(n, FolderName) || EqualsIgnore(n, "HDProfi")
                || EqualsIgnore(n, DeviceName + "1") || EqualsIgnore(n, DeviceName + "2");
        }

        private static bool StartsWithIgnore(string a, string b)
        {
            return a != null && b != null && a.StartsWith(b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EqualsIgnore(string a, string b)
        {
            return a != null && b != null && a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadUtf16(MipsBus bus, uint addr)
        {
            if (addr == 0)
                return "";
            var sb = new StringBuilder();
            try
            {
                for (int i = 0; i < 260; i++)
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
            }
            catch
            {
                return "";
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

        private static class Fat16
        {
            private const int Bps = 512;
            private const int Spc = 8;
            private const int Reserved = 1;
            private const int Fats = 2;
            private const int ClusterSize = Bps * Spc;

            public static byte[] Build(string root)
            {
                byte[] fat = BuildVolume(root);
                const int pre = 63;
                var img = new byte[pre * Bps + fat.Length];
                Buffer.BlockCopy(fat, 0, img, pre * Bps, fat.Length);
                img[0x1BE] = 0x80;
                img[0x1C2] = 0x0E;
                Write32(img, 0x1C6, (uint)pre);
                Write32(img, 0x1CA, (uint)(fat.Length / Bps));
                img[510] = 0x55;
                img[511] = 0xAA;
                return img;
            }

            private static byte[] BuildVolume(string root)
            {
                Node tree = LoadTree(root);
                int rootSlots = DirSlots(tree, true);
                int rootEnt = Math.Max(512, Align(rootSlots, 16));
                int need = CountClusters(tree, true);
                int clusters = Math.Max(4085, need + 8);
                int fatBytes = Align((clusters + 2) * 2, Bps);
                int fatSec = fatBytes / Bps;
                int rootSec = (rootEnt * 32) / Bps;
                int totalSec = Reserved + Fats * fatSec + rootSec + clusters * Spc;
                var img = new byte[totalSec * Bps];
                var fat = new ushort[clusters + 2];
                fat[0] = 0xFFF8;
                fat[1] = 0xFFFF;
                WriteBoot(img, totalSec, fatSec, rootEnt);
                var rootDir = new byte[rootSec * Bps];
                ushort nextCl = 2;
                int dataBase = (Reserved + Fats * fatSec + rootSec) * Bps;
                FillDir(tree, rootDir, true, 0, img, fat, ref nextCl, dataBase);
                int fatOff = Reserved * Bps;
                for (int i = 0; i < fat.Length; i++)
                {
                    img[fatOff + i * 2] = (byte)fat[i];
                    img[fatOff + i * 2 + 1] = (byte)(fat[i] >> 8);
                }
                Buffer.BlockCopy(img, fatOff, img, fatOff + fatSec * Bps, fatSec * Bps);
                Buffer.BlockCopy(rootDir, 0, img, (Reserved + Fats * fatSec) * Bps, rootDir.Length);
                return img;
            }

            private static Node LoadTree(string dir)
            {
                var n = new Node { Name = "", IsDir = true };
                AddChildren(n, dir);
                return n;
            }

            private static void AddChildren(Node parent, string dir)
            {
                string[] ents;
                try { ents = Directory.GetFileSystemEntries(dir); }
                catch { return; }
                Array.Sort(ents, StringComparer.OrdinalIgnoreCase);
                foreach (string p in ents)
                {
                    string name = Path.GetFileName(p);
                    if (string.IsNullOrEmpty(name) || name[0] == '.')
                        continue;
                    if (Directory.Exists(p))
                    {
                        var child = new Node { Name = name, IsDir = true };
                        AddChildren(child, p);
                        parent.Children.Add(child);
                        continue;
                    }
                    byte[] data;
                    try { data = File.ReadAllBytes(p); }
                    catch { continue; }
                    parent.Children.Add(new Node { Name = name, Data = data });
                }
            }

            private static int CountClusters(Node n, bool isRoot)
            {
                int c = 0;
                if (n.IsDir)
                {
                    if (!isRoot)
                        c += ClCount(DirSlots(n, false) * 32);
                    foreach (Node ch in n.Children)
                        c += CountClusters(ch, false);
                }
                else if (n.Data != null && n.Data.Length > 0)
                    c += ClCount(n.Data.Length);
                return c;
            }

            private static int DirSlots(Node n, bool isRoot)
            {
                int slots = isRoot ? 1 : 2;
                foreach (Node ch in n.Children)
                    slots += 1 + (ch.Name.Length + 1 + 12) / 13;
                return slots;
            }

            private static void FillDir(Node n, byte[] dir, bool isRoot, ushort parentCl,
                byte[] img, ushort[] fat, ref ushort nextCl, int dataBase)
            {
                int at = 0;
                var used83 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (isRoot)
                {
                    PutLabel(dir, 0);
                    at = 32;
                    used83.Add("HARD DISK.");
                }
                else
                {
                    Put83(dir, at, ".          ", 0x10, n.FirstCluster, 0);
                    at += 32;
                    Put83(dir, at, "..         ", 0x10, parentCl, 0);
                    at += 32;
                }
                foreach (Node ch in n.Children)
                {
                    string s83 = Make83(ch.Name, used83);
                    if (ch.IsDir)
                    {
                        int bytes = DirSlots(ch, false) * 32;
                        ch.FirstCluster = AllocChain(fat, ref nextCl, bytes);
                        var childDir = new byte[Align(bytes, ClusterSize)];
                        FillDir(ch, childDir, false, isRoot ? (ushort)0 : n.FirstCluster,
                            img, fat, ref nextCl, dataBase);
                        WriteClustered(img, dataBase, ch.FirstCluster, fat, childDir);
                        at += PutLfnAnd83(dir, at, ch.Name, s83, ch.FirstCluster, 0, true);
                    }
                    else
                    {
                        byte[] data = ch.Data ?? Array.Empty<byte>();
                        ushort first = 0;
                        if (data.Length > 0)
                        {
                            first = AllocChain(fat, ref nextCl, data.Length);
                            WriteClustered(img, dataBase, first, fat, data);
                        }
                        at += PutLfnAnd83(dir, at, ch.Name, s83, first, (uint)data.Length, false);
                    }
                }
            }

            private static ushort AllocChain(ushort[] fat, ref ushort next, int nbytes)
            {
                int ncl = ClCount(nbytes);
                ushort first = next;
                for (int i = 0; i < ncl; i++)
                {
                    ushort cl = next++;
                    fat[cl] = (i + 1 == ncl) ? (ushort)0xFFFF : next;
                }
                return first;
            }

            private static void WriteClustered(byte[] img, int dataBase, ushort first, ushort[] fat, byte[] src)
            {
                int off = 0;
                ushort cl = first;
                while (cl >= 2 && cl < 0xFFF8 && off < src.Length)
                {
                    int dest = dataBase + (cl - 2) * ClusterSize;
                    int take = Math.Min(src.Length - off, ClusterSize);
                    Buffer.BlockCopy(src, off, img, dest, take);
                    off += take;
                    cl = fat[cl];
                }
            }

            private static int ClCount(int nbytes)
            {
                return Math.Max(1, (nbytes + ClusterSize - 1) / ClusterSize);
            }

            private static void WriteBoot(byte[] img, int totalSec, int fatSec, int rootEnt)
            {
                img[0] = 0xEB; img[1] = 0x3C; img[2] = 0x90;
                Encoding.ASCII.GetBytes("MSDOS5.0").CopyTo(img, 3);
                img[11] = (byte)(Bps & 0xFF); img[12] = (byte)(Bps >> 8);
                img[13] = (byte)Spc;
                img[14] = (byte)Reserved; img[15] = 0;
                img[16] = (byte)Fats;
                img[17] = (byte)(rootEnt & 0xFF); img[18] = (byte)(rootEnt >> 8);
                if (totalSec < 65536)
                {
                    img[19] = (byte)(totalSec & 0xFF);
                    img[20] = (byte)(totalSec >> 8);
                }
                img[21] = 0xF8;
                img[22] = (byte)(fatSec & 0xFF); img[23] = (byte)(fatSec >> 8);
                img[24] = 0x3F; img[25] = 0;
                img[26] = 0xFF; img[27] = 0;
                Write32(img, 32, (uint)totalSec);
                img[38] = 0x29;
                img[39] = 0x54; img[40] = 0x56; img[41] = 0x32; img[42] = 0x48;
                Encoding.ASCII.GetBytes("HARD DISK  ").CopyTo(img, 43);
                Encoding.ASCII.GetBytes("FAT16   ").CopyTo(img, 54);
                img[510] = 0x55; img[511] = 0xAA;
            }

            private static void PutLabel(byte[] root, int at)
            {
                byte[] lab = Encoding.ASCII.GetBytes("HARD DISK  ");
                Buffer.BlockCopy(lab, 0, root, at, 11);
                root[at + 11] = 0x08;
            }

            private static void Put83(byte[] dir, int at, string name11, byte attr, ushort cl, uint size)
            {
                for (int i = 0; i < 11; i++)
                    dir[at + i] = (byte)name11[i];
                dir[at + 11] = attr;
                dir[at + 26] = (byte)(cl & 0xFF);
                dir[at + 27] = (byte)(cl >> 8);
                Write32(dir, at + 28, size);
            }

            private static int PutLfnAnd83(byte[] dir, int at, string lfn, string s83, ushort cl, uint size, bool isDir)
            {
                string name8 = s83.PadRight(11);
                byte chk = Checksum83(name8);
                string padded = lfn + "\0";
                int slots = (padded.Length + 12) / 13;
                int start = at;
                for (int s = slots; s >= 1; s--)
                {
                    if (at + 32 > dir.Length)
                        return 0;
                    int idx = (s - 1) * 13;
                    dir[at] = (byte)(s | (s == slots ? 0x40 : 0));
                    PutLfnChars(dir, at + 1, padded, idx, 5);
                    dir[at + 11] = 0x0F;
                    dir[at + 13] = chk;
                    PutLfnChars(dir, at + 14, padded, idx + 5, 6);
                    PutLfnChars(dir, at + 28, padded, idx + 11, 2);
                    at += 32;
                }
                if (at + 32 > dir.Length)
                    return slots * 32;
                Put83(dir, at, name8, (byte)(isDir ? 0x10 : 0x01), cl, size);
                return at + 32 - start;
            }

            private static void PutLfnChars(byte[] dir, int at, string s, int idx, int n)
            {
                for (int i = 0; i < n; i++)
                {
                    int p = idx + i;
                    ushort ch = (p < s.Length) ? s[p] : (ushort)0xFFFF;
                    dir[at + i * 2] = (byte)ch;
                    dir[at + i * 2 + 1] = (byte)(ch >> 8);
                }
            }

            private static byte Checksum83(string n)
            {
                byte s = 0;
                for (int i = 0; i < 11; i++)
                    s = (byte)(((s & 1) << 7) + (s >> 1) + (byte)n[i]);
                return s;
            }

            private static string Make83(string lfn, HashSet<string> used)
            {
                string leaf = lfn;
                string stem = leaf;
                string ext = "";
                int dot = leaf.LastIndexOf('.');
                if (dot > 0)
                {
                    stem = leaf.Substring(0, dot);
                    ext = leaf.Substring(dot + 1);
                }
                stem = Sanitize(stem);
                ext = Sanitize(ext);
                if (stem.Length == 0)
                    stem = "FILE";
                if (stem.Length > 8)
                    stem = stem.Substring(0, 8);
                if (ext.Length > 3)
                    ext = ext.Substring(0, 3);
                string key = (stem.PadRight(8) + ext.PadRight(3));
                int n = 1;
                while (used.Contains(key))
                {
                    string t = n.ToString();
                    string s = stem.Length + t.Length + 1 <= 8
                        ? stem + "~" + t
                        : stem.Substring(0, Math.Max(1, 8 - t.Length - 1)) + "~" + t;
                    s = Sanitize(s);
                    if (s.Length > 8)
                        s = s.Substring(0, 8);
                    key = (s.PadRight(8) + ext.PadRight(3));
                    n++;
                }
                used.Add(key);
                return key;
            }

            private static string Sanitize(string s)
            {
                var sb = new StringBuilder();
                foreach (char c in s.ToUpperInvariant())
                {
                    if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '~')
                        sb.Append(c);
                }
                return sb.ToString();
            }

            private static int Align(int n, int a)
            {
                return (n + a - 1) / a * a;
            }

            private static void Write32(byte[] b, int o, uint v)
            {
                b[o] = (byte)v;
                b[o + 1] = (byte)(v >> 8);
                b[o + 2] = (byte)(v >> 16);
                b[o + 3] = (byte)(v >> 24);
            }

            private sealed class Node
            {
                public string Name;
                public bool IsDir;
                public byte[] Data;
                public ushort FirstCluster;
                public readonly List<Node> Children = new List<Node>();
            }
        }
    }
}
