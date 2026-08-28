using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // User-supplied dump as the Hard Disk FAT volume. The host
    // folder is whatever the user feeds (CLI, FirmwarePath,
    // UVERSE_HARD_DISK / PROCESSOR_EMULATOR_HARD_DISK, last-used
    // path next to the exe, or a drop folder next to cwd/exe).
    // Hunt by name, recursively, case-insensitive. Take what is
    // present. Read-only: never write, delete, or rename dump
    // files. Not a BINBlk/BINFS/ExtraROM object. If etc.bin is
    // found, log it as the ExtraROM/XIP file hashes.bin already
    // names; firmware maps it.
    //
    // FSDMGR WFMO #2 (after BINBlk) is already waiting on the
    // BLOCK_DRIVER queue. Deliver HDProf there (7-char CE name).
    // GETNAME is HDProfile so Profiles\HDProfile / Folder Hard Disk
    // apply. mspart calls FSDMGR at FsdmgrIoImpl, not the binfs IAT.
    // After BINFS replaces the hive, HDProfile/FATFS open as
    // ERROR_BADKEY and Dll stays empty (\Windows\.dll). Serve the
    // ROM Folder=Hard Disk / Dll=fatfsd.dll values after the first
    // FAT DISK_READ. Mount GETNAME (0x71800) size 0 writes Hard Disk.
    // Filters enum names ROM sigcheckfilter.dll for HookVolume.
    // After LoadLibrary, FSDMGR opens the global child
    // StorageManager\\Filters\\sigcheckfilter, then
    // StorageManager\\sigcheckfilter. Serve those
    // children; parent Filters stays ERROR_BADKEY.
    // HookVolume 0x03DF22D0 jalrs 0x03DF2178, which
    // FSDMGR_DiskIoControl 0x71C20s the Folder name and
    // wcscmp against Hard Disk before walking \\ETC.bin.
    // The filter object at volume+68 is not a PDSK, so
    // firmware's +188 copy is empty. Same Folder already
    // served on GETNAME / HDProfile.
    // No fake MountDisk. No SetEvent of store/BootPhase/pump.
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
        // 0x03E8332C is FSDMGR_DiskIoControl (also the
        // GetDiskInfo export mspart already hits).
        public const uint FsdmgrGetDiskInfo = 0x03E8332C;
        public const uint FsdmgrStoreIoctl2 = 0x03E8B618;
        // Kernel / filesys RegOpenKeyEx and RegQueryValueEx.
        // FSDMGR uses these after the FAT boot read.
        public const uint KernelRegOpen = 0x8003D200;
        public const uint KernelRegQuery = 0x8003D2E0;
        public const uint FilesysRegOpen = 0x0001FEB0;
        // Between FS_RegOpenKeyEx and FS_RegEnum. FSDMGR queries
        // Dll here, not through the kernel export.
        public const uint FilesysRegQuery = 0x000200D8;
        public const uint FilesysRegQuery2 = 0x000204E0;
        public const uint FilesysRegEnum = 0x00020CC4;
        // FSDMGR InstallFilters enums through this wrapper
        // (coredll IAT), not filesys 0x00020CC4.
        public const uint FsdmgrRegEnum = 0x03E8961C;
        public const uint HkFatfs = 0xFA7F5001;
        public const uint HkProfile = 0xFA7F5002;
        public const uint HkProfileFatfs = 0xFA7F5003;
        public const uint HkPartTable = 0xFA7F5004;
        public const uint HkProfilePart = 0xFA7F5005;
        public const uint HkFilters = 0xFA7F5006;
        public const uint HkSigCheck = 0xFA7F5007;
        public const string FilterDll = "sigcheckfilter.dll";
        public const string FilterName = "sigcheckfilter";
        public const uint IoctlDiskGetInfo = 0x00071C00;
        public const uint IoctlDiskGetVolumeName = 0x00071C20;
        public const uint IoctlDiskReadEx = 0x00075C08;
        public const uint IoctlDiskGetStorageId = 0x00071C24;
        public const uint SectorSize = 512;

        private static readonly uint[] BlockDriverGuid =
        {
            0xA4E7EDDA, 0x4252E575, 0x95416B9D, 0x65B88BD4
        };

        private static string _root = "";
        private static string _offeredFeed = "";
        private static string _extraRom = "";
        private static byte[] _image = Array.Empty<byte>();
        private static bool _notified;
        private static bool _detailFilled;
        private static bool _opened;
        private static bool _fatSeen;
        private static readonly HashSet<string> _logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static bool IsPresent => _image != null && _image.Length > 0;
        public static bool IsOpen => _opened;
        public static bool DetailFilled => _detailFilled;
        public static string Root => _root;
        public static string ExtraRomPath => _extraRom;

        public static void OfferFeed(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                _offeredFeed = path.Trim();
        }

        public static void Attach()
        {
            _root = "";
            _extraRom = "";
            _image = Array.Empty<byte>();
            _notified = false;
            _detailFilled = false;
            _opened = false;
            _fatSeen = false;
            _logged.Clear();
            string dir = ResolveRoot();
            if (string.IsNullOrEmpty(dir))
            {
                System.Console.WriteLine("[HardDisk] no dump root (pass a folder, set " + EnvName + ", or FirmwarePath)");
                return;
            }
            try
            {
                _image = Fat16.Build(dir);
                _root = dir;
                System.Console.WriteLine($"[HardDisk] FAT {_image.Length} bytes root={dir} name={FolderName}");
                if (!string.IsNullOrEmpty(_extraRom))
                    System.Console.WriteLine("[HardDisk] ExtraROM etc.bin at " + _extraRom + " (firmware names ETC.BIN; not mapped here)");
                RememberLastUsed(dir);
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
            // Kernel 0x8003D200 is a shared thunk, not RegOpen.
            // Hook the filesys implementation those opens jalr into.
            if (pc == FilesysRegOpen)
                return TryRegOpen(registers, bus, ref programCounter);
            if (pc == KernelRegQuery || pc == FilesysRegQuery || pc == FilesysRegQuery2)
                return TryRegQuery(registers, bus, ref programCounter);
            if (pc == FilesysRegEnum || pc == FsdmgrRegEnum)
                return TryRegEnum(registers, bus, ref programCounter);
            return false;
        }

        // BINFS hive replace drops the ROM StorageManager keys.
        // Do not steal the attach-time HDProfile open: that still
        // comes from the boot hive (PartitionDriver=mspart.dll)
        // before any FAT DISK_READ.
        private static bool TryRegOpen(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened || !_fatSeen)
                return false;
            string path = ReadUtf16(bus, registers[5]);
            uint hk = KeyForPath(path);
            if (hk == 0)
                hk = ChildKey(registers[4], path);
            if (hk == 0)
                return false;
            uint phk = 0;
            try { phk = bus.Read32(registers[29] + 16); }
            catch { }
            if (phk != 0)
            {
                try { bus.Write32(phk, hk); }
                catch { return false; }
            }
            registers[2] = 0;
            programCounter = registers[31];
            if (_logged.Add("ro:" + path))
                System.Console.WriteLine($"[HardDisk] RegOpen \"{path}\" hk=0x{hk:X8} phk=0x{phk:X8} ra=0x{registers[31]:X8}");
            return true;
        }

        private static bool TryRegEnum(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            uint hKey = registers[4];
            if (hKey != HkFilters)
                return false;
            uint index = registers[5];
            if (index != 0)
            {
                registers[2] = 259;
                programCounter = registers[31];
                if (_logged.Add("re:end"))
                    System.Console.WriteLine("[HardDisk] RegEnum Filters done index=" + index);
                return true;
            }
            uint nameBuf = registers[6];
            uint cchPtr = registers[7];
            if (!LooksLikePtr(nameBuf))
                return false;
            string filt = FilterName;
            uint need = (uint)(filt.Length + 1);
            uint cch = 0;
            if (LooksLikePtr(cchPtr))
            {
                try { cch = bus.Read32(cchPtr); }
                catch { }
            }
            if (cch != 0 && cch < need)
            {
                try { bus.Write32(cchPtr, need); }
                catch { }
                registers[2] = 234;
                programCounter = registers[31];
                return true;
            }
            try
            {
                WriteUtf16(bus, nameBuf, filt);
                if (LooksLikePtr(cchPtr))
                    bus.Write32(cchPtr, (uint)filt.Length);
            }
            catch
            {
                return false;
            }
            registers[2] = 0;
            programCounter = registers[31];
            if (_logged.Add("re:" + filt))
                System.Console.WriteLine("[HardDisk] RegEnum Filters \"" + filt + "\"");
            return true;
        }

        private static bool TryRegQuery(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            uint hKey = registers[4];
            if (!IsStoreKey(hKey))
                return false;
            string name = ReadUtf16(bus, registers[5]);
            string text;
            uint dword;
            bool isDword;
            if (!LookupValue(hKey, name, out text, out dword, out isDword))
            {
                // Unknown a1: this entry may be RegClose/etc.
                // Only force FILE_NOT_FOUND on the kernel query.
                if (programCounter != KernelRegQuery)
                    return false;
                registers[2] = 2;
                programCounter = registers[31];
                if (_logged.Add("rqmiss:" + hKey.ToString("X") + ":" + name))
                    System.Console.WriteLine($"[HardDisk] RegQuery \"{name}\" hk=0x{hKey:X8} miss");
                return true;
            }
            uint type = isDword ? 4u : 1u;
            uint bytes = isDword ? 4u : (uint)((text.Length + 1) * 2);
            uint lpType = registers[7];
            uint lpData = 0, lpcb = 0;
            try { lpData = bus.Read32(registers[29] + 16); }
            catch { }
            try { lpcb = bus.Read32(registers[29] + 20); }
            catch { }
            if (!LooksLikePtr(lpData) && LooksLikePtr(registers[6]))
            {
                lpType = registers[6];
                lpData = registers[7];
                try { lpcb = bus.Read32(registers[29] + 16); }
                catch { }
            }
            if (LooksLikePtr(lpType))
            {
                try { bus.Write32(lpType, type); }
                catch { }
            }
            uint cb = 0;
            if (LooksLikePtr(lpcb))
            {
                try { cb = bus.Read32(lpcb); }
                catch { }
            }
            if (!LooksLikePtr(lpData))
            {
                if (LooksLikePtr(lpcb))
                {
                    try { bus.Write32(lpcb, bytes); }
                    catch { }
                }
                registers[2] = 0;
                programCounter = registers[31];
                return true;
            }
            if (cb != 0 && cb < bytes)
            {
                if (LooksLikePtr(lpcb))
                {
                    try { bus.Write32(lpcb, bytes); }
                    catch { }
                }
                registers[2] = 234;
                programCounter = registers[31];
                return true;
            }
            try
            {
                if (isDword)
                    bus.Write32(lpData, dword);
                else
                    WriteUtf16(bus, lpData, text);
                if (LooksLikePtr(lpcb))
                    bus.Write32(lpcb, bytes);
            }
            catch
            {
                return false;
            }
            registers[2] = 0;
            programCounter = registers[31];
            if (_logged.Add("rq:" + name + "=" + (isDword ? dword.ToString() : text)))
                System.Console.WriteLine($"[HardDisk] RegQuery \"{name}\"={(isDword ? dword.ToString() : text)}");
            return true;
        }

        private static uint KeyForPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;
            string n = path.Replace('/', '\\');
            if (n.Length >= 1 && n[0] == '\\')
                n = n.TrimStart('\\');
            if (EqualsIgnore(n, "System\\StorageManager\\FATFS"))
                return HkFatfs;
            // After the first FAT DISK_READ only (_fatSeen).
            // Folder=Hard Disk is what FSDMGR / sigcheckfilter HookVolume
            // use as the mount name. Attach-time HDProfile still misses
            // this gate so mspart comes from the boot hive.
            if (EqualsIgnore(n, "System\\StorageManager\\Profiles\\HDProfile"))
                return HkProfile;
            // Only the profile Filters key. Serving FATFS\\Filters
            // and StorageManager\\Filters too would InstallFilters
            // three times on the same handle.
            if (EqualsIgnore(n, "System\\StorageManager\\Profiles\\HDProfile\\FATFS\\Filters"))
                return HkFilters;
            if (IsFilterChild(n, "System\\StorageManager\\Profiles\\HDProfile\\FATFS\\Filters"))
                return HkSigCheck;
            // FSDMGR then opens these global children (not
            // the parent Filters key) to resolve HookVolume.
            if (IsFilterChild(n, "System\\StorageManager\\Filters"))
                return HkSigCheck;
            if (EqualsIgnore(n, "System\\StorageManager\\sigcheckfilter")
                || IsFilterChild(n, "System\\StorageManager"))
                return HkSigCheck;
            return 0;
        }

        private static uint ChildKey(uint parent, string sub)
        {
            if (string.IsNullOrEmpty(sub))
                return 0;
            string n = sub.Replace('/', '\\').TrimStart('\\');
            if (parent == HkFilters && IsSigCheckName(n))
                return HkSigCheck;
            if (parent == HkFatfs && EqualsIgnore(n, "Filters"))
                return HkFilters;
            if ((parent == HkFatfs || parent == HkProfile)
                && (EqualsIgnore(n, "FATFS\\Filters") || EqualsIgnore(n, "Filters")))
                return HkFilters;
            return 0;
        }

        private static bool IsFilterChild(string path, string parent)
        {
            if (!StartsWithIgnore(path, parent + "\\"))
                return false;
            return IsSigCheckName(path.Substring(parent.Length + 1));
        }

        private static bool IsSigCheckName(string n)
        {
            return EqualsIgnore(n, FilterName)
                || EqualsIgnore(n, "SigCheckFilter")
                || EqualsIgnore(n, "SigCheck")
                || EqualsIgnore(n, "SIGCHECK");
        }

        private static bool IsStoreKey(uint h)
        {
            return h == HkFatfs || h == HkProfile || h == HkProfileFatfs
                || h == HkPartTable || h == HkProfilePart
                || h == HkFilters || h == HkSigCheck;
        }

        private static bool LookupValue(uint hKey, string name, out string text, out uint dword, out bool isDword)
        {
            text = "";
            dword = 0;
            isDword = false;
            // FSDMGR queries the filter child default (lpValueName
            // NULL / "") for the DLL. Named Dll stays valid too.
            // Do not serve "" on FATFS — that miss is honest.
            if (hKey == HkSigCheck)
            {
                if (string.IsNullOrEmpty(name) || EqualsIgnore(name, "Dll"))
                {
                    text = FilterDll;
                    return true;
                }
                return false;
            }
            if (string.IsNullOrEmpty(name))
                return false;
            if (hKey == HkFatfs || hKey == HkProfileFatfs)
            {
                if (EqualsIgnore(name, "Dll"))
                {
                    text = "fatfsd.dll";
                    return true;
                }
                return false;
            }
            if (hKey == HkFilters)
            {
                if (IsSigCheckName(name) || EqualsIgnore(name, "Dll"))
                {
                    text = FilterDll;
                    return true;
                }
                return false;
            }
            if (hKey == HkProfile)
            {
                if (EqualsIgnore(name, "Folder") || EqualsIgnore(name, "Name"))
                {
                    text = FolderName;
                    return true;
                }
                if (EqualsIgnore(name, "DefaultFileSystem"))
                {
                    text = "FATFS";
                    return true;
                }
                if (EqualsIgnore(name, "PartitionDriver"))
                {
                    text = "mspart.dll";
                    return true;
                }
                return false;
            }
            if (hKey == HkPartTable || hKey == HkProfilePart)
            {
                if (EqualsIgnore(name, "04") || EqualsIgnore(name, "06")
                    || EqualsIgnore(name, "0B") || EqualsIgnore(name, "0C")
                    || EqualsIgnore(name, "0E"))
                {
                    text = "FATFS";
                    return true;
                }
                return false;
            }
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
            // 0x03E8332C passes v0 through to mspart, which beqz-fails
            // on 0. This entry is BOOL (1=ok), not a Win32 error code.
            registers[2] = err == 0 ? 1u : 0u;
            programCounter = registers[31];
            System.Console.WriteLine($"[HardDisk] IOCTL2 0x{code:X} err={err} v0={registers[2]} buf=0x{buf:X8} size={size}");
            return true;
        }

        private static bool TryGetDiskInfo(uint[] registers, MipsBus bus, ref uint programCounter)
        {
            if (!_opened)
                return false;
            uint a0 = registers[4];
            uint a1 = registers[5];
            // HookVolume IsTargetVolume (0x03DF2178) calls
            // FSDMGR_DiskIoControl(filter, 0x71C20, ...,
            // name, 520). Firmware 0x03E9242C copies WCHAR
            // from PDSK+188. a0 is the filter FSD at
            // volume+68, so that name is empty and the
            // Hard Disk compare skips \\ETC.bin.
            if (a1 == IoctlDiskGetVolumeName && _fatSeen)
            {
                uint buf = 0;
                uint size = 0;
                try { buf = bus.Read32(registers[29] + 16); }
                catch { }
                try { size = bus.Read32(registers[29] + 20); }
                catch { }
                if (buf == 0 || !LooksLikePtr(buf))
                    return false;
                if (size > 0x10000)
                    size = 0;
                if (size == 0)
                    size = 520;
                WriteUtf16(bus, buf, FolderName);
                try
                {
                    uint pret = bus.Read32(registers[29] + 24);
                    if (LooksLikePtr(pret))
                        bus.Write32(pret, (uint)((FolderName.Length + 1) * 2));
                }
                catch
                {
                }
                registers[2] = 1;
                programCounter = registers[31];
                if (_logged.Add("volname"))
                    System.Console.WriteLine("[HardDisk] DiskIoControl 0x71C20 \"" + FolderName + "\"");
                return true;
            }
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
                    uint spt = 63;
                    uint heads = 255;
                    uint cyl = sectors / (spt * heads);
                    if (cyl == 0)
                        cyl = 1;
                    bus.Write32(buf + 0, sectors);
                    bus.Write32(buf + 4, SectorSize);
                    bus.Write32(buf + 8, cyl);
                    bus.Write32(buf + 12, heads);
                    bus.Write32(buf + 16, spt);
                    // DISK_INFO_FLAG_MBR. Image has a real MBR + FAT16 LBA.
                    bus.Write32(buf + 20, 1);
                    return 0;
                }
                if (code == BinBlkMedia.IoctlDiskGetName && buf != 0)
                {
                    uint outLen = size;
                    if (outLen == 0)
                    {
                        try { outLen = bus.Read32(sp + 16); } catch { }
                        if (outLen == 0 || outLen > 0x10000)
                        {
                            try { outLen = bus.Read32(sp + 20); } catch { }
                        }
                        if (outLen > 0x10000)
                            outLen = 0;
                    }
                    // Attach: DWORD 0 + HDProfile at +4, size>=20.
                    // Mount IOCTL2: size 0 and WCHAR buf 0x040CEC60.
                    // After the FAT read, write Hard Disk at +0.
                    if (!_fatSeen && outLen >= 20)
                    {
                        bus.Write32(buf, 0);
                        WriteUtf16(bus, buf + 4, ProfileName);
                        return 0;
                    }
                    WriteUtf16(bus, buf, FolderName);
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
            catch (TlbMissException)
            {
                throw;
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
            uint at = KusegAlias(dest, sg);
            try
            {
                WriteSectors(bus, at, off, want);
            }
            catch (TlbMissException)
            {
                if (at != dest)
                    WriteSectors(bus, dest, off, want);
                else
                    throw;
            }
            bus.Write32(sg + 12, 0);
            _fatSeen = true;
            if (_logged.Add("read:" + start))
                System.Console.WriteLine($"[HardDisk] DISK_READ lba={start} n={num}");
            return 0;
        }

        // Slot 0 is the current-process alias. The SG lives in the
        // real slot (0x04xxxxxx); dest 0x00081340 is the same page.
        // Do not invent a PTE. If neither VA is in the TLB, throw so
        // the firmware refill/demand-zero path maps it.
        private static uint KusegAlias(uint dest, uint hint)
        {
            if (dest >= 0x02000000u)
                return dest;
            uint slot = hint & 0xFE000000u;
            if (slot < 0x02000000u || slot >= 0x80000000u)
                return dest;
            return slot | (dest & 0x01FFFFFFu);
        }

        private static void WriteSectors(MipsBus bus, uint dest, ulong off, uint want)
        {
            for (uint i = 0; i < want; i++)
            {
                byte b = 0;
                ulong src = off + i;
                if (src < (ulong)_image.Length)
                    b = _image[(int)src];
                Write8(bus, dest + i, b);
            }
        }

        private const string LastUsedName = "last_dump_root.txt";
        private const int HuntMaxDepth = 10;
        private const int HuntMaxVisit = 2500;

        private static readonly HashSet<string> VolumeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "etc.bin", "BOOT.PRF", "tv2clientce", "tv2clientce.exe", "Application"
        };

        private static readonly HashSet<string> HuntNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nk.bin", "etc.bin", "sec.bin", "XASEC.BIN",
            "BOOT.PRF", "BOOTPRF.BAK",
            "tv2clientce", "tv2clientce.exe",
            "Application", "PlayReady",
            "raven_fw.bin", "WirelessFirmware.img", "ContentVersion.txt",
            "boot.sig", "runonce.sig", "Hard Disk"
        };

        internal static string ResolveRoot()
        {
            foreach (string raw in CandidateFeeds())
            {
                string feed = NormalizeFeed(raw);
                if (string.IsNullOrEmpty(feed))
                    continue;
                System.Console.WriteLine("[HardDisk] hunt feed=" + feed);
                string attach = HuntAttach(feed);
                if (!string.IsNullOrEmpty(attach))
                    return attach;
            }
            return "";
        }

        public static string HuntAttach(string feed)
        {
            if (string.IsNullOrEmpty(feed) || !Directory.Exists(feed))
                return "";
            var scores = new Dictionary<string, VolumeScore>(StringComparer.OrdinalIgnoreCase);
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WalkHunt(feed, 0, 0, scores, seenNames);
            string bestVol = "";
            string bestLoose = "";
            int bestVolMark = 0;
            int bestLooseMark = 0;
            bool bestHasEtc = false;
            foreach (var kv in scores)
            {
                VolumeScore s = kv.Value;
                if (s.Markers > 0)
                {
                    bool take = s.Markers > bestVolMark;
                    if (!take && s.Markers == bestVolMark)
                    {
                        if (s.HasEtc && !bestHasEtc)
                            take = true;
                        else if (s.HasEtc == bestHasEtc
                            && (bestVol.Length == 0 || kv.Key.Length < bestVol.Length))
                            take = true;
                    }
                    if (take)
                    {
                        bestVolMark = s.Markers;
                        bestHasEtc = s.HasEtc;
                        bestVol = kv.Key;
                    }
                }
                else if (s.Extra > 0
                    && (s.Extra > bestLooseMark
                        || (s.Extra == bestLooseMark
                            && (bestLoose.Length == 0 || kv.Key.Length < bestLoose.Length))))
                {
                    bestLooseMark = s.Extra;
                    bestLoose = kv.Key;
                }
            }
            if (!string.IsNullOrEmpty(bestVol))
                return bestVol;
            if (!string.IsNullOrEmpty(bestLoose))
                return bestLoose;
            return "";
        }

        private static int WalkHunt(string dir, int depth, int visited,
            Dictionary<string, VolumeScore> scores, HashSet<string> seenNames)
        {
            if (depth > HuntMaxDepth || visited >= HuntMaxVisit)
                return visited;
            visited++;
            string[] ents;
            try { ents = Directory.GetFileSystemEntries(dir); }
            catch { return visited; }
            foreach (string p in ents)
            {
                if (visited >= HuntMaxVisit)
                    break;
                string name = Path.GetFileName(p);
                if (SkipHuntName(name))
                    continue;
                bool isDir = false;
                try { isDir = Directory.Exists(p); }
                catch { continue; }
                if (isDir)
                {
                    try
                    {
                        if ((File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0)
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (HuntNames.Contains(name) && seenNames.Add(name))
                    System.Console.WriteLine("[HardDisk] found " + name + " at " + p);
                if (name.Equals("etc.bin", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(_extraRom))
                    _extraRom = p;
                if (VolumeNames.Contains(name) || HuntNames.Contains(name))
                {
                    VolumeScore s;
                    if (!scores.TryGetValue(dir, out s))
                    {
                        s = new VolumeScore();
                        scores[dir] = s;
                    }
                    if (VolumeNames.Contains(name))
                    {
                        s.Markers++;
                        if (name.Equals("etc.bin", StringComparison.OrdinalIgnoreCase))
                            s.HasEtc = true;
                    }
                    else
                        s.Extra++;
                }
                if (isDir)
                    visited = WalkHunt(p, depth + 1, visited, scores, seenNames);
            }
            return visited;
        }

        private static bool SkipHuntName(string name)
        {
            if (string.IsNullOrEmpty(name) || name[0] == '.')
                return true;
            if (name.Equals("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase))
                return true;
            if (name.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private static IEnumerable<string> CandidateFeeds()
        {
            if (!string.IsNullOrWhiteSpace(_offeredFeed))
                yield return _offeredFeed;
            foreach (string a in CommandLineFeeds())
                yield return a;
            yield return Environment.GetEnvironmentVariable(EnvName);
            yield return Environment.GetEnvironmentVariable(EnvNameAlt);
            yield return SettingsFirmwarePath();
            yield return ReadLastUsed(AppDomain.CurrentDomain.BaseDirectory);
            string cwd = Environment.CurrentDirectory;
            yield return ReadLastUsed(cwd);
            yield return Path.Combine(cwd, "UverseDriveE");
            string bas = AppDomain.CurrentDomain.BaseDirectory ?? "";
            yield return Path.Combine(bas, "UverseDriveE");
            yield return Path.Combine(cwd, "Uverse Drive E");
            yield return Path.Combine(bas, "Uverse Drive E");
        }

        private static IEnumerable<string> CommandLineFeeds()
        {
            string[] args;
            try { args = Environment.GetCommandLineArgs(); }
            catch { yield break; }
            if (args == null)
                yield break;
            for (int i = 1; i < args.Length; i++)
            {
                string a = args[i];
                if (string.IsNullOrWhiteSpace(a) || a[0] == '-')
                    continue;
                yield return a;
            }
        }

        private static string SettingsFirmwarePath()
        {
            try
            {
                string p = global::ProcessorEmulator.ConfigManager.Config.FirmwarePath;
                if (!string.IsNullOrWhiteSpace(p))
                    return p;
            }
            catch
            {
            }
            return "";
        }

        private static string NormalizeFeed(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "";
            string path = raw.Trim().Trim('"');
            try
            {
                if (File.Exists(path))
                    return Path.GetFullPath(Path.GetDirectoryName(path) ?? "");
                if (Directory.Exists(path))
                    return Path.GetFullPath(path);
            }
            catch
            {
            }
            return "";
        }

        private static string ReadLastUsed(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return "";
            try
            {
                string file = Path.Combine(dir, LastUsedName);
                if (!File.Exists(file))
                    return "";
                string text = File.ReadAllText(file).Trim();
                if (text.Length == 0)
                    return "";
                return text;
            }
            catch
            {
            }
            return "";
        }

        private static void RememberLastUsed(string attach)
        {
            if (string.IsNullOrEmpty(attach))
                return;
            string destDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(destDir))
                return;
            try
            {
                string destFull = Path.GetFullPath(destDir);
                string attachFull = Path.GetFullPath(attach);
                if (destFull.StartsWith(attachFull, StringComparison.OrdinalIgnoreCase))
                    return;
                File.WriteAllText(Path.Combine(destFull, LastUsedName), attachFull);
            }
            catch
            {
            }
        }

        private sealed class VolumeScore
        {
            public int Markers;
            public int Extra;
            public bool HasEtc;
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
