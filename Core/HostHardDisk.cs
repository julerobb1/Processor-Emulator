using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // Existing Uverse Drive E / UverseDriveE / UVERSE_HARD_DISK
    // attach is unchanged: if that folder is present and already
    // looks like the volume, use it. A user may also point at a
    // drive or dump folder (CLI, FirmwarePath, drop, env). Hunt
    // that root and its shallow children by name, case-insensitive.
    // Take what is present. The path need not contain Uverse.
    // Read-only: never write, delete, or rename dump files. Not a
    // BINBlk/BINFS object. Hunt every etc.bin plus any other B000FF
    // sitting next to nk.bin. NkBinLoader maps each file's records
    // at THAT file's imageStart so ExtraROM XIP (tv2clientce.exe
    // and the rest) is in RAM. A Dumps\etc.bin\ extract folder is
    // that same tree unpacked — log it, do not pack it into a fake
    // B000FF. Firmware CreateFile of ETC.bin / BOOT.PRF / sec.bin
    // is the Hard Disk path, not a second XIP. Firmware has no skip
    // for the missing 0x81360000 image. Do not invent that map.
    // Host drops leftover inherit pairs at publish/copy (start==0,
    // start==end, end<start, or size>=32MB). Keep the NK pair.
    // Do not CreateProcess(tv2clientce).
    //
    // filesys hive-init opens \Windows\boot.hv first. boot.hv
    // BootVars Flags=3 (real DWORD). Low nibble != 0 starts
    // device.exe and waits; Start DevMgr is not in that hive.
    // SystemHive is Documents and Settings\system.hv, which is
    // not on this volume. The NK FILESentry default.hv is 266240
    // uncompressed (compressed 65188 at 0x802FA8AC) and holds
    // HKLM\init Launch20/30/56. Helper 0x0003EE14 already opens
    // boot.hv. Clear the Flags nibble at 0x0002A7F8 so that
    // same helper runs for \Windows\default.hv. Do not write
    // Launch keys. Do not SetEvent. Do not invent 0x81360000.
    // RunApps enums Launch20/30/50/53/56/95 then CreateProcess
    // only after Depend WORDs are ready. Depend56 is 20/30/53.
    // Launch record +4 is the ready slot. RunApps writes +4=1
    // only on CreateProcess fail or the device.exe / BootPhase2
    // miss. Success leaves +4=0. filesys 0x000177EC (coredll
    // SignalStarted ordinal 639 → FILESYS API table 0x000111A8)
    // matches a0 to record+0, writes +4=1, then EventModify
    // (a1=3 SET) the unnamed event at 0x00059468 so the Depend
    // WaitForMultipleObjects INFINITE at 0x000180A4 returns.
    // gwes calls SignalStarted(_wtol(cmd)) at slotted 0x0001634C
    // then OpenEvent + EventModify SYSTEM/GweApiSetReady (not
    // GRAPHICS) at slotted 0x00016354. TOC[7] XIP text is
    // 0x80146000 (VA 0x00011000); entry 0x8014B3C8 / WinMain
    // 0x8014B014. CreateProcess sets the new thread PC to
    // trampoline 0x8001FF38, which jalrs module+0x5C.
    // 0x8001E960 skips that store when entryrva is 0.
    // EXE jal/j are linked at VA 0x00010000; 0x800140A8
    // (ASID/slot attach) is jr $ra, so slot 0 still
    // fetches filesys at 0x000163C8. Alias current-process
    // XIP o32[0] to dataptr and keep startip as the VA.
    // 0x8001DD6C skips CallDLL when +0x50 is useg/C2.
    // After WinMain, the first gwes-thread INFINITE WFSO is
    // coredll ThreadExceptionExit (0x03F74B18) waiting on
    // its CreateThread handle (start 0x03FBF69C). Who
    // signals it: that worker's ExitThread, not SetEvent.
    // Log the exception that entered that path. Do not
    // SetEvent. DisplayDll is inside 0x00024BE8 (Reg
    // DisplayDll / Class). LoadDriver(ddi_nop.dll) is
    // proven; TOC-attach ExtraROM TOC[33] on that miss.
    // Do not invent 0x81360000. Do not SetEvent.
    // Display=ddi_nop.dll (default.hv; ExtraROM
    // TOC[33] vbase 0x03980000). Do not SetEvent GweApi or
    // Launch30. Do not host CreateProcess(tv2clientce).
    // ExtraROM FILE tv2clientce.exe is the 5120-byte stub,
    // not the 90-byte root file.
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
        // Inherit LIST path / VALLOC jal. Firmware skips a pair only
        // when start==0 or start==end. Host filters leftovers at
        // SaveList / memcpy of the 0x24 record.
        public const uint InheritListPath = 0x8001B6EC;
        public const uint InheritVallocJal = 0x8001B724;
        public const uint InheritSaveList = 0x8001687C;
        public const uint InheritMemcpy = 0x80016A44;
        public const uint BinfsInheritFill = 0x03EA2B84;
        public const uint InheritRecordSize = 0x24;
        public const uint InheritSlotBytes = 0x02000000;
        // hive-init 0x0002A5E8: Flags nibble gate, then the existing
        // \Windows\default.hv helper. RunApps 0x00017BAC is the
        // HKLM\init open that was ERROR_BADKEY on boot.hv alone.
        public const uint HiveFlagsGate = 0x0002A7F8;
        public const uint HiveDefaultOpen = 0x0002ACD0;
        public const uint HiveDefaultOpenRet = 0x0002ACD8;
        public const uint RunAppsInitChk = 0x00017BAC;
        public const uint RunAppsLaunchCmp = 0x00017C58;
        public const uint RunAppsDependMiss = 0x00017FB0;
        public const uint RunAppsCprocRet = 0x00018080;
        // FILESYS API: coredll SignalStarted. Writes launch +4.
        public const uint FilesysSignalStarted = 0x000177EC;
        public const uint LaunchCountPtr = 0x00059460;
        public const uint LaunchReadyEvent = 0x00059468;
        public const uint LaunchTablePtr = 0x0005946C;
        public const uint LaunchRecordSize = 0x250;
        // gwes preferred 0x00010000; lives in a CE 32MB slot.
        // Slot 0 is filesys — do not treat 0x0001634C there as gwes.
        public const uint GwesSignalStarted = 0x0001634C;
        public const uint GwesGweApiReady = 0x00016354;
        public const uint GwesVaEntry = 0x000163C8;
        public const uint GwesVaWinMain = 0x00016014;
        public const uint GwesVaDisplayDll = 0x00024CD4;
        public const uint GwesVaDisplayFn = 0x00024BE8;
        public const uint GwesVaWinMainJal = 0x00016088;
        public const uint GwesVaWinMainSkip = 0x00016394;
        public const uint GwesInitFlag = 0x000B7A1D;
        public const uint GwesRomInitFlag = 0x801EAA1D;
        public const uint FilesysRomText = 0x80105000;
        public const uint CeSlotMask = 0x01FFFFFF;
        public const uint CeSlotBase = 0xFE000000;
        // TOC[7] o32[0] dataptr; VA = ROM - GwesRomText + 0x00011000.
        public const uint GwesRomText = 0x80146000;
        public const uint GwesRomTextEnd = 0x801EADE0;
        public const uint GwesRomEntry = 0x8014B3C8;
        public const uint GwesRomWinMain = 0x8014B014;
        public const uint GwesRomSignal = 0x8014B34C;
        public const uint GwesRomGweApi = 0x8014B354;
        public const uint GwesRomDisplayDll = 0x80159CD4;
        public const uint GwesRomDisplayFn = 0x80159BE8;
        public const uint GwesSlot = 0x08000000;
        public const uint DdiNopVbase = 0x03980000;
        public const uint DdiNopVend = 0x039B0000;
        public const uint DdiNopEntry = 0x03998014;
        public const uint CoredllActivateDevice = 0x03F6AD08;
        public const uint CoredllActivateDeviceEx = 0x03F6AD54;
        public const uint CoredllExitThread = 0x03F74844;
        public const uint CoredllLoadLibraryW = 0x03F6CB50;
        public const uint CoredllLoadLibraryExW = 0x03F6C84C;
        public const uint CoredllWaitSo = 0x03F6B9AC;
        public const uint CoredllWaitMo = 0x03F6B914;
        // WinMain first jal is SetKMode. The later INFINITE
        // WFSO is ThreadExceptionExit waiting on its
        // CreateThread handle (not an event). Do not SetEvent.
        public const uint CoredllSetKMode = 0x03F71098;
        public const uint CoredllCreateThread = 0x03F71E04;
        public const uint CoredllThreadExceptionExit = 0x03F74B18;
        public const uint CoredllIsApiReady = 0x03F73240;
        public const uint CoredllLoadDriver = 0x03F70C74;
        public const uint CoredllLoadDriverRet = 0x03F70C88;
        public const uint CoredllMessageBoxW = 0x03F8A500;
        public const uint ExceptionWorker = 0x03FBF69C;
        public const uint GwesVaAfterKmode = 0x00016090;
        public const uint GwesVaHeapCreate = 0x00048C8C;
        public const uint GwesVaDisplayParent = 0x00023C60;
        public const uint GwesVaAvHelper = 0x0005377C;
        public const uint GwesVaAvCaller = 0x0005BCF8;
        public const uint GwesDispObj = 0x000BA954;
        public const uint GwesIatGetProc = 0x000B6008;
        public const uint GwesIatLoadLib = 0x000B600C;
        public const uint GwesIatHeapCreate = 0x000B621C;
        public const uint ExceptionVector = 0x80000180;
        public const uint OemIdle = 0x80059E98;
        public const uint OemIdleLoop = 0x80059D20;
        public const uint FilesysCreateProcess = 0x0004BCA4;
        public const uint KernelCreateProcess = 0x80034D2C;
        public const uint KernelValloc = 0x800283FC;
        public const uint ThreadStartTrampoline = 0x8001FF38;
        public const uint ThreadContextSetup = 0x80020BE4;
        public const uint ThreadCtxPc = 0xEC;
        public const uint ThreadStartip = 0x5C;
        public const uint ThreadStack = 0x24;
        public const uint ThreadProc = 0x0C;
        public const uint ProcModule = 0x50;
        public const uint ErrorBadKey = 0x3F2;
        public const uint ThreadPtr = 0xFFFFDAC0;
        public const uint ThreadLastErr = 56;
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
        private static string _nkDir = "";
        private static readonly List<string> _extraRoms = new List<string>();
        private static byte[] _image = Array.Empty<byte>();
        private static bool _notified;
        private static bool _detailFilled;
        private static bool _opened;
        private static bool _fatSeen;
        private static readonly HashSet<string> _logged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static bool _inheritListLogged;
        private static readonly HashSet<uint> _vallocLogged = new HashSet<uint>();
        private static bool _extractLogged;
        private static bool _hiveFlagsLogged;
        private static string _cprocName = "";
        private static uint _cprocRa;
        private static uint _cprocThread;
        private static bool _gwesWatch;
        private static bool _gwesIn;
        private static uint _gwesLastPc;
        private static bool _gwesSummary;
        private static bool _gwesSawExit;
        private static bool _gwesSawWait;
        private static bool _gwesSawDdi;
        private static bool _gwesSawSignal;
        private static bool _gwesSawThrEx;
        private static bool _gwesSawCreateThr;
        private static bool _gwesSawWorker;
        private static int _gwesExnLogged;
        private static uint _gwesThr;

        public static bool IsPresent => _image != null && _image.Length > 0;
        public static bool IsOpen => _opened;
        public static bool DetailFilled => _detailFilled;
        public static string Root => _root;
        public static string ExtraRomPath
        {
            get
            {
                foreach (string p in _extraRoms)
                {
                    if (Path.GetFileName(p).Equals("etc.bin", StringComparison.OrdinalIgnoreCase))
                        return p;
                }
                return _extraRoms.Count > 0 ? _extraRoms[0] : "";
            }
        }

        public static IReadOnlyList<string> ExtraRomPaths => _extraRoms;

        public static void OfferFeed(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                _offeredFeed = path.Trim();
        }

        public static void Attach()
        {
            _root = "";
            _nkDir = "";
            _extraRoms.Clear();
            _image = Array.Empty<byte>();
            _notified = false;
            _detailFilled = false;
            _opened = false;
            _fatSeen = false;
            _logged.Clear();
            _inheritListLogged = false;
            _vallocLogged.Clear();
            _extractLogged = false;
            _hiveFlagsLogged = false;
            _cprocName = "";
            _cprocRa = 0;
            _cprocThread = 0;
            _gwesWatch = false;
            _gwesIn = false;
            _gwesLastPc = 0;
            _gwesSummary = false;
            _gwesSawExit = false;
            _gwesSawWait = false;
            _gwesSawDdi = false;
            _gwesSawSignal = false;
            _gwesSawThrEx = false;
            _gwesSawCreateThr = false;
            _gwesSawWorker = false;
            _gwesExnLogged = 0;
            _gwesThr = 0;
            CeRomTocFiles.ResetExeXipAlias();
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
                NoteDumpImages(dir);
                if (!string.IsNullOrEmpty(_nkDir))
                    NoteDumpImages(_nkDir);
                foreach (string extra in _extraRoms)
                    System.Console.WriteLine("[HardDisk] ExtraROM candidate " + extra);
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
            if (pc == BinfsInheritFill)
            {
                uint plus14 = registers[12];
                uint plus18 = registers[24];
                uint start = plus14 << 16;
                if (BadInheritPair(start, plus18))
                    System.Console.WriteLine("[Inherit] skip +14=0x" + plus14.ToString("X8") +
                        " +18=0x" + plus18.ToString("X8") +
                        " start=0x" + start.ToString("X8") +
                        " end=0x" + plus18.ToString("X8"));
                return false;
            }
            if (pc == InheritSaveList)
            {
                if (registers[4] == InheritRecordSize)
                    CompactInheritRecord(bus, registers[5]);
                return false;
            }
            if (pc == InheritMemcpy)
            {
                if (registers[6] == InheritRecordSize)
                    CompactInheritRecord(bus, registers[5]);
                return false;
            }
            if (pc == InheritListPath)
            {
                LogInheritList(bus, registers[2]);
                return false;
            }
            if (pc == InheritVallocJal)
            {
                uint a0 = registers[4];
                uint a1 = registers[5];
                uint a2 = registers[6];
                if (_vallocLogged.Add(a0))
                    System.Console.WriteLine("[Inherit] VALLOC a0=0x" + a0.ToString("X8") +
                        " a1=0x" + a1.ToString("X8") +
                        " a2=0x" + a2.ToString("X8"));
                return false;
            }
            if (pc == KernelValloc && (!string.IsNullOrEmpty(_cprocName)
                || _logged.Contains("hive:ldde32")))
            {
                uint a0 = registers[4];
                uint a1 = registers[5];
                uint a2 = registers[6];
                string who = !string.IsNullOrEmpty(_cprocName) ? _cprocName : "LoadE32";
                if (_logged.Add("hive:va:" + who + ":" + a0.ToString("X")))
                    System.Console.WriteLine("[Hive] VALLOC \"" + who +
                        "\" a0=0x" + a0.ToString("X8") +
                        " a1=0x" + a1.ToString("X8") +
                        " a2=0x" + a2.ToString("X8"));
                return false;
            }
            if (pc == ThreadContextSetup && !string.IsNullOrEmpty(_cprocName))
            {
                LogCprocThreadCtx(registers, bus);
                return false;
            }
            if (pc == ThreadStartTrampoline)
            {
                CeRomTocFiles.TryFillProcExeStartip(bus);
                LogThreadTrampoline(registers, bus);
                return false;
            }
            if (pc == CeRomTocFiles.LoadExeE32Ret)
            {
                CeRomTocFiles.TryFillProcExeStartip(bus);
                LogLoadExeStartip(bus);
                return false;
            }
            if (pc == CeRomTocFiles.CallDllStartip)
            {
                CeRomTocFiles.TryFillTocStartip(bus, registers[23], true);
                LogCallDllStartip(registers, bus);
                return false;
            }
            if (pc == CeRomTocFiles.XipExeCallDllSkip)
            {
                LogXipExeCallDllSkip(registers, bus);
                return false;
            }
            if (pc == HiveFlagsGate)
            {
                TryRomDefaultHive(registers, bus);
                return false;
            }
            if (pc == HiveDefaultOpen)
            {
                LogHiveHelper(registers, bus);
                return false;
            }
            if (pc == HiveDefaultOpenRet)
            {
                LogHiveHelperRet(registers);
                return false;
            }
            if (pc == RunAppsInitChk)
            {
                LogRunAppsInit(registers);
                return false;
            }
            if (pc == RunAppsLaunchCmp)
            {
                LogRunAppsLaunch(registers, bus);
                return false;
            }
            if (pc == RunAppsDependMiss)
            {
                LogRunAppsDepend(registers, bus);
                return false;
            }
            if (pc == FilesysSignalStarted)
            {
                LogSignalStarted(registers, bus);
                return false;
            }
            // CE user slots are 0x02000000..0x20000000. 0x8001634C is
            // KSEG0 filesys, not gwes (slot 0 / filesys owns 0x0001634C).
            uint slot = pc >> 25;
            if (slot >= 1 && slot <= 16)
            {
                uint gwesOff = pc & CeSlotMask;
                if (gwesOff == GwesSignalStarted || gwesOff == GwesGweApiReady)
                {
                    LogGwesReadySite(pc, gwesOff, registers, bus);
                    return false;
                }
            }
            if (pc == CeRomTocFiles.MapO32VirtualCopy
                && _logged.Contains("hive:ldde32")
                && CeRomTocFiles.TryRedirectExtraRomVirtualCopyToDecompress(
                    bus, registers, ref programCounter))
                return false;
            ObserveGwesPath(pc, registers, bus);
            if (pc == FilesysCreateProcess
                || (pc == KernelCreateProcess && _cprocRa == 0))
            {
                LogHiveCreateProcess(registers, bus);
                return false;
            }
            if (_cprocRa != 0 && (pc == _cprocRa || pc == RunAppsCprocRet))
            {
                LogHiveCreateProcessRet(registers, bus);
                return false;
            }
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

        // Do not rewrite slot +14/+18 into packed offsets. Drop the
        // published pair when start/end cannot be a 32MB slot region.
        private static bool BadInheritPair(uint start, uint end)
        {
            if (start == 0 || start == end)
                return true;
            if (end < start)
                return true;
            return (end - start) >= InheritSlotBytes;
        }

        private static void CompactInheritRecord(MipsBus bus, uint rec)
        {
            if (bus == null || rec == 0)
                return;
            try
            {
                uint count = bus.Read32(rec + 8);
                if (count == 0 || count > 8)
                    return;
                uint write = 0;
                for (uint i = 0; i < count; i++)
                {
                    uint pair = rec + 12 + i * 8;
                    uint start = bus.Read32(pair);
                    uint end = bus.Read32(pair + 4);
                    if (BadInheritPair(start, end))
                    {
                        System.Console.WriteLine("[Inherit] drop pair start=0x" + start.ToString("X8") +
                            " end=0x" + end.ToString("X8"));
                        continue;
                    }
                    if (write != i)
                    {
                        bus.Write32(rec + 12 + write * 8, start);
                        bus.Write32(rec + 16 + write * 8, end);
                    }
                    write++;
                }
                if (write == count)
                    return;
                bus.Write32(rec + 8, write);
                for (uint i = write; i < count; i++)
                {
                    bus.Write32(rec + 12 + i * 8, 0);
                    bus.Write32(rec + 16 + i * 8, 0);
                }
                System.Console.WriteLine("[Inherit] compacted count=" + write + " (was " + count + ")");
            }
            catch
            {
            }
        }

        // boot.hv Flags=3 takes Start DevMgr / device.exe and never
        // calls the \Windows\default.hv helper. That hive is the NK
        // FILESentry with Launch20/30/56. Clear the nibble so the
        // existing beq at 0x0002A7F8 falls into 0x0002AB04.
        private static void TryRomDefaultHive(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 24 || bus == null)
                return;
            uint s3 = registers[19];
            if (!LooksLikePtr(s3))
                return;
            uint flags;
            try { flags = bus.Read32(s3); }
            catch { return; }
            if ((flags & 0xF) == 0)
                return;
            try { bus.Write32(s3, flags & ~0xFu); }
            catch { return; }
            registers[24] = 0;
            if (!_hiveFlagsLogged)
            {
                _hiveFlagsLogged = true;
                System.Console.WriteLine("[Hive] Flags 0x" + flags.ToString("X") +
                    " would skip \\Windows\\default.hv; take ROM FILESentry");
            }
        }

        private static void LogHiveHelper(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 7 || bus == null)
                return;
            string path = ReadUtf16(bus, registers[5]);
            if (string.IsNullOrEmpty(path))
                path = "(null)";
            if (_logged.Add("hive:open:" + path))
                System.Console.WriteLine("[Hive] helper \"" + path + "\" a3=" + registers[7]);
        }

        private static void LogHiveHelperRet(uint[] registers)
        {
            if (registers == null || registers.Length <= 2)
                return;
            if (_logged.Add("hive:openret"))
                System.Console.WriteLine("[Hive] helper v0=0x" + registers[2].ToString("X8"));
        }

        private static void LogRunAppsInit(uint[] registers)
        {
            if (registers == null || registers.Length <= 2)
                return;
            uint v0 = registers[2];
            if (!_logged.Add("hive:init:" + v0.ToString("X")))
                return;
            string note = v0 == 0 ? "OK" : (v0 == ErrorBadKey ? "ERROR_BADKEY" : "");
            System.Console.WriteLine("[Hive] RunApps HKLM\\init v0=0x" + v0.ToString("X8") +
                (note.Length == 0 ? "" : " " + note));
        }

        private static void LogRunAppsLaunch(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 29 || bus == null)
                return;
            if (registers[2] != 0)
                return;
            string name = ReadUtf16(bus, registers[29] + 96);
            if (string.IsNullOrEmpty(name))
                return;
            if (_logged.Add("hive:launch:" + name))
                System.Console.WriteLine("[Hive] RunApps \"" + name + "\"");
        }

        // RunApps 0x00017FB0: Depend WORD in v0, ready flag at
        // record+4. Zero means WaitForMultipleObjects INFINITE
        // (0x000180A4) instead of CreateProcess. Depend56 is
        // 20/30/53. Do not SetEvent.
        private static void LogRunAppsDepend(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 23 || bus == null)
                return;
            if (registers[13] != 0)
                return;
            uint need = registers[2];
            string img = ReadUtf16(bus, registers[23]);
            if (string.IsNullOrEmpty(img))
                img = "(null)";
            if (!_logged.Add("hive:dep:" + img + ":" + need.ToString("X")))
                return;
            System.Console.WriteLine("[Hive] Depend wait \"" + img + "\" need=" + need);
            LogLaunchReadySlots(bus, need);
        }

        // filesys 0x000177EC: the only success-path writer of
        // launch record+4. a0 is the Launch number (20, 30, …).
        // a0==0 pulses 0x00059468 and does not set any +4.
        private static void LogSignalStarted(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 4)
                return;
            uint a0 = registers[4];
            if (!_logged.Add("hive:sig:" + a0.ToString("X")))
                return;
            System.Console.WriteLine("[Hive] SignalStarted a0=" + a0 +
                " (filesys 0x000177EC writes record+4, EventModify SET 0x00059468)");
            if (bus != null)
                LogLaunchReadySlots(bus, a0);
        }

        // Observe only. Do not SetEvent GweApi or Launch30.
        private static void ObserveGwesPath(uint pc, uint[] registers, MipsBus bus)
        {
            if (pc == GwesRomEntry || pc == GwesVaEntry || IsSlottedVa(pc, GwesVaEntry))
            {
                NoteGwesPc(pc, "entry", GwesRomEntry, bus);
                return;
            }
            if (pc == GwesRomWinMain || pc == GwesVaWinMain || IsSlottedVa(pc, GwesVaWinMain))
            {
                NoteGwesPc(pc, "WinMain", GwesRomWinMain, bus);
                LogGwesInitFlag(bus);
                return;
            }
            if (pc == GwesVaWinMainJal || IsSlottedVa(pc, GwesVaWinMainJal))
            {
                NoteGwesPc(pc, "WinMain-jal", GwesRomWinMain + (GwesVaWinMainJal - GwesVaWinMain), bus);
                LogGwesIat(bus);
                return;
            }
            if (pc == GwesVaAfterKmode || IsSlottedVa(pc, GwesVaAfterKmode))
            {
                NoteGwesPc(pc, "after-SetKMode", GwesRomWinMain + (GwesVaAfterKmode - GwesVaWinMain), bus);
                return;
            }
            if (pc == GwesVaHeapCreate || IsSlottedVa(pc, GwesVaHeapCreate))
            {
                NoteGwesPc(pc, "HeapCreate-site", GwesRomText + (GwesVaHeapCreate - 0x00011000), bus);
                return;
            }
            if (pc == GwesVaDisplayParent || IsSlottedVa(pc, GwesVaDisplayParent))
            {
                NoteGwesPc(pc, "display-parent", GwesRomText + (GwesVaDisplayParent - 0x00011000), bus);
                LogGwesDispObj(bus, "display-parent");
                return;
            }
            if (pc == GwesVaWinMainSkip || IsSlottedVa(pc, GwesVaWinMainSkip))
            {
                NoteGwesPc(pc, "WinMain-skip", GwesRomWinMain + (GwesVaWinMainSkip - GwesVaWinMain), bus);
                return;
            }
            if (pc == CoredllThreadExceptionExit && _gwesWatch && IsGwesThread(registers, bus))
            {
                LogThreadExceptionExit(pc, registers, bus);
                return;
            }
            if (pc == CoredllCreateThread && _gwesWatch)
            {
                LogGwesCreateThread(pc, registers, bus);
                return;
            }
            if (pc == ExceptionWorker)
            {
                _gwesSawWorker = true;
                if (_logged.Add("hive:worker"))
                    System.Console.WriteLine("[Hive] exception-worker pc=0x" +
                        pc.ToString("X8") + " a0=0x" +
                        (registers != null && registers.Length > 4
                            ? registers[4].ToString("X8") : "0") +
                        " (ThreadExceptionExit CreateThread start)");
                return;
            }
            if (pc == CoredllIsApiReady && _gwesSawThrEx)
            {
                uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
                if (_logged.Add("hive:isapi:" + a0.ToString("X")))
                    System.Console.WriteLine("[Hive] IsAPIReady a0=" + a0 +
                        " pc=0x" + pc.ToString("X8") +
                        " (worker uses 17 before MessageBoxW)");
                return;
            }
            if (pc == GwesRomDisplayFn || pc == GwesVaDisplayFn || IsSlottedVa(pc, GwesVaDisplayFn))
            {
                NoteGwesPc(pc, "DisplayFn", GwesRomDisplayFn, bus);
                return;
            }
            if (pc == GwesRomDisplayDll || pc == GwesVaDisplayDll || IsSlottedVa(pc, GwesVaDisplayDll))
            {
                NoteGwesPc(pc, "DisplayDll", GwesRomDisplayDll, bus);
                return;
            }
            if (pc == GwesRomSignal || pc == GwesRomGweApi)
            {
                NoteGwesPc(pc, pc == GwesRomSignal ? "SignalStarted-ROM" : "GweApi-ROM",
                    pc, bus);
                return;
            }
            if (pc == DdiNopEntry || (pc >= DdiNopVbase && pc < DdiNopVend))
            {
                _gwesSawDdi = true;
                if (_logged.Add("hive:ddi:" + (pc == DdiNopEntry ? "entry" : "run")))
                    System.Console.WriteLine("[Hive] ddi_nop pc=0x" + pc.ToString("X8") +
                        (pc == DdiNopEntry ? " entry" : ""));
                return;
            }
            if (pc == CoredllActivateDevice || pc == CoredllActivateDeviceEx)
            {
                string n = registers != null && registers.Length > 4 && bus != null
                    ? ReadUtf16(bus, registers[4]) : "";
                if (string.IsNullOrEmpty(n))
                    n = "(null)";
                if (_logged.Add("hive:act:" + n))
                    System.Console.WriteLine("[Hive] ActivateDevice \"" + n + "\" pc=0x" +
                        pc.ToString("X8"));
                return;
            }
            if (pc == CoredllLoadDriverRet && _logged.Contains("hive:ll:ddi_nop.dll"))
            {
                if (_logged.Add("hive:ldret"))
                    System.Console.WriteLine("[Hive] LoadDriver ret v0=0x" +
                        (registers != null && registers.Length > 2
                            ? registers[2].ToString("X8") : "0") +
                        " last-error=" + ReadLastError(bus) +
                        " ddi_nop@0x03998014 " +
                        (DdiNopMapped(bus) ? "mapped" : "unmapped"));
                return;
            }
            if (pc == CeRomTocFiles.LoadE32Rom
                && registers != null && registers.Length > 4
                && _logged.Contains("hive:ll:ddi_nop.dll")
                && CeRomTocFiles.IsDdiNopTocObject(bus, registers[4]))
            {
                if (_logged.Add("hive:ldde32"))
                {
                    CeRomTocFiles.TryMarkExtraRomO32Compressed(bus, CeRomTocFiles.DdiNopTocEntry);
                    System.Console.WriteLine("[Hive] 0x800196E4 ExtraROM ddi_nop obj=0x" +
                        registers[4].ToString("X8") +
                        " entry=0x" + CeRomTocFiles.DdiNopTocEntry.ToString("X8") +
                        " (firmware decompress/map; do not invent 0x81360000)");
                }
                return;
            }
            if (pc == CeRomTocFiles.LoadE32RomRet
                && _logged.Contains("hive:ldde32")
                && _logged.Add("hive:ldde32ret"))
            {
                System.Console.WriteLine("[Hive] 0x800196E4 ret v0=0x" +
                    (registers != null && registers.Length > 2
                        ? registers[2].ToString("X8") : "0") +
                    " ddi_nop@0x03998014 " +
                    (DdiNopMapped(bus) ? "mapped" : "unmapped"));
                return;
            }
            if (pc == CeRomTocFiles.LoadO32RomRet
                && _logged.Contains("hive:ldde32")
                && _logged.Add("hive:ldo32ret"))
            {
                System.Console.WriteLine("[Hive] 0x800165DC ret v0=0x" +
                    (registers != null && registers.Length > 2
                        ? registers[2].ToString("X8") : "0") +
                    " ddi_nop@0x03998014 " +
                    (DdiNopMapped(bus) ? "mapped" : "unmapped"));
                return;
            }
            if (pc == CeRomTocFiles.CopyO32Rom
                && _logged.Contains("hive:ldde32"))
            {
                CeRomTocFiles.TryMarkExtraRomO32Compressed(bus, CeRomTocFiles.DdiNopTocEntry);
                if (_logged.Add("hive:copyo32"))
                    System.Console.WriteLine("[Hive] 0x8001AFA4 CopyO32 ExtraROM ddi_nop" +
                        " (firmware MapO32; do not XIP-alias 0x80764CE0)");
                return;
            }
            if (pc == CeRomTocFiles.MapO32Rom
                && _logged.Contains("hive:ldde32")
                && registers != null && registers.Length > 5)
            {
                CeRomTocFiles.TrySteerExtraRomMapO32(bus, registers[5]);
                LogMapO32(registers, bus);
                return;
            }
            if (pc == CeRomTocFiles.MapO32Decompress
                && _logged.Contains("hive:ldde32")
                && registers != null && registers.Length > 4)
            {
                uint dest = registers[4];
                uint src = registers.Length > 5 ? registers[5] : 0;
                if (_logged.Add("hive:decomp:" + dest.ToString("X")))
                {
                    bool destOk = DestMapped(bus, dest);
                    System.Console.WriteLine("[Hive] 0x80028844 decompress dest=0x" +
                        dest.ToString("X8") +
                        " src=0x" + src.ToString("X8") +
                        " a2=0x" + (registers.Length > 6
                            ? registers[6].ToString("X8") : "0") +
                        " a3=0x" + (registers.Length > 7
                            ? registers[7].ToString("X8") : "0") +
                        " dest-" + (destOk ? "mapped" : "unmapped") +
                        " (firmware; do not host-alias XIP)");
                }
                return;
            }
            if (pc == CeRomTocFiles.MapO32VirtualCopy
                && _logged.Contains("hive:ldde32")
                && _logged.Add("hive:vcopy"))
            {
                System.Console.WriteLine("[Hive] 0x80043298 VirtualCopy a0=0x" +
                    (registers != null && registers.Length > 4
                        ? registers[4].ToString("X8") : "0") +
                    " a1=0x" + (registers != null && registers.Length > 5
                        ? registers[5].ToString("X8") : "0") +
                    " a2=0x" + (registers != null && registers.Length > 6
                        ? registers[6].ToString("X8") : "0") +
                    " (XIP path; ExtraROM o32 should decompress instead)");
                return;
            }
            if (pc == CeRomTocFiles.LoadLibSyscallRet
                && _logged.Contains("hive:ll:ddi_nop.dll")
                && _logged.Add("hive:ldsys"))
            {
                System.Console.WriteLine("[Hive] LoadLibraryExW syscall ret v0=0x" +
                    (registers != null && registers.Length > 2
                        ? registers[2].ToString("X8") : "0") +
                    " last-error=" + ReadLastError(bus) +
                    " ddi_nop@0x03998014 " +
                    (DdiNopMapped(bus) ? "mapped" : "unmapped"));
                return;
            }
            if (pc == CoredllLoadLibraryW || pc == CoredllLoadLibraryExW
                || pc == CoredllLoadDriver)
            {
                string n = registers != null && registers.Length > 4 && bus != null
                    ? ReadUtf16(bus, registers[4]) : "";
                if (string.IsNullOrEmpty(n))
                    return;
                bool after = _logged.Contains("hive:gpc:WinMain");
                bool ddi = n.IndexOf("ddi", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("display", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("gwes", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("mon", StringComparison.OrdinalIgnoreCase) >= 0;
                if ((after || ddi) && _logged.Add("hive:ll:" + n))
                    System.Console.WriteLine("[Hive] " +
                        (pc == CoredllLoadDriver ? "LoadDriver" : "LoadLibrary") +
                        " \"" + n + "\" pc=0x" + pc.ToString("X8"));
                return;
            }
            if ((pc == GwesVaAvHelper || IsSlottedVa(pc, GwesVaAvHelper)
                || pc == GwesVaAvCaller || IsSlottedVa(pc, GwesVaAvCaller))
                && _gwesWatch)
            {
                LogGwesAvSite(pc, registers, bus);
                return;
            }
            if (pc == CoredllMessageBoxW && _gwesSawThrEx)
            {
                if (_logged.Add("hive:msgbox"))
                    System.Console.WriteLine("[Hive] MessageBoxW pc=0x" + pc.ToString("X8") +
                        " (exception worker; needs gwes)");
                return;
            }
            if (pc == CoredllExitThread && _gwesWatch && (_gwesIn || _gwesLastPc != 0))
            {
                _gwesSawExit = true;
                if (_logged.Add("hive:exit"))
                    System.Console.WriteLine("[Hive] ExitThread pc=0x" + pc.ToString("X8") +
                        " last-gwes=0x" + _gwesLastPc.ToString("X8"));
                return;
            }
            if ((pc == CoredllWaitSo || pc == CoredllWaitMo) && _gwesWatch
                && IsGwesThread(registers, bus))
            {
                LogGwesWait(pc, registers, bus);
                return;
            }
            if (pc >= GwesRomText && pc < GwesRomTextEnd)
            {
                _gwesIn = true;
                _gwesLastPc = pc;
                if (_logged.Add("hive:gwesrun"))
                    System.Console.WriteLine("[Hive] gwes first-ROM pc=0x" + pc.ToString("X8"));
                return;
            }
            if (IsSlottedGwesText(pc) || IsGwesUsegPc(pc, bus))
            {
                _gwesIn = true;
                _gwesLastPc = pc;
                if (IsSlottedGwesText(pc) && _logged.Add("hive:gwesslot"))
                    System.Console.WriteLine("[Hive] gwes first-slot pc=0x" + pc.ToString("X8"));
                else if (IsGwesUsegPc(pc, bus) && _logged.Add("hive:gwesva"))
                    System.Console.WriteLine("[Hive] gwes first-VA pc=0x" + pc.ToString("X8"));
                return;
            }
            if (_gwesWatch)
            {
                uint slot = pc >> 25;
                if (slot >= 1 && slot <= 16 && _logged.Add("hive:userslot"))
                {
                    uint off = pc & CeSlotMask;
                    System.Console.WriteLine("[Hive] first user-slot pc=0x" + pc.ToString("X8") +
                        " slot=" + slot +
                        (off >= 0x00011000 && off < 0x000BB000
                            ? " (gwes .text range)"
                            : " (not gwes .text; coredll shared is 0x03F5xxxx)"));
                }
            }
            // OEMIdle is hit during CreateProcess; only summarize
            // after RunApps is already stuck on Depend30.
            if (_gwesWatch && _logged.Contains("hive:dep:RunOnce.exe:1E")
                && (pc == OemIdle || pc == OemIdleLoop))
                LogGwesSummary(pc);
        }

        private static bool IsSlottedVa(uint pc, uint va)
        {
            uint slot = pc >> 25;
            return slot >= 1 && slot <= 16 && (pc & CeSlotMask) == va;
        }

        private static bool IsSlottedGwesText(uint pc)
        {
            uint slot = pc >> 25;
            if (slot < 1 || slot > 16)
                return false;
            uint off = pc & CeSlotMask;
            return off >= 0x00011000 && off < 0x000BB000;
        }

        private static bool IsGwesUsegPc(uint pc, MipsBus bus)
        {
            if (!_gwesWatch || bus == null || pc < 0x00011000 || pc >= 0x000BB000)
                return false;
            try
            {
                uint got = bus.Read32(pc);
                uint gwes = bus.Read32(GwesRomText + (pc - 0x00011000));
                uint filesys = bus.Read32(FilesysRomText + (pc - 0x00011000));
                return got != 0 && got == gwes && got != filesys;
            }
            catch
            {
                return false;
            }
        }

        private static void LogGwesIat(MipsBus bus)
        {
            if (bus == null || !_logged.Add("hive:iat"))
                return;
            uint[] addrs =
            {
                GwesIatGetProc, GwesIatLoadLib, 0x000B607C, GwesIatHeapCreate,
                0x000B7A1C, GwesSlot | GwesIatGetProc, GwesSlot | 0x000B607C
            };
            for (int i = 0; i < addrs.Length; i++)
            {
                uint a = addrs[i];
                try
                {
                    uint w = bus.Read32(a);
                    System.Console.WriteLine("[Hive] gwes data 0x" + a.ToString("X8") +
                        " =0x" + w.ToString("X8"));
                }
                catch
                {
                    System.Console.WriteLine("[Hive] gwes data 0x" + a.ToString("X8") +
                        " unmapped");
                }
            }
        }

        private static void LogGwesInitFlag(MipsBus bus)
        {
            if (bus == null || !_logged.Add("hive:initflag"))
                return;
            try
            {
                uint word = bus.Read32(GwesInitFlag & ~3u);
                uint b = ((GwesInitFlag & 3) == 0)
                    ? (word & 0xFF)
                    : ((word >> (8 * (int)(GwesInitFlag & 3))) & 0xFF);
                System.Console.WriteLine("[Hive] WinMain already-init *0x000B7A1D=" +
                    b + " (nonzero skips to epilogue, no DisplayDll)");
            }
            catch
            {
                System.Console.WriteLine("[Hive] WinMain already-init *0x000B7A1D unmapped");
            }
        }

        private static bool IsGwesThread(uint[] registers, MipsBus bus)
        {
            uint sp = registers != null && registers.Length > 29 ? registers[29] : 0;
            if ((sp & 0xFE000000u) == GwesSlot)
                return true;
            if (_gwesThr == 0 || bus == null)
                return false;
            try
            {
                uint thr = bus.Read32(ThreadPtr);
                return thr == _gwesThr;
            }
            catch
            {
                return false;
            }
        }

        // 0x0005BCF8 jal 0x0005377C; delay lw a0, 0xC8(fp).
        // Helper is lhu 8(a0). a0==0 is the C0000005.
        private static void LogGwesAvSite(uint pc, uint[] registers, MipsBus bus)
        {
            uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
            string key = "hive:av:" + (pc & CeSlotMask).ToString("X") + ":" + a0.ToString("X");
            if (!_logged.Add(key))
                return;
            System.Console.WriteLine("[Hive] gwes AV-site pc=0x" + pc.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " (lhu 8(a0) / *(gdi+0xC8))");
            LogGwesDispObj(bus, "AV-site");
        }

        private static void LogGwesDispObj(MipsBus bus, string when)
        {
            if (bus == null || !_logged.Add("hive:dispobj:" + when))
                return;
            try
            {
                uint obj = bus.Read32(GwesDispObj);
                uint field = 0;
                bool have = false;
                if (obj != 0 && obj != 0xDEADBEEFu)
                {
                    try
                    {
                        field = bus.Read32(obj + 0xC8);
                        have = true;
                    }
                    catch
                    {
                    }
                }
                System.Console.WriteLine("[Hive] gwes *0x000BA954=0x" + obj.ToString("X8") +
                    " +0xC8=" + (have ? "0x" + field.ToString("X8") : "unmapped") +
                    " (" + when + ")");
            }
            catch
            {
                System.Console.WriteLine("[Hive] gwes *0x000BA954 unmapped (" + when + ")");
            }
        }

        // Observe only. Handle is the CreateThread object;
        // the worker's ExitThread signals it. Do not SetEvent.
        private static void LogThreadExceptionExit(uint pc, uint[] registers, MipsBus bus)
        {
            _gwesSawThrEx = true;
            if (!_logged.Add("hive:threx"))
                return;
            uint ra = registers != null && registers.Length > 31 ? registers[31] : 0;
            uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
            uint a1 = registers != null && registers.Length > 5 ? registers[5] : 0;
            System.Console.WriteLine("[Hive] ThreadExceptionExit pc=0x" + pc.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " last-gwes=0x" + _gwesLastPc.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " (CreateThread+WFSO; do not SetEvent)");
        }

        private static void LogGwesCreateThread(uint pc, uint[] registers, MipsBus bus)
        {
            uint start = registers != null && registers.Length > 6 ? registers[6] : 0;
            bool gwes = IsGwesThread(registers, bus);
            bool worker = start == ExceptionWorker;
            if (!gwes && !worker && !_gwesSawThrEx)
                return;
            _gwesSawCreateThr = true;
            string key = "hive:ct:" + start.ToString("X");
            if (!_logged.Add(key))
                return;
            uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
            uint a1 = registers != null && registers.Length > 5 ? registers[5] : 0;
            uint a3 = registers != null && registers.Length > 7 ? registers[7] : 0;
            System.Console.WriteLine("[Hive] CreateThread pc=0x" + pc.ToString("X8") +
                " start=0x" + start.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " a3=0x" + a3.ToString("X8") +
                (worker ? " (ThreadExceptionExit worker)" : "") +
                " gwes-thr=" + gwes);
        }

        private static void LogMapO32(uint[] registers, MipsBus bus)
        {
            uint o32 = registers != null && registers.Length > 5 ? registers[5] : 0;
            uint dest = 0;
            uint flags = 0;
            uint dataptr = 0;
            uint vsize = 0;
            uint psize = 0;
            try
            {
                if (bus != null && o32 != 0)
                {
                    vsize = bus.Read32(o32);
                    dest = bus.Read32(o32 + 8);
                    flags = bus.Read32(o32 + 0x10);
                    psize = bus.Read32(o32 + 0x14);
                    dataptr = bus.Read32(o32 + 0x18);
                }
            }
            catch
            {
            }
            string key = "hive:mapo32:" + dest.ToString("X") + ":" + flags.ToString("X");
            if (!_logged.Add(key))
                return;
            System.Console.WriteLine("[Hive] 0x8001AC30 MapO32 dest=0x" + dest.ToString("X8") +
                " dataptr=0x" + dataptr.ToString("X8") +
                " flags=0x" + flags.ToString("X8") +
                " vsize=0x" + vsize.ToString("X") +
                " psize=0x" + psize.ToString("X") +
                " dest-" + (DestMapped(bus, dest) ? "mapped" : "unmapped") +
                " ddi_nop@0x03998014 " +
                (DdiNopMapped(bus) ? "mapped" : "unmapped"));
        }

        // Refills stay on 0x80000000. Only the general vector
        // after WinMain is the unhandled path into
        // ThreadExceptionExit. Do not SetEvent that handle.
        public static void NoteCpuException(uint code, uint epc, uint vaddr, uint vector)
        {
            bool loader = _logged.Contains("hive:ldde32")
                && ((epc >= 0x80016000u && epc < 0x8001C000u)
                    || (vaddr >= 0x03980000u && vaddr < 0x039B0000u)
                    || (vaddr >= 0x80764CE0u && vaddr < 0x80776000u)
                    || (vaddr >= 0x01F57000u && vaddr < 0x01F66000u));
            if (!_gwesWatch || !_logged.Contains("hive:gpc:WinMain"))
                return;
            // 0 is a timer interrupt. Those ate the cap and hid the AV.
            if (code == 0)
                return;
            if (vector != ExceptionVector && vector != 0xBFC00380u)
                return;
            if (!loader && _gwesExnLogged >= 8)
                return;
            string key = "hive:exn:" + epc.ToString("X") + ":" + code.ToString("X") + ":" + vaddr.ToString("X");
            if (!_logged.Add(key))
                return;
            _gwesExnLogged++;
            System.Console.WriteLine("[Hive] exception code=" + code +
                " epc=0x" + epc.ToString("X8") +
                " vaddr=0x" + vaddr.ToString("X8") +
                " vec=0x" + vector.ToString("X8") +
                " last-gwes=0x" + _gwesLastPc.ToString("X8"));
        }

        // Observe only. Do not SetEvent the waited handle.
        private static void LogGwesWait(uint pc, uint[] registers, MipsBus bus)
        {
            if (!IsGwesThread(registers, bus))
                return;
            _gwesSawWait = true;
            _gwesIn = false;
            uint ra = registers != null && registers.Length > 31 ? registers[31] : 0;
            uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
            uint a1 = registers != null && registers.Length > 5 ? registers[5] : 0;
            uint a2 = registers != null && registers.Length > 6 ? registers[6] : 0;
            uint a3 = registers != null && registers.Length > 7 ? registers[7] : 0;
            string kind = pc == CoredllWaitSo ? "WaitForSingleObject" : "WaitForMultipleObjects";
            string key = "hive:gwait:" + ra.ToString("X") + ":" + a0.ToString("X") + ":" + a1.ToString("X");
            if (!_logged.Add(key))
                return;
            System.Console.WriteLine("[Hive] gwes wait " + kind +
                " pc=0x" + pc.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " last-gwes=0x" + _gwesLastPc.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " a2=0x" + a2.ToString("X8") +
                " a3=0x" + a3.ToString("X8"));
            if (bus == null)
                return;
            try
            {
                if (pc == CoredllWaitSo)
                {
                    System.Console.WriteLine("[Hive] gwes wait handle=0x" + a0.ToString("X8") +
                        " timeout=0x" + a1.ToString("X8"));
                    if (ra >= CoredllThreadExceptionExit && ra < CoredllThreadExceptionExit + 0x1B0)
                        System.Console.WriteLine("[Hive] gwes wait is ThreadExceptionExit CreateThread handle (not an event; do not SetEvent)");
                }
                else
                {
                    uint n = a0;
                    if (n > 8)
                        n = 8;
                    for (uint i = 0; i < n && a1 != 0; i++)
                    {
                        uint h = bus.Read32(a1 + i * 4);
                        System.Console.WriteLine("[Hive] gwes wait handle[" + i + "]=0x" +
                            h.ToString("X8"));
                    }
                }
            }
            catch
            {
            }
        }

        private static void NoteGwesPc(uint pc, string what, uint rom, MipsBus bus)
        {
            uint got = 0;
            uint want = 0;
            try
            {
                if (bus != null)
                {
                    got = bus.Read32(pc);
                    want = bus.Read32(rom);
                }
            }
            catch
            {
            }
            if (want == 0 || got != want)
            {
                if (_logged.Add("hive:gpcmiss:" + what))
                    System.Console.WriteLine("[Hive] " + what + " pc=0x" + pc.ToString("X8") +
                        " word=0x" + got.ToString("X8") + " (not gwes 0x" + want.ToString("X8") + ")");
                return;
            }
            _gwesIn = true;
            _gwesLastPc = pc;
            if (what.IndexOf("Signal", StringComparison.Ordinal) >= 0
                || what.IndexOf("GweApi", StringComparison.Ordinal) >= 0)
                _gwesSawSignal = true;
            if (_logged.Add("hive:gpc:" + what))
                System.Console.WriteLine("[Hive] gwes " + what + " pc=0x" + pc.ToString("X8") +
                    " word=0x" + got.ToString("X8"));
        }

        private static bool DestMapped(MipsBus bus, uint dest)
        {
            if (bus == null || dest == 0)
                return false;
            try
            {
                bus.Read32(dest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DdiNopMapped(MipsBus bus)
        {
            if (bus == null)
                return false;
            try
            {
                uint w = bus.Read32(DdiNopEntry);
                return w != 0 && w != 0xDEADBEEFu;
            }
            catch
            {
                return false;
            }
        }

        private static void LogDdiNopMapped(MipsBus bus)
        {
            if (bus == null || !_logged.Add("hive:ddimap"))
                return;
            try
            {
                uint w = bus.Read32(DdiNopEntry);
                System.Console.WriteLine("[Hive] ddi_nop entry@0x03998014 word=0x" +
                    w.ToString("X8") + (w == 0 || w == 0xDEADBEEFu ? " (not mapped)" : ""));
            }
            catch
            {
                System.Console.WriteLine("[Hive] ddi_nop entry@0x03998014 unmapped");
            }
        }

        private static void LogGwesSummary(uint idlePc)
        {
            if (_gwesSummary)
                return;
            _gwesSummary = true;
            System.Console.WriteLine("[Hive] gwes summary idle=0x" + idlePc.ToString("X8") +
                " last=0x" + _gwesLastPc.ToString("X8") +
                " entry=" + _logged.Contains("hive:gpc:entry") +
                " WinMain=" + _logged.Contains("hive:gpc:WinMain") +
                " WinMain-jal=" + _logged.Contains("hive:gpc:WinMain-jal") +
                " WinMain-skip=" + _logged.Contains("hive:gpc:WinMain-skip") +
                " DisplayFn=" + _logged.Contains("hive:gpc:DisplayFn") +
                " DisplayDll=" + _logged.Contains("hive:gpc:DisplayDll") +
                " SignalStarted=" + _gwesSawSignal +
                " first-wait=" + _gwesSawWait +
                " ThreadExceptionExit=" + _gwesSawThrEx +
                " CreateThread=" + _gwesSawCreateThr +
                " exn-worker=" + _gwesSawWorker +
                " ddi_nop=" + _gwesSawDdi +
                " ExitThread=" + _gwesSawExit);
        }

        // gwes slotted PCs only. 0x0001634C is SignalStarted(_wtol).
        // 0x00016354 is OpenEvent(SYSTEM/GweApiSetReady) then
        // EventModify SET. There is no GRAPHICS event name.
        private static void LogGwesReadySite(uint pc, uint off, uint[] registers, MipsBus bus)
        {
            string key = "hive:gwes:" + off.ToString("X") + ":" + (pc & CeSlotBase).ToString("X");
            if (!_logged.Add(key))
                return;
            _gwesSawSignal = true;
            if (off == GwesSignalStarted)
            {
                uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
                System.Console.WriteLine("[Hive] gwes SignalStarted site pc=0x" +
                    pc.ToString("X8") + " a0=" + a0);
            }
            else
            {
                string name = "";
                if (registers != null && registers.Length > 6 && bus != null)
                    name = ReadUtf16(bus, registers[6]);
                System.Console.WriteLine("[Hive] gwes OpenEvent \"" +
                    (string.IsNullOrEmpty(name) ? "(null)" : name) +
                    "\" pc=0x" + pc.ToString("X8") +
                    " (SYSTEM/GweApiSetReady, not GRAPHICS)");
            }
        }

        private static void LogLaunchReadySlots(MipsBus bus, uint need)
        {
            if (bus == null || !_logged.Add("hive:slots:" + need))
                return;
            try
            {
                uint table = bus.Read32(LaunchTablePtr);
                uint count = bus.Read32(LaunchCountPtr);
                uint ev = bus.Read32(LaunchReadyEvent);
                System.Console.WriteLine("[Hive] ready-slot table=0x" + table.ToString("X8") +
                    " count=" + count + " event=0x" + ev.ToString("X8") +
                    " (WFMO waits this unnamed handle)");
                if (!LooksLikePtr(table) || count == 0 || count > 32)
                    return;
                for (uint i = 0; i < count; i++)
                {
                    uint rec = table + i * LaunchRecordSize;
                    uint id = bus.Read32(rec);
                    uint ready = bus.Read32(rec + 4);
                    string img = ReadUtf16(bus, rec + 72);
                    if (string.IsNullOrEmpty(img))
                        img = "?";
                    System.Console.WriteLine("[Hive] ready-slot Launch" + id +
                        " +4=" + ready + " \"" + img + "\"");
                }
                LogGwesMappedSlots(bus);
            }
            catch
            {
            }
        }

        // gwes image_base 0x00010000; SYSTEM/GweApiSetReady at +0x11020.
        // Also walk 4KB pages in low RAM — CreateProcess v0=1 does not
        // mean the PE was placed in a CE slot.
        private static void LogGwesMappedSlots(MipsBus bus)
        {
            if (bus == null)
                return;
            int found = 0;
            for (uint slot = 1; slot <= 16; slot++)
            {
                uint va32 = (slot * 0x02000000u) + 0x00011020u;
                if (LooksLikeGweApi(bus, va32))
                {
                    System.Console.WriteLine("[Hive] gwes mapped 32MB slot=" + slot +
                        " GweApi@0x" + va32.ToString("X8") +
                        " \"" + ReadUtf16(bus, va32) + "\"");
                    found++;
                }
                if (slot <= 8)
                {
                    uint va64 = (slot * 0x04000000u) + 0x00011020u;
                    if (va64 != va32 && LooksLikeGweApi(bus, va64))
                    {
                        System.Console.WriteLine("[Hive] gwes mapped 64MB slot=" + slot +
                            " GweApi@0x" + va64.ToString("X8") +
                            " \"" + ReadUtf16(bus, va64) + "\"");
                        found++;
                    }
                }
            }
            if (found == 0)
            {
                for (uint a = 0x00010000; a < 0x02000000; a += 0x1000)
                {
                    if (!LooksLikeGweApi(bus, a))
                        continue;
                    System.Console.WriteLine("[Hive] gwes GweApi@0x" + a.ToString("X8") +
                        " \"" + ReadUtf16(bus, a) + "\"");
                    found++;
                    break;
                }
            }
            if (found == 0)
                System.Console.WriteLine("[Hive] gwes SYSTEM/GweApiSetReady not in slots 1-16 or 0x00010000-0x02000000");
        }

        private static bool LooksLikeGweApi(MipsBus bus, uint va)
        {
            try
            {
                uint w0 = bus.Read32(va);
                if ((w0 & 0xFFFF) != 0x0053 || (w0 >> 16) != 0x0059)
                    return false;
                uint w1 = bus.Read32(va + 4);
                return (w1 & 0xFFFF) == 0x0053 && (w1 >> 16) == 0x0054;
            }
            catch
            {
                return false;
            }
        }

        private static void LogHiveCreateProcess(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 31 || bus == null)
                return;
            string img = ReadUtf16(bus, registers[4]);
            if (string.IsNullOrEmpty(img))
                return;
            _cprocName = img;
            _cprocRa = registers[31];
            if (_logged.Add("hive:cp:" + img))
            {
                string cmd = "";
                if (registers.Length > 5)
                    cmd = ReadUtf16(bus, registers[5]);
                System.Console.WriteLine("[Hive] CreateProcess \"" + img + "\"" +
                    (string.IsNullOrEmpty(cmd) ? "" : " cmd=\"" + cmd + "\""));
            }
        }

        private static void LogHiveCreateProcessRet(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 2)
                return;
            string img = _cprocName;
            uint v0 = registers[2];
            uint err = ReadLastError(bus);
            _cprocName = "";
            _cprocRa = 0;
            if (string.IsNullOrEmpty(img))
                img = "(null)";
            if (_logged.Add("hive:cpret:" + img))
                System.Console.WriteLine("[Hive] CreateProcess \"" + img +
                    "\" v0=0x" + v0.ToString("X8") +
                    " last-error=" + err);
            if (v0 != 0 && img.IndexOf("gwes", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _gwesWatch = true;
                System.Console.WriteLine("[Hive] gwes watch entry VA 0x000163C8 ROM 0x8014B3C8 " +
                    "WinMain 0x8014B014 Display=ddi_nop.dll (etc XIP vbase 0x03980000)");
                LogDdiNopMapped(bus);
            }
            if (v0 != 0)
                LogCprocThreadAtRet(bus, img);
            _cprocThread = 0;
        }

        private static void LogCprocThreadCtx(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 4 || bus == null)
                return;
            uint thr = registers[4];
            if (thr == 0)
                return;
            if (_cprocThread == 0)
                _cprocThread = thr;
            if (_gwesThr == 0 && !string.IsNullOrEmpty(_cprocName)
                && _cprocName.IndexOf("gwes", StringComparison.OrdinalIgnoreCase) >= 0)
                _gwesThr = thr;
            if (!_logged.Add("hive:thr:" + _cprocName + ":" + thr.ToString("X")))
                return;
            DumpThreadStart(bus, _cprocName, thr);
        }

        private static void LogCprocThreadAtRet(MipsBus bus, string img)
        {
            if (bus == null || _cprocThread == 0)
                return;
            if (!_logged.Add("hive:thrret:" + img))
                return;
            DumpThreadStart(bus, img + "-ret", _cprocThread);
        }

        private static void LogThreadTrampoline(uint[] registers, MipsBus bus)
        {
            uint procKey = 0;
            try
            {
                if (bus != null)
                    procKey = bus.Read32(CeRomTocFiles.CurProc);
            }
            catch
            {
            }
            if (!_logged.Add("hive:tramp:" + procKey.ToString("X")))
                return;
            uint a0 = registers != null && registers.Length > 4 ? registers[4] : 0;
            uint a1 = registers != null && registers.Length > 5 ? registers[5] : 0;
            uint proc = 0;
            uint startip = 0;
            try
            {
                proc = bus != null ? bus.Read32(CeRomTocFiles.CurProc) : 0;
                if (proc != 0 && proc != 0xDEADBEEFu)
                    startip = ReadModuleStartip(bus, proc);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] thread trampoline 0x8001FF38 a0=0x" +
                a0.ToString("X8") + " a1=0x" + a1.ToString("X8") +
                " CurProc=0x" + proc.ToString("X8") +
                " startip=0x" + startip.ToString("X8"));
        }

        private static void DumpThreadStart(MipsBus bus, string tag, uint thr)
        {
            try
            {
                uint ip = bus.Read32(thr + ThreadStartip);
                uint pc = bus.Read32(thr + ThreadCtxPc);
                uint sp = bus.Read32(thr + ThreadStack);
                uint proc = bus.Read32(thr + ThreadProc);
                uint startip = ReadModuleStartip(bus, proc);
                System.Console.WriteLine("[Hive] thread \"" + tag +
                    "\" thr=0x" + thr.ToString("X8") +
                    " +5C=0x" + ip.ToString("X8") +
                    " ctxPC=0x" + pc.ToString("X8") +
                    " sp=0x" + sp.ToString("X8") +
                    " proc=0x" + proc.ToString("X8") +
                    " startip=0x" + startip.ToString("X8"));
            }
            catch
            {
            }
        }

        private static void LogCallDllStartip(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 23 || bus == null)
                return;
            uint module = registers[23];
            if (module == 0)
                return;
            uint ip = 0;
            try
            {
                ip = bus.Read32(module + ThreadStartip);
            }
            catch
            {
                return;
            }
            if (!_logged.Add("hive:calldll:" + module.ToString("X") + ":" + ip.ToString("X")))
                return;
            System.Console.WriteLine("[Hive] CallDLL module=0x" + module.ToString("X8") +
                " startip=0x" + ip.ToString("X8"));
        }

        private static void LogXipExeCallDllSkip(uint[] registers, MipsBus bus)
        {
            if (registers == null || registers.Length <= 30 || bus == null)
                return;
            uint module = registers[30];
            if (module == 0)
                return;
            uint p50 = 0;
            uint ip = 0;
            try
            {
                p50 = bus.Read32(module + ProcModule);
                ip = bus.Read32(module + ThreadStartip);
            }
            catch
            {
                return;
            }
            if (!_logged.Add("hive:exeskip:" + module.ToString("X") + ":" + p50.ToString("X")))
                return;
            System.Console.WriteLine("[Hive] EXE CallDLL-skip module=0x" + module.ToString("X8") +
                " +50=0x" + p50.ToString("X8") +
                " +5C=0x" + ip.ToString("X8"));
        }

        private static void LogLoadExeStartip(MipsBus bus)
        {
            uint proc = 0;
            uint startip = 0;
            try
            {
                if (bus != null)
                    proc = bus.Read32(CeRomTocFiles.CurProc);
                startip = ReadModuleStartip(bus, proc);
            }
            catch
            {
            }
            if (!_logged.Add("hive:ldxe:" + proc.ToString("X") + ":" + startip.ToString("X")))
                return;
            System.Console.WriteLine("[Hive] load-exe CurProc=0x" + proc.ToString("X8") +
                " startip=0x" + startip.ToString("X8"));
        }

        private static uint ReadModuleStartip(MipsBus bus, uint proc)
        {
            if (bus == null || proc == 0 || proc == 0xDEADBEEFu)
                return 0;
            try
            {
                uint ip = bus.Read32(proc + ThreadStartip);
                if (ip != 0)
                    return ip;
                uint mod = bus.Read32(proc + ProcModule);
                if (mod != 0 && mod != 0xDEADBEEFu)
                    return bus.Read32(mod + ThreadStartip);
            }
            catch
            {
            }
            return 0;
        }

        private static uint ReadLastError(MipsBus bus)
        {
            if (bus == null)
                return 0xFFFFFFFF;
            try
            {
                uint thr = bus.Read32(ThreadPtr);
                if (thr != 0 && thr != 0xDEADBEEFu)
                    return bus.Read32(thr + ThreadLastErr);
            }
            catch
            {
            }
            return 0xFFFFFFFF;
        }

        // Observe only after compact. Do not rewrite +14/+18.
        private static void LogInheritList(MipsBus bus, uint list)
        {
            if (_inheritListLogged || bus == null || list == 0)
                return;
            _inheritListLogged = true;
            try
            {
                uint count = bus.Read32(list + 8);
                System.Console.WriteLine("[Inherit] LIST @0x" + list.ToString("X8") + " count=" + count);
                if (count > 8)
                    count = 8;
                for (uint i = 0; i < count; i++)
                {
                    uint pair = list + 12 + i * 8;
                    uint start = bus.Read32(pair);
                    uint end = bus.Read32(pair + 4);
                    System.Console.WriteLine("[Inherit] pair" + i +
                        " start=0x" + start.ToString("X8") +
                        " end=0x" + end.ToString("X8"));
                }
            }
            catch
            {
            }
        }

        private const string LastUsedName = "last_dump_root.txt";
        private const int HuntMaxDepth = 3;
        private const int HuntMaxVisit = 400;

        private static readonly HashSet<string> VolumeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "etc.bin", "BOOT.PRF", "tv2clientce", "tv2clientce.exe", "Application"
        };

        private static readonly HashSet<string> HuntNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nk.bin", "etc.bin", "sec.bin", "XASEC.BIN",
            "BOOT.PRF", "BOOTPRF.BAK",
            "tv2clientce", "tv2clientce.exe",
            "tv2clientcorece.dll", "tv2engine.dll", "iptvdriver.dll",
            "default.hv", "hashes.bin", "gwes.exe",
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
                if (LooksLikeVolume(feed))
                {
                    NoteDumpImages(feed);
                    return feed;
                }
                System.Console.WriteLine("[HardDisk] hunt feed=" + feed);
                string attach = HuntAttach(feed);
                if (!string.IsNullOrEmpty(attach))
                    return attach;
            }
            return "";
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

        // etc.bin by name (HD file + ExtraROM candidate). Other
        // B000FF only when they sit next to nk.bin. sec.bin /
        // raven_fw.bin / BOOT.PRF stay hunt names for FAT, not XIP.
        private static void NoteDumpImages(string dir)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return;
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    string name = Path.GetFileName(f);
                    if (name.Equals("nk.bin", StringComparison.OrdinalIgnoreCase))
                    {
                        _nkDir = dir;
                        continue;
                    }
                    if (name.Equals("etc.bin", StringComparison.OrdinalIgnoreCase) || PeekB000Ff(f))
                        AddExtraRom(f);
                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string name = Path.GetFileName(d);
                    if (name.Equals("etc.bin", StringComparison.OrdinalIgnoreCase))
                        NoteExtractedExtraRom(d);
                }
            }
            catch
            {
            }
        }

        // Extracted ExtraROM tree (Dumps\etc.bin\). Not a B000FF.
        // Firmware sees those XIP files after the raw etc.bin map.
        private static void NoteExtractedExtraRom(string dir)
        {
            if (string.IsNullOrEmpty(dir) || _extractLogged || !Directory.Exists(dir))
                return;
            try
            {
                string marker = Path.Combine(dir, "tv2clientce.exe");
                if (!File.Exists(marker))
                    marker = Path.Combine(dir, "tv2clientcorece.dll");
                if (!File.Exists(marker))
                    return;
                _extractLogged = true;
                System.Console.WriteLine("[HardDisk] ExtraROM extract dir=" + dir +
                    " (not B000FF; firmware sees XIP after map at imageStart)");
                int n = 0;
                foreach (string f in Directory.GetFiles(dir))
                {
                    string name = Path.GetFileName(f);
                    if (n < 16)
                        System.Console.WriteLine("[HardDisk] ExtraROM extract file " + name +
                            " " + new FileInfo(f).Length);
                    n++;
                }
                if (n > 16)
                    System.Console.WriteLine("[HardDisk] ExtraROM extract files=" + n);
            }
            catch
            {
            }
        }

        private static void AddExtraRom(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            try { path = Path.GetFullPath(path); }
            catch { return; }
            foreach (string existing in _extraRoms)
            {
                if (existing.Equals(path, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            _extraRoms.Add(path);
        }

        private static bool PeekB000Ff(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 15)
                        return false;
                    byte[] h = new byte[7];
                    return fs.Read(h, 0, 7) == 7
                        && h[0] == (byte)'B'
                        && h[1] == (byte)'0'
                        && h[2] == (byte)'0'
                        && h[3] == (byte)'0'
                        && h[4] == (byte)'F'
                        && h[5] == (byte)'F'
                        && h[6] == (byte)'\n';
                }
            }
            catch
            {
                return false;
            }
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
            {
                NoteDumpImages(bestVol);
                if (!string.IsNullOrEmpty(_nkDir))
                    NoteDumpImages(_nkDir);
                return bestVol;
            }
            if (!string.IsNullOrEmpty(bestLoose))
            {
                NoteDumpImages(bestLoose);
                if (!string.IsNullOrEmpty(_nkDir))
                    NoteDumpImages(_nkDir);
                return bestLoose;
            }
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
                if (name.Equals("nk.bin", StringComparison.OrdinalIgnoreCase) && !isDir)
                    _nkDir = Path.GetDirectoryName(p) ?? "";
                if (name.Equals("etc.bin", StringComparison.OrdinalIgnoreCase))
                {
                    if (isDir)
                        NoteExtractedExtraRom(p);
                    else
                        AddExtraRom(p);
                }
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
            // Local/dev defaults only. Missing paths are skipped.
            // Shipped attach is the user feed + name hunt above.
            yield return "/workspace/UverseDriveE";
            yield return @"E:\EVO backup 2026 august 26\DVR Stuff\UVERSE STUFF\Uverse Drive E";
            yield return @"E:\EVO backup 2026 august 26\DVR Stuff\UVERSE STUFF\Dumps";
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
