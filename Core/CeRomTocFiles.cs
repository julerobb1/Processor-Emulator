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
        // 0x8001D3A0 jal 0x8003D700 then bne v0,-1 at
        // 0x8001D3F8. 0x8001D400 only runs on INVALID_HANDLE.
        // wait59: OpenExe \mscoree.dll entered CreateFile and
        // never hit 0x8001D400, so filesys returned a handle.
        // NK/ExtraROM FILE tables and the volume have no
        // mscoree.dll. That type-8 handle is 193. Force
        // INVALID_HANDLE here so type-7 attaches TOC[46].
        public const uint CreateFileWin32Chk = 0x8001D3F8;
        public const uint NameCopyContinue = 0x8001D464;
        // CreateFile success epilogue. Type 7 must not take
        // NameCopyContinue: that CreateFileMappings object+0.
        // A TOCentry is not a handle. CreateFile then returned
        // 14/1392, OpenExe failed, 0x8001DFC4 retried
        // .dll.dll, and 0x8001E3AC was 126. Same object as
        // TocWalk (entry + type 7); v0=0 so LoadE32 runs.
        public const uint CreateFileOk = 0x8001D568;
        // 0x80016AFC walks *(0x80342B10) ROMHDR nodes. ExtraROM
        // 0x8134DA84 is mapped but never linked, so LoadDriver of
        // bare ddi_nop.dll misses (v0=2) and never CreateFile
        // (OpenExe 0x8001D6F0 stores 24($sp)=0 when the name has
        // no \ or /). Same hit layout as NK TOC: object+0=entry,
        // +4=7, v0=0. 0x800196E4 then uses e32 at TOC+0x14.
        public const uint TocWalkMiss = 0x80016B74;
        public const uint TocWalkMissContinue = 0x80016B78;
        public const uint LoadE32Rom = 0x800196E4;
        public const uint LoadE32RomRet = 0x8001E3E8;
        // After OpenE32, 0x8001E418 jal 0x800165DC then
        // 0x8001E750 jal 0x8001AFA4 (CopyO32). MapO32
        // 0x8001AC30 jal 0x80028844 only when flags lack
        // 0x80002000. ExtraROM o32[0] 0x60002020 has 0x2000
        // and skips to VirtualCopy 0x80043298 of compressed
        // dataptr 0x80764CE0. Do not host-alias that XIP.
        public const uint LoadO32Rom = 0x800165DC;
        public const uint LoadO32RomRet = 0x8001E420;
        public const uint CopyO32Rom = 0x8001AFA4;
        public const uint MapO32Rom = 0x8001AC30;
        // 0x8001AC9C: bne (flags & 0x80002000), AD50.
        // flags 0x60006020 have 0x2000, so jal 0x80028844 is
        // skipped. AD50 VALLOCs only when object+6>=2 or flags
        // have 0x08000000; type-7 attach stores neither, so
        // dest stays zeros. Clear 0x2000 on TOC[46] o32_lite
        // only (a3==0) so firmware jals 0x80028844 onto the
        // steered dest. Do not VALLOC. Do not poke object+6.
        public const uint MapO32RomEpilogue = 0x8001AE50;
        public const uint MapO32Decompress = 0x80028844;
        public const uint MapO32DecompressSrcChk = 0x80028A48;
        public const uint MapO32DecompressFail = 0x80028A90;
        public const uint MapO32DecompressCommitChk = 0x800289F8;
        public const uint MapO32CommitDest = 0x80026F50;
        public const uint MapO32VirtualCopy = 0x80043298;
        public const uint MapO32VallocRet = 0x8001AE08;
        // 0x80028844 remaps dest PTEs onto src (XIP alias). Its
        // kseg0 src path (0x80028A60) sets 32($sp)=1 and never
        // writes dest bytes, so startip stays VALLOC zeros.
        // Kernel 0x80050974 is CEDecompress (CE3 inner
        // 0x800504B4). ExtraROM pages are not that codec:
        // after the 3-byte table each slice is
        // window=16 / vsize / … (LZX). 0x800504B4 then
        // returns -10/-12 on B5/B4. Official 4K wrapper
        // 0x80043B8C jals CEDecompressROM 0x8004DBF8
        // (inner 0x80050F78). Same args: skip, convert,
        // stepsize. Byte 3 stays the first page-offset
        // low byte. Do not cap leftover at 0x1000.
        public const uint BinaryDecompressRom = 0x8004DBF8;
        public const uint BinaryDecompressInner = 0x80050F78;
        public const uint BinaryDecompressAfterInner = 0x8004DD80;
        public const uint MemReserve = 0x2000;
        public const uint SlotMask = 0x01FFFFFF;
        public const uint LoadLibSyscallRet = 0x03F6C8F4;
        public const uint BindImpMiss = 0x80018F9C;
        public const uint BindImpWalk = 0x80018F3C;
        // 0x80018E94 lw ImpHdr+0; 0x80018EC0 lbu name at
        // vbase+NameRVA. ExtraROM IMP is RVA 0x18350
        // (e32+0x2C). Name RVA 0 reads the unmapped
        // header page at vbase (0x01980000).
        public const uint BindImpHdr = 0x80018E94;
        public const uint BindImpDllName = 0x80018EC0;
        public const uint BindImpLoadLib = 0x8001E9D4;
        public const uint BindImpLoadLibRet = 0x80018EF8;
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
        // 0x8001DD90 is addiu a1,0,0 / jal 0x80018B34. EXE
        // wants reason 0. ExtraROM ddi_nop DllMain needs
        // a1=1 (PROCESS_ATTACH); landing on 0x8001DD90
        // wipes that and CallDLL returns 0 (last-error 1114).
        // 0x8001DD94 is the jal; delay or $a0, $fp, $0.
        public const uint CallDllStartip = 0x80018BAC;
        public const uint CallDllAfterJalr = 0x80018BB8;
        public const uint XipExeCallDllSkip = 0x8001DDA4;
        public const uint XipExeCallDllJal = 0x8001DD90;
        public const uint XipDllCallDllJal = 0x8001DD94;
        public const uint ThreadStartTrampoline = 0x8001FF38;
        public const uint LoadExeE32Ret = 0x8001F870;
        // LoadExe 0x8001F81C. 0x8001F870 is jal 0x800196E4 ret.
        // startip is not stored there. 0x8001FD74 lw 28($sp)
        // (FILE LoadE32 AddressOfEntryPoint) then jal 0x8001B388
        // unless e32_lite+16 (COM) takes BindImpLoadLib first.
        public const uint LoadExeStartipArg = 0x8001FD74;
        public const uint LoadExeStartipRet = 0x8001FD80;
        public const uint ThreadContextSetup = 0x80020BE4;
        // 0x80015404 / 0x8001566C lw k0, 236(s0) then ERET.
        // wait68: tv2 thread+5C is firmware 0x014B9D98 but
        // +0xEC is 0x800517B8 (mid CEDecompressROM). Resume
        // that leftover never I-fetches _CorExeMain.
        public const uint ThreadCtxRestore = 0x80015404;
        public const uint ThreadCtxRestore2 = 0x8001566C;
        public const uint ThreadCtxPc = 0xEC;
        public const uint ThreadStartip = 0x5C;
        public const uint ThreadCtxSr = 0xF0;
        // 0x800397B0 stores +F0=3 when the syscall
        // frame is kernel, +F0=0x13 when it is user
        // (0x8003980C addiu $v0, $0, 19). 0x8001589C
        // andi Status, 0x10: bit 4 takes the user
        // frame/ERET path. 3 skips that and 0x80015A28
        // jr $ra of *(thread+0x18)+4. wait81 that
        // return was 0, I-fetch 0, ra=0. Not a null
        // user RA (that was 0x03F6C8F4). Do not map
        // page 0.
        public const uint ThreadCtxSrKernel = 3;
        public const uint ThreadCtxSrUser = 0x13;
        public const uint ThreadSyscallFrame = 0x18;
        // 0x80020C30 / 0x80020D10 sw $0, 220(a0) at
        // ThreadContextSetup. Implicit-API 0x8001586C
        // never hits 0x800152CC, so +0xDC stays 0.
        // 0x80015404 then lw ra, 220(s0) and ERET.
        public const uint ThreadCtxRa = 0xDC;
        // 0x800154DC sw $s7, 188(s0); 0x800155C4 lw.
        // Implicit-API 0x8001586C never hits that save.
        public const uint ThreadCtxS7 = 0xBC;
        // dest 0x800908B0 / VA 0x03F6C8B0 addiu $s7, $0, 22528
        public const uint UserKData = 0x5800;
        // firmware 0x80014488 / 0x800146AC lw $sp, 212(s0)
        // then ERET. Implicit-API 0x8001586C never
        // stores +0xD4. wait85 leftover ERET then
        // TLB store 0x03F6CABC vaddr=0xE4DA9AA4
        // dest-word=0 pte-miss. va>>25=114, not a
        // process slot, not BCM 0x10xxxxxx / kseg1 /
        // 0xF0600000 / 0x1F000000. Garbage GPR, not
        // MMIO. Do not map page 0. Do not invent dest
        // at 0xE4DA9AA4.
        public const uint ThreadCtxSp = 0xD4;
        public const uint ThreadPrc = 0x0C;
        // 0x8001554C beq s0, v0, 0x800155A8 skips CurProc
        // update when the same thread is rescheduled.
        // wait69: tv2 +0x0C was filesys, so even the slow path
        // stored CurProc=0x80340110 and I-fetch 0x014B9D98
        // faulted to 0x8001588C. Do not invent a slot map.
        public const uint ThreadSwitchProcChk = 0x8001554C;
        public const uint ThreadSwitchProcSlow = 0x80015550;
        public const uint ThreadSwitchProcStore = 0x80015570;
        public const uint ExnAfterFetch = 0x8001588C;
        public const uint ExnAfterFetch2 = 0x80015B9C;
        // leftover 0x800159A8 jal 0x800397B0 then
        // 0x800159B4 or $ra,$v0,$0. wait99: that jal
        // returned -1 so EPC became 0xFFFFFFFF.
        // 0x80015A08 mtc0 $t4,EPC; 0x80015A24 ERET.
        public const uint LeftoverOrRa = 0x800159B4;
        public const uint LeftoverMtc0Epc = 0x80015A08;
        public const uint LeftoverJrRa = 0x80015A28;
        public const uint LeftoverEret = 0x80015A24;
        public const uint LeftoverContinue = 0x03F6CAF0;
        // wait100: leftover jr to CAF0 (nop delay of
        // beq $v0,$0,+12 at CAEC). Fallthrough CAF4
        // with the beq skipped I-fetched 0. Taken
        // target is CAFC. Do not map page 0.
        public const uint LeftoverAfterCaf0 = 0x03F6CAF4;
        public const uint LeftoverBeqTaken = 0x03F6CAFC;
        // wait101: leftover CAFC addiu $v0 then
        // 0x03F6CB0C lw $a1,-20($s6) vaddr=0xFFFFFFEC.
        // vaddr == -20 means $s6==0 (null-20). Not a
        // ROM page. leftover-CAE8 already lw $v0,0($s6).
        public const uint LeftoverCb0c = 0x03F6CB0C;
        public const uint LeftoverCb0cNext = 0x03F6CB10;
        // wait102: leftover past CB10 dest-word
        // 0x30A40001 (andi $a0,$a1,1). Next coredll
        // insn is CB14. After that, tv2 ctxPC is
        // ERET2 0x80015B9C, not leftover mid
        // 0x8001586C and not OEMIdle. Resume to
        // CB14 after dest peek. Do not rewrite
        // 0x80015B9C.
        public const uint LeftoverCb14 = 0x03F6CB14;
        // wait103: leftover past CB14 dest-word
        // 0x10800007 (beq $a0,$0,+7). Taken target
        // is CB34. After that, tv2 +DC=0x03F6CB34
        // then ctxPC is leftover mid 0x8001588C
        // then ERET2 0x80015B9C. Not OEMIdle
        // (later 600M DONE). Resume to CB34 after
        // dest peek. Do not rewrite 0x80015B9C.
        public const uint LeftoverCb34 = 0x03F6CB34;
        // wait74: after I-fetch, ctxPC=0x80040298 then
        // ThreadExceptionExit. 0x800154EC beq a1,0 skips
        // jal 0x80020D80; that jal 0x80040278. 0x80040298
        // is sw $s2,40($sp) in that VM/PTE check. Not a
        // vector. I-fetch log is before FetchInstruction.
        public const uint SwitcherExnCall = 0x80020D80;
        public const uint ExnVmCheck = 0x80040278;
        public const uint ExnVmCheckMid = 0x80040298;
        public const uint ExnVmCheckEnd = 0x80040400;
        // wait75: TLB I-fetch 0x03F73380 after startip+4.
        // Slot 1. Coredll code is 0x03F5xxxx (IsApiReady
        // 0x03F73240, CreateThread 0x03F71E04). wait77:
        // 0x80018580 lw 0(s5) in the module name walk
        // (a0+0x50 vbase + e32 RVA). 0x03FAC0A0 /
        // 0x03FB4A60 / 0x03FBF69C / 0x03FD1FD8 are that
        // same slot-1 module past the 0x03FA0000 code cap.
        // Walk the live section. Do not invent 0x03FD0000.
        public const uint CoredllSharedLo = 0x03F50000;
        public const uint CoredllSharedHi = 0x03FE0000;
        public const uint BindImpNameWalk = 0x80018580;
        public const uint KDataSection = 0xFFFFD8C0;
        // 0x8001521C ori k1, epc, 0xFFFC / addiu 2 / beq
        // syscall. 0xFFFFF3DA is coredll 0x80095A98
        // addiu $v0, $0, -3110 / jalr $v0. Same class as
        // SetFilePointer 0xFFFFDFEE. Not KData. Not a slot.
        public const uint KDataNest = 0xFFFFD885;
        public const uint ExeVbase = 0x00010000;
        public const uint ProcModule = 0x50;
        public const uint ProcSlot = 0x0C;
        public const uint ProcTable = 0x80340040;
        public const uint ProcSize = 0xD0;
        public const uint ThreadPtr = 0xFFFFDAC0;
        public const uint ThreadStack = 0x24;
        public const uint O32Compressed = 0x4000;
        // ExtraROM o32[0] 0x60002020: 0x2000 lets CopyO32 accept
        // unaligned dataptr 0x80764CE0. MapO32 still VirtualCopys
        // those bytes as XIP unless 0x2000 is cleared on the lite.
        public const uint O32RomXip = 0x2000;
        public const uint O32Writable = 0x80000000;
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
        public const uint RomHdrNumFiles = 0x30;
        public const uint TocFirst = 0x54;
        public const uint TocEntrySize = 32;
        public const uint FilesEntrySize = 28;
        public const uint FilesRealSize = 0x0C;
        public const uint FilesCompSize = 0x10;
        public const uint FilesNameOff = 0x14;
        public const uint FilesLoadOff = 0x18;
        public const byte TocAttachType = 7;
        // CreateFile success stores 8 (file handle). LoadE32
        // (type&2)==0 then SetFilePointer/ReadFile and checks
        // PE 0x4550. Type 7 reads entry+0x14 as e32 (wait55 193).
        public const byte FileAttachType = 8;
        public const uint KernelReadFile = 0x8003D7E0;
        public const uint KernelCreateFileMapping = 0x8003DA64;
        // jalr -8210 is SetFilePointer (a1=dist, a3=method).
        public const uint Win32SetFilePointer = 0xFFFFDFEE;
        // Scratch for FILE[25] CEDecompressROM. Not ExtraROM
        // tail and not a dump 0x81360000 map.
        public const uint Tv2FileDest = 0x8F140000;
        public const uint Tv2FileSrcAlign = 0x8F030000;
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
        private static string _pendingRomFile;
        private static uint _ddiNopTocEntry;
        private static uint _ddiNopAttr;
        // ExtraROM TOC/e32/o32 live at 0x8134xxxx / 0x80E99Cxx.
        // Firmware later reuses that phys as RAM and zeros the
        // TOC. Cache the dump bytes at map time and put them
        // back when LoadDriver asks. Do not invent 0x81360000.
        private static uint[] _ddiNopTocWords;
        private static uint _ddiNopE32;
        private static uint[] _ddiNopE32Words;
        private static uint _ddiNopO32;
        private static uint[] _ddiNopO32Words;
        private static uint[] _ddiNopDataPtr;
        private static uint[] _ddiNopDataLen;
        private static uint[][] _ddiNopData;
        // wait59: ExtraROM TOC[46] mscoree.dll. FILE table has
        // mscorlib.dll / system*.dll, not this name. Same tail
        // reuse as TOC[33] / FILE[25]. Cache at map time.
        // Do not invent a FILE. Do not invent 0x81360000.
        private static uint _mscoreeTocEntry;
        private static uint _mscoreeAttr;
        private static uint[] _mscoreeTocWords;
        private static uint _mscoreeE32;
        private static uint[] _mscoreeE32Words;
        private static uint _mscoreeO32;
        private static uint[] _mscoreeO32Words;
        private static uint[] _mscoreeDataPtr;
        private static uint[] _mscoreeDataLen;
        private static uint[][] _mscoreeData;
        // wait65: OpenExe \ole32.dll after TOC[46] MapO32, then
        // CreateProcess 193. ExtraROM TOC[34] is that name
        // (e32 0x80E99CC8). FILE table has no ole32.dll.
        // Do not invent a FILE. Do not attach oleaut32 (TOC[35])
        // unless firmware asks.
        private static uint _ole32TocEntry;
        private static uint _ole32Attr;
        private static uint[] _ole32TocWords;
        private static uint _ole32E32;
        private static uint[] _ole32E32Words;
        private static uint _ole32O32;
        private static uint[] _ole32O32Words;
        private static uint[] _ole32DataPtr;
        private static uint[] _ole32DataLen;
        private static uint[][] _ole32Data;
        private static bool _ole32DestOn;
        private static uint _ole32Slot0;
        private static uint _ole32Vbase;
        // wait54: ExtraROM FILE[25] tv2clientce.exe lives at
        // 0x8134E794 (28-byte FILESentry). Firmware later reuses
        // that tail as RAM (same class as TOC[33]). Cache at map
        // time and put the dump bytes back before CreateFileFail
        // attach. Do not invent 0x81360000.
        private static uint _tv2FileEntry;
        private static uint[] _tv2FileWords;
        private static uint _tv2FileName;
        private static uint[] _tv2FileNameWords;
        private static uint _tv2FileReal;
        private static uint _tv2FileComp;
        private static uint _tv2FileLoad;
        private static uint[] _tv2FileData;
        private static uint _tv2FileDecompRa;
        private static uint _tv2FileSavedSp;
        private static uint _tv2FilePos;
        private static bool _tv2FileDestOn;
        private static bool _tv2FileIoLogged;
        // wait56: firmware VALLOC a0=0x00010000 a1=0x00008000
        // a2=0x01002000 (MEM_IMAGE|RESERVE) for this dump PE.
        // MapO32 dests 0x00012000/0x00014000/0x00016000 are in
        // that range; dataptr 0x200/0xC00/0x1200 are PE raw
        // offsets, not ExtraROM XIP. Dedicated RA: the shared
        // _vallocRa slot is overwritten before return.
        private static uint _tv2PeImageVa;
        private static uint _tv2PeImageBytes;
        private static uint _tv2PeVallocRa;
        private static bool _tv2BindLogged;
        private static uint _tv2PeEntryRva;
        private static uint _tv2PeImageBase;
        private static uint _tv2PeComRva;
        private static uint _tv2Proc;
        // wait67: LoadExe 0x8001F870 logs proc+0x5C before
        // e32+16 COM takes BindImpLoadLib(mscoree) /
        // GetProcAddress(_CorExeMain). That VA is s3 at
        // 0x8001FD80 and thread+5C (0x014B9D98). Type-8
        // never fills proc+0x5C. Keep firmware s3 only
        // when it lands on a mapped dest. Do not write
        // dump AddressOfEntryPoint 0x7F54 (that invents
        // 0x00017F54; filesys already I-fetches there).
        private static uint _tv2Startip;
        private static uint _tv2Thread;
        private static bool _tv2FetchLogged;
        private static bool _tv2ContinueLogged;
        private static bool _tv2ExnHelperLogged;
        private static bool _tv2PostFetchExnLogged;
        private static bool _tv2ImplAdelLogged;
        private static bool _tv2AfterExnContLogged;
        private static bool _pteMapBusy;
        private static bool _pteMapLogged;
        private static bool _slot2MapLogged;
        private static bool _slot0InfoMapLogged;
        private static bool _slot0FetchMapLogged;
        private static bool _tv2CoredllLogged;
        private static bool _tv2CoredllContLogged;
        private static uint _coredllLiveSec;
        private static bool _coredllLiveLogged;
        private static bool _coredllMapLogged;
        private static bool _coredllHighLogged;
        private static bool _coredllZeroLogged;
        private static bool _tv2ZeroContLogged;
        private static bool _tv2HighContLogged;
        private static uint _tv2ImplRa;
        private static uint _tv2ImplResume;
        private static uint _tv2ImplEpc;
        private static uint _tv2ImplK1Before;
        private static bool _tv2ImplContLogged;
        private static bool _tv2ImplPastLogged;
        private static bool _tv2UserSrLogged;
        private static bool _tv2DispatchCtxLogged;
        private static bool _tv2UserSpLogged;
        private static bool _tv2UserRaLogged;
        private static uint _tv2StoreSp;
        private static bool _tv2StoreContLogged;
        private static bool _tv2LeftoverStoreFrame;
        private static bool _tv2StoreFrameLogged;
        private static bool _tv2LeftoverLiveLogged;
        private static bool _tv2LeftoverPastLogged;
        private static bool _tv2LeftoverCae8Logged;
        private static bool _tv2LeftoverSkipLogged;
        private static bool _tv2LeftoverCaf0Logged;
        private static bool _tv2LeftoverCaf0Peeked;
        private static uint _tv2LeftoverCaf0Word;
        private static bool _tv2LeftoverCae8V0Set;
        private static uint _tv2LeftoverCae8V0;
        private static bool _tv2LeftoverCae8S6Set;
        private static uint _tv2LeftoverCae8S6;
        private static bool _tv2LeftoverS6Logged;
        private static bool _tv2LeftoverCaf4Peeked;
        private static uint _tv2LeftoverCaf4Word;
        private static bool _tv2LeftoverCafcPeeked;
        private static uint _tv2LeftoverCafcWord;
        private static bool _tv2LeftoverAfterCaf0Logged;
        private static bool _tv2LeftoverPastAfterLogged;
        private static bool _tv2LeftoverPastCb0cLogged;
        private static bool _tv2LeftoverCb14Peeked;
        private static uint _tv2LeftoverCb14Word;
        private static bool _tv2LeftoverAfterCb10Logged;
        private static bool _tv2LeftoverPastCb14Logged;
        private static bool _tv2LeftoverCb34Peeked;
        private static uint _tv2LeftoverCb34Word;
        private static bool _tv2LeftoverAfterCb14Logged;
        private static bool _tv2LeftoverPastCb34Logged;
        private static bool _tv2LeftoverEretLogged;
        private static bool _tv2GwesFetchLogged;
        private static bool _tv2GwesContLogged;
        private static bool _tv2MscoreeSlotLogged;
        private static bool _coredllMapBusy;
        private static bool _tv2ProcSwitchLogged;
        private static bool _tv2CurThreadLogged;
        private static bool _tv2RestoreLogged;
        private static bool _tv2SwitchForced;
        private static bool _tv2SwitchStoreLogged;

        public static void NotePendingRomFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            int slash = path.LastIndexOf('\\');
            if (slash < 0)
                slash = path.LastIndexOf('/');
            _pendingRomFile = slash >= 0 ? path.Substring(slash + 1) : path;
        }

        public static bool TryContinueRomModule(MipsBus bus, uint path, out uint attr, out uint tocEntry)
        {
            return TryContinueRomModule(bus, path, out attr, out tocEntry, out _);
        }

        public static bool TryContinueRomModule(MipsBus bus, uint path, out uint attr, out uint tocEntry, out byte attachType)
        {
            attr = 0;
            tocEntry = 0;
            attachType = TocAttachType;
            if (bus == null || path == 0)
                return false;

            string baseName = Basename(bus, path);
            if (string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(_pendingRomFile))
                baseName = _pendingRomFile;
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
                && !NamesEqual(baseName, "ddi_nop.dll")
                && !IsMscoreeDll(baseName)
                && !IsOle32Dll(baseName)
                && !IsTv2ClientCe(baseName))
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
                TryMarkExtraRomO32Compressed(bus, tocEntry);
                return true;
            }
            // wait59: BindImp of FILE[25] OpenExe \mscoree.dll.
            // ExtraROM TOC[46] is that name (e32 0x80E9A658).
            // FILE table has mscorlib/system*.dll, not mscoree.dll.
            // Do not invent a FILE. Do not attach TOC[79]
            // mscoree3_5.dll. Type 7: e32 at entry+0x14.
            if (IsMscoreeDll(baseName))
            {
                TryRestoreExtraRomMscoreeIfClobbered(bus);
                if (_mscoreeTocEntry != 0 && _mscoreeTocWords != null)
                {
                    tocEntry = _mscoreeTocEntry;
                    attr = _mscoreeAttr != 0 ? _mscoreeAttr : _mscoreeTocWords[0];
                }
                else if (!TryFindTocModule(bus, ExtraRomToc(bus), 128, "mscoree.dll", out tocEntry, out attr))
                {
                    System.Console.WriteLine("[Hive] TOC-attach ExtraROM mscoree.dll miss" +
                        " (FILE table has no mscoree.dll; do not invent a FILE)");
                    return false;
                }
                attachType = TocAttachType;
                System.Console.WriteLine("[Hive] TOC-attach ExtraROM mscoree.dll entry=0x" +
                    tocEntry.ToString("X8") +
                    " type=7 attr=0x" + attr.ToString("X8") +
                    " e32=0x" + (_mscoreeE32 != 0 ? _mscoreeE32 : (uint)0).ToString("X8") +
                    " (TOC[46]; not a FILE; do not invent 0x81360000)");
                TryMarkExtraRomO32Compressed(bus, tocEntry);
                _pendingRomFile = null;
                return true;
            }
            // wait65: BindImp OpenExe \ole32.dll after mscoree
            // MapO32. ExtraROM TOC[34] is that name. FILE table
            // has no ole32.dll. Type 7: e32 at entry+0x14.
            // Do not invent a FILE. Do not attach TOC[35].
            if (IsOle32Dll(baseName))
            {
                TryRestoreExtraRomOle32IfClobbered(bus);
                if (_ole32TocEntry != 0 && _ole32TocWords != null)
                {
                    tocEntry = _ole32TocEntry;
                    attr = _ole32Attr != 0 ? _ole32Attr : _ole32TocWords[0];
                }
                else if (!TryFindTocModule(bus, ExtraRomToc(bus), 128, "ole32.dll", out tocEntry, out attr))
                {
                    System.Console.WriteLine("[Hive] TOC-attach ExtraROM ole32.dll miss" +
                        " (FILE table has no ole32.dll; do not invent a FILE)");
                    return false;
                }
                attachType = TocAttachType;
                System.Console.WriteLine("[Hive] TOC-attach ExtraROM ole32.dll entry=0x" +
                    tocEntry.ToString("X8") +
                    " type=7 attr=0x" + attr.ToString("X8") +
                    " e32=0x" + (_ole32E32 != 0 ? _ole32E32 : (uint)0).ToString("X8") +
                    " (TOC[34]; not a FILE; do not invent 0x81360000)");
                TryMarkExtraRomO32Compressed(bus, tocEntry);
                _pendingRomFile = null;
                return true;
            }
            // wait53: CreateFile \Windows\tv2clientce.exe is
            // INVALID_HANDLE. ExtraROM FILE[25] is that name
            // (5120/2421 at 0x81050DCC), not a TOC module and
            // not the 90-byte root stub. Same attach as TOC
            // (object+0=entry, +4=7). Image bytes are already
            // in ExtraROM RAM. wait54: live FILE table at
            // 0x8134xxxx is zeros by Launch56 (ExtraROM tail
            // RAM reuse). Restore the cached FILESentry first.
            // Do not invent 0x81360000. Do not host CreateProcess.
            if (IsTv2ClientCe(baseName))
            {
                TryRestoreExtraRomFileIfClobbered(bus);
                uint real = 0;
                uint comp = 0;
                uint load = 0;
                if (_tv2FileEntry != 0 && _tv2FileWords != null)
                {
                    tocEntry = _tv2FileEntry;
                    // Real FILE attr 0x807 (COMPRESSED). Do not set
                    // 0x2000: that is ROMMODULE and LoadE32 reads
                    // entry+0x14 as e32 (wait55 193).
                    attr = _tv2FileWords[0];
                    real = _tv2FileReal;
                    comp = _tv2FileComp;
                    load = _tv2FileLoad;
                }
                else if (!TryFindExtraRomFile(bus, "tv2clientce.exe", out tocEntry, out attr,
                    out real, out comp, out load))
                {
                    return false;
                }
                attachType = FileAttachType;
                System.Console.WriteLine("[Hive] FILE-attach ExtraROM tv2clientce.exe entry=0x" +
                    tocEntry.ToString("X8") +
                    " type=8 attr=0x" + attr.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    " (FILESentry; firmware SetFilePointer/ReadFile; not a dump 0x81360000 map)");
                _pendingRomFile = null;
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
            if (!NamesEqual(baseName, "ddi_nop.dll") && !IsMscoreeDll(baseName)
                && !IsOle32Dll(baseName))
                return false;
            if (IsMscoreeDll(baseName))
                TryRestoreExtraRomMscoreeIfClobbered(bus);
            else if (IsOle32Dll(baseName))
                TryRestoreExtraRomOle32IfClobbered(bus);
            uint tocEntry = IsOle32Dll(baseName) ? _ole32TocEntry
                : (IsMscoreeDll(baseName) ? _mscoreeTocEntry : _ddiNopTocEntry);
            string findName = IsOle32Dll(baseName) ? "ole32.dll"
                : (IsMscoreeDll(baseName) ? "mscoree.dll" : baseName);
            if (tocEntry == 0
                && !TryFindTocModule(bus, ExtraRomToc(bus), 128, findName, out tocEntry, out _))
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
                System.Console.WriteLine("[Hive] TOC-walk ExtraROM " + baseName + " miss toc=0x" +
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
            System.Console.WriteLine("[Hive] TOC-walk ExtraROM " + baseName + " entry=0x" +
                tocEntry.ToString("X8") +
                (IsOle32Dll(baseName)
                    ? " (OpenExe; TOC[34]; do not invent a FILE)"
                    : (IsMscoreeDll(baseName)
                        ? " (OpenExe; TOC[46]; do not invent a FILE)"
                        : " (LoadDriver; do not invent 0x81360000)")));
            TryMarkExtraRomO32Compressed(bus, tocEntry);
            return true;
        }

        // ExtraROM o32[0] first word B501743A / psize<vsize is CE
        // compressed, but flags 0x60002020 lack 0x4000.
        // CopyO32 0x8001B0F8 ands flags with 0x80002002 and, if
        // that is 0, requires dataptr page-aligned. 0x80764CE0
        // is not (off 0xCE0), so clearing 0x2000 on the ROM o32
        // makes CopyO32 return 193 before MapO32. Keep 0x2000 on
        // the ROM copy; clear it on o32_lite at MapO32 so
        // 0x80028844 decompresses onto the existing o32.real.
        // Do not host-alias XIP. Do not invent 0x81360000.
        public static void TryMarkExtraRomO32Compressed(MipsBus bus, uint tocEntry)
        {
            if (bus == null || tocEntry == 0)
                return;
            if (tocEntry != _ddiNopTocEntry && tocEntry != _mscoreeTocEntry
                && tocEntry != _ole32TocEntry)
                return;
            if (tocEntry == _mscoreeTocEntry)
                TryRestoreExtraRomMscoreeIfClobbered(bus);
            else if (tocEntry == _ole32TocEntry)
                TryRestoreExtraRomOle32IfClobbered(bus);
            else
                TryRestoreExtraRomIfClobbered(bus, tocEntry);
            uint e32 = 0;
            uint o32 = 0;
            try
            {
                uint attr = bus.Read32(tocEntry);
                uint name = bus.Read32(tocEntry + 0x10);
                e32 = bus.Read32(tocEntry + 0x14);
                o32 = bus.Read32(tocEntry + 0x18);
                string tag = tocEntry == _ole32TocEntry ? "TOC[34]"
                    : (tocEntry == _mscoreeTocEntry ? "TOC[46]" : "TOC[33]");
                uint cachedE32 = tocEntry == _ole32TocEntry ? _ole32E32
                    : (tocEntry == _mscoreeTocEntry ? _mscoreeE32 : _ddiNopE32);
                System.Console.WriteLine("[Hive] ExtraROM " + tag + " live entry=0x" +
                    tocEntry.ToString("X8") +
                    " attr=0x" + attr.ToString("X8") +
                    " name=0x" + name.ToString("X8") +
                    " e32=0x" + e32.ToString("X8") +
                    " o32=0x" + o32.ToString("X8") +
                    " cachedE32=0x" + cachedE32.ToString("X8"));
            }
            catch (System.Exception ex)
            {
                string tag = tocEntry == _ole32TocEntry ? "TOC[34]"
                    : (tocEntry == _mscoreeTocEntry ? "TOC[46]" : "TOC[33]");
                System.Console.WriteLine("[Hive] ExtraROM " + tag + " live entry=0x" +
                    tocEntry.ToString("X8") + " read-fail " + ex.Message);
                return;
            }
            try
            {
                if (e32 == 0 || o32 == 0)
                    return;
                uint objcnt = bus.Read32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return;
                for (uint s = 0; s < objcnt; s++)
                {
                    uint src = o32 + s * O32RomSize;
                    uint vsize = bus.Read32(src);
                    uint psize = bus.Read32(src + 8);
                    uint dataptr = bus.Read32(src + 0xC);
                    uint real = bus.Read32(src + 0x10);
                    uint flags = bus.Read32(src + 0x14);
                    if (!LooksCompressed(bus, dataptr, vsize, psize))
                        continue;
                    uint next = flags | O32Compressed;
                    if (next == flags)
                        continue;
                    bus.Write32(src + 0x14, next);
                    System.Console.WriteLine("[Hive] ExtraROM o32[" + s +
                        "] flags 0x" + flags.ToString("X8") +
                        " -> 0x" + next.ToString("X8") +
                        " dataptr=0x" + dataptr.ToString("X8") +
                        " real=0x" + real.ToString("X8") +
                        " (keep 0x2000 for CopyO32 align; MapO32 clears it)");
                }
            }
            catch
            {
            }
        }

        private static bool LooksCompressed(MipsBus bus, uint dataptr, uint vsize, uint psize)
        {
            if (bus == null || dataptr == 0 || vsize == 0 || psize == 0 || psize >= vsize)
                return false;
            try
            {
                uint first = bus.Read32(dataptr);
                uint declared = first & 0x00FFFFFFu;
                uint sig = first >> 24;
                return declared == vsize
                    || sig == 0xB5 || sig == 0xB4 || sig == 0x11 || sig == 0x0C;
            }
            catch
            {
                return psize < vsize;
            }
        }

        // Slot-1 o32.real 0x03981000 is the ExtraROM vbase. VALLOC
        // only MEM_COMMITs and the current process has no reservation
        // there (last-error 14). Use the same slot offset in slot 0
        // (0x01981000) so firmware can RESERVE|COMMIT, then
        // 0x80028844 writes those pages. Alias 0x0398xxxx to that
        // dest after VALLOC. Do not host-alias src XIP.
        public static void TrySteerExtraRomMapO32(MipsBus bus, uint o32Lite)
        {
            if (bus == null || o32Lite == 0)
                return;
            try
            {
                uint dest = bus.Read32(o32Lite + 8);
                uint dataptr = bus.Read32(o32Lite + 0x18);
                if (!IsExtraRomCompressedDest(dest) && !IsExtraRomCompressedData(dataptr))
                    return;
                uint slot = dest & SlotMask;
                if (slot == dest)
                    return;
                bus.Write32(o32Lite + 8, slot);
                System.Console.WriteLine("[Hive] ExtraROM MapO32 dest 0x" +
                    dest.ToString("X8") + " -> 0x" + slot.ToString("X8") +
                    " (slot-0 view of dump o32.real; firmware CEDecompressROM of dump LZX)");
            }
            catch
            {
            }
        }

        // wait63: dest 0x014B1000 dest-word=0. 0x8001AC9C
        // ands flags with 0x80002000; 0x60006020 leaves
        // 0x2000 so jal 0x80028844 is skipped. AD50 then
        // returns at 0x8001AE4C when object+6<2 and flags
        // lack 0x08000000 (type-7 never stores those).
        // ddi_nop keeps 0x2000 and VALLOCs because LoadDriver
        // set object+6>=2. Do not force that VALLOC.
        // CopyO32 already passed; clear O32RomXip on the
        // lite only so 0x8001AC9C falls through. a3!=0
        // still hits 0x8001ACB0 and skips the jal: leave
        // flags alone. Do not invent dest bytes.
        public static void TryClearO32RomXipForMscoree(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null || regs.Length <= 7)
                return;
            uint o32Lite = regs[5];
            if (o32Lite == 0)
                return;
            try
            {
                uint dest = bus.Read32(o32Lite + 8);
                uint dataptr = bus.Read32(o32Lite + 0x18);
                uint flags = bus.Read32(o32Lite + 0x10);
                if (!IsExtraRomMscoreeDest(dest) && !IsExtraRomMscoreeData(dataptr)
                    && !IsExtraRomOle32Dest(dest) && !IsExtraRomOle32Data(dataptr))
                    return;
                uint a3 = regs[7];
                uint obj = regs[4];
                uint obj6 = 0;
                uint type = 0;
                if (obj != 0)
                {
                    obj6 = (uint)(bus.Read8(obj + 6) | (bus.Read8(obj + 7) << 8));
                    type = bus.Read8(obj + 4);
                }
                uint gate = flags & 0x80002000u;
                System.Console.WriteLine("[Hive] ExtraROM MapO32 0x8001AC9C dest=0x" +
                    dest.ToString("X8") + " flags=0x" + flags.ToString("X8") +
                    " &0x80002000=0x" + gate.ToString("X") +
                    " a3=0x" + a3.ToString("X8") +
                    " type=" + type +
                    " object+6=" + obj6 +
                    (gate != 0
                        ? " (skip jal 0x80028844; 0x2000 set)"
                        : " (jal 0x80028844 if a3==0 and type bit2)"));
                if (a3 != 0)
                {
                    System.Console.WriteLine("[Hive] ExtraROM MapO32 dest=0x" +
                        dest.ToString("X8") +
                        " a3!=0 (0x8001ACB0 would still skip jal; leave 0x2000; no VALLOC)");
                    return;
                }
                if ((flags & O32RomXip) == 0)
                    return;
                uint next = flags & ~O32RomXip;
                bus.Write32(o32Lite + 0x10, next);
                System.Console.WriteLine("[Hive] ExtraROM MapO32 clear-xip dest=0x" +
                    dest.ToString("X8") + " flags 0x" + flags.ToString("X8") +
                    " -> 0x" + next.ToString("X8") +
                    " (o32_lite only; jal 0x80028844; dump LZX; no VALLOC)");
            }
            catch
            {
            }
        }

        // 0x80028844 is a0=dest a1=dataptr a2=vsize. Same
        // CEDecompressROM as ddi_nop VirtualCopy. TOC[46]
        // and TOC[34] dests. ddi_nop keeps 0x2000 and
        // VALLOC+VirtualCopy.
        public static bool TryRedirectExtraRomMapO32Decompress(
            MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 23)
                return false;
            uint dest = regs[4];
            uint src = regs[5];
            uint vsize = regs[6];
            if (!IsExtraRomMscoreeDest(dest) && !IsExtraRomMscoreeData(src)
                && !IsExtraRomOle32Dest(dest) && !IsExtraRomOle32Data(src))
                return false;
            uint o32Lite = regs[23];
            uint psize = 0;
            try
            {
                if (o32Lite != 0)
                {
                    if (vsize == 0)
                        vsize = bus.Read32(o32Lite);
                    psize = bus.Read32(o32Lite + 0x14);
                    if (src == 0)
                        src = bus.Read32(o32Lite + 0x18);
                }
            }
            catch
            {
                return false;
            }
            if (psize == 0 || vsize == 0)
                return false;
            regs[4] = src;
            regs[5] = psize;
            regs[6] = dest;
            regs[7] = vsize;
            System.Console.WriteLine("[Hive] ExtraROM MapO32 0x80028844 -> CEDecompressROM dest=0x" +
                dest.ToString("X8") + " src=0x" + src.ToString("X8") +
                " vsize=0x" + vsize.ToString("X") +
                " psize=0x" + psize.ToString("X") +
                " (dump LZX; same 0x8004DBF8 as ddi_nop; no VALLOC)");
            return TryRedirectExtraRomVirtualCopyToDecompress(bus, regs, ref programCounter);
        }

        public static void TryLogMscoreeMapO32Ret(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null || regs.Length <= 20)
                return;
            uint dest = regs[20];
            if (dest == 0 || (!IsExtraRomMscoreeDest(dest) && !IsExtraRomOle32Dest(dest)))
                return;
            uint word = 0;
            uint word4 = 0;
            bool mapped = false;
            try
            {
                word = bus.Read32(dest);
                word4 = bus.Read32(dest + 4);
                mapped = true;
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] ExtraROM MapO32 ret dest=0x" +
                dest.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " dest+4=0x" + word4.ToString("X8") +
                (mapped && (word != 0 || word4 != 0)
                    ? " (firmware dest after MapO32)"
                    : " (dest still empty)"));
            // wait90: RI dest-word 0x603E984F at RVA 0x7DA8.
            // o32[0] dest 0x014B1000 is RVA 0x1000. Peek the
            // same CEDecompressROM dest at RVA 0x7D7C / 0x7DA8
            // / startip 0x9D98. Do not write dest. Do not
            // alias that dest a second time.
            if (mapped && IsExtraRomMscoreeDest(dest)
                && (dest & SlotMask) == 0x014B1000u)
            {
                uint jal = 0;
                uint ri = 0;
                uint startip = 0;
                TryPeekWord(bus, dest + 0x6D7Cu, out jal);
                TryPeekWord(bus, dest + 0x6DA8u, out ri);
                TryPeekWord(bus, dest + 0x8D98u, out startip);
                System.Console.WriteLine("[Hive] ExtraROM MapO32 mscoree o32[0] dest=0x" +
                    dest.ToString("X8") +
                    " rva7D7C=0x" + jal.ToString("X8") +
                    " rva7DA8=0x" + ri.ToString("X8") +
                    " rva9D98=0x" + startip.ToString("X8") +
                    " (peek only; same CEDecompressROM dest as startip; do not invent dest bytes; not a second alias)");
            }
        }

        // kseg0 scratch for an aligned copy of ExtraROM compressed
        // o32. 0x80028844 xors dest^src and requires the page
        // offsets to match; dataptr 0x80764CE0 is off 0xCE0.
        // This is not ExtraROM and not 0x81360000.
        public const uint AlignedCompSrc = 0x8F000000;
        public const uint AlignedCompStride = 0x10000;
        // VALLOC dest is useg. CEDecompress lbu/sb that VA
        // before the first store, so DestReadable is false and
        // lookbacks TLB-miss. Host-back the already-VALLOC'd
        // pages at kseg0 (zeros only). Not ExtraROM XIP and
        // not 0x81360000.
        public const uint ExtraRomDestKseg0 = 0x8F100000;
        public const uint ExtraRomDestKseg1 = 0x8F180000;
        // wait62: TOC[46] slot-0 view of dump o32.real
        // 0x034B1000 / 0x034Cxxxx. Not 0x81360000.
        public const uint ExtraRomDestKsegMscoree = 0x8F1A0000;
        public const uint ExtraRomDestKsegMscoree1 = 0x8F1C0000;
        // wait65: TOC[34] slot-0 view of dump o32.real
        // 0x03941000 / 0x03972000. Not 0x81360000.
        // 0x8F080000 is ole32 aligned-src slot 8
        // (0x8F000000 + 8*0x10000). HostCommit of dest
        // 0x01941000 at that kseg zeroed psize 0x17BDC
        // and CEDecompressROM page 3 was v0=4.
        public const uint ExtraRomDestKsegOle32 = 0x8F0C0000;
        // Firmware VirtualAlloc(NULL) useg must not alias kseg0
        // 0x80000000|va: 0x000E1700 would be NK at 0x800E1700.
        // Dedicated unused kseg0, same class as ExtraROM dest.
        public const uint VallocHostKseg = 0x8F200000;
        public const uint VallocHostKsegLim = 0x8F400000;
        public const uint CeAllocGranularity = 0x10000;

        public static bool TryReserveExtraRomValloc(uint[] regs)
        {
            if (regs == null || regs.Length <= 6)
                return false;
            uint dest = regs[4];
            if (!IsExtraRomCompressedDest(dest))
                return false;
            // o32[0].real is vbase+0x1000. BindImp reads IMP
            // at vbase+NameRVA. VALLOC of dest alone leaves
            // the header page unmapped. Pull dest down one
            // page. Do not invent a PE header.
            uint slot = dest & SlotMask;
            uint header = 0;
            if (IsExtraRomHeaderDestPage(slot & 0xFFFFF000u))
            {
                header = 0x1000;
                dest -= header;
                regs[4] = dest;
            }
            uint type = regs[6];
            bool needReserve = (type & MemReserve) == 0;
            if (needReserve)
                regs[6] = type | MemReserve;
            // CEDecompress step 0x1000 can lbu the next dest page
            // (section 2 vsize 0xB04 read 0x019A9000 and took
            // 0x80000180). Commit one extra page. Not ExtraROM XIP.
            if (regs.Length > 5)
            {
                uint size = regs[5] + header;
                uint pages = (size + 0xFFFu) & ~0xFFFu;
                if (pages < size + 0x1000)
                    pages += 0x1000;
                if (pages > regs[5])
                    regs[5] = pages;
            }
            if (!needReserve && header == 0)
                return false;
            System.Console.WriteLine("[Hive] ExtraROM VALLOC a0=0x" +
                dest.ToString("X8") + " type 0x" + type.ToString("X") +
                " -> 0x" + regs[6].ToString("X") +
                " size 0x" + (regs.Length > 5 ? regs[5].ToString("X") : "0") +
                (header != 0 ? " (vbase header page + extra; do not invent 0x81360000)"
                    : " (MEM_RESERVE|COMMIT + extra page; do not invent 0x81360000)"));
            return true;
        }

        // 0x80026F50 returns how many NEW pages it committed.
        // VALLOC already committed ExtraROM dest, so that is 0.
        // 0x800289F8 bne v0, s4 then last-error 87. s4 is the
        // page count (dest+vsize). Keep the VALLOC pages.
        public static bool TryAcceptExtraRomDestCommit(uint[] regs)
        {
            if (regs == null || regs.Length <= 30)
                return false;
            uint dest = regs[30];
            if (!IsExtraRomCompressedDest(dest))
                return false;
            uint v0 = regs[2];
            uint pages = regs[20];
            if (v0 != 0 || pages == 0 || pages > 0x100)
                return false;
            regs[2] = pages;
            System.Console.WriteLine("[Hive] ExtraROM 0x80026F50 v0=0 pages=" +
                pages + " dest=0x" + dest.ToString("X8") +
                " (VALLOC already committed; do not invent 0x81360000)");
            return true;
        }

        public static void NoteExtraRomVallocRet(uint dest, uint v0)
        {
            if (!IsExtraRomCompressedDest(dest))
                return;
            System.Console.WriteLine("[Hive] ExtraROM VALLOC dest=0x" +
                dest.ToString("X8") + " v0=0x" + v0.ToString("X8") +
                (v0 == 0 ? " (firmware miss)" : " (slot-0 dest ready)"));
            if (v0 != 0)
            {
                if (IsExtraRomDdiNopDest(dest))
                {
                    _ddiNopDestOn = true;
                    _ddiNopSlot0 = DdiNopVbase & SlotMask;
                }
                if (IsExtraRomMscoreeDest(dest))
                {
                    _mscoreeDestOn = true;
                    if (_mscoreeVbase != 0)
                        _mscoreeSlot0 = _mscoreeVbase & SlotMask;
                }
                if (IsExtraRomOle32Dest(dest))
                {
                    _ole32DestOn = true;
                    if (_ole32Vbase != 0)
                        _ole32Slot0 = _ole32Vbase & SlotMask;
                }
            }
        }

        // MapO32 VALLOCs dest only when flags keep 0x2000 (the early
        // 0x80028844 path does not). After that VALLOC it VirtualCopys
        // compressed ExtraROM bytes as XIP. 0x80028844 is a PTE remap
        // (kseg0 src takes the XIP shortcut and dest stays zeros).
        // Rewrite that jal to kernel CEDecompressROM so
        // firmware expands the real ExtraROM LZX pages onto
        // the VALLOC dest. Do not host-alias XIP. Do not
        // invent 0x81360000. Do not jal CE3 0x80050974.
        private static uint _ddiNopDecompRa;
        private static uint _ddiNopDecompDest;
        private static uint _ddiNopDecompVsize;
        private static bool _ddiNopInnerCap;
        private static int _ddiNopInnerPages;

        public static bool TryRedirectExtraRomVirtualCopyToDecompress(
            MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 7)
                return false;
            uint src = regs[4];
            uint psize = regs[5];
            uint dest = regs[6];
            uint vsize = regs[7];
            if (!IsExtraRomCompressedDest(dest) && !IsExtraRomCompressedData(src))
                return false;
            if (psize == 0 || psize > 0x200000 || vsize == 0 || vsize > 0x200000)
                return false;
            uint aligned = CopyExtraRomSrcPageAligned(bus, src, psize);
            if (aligned != 0)
                src = aligned;
            if (IsExtraRomMscoreeDest(dest) || IsExtraRomMscoreeData(src))
            {
                _mscoreeDestOn = true;
                if (_mscoreeVbase != 0)
                    _mscoreeSlot0 = _mscoreeVbase & SlotMask;
            }
            if (IsExtraRomOle32Dest(dest) || IsExtraRomOle32Data(src))
            {
                _ole32DestOn = true;
                if (_ole32Vbase != 0)
                    _ole32Slot0 = _ole32Vbase & SlotMask;
            }
            HostCommitExtraRomDest(bus, dest, vsize);
            // ExtraROM first word is [size0][size1][size2][b0].
            // Kernel 0x80050A10 takes the 3-byte LE size, then
            // 3-byte page offsets starting at src+3. Byte 3 is
            // the low byte of the first offset (0xB5 08 00 =
            // 0x8B5), not a type to drop. Dropping it made
            // every offset 0xDD0008-style and left entry/ImpHdr
            // empty (BindImp LoadLibrary "").
            regs[4] = src;
            regs[5] = psize;
            regs[6] = dest;
            regs[7] = vsize;
            if (regs.Length > 29)
            {
                try
                {
                    uint sp = regs[29];
                    bus.Write32(sp + 16, 0);
                    bus.Write32(sp + 20, 1);
                    bus.Write32(sp + 24, 0x1000);
                }
                catch
                {
                }
            }
            programCounter = BinaryDecompressRom;
            _ddiNopDecompRa = regs.Length > 31 ? regs[31] : 0;
            _ddiNopDecompDest = dest;
            _ddiNopDecompVsize = vsize;
            _ddiNopInnerCap = false;
            _ddiNopInnerPages = 0;
            uint first = 0;
            uint page0 = 0;
            try
            {
                first = bus.Read32(src);
            }
            catch
            {
            }
            try
            {
                // 3-byte size then 3-byte offsets. First LZX
                // block header sits at the first page-offset
                // (byte 3..5 = 0x8B5 for ddi_nop o32[0]; the
                // table length is (pages+2)*3).
                uint size3 = first & 0xFFFFFFu;
                uint n = ((size3 >> 12) + 2) * 3;
                if (n >= 6 && n < psize)
                    page0 = bus.Read32(src + n);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] ExtraROM VALLOC dest then CEDecompressROM dest=0x" +
                dest.ToString("X8") + " src=0x" + src.ToString("X8") +
                " vsize=0x" + vsize.ToString("X") +
                " psize=0x" + psize.ToString("X") +
                " src0=0x" + first.ToString("X8") +
                " page0=0x" + page0.ToString("X8") +
                " dest-" + (DestReadable(bus, dest) ? "mapped" : "unmapped") +
                " (firmware 0x8004DBF8 skip=0 convert=1 step=0x1000; LZX window at page0; keep ExtraROM first word)");
            return true;
        }

        public static bool TryNoteExtraRomInnerDest(MipsBus bus, uint[] regs)
        {
            if ((_ddiNopDecompRa == 0 && _tv2FileDecompRa == 0)
                || bus == null || regs == null || regs.Length <= 7)
                return false;
            try
            {
                uint src = regs[4];
                uint slen = regs[5];
                uint dest = regs[6];
                uint work = regs[7];
                uint leftover = 0;
                uint src0 = 0;
                if (work != 0)
                    leftover = bus.Read32(work);
                if (src != 0)
                    src0 = bus.Read32(src);
                if (!_ddiNopInnerCap)
                {
                    _ddiNopInnerCap = true;
                    System.Console.WriteLine("[Hive] ExtraROM CEDecompressROM inner src=0x" +
                        src.ToString("X8") + " slen=0x" + slen.ToString("X") +
                        " dest=0x" + dest.ToString("X8") +
                        " leftover=0x" + leftover.ToString("X") +
                        " src0=0x" + src0.ToString("X8") +
                        " (LZX window/vsize header; do not cap leftover)");
                }
            }
            catch
            {
            }
            return false;
        }

        public static bool TryNoteExtraRomInnerRet(uint[] regs)
        {
            if ((_ddiNopDecompRa == 0 && _tv2FileDecompRa == 0)
                || regs == null || regs.Length <= 2)
                return false;
            // TOC[34] o32[0] vsize 0x2E705 is 47 pages.
            if (_ddiNopInnerPages >= 48)
                return false;
            _ddiNopInnerPages++;
            uint v0 = regs[2];
            uint page = regs.Length > 23 ? regs[23] : 0;
            uint total = regs.Length > 21 ? regs[21] : 0;
            System.Console.WriteLine("[Hive] ExtraROM CEDecompressROM inner v0=0x" +
                v0.ToString("X8") + " page=" + page +
                " total=0x" + total.ToString("X") +
                (v0 == 0 ? " (LZX page ok)" :
                    v0 == 3 ? " (window/leftover miss)" :
                    v0 == 4 ? " (bad LZX window)" :
                    (int)v0 < 0 ? " (ROM inner fail)" : " (ROM inner status)"));
            return false;
        }

        public static bool TryNoteExtraRomDecompressRet(MipsBus bus, uint[] regs, uint pc)
        {
            if (_ddiNopDecompRa == 0 || pc != _ddiNopDecompRa)
                return false;
            uint dest = _ddiNopDecompDest;
            uint vsize = _ddiNopDecompVsize;
            _ddiNopDecompRa = 0;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint word = 0;
            uint entry = 0;
            bool mapped = false;
            bool entryMapped = false;
            try
            {
                if (bus != null && dest != 0)
                {
                    word = bus.Read32(dest);
                    mapped = true;
                }
            }
            catch
            {
            }
            try
            {
                // entryrva 0x18014 is dest+0x17014 (o32[0] rva 0x1000).
                if (bus != null && dest != 0 && vsize > 0x17014)
                {
                    entry = bus.Read32(dest + 0x17014);
                    entryMapped = true;
                }
            }
            catch
            {
            }
            string imp = "";
            if (bus != null && dest != 0 && vsize > 0x17370)
            {
                try
                {
                    uint lookup = bus.Read32(dest + 0x17350);
                    uint nameRva = bus.Read32(dest + 0x1735C);
                    string dll = "";
                    if (nameRva >= 0x1000 && nameRva < 0x1843Au)
                        dll = ReadAscii(bus, dest + (nameRva - 0x1000));
                    imp = " imp0=0x" + lookup.ToString("X8") +
                        " nameRVA=0x" + nameRva.ToString("X") +
                        (dll.Length > 0 ? " \"" + dll + "\"" : "");
                }
                catch
                {
                }
            }
            string note;
            if (v0 == 0xFFFFFFFFu)
                note = " (firmware CEDecompressROM miss)";
            else if (vsize != 0 && v0 == vsize)
                note = " (firmware expanded vsize)";
            else if (v0 == 0)
                note = " (firmware returned 0)";
            else
                note = "";
            System.Console.WriteLine("[Hive] ExtraROM CEDecompressROM ret v0=0x" +
                v0.ToString("X8") + " dest=0x" + dest.ToString("X8") +
                (mapped ? " word=0x" + word.ToString("X8") : " dest-unmapped") +
                (entryMapped ? " entry=0x" + entry.ToString("X8") : "") +
                imp +
                note);
            if (bus != null && dest == 0x01981000u && v0 == vsize)
                DumpDdiNopTextSites(bus, dest);
            return false;
        }

        // DllMain TLB epc 0x03981520 is dest+0x520 (rva 0x1520).
        // 0x000E1970 is not an o32 RVA (e32 vsize 0x2B000) and not
        // sec1 BSS (0x01F57xxx / leftover after 0xAB28). nk TOC[7]
        // gwes.exe e32 vbase 0x00010000 vsize 0xBB000 ends
        // 0x000CB000 (o32[3] real 0x000C6000+0x42C4). ExtraROM
        // has no module or B000FF record in 0x000E0000-0x000F0000.
        // 0x000E1970 / 0x000E1700 are not LE words in nk.bin or
        // etc.bin. Observe the store. Do not invent 0x000E0000.
        private static void DumpDdiNopTextSites(MipsBus bus, uint dest)
        {
            uint[] off = { 0x520, 0x1D50, 0x1DD4, 0x5FA8, 0x70C4, 0x70E4, 0x17014, 0x170F0 };
            for (int i = 0; i < off.Length; i++)
            {
                try
                {
                    uint va = dest + off[i];
                    uint w0 = bus.Read32(va);
                    uint w1 = bus.Read32(va + 4);
                    uint w2 = bus.Read32(va + 8);
                    System.Console.WriteLine("[Hive] ExtraROM ddi_nop dest+0x" +
                        off[i].ToString("X") + " @0x" + va.ToString("X8") +
                        " " + w0.ToString("X8") + " " + w1.ToString("X8") +
                        " " + w2.ToString("X8"));
                }
                catch
                {
                }
            }
        }

        private static bool _ddiNopBindHdr;
        private static bool _ddiNopBindName;
        private static bool _ddiNopBindLib;
        private static bool _ddiNopBindLibRet;

        public static bool TryNoteExtraRomBindImp(MipsBus bus, uint[] regs, uint pc)
        {
            if (regs == null || regs.Length <= 30)
                return false;
            if (pc == BindImpHdr && !_ddiNopBindHdr)
            {
                _ddiNopBindHdr = true;
                uint hdr = regs[20];
                uint vbase = regs[22];
                uint e32 = regs[23];
                uint impRva = 0;
                uint impSize = 0;
                uint w0 = 0;
                uint nameRva = 0;
                try
                {
                    if (e32 != 0)
                    {
                        impRva = bus != null ? bus.Read32(e32 + 0x24) : 0;
                        impSize = bus != null ? bus.Read32(e32 + 0x28) : 0;
                    }
                    if (bus != null && hdr != 0)
                    {
                        w0 = bus.Read32(hdr);
                        nameRva = bus.Read32(hdr + 12);
                    }
                }
                catch
                {
                }
                string dll = "";
                try
                {
                    if (bus != null && nameRva != 0)
                        dll = ReadAscii(bus, vbase + nameRva);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] ExtraROM BindImp hdr=0x" +
                    hdr.ToString("X8") + " vbase=0x" + vbase.ToString("X8") +
                    " e32IMP=0x" + impRva.ToString("X") + "/0x" + impSize.ToString("X") +
                    " word0=0x" + w0.ToString("X8") +
                    " nameRVA=0x" + nameRva.ToString("X") +
                    (dll.Length > 0 ? " \"" + dll + "\"" : " (name unread)") +
                    " (do not invent 0x81360000)");
                return false;
            }
            if (pc == BindImpDllName && !_ddiNopBindName)
            {
                _ddiNopBindName = true;
                uint nameVa = regs[3];
                string dll = "";
                try
                {
                    if (bus != null && nameVa != 0)
                        dll = ReadAscii(bus, nameVa);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] ExtraROM BindImp nameVA=0x" +
                    nameVa.ToString("X8") +
                    (dll.Length > 0 ? " \"" + dll + "\"" : " (empty or unmapped)") +
                    " (LoadLibrary of this import; 126 is this miss)");
                return false;
            }
            if (pc == BindImpLoadLib && _ddiNopBindHdr && !_ddiNopBindLib)
            {
                _ddiNopBindLib = true;
                uint a0 = regs[4];
                string dll = "";
                try
                {
                    if (bus != null && a0 != 0)
                        dll = ReadUtf16Name(bus, a0);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] ExtraROM BindImp LoadLibrary \"" +
                    (dll.Length > 0 ? dll : "(empty)") +
                    "\" a0=0x" + a0.ToString("X8"));
                return false;
            }
            if (pc == BindImpLoadLibRet && _ddiNopBindLib && !_ddiNopBindLibRet)
            {
                _ddiNopBindLibRet = true;
                uint v0 = regs[2];
                System.Console.WriteLine("[Hive] ExtraROM BindImp LoadLibrary ret v0=0x" +
                    v0.ToString("X8") +
                    (v0 == 0 ? " (import miss; last-error 126)" : " (import loaded)"));
                return false;
            }
            return false;
        }

        private static void HostCommitExtraRomDest(MipsBus bus, uint dest, uint vsize)
        {
            if (bus == null || dest == 0 || vsize == 0)
                return;
            uint kseg = 0;
            uint off = 0;
            if (dest >= 0x01980000u && dest < 0x019B0000u)
            {
                kseg = ExtraRomDestKseg0;
                off = dest - 0x01980000u;
            }
            else if (dest >= 0x01F57000u && dest < 0x01F67000u)
            {
                kseg = ExtraRomDestKseg1;
                off = dest - 0x01F57000u;
            }
            else if (dest >= 0x014B0000u && dest < 0x014D0000u)
            {
                kseg = ExtraRomDestKsegMscoree;
                off = dest - 0x014B0000u;
            }
            else if (dest >= 0x01F32000u && dest < 0x01F33000u)
            {
                kseg = ExtraRomDestKsegMscoree1;
                off = dest - 0x01F32000u;
            }
            else if (dest >= 0x01940000u && dest < 0x01980000u)
            {
                kseg = ExtraRomDestKsegOle32;
                off = dest - 0x01940000u;
            }
            if (kseg == 0)
                return;
            try
            {
                uint n = (vsize + 0x1FFFu) & ~0xFFFu;
                for (uint i = 0; i < n; i += 4)
                    bus.Write32(kseg + off + i, 0);
            }
            catch
            {
            }
        }

        private static uint CopyExtraRomSrcPageAligned(MipsBus bus, uint src, uint psize)
        {
            if (bus == null || src == 0 || psize == 0 || psize > 0x20000)
                return 0;
            if ((src & 0xFFF) == 0)
                return src;
            int slot = -1;
            uint[][] cache = null;
            int baseSlot = 0;
            if (_mscoreeDataPtr != null)
            {
                for (int s = 0; s < _mscoreeDataPtr.Length; s++)
                {
                    if (_mscoreeDataPtr[s] == src)
                    {
                        slot = s;
                        cache = _mscoreeData;
                        baseSlot = 4;
                        break;
                    }
                }
            }
            if (slot < 0 && _ole32DataPtr != null)
            {
                for (int s = 0; s < _ole32DataPtr.Length; s++)
                {
                    if (_ole32DataPtr[s] == src)
                    {
                        slot = s;
                        cache = _ole32Data;
                        baseSlot = 8;
                        break;
                    }
                }
            }
            if (slot < 0 && _ddiNopDataPtr != null)
            {
                for (int s = 0; s < _ddiNopDataPtr.Length; s++)
                {
                    if (_ddiNopDataPtr[s] == src)
                    {
                        slot = s;
                        cache = _ddiNopData;
                        break;
                    }
                }
            }
            if (slot < 0)
                slot = 0;
            uint dest = AlignedCompSrc + (uint)(baseSlot + slot) * AlignedCompStride;
            try
            {
                uint[] blob = null;
                if (cache != null && slot < cache.Length)
                    blob = cache[slot];
                uint n = (psize + 3) / 4;
                if (blob != null && blob.Length < n)
                    n = (uint)blob.Length;
                for (uint w = 0; w < n; w++)
                {
                    uint word = blob != null && w < blob.Length
                        ? blob[w]
                        : bus.Read32(src + w * 4);
                    bus.Write32(dest + w * 4, word);
                }
                return dest;
            }
            catch
            {
                return 0;
            }
        }

        private static uint ExtraRomO32Access(MipsBus bus, uint[] regs)
        {
            uint o32 = regs != null && regs.Length > 23 ? regs[23] : 0;
            uint access = 0;
            uint flags = 0;
            try
            {
                if (bus != null && o32 != 0)
                {
                    access = bus.Read32(o32 + 0xC);
                    flags = bus.Read32(o32 + 0x10);
                }
            }
            catch
            {
            }
            uint page = access & 0xFF;
            if (page == 1 || page == 2 || page == 4 || page == 8
                || page == 0x10 || page == 0x20 || page == 0x40 || page == 0x80)
                return page;
            if ((flags & 0x80000000u) != 0)
                return 0x40;
            return 0x20;
        }

        private static bool DestReadable(MipsBus bus, uint dest)
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

        private static bool IsExtraRomDdiNopDest(uint dest)
        {
            uint slot = dest & SlotMask;
            return (dest >= DdiNopVbase && dest < 0x039B0000u)
                || (slot >= 0x01980000u && slot < 0x019B0000u)
                || (dest >= 0x01F57000u && dest < 0x01F66000u);
        }

        private static bool IsExtraRomDdiNopData(uint dataptr)
        {
            // ole32 o32[0] dataptr 0x807752F4 sits past ddi_nop
            // o32[2]. Do not treat that dump blob as ddi_nop.
            if (IsExtraRomOle32Data(dataptr))
                return false;
            return dataptr >= 0x80764CE0u && dataptr < 0x807752F4u;
        }

        // wait62: TOC[46] dump o32.real / dataptr. Not invented.
        private static bool IsExtraRomMscoreeDest(uint dest)
        {
            if (dest == 0 || _mscoreeO32Words == null)
                return false;
            uint slot = dest & SlotMask;
            for (int s = 0; s + 5 < _mscoreeO32Words.Length; s += 6)
            {
                uint vsize = _mscoreeO32Words[s];
                uint rva = _mscoreeO32Words[s + 1];
                uint real = _mscoreeO32Words[s + 4];
                if (real == 0)
                    continue;
                uint span = vsize == 0 ? 0x1000u : ((vsize + 0xFFFu) & ~0xFFFu);
                if (span < 0x1000)
                    span = 0x1000;
                uint loSlot = real & SlotMask;
                if ((dest >= real && dest < real + span)
                    || (slot >= loSlot && slot < loSlot + span))
                    return true;
                if (rva == 0x1000 && real >= 0x1000)
                {
                    uint vbase = real - 0x1000;
                    uint vbaseSlot = vbase & SlotMask;
                    if (dest == vbase || dest == vbaseSlot || slot == vbaseSlot)
                        return true;
                }
            }
            return false;
        }

        private static bool IsExtraRomMscoreeData(uint dataptr)
        {
            if (dataptr == 0 || _mscoreeDataPtr == null)
                return false;
            for (int s = 0; s < _mscoreeDataPtr.Length; s++)
            {
                uint p = _mscoreeDataPtr[s];
                if (p == 0)
                    continue;
                if (dataptr == p)
                    return true;
                uint n = _mscoreeDataLen != null && s < _mscoreeDataLen.Length
                    ? _mscoreeDataLen[s] : 0;
                if (n != 0 && dataptr > p && dataptr < p + n)
                    return true;
            }
            return false;
        }

        // wait65: TOC[34] dump o32.real / dataptr. Not invented.
        private static bool IsExtraRomOle32Dest(uint dest)
        {
            if (dest == 0 || _ole32O32Words == null)
                return false;
            uint slot = dest & SlotMask;
            for (int s = 0; s + 5 < _ole32O32Words.Length; s += 6)
            {
                uint vsize = _ole32O32Words[s];
                uint rva = _ole32O32Words[s + 1];
                uint real = _ole32O32Words[s + 4];
                if (real == 0)
                    continue;
                uint span = vsize == 0 ? 0x1000u : ((vsize + 0xFFFu) & ~0xFFFu);
                if (span < 0x1000)
                    span = 0x1000;
                uint loSlot = real & SlotMask;
                if ((dest >= real && dest < real + span)
                    || (slot >= loSlot && slot < loSlot + span))
                    return true;
                if (rva == 0x1000 && real >= 0x1000)
                {
                    uint vbase = real - 0x1000;
                    uint vbaseSlot = vbase & SlotMask;
                    if (dest == vbase || dest == vbaseSlot || slot == vbaseSlot)
                        return true;
                }
            }
            return false;
        }

        private static bool IsExtraRomOle32Data(uint dataptr)
        {
            if (dataptr == 0 || _ole32DataPtr == null)
                return false;
            for (int s = 0; s < _ole32DataPtr.Length; s++)
            {
                uint p = _ole32DataPtr[s];
                if (p == 0)
                    continue;
                if (dataptr == p)
                    return true;
                uint n = _ole32DataLen != null && s < _ole32DataLen.Length
                    ? _ole32DataLen[s] : 0;
                if (n != 0 && dataptr > p && dataptr < p + n)
                    return true;
            }
            return false;
        }

        private static bool IsExtraRomCompressedDest(uint dest)
        {
            return IsExtraRomDdiNopDest(dest) || IsExtraRomMscoreeDest(dest)
                || IsExtraRomOle32Dest(dest);
        }

        private static bool IsExtraRomCompressedData(uint dataptr)
        {
            return IsExtraRomDdiNopData(dataptr) || IsExtraRomMscoreeData(dataptr)
                || IsExtraRomOle32Data(dataptr);
        }

        private static bool IsExtraRomHeaderDestPage(uint slotPage)
        {
            if (slotPage == 0x01981000u || slotPage == 0x01941000u)
                return true;
            if (_mscoreeO32Words == null || _mscoreeO32Words.Length < 6)
                return false;
            uint rva = _mscoreeO32Words[1];
            uint real = _mscoreeO32Words[4];
            if (rva != 0x1000 || real == 0)
                return false;
            return (real & SlotMask & 0xFFFFF000u) == slotPage;
        }

        public static bool IsDdiNopTocObject(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0 || _ddiNopTocEntry == 0)
                return false;
            try
            {
                return bus.Read32(obj) == _ddiNopTocEntry
                    && bus.Read8(obj + 4) == TocAttachType;
            }
            catch
            {
                return false;
            }
        }

        public static uint DdiNopTocEntry
        {
            get { return _ddiNopTocEntry; }
        }

        public static bool IsMscoreeTocObject(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0 || _mscoreeTocEntry == 0)
                return false;
            try
            {
                return bus.Read32(obj) == _mscoreeTocEntry
                    && bus.Read8(obj + 4) == TocAttachType;
            }
            catch
            {
                return false;
            }
        }

        public static uint MscoreeTocEntry
        {
            get { return _mscoreeTocEntry; }
        }

        public static uint MscoreeE32
        {
            get { return _mscoreeE32; }
        }

        public static bool IsOle32TocObject(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0 || _ole32TocEntry == 0)
                return false;
            try
            {
                return bus.Read32(obj) == _ole32TocEntry
                    && bus.Read8(obj + 4) == TocAttachType;
            }
            catch
            {
                return false;
            }
        }

        public static uint Ole32TocEntry
        {
            get { return _ole32TocEntry; }
        }

        public static uint Ole32E32
        {
            get { return _ole32E32; }
        }

        public static void NoteExtraRom(uint imageStart)
        {
            _extraRomStart = imageStart;
            _extraRomHdr = 0;
            _pendingRomFile = null;
            _ddiNopTocEntry = 0;
            _ddiNopAttr = 0;
            _ddiNopTocWords = null;
            _ddiNopE32 = 0;
            _ddiNopE32Words = null;
            _ddiNopO32 = 0;
            _ddiNopO32Words = null;
            _ddiNopDataPtr = null;
            _ddiNopDataLen = null;
            _ddiNopData = null;
            _ddiNopDestOn = false;
            _ddiNopSlot0 = 0;
            _mscoreeDestOn = false;
            _mscoreeSlot0 = 0;
            _mscoreeVbase = 0;
            _ole32DestOn = false;
            _ole32Slot0 = 0;
            _ole32Vbase = 0;
            _ddiNopDecompRa = 0;
            _ddiNopDecompDest = 0;
            _ddiNopDecompVsize = 0;
            _ddiNopInnerCap = false;
            _ddiNopInnerPages = 0;
            _ddiNopBindHdr = false;
            _ddiNopBindName = false;
            _ddiNopBindLib = false;
            _ddiNopBindLibRet = false;
            _tv2FileEntry = 0;
            _tv2FileWords = null;
            _tv2FileName = 0;
            _tv2FileNameWords = null;
            _tv2FileReal = 0;
            _tv2FileComp = 0;
            _tv2FileLoad = 0;
            _tv2FileData = null;
            _tv2FileDecompRa = 0;
            _tv2FileSavedSp = 0;
            _tv2FilePos = 0;
            _tv2FileDestOn = false;
            _tv2FileIoLogged = false;
            _tv2PeImageVa = 0;
            _tv2PeImageBytes = 0;
            _tv2PeVallocRa = 0;
            _tv2BindLogged = false;
            _tv2PeEntryRva = 0;
            _tv2PeImageBase = 0;
            _tv2PeComRva = 0;
            _tv2Proc = 0;
            _tv2Startip = 0;
            _tv2Thread = 0;
            _tv2FetchLogged = false;
            _tv2ContinueLogged = false;
            _tv2ExnHelperLogged = false;
            _tv2PostFetchExnLogged = false;
            _tv2ImplAdelLogged = false;
            _tv2AfterExnContLogged = false;
            _pteMapBusy = false;
            _pteMapLogged = false;
            _slot2MapLogged = false;
            _slot0InfoMapLogged = false;
            _slot0FetchMapLogged = false;
            _tv2CoredllLogged = false;
            _tv2CoredllContLogged = false;
            _coredllLiveSec = 0;
            _coredllLiveLogged = false;
            _coredllMapLogged = false;
            _coredllHighLogged = false;
            _coredllZeroLogged = false;
            _tv2ZeroContLogged = false;
            _tv2HighContLogged = false;
            _tv2ImplRa = 0;
            _tv2ImplResume = 0;
            _tv2ImplEpc = 0;
            _tv2ImplK1Before = 0;
            _tv2ImplContLogged = false;
            _tv2ImplPastLogged = false;
            _tv2UserSrLogged = false;
            _tv2DispatchCtxLogged = false;
            _tv2UserSpLogged = false;
            _tv2UserRaLogged = false;
            _tv2StoreSp = 0;
            _tv2StoreContLogged = false;
            _tv2LeftoverStoreFrame = false;
            _tv2StoreFrameLogged = false;
            _tv2LeftoverLiveLogged = false;
            _tv2LeftoverPastLogged = false;
            _tv2LeftoverCae8Logged = false;
            _tv2LeftoverSkipLogged = false;
            _tv2LeftoverCaf0Logged = false;
            _tv2LeftoverCaf0Peeked = false;
            _tv2LeftoverCaf0Word = 0;
            _tv2LeftoverCae8V0Set = false;
            _tv2LeftoverCae8V0 = 0;
            _tv2LeftoverCae8S6Set = false;
            _tv2LeftoverCae8S6 = 0;
            _tv2LeftoverS6Logged = false;
            _tv2LeftoverCaf4Peeked = false;
            _tv2LeftoverCaf4Word = 0;
            _tv2LeftoverCafcPeeked = false;
            _tv2LeftoverCafcWord = 0;
            _tv2LeftoverAfterCaf0Logged = false;
            _tv2LeftoverPastAfterLogged = false;
            _tv2LeftoverPastCb0cLogged = false;
            _tv2LeftoverCb14Peeked = false;
            _tv2LeftoverCb14Word = 0;
            _tv2LeftoverAfterCb10Logged = false;
            _tv2LeftoverPastCb14Logged = false;
            _tv2LeftoverCb34Peeked = false;
            _tv2LeftoverCb34Word = 0;
            _tv2LeftoverAfterCb14Logged = false;
            _tv2LeftoverPastCb34Logged = false;
            _tv2LeftoverEretLogged = false;
            _tv2GwesFetchLogged = false;
            _tv2GwesContLogged = false;
            _tv2MscoreeSlotLogged = false;
            _coredllMapBusy = false;
            _tv2ProcSwitchLogged = false;
            _tv2CurThreadLogged = false;
            _tv2RestoreLogged = false;
            _tv2SwitchForced = false;
            _tv2SwitchStoreLogged = false;
            _mscoreeTocEntry = 0;
            _mscoreeAttr = 0;
            _mscoreeTocWords = null;
            _mscoreeE32 = 0;
            _mscoreeE32Words = null;
            _mscoreeO32 = 0;
            _mscoreeO32Words = null;
            _mscoreeDataPtr = null;
            _mscoreeDataLen = null;
            _mscoreeData = null;
            _ole32TocEntry = 0;
            _ole32Attr = 0;
            _ole32TocWords = null;
            _ole32E32 = 0;
            _ole32E32Words = null;
            _ole32O32 = 0;
            _ole32O32Words = null;
            _ole32DataPtr = null;
            _ole32DataLen = null;
            _ole32Data = null;
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

        public static void CacheExtraRomDdiNop(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint tocEntry)
        {
            if (memory == null || tocEntry == 0)
                return;
            try
            {
                var toc = new uint[8];
                for (int i = 0; i < 8; i++)
                    toc[i] = memory.ReadMemory32(tocEntry + (uint)(i * 4));
                uint e32 = toc[5];
                uint o32 = toc[6];
                if (e32 == 0 || o32 == 0)
                    return;
                uint objcnt = memory.ReadMemory32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return;
                var e32Words = new uint[32];
                for (int i = 0; i < e32Words.Length; i++)
                    e32Words[i] = memory.ReadMemory32(e32 + (uint)(i * 4));
                var o32Words = new uint[objcnt * 6];
                for (int i = 0; i < o32Words.Length; i++)
                    o32Words[i] = memory.ReadMemory32(o32 + (uint)(i * 4));
                var dataPtr = new uint[objcnt];
                var dataLen = new uint[objcnt];
                var data = new uint[objcnt][];
                for (uint s = 0; s < objcnt; s++)
                {
                    uint psize = o32Words[s * 6 + 2];
                    uint dataptr = o32Words[s * 6 + 3];
                    if (dataptr == 0 || psize == 0 || psize > 0x20000)
                        continue;
                    uint n = (psize + 3) / 4;
                    var blob = new uint[n];
                    for (uint w = 0; w < n; w++)
                        blob[w] = memory.ReadMemory32(dataptr + w * 4);
                    dataPtr[s] = dataptr;
                    dataLen[s] = psize;
                    data[s] = blob;
                }
                _ddiNopTocWords = toc;
                _ddiNopE32 = e32;
                _ddiNopE32Words = e32Words;
                _ddiNopO32 = o32;
                _ddiNopO32Words = o32Words;
                _ddiNopDataPtr = dataPtr;
                _ddiNopDataLen = dataLen;
                _ddiNopData = data;
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[33] cached e32=0x" +
                    e32.ToString("X8") + " o32=0x" + o32.ToString("X8") +
                    " (restore if firmware RAM reuses ExtraROM tail)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[33] cache skipped: " + ex.Message);
            }
        }

        // wait54: FILE[25] FILESentry is 28 bytes at 0x8134E794
        // plus name at +0x14 and compressed bytes at load.
        // Same ExtraROM-tail reuse that zeros TOC[33].
        public static void CacheExtraRomTv2File(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint filesEntry)
        {
            if (memory == null || filesEntry == 0)
                return;
            try
            {
                var words = new uint[7];
                for (int i = 0; i < words.Length; i++)
                    words[i] = memory.ReadMemory32(filesEntry + (uint)(i * 4));
                uint real = words[3];
                uint comp = words[4];
                uint name = words[5];
                uint load = words[6];
                if (real == 0 || name == 0 || load == 0)
                    return;
                uint[] nameWords = null;
                if (name != 0)
                {
                    nameWords = new uint[8];
                    for (int i = 0; i < nameWords.Length; i++)
                        nameWords[i] = memory.ReadMemory32(name + (uint)(i * 4));
                }
                uint[] blob = null;
                if (comp > 0 && comp <= 0x10000)
                {
                    uint n = (comp + 3) / 4;
                    blob = new uint[n];
                    for (uint w = 0; w < n; w++)
                        blob[w] = memory.ReadMemory32(load + w * 4);
                }
                _tv2FileEntry = filesEntry;
                _tv2FileWords = words;
                _tv2FileName = name;
                _tv2FileNameWords = nameWords;
                _tv2FileReal = real;
                _tv2FileComp = comp;
                _tv2FileLoad = load;
                _tv2FileData = blob;
                System.Console.WriteLine("[NkBinLoader] ExtraROM FILE[25] cached entry=0x" +
                    filesEntry.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    " (restore if firmware RAM reuses ExtraROM tail)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[NkBinLoader] ExtraROM FILE[25] cache skipped: " + ex.Message);
            }
        }

        public static void CacheExtraRomMscoree(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint tocEntry)
        {
            if (memory == null || tocEntry == 0)
                return;
            try
            {
                var toc = new uint[8];
                for (int i = 0; i < 8; i++)
                    toc[i] = memory.ReadMemory32(tocEntry + (uint)(i * 4));
                uint e32 = toc[5];
                uint o32 = toc[6];
                if (e32 == 0 || o32 == 0)
                    return;
                uint objcnt = memory.ReadMemory32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return;
                var e32Words = new uint[32];
                for (int i = 0; i < e32Words.Length; i++)
                    e32Words[i] = memory.ReadMemory32(e32 + (uint)(i * 4));
                var o32Words = new uint[objcnt * 6];
                for (int i = 0; i < o32Words.Length; i++)
                    o32Words[i] = memory.ReadMemory32(o32 + (uint)(i * 4));
                var dataPtr = new uint[objcnt];
                var dataLen = new uint[objcnt];
                var data = new uint[objcnt][];
                for (uint s = 0; s < objcnt; s++)
                {
                    uint psize = o32Words[s * 6 + 2];
                    uint dataptr = o32Words[s * 6 + 3];
                    if (dataptr == 0 || psize == 0 || psize > 0x20000)
                        continue;
                    uint n = (psize + 3) / 4;
                    var blob = new uint[n];
                    for (uint w = 0; w < n; w++)
                        blob[w] = memory.ReadMemory32(dataptr + w * 4);
                    dataPtr[s] = dataptr;
                    dataLen[s] = psize;
                    data[s] = blob;
                }
                _mscoreeTocEntry = tocEntry;
                _mscoreeAttr = toc[0];
                _mscoreeTocWords = toc;
                _mscoreeE32 = e32;
                _mscoreeE32Words = e32Words;
                _mscoreeO32 = o32;
                _mscoreeO32Words = o32Words;
                _mscoreeDataPtr = dataPtr;
                _mscoreeDataLen = dataLen;
                _mscoreeData = data;
                _mscoreeVbase = 0;
                if (o32Words.Length >= 6 && o32Words[1] == 0x1000 && o32Words[4] >= 0x1000)
                    _mscoreeVbase = o32Words[4] - o32Words[1];
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[46] cached e32=0x" +
                    e32.ToString("X8") + " o32=0x" + o32.ToString("X8") +
                    " vbase=0x" + _mscoreeVbase.ToString("X8") +
                    " (restore if firmware RAM reuses ExtraROM tail; not a FILE)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[46] cache skipped: " + ex.Message);
            }
        }

        public static void CacheExtraRomOle32(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint tocEntry)
        {
            if (memory == null || tocEntry == 0)
                return;
            try
            {
                var toc = new uint[8];
                for (int i = 0; i < 8; i++)
                    toc[i] = memory.ReadMemory32(tocEntry + (uint)(i * 4));
                uint e32 = toc[5];
                uint o32 = toc[6];
                if (e32 == 0 || o32 == 0)
                    return;
                uint objcnt = memory.ReadMemory32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return;
                var e32Words = new uint[32];
                for (int i = 0; i < e32Words.Length; i++)
                    e32Words[i] = memory.ReadMemory32(e32 + (uint)(i * 4));
                var o32Words = new uint[objcnt * 6];
                for (int i = 0; i < o32Words.Length; i++)
                    o32Words[i] = memory.ReadMemory32(o32 + (uint)(i * 4));
                var dataPtr = new uint[objcnt];
                var dataLen = new uint[objcnt];
                var data = new uint[objcnt][];
                for (uint s = 0; s < objcnt; s++)
                {
                    uint psize = o32Words[s * 6 + 2];
                    uint dataptr = o32Words[s * 6 + 3];
                    if (dataptr == 0 || psize == 0 || psize > 0x20000)
                        continue;
                    uint n = (psize + 3) / 4;
                    var blob = new uint[n];
                    for (uint w = 0; w < n; w++)
                        blob[w] = memory.ReadMemory32(dataptr + w * 4);
                    dataPtr[s] = dataptr;
                    dataLen[s] = psize;
                    data[s] = blob;
                }
                _ole32TocEntry = tocEntry;
                _ole32Attr = toc[0];
                _ole32TocWords = toc;
                _ole32E32 = e32;
                _ole32E32Words = e32Words;
                _ole32O32 = o32;
                _ole32O32Words = o32Words;
                _ole32DataPtr = dataPtr;
                _ole32DataLen = dataLen;
                _ole32Data = data;
                _ole32Vbase = 0;
                if (o32Words.Length >= 6 && o32Words[1] == 0x1000 && o32Words[4] >= 0x1000)
                    _ole32Vbase = o32Words[4] - o32Words[1];
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[34] cached e32=0x" +
                    e32.ToString("X8") + " o32=0x" + o32.ToString("X8") +
                    " vbase=0x" + _ole32Vbase.ToString("X8") +
                    " (restore if firmware RAM reuses ExtraROM tail; not a FILE)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[NkBinLoader] ExtraROM TOC[34] cache skipped: " + ex.Message);
            }
        }

        private static void TryRestoreExtraRomIfClobbered(MipsBus bus, uint tocEntry)
        {
            if (bus == null || tocEntry == 0 || _ddiNopTocWords == null)
                return;
            uint liveE32 = 0;
            uint liveO32 = 0;
            uint liveObjcnt = 0;
            uint liveVsize = 0;
            try
            {
                liveE32 = bus.Read32(tocEntry + 0x14);
                liveO32 = bus.Read32(tocEntry + 0x18);
                if (liveE32 != 0)
                    liveObjcnt = bus.Read32(liveE32) & 0xFFFF;
                if (liveO32 != 0)
                    liveVsize = bus.Read32(liveO32);
            }
            catch
            {
            }
            if (liveE32 == _ddiNopE32 && liveE32 != 0 && liveObjcnt != 0 && liveVsize != 0)
                return;
            try
            {
                for (int i = 0; i < _ddiNopTocWords.Length; i++)
                    bus.Write32(tocEntry + (uint)(i * 4), _ddiNopTocWords[i]);
                if (_ddiNopE32 != 0 && _ddiNopE32Words != null)
                {
                    for (int i = 0; i < _ddiNopE32Words.Length; i++)
                        bus.Write32(_ddiNopE32 + (uint)(i * 4), _ddiNopE32Words[i]);
                }
                if (_ddiNopO32 != 0 && _ddiNopO32Words != null)
                {
                    for (int i = 0; i < _ddiNopO32Words.Length; i++)
                        bus.Write32(_ddiNopO32 + (uint)(i * 4), _ddiNopO32Words[i]);
                }
                if (_ddiNopData != null)
                {
                    for (int s = 0; s < _ddiNopData.Length; s++)
                    {
                        uint[] blob = _ddiNopData[s];
                        if (blob == null || _ddiNopDataPtr[s] == 0)
                            continue;
                        for (int w = 0; w < blob.Length; w++)
                            bus.Write32(_ddiNopDataPtr[s] + (uint)(w * 4), blob[w]);
                    }
                }
                System.Console.WriteLine("[Hive] ExtraROM TOC[33] restored e32=0x" +
                    _ddiNopE32.ToString("X8") + " o32=0x" + _ddiNopO32.ToString("X8") +
                    " (was 0x" + liveE32.ToString("X8") +
                    "; firmware RAM reused ExtraROM tail; do not invent 0x81360000)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[33] restore-fail " + ex.Message);
            }
        }

        private static void TryRestoreExtraRomMscoreeIfClobbered(MipsBus bus)
        {
            if (bus == null || _mscoreeTocEntry == 0 || _mscoreeTocWords == null)
                return;
            uint liveE32 = 0;
            uint liveO32 = 0;
            uint liveObjcnt = 0;
            uint liveVsize = 0;
            try
            {
                liveE32 = bus.Read32(_mscoreeTocEntry + 0x14);
                liveO32 = bus.Read32(_mscoreeTocEntry + 0x18);
                if (liveE32 != 0)
                    liveObjcnt = bus.Read32(liveE32) & 0xFFFF;
                if (liveO32 != 0)
                    liveVsize = bus.Read32(liveO32);
            }
            catch
            {
            }
            if (liveE32 == _mscoreeE32 && liveE32 != 0 && liveObjcnt != 0 && liveVsize != 0)
                return;
            try
            {
                for (int i = 0; i < _mscoreeTocWords.Length; i++)
                    bus.Write32(_mscoreeTocEntry + (uint)(i * 4), _mscoreeTocWords[i]);
                if (_mscoreeE32 != 0 && _mscoreeE32Words != null)
                {
                    for (int i = 0; i < _mscoreeE32Words.Length; i++)
                        bus.Write32(_mscoreeE32 + (uint)(i * 4), _mscoreeE32Words[i]);
                }
                if (_mscoreeO32 != 0 && _mscoreeO32Words != null)
                {
                    for (int i = 0; i < _mscoreeO32Words.Length; i++)
                        bus.Write32(_mscoreeO32 + (uint)(i * 4), _mscoreeO32Words[i]);
                }
                if (_mscoreeData != null)
                {
                    for (int s = 0; s < _mscoreeData.Length; s++)
                    {
                        uint[] blob = _mscoreeData[s];
                        if (blob == null || _mscoreeDataPtr[s] == 0)
                            continue;
                        for (int w = 0; w < blob.Length; w++)
                            bus.Write32(_mscoreeDataPtr[s] + (uint)(w * 4), blob[w]);
                    }
                }
                System.Console.WriteLine("[Hive] ExtraROM TOC[46] restored e32=0x" +
                    _mscoreeE32.ToString("X8") + " o32=0x" + _mscoreeO32.ToString("X8") +
                    " (was 0x" + liveE32.ToString("X8") +
                    "; firmware RAM reused ExtraROM tail; do not invent a FILE)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[46] restore-fail " + ex.Message);
            }
        }

        private static void TryRestoreExtraRomOle32IfClobbered(MipsBus bus)
        {
            if (bus == null || _ole32TocEntry == 0 || _ole32TocWords == null)
                return;
            uint liveE32 = 0;
            uint liveO32 = 0;
            uint liveObjcnt = 0;
            uint liveVsize = 0;
            try
            {
                liveE32 = bus.Read32(_ole32TocEntry + 0x14);
                liveO32 = bus.Read32(_ole32TocEntry + 0x18);
                if (liveE32 != 0)
                    liveObjcnt = bus.Read32(liveE32) & 0xFFFF;
                if (liveO32 != 0)
                    liveVsize = bus.Read32(liveO32);
            }
            catch
            {
            }
            if (liveE32 == _ole32E32 && liveE32 != 0 && liveObjcnt != 0 && liveVsize != 0)
                return;
            try
            {
                for (int i = 0; i < _ole32TocWords.Length; i++)
                    bus.Write32(_ole32TocEntry + (uint)(i * 4), _ole32TocWords[i]);
                if (_ole32E32 != 0 && _ole32E32Words != null)
                {
                    for (int i = 0; i < _ole32E32Words.Length; i++)
                        bus.Write32(_ole32E32 + (uint)(i * 4), _ole32E32Words[i]);
                }
                if (_ole32O32 != 0 && _ole32O32Words != null)
                {
                    for (int i = 0; i < _ole32O32Words.Length; i++)
                        bus.Write32(_ole32O32 + (uint)(i * 4), _ole32O32Words[i]);
                }
                if (_ole32Data != null)
                {
                    for (int s = 0; s < _ole32Data.Length; s++)
                    {
                        uint[] blob = _ole32Data[s];
                        if (blob == null || _ole32DataPtr[s] == 0)
                            continue;
                        for (int w = 0; w < blob.Length; w++)
                            bus.Write32(_ole32DataPtr[s] + (uint)(w * 4), blob[w]);
                    }
                }
                System.Console.WriteLine("[Hive] ExtraROM TOC[34] restored e32=0x" +
                    _ole32E32.ToString("X8") + " o32=0x" + _ole32O32.ToString("X8") +
                    " (was 0x" + liveE32.ToString("X8") +
                    "; firmware RAM reused ExtraROM tail; do not invent a FILE)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[34] restore-fail " + ex.Message);
            }
        }

        private static void TryRestoreExtraRomFileIfClobbered(MipsBus bus)
        {
            if (bus == null || _tv2FileEntry == 0 || _tv2FileWords == null)
                return;
            uint liveAttr = 0;
            uint liveName = 0;
            uint liveReal = 0;
            uint liveComp = 0;
            uint liveLoad = 0;
            try
            {
                liveAttr = bus.Read32(_tv2FileEntry);
                liveName = bus.Read32(_tv2FileEntry + FilesNameOff);
                liveReal = bus.Read32(_tv2FileEntry + FilesRealSize);
                liveComp = bus.Read32(_tv2FileEntry + FilesCompSize);
                liveLoad = bus.Read32(_tv2FileEntry + FilesLoadOff);
            }
            catch
            {
            }
            if (liveAttr == _tv2FileWords[0] && liveName == _tv2FileName
                && liveReal == _tv2FileReal && liveComp == _tv2FileComp
                && liveLoad == _tv2FileLoad && liveReal != 0)
                return;
            try
            {
                for (int i = 0; i < _tv2FileWords.Length; i++)
                    bus.Write32(_tv2FileEntry + (uint)(i * 4), _tv2FileWords[i]);
                if (_tv2FileName != 0 && _tv2FileNameWords != null)
                {
                    for (int i = 0; i < _tv2FileNameWords.Length; i++)
                        bus.Write32(_tv2FileName + (uint)(i * 4), _tv2FileNameWords[i]);
                }
                uint liveLoad0 = 0;
                try
                {
                    if (_tv2FileLoad != 0)
                        liveLoad0 = bus.Read32(_tv2FileLoad);
                }
                catch
                {
                }
                if (_tv2FileData != null && _tv2FileLoad != 0 && liveLoad0 == 0)
                {
                    for (int w = 0; w < _tv2FileData.Length; w++)
                        bus.Write32(_tv2FileLoad + (uint)(w * 4), _tv2FileData[w]);
                }
                System.Console.WriteLine("[Hive] ExtraROM FILE[25] restored entry=0x" +
                    _tv2FileEntry.ToString("X8") +
                    " real=" + _tv2FileReal +
                    " load=0x" + _tv2FileLoad.ToString("X8") +
                    " (was attr=0x" + liveAttr.ToString("X8") +
                    " real=" + liveReal +
                    "; firmware RAM reused ExtraROM tail; do not invent 0x81360000)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM FILE[25] restore-fail " + ex.Message);
            }
        }

        // wait55: type 7 made LoadE32 read FILE+0x14 (name). Firmware
        // loads a compressed FILE like runonce.exe via CreateFile
        // type 8, then CEDecompressROM of the dump record, then
        // SetFilePointer/ReadFile. Do not invent e32/o32.
        public static bool TryStartTv2FileDecompress(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 31)
                return false;
            if (_tv2FileEntry == 0 || _tv2FileReal == 0 || _tv2FileComp == 0)
                return false;
            uint src = Tv2FileSrcAlign;
            uint dest = Tv2FileDest;
            try
            {
                uint n = (_tv2FileComp + 3) / 4;
                uint[] blob = _tv2FileData;
                for (uint w = 0; w < n; w++)
                {
                    uint word = blob != null && w < blob.Length
                        ? blob[w]
                        : bus.Read32(_tv2FileLoad + w * 4);
                    bus.Write32(src + w * 4, word);
                }
                uint pages = (_tv2FileReal + 0x1FFFu) & ~0xFFFu;
                for (uint i = 0; i < pages; i += 4)
                    bus.Write32(dest + i, 0);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] FILE[25] dest-prep fail " + ex.Message +
                    " (do not invent 0x81360000)");
                return false;
            }
            regs[4] = src;
            regs[5] = _tv2FileComp;
            regs[6] = dest;
            regs[7] = _tv2FileReal;
            _tv2FileSavedSp = regs[29];
            regs[29] = _tv2FileSavedSp - 32;
            try
            {
                bus.Write32(regs[29] + 16, 0);
                bus.Write32(regs[29] + 20, 1);
                bus.Write32(regs[29] + 24, 0x1000);
            }
            catch
            {
            }
            _tv2FileDecompRa = NameCopyContinue;
            _tv2FilePos = 0;
            _tv2FileDestOn = true;
            regs[31] = NameCopyContinue;
            programCounter = BinaryDecompressRom;
            uint src0 = 0;
            try
            {
                src0 = bus.Read32(src);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] CEDecompressROM dest=0x" +
                dest.ToString("X8") + " src=0x" + src.ToString("X8") +
                " real=" + _tv2FileReal +
                " comp=" + _tv2FileComp +
                " src0=0x" + src0.ToString("X8") +
                " (firmware 0x8004DBF8; dump FILE record; do not invent e32)");
            return true;
        }

        public static bool TryFinishTv2FileDecompress(MipsBus bus, uint[] regs, uint pc)
        {
            if (_tv2FileDecompRa == 0 || pc != _tv2FileDecompRa)
                return false;
            _tv2FileDecompRa = 0;
            if (regs != null && regs.Length > 29 && _tv2FileSavedSp != 0)
                regs[29] = _tv2FileSavedSp;
            _tv2FileSavedSp = 0;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint word = 0;
            uint pe = 0;
            uint lfanew = 0;
            bool mz = false;
            try
            {
                if (bus != null)
                {
                    word = bus.Read32(Tv2FileDest);
                    mz = (word & 0xFFFF) == 0x5A4D;
                    if (mz)
                    {
                        lfanew = bus.Read32(Tv2FileDest + 0x3C);
                        if (lfanew + 4 <= _tv2FileReal)
                            pe = bus.Read32(Tv2FileDest + lfanew);
                        if (pe == 0x00004550u && lfanew + 56 <= _tv2FileReal)
                        {
                            _tv2PeEntryRva = bus.Read32(Tv2FileDest + lfanew + 40);
                            _tv2PeImageBase = bus.Read32(Tv2FileDest + lfanew + 52);
                            if (lfanew + 24 + 96 + 14 * 8 + 4 <= _tv2FileReal)
                                _tv2PeComRva = bus.Read32(Tv2FileDest + lfanew + 24 + 96 + 14 * 8);
                        }
                    }
                }
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] CEDecompressROM ret v0=0x" +
                v0.ToString("X8") + " dest=0x" + Tv2FileDest.ToString("X8") +
                " word=0x" + word.ToString("X8") +
                (mz ? " MZ e_lfanew=0x" + lfanew.ToString("X") +
                    " pe=0x" + pe.ToString("X8") : " (not MZ)") +
                (_tv2PeEntryRva != 0 || _tv2PeImageBase != 0
                    ? " entryrva=0x" + _tv2PeEntryRva.ToString("X") +
                      " imagebase=0x" + _tv2PeImageBase.ToString("X8") +
                      " comrva=0x" + _tv2PeComRva.ToString("X")
                    : "") +
                (v0 == _tv2FileReal ? " (firmware expanded FILE real)" : "") +
                " (do not invent e32; FILE[26] tv2clientcorece.dll is 6398464)");
            return false;
        }

        public static bool IsTv2FileHandle(uint handle)
        {
            return _tv2FileDestOn && _tv2FileEntry != 0 && handle == _tv2FileEntry;
        }

        public static bool TryServeTv2SetFilePointer(uint[] regs, uint jalrTarget, ref uint target)
        {
            if (jalrTarget != Win32SetFilePointer || regs == null || regs.Length <= 7)
                return false;
            if (!IsTv2FileHandle(regs[4]))
                return false;
            uint dist = regs[5];
            uint method = regs[7];
            uint pos = _tv2FilePos;
            if (method == 0)
                pos = dist;
            else if (method == 1)
                pos = _tv2FilePos + dist;
            else if (method == 2)
                pos = _tv2FileReal + dist;
            if (pos > _tv2FileReal)
                pos = _tv2FileReal;
            _tv2FilePos = pos;
            regs[2] = pos;
            target = regs.Length > 31 ? regs[31] : target;
            if (!_tv2FileIoLogged)
            {
                _tv2FileIoLogged = true;
                System.Console.WriteLine("[Hive] FILE[25] SetFilePointer pos=0x" +
                    pos.ToString("X") + " method=" + method +
                    " (dump FILE bytes; do not invent e32)");
            }
            else if (method == 0 && dist < _tv2FileReal)
            {
                System.Console.WriteLine("[Hive] FILE[25] MapO32 SetFilePointer pos=0x" +
                    pos.ToString("X") + " (PE raw; firmware 0x8001AECC)");
            }
            return true;
        }

        public static bool TryServeTv2FileRead(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 31)
                return false;
            if (!IsTv2FileHandle(regs[4]))
                return false;
            uint dest = regs[5];
            uint count = regs[6];
            uint outN = regs[7];
            if (dest == 0 || count == 0 || count > 0x10000)
                return false;
            if (IsTv2DumpPeDest(dest) && !DestReadable(bus, dest))
                TryHostBackTv2PeDest(dest, count);
            uint left = _tv2FileReal > _tv2FilePos ? _tv2FileReal - _tv2FilePos : 0;
            if (count > left)
                count = left;
            uint srcPos = _tv2FilePos;
            try
            {
                for (uint i = 0; i < count; i += 4)
                {
                    uint word = bus.Read32(Tv2FileDest + _tv2FilePos + i);
                    if (i + 4 <= count)
                        bus.Write32((dest + i) & ~3u, word);
                    else
                    {
                        for (uint b = 0; b < count - i; b++)
                        {
                            uint src = Tv2FileDest + _tv2FilePos + i + b;
                            uint w = bus.Read32(src & ~3u);
                            uint ch = (w >> (8 * (int)(src & 3))) & 0xFF;
                            uint d = dest + i + b;
                            uint dw = bus.Read32(d & ~3u);
                            int sh = 8 * (int)(d & 3);
                            dw = (dw & ~(0xFFu << sh)) | (ch << sh);
                            bus.Write32(d & ~3u, dw);
                        }
                    }
                }
                if (outN != 0)
                    bus.Write32(outN, count);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] FILE[25] ReadFile fail " + ex.Message);
                return false;
            }
            _tv2FilePos += count;
            regs[2] = 1;
            programCounter = regs[31];
            if (IsTv2DumpPeDest(dest) && count != 0)
            {
                uint destWord = 0;
                uint fileWord = 0;
                try
                {
                    destWord = bus.Read32(dest);
                    fileWord = bus.Read32(Tv2FileDest + srcPos);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] FILE[25] MapO32 ReadFile dest=0x" +
                    dest.ToString("X8") + " pos=0x" + srcPos.ToString("X") +
                    " n=0x" + count.ToString("X") +
                    " dest-word=0x" + destWord.ToString("X8") +
                    " file-word=0x" + fileWord.ToString("X8") +
                    (destWord == fileWord && destWord != 0
                        ? " (firmware copied dump PE)"
                        : " (copy miss)") +
                    " (Tv2FileDest+raw; do not invent section bytes)");
            }
            return true;
        }

        // wait58: v0=Tv2FileDest made CreateFile think the FILE was
        // mapped. Firmware then skips sh object+6=3 (0x8001D4F0)
        // and MapO32 lhu +6 < 2 returns success without ReadFile
        // (BindImp word=0). Mapping miss is the real FILE path:
        // object+6=3, then 0x8001AECC SetFilePointer(dataptr raw)
        // + ReadFile onto the VALLOC dest from Tv2FileDest+pos.
        public static bool TryServeTv2FileMap(uint[] regs, ref uint programCounter)
        {
            if (regs == null || regs.Length <= 31)
                return false;
            if (!IsTv2FileHandle(regs[4]))
                return false;
            regs[2] = 0;
            programCounter = regs[31];
            System.Console.WriteLine("[Hive] FILE[25] CreateFileMapping v0=0" +
                " (firmware object+6=3; MapO32 ReadFile of dump PE; do not invent e32)");
            return true;
        }

        public static bool IsTv2FileExpanded()
        {
            return _tv2FileDestOn && _tv2FileReal != 0;
        }

        public static bool IsTv2DumpPeDest(uint dest)
        {
            if (!_tv2FileDestOn || dest == 0 || dest == 0x000E0000u)
                return false;
            if (dest >= 0x80000000u)
                return false;
            // wait67: filesys I-fetches 0x00017F54 / 0x00017000.
            // VALLOC 0x00010000/0x8000 covers that useg. Those
            // pages are not MapO32 dests. Do not invent them.
            if (dest >= 0x00017000u && dest < 0x00018000u)
                return false;
            if (_tv2PeImageVa != 0 && _tv2PeImageBytes != 0)
                return dest >= _tv2PeImageVa && dest < _tv2PeImageVa + _tv2PeImageBytes;
            return dest >= ExeVbase && dest < ExeVbase + 0x8000u;
        }

        // wait56: MEM_IMAGE VALLOC of this dump PE. Capture even
        // when the shared VALLOC-ret slot is overwritten.
        public static bool NoteTv2PeImageValloc(uint dest, uint size, uint type, uint ra)
        {
            if (!_tv2FileDestOn || dest != ExeVbase)
                return false;
            if ((type & 0x01000000u) == 0)
                return false;
            if (size < 0x4000u || size > 0x10000u)
                return false;
            _tv2PeImageVa = dest;
            _tv2PeImageBytes = size;
            _tv2PeVallocRa = ra;
            System.Console.WriteLine("[Hive] FILE[25] VALLOC image a0=0x" +
                dest.ToString("X8") + " a1=0x" + size.ToString("X8") +
                " a2=0x" + type.ToString("X8") +
                " (dump PE MEM_IMAGE; dests host-backed at MapO32 only)");
            return true;
        }

        public static bool TryFinishTv2PeImageValloc(uint pc, uint v0)
        {
            if (_tv2PeVallocRa == 0 || pc != _tv2PeVallocRa)
                return false;
            _tv2PeVallocRa = 0;
            if (v0 != 0)
            {
                _tv2PeImageVa = v0;
                if (_tv2PeImageBytes == 0)
                    _tv2PeImageBytes = 0x8000;
            }
            System.Console.WriteLine("[Hive] FILE[25] VALLOC image ret v0=0x" +
                v0.ToString("X8") +
                (v0 == 0 ? " (firmware miss)" : " (dump PE dest range)") +
                " (do not invent 0x81360000; do not host-back 0x000E0000)");
            if (v0 != 0)
                TryHostBackTv2PeVallocGapPage();
            return true;
        }

        // wait59: I-fetch 0x00013628 is page 0x00013000. That page
        // sits inside firmware VALLOC 0x00010000/0x8000, past
        // dest+vsize 0x8C4 (MapO32 host-back ends at 0x00013000).
        // Host-back this one page only. Not a MapO32 dest. Not
        // 0x000E0000. Do not invent section bytes.
        private static void TryHostBackTv2PeVallocGapPage()
        {
            const uint page = 0x00013000u;
            if (!_tv2FileDestOn || _tv2PeImageVa == 0 || _tv2PeImageBytes == 0)
                return;
            if (page < _tv2PeImageVa || page + 0x1000u > _tv2PeImageVa + _tv2PeImageBytes)
                return;
            TryHostBackTv2PeDest(page, 0x1000);
        }

        // wait57: type 8 MapO32 does not jal 0x80028844 or VirtualCopy.
        // object+4 bit2 is 0, so 0x8001AECC SetFilePointer(dataptr,
        // FILE_BEGIN) then ReadFile(dest, min(vsize,psize)). dataptr
        // must stay PE raw. wait57 rewrote it to Tv2FileDest+raw;
        // jalr -8210 then v0!=dataptr and skipped the copy (BindImp
        // word=0). Host-back dest only; firmware ReadFile copies
        // from Tv2FileDest+_tv2FilePos. Do not invent section bytes.
        public static void TryMapTv2DumpPeO32(MipsBus bus, uint o32Lite)
        {
            if (!_tv2FileDestOn || bus == null || o32Lite == 0 || _tv2FileReal == 0)
                return;
            try
            {
                uint vsize = bus.Read32(o32Lite);
                uint dest = bus.Read32(o32Lite + 8);
                uint dataptr = bus.Read32(o32Lite + 0x18);
                if (!IsTv2DumpPeDest(dest))
                    return;
                if (dataptr >= _tv2FileReal)
                    return;
                uint raw = dataptr;
                if (!DestReadable(bus, dest))
                    TryHostBackTv2PeDest(dest, vsize);
                uint fileWord = 0;
                try
                {
                    fileWord = bus.Read32(Tv2FileDest + raw);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] FILE[25] MapO32 dest=0x" +
                    dest.ToString("X8") + " dataptr raw=0x" + raw.ToString("X") +
                    " vsize=0x" + vsize.ToString("X") +
                    " file-word=0x" + fileWord.ToString("X8") +
                    " dest-" + (DestReadable(bus, dest) ? "mapped" : "unmapped") +
                    " (SetFilePointer+ReadFile of dump PE; do not rewrite dataptr)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] FILE[25] MapO32 fail " + ex.Message +
                    " (do not invent 0x81360000)");
            }
        }

        public static void TryNoteTv2BindImp(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FileDestOn || _tv2BindLogged || regs == null)
                return;
            if (pc != BindImpHdr)
                return;
            _tv2BindLogged = true;
            uint hdr = regs.Length > 20 ? regs[20] : 0;
            uint vbase = regs.Length > 22 ? regs[22] : 0;
            uint destWord = 0;
            bool destOk = false;
            try
            {
                if (bus != null && hdr != 0)
                {
                    destWord = bus.Read32(hdr);
                    destOk = true;
                }
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] BindImp hdr=0x" +
                hdr.ToString("X8") + " vbase=0x" + vbase.ToString("X8") +
                " word=0x" + destWord.ToString("X8") +
                " dest-" + (destOk ? "mapped" : "unmapped") +
                " (dump PE dest; do not invent 0x81360000)");
        }

        private static void TryHostBackTv2PeDest(uint dest, uint vsize)
        {
            if (dest == 0 || dest == 0x000E0000u || dest >= 0x80000000u)
                return;
            if (!IsTv2DumpPeDest(dest))
                return;
            uint baseVa = dest & ~0xFFFu;
            uint size = vsize == 0 ? 0x1000u : vsize;
            uint end = (dest + size + 0xFFFu) & ~0xFFFu;
            if (end <= baseVa)
                return;
            if (end > 0x000E0000u && baseVa < 0x000E0000u)
                end = 0x000E0000u;
            if (_tv2PeImageVa != 0 && _tv2PeImageBytes != 0)
            {
                uint imageEnd = _tv2PeImageVa + _tv2PeImageBytes;
                if (end > imageEnd)
                    end = imageEnd;
            }
            else if (end > ExeVbase + 0x8000u)
                end = ExeVbase + 0x8000u;
            if (end <= baseVa)
                return;
            if (MapVallocHostVa(baseVa) != baseVa)
                return;
            uint span = end - baseVa;
            if (_vallocHostN >= _vallocHostLo.Length)
                return;
            uint kseg = _vallocHostPool;
            if (kseg < VallocHostKseg || kseg + span > VallocHostKsegLim)
                return;
            _vallocHostLo[_vallocHostN] = baseVa;
            _vallocHostHi[_vallocHostN] = end;
            _vallocHostKseg[_vallocHostN] = kseg;
            _vallocHostN++;
            _vallocHostPool += span;
            string why = baseVa == 0x00013000u
                ? " (firmware VALLOC image page; not a MapO32 dest; do not invent 0x000E0000)"
                : " (firmware MapO32 of dump PE; do not invent 0x000E0000)";
            System.Console.WriteLine("[Hive] FILE[25] dest host-back 0x" +
                baseVa.ToString("X8") + "-0x" + end.ToString("X8") +
                " -> 0x" + kseg.ToString("X8") + why);
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

        // ExtraROM FILESentry follows TOC modules
        // (romhdr+0x54+nmods*32, 28 bytes). wait53 CreateFile
        // miss: NK/BINFS never walks this table.
        private static bool TryFindExtraRomFile(MipsBus bus, string baseName,
            out uint filesEntry, out uint attr, out uint real, out uint comp, out uint load)
        {
            filesEntry = 0;
            attr = 0;
            real = 0;
            comp = 0;
            load = 0;
            if (bus == null || string.IsNullOrEmpty(baseName))
                return false;
            try
            {
                uint toc = ExtraRomToc(bus);
                if (toc == 0)
                    return false;
                uint nmods = bus.Read32(toc + RomHdrNumMods);
                uint nfiles = bus.Read32(toc + RomHdrNumFiles);
                if (nmods > 128 || nfiles == 0 || nfiles > 128)
                    return false;
                uint first = toc + TocFirst + nmods * TocEntrySize;
                for (uint i = 0; i < nfiles; i++)
                {
                    uint entry = first + i * FilesEntrySize;
                    uint name = bus.Read32(entry + FilesNameOff);
                    if (!NamesEqual(baseName, ReadAscii(bus, name)))
                        continue;
                    uint fileAttr = bus.Read32(entry);
                    real = bus.Read32(entry + FilesRealSize);
                    comp = bus.Read32(entry + FilesCompSize);
                    load = bus.Read32(entry + FilesLoadOff);
                    // Keep the dump FILE attr (0x807). Do not set
                    // ROMMODULE 0x2000: LoadE32 then reads +0x14 as e32.
                    attr = fileAttr;
                    filesEntry = entry;
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

        public static bool TryMissMscoreeWin32(MipsBus bus, uint path, uint[] regs, ref uint programCounter)
        {
            if (regs == null || regs.Length <= 31)
                return false;
            string baseName = "";
            try
            {
                if (bus != null && path != 0)
                    baseName = Basename(bus, path);
            }
            catch
            {
            }
            if (string.IsNullOrEmpty(baseName))
                baseName = _pendingRomFile;
            if (!IsMscoreeDll(baseName) && !IsOle32Dll(baseName))
                return false;
            regs[2] = 0xFFFFFFFFu;
            programCounter = regs[31];
            System.Console.WriteLine("[Hive] Win32 CreateFile " + baseName +
                " INVALID_HANDLE (no dump FILE; ExtraROM TOC type-7 attach at 0x8001D400)");
            return true;
        }

        public static void TryRejectMscoreeFileHandle(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null || regs.Length <= 23)
                return;
            uint v0 = regs[2];
            if (v0 == 0xFFFFFFFFu)
                return;
            string baseName = _pendingRomFile;
            if (!IsMscoreeDll(baseName) && !IsOle32Dll(baseName))
            {
                try
                {
                    baseName = Basename(bus, regs[23]);
                    if (!IsMscoreeDll(baseName) && !IsOle32Dll(baseName) && regs[4] != 0)
                        baseName = Basename(bus, regs[4]);
                }
                catch
                {
                    return;
                }
            }
            if (!IsMscoreeDll(baseName) && !IsOle32Dll(baseName))
                return;
            regs[2] = 0xFFFFFFFFu;
            System.Console.WriteLine("[Hive] Win32 CreateFile " + baseName +
                " v0=0x" + v0.ToString("X8") +
                " (filesys handle; FILE table has no " + baseName +
                "; INVALID_HANDLE so ExtraROM TOC type-7 attach)");
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

        public static bool TryForceDdiNopCallDll(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 30)
                return false;
            uint module = regs[30];
            if (module == 0)
                return false;
            try
            {
                uint ip = bus.Read32(module + ModuleStartip);
                uint vbase = bus.Read32(module + ProcModule);
                bool ddi = (ip >= 0x01980000u && ip < 0x019B0000u)
                    || ip == 0x03998014u
                    || vbase == DdiNopVbase
                    || IsDdiNopTocObject(bus, module + ModuleFileObj);
                if (!ddi || ip == 0)
                    return false;
                regs[4] = module;
                regs[5] = 1;
                programCounter = XipDllCallDllJal;
                System.Console.WriteLine("[Hive] force CallDLL ExtraROM ddi_nop module=0x" +
                    module.ToString("X8") + " startip=0x" + ip.ToString("X8") +
                    " a1=1 (jal 0x80018B34; do not land on addiu a1,0,0)");
                return true;
            }
            catch
            {
                return false;
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

        public static bool IsAllowedTv2Startip(uint va)
        {
            if (va >= 0x00012000u && va < 0x00013000u) return true;
            if (va >= 0x00014000u && va < 0x00015000u) return true;
            if (va >= 0x00016000u && va < 0x00017000u) return true;
            if (va >= 0x014B1000u && va < 0x014D0000u) return true;
            return false;
        }

        public static void TryNoteTv2LoadExeE32(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FileDestOn || pc != LoadExeE32Ret || bus == null || regs == null)
                return;
            if (regs.Length <= 29)
                return;
            try
            {
                uint proc = bus.Read32(CurProc);
                if (proc >= 0x80000000u && (_tv2Proc == 0 || IsNkOrFilesysProc(_tv2Proc)))
                    _tv2Proc = proc;
                uint sp = regs[29];
                uint s5 = regs.Length > 21 ? regs[21] : 0;
                uint entryRva = sp != 0 ? bus.Read32(sp + 28) : 0;
                uint e32plus4 = 0;
                uint e32plus8 = 0;
                uint e32plus16 = 0;
                if (s5 >= 0x80000000u)
                {
                    e32plus4 = bus.Read32(s5 + 4);
                    e32plus8 = bus.Read32(s5 + 8);
                    e32plus16 = bus.Read32(s5 + 16);
                }
                System.Console.WriteLine("[Hive] FILE[25] load-exe e32: 28(sp)=0x" +
                    entryRva.ToString("X8") +
                    " e32+4=0x" + e32plus4.ToString("X8") +
                    " e32+8=0x" + e32plus8.ToString("X8") +
                    " e32+16=0x" + e32plus16.ToString("X8") +
                    " dump-entryrva=0x" + _tv2PeEntryRva.ToString("X8") +
                    " dump-imagebase=0x" + _tv2PeImageBase.ToString("X8") +
                    " dump-comrva=0x" + _tv2PeComRva.ToString("X8") +
                    " (COM path if e32+16!=0; do not invent 0x00017F54)");
            }
            catch
            {
            }
        }

        public static void TryKeepTv2FileStartip(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FileDestOn || pc != LoadExeStartipRet || bus == null || regs == null)
                return;
            if (regs.Length <= 19)
                return;
            uint s3 = regs[19];
            System.Console.WriteLine("[Hive] FILE[25] load-exe startip-ret: s3=0x" +
                s3.ToString("X8") +
                " dump-entryrva=0x" + _tv2PeEntryRva.ToString("X8") +
                " dump-comrva=0x" + _tv2PeComRva.ToString("X8") +
                " (firmware RVA->VA or _CorExeMain; not invented 0x00017F54)");
            if (!IsAllowedTv2Startip(s3))
                return;
            _tv2Startip = s3;
            try
            {
                uint proc = bus.Read32(CurProc);
                if (proc >= 0x80000000u && (_tv2Proc == 0 || IsNkOrFilesysProc(_tv2Proc)))
                    _tv2Proc = proc;
                if (_tv2Proc != 0)
                    TryFillFileExeStartip(bus, _tv2Proc);
            }
            catch
            {
            }
        }

        // wait70: dest-on gated this past the primary ThreadContextSetup
        // (thr+5C still 0). First noted thread after FILE dest-on was
        // the NK helper. Bind the CreateProcess thread even before
        // dest-on; displace only when incoming +5C is firmware startip
        // and the current bind is not.
        public static void NoteTv2Thread(uint thr)
        {
            NoteTv2Thread(null, thr);
        }

        public static void NoteTv2Thread(MipsBus bus, uint thr)
        {
            if (thr < 0x80000000u)
                return;
            uint incomingIp = 0;
            if (bus != null)
            {
                try
                {
                    incomingIp = bus.Read32(thr + ThreadStartip);
                }
                catch
                {
                }
            }
            bool incomingPrimary = incomingIp != 0 && IsAllowedTv2Startip(incomingIp);
            if (_tv2Thread != 0 && _tv2Thread != thr)
            {
                if (!incomingPrimary)
                    return;
                uint curIp = 0;
                if (bus != null)
                {
                    try
                    {
                        curIp = bus.Read32(_tv2Thread + ThreadStartip);
                    }
                    catch
                    {
                    }
                }
                if (curIp != 0 && IsAllowedTv2Startip(curIp))
                    return;
                System.Console.WriteLine("[Hive] FILE[25] thread bind: was=0x" +
                    _tv2Thread.ToString("X8") +
                    " now=0x" + thr.ToString("X8") +
                    " +5C=0x" + incomingIp.ToString("X8") +
                    " (firmware startip; not NK helper)");
            }
            else if (_tv2Thread == 0)
            {
                System.Console.WriteLine("[Hive] FILE[25] thread bind: now=0x" +
                    thr.ToString("X8") +
                    " +5C=0x" + incomingIp.ToString("X8") +
                    " (tv2 CreateProcess thread; dest-on not required)");
            }
            _tv2Thread = thr;
        }

        private static bool IsNkOrFilesysProc(uint proc)
        {
            return proc == ProcTable || proc == ProcTable + ProcSize;
        }

        // wait71: keep/force while +5C was trampoline 0x8001FF38
        // aborted CreateProcess (v0=0 last-error=193). Wait until
        // firmware has stored startip on the primary (CreateProcess-ret).
        private static bool IsTv2PrimaryStartipReady(MipsBus bus)
        {
            if (bus == null || _tv2Thread == 0)
                return false;
            try
            {
                uint ip = bus.Read32(_tv2Thread + ThreadStartip);
                return IsAllowedTv2Startip(ip);
            }
            catch
            {
                return false;
            }
        }

        public static void TryKeepTv2ThreadOwner(MipsBus bus, string tag)
        {
            if (!_tv2FileDestOn || bus == null || _tv2Thread == 0 || _tv2Proc == 0)
                return;
            if (!IsTv2PrimaryStartipReady(bus))
                return;
            if (IsNkOrFilesysProc(_tv2Proc))
                return;
            uint owner;
            try
            {
                owner = bus.Read32(_tv2Thread + ThreadPrc);
            }
            catch
            {
                return;
            }
            if (owner == _tv2Proc)
                return;
            if (owner != 0 && !IsNkOrFilesysProc(owner))
                return;
            try
            {
                bus.Write32(_tv2Thread + ThreadPrc, _tv2Proc);
                System.Console.WriteLine("[Hive] FILE[25] thread +0C: " + tag +
                    " thr=0x" + _tv2Thread.ToString("X8") +
                    " was=0x" + owner.ToString("X8") +
                    " now=0x" + _tv2Proc.ToString("X8") +
                    " (firmware tv2 proc; switcher CurProc; not a slot map)");
            }
            catch
            {
            }
        }

        public static bool TryForceTv2ProcSwitch(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (!_tv2FileDestOn || _tv2Thread == 0 || _tv2Proc == 0 || regs == null || regs.Length <= 2)
                return false;
            if (programCounter != ThreadSwitchProcChk)
                return false;
            if (regs[2] != _tv2Thread)
                return false;
            if (!IsTv2PrimaryStartipReady(bus))
                return false;
            if (IsNkOrFilesysProc(_tv2Proc))
                return false;
            uint cur = 0;
            if (bus != null)
            {
                try
                {
                    cur = bus.Read32(CurProc);
                }
                catch
                {
                    return false;
                }
                if (cur == _tv2Proc)
                    return false;
            }
            programCounter = ThreadSwitchProcSlow;
            if (!_tv2SwitchForced)
            {
                _tv2SwitchForced = true;
                System.Console.WriteLine("[Hive] FILE[25] switcher force-slow v0=0x" +
                    regs[2].ToString("X8") +
                    " CurProc=0x" + cur.ToString("X8") +
                    " owner=0x" + _tv2Proc.ToString("X8") +
                    " (firmware 0x8001554C; not an invented slot map)");
            }
            return true;
        }

        // wait73: 0x800154FC can skip 0x8001554C and land on
        // 0x800155A8; fast ERET 0x8001566C then restores startip
        // with CurProc still filesys. Send that ERET through
        // 0x80015550 (v0=s0) so firmware 0x80015570 re-reads +0C.
        // Do not poke CurProc.
        public static bool TryForceTv2EretSlowPath(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (!_tv2FileDestOn || _tv2Thread == 0 || _tv2Proc == 0 || regs == null || regs.Length <= 16)
                return false;
            if (programCounter != ThreadCtxRestore2)
                return false;
            if (regs[16] != _tv2Thread)
                return false;
            if (!IsTv2PrimaryStartipReady(bus))
                return false;
            if (IsNkOrFilesysProc(_tv2Proc))
                return false;
            uint cur = 0;
            if (bus != null)
            {
                try
                {
                    cur = bus.Read32(CurProc);
                }
                catch
                {
                    return false;
                }
                if (cur == _tv2Proc)
                    return false;
            }
            regs[2] = regs[16];
            programCounter = ThreadSwitchProcSlow;
            _tv2SwitchStoreLogged = false;
            System.Console.WriteLine("[Hive] FILE[25] ERET2 force-slow s0=0x" +
                regs[16].ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " owner=0x" + _tv2Proc.ToString("X8") +
                " (firmware 0x80015550; not an invented slot map)");
            return true;
        }

        public static void TryNoteTv2ProcSwitchStore(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FileDestOn || pc != ThreadSwitchProcStore || bus == null || regs == null)
                return;
            if (regs.Length <= 8)
                return;
            uint t0 = regs[8];
            if (t0 != _tv2Proc)
                return;
            uint cur = 0;
            try
            {
                cur = bus.Read32(CurProc);
            }
            catch
            {
                return;
            }
            if (cur == _tv2Proc)
                return;
            if (_tv2SwitchStoreLogged)
                return;
            _tv2SwitchStoreLogged = true;
            try
            {
                uint slot = 0;
                if (_tv2Proc != 0)
                    slot = bus.Read32(_tv2Proc + ProcSlot);
                System.Console.WriteLine("[Hive] FILE[25] switcher CurProc t0=0x" +
                    t0.ToString("X8") +
                    " before=0x" + cur.ToString("X8") +
                    " proc+0C=0x" + slot.ToString("X8") +
                    " (firmware 0x80015570; not an invented slot map)");
            }
            catch
            {
            }
        }

        public static void TryKeepTv2ThreadStartip(MipsBus bus, uint threadStartip)
        {
            if (!_tv2FileDestOn || bus == null)
                return;
            if (IsAllowedTv2Startip(threadStartip) && _tv2Startip == 0)
                _tv2Startip = threadStartip;
            uint proc = _tv2Proc;
            if (proc == 0)
                return;
            TryFillFileExeStartip(bus, proc);
            try
            {
                uint p50 = bus.Read32(proc + ProcModule);
                uint p5c = bus.Read32(proc + ModuleStartip);
                uint m5c = 0;
                if (p50 != 0 && p50 != 0xDEADBEEFu && p50 != proc)
                    m5c = bus.Read32(p50 + ModuleStartip);
                uint ctxPc = 0;
                uint ctxSr = 0;
                if (_tv2Thread != 0)
                {
                    ctxPc = bus.Read32(_tv2Thread + ThreadCtxPc);
                    ctxSr = bus.Read32(_tv2Thread + ThreadCtxSr);
                }
                uint owner = 0;
                uint slot = 0;
                uint p0 = 0;
                uint p8 = 0;
                if (_tv2Thread != 0)
                    owner = bus.Read32(_tv2Thread + ThreadPrc);
                if (proc >= 0x80000000u)
                {
                    p0 = bus.Read32(proc);
                    p8 = bus.Read32(proc + 8);
                    slot = bus.Read32(proc + ProcSlot);
                }
                System.Console.WriteLine("[Hive] FILE[25] CreateProcess-ret proc=0x" +
                    proc.ToString("X8") +
                    " +0=0x" + p0.ToString("X8") +
                    " +8=0x" + p8.ToString("X8") +
                    " +0C=0x" + slot.ToString("X8") +
                    " +50=0x" + p50.ToString("X8") +
                    " +5C=0x" + p5c.ToString("X8") +
                    " module+5C=0x" + m5c.ToString("X8") +
                    " thread=0x" + _tv2Thread.ToString("X8") +
                    " thread+0C=0x" + owner.ToString("X8") +
                    " thread+5C=0x" + threadStartip.ToString("X8") +
                    " ctxPC=0x" + ctxPc.ToString("X8") +
                    " +F0=0x" + ctxSr.ToString("X8") +
                    " kept=0x" + _tv2Startip.ToString("X8") +
                    " (tv2 proc, not CurProc/filesys)");
                TryKeepTv2ThreadOwner(bus, "CreateProcess-ret");
                TryKeepTv2UserStatus(bus);
                TryKeepTv2ThreadCtx(bus, "CreateProcess-ret");
            }
            catch
            {
            }
        }

        public static bool IsDecompressLeftoverPc(uint pc)
        {
            return pc >= BinaryDecompressInner && pc < 0x80053000u;
        }

        public static bool IsExnDispatchLeftover(uint pc)
        {
            // wait82: 0x80015404 ctxPC=0x8001588C only.
            // 0x80015B9C is the live ERET2 frame; rewriting
            // it loops the switcher. Do not touch that.
            return pc == ExnAfterFetch;
        }

        // wait91-94: leftover 0x8001588C is still mid
        // 0x8001586C. 0x800159B4 or $ra,$v0 then
        // 0x80015A24 ERET mtc0 $t4,EPC ($t4=$ra=$v0).
        // That is not thread+0xEC. 0x800153E8 already
        // lw $ra,220($s0) before the 0x80015404 hook.
        // I-fetch of leftover after startip/store
        // continue returns to 0x03F6CAC0 (real insn).
        // Do not skip that to 28($sp). Do not yank
        // startip. Do not invent dest bytes.
        public static void TryResumeTv2LeftoverFetch(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (!_tv2FetchLogged || !_tv2StoreContLogged)
                return;
            if (pc != ExnAfterFetch)
                return;
            // wait98: leftover already continued past CAE8 on
            // tv2. A later I-fetch 0x8001588C is firmware
            // leftover still mid 0x8001586C (filesys TEE).
            // Re-applying 0x03F6CAC0 rewinds leftover and
            // yanks the current thread. Do not rewind.
            if (_tv2LeftoverLiveLogged)
            {
                if (_tv2LeftoverSkipLogged)
                    return;
                _tv2LeftoverSkipLogged = true;
                uint curThr = 0;
                uint cur = 0;
                try
                {
                    if (bus != null)
                        curThr = bus.Read32(ThreadPtr);
                    if (bus != null)
                        cur = bus.Read32(CurProc);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] FILE[25] leftover skip-resume pc=0x8001588C CurThread=0x" +
                    curThr.ToString("X8") +
                    " CurProc=0x" + cur.ToString("X8") +
                    " bound=0x" + _tv2Thread.ToString("X8") +
                    " (firmware leftover still mid 0x8001586C after leftover-CAE8; do not rewind 0x03F6CAC0; do not skip to 28($sp); not TV UI)");
                return;
            }
            uint resume = _tv2ImplResume != 0
                ? _tv2ImplResume
                : 0x03F6CAC0u;
            if (resume != 0x03F6CAC0u)
                return;
            uint destWord = 0;
            TryPeekWord(bus, resume, out destWord);
            uint liveRa = regs != null && regs.Length > 31 ? regs[31] : 0;
            uint liveV0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint liveT9 = regs != null && regs.Length > 25 ? regs[25] : 0;
            pc = resume;
            if (_tv2Thread != 0 && bus != null)
            {
                try
                {
                    bus.Write32(_tv2Thread + ThreadCtxPc, resume);
                }
                catch
                {
                }
            }
            _tv2LeftoverStoreFrame = true;
            TryKeepTv2StoreFrame(bus, regs);
            if (_tv2LeftoverLiveLogged)
                return;
            _tv2LeftoverLiveLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover live-pc was=0x8001588C now=0x" +
                resume.ToString("X8") +
                " dest-word=0x" + destWord.ToString("X8") +
                " ra=0x" + liveRa.ToString("X8") +
                " v0=0x" + liveV0.ToString("X8") +
                " t9=0x" + liveT9.ToString("X8") +
                " (firmware leftover still mid 0x8001586C; 0x80015A24 ERET uses $v0 not ctxPC; do not skip 0x03F6CAC0 to 28($sp); do not yank startip; not dest 0xE4DA9AA4; not a mapped page 0)");
        }

        // wait99: leftover still mid 0x8001586C after
        // leftover-CAE8. jal 0x800397B0 returned -1.
        // 0x800159B4 or $ra,$v0,$0 then mtc0 $t4,EPC
        // set EPC/ra to 0xFFFFFFFF. That or can run
        // before the skip-resume I-fetch log. After
        // leftover-CAE8, set $v0/$t4 to leftover
        // continue 0x03F6CAF0 after dest peek.
        // Leftover ERET 0x80015A24 then returns to
        // that insn. Do not rewrite 0x80015B9C. Do
        // not rewind 0x03F6CAC0. Do not skip to
        // 28($sp). Do not invent dest.
        public static void TryRestoreTv2LeftoverEret(MipsBus bus, uint[] regs, uint pc)
        {
            if (_tv2LeftoverEretLogged)
                return;
            if (!_tv2LeftoverCae8Logged)
                return;
            if (pc != LeftoverOrRa && pc != LeftoverMtc0Epc && pc != LeftoverJrRa)
                return;
            if (regs == null || regs.Length <= 31)
                return;
            uint was = pc == LeftoverOrRa ? regs[2] : (pc == LeftoverMtc0Epc ? regs[12] : regs[31]);
            if (IsFirmwareUserOrCoredllVa(was) && was != 0)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryResolveLeftoverContinue(bus, out dest, out word, out live))
                return;
            if (pc == LeftoverOrRa)
                regs[2] = dest;
            else
            {
                regs[12] = dest;
                regs[31] = dest;
            }
            _tv2LeftoverEretLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover eret-restore was=0x" +
                was.ToString("X8") +
                " at=0x" + pc.ToString("X8") +
                " dest=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cae8") +
                " (jal 0x800397B0 returned -1; leftover ERET dest; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static bool TryFixTv2LeftoverJump(MipsBus bus, uint[] regs, ref uint target)
        {
            if (_tv2LeftoverEretLogged || !_tv2LeftoverCae8Logged)
                return false;
            if (target != 0xFFFFFFFFu)
                return false;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryResolveLeftoverContinue(bus, out dest, out word, out live))
                return false;
            target = dest;
            if (regs != null && regs.Length > 31)
            {
                regs[12] = dest;
                regs[31] = dest;
            }
            _tv2LeftoverEretLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover eret-restore was=0xFFFFFFFF at=jr dest=0x" +
                dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cae8") +
                " (jal 0x800397B0 returned -1; leftover jr $ra; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
            return true;
        }

        private static bool TryResolveLeftoverContinue(MipsBus bus, out uint dest, out uint word, out bool live)
        {
            dest = LeftoverContinue;
            word = 0;
            live = TryPeekWord(bus, dest, out word);
            if (!live)
            {
                if (!_tv2LeftoverCaf0Peeked)
                    return false;
                word = _tv2LeftoverCaf0Word;
            }
            return (dest & 0x1FFFFFFFu) >= 0x00010000u;
        }

        // wait100: leftover jr to CAF0 executed the nop
        // delay and fell through, skipping beq at CAEC.
        // I-fetch 0. Not leftover mid 0x8001586C. Not
        // jr $ra ra=0 (ra was 0x03F6CB08). After CAF0,
        // continue at CAFC if leftover-CAE8 $v0==0 else
        // CAF4. Dest peek only. Do not map page 0.
        public static void TryResumeTv2LeftoverAfterCaf0(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCaf0Logged)
                return;
            if (!_tv2LeftoverCae8Logged)
                return;
            if (pc != LeftoverContinue)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryResolveLeftoverAfterCaf0(bus, out dest, out word, out live))
                return;
            pc = dest;
            _tv2LeftoverAfterCaf0Logged = true;
            uint v0 = _tv2LeftoverCae8V0Set ? _tv2LeftoverCae8V0 : 0xFFFFFFFFu;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-caf0 was=0x03F6CAF0 now=0x" +
                dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cae8") +
                " cae8-v0=0x" + v0.ToString("X8") +
                " (dest nop then fallthrough skipped beq $v0,$0,+12; do not map page 0; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait101: leftover CAFC then lw $a1,-20($s6)
        // at CB0C vaddr=0xFFFFFFEC. $s6==0 (null-20).
        // leftover-CAE8 already lw $v0,0($s6) from a
        // real process-info VA. leftover firmware
        // mid 0x8001586C clobbered $s6. Keep that
        // leftover-CAE8 $s6 after dest peek of $s6
        // and $s6-20. Do not map page 0. Do not map
        // 0xFFFFFFEC. Do not invent dest. If leftover
        // -CAE8 $s6 is 0 this is an honest null deref.
        public static void TryKeepTv2LeftoverS6(MipsBus bus, uint[] regs, uint pc)
        {
            if (pc != LeftoverCb0c)
                return;
            if (!_tv2LeftoverCae8Logged)
                return;
            if (regs == null || regs.Length <= 22)
                return;
            uint live = regs[22];
            if (live != 0)
                return;
            if (!_tv2LeftoverCae8S6Set || _tv2LeftoverCae8S6 == 0)
            {
                if (_tv2LeftoverS6Logged)
                    return;
                _tv2LeftoverS6Logged = true;
                System.Console.WriteLine("[Hive] FILE[25] leftover s6-keep skip live=0x00000000 cae8-s6=0x" +
                    _tv2LeftoverCae8S6.ToString("X8") +
                    " (leftover-CAE8 $s6 unset/0; honest null-20; do not map page 0; do not map 0xFFFFFFEC; not TV UI)");
                return;
            }
            uint keep = _tv2LeftoverCae8S6;
            if (!IsFirmwareUserOrCoredllVa(keep))
                return;
            if ((keep & 0x1FFFFFFFu) < 0x00010000u)
                return;
            uint minus20 = keep - 20u;
            if ((minus20 & 0x1FFFFFFFu) < 0x00010000u)
                return;
            uint wordS6 = 0;
            uint wordM20 = 0;
            if (!TryPeekWord(bus, keep, out wordS6))
                return;
            if (!TryPeekWord(bus, minus20, out wordM20))
                return;
            regs[22] = keep;
            if (_tv2LeftoverS6Logged)
                return;
            _tv2LeftoverS6Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover s6-keep was=0x00000000 now=0x" +
                keep.ToString("X8") +
                " s6-word=0x" + wordS6.ToString("X8") +
                " m20=0x" + minus20.ToString("X8") +
                " m20-word=0x" + wordM20.ToString("X8") +
                " (leftover-CAE8 $s6 after dest peek; lw $a1,-20($s6); do not map page 0; do not map 0xFFFFFFEC; not TV UI)");
        }

        // wait102: leftover past CB10 then tv2 ctxPC
        // is ERET2 0x80015B9C. Not the next coredll
        // insn. Not leftover mid 0x8001586C (skip-
        // resume already ran after leftover-CAE8).
        // Not OEMIdle (that is the later 600M DONE).
        // After leftover-CB10, I-fetch of ERET2 or
        // leftover 0x8001588C resumes at CB14 after
        // dest peek. Do not rewrite 0x80015B9C.
        // Do not rewind 0x03F6CAC0. Do not invent dest.
        public static void TryResumeTv2LeftoverAfterCb10(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb10Logged)
                return;
            if (!_tv2LeftoverPastCb0cLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb14, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb10Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb10 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb10") +
                " (ERET2/leftover mid after leftover CB10; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait103: leftover past CB14 then leftover
        // mid 0x8001588C and ERET2 0x80015B9C. +DC
        // later 0x03F6CB34 (beq taken). After leftover
        // -CB14, I-fetch of ERET2 or leftover 0x8001588C
        // resumes at CB34 after dest peek. Do not
        // rewrite 0x80015B9C. Do not rewind 0x03F6CAC0
        // or CB14. Do not invent dest.
        public static void TryResumeTv2LeftoverAfterCb14(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb14Logged)
                return;
            if (!_tv2LeftoverPastCb14Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb34, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb14Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb14 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb14") +
                " (ERET2/leftover mid after leftover CB14; beq taken 0x03F6CB34; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        private static bool TryResolveLeftoverAfterCaf0(MipsBus bus, out uint dest, out uint word, out bool live)
        {
            dest = 0;
            word = 0;
            live = false;
            uint prefer = LeftoverAfterCaf0;
            uint other = LeftoverBeqTaken;
            if (_tv2LeftoverCae8V0Set && _tv2LeftoverCae8V0 == 0)
            {
                prefer = LeftoverBeqTaken;
                other = LeftoverAfterCaf0;
            }
            if (TryAcceptLeftoverAfterDest(bus, prefer, out dest, out word, out live))
                return true;
            return TryAcceptLeftoverAfterDest(bus, other, out dest, out word, out live);
        }

        private static bool TryAcceptLeftoverAfterDest(MipsBus bus, uint va, out uint dest, out uint word, out bool live)
        {
            dest = va;
            word = 0;
            live = TryPeekWord(bus, va, out word);
            if (!live)
            {
                if (va == LeftoverAfterCaf0 && _tv2LeftoverCaf4Peeked)
                    word = _tv2LeftoverCaf4Word;
                else if (va == LeftoverBeqTaken && _tv2LeftoverCafcPeeked)
                    word = _tv2LeftoverCafcWord;
                else if (va == LeftoverCb14 && _tv2LeftoverCb14Peeked)
                    word = _tv2LeftoverCb14Word;
                else if (va == LeftoverCb34 && _tv2LeftoverCb34Peeked)
                    word = _tv2LeftoverCb34Word;
                else
                    return false;
            }
            if ((va & 0x1FFFFFFFu) < 0x00010000u)
                return false;
            if (word == 0 || IsFirmwareJumpToZero(word))
                return false;
            return true;
        }

        private static bool IsFirmwareJumpToZero(uint word)
        {
            uint op = word >> 26;
            uint rs = (word >> 21) & 31;
            uint funct = word & 63;
            if (op == 0 && rs == 0 && (funct == 8 || funct == 9))
                return true;
            if (op == 2 && (word & 0x3FFFFFFu) == 0)
                return true;
            return false;
        }

        public static void TryKeepTv2ThreadCtx(MipsBus bus, string tag)
        {
            if (!_tv2FileDestOn || bus == null || _tv2Thread == 0)
                return;
            if (!IsTv2PrimaryStartipReady(bus))
                return;
            uint startip = _tv2Startip;
            if (startip == 0)
            {
                try
                {
                    startip = bus.Read32(_tv2Thread + ThreadStartip);
                }
                catch
                {
                    return;
                }
            }
            if (!IsAllowedTv2Startip(startip))
                return;
            _tv2Startip = startip;
            uint ctxPc;
            try
            {
                ctxPc = bus.Read32(_tv2Thread + ThreadCtxPc);
            }
            catch
            {
                return;
            }
            if (ctxPc == startip)
                return;
            // wait82: after 0x03F6C8F8, 0x80015404 ERET
            // ctxPC=0x8001588C (mid 0x8001586C). +0xDC
            // is still 0 from 0x80020C30, so 0x80015A28
            // jr $ra fetches 0. 0x80020D80 a1=8 is Cause
            // code 2; ctxPC=0 is that EPC. Not a missing
            // page. wait83: dest 0x800908B0 / VA
            // 0x03F6C8B0 is addiu $s7, $0, 0x5800 then
            // jalr; 0x03F6C8F4 is lw $a2, 0($s7).
            // Rewriting leftover to that RA after
            // continue rewinds the lw with s7=0.
            // wait84: keeping leftover ERET to
            // 0x8001588C. 0x800159B4 or $ra, $v0
            // after 0x800397B0, then 0x80015A28
            // jr $ra. +DC was 0x03F70830 (live);
            // that or left ra=0. I-fetch 0. Do not
            // keep leftover. wait85: resume user
            // RA 0x03F6C8F4. wait86: first pass
            // already continued 0x03F6CAC0 with
            // slot-6 $sp; leftover rewrite to
            // 0x03F6C8F4 rewound and live $sp
            // became 0xE4DA9A88. Firmware would
            // not ERET that leftover after user
            // continued. Resume last continued
            // user PC. wait87 held 0x03F6CAC0.
            // wait88: leftover hook ra=0x800159A0
            // (mid 0x8001586C). 28($sp) was
            // 0x03F731E4. +D4 $sp 0x0C03F518 is
            // not that frame, so jr $ra ra=0.
            // Not a real CE jump. Do not map page 0.
            if (IsExnDispatchLeftover(ctxPc) && _tv2FetchLogged)
            {
                // wait98: leftover already continued past CAE8.
                // Rewriting ctxPC back to 0x03F6CAC0 rewinds.
                if (_tv2LeftoverCae8Logged)
                    return;
                uint resume = _tv2ImplResume != 0
                    ? _tv2ImplResume
                    : (_tv2ImplRa != 0 ? _tv2ImplRa : startip);
                if (resume == 0 || resume == ctxPc)
                    return;
                try
                {
                    uint dc = bus.Read32(_tv2Thread + ThreadCtxRa);
                    bus.Write32(_tv2Thread + ThreadCtxPc, resume);
                    TryKeepTv2UserStatus(bus);
                    TryKeepTv2UserS7(bus, null);
                    TryKeepTv2UserSp(bus, null);
                    TryKeepTv2UserRa(bus, null);
                    if (_tv2StoreContLogged && resume == 0x03F6CAC0u)
                    {
                        _tv2LeftoverStoreFrame = true;
                        TryKeepTv2StoreFrame(bus, null);
                    }
                    if (!_tv2DispatchCtxLogged)
                    {
                        _tv2DispatchCtxLogged = true;
                        System.Console.WriteLine("[Hive] FILE[25] thread ctxPC: " + tag +
                            " thr=0x" + _tv2Thread.ToString("X8") +
                            " was=0x" + ctxPc.ToString("X8") +
                            " now=0x" + resume.ToString("X8") +
                            " +DC=0x" + dc.ToString("X8") +
                            (_tv2StoreContLogged
                                ? " (firmware leftover 0x8001588C; after 0x03F6CAC0; +DC unsaved; do not rewind 0x03F6C8F4; not dest 0xE4DA9AA4; not a mapped page 0)"
                                : (_tv2ImplContLogged
                                ? " (firmware leftover 0x8001588C; after implicit-api continue; do not keep jr $ra; s7=0x5800; not a mapped page 0)"
                                : " (firmware leftover 0x8001588C; live user RA; not a mapped page 0)")));
                    }
                }
                catch
                {
                }
                return;
            }
            if (!IsDecompressLeftoverPc(ctxPc) && ctxPc != 0)
                return;
            try
            {
                bus.Write32(_tv2Thread + ThreadCtxPc, startip);
                TryKeepTv2UserStatus(bus);
                System.Console.WriteLine("[Hive] FILE[25] thread ctxPC: " + tag +
                    " thr=0x" + _tv2Thread.ToString("X8") +
                    " was=0x" + ctxPc.ToString("X8") +
                    " now=0x" + startip.ToString("X8") +
                    " (firmware +5C; CEDecompressROM leftover; not invented 0x00017F54)");
            }
            catch
            {
            }
        }

        // 0x80015370 mtc0 thread+0xF0. 3 is kernel
        // (bit 4 clear). 0x13 is firmware user so
        // 0x8001589C takes the frame/ERET path.
        // Do not map page 0. Do not poke CurProc.
        public static void TryKeepTv2UserStatus(MipsBus bus)
        {
            if (!_tv2FileDestOn || bus == null || _tv2Thread == 0)
                return;
            if (!IsTv2PrimaryStartipReady(bus))
                return;
            uint sr;
            try
            {
                sr = bus.Read32(_tv2Thread + ThreadCtxSr);
            }
            catch
            {
                return;
            }
            if (sr != 0 && sr != ThreadCtxSrKernel)
                return;
            try
            {
                bus.Write32(_tv2Thread + ThreadCtxSr, ThreadCtxSrUser);
                if (_tv2UserSrLogged)
                    return;
                _tv2UserSrLogged = true;
                System.Console.WriteLine("[Hive] FILE[25] thread +F0: user 0x" +
                    ThreadCtxSrUser.ToString("X8") +
                    " was=0x" + sr.ToString("X8") +
                    " thr=0x" + _tv2Thread.ToString("X8") +
                    " (firmware 0x8003980C; bit 4; not kernel 3; not a mapped page 0)");
            }
            catch
            {
            }
        }

        // wait83 leftover rewrite to 0x03F6C8F4 had s7=0
        // because 0x8001586C skipped 0x800154DC and
        // 0x800155C4 loaded +0xBC. 0x80015404 does not
        // restore s7. Firmware 0x03F6C8B0 is addiu $s7,
        // $0, 0x5800. wait85 leftover then s7=0xE4DA9AB8
        // (slot 114). Same unsaved +0xBC, not MMIO.
        // Do not map page 0. Do not poke CurProc.
        public static void TryKeepTv2UserS7(MipsBus bus, uint[] regs)
        {
            if (!_tv2FileDestOn || !_tv2FetchLogged || !_tv2ImplContLogged)
                return;
            if (bus == null || _tv2Thread == 0)
                return;
            if (regs != null && regs.Length > 23 && !IsFirmwareUserKdataOrSlot(regs[23]))
                regs[23] = UserKData;
            try
            {
                uint saved = bus.Read32(_tv2Thread + ThreadCtxS7);
                if (!IsFirmwareUserKdataOrSlot(saved))
                    bus.Write32(_tv2Thread + ThreadCtxS7, UserKData);
            }
            catch
            {
            }
        }

        // wait85: leftover ERET lw $sp, 212(s0). +0xD4
        // was never saved on implicit-API. Store
        // 0x03F6CABC vaddr=0xE4DA9AA4 is that $sp, not
        // BCM MMIO. Restore live/$+D4 from firmware
        // thread+0x24 when that is a process-slot VA.
        // Do not map page 0. Do not invent dest at
        // 0xE4DA9AA4. Do not poke CurProc.
        public static void TryKeepTv2UserSp(MipsBus bus, uint[] regs)
        {
            if (!_tv2FileDestOn || !_tv2FetchLogged || !_tv2ImplContLogged)
                return;
            if (bus == null || _tv2Thread == 0)
                return;
            uint stack = 0;
            uint saved = 0;
            try
            {
                stack = bus.Read32(_tv2Thread + ThreadStack);
                saved = bus.Read32(_tv2Thread + ThreadCtxSp);
            }
            catch
            {
                return;
            }
            uint src = IsFirmwareUserSlotVa(saved) ? saved : stack;
            if (!IsFirmwareUserSlotVa(src))
                return;
            uint live = regs != null && regs.Length > 29 ? regs[29] : 0;
            bool fixLive = regs != null && regs.Length > 29 && !IsFirmwareUserSlotVa(live);
            bool fixSaved = !IsFirmwareUserSlotVa(saved);
            if (!fixLive && !fixSaved)
                return;
            if (fixLive)
                regs[29] = src;
            if (fixSaved)
            {
                try
                {
                    bus.Write32(_tv2Thread + ThreadCtxSp, src);
                }
                catch
                {
                }
            }
            if (_tv2UserSpLogged || regs == null)
                return;
            _tv2UserSpLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] thread +D4: user sp=0x" +
                src.ToString("X8") +
                " live=0x" + live.ToString("X8") +
                " saved=0x" + saved.ToString("X8") +
                " thr=0x" + _tv2Thread.ToString("X8") +
                " (firmware 0x80014488 lw $sp,212(s0); leftover +0xD4; not 0xE4DA9AA4; not a mapped page 0)");
        }

        // wait87: leftover 0x8001588C -> 0x03F6CAC0
        // then jr $ra ra=0. +DC=0. Implicit-API
        // 0x8001586C never hits 0x800152CC.
        // 0x80020C30 zeros +DC. Firmware
        // 0x80014434 lw $ra, 220(s0). First pass
        // already sw $ra, 28($sp) at 0x03F6CABC
        // (AFBF001C). Landing at 0x03F6CAC0 skips
        // that sw. Restore from 28($sp) or live
        // +DC. Do not write 0. Do not map page 0.
        // Do not rewind leftover to 0x03F6C8F4.
        public static void TryKeepTv2UserRa(MipsBus bus, uint[] regs)
        {
            if (!_tv2FileDestOn || !_tv2FetchLogged || !_tv2ImplContLogged)
                return;
            if (bus == null || _tv2Thread == 0)
                return;
            uint live = regs != null && regs.Length > 31 ? regs[31] : 0;
            if (IsFirmwareUserOrCoredllVa(live))
                return;
            uint saved = 0;
            uint savedSp = 0;
            try
            {
                saved = bus.Read32(_tv2Thread + ThreadCtxRa);
                savedSp = bus.Read32(_tv2Thread + ThreadCtxSp);
            }
            catch
            {
                return;
            }
            uint keep = 0;
            uint sp = 0;
            if (IsFirmwareUserSlotVa(_tv2StoreSp))
                sp = _tv2StoreSp;
            else if (regs != null && regs.Length > 29 && IsFirmwareUserSlotVa(regs[29]))
                sp = regs[29];
            else if (IsFirmwareUserSlotVa(savedSp))
                sp = savedSp;
            uint stacked = 0;
            if (sp != 0 && TryPeekWord(bus, sp + 28, out stacked)
                && IsFirmwareUserOrCoredllVa(stacked))
                keep = stacked;
            if (keep == 0 && IsFirmwareUserOrCoredllVa(saved))
                keep = saved;
            if (keep == 0 || keep == live)
                return;
            if (regs != null && regs.Length > 31)
                regs[31] = keep;
            if (saved == 0 || !IsFirmwareUserOrCoredllVa(saved))
            {
                try
                {
                    bus.Write32(_tv2Thread + ThreadCtxRa, keep);
                }
                catch
                {
                }
            }
            if (_tv2UserRaLogged || regs == null)
                return;
            _tv2UserRaLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] thread +DC: user ra=0x" +
                keep.ToString("X8") +
                " live=0x" + live.ToString("X8") +
                " saved=0x" + saved.ToString("X8") +
                " 28($sp)=0x" + stacked.ToString("X8") +
                " thr=0x" + _tv2Thread.ToString("X8") +
                " (firmware 0x80014434 lw $ra,220(s0); leftover +0xDC; first-pass 28($sp); not a mapped page 0)");
        }

        // wait88: leftover 0x8001588C -> 0x03F6CAC0
        // skips sw $ra, 28($sp). +DC keep wrote
        // 0x03F731E4; live ra was 0x800159A0
        // (mid 0x8001586C). ERET used +D4
        // 0x0C03F518, not first-pass 0x0C03F550,
        // so lw 28($sp) then jr $ra fetched 0.
        // Land with that frame and complete the
        // skipped sw. Do not write 0. Do not map
        // page 0. Do not rewind 0x03F6C8F4.
        public static void TryKeepTv2StoreFrame(MipsBus bus, uint[] regs)
        {
            if (!_tv2LeftoverStoreFrame || !_tv2StoreContLogged)
                return;
            if (bus == null || _tv2Thread == 0)
                return;
            if (_tv2ImplResume != 0x03F6CAC0u)
                return;
            if (!IsFirmwareUserSlotVa(_tv2StoreSp))
                return;
            uint stacked = 0;
            if (!TryPeekWord(bus, _tv2StoreSp + 28, out stacked))
                return;
            uint keep = 0;
            if (IsFirmwareUserOrCoredllVa(stacked))
                keep = stacked;
            if (keep == 0)
            {
                uint saved = 0;
                try
                {
                    saved = bus.Read32(_tv2Thread + ThreadCtxRa);
                }
                catch
                {
                    return;
                }
                if (IsFirmwareUserOrCoredllVa(saved))
                    keep = saved;
            }
            if (keep == 0)
                return;
            if (!IsFirmwareUserOrCoredllVa(stacked))
            {
                try
                {
                    bus.Write32(_tv2StoreSp + 28, keep);
                    stacked = keep;
                }
                catch
                {
                    return;
                }
            }
            try
            {
                bus.Write32(_tv2Thread + ThreadCtxSp, _tv2StoreSp);
            }
            catch
            {
            }
            if (regs != null && regs.Length > 29)
                regs[29] = _tv2StoreSp;
            if (regs != null && regs.Length > 31
                && !IsFirmwareUserOrCoredllVa(regs[31]))
                regs[31] = keep;
            try
            {
                uint dc = bus.Read32(_tv2Thread + ThreadCtxRa);
                if (dc == 0 || !IsFirmwareUserOrCoredllVa(dc))
                    bus.Write32(_tv2Thread + ThreadCtxRa, keep);
            }
            catch
            {
            }
            if (_tv2StoreFrameLogged || regs == null)
                return;
            _tv2StoreFrameLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover store-frame sp=0x" +
                _tv2StoreSp.ToString("X8") +
                " ra=0x" + keep.ToString("X8") +
                " 28($sp)=0x" + stacked.ToString("X8") +
                " thr=0x" + _tv2Thread.ToString("X8") +
                " (first-pass 0x03F6CABC sw $ra,28($sp); leftover 0x03F6CAC0; not rewind 0x03F6C8F4; not a mapped page 0)");
        }

        private static bool IsFirmwareUserSlotVa(uint va)
        {
            uint slot = va >> 25;
            return va != 0 && va < 0x80000000u && slot >= 1 && slot <= 16;
        }

        private static bool IsFirmwareUserOrCoredllVa(uint va)
        {
            if (va == 0)
                return false;
            if (va >= CoredllSharedLo && va < CoredllSharedHi)
                return true;
            if (IsFirmwareUserSlotVa(va))
                return true;
            uint slot = va >> 25;
            return slot == 0 && va >= 0x00010000u && va < 0x02000000u;
        }

        private static bool IsFirmwareUserKdataOrSlot(uint va)
        {
            return va == UserKData || IsFirmwareUserSlotVa(va);
        }

        public static void TryNoteTv2ThreadRestore(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FileDestOn || _tv2Thread == 0 || bus == null || regs == null)
                return;
            if (pc != ThreadCtxRestore && pc != ThreadCtxRestore2)
                return;
            if (regs.Length <= 16)
                return;
            uint s0 = regs[16];
            if (s0 != _tv2Thread)
                return;
            TryKeepTv2UserStatus(bus);
            TryKeepTv2ThreadOwner(bus, pc == ThreadCtxRestore ? "ERET" : "ERET2");
            TryKeepTv2ThreadCtx(bus, pc == ThreadCtxRestore ? "ERET" : "ERET2");
            TryKeepTv2UserS7(bus, regs);
            TryKeepTv2UserSp(bus, regs);
            TryKeepTv2UserRa(bus, regs);
            TryKeepTv2StoreFrame(bus, regs);
            try
            {
                uint ctxPc = bus.Read32(_tv2Thread + ThreadCtxPc);
                uint startip = bus.Read32(_tv2Thread + ThreadStartip);
                uint cur = bus.Read32(CurProc);
                uint owner = bus.Read32(_tv2Thread + ThreadPrc);
                uint savedRa = bus.Read32(_tv2Thread + ThreadCtxRa);
                bool notable = ctxPc == _tv2Startip
                    || ctxPc == ExnAfterFetch
                    || ctxPc == ExnAfterFetch2
                    || ctxPc == _tv2ImplRa
                    || !_tv2RestoreLogged;
                if (notable)
                {
                    _tv2RestoreLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] thread restore pc=0x" +
                        pc.ToString("X8") +
                        " thr=0x" + s0.ToString("X8") +
                        " ctxPC=0x" + ctxPc.ToString("X8") +
                        " +5C=0x" + startip.ToString("X8") +
                        " +0C=0x" + owner.ToString("X8") +
                        " +DC=0x" + savedRa.ToString("X8") +
                        " CurProc=0x" + cur.ToString("X8"));
                }
            }
            catch
            {
            }
            TryNoteTv2ProcSwitch(bus);
        }

        public static void TryNoteTv2CurThread(MipsBus bus)
        {
            if (_tv2CurThreadLogged || _tv2Thread == 0 || bus == null)
                return;
            try
            {
                uint curThr = bus.Read32(ThreadPtr);
                if (curThr != _tv2Thread)
                    return;
                _tv2CurThreadLogged = true;
                uint cur = bus.Read32(CurProc);
                uint ctxPc = bus.Read32(_tv2Thread + ThreadCtxPc);
                System.Console.WriteLine("[Hive] FILE[25] CurThread=0x" +
                    curThr.ToString("X8") +
                    " CurProc=0x" + cur.ToString("X8") +
                    " ctxPC=0x" + ctxPc.ToString("X8") +
                    " startip=0x" + _tv2Startip.ToString("X8") +
                    " (scheduler switched onto tv2 thread)");
                TryNoteTv2ProcSwitch(bus);
            }
            catch
            {
            }
        }

        public static bool IsTv2StartipFault(uint va)
        {
            if (_tv2Startip != 0 && va == _tv2Startip)
                return true;
            if (_tv2Startip != 0
                && (va & ~0xFFFu) == (_tv2Startip & ~0xFFFu))
                return true;
            if (va >= 0x014B1000u && va < 0x014D0000u)
                return true;
            uint slot0 = va & SlotMask;
            return _mscoreeDestOn
                && slot0 >= 0x014B1000u && slot0 < 0x014D0000u;
        }

        public static bool IsTv2CoredllShared(uint va)
        {
            return va >= CoredllSharedLo && va < CoredllSharedHi;
        }

        private static uint PeekSection(MipsBus bus, uint slot)
        {
            if (bus == null || slot > 16)
                return 0;
            try
            {
                return bus.Read32(KDataSection + (slot * 4));
            }
            catch
            {
                return 0;
            }
        }

        private static bool TryPeekWord(MipsBus bus, uint va, out uint word)
        {
            word = 0;
            if (bus == null || va == 0)
                return false;
            try
            {
                word = bus.Read32(va);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 0x80040278 user walk: l1 = section[((va>>16)&0x1FF)*4],
        // l2 = l1[(((va>>12)&0xF)+3)*4]. bit1 is valid. PFN is
        // bits 10+. Do not invent a static 0x03F73000 map.
        private static bool WalkFirmwarePte(MipsBus bus, uint section, uint va,
            out uint l1, out uint l2, out uint pfn, out uint kseg)
        {
            l1 = 0;
            l2 = 0;
            pfn = 0;
            kseg = 0;
            if (bus == null || section == 0 || section == 1)
                return false;
            uint l1Ptr = section + (((va >> 16) & 0x1FFu) * 4);
            if (!TryPeekWord(bus, l1Ptr, out l1) || l1 == 0 || l1 == 1)
                return false;
            uint l2Ptr = l1 + ((((va >> 12) & 0xFu) + 3) * 4);
            if (!TryPeekWord(bus, l2Ptr, out l2) || l2 == 0)
                return false;
            if ((l2 & 2) == 0)
                return false;
            // pfn6 is the live dest. wait77: 0x40002A1A
            // pfn10 is 0x0000A000 (empty low RAM);
            // pfn6 is 0x000A8000 dest-word 0x27BDFFD8.
            // wait81: 0x400023DA pfn6 is 0x0008F000,
            // linear with 0x03F73000->0x00097000.
            // dest-word at 0x8008FE10 is 0 (delay-slot
            // nop in the ROM page), not a miss. Accept
            // pfn6 when the dest is readable. Do not
            // let pfn10 zeros win.
            uint phys6 = (l2 >> 6) << 12;
            uint dest6 = 0x80000000u | (phys6 & 0x1FFFFFFFu);
            uint word6 = 0;
            if (TryPeekWord(bus, dest6 | (va & 0xFFFu), out word6))
            {
                pfn = phys6;
                kseg = dest6;
                return true;
            }
            uint phys10 = (l2 >> 10) << 12;
            uint dest10 = 0x80000000u | (phys10 & 0x1FFFFFFFu);
            uint word10 = 0;
            if (TryPeekWord(bus, dest10 | (va & 0xFFFu), out word10) && word10 != 0)
            {
                pfn = phys10;
                kseg = dest10;
                return true;
            }
            return false;
        }

        public static void TryCacheLiveCoredllSec(MipsBus bus, uint pc)
        {
            if (_coredllLiveSec != 0 || _tv2FetchLogged || bus == null)
                return;
            if (!IsTv2CoredllShared(pc))
                return;
            uint word = 0;
            if (!TryPeekWord(bus, pc, out word) || word == 0)
                return;
            uint sec1 = PeekSection(bus, 1);
            if (sec1 == 0)
                return;
            _coredllLiveSec = sec1;
            uint l1 = 0;
            uint l2 = 0;
            uint pfn = 0;
            uint kseg = 0;
            bool pte = WalkFirmwarePte(bus, sec1, pc, out l1, out l2, out pfn, out kseg);
            if (_coredllLiveLogged)
                return;
            _coredllLiveLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] coredll live-sec=0x" +
                sec1.ToString("X8") +
                " pc=0x" + pc.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " l1=0x" + l1.ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " pfn=0x" + pfn.ToString("X8") +
                " kseg=0x" + kseg.ToString("X8") +
                (pte
                    ? " (firmware 0x80040278 walk; not a static slot map)"
                    : " (dest-mapped; PTE walk miss; do not invent a slot map)"));
        }

        public static uint MapCoredllSharedVa(MipsBus bus, uint va)
        {
            if (_coredllMapBusy || bus == null || !IsTv2CoredllShared(va))
                return va;
            // wait77 code 0x03F5xxxx-0x03FA0000 is safe from
            // first I-fetch. 0x03FDxxxx during NK CallDLL hung
            // OEMIdle before filesys. Walk those pages only
            // after tv2 startip. Not a static 0x03FD0000 map.
            if (va >= 0x03FA0000u && !_tv2FetchLogged)
                return va;
            uint sec = _coredllLiveSec != 0 ? _coredllLiveSec : PeekSection(bus, 1);
            if (sec == 0)
                return va;
            try
            {
                _coredllMapBusy = true;
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (!WalkFirmwarePte(bus, sec, va, out l1, out l2, out pfn, out kseg))
                    return va;
                uint dest = kseg | (va & 0xFFFu);
                if (dest == va)
                    return va;
                if (!_coredllMapLogged)
                {
                    _coredllMapLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] coredll PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " (firmware section; not a static slot map)");
                }
                if (va >= 0x03FA0000u && !_coredllHighLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _coredllHighLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] coredll high PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " dest-word=0x" + word.ToString("X8") +
                        " (slot-1 past 0x03FA0000; firmware PTE; not invented 0x03FD0000)");
                }
                if (!_coredllZeroLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    if (word == 0)
                    {
                        _coredllZeroLogged = true;
                        System.Console.WriteLine("[Hive] FILE[25] coredll dest-word 0 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " sec=0x" + sec.ToString("X8") +
                            " l1=0x" + l1.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " pfn=0x" + pfn.ToString("X8") +
                            " (delay-slot nop; pfn6 live; not a miss; not a static slot map)");
                    }
                }
                return dest;
            }
            finally
            {
                _coredllMapBusy = false;
            }
        }

        // After the 0xFFFFF3DA jalr, a1=0xC at 0x80020D80 is
        // Cause for TLB store (code 3). nest was 2 so the
        // general path built a frame at sp-248 (0x0C03E930)
        // and jal 0x80040278. Walk that VA's live section.
        // Slot 1 is coredll. Slot 6 is tv2 proc+0C.
        // Slot 2 (filesys) after leftover live-pc only: wait95
        // dest-unmapped 0x0407F6DC while dest 0x86FAA6DC already
        // holds 0x86FA5000 (pte-live). wait77 walked slot-2 after
        // store-continue and hung in OEMIdle.
        // Slot 0 process-info page after leftover-past only:
        // wait96 dest-unmapped 0x01FFFCA4 (same page as
        // 0x01FFFFA0). leftover 0x03F6CAE8 lw $v0,0($s6)
        // dest-word 0x8EC20000. Not leftover mid 0x8001586C.
        // Slot 0 I-fetch after leftover-CAE8 only: wait97
        // dest-unmapped 0x00044154 while dest 0x80179154 is
        // pte-live (kseg 0x80179000). Not page 0.
        // Do not invent dest. Do not invent a slot map.
        public static uint MapFirmwareSlotVa(MipsBus bus, uint va)
        {
            if (_pteMapBusy || bus == null || _tv2ImplRa == 0)
                return va;
            if (va >= 0x80000000u)
                return va;
            if (IsTv2CoredllShared(va))
                return va;
            uint slot = va >> 25;
            bool walkSlot2 = slot == 2 && _tv2LeftoverLiveLogged;
            bool walkSlot0Info = slot == 0
                && _tv2LeftoverPastLogged
                && va >= 0x01FFF000u
                && va < 0x02000000u;
            bool walkSlot0Fetch = slot == 0
                && _tv2LeftoverCae8Logged
                && va >= 0x00010000u
                && va < 0x01FFF000u;
            if (slot != 1 && slot != 6 && !walkSlot2 && !walkSlot0Info && !walkSlot0Fetch)
                return va;
            uint sec = PeekSection(bus, slot);
            if (sec == 0)
                return va;
            try
            {
                _pteMapBusy = true;
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (!WalkFirmwarePte(bus, sec, va, out l1, out l2, out pfn, out kseg))
                    return va;
                uint dest = kseg | (va & 0xFFFu);
                if (dest == va)
                    return va;
                // KSEG0 0x80000000 is physical page 0. Do not map it.
                if ((dest & 0x1FFFFFFFu) < 0x00010000u)
                    return va;
                if (walkSlot0Info && !_slot0InfoMapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _slot0InfoMapLogged = true;
                    _pteMapLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] slot-0 info PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " slot=" + slot +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " dest-word=0x" + word.ToString("X8") +
                        " (process-info leftover-past; firmware 0x80040278; dest already expanded; do not map page 0; do not invent dest bytes)");
                }
                else if (walkSlot0Fetch && !_slot0FetchMapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _slot0FetchMapLogged = true;
                    _pteMapLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] slot-0 fetch PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " slot=" + slot +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " dest-word=0x" + word.ToString("X8") +
                        " (gwes leftover-CAE8; firmware 0x80040278; dest already expanded; do not map page 0; do not invent dest bytes)");
                }
                else if (walkSlot2 && !_slot2MapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _slot2MapLogged = true;
                    _pteMapLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] slot-2 PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " slot=" + slot +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " dest-word=0x" + word.ToString("X8") +
                        " (filesys leftover-live; firmware 0x80040278; dest already expanded; do not invent dest bytes)");
                }
                else if (!_pteMapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _pteMapLogged = true;
                    System.Console.WriteLine("[Hive] FILE[25] slot PTE 0x" +
                        va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                        " slot=" + slot +
                        " sec=0x" + sec.ToString("X8") +
                        " l1=0x" + l1.ToString("X8") +
                        " l2=0x" + l2.ToString("X8") +
                        " pfn=0x" + pfn.ToString("X8") +
                        " dest-word=0x" + word.ToString("X8") +
                        " (firmware 0x80040278 walk after jalr; not a static slot map)");
                }
                return dest;
            }
            finally
            {
                _pteMapBusy = false;
            }
        }

        public static void TryNoteTv2StartipFetch(MipsBus bus, uint pc)
        {
            if (_tv2Startip == 0 || pc != _tv2Startip || _tv2FetchLogged)
                return;
            _tv2FetchLogged = true;
            uint cur = 0;
            uint owner = 0;
            uint slot = 0;
            uint curThr = 0;
            uint word = 0;
            bool mapped = false;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
                if (bus != null && _tv2Thread != 0)
                    owner = bus.Read32(_tv2Thread + ThreadPrc);
                if (bus != null && _tv2Proc != 0)
                    slot = bus.Read32(_tv2Proc + ProcSlot);
                if (bus != null)
                {
                    mapped = DestReadable(bus, pc);
                    if (mapped)
                        word = bus.Read32(pc);
                }
            }
            catch
            {
                mapped = false;
            }
            System.Console.WriteLine("[Hive] FILE[25] I-fetch startip=0x" +
                pc.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " bound=0x" + _tv2Thread.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " thread+0C=0x" + owner.ToString("X8") +
                " proc+0C=0x" + slot.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (peek only; do not invent dest bytes)");
            TryNoteTv2ProcSwitch(bus);
        }

        public static void TryNoteTv2StartipContinue(MipsBus bus, uint pc)
        {
            if (!_tv2FetchLogged || _tv2ContinueLogged || _tv2Startip == 0)
                return;
            if (pc == _tv2Startip)
                return;
            if (pc != _tv2Startip + 4
                && (pc < 0x014B1000u || pc >= 0x014D0000u))
                return;
            _tv2ContinueLogged = true;
            uint cur = 0;
            uint curThr = 0;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] startip continue pc=0x" +
                pc.ToString("X8") +
                " from=0x" + _tv2Startip.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past first instruction; peek only; not TV UI)");
        }

        public static void TryNoteTv2ExnHelper(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2FetchLogged || _tv2ExnHelperLogged)
                return;
            if (pc != SwitcherExnCall
                && (pc < ExnVmCheck || pc >= ExnVmCheckEnd))
                return;
            _tv2ExnHelperLogged = true;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint cur = 0;
            uint curThr = 0;
            uint ctxPc = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
                if (bus != null && _tv2Thread != 0)
                    ctxPc = bus.Read32(_tv2Thread + ThreadCtxPc);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] switcher VM-check pc=0x" +
                pc.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " ctxPC=0x" + ctxPc.ToString("X8") +
                " (0x80040278; not a vector; do not invent dest bytes)");
        }

        // 0x8001521C: (EPC | 0xFFFC) + 2 == 0. Any
        // 0xFFFF???? with bits 1:0 == 2 is a jalr trap
        // (0x80095A98 addiu/jalr; 0xFFFFDFEE SetFilePointer).
        public static bool IsFirmwareImplicitApi(uint epc)
        {
            return ((epc | 0xFFFCu) + 2u) == 0;
        }

        // 0x8001567C clears k1 before ERET. Copied
        // 0x80000180 is 0x80015210: bne k1, 0 skips the
        // syscall ori/addiu/beq and 0x80015484 overwrites
        // EPC with t0. Stale k1 makes 0xFFFFF3DA a fatal
        // AdEL. Do not poke CurProc.
        public static bool TryClearImplicitApiK1(uint[] regs, uint vaddr)
        {
            if (!_tv2FetchLogged || regs == null || regs.Length <= 27)
                return false;
            if (!IsFirmwareImplicitApi(vaddr))
                return false;
            _tv2ImplK1Before = regs[27];
            regs[27] = 0;
            return true;
        }

        public static void TryNoteTv2PostFetchException(uint code, uint epc, uint vaddr,
            uint vector, MipsBus bus)
        {
            TryNoteTv2PostFetchException(code, epc, vaddr, vector, bus, null);
        }

        public static void TryNoteTv2PostFetchException(uint code, uint epc, uint vaddr,
            uint vector, MipsBus bus, uint[] regs)
        {
            if (!_tv2FetchLogged || code == 0)
                return;
            bool implicitApi = IsFirmwareImplicitApi(epc) || IsFirmwareImplicitApi(vaddr);
            uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
            if (implicitApi && ra != 0 && _tv2ImplRa == 0)
            {
                _tv2ImplRa = ra;
                _tv2ImplEpc = epc != 0 ? epc : vaddr;
            }
            if (implicitApi)
            {
                if (_tv2ImplAdelLogged)
                    return;
                _tv2ImplAdelLogged = true;
            }
            else
            {
                if (_tv2PostFetchExnLogged)
                    return;
                _tv2PostFetchExnLogged = true;
            }
            uint cur = 0;
            uint curThr = 0;
            uint nest = 0;
            uint k1 = _tv2ImplK1Before != 0
                ? _tv2ImplK1Before
                : (regs != null && regs.Length > 27 ? regs[27] : 0);
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
                if (bus != null)
                    nest = bus.Read8(KDataNest);
            }
            catch
            {
            }
            bool startip = IsTv2StartipFault(epc) || IsTv2StartipFault(vaddr);
            bool coredll = IsTv2CoredllShared(epc) || IsTv2CoredllShared(vaddr);
            uint va = (vaddr != 0 && vaddr != epc) ? vaddr : epc;
            uint slot = va >> 25;
            uint sec0 = PeekSection(bus, 0);
            uint sec1 = PeekSection(bus, 1);
            uint sec6 = PeekSection(bus, 6);
            uint destWord = 0;
            bool mapped = TryPeekWord(bus, va, out destWord);
            uint l1 = 0;
            uint l2 = 0;
            uint pfn = 0;
            uint kseg = 0;
            uint walkSlot;
            if (va >= 0x00010000u && va < 0x02000000u)
                walkSlot = 0;
            else if (va < 0x80000000u && slot >= 1 && slot <= 16)
                walkSlot = slot;
            else
                walkSlot = 1u;
            uint walkSec = PeekSection(bus, walkSlot);
            if (walkSec == 0)
                walkSec = _coredllLiveSec != 0 ? _coredllLiveSec : sec1;
            bool pte = WalkFirmwarePte(bus, walkSec, va, out l1, out l2, out pfn, out kseg);
            string where;
            if (implicitApi)
                where = " (coredll jalr 0xFFFFFxxx; firmware 0x8001521C; not KData; not a slot map)";
            else if (startip)
                where = " (startip/mscoree dest; do not invent dest bytes)";
            else if (epc == LeftoverCb0c && vaddr == 0xFFFFFFECu)
                where = " (lw $a1,-20($s6); $s6==0 null-20; not a ROM page; do not map page 0)";
            else if (va >= 0x01FFF000u && va < 0x02000000u)
                where = " (process-info page; firmware PTE; not page 0; not a slot map)";
            else if (coredll)
                where = " (coredll shared slot-1; not mscoree; do not invent a slot map)";
            else if (epc == 0 && vaddr == 0)
                where = ra == 0
                    ? " (jr $ra ra=0; not a null user RA; do not map page 0)"
                    : " (I-fetch 0; do not map page 0)";
            else
                where = " (after jalr return; firmware PTE walk; not a static slot map)";
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint t9 = regs != null && regs.Length > 25 ? regs[25] : 0;
            uint s6 = regs != null && regs.Length > 22 ? regs[22] : 0;
            uint s7 = regs != null && regs.Length > 23 ? regs[23] : 0;
            uint sp = regs != null && regs.Length > 29 ? regs[29] : 0;
            uint frame = 0;
            uint retpc = 0;
            uint ctxSr = 0;
            uint savedSp = 0;
            uint savedRa = 0;
            uint thrStack = 0;
            uint epcWord = 0;
            bool epcMapped = false;
            try
            {
                if (bus != null && curThr != 0)
                    frame = bus.Read32(curThr + ThreadSyscallFrame);
                if (frame != 0)
                    TryPeekWord(bus, frame + 4, out retpc);
                if (bus != null && _tv2Thread != 0)
                    ctxSr = bus.Read32(_tv2Thread + ThreadCtxSr);
                if (bus != null && _tv2Thread != 0)
                    savedSp = bus.Read32(_tv2Thread + ThreadCtxSp);
                if (bus != null && _tv2Thread != 0)
                    savedRa = bus.Read32(_tv2Thread + ThreadCtxRa);
                if (bus != null && _tv2Thread != 0)
                    thrStack = bus.Read32(_tv2Thread + ThreadStack);
                if (epc != 0)
                    epcMapped = TryPeekWord(bus, epc, out epcWord);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] post-fetch exception code=" +
                code +
                " epc=0x" + epc.ToString("X8") +
                " vaddr=0x" + vaddr.ToString("X8") +
                " vec=0x" + vector.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " slot=" + slot +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + destWord.ToString("X8") +
                " nest=0x" + nest.ToString("X2") +
                " k1=0x" + k1.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " v0=0x" + v0.ToString("X8") +
                " t9=0x" + t9.ToString("X8") +
                " s6=0x" + s6.ToString("X8") +
                " s7=0x" + s7.ToString("X8") +
                " sp=0x" + sp.ToString("X8") +
                " +D4=0x" + savedSp.ToString("X8") +
                " +DC=0x" + savedRa.ToString("X8") +
                " +24=0x" + thrStack.ToString("X8") +
                " epc-" + (epcMapped ? "mapped" : "unmapped") +
                " epc-word=0x" + epcWord.ToString("X8") +
                " frame=0x" + frame.ToString("X8") +
                " frame+4=0x" + retpc.ToString("X8") +
                " +F0=0x" + ctxSr.ToString("X8") +
                " implicit=" + implicitApi +
                " walk-slot=" + walkSlot +
                " walk-sec=0x" + walkSec.ToString("X8") +
                " sec0=0x" + sec0.ToString("X8") +
                " sec1=0x" + sec1.ToString("X8") +
                " sec6=0x" + sec6.ToString("X8") +
                " live-sec=0x" + _coredllLiveSec.ToString("X8") +
                " l1=0x" + l1.ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " pfn=0x" + pfn.ToString("X8") +
                " kseg=0x" + kseg.ToString("X8") +
                (pte ? " pte-live" : " pte-miss") +
                where);
        }

        public static void TryNoteTv2CoredllFetch(MipsBus bus, uint pc)
        {
            if (!_tv2FetchLogged || _tv2CoredllLogged || !IsTv2CoredllShared(pc))
                return;
            _tv2CoredllLogged = true;
            uint cur = 0;
            uint curThr = 0;
            uint procSlot = 0;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint slot = pc >> 25;
            uint sec0 = PeekSection(bus, 0);
            uint sec1 = PeekSection(bus, 1);
            uint sec6 = PeekSection(bus, 6);
            uint l1 = 0;
            uint l2 = 0;
            uint pfn = 0;
            uint kseg = 0;
            uint walkSec = _coredllLiveSec != 0 ? _coredllLiveSec : sec1;
            bool pte = WalkFirmwarePte(bus, walkSec, pc, out l1, out l2, out pfn, out kseg);
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
                if (bus != null && _tv2Proc != 0)
                    procSlot = bus.Read32(_tv2Proc + ProcSlot);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] I-fetch coredll=0x" +
                pc.ToString("X8") +
                " page=0x" + (pc & ~0xFFFu).ToString("X8") +
                " slot=" + slot +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " proc+0C=0x" + procSlot.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " sec0=0x" + sec0.ToString("X8") +
                " sec1=0x" + sec1.ToString("X8") +
                " sec6=0x" + sec6.ToString("X8") +
                " live-sec=0x" + _coredllLiveSec.ToString("X8") +
                " l1=0x" + l1.ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " pfn=0x" + pfn.ToString("X8") +
                " kseg=0x" + kseg.ToString("X8") +
                (pte ? " pte-live" : " pte-miss") +
                " (coredll shared 0x03F5xxxx; not mscoree; firmware PTE; do not invent a slot map)");
        }

        public static void TryNoteTv2CoredllContinue(MipsBus bus, uint pc)
        {
            if (!_tv2CoredllLogged || _tv2CoredllContLogged)
                return;
            if (pc == 0x03F73380u)
                return;
            if (pc != 0x03F73384u
                && (pc < 0x014B1000u || pc >= 0x014D0000u))
                return;
            _tv2CoredllContLogged = true;
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] coredll continue pc=0x" +
                pc.ToString("X8") +
                " from=0x03F73380 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " (past coredll I-fetch; not TV UI)");
        }

        public static void TryNoteTv2HighContinue(MipsBus bus, uint pc)
        {
            if (!_tv2FetchLogged || !_coredllHighLogged || _tv2HighContLogged)
                return;
            if (pc != BindImpNameWalk + 4)
                return;
            _tv2HighContLogged = true;
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] coredll-high continue pc=0x" +
                pc.ToString("X8") +
                " from=0x03FD1FD8 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " (past name-walk load; not TV UI)");
        }

        public static void TryNoteTv2ImplicitContinue(MipsBus bus, uint pc)
        {
            TryNoteTv2ImplicitContinue(bus, pc, null);
        }

        public static void TryNoteTv2ImplicitContinue(MipsBus bus, uint pc, uint[] regs)
        {
            if (!_tv2FetchLogged || _tv2ImplContLogged || _tv2ImplRa == 0)
                return;
            if (pc != _tv2ImplRa)
                return;
            _tv2ImplContLogged = true;
            if (_tv2ImplRa != 0)
                _tv2ImplResume = _tv2ImplRa;
            TryKeepTv2UserS7(bus, regs);
            TryKeepTv2UserSp(bus, regs);
            uint cur = 0;
            uint curThr = 0;
            uint s7 = regs != null && regs.Length > 23 ? regs[23] : 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] implicit-api continue pc=0x" +
                pc.ToString("X8") +
                " from=0x" + (_tv2ImplEpc != 0 ? _tv2ImplEpc.ToString("X8") : "FFFFF9B2") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " s7=0x" + s7.ToString("X8") +
                " (past jalr implicit-API; not TV UI)");
        }

        public static void TryNoteTv2ImplicitPast(MipsBus bus, uint pc)
        {
            TryNoteTv2ImplicitPast(bus, pc, null);
        }

        public static void TryNoteTv2ImplicitPast(MipsBus bus, uint pc, uint[] regs)
        {
            if (!_tv2FetchLogged || !_tv2ImplContLogged || _tv2ImplPastLogged)
                return;
            if (_tv2ImplRa == 0 || pc != _tv2ImplRa + 4)
                return;
            _tv2ImplPastLogged = true;
            if (_tv2ImplRa != 0)
                _tv2ImplResume = _tv2ImplRa + 4;
            TryKeepTv2UserS7(bus, regs);
            TryKeepTv2UserSp(bus, regs);
            uint cur = 0;
            uint curThr = 0;
            uint s7 = regs != null && regs.Length > 23 ? regs[23] : 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] implicit-api past pc=0x" +
                pc.ToString("X8") +
                " from=0x" + _tv2ImplRa.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " s7=0x" + s7.ToString("X8") +
                " (past lw 0($s7); firmware 0x03F6C8B0 s7=0x5800; not a mapped page 0; not TV UI)");
        }

        public static void TryNoteTv2StoreContinue(MipsBus bus, uint pc)
        {
            TryNoteTv2StoreContinue(bus, pc, null);
        }

        public static void TryNoteTv2StoreContinue(MipsBus bus, uint pc, uint[] regs)
        {
            if (!_tv2FetchLogged || !_tv2ImplContLogged || _tv2StoreContLogged)
                return;
            if (pc != 0x03F6CAC0u)
                return;
            _tv2StoreContLogged = true;
            _tv2ImplResume = pc;
            if (regs != null && regs.Length > 29 && IsFirmwareUserSlotVa(regs[29]))
                _tv2StoreSp = regs[29];
            uint cur = 0;
            uint curThr = 0;
            uint sp = regs != null && regs.Length > 29 ? regs[29] : 0;
            uint s7 = regs != null && regs.Length > 23 ? regs[23] : 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] store continue pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CABC CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " sp=0x" + sp.ToString("X8") +
                " s7=0x" + s7.ToString("X8") +
                " (past leftover $sp store; firmware thread+0x24; not dest 0xE4DA9AA4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPast(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverLiveLogged || _tv2LeftoverPastLogged)
                return;
            if (pc != 0x03F6CAC4u)
                return;
            _tv2LeftoverPastLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CAC0 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover sw $fp,16($sp); do not skip to 28($sp); not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCae8(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverPastLogged || _tv2LeftoverCae8Logged)
                return;
            if (pc != 0x03F6CAECu)
                return;
            _tv2LeftoverCae8Logged = true;
            if (regs != null && regs.Length > 2)
            {
                _tv2LeftoverCae8V0Set = true;
                _tv2LeftoverCae8V0 = regs[2];
            }
            uint s6 = regs != null && regs.Length > 22 ? regs[22] : 0u;
            if (s6 != 0 && IsFirmwareUserOrCoredllVa(s6) && (s6 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCae8S6 = s6;
                _tv2LeftoverCae8S6Set = true;
            }
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint nextWord = 0;
            if (TryPeekWord(bus, LeftoverContinue, out nextWord)
                && (LeftoverContinue & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCaf0Peeked = true;
                _tv2LeftoverCaf0Word = nextWord;
            }
            uint caf4 = 0;
            if (TryPeekWord(bus, LeftoverAfterCaf0, out caf4)
                && (LeftoverAfterCaf0 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCaf4Peeked = true;
                _tv2LeftoverCaf4Word = caf4;
            }
            uint cafc = 0;
            if (TryPeekWord(bus, LeftoverBeqTaken, out cafc)
                && (LeftoverBeqTaken & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCafcPeeked = true;
                _tv2LeftoverCafcWord = cafc;
            }
            uint cb14 = 0;
            if (TryPeekWord(bus, LeftoverCb14, out cb14)
                && (LeftoverCb14 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb14Peeked = true;
                _tv2LeftoverCb14Word = cb14;
            }
            uint cb34 = 0;
            if (TryPeekWord(bus, LeftoverCb34, out cb34)
                && (LeftoverCb34 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb34Peeked = true;
                _tv2LeftoverCb34Word = cb34;
            }
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CAE8 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next=0x03F6CAF0 next-word=0x" + nextWord.ToString("X8") +
                " caf4-word=0x" + caf4.ToString("X8") +
                " cafc-word=0x" + cafc.ToString("X8") +
                " cb14-word=0x" + cb14.ToString("X8") +
                " cb34-word=0x" + cb34.ToString("X8") +
                " v0=0x" + (_tv2LeftoverCae8V0Set ? _tv2LeftoverCae8V0.ToString("X8") : "unset") +
                " s6=0x" + s6.ToString("X8") +
                " (past leftover lw $v0,0($s6); do not skip to 28($sp); not page 0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCaf0(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverCae8Logged || _tv2LeftoverCaf0Logged)
                return;
            if (pc != 0x03F6CAF0u)
                return;
            _tv2LeftoverCaf0Logged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CAEC CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover beq $v0,$0,+12; do not rewind 0x03F6CAC0; do not skip to 28($sp); not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastAfterCaf0(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverAfterCaf0Logged || _tv2LeftoverPastAfterLogged)
                return;
            if (pc != LeftoverAfterCaf0 && pc != LeftoverBeqTaken)
                return;
            _tv2LeftoverPastAfterLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CAF0 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover CAF0 nop; do not map page 0; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb0c(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverAfterCaf0Logged || _tv2LeftoverPastCb0cLogged)
                return;
            if (pc != LeftoverCb0cNext)
                return;
            _tv2LeftoverPastCb0cLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb14 = 0;
            if (TryPeekWord(bus, LeftoverCb14, out cb14)
                && (LeftoverCb14 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb14Peeked = true;
                _tv2LeftoverCb14Word = cb14;
            }
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CB0C CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb14-word=0x" + cb14.ToString("X8") +
                " (past leftover lw $a1,-20($s6); do not map page 0; do not map 0xFFFFFFEC; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb14(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb0cLogged || _tv2LeftoverPastCb14Logged)
                return;
            if (pc != LeftoverCb14)
                return;
            _tv2LeftoverPastCb14Logged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb34 = 0;
            if (TryPeekWord(bus, LeftoverCb34, out cb34)
                && (LeftoverCb34 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb34Peeked = true;
                _tv2LeftoverCb34Word = cb34;
            }
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CB10 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb34-word=0x" + cb34.ToString("X8") +
                " (past leftover andi $a0,$a1,1; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb34(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb14Logged || _tv2LeftoverPastCb34Logged)
                return;
            if (pc != LeftoverCb34)
                return;
            _tv2LeftoverPastCb34Logged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] leftover past pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6CB14 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover beq $a0,$0,+7; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2GwesFetch(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverCae8Logged || _tv2GwesFetchLogged)
                return;
            if (pc != 0x00044154u)
                return;
            _tv2GwesFetchLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] I-fetch gwes=0x" +
                pc.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (slot-0 leftover-CAE8; firmware PTE dest 0x80179154; do not map page 0; do not invent dest bytes)");
        }

        public static void TryNoteTv2GwesContinue(MipsBus bus, uint pc)
        {
            if (!_tv2GwesFetchLogged || _tv2GwesContLogged)
                return;
            if (pc != 0x00044158u)
                return;
            _tv2GwesContLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] gwes continue pc=0x" +
                pc.ToString("X8") +
                " from=0x00044154 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past gwes I-fetch; leftover/_CorExeMain not skipped; not page 0; not TV UI)");
        }

        public static void TryNoteTv2ZeroDestContinue(MipsBus bus, uint pc)
        {
            if (!_tv2FetchLogged || !_coredllZeroLogged || _tv2ZeroContLogged)
                return;
            if (pc != 0x03F6BE14u)
                return;
            _tv2ZeroContLogged = true;
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] dest-word-0 continue pc=0x" +
                pc.ToString("X8") +
                " from=0x03F6BE10 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " (past delay-slot nop; not TV UI)");
        }

        public static void TryNoteTv2AfterExnContinue(MipsBus bus, uint pc)
        {
            if (!_tv2PostFetchExnLogged || _tv2AfterExnContLogged)
                return;
            if (pc < 0x014B1000u || pc >= 0x014D0000u)
                return;
            _tv2AfterExnContLogged = true;
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] FILE[25] after-exn continue pc=0x" +
                pc.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " (past post-jalr exception; not TV UI)");
        }

        public static void TryNoteTv2ProcSwitch(MipsBus bus)
        {
            if (_tv2ProcSwitchLogged || _tv2Proc == 0 || bus == null)
                return;
            try
            {
                uint cur = bus.Read32(CurProc);
                if (cur != _tv2Proc)
                    return;
                _tv2ProcSwitchLogged = true;
                uint startip = 0;
                try
                {
                    startip = bus.Read32(_tv2Proc + ModuleStartip);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] FILE[25] CurProc=0x" +
                    cur.ToString("X8") +
                    " startip=0x" + startip.ToString("X8") +
                    " (thread switched onto tv2 proc)");
            }
            catch
            {
            }
        }

        private static bool TryFillFileExeStartip(MipsBus bus, uint module)
        {
            if (!_tv2FileDestOn || bus == null || module == 0 || _tv2Startip == 0)
                return false;
            if (!IsAllowedTv2Startip(_tv2Startip))
                return false;
            try
            {
                uint cur = bus.Read32(module + ModuleStartip);
                if (cur != 0 && IsAllowedTv2Startip(cur))
                    return true;
                bus.Write32(module + ModuleStartip, _tv2Startip);
                System.Console.WriteLine("[Hive] FILE[25] startip: fill 0x" +
                    module.ToString("X8") + "+0x5C=0x" +
                    _tv2Startip.ToString("X8") +
                    " (firmware thread/s3 dest; not invented 0x00017F54)");
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
                if (!TryFillFileExeStartip(bus, proc)
                    && !TryFillFileExeStartip(bus, proc + ProcModule))
                {
                    uint p50 = bus.Read32(proc + ProcModule);
                    if (p50 != 0 && p50 != proc && p50 != proc + ProcModule)
                        TryFillFileExeStartip(bus, p50);
                }
                TryFillTocStartip(bus, proc);
                TryFillTocStartip(bus, proc + ProcModule);
                uint p50Toc = bus.Read32(proc + ProcModule);
                if (p50Toc != 0 && p50Toc != proc && p50Toc != proc + ProcModule)
                    TryFillTocStartip(bus, p50Toc);
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
        // After firmware VALLOC of the slot-0 view of o32.real,
        // fetch 0x0398xxxx from 0x0198xxxx. Do not host-alias src.
        private static bool _ddiNopDestOn;
        private static uint _ddiNopSlot0;
        private static bool _mscoreeDestOn;
        private static uint _mscoreeSlot0;
        private static uint _mscoreeVbase;

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
            _ddiNopDestOn = false;
            _ddiNopSlot0 = 0;
            _mscoreeDestOn = false;
            _mscoreeSlot0 = 0;
            _ole32DestOn = false;
            _ole32Slot0 = 0;
            _ddiNopDecompRa = 0;
            _ddiNopDecompDest = 0;
            _ddiNopDecompVsize = 0;
            _ddiNopInnerCap = false;
            _ddiNopInnerPages = 0;
            _ddiNopBindHdr = false;
            _ddiNopBindName = false;
            _ddiNopBindLib = false;
            _ddiNopBindLibRet = false;
            _vallocHostN = 0;
            _vallocHostPool = VallocHostKseg;
            _heapSlotBusy = false;
            _heapSlotLogged = false;
            _heapSlotCached = 0;
            _heapOffCached = 0;
            _heapSlotCacheLogged = false;
            for (int i = 0; i < _vallocHostLo.Length; i++)
            {
                _vallocHostLo[i] = 0;
                _vallocHostHi[i] = 0;
                _vallocHostKseg[i] = 0;
            }
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

        public static uint MapDdiNopDestVa(uint va)
        {
            if (_ddiNopDestOn && _ddiNopSlot0 != 0)
            {
                if (va >= DdiNopVbase && va < 0x039B0000u)
                    va = _ddiNopSlot0 + (va - DdiNopVbase);
                if (va >= 0x01980000u && va < 0x019B0000u)
                    return ExtraRomDestKseg0 + (va - 0x01980000u);
                if (va >= 0x01F57000u && va < 0x01F67000u)
                    return ExtraRomDestKseg1 + (va - 0x01F57000u);
            }
            // wait89: RI at 0x034B7DA8 dest-word 0x603E984F.
            // TOC[46] vbase 0x034B0000. MapO32 steered
            // 0x034B1000 -> 0x014B1000. startip 0x014B9D98
            // already fetched that dest (0x27BDFFA8). Linked
            // 0x034B7DA8 is the same RVA. Slot-1 firmware
            // PTE is miss (coredll sec). Use the steered
            // dest. Do not invent dest bytes. Do not map
            // page 0. Do not invent a slot map.
            if (_mscoreeDestOn)
            {
                uint slot0 = va & SlotMask;
                if (slot0 >= 0x014B0000u && slot0 < 0x014D0000u)
                {
                    uint dest = ExtraRomDestKsegMscoree + (slot0 - 0x014B0000u);
                    if (!_tv2MscoreeSlotLogged && slot0 != va)
                    {
                        _tv2MscoreeSlotLogged = true;
                        System.Console.WriteLine("[Hive] FILE[25] mscoree dest 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " slot0=0x" + slot0.ToString("X8") +
                            " (MapO32 0x034B1000->0x014B1000; firmware CEDecompressROM; not invented dest; not a slot map)");
                    }
                    return dest;
                }
                if (va >= 0x01F32000u && va < 0x01F33000u)
                    return ExtraRomDestKsegMscoree1 + (va - 0x01F32000u);
            }
            if (_ole32DestOn && _ole32Vbase != 0 && _ole32Slot0 != 0)
            {
                uint vbase = _ole32Vbase;
                uint vbaseEnd = vbase + 0x40000u;
                if (va >= vbase && va < vbaseEnd)
                    va = _ole32Slot0 + (va - vbase);
                if (va >= 0x01940000u && va < 0x01980000u)
                    return ExtraRomDestKsegOle32 + (va - 0x01940000u);
            }
            return va;
        }

        // Firmware VirtualAlloc returned a useg base the TLB has
        // no PTE for (same class as ExtraROM dest). Host-back that
        // returned range only. Not a static 0x000E0000 map.
        // NULL+RESERVE uses CE 64K granularity (HeapAlloc of the
        // 0x70 HEAP header then hands out +0x1700 in that reserve).
        // Skip MEM_IMAGE and the process-info page.
        private static readonly uint[] _vallocHostLo = new uint[16];
        private static readonly uint[] _vallocHostHi = new uint[16];
        private static readonly uint[] _vallocHostKseg = new uint[16];
        private static int _vallocHostN;
        private static uint _vallocHostPool = VallocHostKseg;

        public static void TryHostBackValloc(uint baseVa, uint reqVa, uint size, uint type, bool alreadyMapped)
        {
            if (alreadyMapped || baseVa == 0 || baseVa >= 0x80000000u)
                return;
            if ((type & 0x01000000u) != 0)
                return;
            if ((type & 0x3000u) == 0)
                return;
            if (baseVa >= 0x00010000u && baseVa < 0x000CB000u)
                return;
            if (baseVa >= 0x01FFF000u && baseVa < 0x02000000u)
                return;
            if (size == 0)
                size = 0x1000;
            size = (size + 0xFFFu) & ~0xFFFu;
            if (reqVa == 0 && (type & 0x2000u) != 0 && size < CeAllocGranularity)
                size = CeAllocGranularity;
            uint end = baseVa + size;
            // CE returns a 64K-aligned base below dest
            // (0x01F57000 -> 0x01F50000, 0x019A8000 -> 0x019A0000).
            // Host-back [v0, dest+size] so ExtraROM o32.real
            // (TOC[33] 0x01F57xxx / slot-0 0x019A8xxx) is covered.
            // Not a static 0x000E0000 map.
            if (reqVa != 0 && reqVa < 0x80000000u)
            {
                uint reqEnd = reqVa + size;
                if (reqEnd > end)
                    end = reqEnd;
            }
            if (end <= baseVa)
                return;
            uint span = end - baseVa;
            if (_vallocHostN >= _vallocHostLo.Length)
                return;
            uint kseg = _vallocHostPool;
            if (kseg < VallocHostKseg || kseg + span > VallocHostKsegLim)
                return;
            _vallocHostLo[_vallocHostN] = baseVa;
            _vallocHostHi[_vallocHostN] = end;
            _vallocHostKseg[_vallocHostN] = kseg;
            _vallocHostN++;
            _vallocHostPool += span;
            System.Console.WriteLine("[Hive] VALLOC host-back 0x" +
                baseVa.ToString("X8") + "-0x" + end.ToString("X8") +
                " -> 0x" + kseg.ToString("X8") +
                " (firmware returned this; do not invent 0x000E0000)");
        }

        public static uint MapVallocHostVa(uint va)
        {
            for (int i = 0; i < _vallocHostN; i++)
            {
                if (va >= _vallocHostLo[i] && va < _vallocHostHi[i])
                    return _vallocHostKseg[i] + (va - _vallocHostLo[i]);
            }
            return va;
        }

        // wait42: DllMain dest+0x520 $fp=0x080E1970 is slot-4 of
        // the LocalAlloc GDI object (heap 0x080E0000+0x1970).
        // VALLOC(0x08000000) returned 0x080D0000, host-back ended
        // 0x080E0000. Firmware HEAP is the next 64K (*heap=HeaP).
        // Not a dump ExtraROM page. Not a static 0x000E0000 map.
        // wait43/44: host-back of that 64K at HeapCreate copied=0
        // (no DestMapped words yet) hid the live firmware HEAP.
        // Host-back only DestMapped pages. Retry after LocalAlloc
        // (wait45: 0x080E0000-0x080E2000, dest+0x520 gone) and
        // again at LoadDriver ret / AV-site for later HEAP pages
        // (wait45 miss 0x080E7ECC). Not a dump 0x000E0000 map.
        public static bool TryHostBackProcessHeap(MipsBus bus, uint heap)
        {
            return TryHostBackProcessHeap(bus, heap, 0);
        }

        // wait48: AV-site host-back of 0x080E6000-0x080E9000
        // included GDI +0xC8 (0x000E8370 / page 0x080E8000).
        // skipVa is that object; leave its 4K on firmware TLB.
        // 0x080E7ECC is page 0x080E7000 (ddi_nop load). DestMapped
        // only. Not a dump 0x000E0000 page.
        public static bool TryHostBackProcessHeap(MipsBus bus, uint heap, uint skipVa)
        {
            if (bus == null || heap < 0x04000000u || heap >= 0x20000000u)
                return false;
            uint slot = heap & 0xFE000000u;
            uint heapOff = heap & 0x01FFFFFF;
            if (slot == 0 || heapOff < 0x000CB000u)
                return false;
            uint lo = heap & ~0xFFFFu;
            uint hi = lo + CeAllocGranularity;
            if (hi <= lo)
                return false;
            if (VallocHostCovers(lo, hi))
                return false;
            uint skipPage = 0;
            if (skipVa != 0)
            {
                uint skipOff = skipVa & 0x01FFFFFF;
                if ((skipOff & ~0xFFFFu) == (heapOff & ~0xFFFFu))
                    skipPage = (slot | skipOff) & ~0xFFFu;
            }
            uint span = hi - lo;
            uint[] words = new uint[span / 4];
            bool[] pageOk = new bool[span / 0x1000];
            uint copied = 0;
            for (uint i = 0; i < span; i += 4)
            {
                try
                {
                    words[i / 4] = bus.Read32(lo + i);
                    copied++;
                    pageOk[i / 0x1000] = true;
                }
                catch
                {
                    words[i / 4] = 0;
                }
            }
            if (copied == 0)
            {
                System.Console.WriteLine("[Hive] process-heap host-back skip heap=0x" +
                    heap.ToString("X8") +
                    " copied=0 (wait43/44 empty 64K hid live HEAP; not a dump 0x000E0000 page)");
                return false;
            }
            bool installed = false;
            int p = 0;
            while (p < pageOk.Length)
            {
                uint page = lo + (uint)p * 0x1000u;
                if (!pageOk[p] || (skipPage != 0 && page == skipPage))
                {
                    p++;
                    continue;
                }
                uint runLo = page;
                int q = p + 1;
                while (q < pageOk.Length && pageOk[q])
                {
                    uint n = lo + (uint)q * 0x1000u;
                    if (skipPage != 0 && n == skipPage)
                        break;
                    q++;
                }
                uint runHi = lo + (uint)q * 0x1000u;
                if (!VallocHostCovers(runLo, runHi))
                {
                    InstallProcessHeapHost(bus, runLo, runHi, words, lo, heap, copied);
                    installed = true;
                }
                p = q;
            }
            return installed;
        }

        private static bool VallocHostCovers(uint lo, uint hi)
        {
            for (int i = 0; i < _vallocHostN; i++)
            {
                if (_vallocHostLo[i] <= lo && _vallocHostHi[i] >= hi)
                    return true;
            }
            return false;
        }

        private static void InstallProcessHeapHost(MipsBus bus, uint runLo, uint runHi,
            uint[] words, uint wordBase, uint heap, uint copied)
        {
            if (_vallocHostN >= _vallocHostLo.Length || runHi <= runLo)
                return;
            uint span = runHi - runLo;
            uint kseg = _vallocHostPool;
            if (kseg < VallocHostKseg || kseg + span > VallocHostKsegLim)
                return;
            _vallocHostLo[_vallocHostN] = runLo;
            _vallocHostHi[_vallocHostN] = runHi;
            _vallocHostKseg[_vallocHostN] = kseg;
            _vallocHostN++;
            _vallocHostPool += span;
            try
            {
                uint off = runLo - wordBase;
                for (uint i = 0; i < span; i += 4)
                {
                    uint va = runLo + i;
                    // wait49: do not host-back-overwrite GDI +0xC8
                    // (0x000E1700+0xC8 / 0x080E17C8). Do not poke it.
                    if ((va & 0x01FFFFFF) == 0x000E17C8u)
                    {
                        System.Console.WriteLine("[Hive] process-heap host-back skip-word va=0x" +
                            va.ToString("X8") + " word=0x" + words[(off + i) / 4].ToString("X8") +
                            " (GDI +0xC8; not a dump 0x000E0000 page)");
                        continue;
                    }
                    bus.Write32(kseg + i, words[(off + i) / 4]);
                }
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] process-heap host-back 0x" +
                runLo.ToString("X8") + "-0x" + runHi.ToString("X8") +
                " -> 0x" + kseg.ToString("X8") +
                " heap=0x" + heap.ToString("X8") +
                " copied=" + copied +
                " (firmware HEAP pages; not a dump 0x000E0000 page)");
        }

        // coredll HeapAlloc (0x03F796A4) keeps the heap in the
        // process slot (0x080E0000) and returns the slot-0 view
        // (0x000E1700). 0x800140A8 is jr $ra, so slot 0 never
        // got those PTEs. Rewrite only the 64K that holds
        // *0x01FFFFA0, and only past image end. Not a dump
        // ExtraROM page. Not a static 0x000E0000 map.
        // wait51: compare 0x0005BCA4 Read32(0x000E17C8) returned
        // va unchanged (slot-0 zeros) while the write and AV-site
        // used 0x080E17C8 -> 0x8F2217C8. No store of 0 after the
        // set. Cache the proven slot so a busy/heap-ptr miss still
        // hits that page. Do not poke +0xC8.
        public const uint HeapSignature = 0x50616548;
        private static bool _heapSlotBusy;
        private static bool _heapSlotLogged;
        private static uint _heapSlotCached;
        private static uint _heapOffCached;
        private static bool _heapSlotCacheLogged;

        public static uint MapProcessHeapSlotVa(MipsBus bus, uint va)
        {
            if (bus == null || va >= 0x02000000u)
                return va;
            uint off = va & 0x01FFFFFF;
            if (off < 0x000CB000u)
                return va;
            if (_heapSlotBusy)
                return MapCachedHeapSlot(va, off, "busy");
            try
            {
                _heapSlotBusy = true;
                uint heap = bus.Read32(ProcessHeapPtr);
                if (heap >= 0x000CB000u && heap < 0x02000000u
                    && _heapSlotCached != 0
                    && (heap & ~0xFFFFu) == _heapOffCached)
                    heap = _heapSlotCached | (heap & 0x01FFFFFF);
                if (heap < 0x04000000u || heap >= 0x20000000u)
                    return MapCachedHeapSlot(va, off, "heap-range");
                uint slot = heap & 0xFE000000u;
                uint heapOff = heap & 0x01FFFFFF;
                if (slot == 0 || heapOff < 0x000CB000u)
                    return MapCachedHeapSlot(va, off, "heap-slot");
                _heapSlotCached = slot;
                _heapOffCached = heapOff & ~0xFFFFu;
                if ((off & ~0xFFFFu) != _heapOffCached)
                    return va;
                uint slotted = slot | off;
                if (slotted == va)
                    return va;
                if (!_heapSlotLogged)
                {
                    _heapSlotLogged = true;
                    System.Console.WriteLine("[Hive] process-heap slot-0 0x" +
                        va.ToString("X8") + " -> 0x" + slotted.ToString("X8") +
                        " heap=0x" + heap.ToString("X8") +
                        " (not a dump 0x000E0000 page)");
                }
                return slotted;
            }
            catch
            {
                return MapCachedHeapSlot(va, off, "heap-read");
            }
            finally
            {
                _heapSlotBusy = false;
            }
        }

        // wait51: compare saw slot-0 0x000E17C8 / +0xC8=0 with no
        // store after 0x000631D4. Reuse the last *0x01FFFFA0 slot
        // for that same 64K. Not a static 0x000E0000 map.
        private static uint MapCachedHeapSlot(uint va, uint off, string why)
        {
            if (_heapSlotCached == 0 || _heapOffCached == 0)
                return va;
            if ((off & ~0xFFFFu) != _heapOffCached)
                return va;
            uint slotted = _heapSlotCached | off;
            if (slotted == va)
                return va;
            if (!_heapSlotCacheLogged && (off & ~3u) == 0x000E17C8u)
            {
                _heapSlotCacheLogged = true;
                System.Console.WriteLine("[Hive] process-heap slot-0 cache 0x" +
                    va.ToString("X8") + " -> 0x" + slotted.ToString("X8") +
                    " why=" + why +
                    " (wait51 compare missed live +0xC8; not a dump 0x000E0000 page)");
            }
            return slotted;
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
            if (bus == null || path == 0)
                return "";
            var sb = new System.Text.StringBuilder();
            int start = 0;
            try
            {
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
            }
            catch
            {
                return "";
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

        // ExtraROM TOC[46] only. wait61 retry is \mscoree.dll.dll
        // (same class as wait53 tv2clientce.exe.exe). Do not match
        // mscoree3_5.dll (TOC[79]).
        private static bool IsMscoreeDll(string name)
        {
            return NamesEqual(name, "mscoree.dll")
                || NamesEqual(name, "mscoree.dll.dll");
        }

        // ExtraROM TOC[34] only. Do not match oleaut32.dll (TOC[35]).
        private static bool IsOle32Dll(string name)
        {
            return NamesEqual(name, "ole32.dll")
                || NamesEqual(name, "ole32.dll.dll");
        }

        // wait53 retry is \Windows\tv2clientce.exe.exe
        private static bool IsTv2ClientCe(string name)
        {
            return NamesEqual(name, "tv2clientce.exe")
                || NamesEqual(name, "tv2clientce.exe.exe");
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
