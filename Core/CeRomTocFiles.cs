using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // 0x8001D3A0 CreateFile has no TOC fallback: INVALID_HANDLE returns 2.
    // LoadLibraryExW and CreateProcess already map TOC modules on that miss.
    // device.exe CreateFile of DEVMGR.dll does not. Helper 0 with type 2
    // left object+0 as INVALID_HANDLE, and 0x80016584/0x8003DFD8 then
    // failed that handle (ERROR_BAD_EXE_FORMAT). 0x80016AFC already
    // attaches a TOC module as object+0=TOCentry, object+4=7 so
    // 0x800196E4 uses e32 at TOC+0x14. DEVMGR, TOC[31]
    // iptvcryptohal.dll, TOC[20] ceddk.dll (packed at
    // 0x8024B000), and TOC[26] sigcheckfilter.dll (0x4600 at
    // 0x8027C000) get that attach. Image bytes stay in RAM.
    public static class CeRomTocFiles
    {
        public const uint CreateFileFail = 0x8001D400;
        public const uint NameCopyContinue = 0x8001D464;
        // 0x80016AFC walks *(0x80342B10) ROMHDR nodes. ExtraROM
        // 0x8134DA84 is mapped but never linked, so LoadDriver of
        // bare ddi_nop.dll misses (v0=2) and never CreateFile
        // (OpenExe 0x8001D6F0 stores 24($sp)=0 when the name has
        // no \ or /). Same hit layout as NK TOC: object+0=entry,
        // +4=7, v0=0. 0x800196E4 then uses e32 at TOC+0x14.
        public const uint TocWalkMiss = 0x80016B74;
        public const uint TocWalkMissContinue = 0x80016B78;
        public const uint BindImpMiss = 0x80018F9C;
        public const uint BindImpWalk = 0x80018F3C;
        // 0x80018B34 CallDLLEntry jalrs module+0x5C with no
        // null check. TOC-attach writes object+0/4 so 0x800196E4
        // can read e32, but 0x8001E960 skips the startip store
        // when 32($sp) entryrva is still 0. jalr 0 never returns.
        // DLL vbase is unique: store vbase+entryrva. EXE vbase
        // 0x00010000 is shared (filesys/gwes/device) and the
        // image is linked there: jal/j stay in region 0. A ROM
        // startip (0x8014B3C8) makes entry's jal go to
        // 0x80016014, not WinMain. 0x800140A8 (ASID/slot
        // attach after VALLOC) is jr $ra, so slot 0 still
        // fetches filesys. Alias current-process uncompressed
        // XIP o32[0] VA to dataptr, and store startip as VA.
        // 0x8001DD6C skips CallDLL when module+0x50 is useg
        // or 0xC2xxxxxx; that skip never jalrs EXE entry.
        public const uint CallDllStartip = 0x80018BAC;
        public const uint XipExeCallDllSkip = 0x8001DDA4;
        public const uint XipExeCallDllJal = 0x8001DD90;
        public const uint ThreadStartTrampoline = 0x8001FF38;
        public const uint LoadExeE32Ret = 0x8001F870;
        public const uint ThreadContextSetup = 0x80020BE4;
        public const uint ExeVbase = 0x00010000;
        public const uint ProcModule = 0x50;
        public const uint ProcSlot = 0x0C;
        public const uint ProcTable = 0x80340040;
        public const uint ProcSize = 0xD0;
        public const uint ThreadPtr = 0xFFFFDAC0;
        public const uint ThreadStack = 0x24;
        public const uint O32Compressed = 0x4000;
        // 0x8001F12C andi s4, 0x8000 / beq skip CallDLL a1=1.
        // User-mode LoadLibrary keeps s4=0 (same for CEDDK/HAL/
        // filter). coredll 0x03F73050 then walks 3 new modules
        // with reason 1: CEDDK, HAL, filter. HAL DllMain never
        // returns, so the outer filter never gets a1=1 and
        // LoadLibrary never returns to FSDMGR 0x03E8604C.
        // Only the filter startip takes the firmware CallDLL
        // path; CEDDK/HAL already get a1=1 from that walk.
        //
        // HAL DllMain then CreateFileW(L"BTV1:"). Win32
        // 0x8003D700 calls filesys 0x00019CB8 on the same
        // stack (sp 0x040CE438). That prologue needs 1784
        // bytes and stores into 0x040CD000; the thread dies
        // at 0x80000180. No BTV1 device is in this image.
        // INVALID_HANDLE is the honest miss. Both HAL
        // CreateFile paths return 1, so LoadLibrary can
        // still reach FSDMGR 0x03E8604C.
        public const uint ProcessAttachGate = 0x8001F12C;
        public const uint Win32CreateFile = 0x8003D700;
        public const uint FilterStartip = 0x03DF4BDC;
        public const uint CallDllFlag = 0x8000;
        public const uint ModuleStartip = 0x5C;
        public const uint ModuleFileObj = 96;
        public const uint CurProc = 0xFFFFDAC4;
        public const uint EcecTocPtr = 0x80010044;
        public const uint RomHdrNumMods = 0x10;
        public const uint TocFirst = 0x54;
        public const uint TocEntrySize = 32;
        public const byte TocAttachType = 7;
        public const uint O32RomSize = 0x18;
        public const uint O32LiteSize = 0x1C;
        // coredll 0x03F7A960 bne v0,0 / delay sw v0, (0x01FFFFA0).
        // HeapCreate(0,0,0) returned 0 in device.exe and the delay
        // slot wrote that 0 over the heap filesys already stored.
        // 0x01FFF000 is one physical page here, so that wipe makes
        // LocalAlloc call HeapAlloc(0) and RegOpen returns 14.
        public const uint HeapCreateStore = 0x03F7A964;
        public const uint ProcessHeapPtr = 0x01FFFFA0;
        // FSDMGR 0x03E896D8 is GetProcAddress. After TOC-attach,
        // 0x800196E4 copies e32_rom units to e32_lite+0x1C.
        // Kernel GPA reads EXP at +0x20 (that dword is the
        // size 0x303) so HookVolume / CreateFileW miss.
        // FindFSD then prefixes sigcheckfilter_ and never
        // stores HookVolume at FSD+24. 0x03E82654 jalrs
        // fatfsd *(vtable+16) because LoadFilters left
        // volume+8 on the original FSD. That jalr is the
        // MountDisk slot. Serve TOC exports (bare or with
        // FSD_ stripped) so FindFSD can attach the filter.
        public const uint FsGetProc = 0x03E896D8;
        public const uint FilterVbase = 0x03DF0000;
        public const uint E32RomExpRva = 0x24;
        public const uint ExtraRomCece = 0x43454345;
        public const uint DdiNopVbase = 0x03980000;
        private static uint _extraRomStart;
        private static uint _extraRomHdr;
        private static uint _ddiNopTocEntry;
        private static uint _ddiNopAttr;

        public static bool TryContinueRomModule(MipsBus bus, uint path, out uint attr, out uint tocEntry)
        {
            attr = 0;
            tocEntry = 0;
            if (bus == null || path == 0)
                return false;

            string baseName = Basename(bus, path);
            if (string.IsNullOrEmpty(baseName))
                return false;
            // LoadLibraryExW and CreateProcess already map TOC modules when
            // this helper returns 2. DEVMGR CreateFile treats that miss as
            // fatal. Filter LoadLibrary of sigcheckfilter.dll is the same
            // miss: without this attach the entry ran a1=3 (not
            // PROCESS_ATTACH) and FSDMGR never HookVolume. TOC[26] is
            // already in this image (not FILESentry).
            if (!NamesEqual(baseName, "devmgr.dll")
                && !NamesEqual(baseName, "iptvcryptohal.dll")
                && !NamesEqual(baseName, "ceddk.dll")
                && !NamesEqual(baseName, "sigcheckfilter.dll")
                && !NamesEqual(baseName, "ddi_nop.dll"))
                return false;

            if (TryFindTocModule(bus, 0, 64, baseName, out tocEntry, out attr))
                return true;
            // ExtraROM TOC[33] ddi_nop.dll. LoadDriver of it is
            // proven; NK TOC does not list it. Do not invent
            // 0x81360000. Do not map until firmware asks.
            if (NamesEqual(baseName, "ddi_nop.dll")
                && TryFindTocModule(bus, ExtraRomToc(bus), 128, baseName, out tocEntry, out attr))
            {
                System.Console.WriteLine("[Hive] TOC-attach ExtraROM ddi_nop.dll entry=0x" +
                    tocEntry.ToString("X8") + " (CreateFile miss; do not invent 0x81360000)");
                return true;
            }
            return false;
        }

        // LoadDriver does not CreateFile. OpenExe 0x8001D6F0 calls
        // this walk at 0x8001DA58 for a bare name. NK modules hit
        // because they sit on *(0x80342B10). ExtraROM TOC[33] does
        // not. Write the same object the hit path at 0x80016B9C
        // writes and return 0 so 0x800196E4 can decompress/map.
        public static bool TryAttachExtraRomTocWalk(MipsBus bus, uint path, uint obj)
        {
            if (bus == null || path == 0 || obj == 0)
                return false;
            string baseName = Basename(bus, path);
            if (!NamesEqual(baseName, "ddi_nop.dll"))
                return false;
            uint tocEntry = _ddiNopTocEntry;
            if (tocEntry == 0
                && !TryFindTocModule(bus, ExtraRomToc(bus), 128, baseName, out tocEntry, out _))
            {
                uint toc = ExtraRomToc(bus);
                uint nmods = 0;
                try
                {
                    if (toc != 0)
                        nmods = bus.Read32(toc + RomHdrNumMods);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] TOC-walk ExtraROM ddi_nop.dll miss toc=0x" +
                    toc.ToString("X8") + " nmods=" + nmods +
                    " cached-hdr=0x" + _extraRomHdr.ToString("X8") +
                    " (do not invent 0x81360000)");
                return false;
            }
            try
            {
                bus.Write32(obj, tocEntry);
                bus.Write8(obj + 4, TocAttachType);
            }
            catch
            {
                return false;
            }
            System.Console.WriteLine("[Hive] TOC-walk ExtraROM ddi_nop.dll entry=0x" +
                tocEntry.ToString("X8") + " (LoadDriver; do not invent 0x81360000)");
            return true;
        }

        public static void NoteExtraRom(uint imageStart)
        {
            _extraRomStart = imageStart;
            _extraRomHdr = 0;
            _ddiNopTocEntry = 0;
            _ddiNopAttr = 0;
        }

        public static void NoteExtraRomModule(uint romhdr, uint tocEntry, uint attr)
        {
            if (romhdr != 0)
                _extraRomHdr = romhdr;
            if (tocEntry != 0)
            {
                _ddiNopTocEntry = tocEntry;
                _ddiNopAttr = attr;
            }
        }

        private static bool TryFindTocModule(MipsBus bus, uint tocOrZero, uint maxMods,
            string baseName, out uint tocEntry, out uint attr)
        {
            tocEntry = 0;
            attr = 0;
            if (bus == null || string.IsNullOrEmpty(baseName))
                return false;
            try
            {
                uint toc = tocOrZero;
                if (toc == 0)
                    toc = bus.Read32(EcecTocPtr);
                if (toc == 0)
                    return false;
                uint nmods = bus.Read32(toc + RomHdrNumMods);
                if (nmods == 0 || nmods > maxMods)
                    return false;
                for (uint i = 0; i < nmods; i++)
                {
                    uint entry = toc + TocFirst + i * TocEntrySize;
                    uint name = bus.Read32(entry + 0x10);
                    if (!NamesEqual(baseName, ReadAscii(bus, name)))
                        continue;
                    uint tocAttr = bus.Read32(entry);
                    attr = (tocAttr & 0xFFFFEFFFu) | 0x2040u;
                    tocEntry = entry;
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private static uint ExtraRomToc(MipsBus bus)
        {
            if (_extraRomHdr != 0)
                return _extraRomHdr;
            if (bus == null || _extraRomStart == 0)
                return 0;
            try
            {
                uint sig = bus.Read32(_extraRomStart + 0x40);
                uint romhdr = bus.Read32(_extraRomStart + 0x44);
                if (sig != ExtraRomCece || romhdr == 0)
                    return _extraRomStart;
                _extraRomHdr = romhdr;
                return romhdr;
            }
            catch
            {
                return 0;
            }
        }

        public static bool TryMissMissingDevice(MipsBus bus, uint path, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 31 || path == 0)
                return false;
            try
            {
                string name = Basename(bus, path);
                if (string.IsNullOrEmpty(name))
                    return false;
                int n = name.Length;
                if (n > 0 && name[n - 1] == ':')
                    n--;
                if (n != 4)
                    return false;
                if ((name[0] != 'B' && name[0] != 'b')
                    || (name[1] != 'T' && name[1] != 't')
                    || (name[2] != 'V' && name[2] != 'v')
                    || name[3] != '1')
                    return false;
                regs[2] = 0xFFFFFFFFu;
                programCounter = regs[31];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void TryEnableFilterProcessAttach(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null || regs.Length <= 30)
                return;
            try
            {
                if ((regs[20] & CallDllFlag) != 0)
                    return;
                uint module = regs[30];
                if (module == 0)
                    return;
                if (bus.Read32(module + ModuleStartip) != FilterStartip)
                    return;
                regs[20] |= CallDllFlag;
            }
            catch
            {
            }
        }

        public static void TryFillTocStartip(MipsBus bus, uint module)
        {
            TryFillTocStartip(bus, module, false);
        }

        public static void TryFillTocStartip(MipsBus bus, uint module, bool replaceWrong)
        {
            if (bus == null || module == 0)
                return;
            try
            {
                uint obj = module + ModuleFileObj;
                if (bus.Read8(obj + 4) != TocAttachType)
                    return;
                uint tocEntry = bus.Read32(obj);
                if (tocEntry == 0)
                    return;
                uint e32 = bus.Read32(tocEntry + 0x14);
                if (e32 == 0)
                    return;
                uint entryrva = bus.Read32(e32 + 4);
                uint vbase = bus.Read32(e32 + 8);
                if (entryrva == 0)
                    return;
                uint cur = bus.Read32(module + ModuleStartip);
                if (vbase >= DdiNopVbase && vbase < 0x04000000u)
                {
                    if (cur == 0)
                        bus.Write32(module + ModuleStartip, vbase + entryrva);
                    return;
                }
                if (vbase != ExeVbase)
                    return;
                uint va = vbase + entryrva;
                if (cur == va)
                    return;
                if (!replaceWrong && cur != 0)
                    return;
                bus.Write32(module + ModuleStartip, va);
            }
            catch
            {
            }
        }

        public static bool TryForceXipExeCallDll(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 30)
                return false;
            uint module = regs[30];
            if (module == 0)
                return false;
            try
            {
                RefreshExeXipAlias(bus);
                if (!_aliasOn)
                    return false;
                TryFillTocStartip(bus, module, true);
                uint cur = bus.Read32(module + ModuleStartip);
                if (cur == 0)
                    return false;
                regs[4] = module;
                regs[5] = 0;
                programCounter = XipExeCallDllJal;
                System.Console.WriteLine("[Hive] force CallDLL module=0x" + module.ToString("X8") +
                    " startip=0x" + cur.ToString("X8"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void TryFillProcExeStartip(MipsBus bus)
        {
            if (bus == null)
                return;
            try
            {
                uint proc = bus.Read32(CurProc);
                if (proc == 0 || proc == 0xDEADBEEFu)
                    return;
                TryFillTocStartip(bus, proc);
                TryFillTocStartip(bus, proc + ProcModule);
                uint p50 = bus.Read32(proc + ProcModule);
                if (p50 != 0 && p50 != proc && p50 != proc + ProcModule)
                    TryFillTocStartip(bus, p50);
                RefreshExeXipAlias(bus);
            }
            catch
            {
            }
        }

        private static bool _aliasBusy;
        private static uint _aliasProc;
        private static uint _aliasReal;
        private static uint _aliasEnd;
        private static uint _aliasRom;
        private static uint _aliasSlot;
        private static bool _aliasOn;
        private static uint _aliasLoggedRom;

        public static void ResetExeXipAlias()
        {
            _aliasBusy = false;
            _aliasProc = 0;
            _aliasReal = 0;
            _aliasEnd = 0;
            _aliasRom = 0;
            _aliasSlot = 0;
            _aliasOn = false;
            _aliasLoggedRom = 0;
        }

        public static void RefreshExeXipAlias(MipsBus bus)
        {
            if (bus == null || _aliasBusy)
                return;
            try
            {
                _aliasBusy = true;
                uint proc = ProcessForXipAlias(bus);
                if (proc != _aliasProc)
                    RebuildExeXipAlias(bus, proc);
            }
            catch
            {
            }
            finally
            {
                _aliasBusy = false;
            }
        }

        public static uint MapExeXipVa(MipsBus bus, uint va)
        {
            uint off = va & 0x01FFFFFF;
            if (off < 0x00010000u || off >= 0x00100000u)
                return va;
            if (bus == null || _aliasBusy)
                return va;
            try
            {
                _aliasBusy = true;
                uint proc = ProcessForXipAlias(bus);
                if (proc != _aliasProc)
                    RebuildExeXipAlias(bus, proc);
            }
            catch
            {
                return va;
            }
            finally
            {
                _aliasBusy = false;
            }
            if (!_aliasOn || off < _aliasReal || off >= _aliasEnd)
                return va;
            uint region = va & 0xFE000000u;
            if (region != 0 && region != _aliasSlot)
                return va;
            return _aliasRom + (off - _aliasReal);
        }

        private static uint ProcessForXipAlias(MipsBus bus)
        {
            uint cur = bus.Read32(CurProc);
            try
            {
                uint thr = bus.Read32(ThreadPtr);
                if (thr != 0 && thr != 0xDEADBEEFu)
                {
                    uint sp = bus.Read32(thr + ThreadStack);
                    uint slot = sp & 0xFE000000u;
                    if (slot >= 0x04000000u && slot < 0x20000000u)
                    {
                        uint bySlot = FindProcBySlot(bus, slot);
                        if (bySlot != 0)
                            return bySlot;
                        uint tproc = bus.Read32(thr + ProcSlot);
                        if (tproc != 0 && tproc != 0xDEADBEEFu)
                            return tproc;
                    }
                }
            }
            catch
            {
            }
            return cur;
        }

        private static uint FindProcBySlot(MipsBus bus, uint slot)
        {
            for (uint i = 0; i < 16; i++)
            {
                uint p = ProcTable + i * ProcSize;
                uint vm = bus.Read32(p + ProcSlot) & 0xFE000000u;
                if (vm == slot)
                    return p;
            }
            return 0;
        }

        private static void RebuildExeXipAlias(MipsBus bus, uint proc)
        {
            _aliasProc = proc;
            _aliasOn = false;
            _aliasSlot = 0;
            _aliasReal = 0;
            _aliasEnd = 0;
            _aliasRom = 0;
            if (proc == 0 || proc == 0xDEADBEEFu)
                return;
            if (!TryBuildExeXipAlias(bus, proc)
                && !TryBuildExeXipAlias(bus, proc + ProcModule))
            {
                uint p50 = 0;
                try
                {
                    p50 = bus.Read32(proc + ProcModule);
                }
                catch
                {
                    return;
                }
                if (p50 != 0 && p50 != proc && p50 != proc + ProcModule)
                    TryBuildExeXipAlias(bus, p50);
            }
        }

        private static bool TryBuildExeXipAlias(MipsBus bus, uint module)
        {
            if (module == 0)
                return false;
            if (bus.Read8(module + ModuleFileObj + 4) != TocAttachType)
                return false;
            uint tocEntry = bus.Read32(module + ModuleFileObj);
            if (tocEntry == 0)
                return false;
            uint e32 = bus.Read32(tocEntry + 0x14);
            if (e32 == 0)
                return false;
            uint vbase = bus.Read32(e32 + 8);
            if (vbase != ExeVbase)
                return false;
            uint objcnt = bus.Read32(e32) & 0xFFFF;
            if (!TryGetTocO32(bus, tocEntry, objcnt, out uint o32Rom))
                return false;
            uint vsize = bus.Read32(o32Rom);
            uint dataptr = bus.Read32(o32Rom + 0xC);
            uint real = bus.Read32(o32Rom + 0x10);
            uint flags = bus.Read32(o32Rom + 0x14);
            if (vsize == 0 || real == 0)
                return false;
            if (dataptr < 0x80000000u || dataptr >= 0xA0000000u)
                return false;
            if ((flags & O32Compressed) != 0)
                return false;
            uint vaWord = 0;
            uint romWord = 0;
            try
            {
                vaWord = bus.Read32(real);
            }
            catch
            {
            }
            try
            {
                romWord = bus.Read32(dataptr);
            }
            catch
            {
                return false;
            }
            if (romWord == 0 || vaWord == romWord)
                return false;
            _aliasReal = real;
            _aliasEnd = real + vsize;
            _aliasRom = dataptr;
            try
            {
                uint proc = bus.Read32(CurProc);
                if (proc != 0 && proc != 0xDEADBEEFu)
                    _aliasSlot = bus.Read32(proc + ProcSlot) & 0xFE000000u;
            }
            catch
            {
            }
            _aliasOn = true;
            if (_aliasLoggedRom != dataptr)
            {
                _aliasLoggedRom = dataptr;
                System.Console.WriteLine("[Hive] XIP alias 0x" + real.ToString("X8") +
                    "-0x" + (real + vsize).ToString("X8") +
                    " -> 0x" + dataptr.ToString("X8") +
                    " slot=0x" + _aliasSlot.ToString("X8"));
            }
            return true;
        }

        // 0x80018F9C walks o32_lite at 180($fp). device.exe PROCESS stores
        // that pointer at e32_lite+0x54 (0x06012008) but the list stays
        // zero. TOC o32_rom already has FirstThunk 0x2000 in section 1.
        // Copy those bytes into the firmware dest; do not invent a slot.
        //
        // After TOC-attach of a DLL, BindImp uses that module's e32_lite,
        // not the current process. CurProc+0x50 is filesys.exe (objcnt 4);
        // iptvcryptohal / ceddk / sigcheckfilter are objcnt 3, so the
        // process TOC never matches. 0x800196E4 already wrote e32 vbase
        // at e32_lite+8 (0x03D90000 / 0x03E60000 / 0x03DF0000). Use that
        // when the process TOC misses. Slot vbase 0x00010000 is shared
        // by filesys/gwes/device and stays on CurProc+0x50.
        public static bool TryFillEmptyO32Lite(MipsBus bus, uint e32Lite, uint o32List, uint lookup)
        {
            if (bus == null || e32Lite == 0 || o32List == 0)
                return false;

            uint objcnt;
            uint tocEntry;
            uint vbase;
            try
            {
                objcnt = bus.Read32(e32Lite) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return false;
                if (bus.Read32(o32List) != 0 || bus.Read32(o32List + 4) != 0)
                    return false;
                uint proc = bus.Read32(CurProc);
                if (proc == 0)
                    return false;
                tocEntry = bus.Read32(proc + 0x50);
                vbase = bus.Read32(e32Lite + 8);
            }
            catch
            {
                return false;
            }

            if (!TryGetTocO32(bus, tocEntry, objcnt, out uint o32Rom)
                && !TryGetTocO32ByVbase(bus, vbase, objcnt, out o32Rom))
                return false;

            try
            {
                bool hit = false;
                for (uint s = 0; s < objcnt; s++)
                {
                    uint src = o32Rom + s * O32RomSize;
                    uint dst = o32List + s * O32LiteSize;
                    uint vsize = bus.Read32(src);
                    uint rva = bus.Read32(src + 4);
                    uint psize = bus.Read32(src + 8);
                    uint dataptr = bus.Read32(src + 0xC);
                    uint real = bus.Read32(src + 0x10);
                    uint flags = bus.Read32(src + 0x14);
                    bus.Write32(dst, vsize);
                    bus.Write32(dst + 4, rva);
                    bus.Write32(dst + 8, real);
                    bus.Write32(dst + 0xC, 0);
                    bus.Write32(dst + 0x10, flags);
                    bus.Write32(dst + 0x14, psize);
                    bus.Write32(dst + 0x18, dataptr);
                    if (lookup >= rva && lookup < rva + vsize)
                        hit = true;
                }
                return hit;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetTocO32(MipsBus bus, uint tocEntry, uint objcnt, out uint o32Rom)
        {
            o32Rom = 0;
            if (tocEntry == 0)
                return false;
            if (TryGetTocO32In(bus, 0, 64, tocEntry, objcnt, 0, out o32Rom))
                return true;
            return TryGetTocO32In(bus, ExtraRomToc(bus), 128, tocEntry, objcnt, 0, out o32Rom);
        }

        // ROM DLL vbases in this image are unique (HAL 0x03D90000,
        // CEDDK 0x03E60000, sigcheckfilter 0x03DF0000). Process EXEs
        // share 0x00010000; those stay on CurProc+0x50.
        private static bool TryGetTocO32ByVbase(MipsBus bus, uint vbase, uint objcnt, out uint o32Rom)
        {
            o32Rom = 0;
            if (vbase < DdiNopVbase || vbase >= 0x04000000u)
                return false;
            if (TryGetTocO32In(bus, 0, 64, 0, objcnt, vbase, out o32Rom))
                return true;
            return TryGetTocO32In(bus, ExtraRomToc(bus), 128, 0, objcnt, vbase, out o32Rom);
        }

        private static bool TryGetTocO32In(MipsBus bus, uint tocOrZero, uint maxMods,
            uint wantEntry, uint objcnt, uint wantVbase, out uint o32Rom)
        {
            o32Rom = 0;
            if (bus == null)
                return false;
            try
            {
                uint toc = tocOrZero;
                if (toc == 0)
                    toc = bus.Read32(EcecTocPtr);
                if (toc == 0)
                    return false;
                uint nmods = bus.Read32(toc + RomHdrNumMods);
                if (nmods == 0 || nmods > maxMods)
                    return false;
                uint found = 0;
                for (uint i = 0; i < nmods; i++)
                {
                    uint entry = toc + TocFirst + i * TocEntrySize;
                    if (wantEntry != 0 && entry != wantEntry)
                        continue;
                    uint e32 = bus.Read32(entry + 0x14);
                    uint o32 = bus.Read32(entry + 0x18);
                    if (e32 == 0 || o32 == 0)
                        continue;
                    if ((bus.Read32(e32) & 0xFFFF) != objcnt)
                        continue;
                    if (wantVbase != 0 && bus.Read32(e32 + 8) != wantVbase)
                        continue;
                    if (wantEntry != 0)
                    {
                        o32Rom = o32;
                        return true;
                    }
                    if (found != 0)
                        return false;
                    found = o32;
                }
                if (found == 0)
                    return false;
                o32Rom = found;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Basename(MipsBus bus, uint path)
        {
            var sb = new System.Text.StringBuilder();
            int start = 0;
            for (int i = 0; i < 260; i++)
            {
                uint p = path + (uint)(i * 2);
                uint word = bus.Read32(p & ~3u);
                uint ch = ((p & 2) == 0) ? (word & 0xFFFF) : (word >> 16);
                if (ch == 0)
                    break;
                if (ch == '\\' || ch == '/')
                {
                    sb.Length = 0;
                    start = i + 1;
                    continue;
                }
                if (ch < 0x20 || ch > 0x7E)
                    return "";
                sb.Append((char)ch);
            }
            return start >= 0 ? sb.ToString() : "";
        }

        private static string ReadAscii(MipsBus bus, uint addr)
        {
            if (addr == 0)
                return "";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 80; i++)
            {
                uint src = addr + (uint)i;
                uint word = bus.Read32(src & ~3u);
                uint ch = (word >> (8 * (int)(src & 3))) & 0xFF;
                if (ch == 0)
                    break;
                if (ch < 0x20 || ch > 0x7E)
                    return "";
                sb.Append((char)ch);
            }
            return sb.ToString();
        }

        public static bool TryResolveFilterExport(MipsBus bus, uint module, uint namePtr, uint[] regs, ref uint programCounter)
        {
            if (bus == null || module == 0 || namePtr == 0 || regs == null || regs.Length <= 31)
                return false;
            try
            {
                if (bus.Read32(module + ModuleStartip) != FilterStartip)
                    return false;
                string want = ReadUtf16Name(bus, namePtr);
                if (string.IsNullOrEmpty(want))
                    return false;
                if (want.Length > 4
                    && (want[0] == 'F' || want[0] == 'f')
                    && (want[1] == 'S' || want[1] == 's')
                    && (want[2] == 'D' || want[2] == 'd')
                    && want[3] == '_')
                    want = want.Substring(4);
                if (string.IsNullOrEmpty(want))
                    return false;
                if (!TryFindTocExport(bus, FilterVbase, want, out uint va))
                    return false;
                if (va < FilterVbase || va >= FilterVbase + 0xA000u)
                    return false;
                regs[2] = va;
                programCounter = regs[31];
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryFindTocExport(MipsBus bus, uint vbase, string want, out uint va)
        {
            va = 0;
            uint toc = bus.Read32(EcecTocPtr);
            uint nmods = bus.Read32(toc + RomHdrNumMods);
            if (nmods == 0 || nmods > 64)
                return false;
            for (uint i = 0; i < nmods; i++)
            {
                uint entry = toc + TocFirst + i * TocEntrySize;
                uint e32 = bus.Read32(entry + 0x14);
                uint o32 = bus.Read32(entry + 0x18);
                if (e32 == 0 || o32 == 0)
                    continue;
                if (bus.Read32(e32 + 8) != vbase)
                    continue;
                uint objcnt = bus.Read32(e32) & 0xFFFF;
                uint expRva = bus.Read32(e32 + E32RomExpRva);
                uint expSize = bus.Read32(e32 + E32RomExpRva + 4);
                if (expRva == 0 || expSize < 0x28 || expSize > 0x800)
                    return false;
                if (!TryPackedFromRva(bus, o32, objcnt, expRva, out uint expPacked))
                    return false;
                uint nNames = bus.Read32(expPacked + 0x18);
                uint addrFuncs = bus.Read32(expPacked + 0x1C);
                uint addrNames = bus.Read32(expPacked + 0x20);
                uint addrOrds = bus.Read32(expPacked + 0x24);
                if (nNames == 0 || nNames > 64)
                    return false;
                if (!TryPackedFromRva(bus, o32, objcnt, addrNames, out uint namesPacked)
                    || !TryPackedFromRva(bus, o32, objcnt, addrFuncs, out uint funcsPacked)
                    || !TryPackedFromRva(bus, o32, objcnt, addrOrds, out uint ordsPacked))
                    return false;
                for (uint n = 0; n < nNames; n++)
                {
                    uint nameRva = bus.Read32(namesPacked + n * 4);
                    if (!TryPackedFromRva(bus, o32, objcnt, nameRva, out uint namePacked))
                        continue;
                    if (!NamesEqual(ReadAscii(bus, namePacked), want))
                        continue;
                    uint ordWord = bus.Read32((ordsPacked + n * 2) & ~3u);
                    uint ord = ((ordsPacked + n * 2) & 2) == 0 ? (ordWord & 0xFFFF) : (ordWord >> 16);
                    if (ord >= nNames)
                        return false;
                    uint funcRva = bus.Read32(funcsPacked + ord * 4);
                    if (funcRva == 0 || funcRva >= 0x10000)
                        return false;
                    va = vbase + funcRva;
                    return true;
                }
                return false;
            }
            return false;
        }

        private static bool TryPackedFromRva(MipsBus bus, uint o32, uint objcnt, uint rva, out uint packed)
        {
            packed = 0;
            if (objcnt == 0 || objcnt > 16 || rva == 0)
                return false;
            for (uint s = 0; s < objcnt; s++)
            {
                uint src = o32 + s * O32RomSize;
                uint vsize = bus.Read32(src);
                uint sectRva = bus.Read32(src + 4);
                uint dataptr = bus.Read32(src + 0xC);
                if (dataptr == 0 || vsize == 0)
                    continue;
                if (rva < sectRva || rva >= sectRva + vsize)
                    continue;
                packed = dataptr + (rva - sectRva);
                return packed != 0;
            }
            return false;
        }

        private static string ReadUtf16Name(MipsBus bus, uint addr)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 80; i++)
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

        public static uint KeepProcessHeapIfCreateFailed(MipsBus bus, uint created, uint dest)
        {
            if (created != 0 || dest != ProcessHeapPtr || bus == null)
                return created;
            try
            {
                uint old = bus.Read32(ProcessHeapPtr);
                if (old != 0)
                    return old;
            }
            catch
            {
            }
            return created;
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
