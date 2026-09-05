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
        // 0x80016AFC walks *(0x80342B10) ROMHDR nodes
        // (lw head; node+4 ROMHDR; TOC at hdr+0x54; name at
        // entry+0x10; miss v0=2). ExtraROM 0x8134DA84 is
        // mapped but never linked, so LoadDriver/ActivateDevice
        // never sees ExtraROM TOC names without host attach.
        // Dump nk.exe DOES sw 0x80342B10. Earlier lui/lw
        // scan missed the addiu form. Linker is 0x8001728C:
        //   a2=0x803429C8 source chain ptr
        //   a1=0x80342B10 published ROMHDR list head
        //   s6=0x8001101C dump word is NK romhdr 0x802808B4
        //   (pExtensions 0x80011020 is still 32 zeros next
        //   to this; not a linker)
        // walk *0x803429C8: if node+4 == *0x8001101C,
        // 0x80017308 sw a3,(a1) publishes that source chain
        // as head; else if walk misses and a3!=0: 0x8001731C
        // lw old head; sw old,(a0 last node); sw *0x803429C8,
        // (a1) splices source chain in front.
        // Linker 0x8001728C has ONE caller: 0x80014420
        // (early kernel, before mtc0 Status at 0x8001442C).
        // One-shot. ExtraROM bytes can already be mapped
        // (host NkBinLoader at Boot), but dump never
        // publishes *0x803429C8: nk.exe .text only
        // 0x800172B8 addiu/lw of 0x29c8, no sw; ExtraROM
        // extracted PEs: zero lui 0x8034 + imm 0x29c8.
        // If live *0x803429C8 is 0 at 0x80014420, firmware
        // never links 0x8134DA84. Do not invent a
        // ROMChain_t before that jal. Do not host-write
        // 0x803429C8. ExtraROM ulCopyEntries=0, copy_table
        // empty. NK copy[0] src=0x8021F8EC dst=0x80320000
        // copy_len=0x5A4 dest_len=0x22C88. 0x80342B10 is
        // dst+0x22B10 (BSS tail past copy_len) until the
        // linker sw. Host attach is a workaround because
        // ExtraROM is unlinked.
        // LoadO32 jal CreateFileMapping 0x8003DA64 at
        // 0x800167AC is on the 0x200 TAKEN path (after
        // 0x8001665C andi/beqz skip). ExtraROM dumpToc0
        // 0x807 never reaches it. ddi_nop dest is MapO32
        // 0x8001AEB4 CreateFileMapping miss then 0x8001AECC
        // SetFilePointer (object+6>=2), not LoadO32
        // 0x800167AC. Do not set 0x200. Do not write
        // object+6. Firmware sh s5,6(fp) at 0x8001D4F0
        // only when CreateFileMapping 0x8003DA64 returns 0.
        // BuiltIn LoadLibrary never takes that jal.
        public const uint TocWalkMiss = 0x80016B74;
        public const uint TocWalkMissContinue = 0x80016B78;
        public const uint LoadE32Rom = 0x800196E4;
        public const uint LoadE32RomRet = 0x8001E3E8;
        // LoadE32 0x800196E4: addiu sp,-0x1A0; lbu v0,4(a0);
        // andi v1,v0,2. Bit 1 of object+4 is the ROM path
        // (type 7 has it; type 8 FILE does not). Then e32
        // copy and jal memcpy 0x80058B24 (e32_lite+0x1C <-
        // e32_rom+0x24, a2=0x38). Dump nk.exe: 0x80055DB0
        // is CurMSec / OEM tick, not an o32 probe. Incoming
        // a0/a1/a2 are leftover LoadE32 regs; jal a1 is
        // overwritten. Do not treat CurMSec v0=0 as LoadE32
        // fail. Do not invent a +0x5C pointer.
        public const uint LoadE32UnitCopy = 0x80058B24;
        public const uint LoadE32Frame = 0x1A0;
        public const uint LoadE32RomBit = 2;
        public const uint LoadE32BodyLim = 0x8001A800;
        // Dump nk.exe LoadE32: type-7 obj+4=7 takes the ROM
        // path (andi 2 / andi 4), memcpy e32_lite+0x1C, then
        // 0x80019990 b 0x800199A4; move v0,0. That v0=0 is
        // SUCCESS. Fail is v0=0x47E at 0x80019998 or v0=0xC1
        // ERROR_BAD_EXE_FORMAT at 0x800199A0. Epilogue
        // 0x800199A4 jr ra. Do not treat ExtraROM v0=0 as
        // miss. Do not force v0=1.
        public const uint LoadE32Ok = 0x80019990;
        public const uint LoadE32Fail47E = 0x80019998;
        public const uint LoadE32FailBadExe = 0x800199A0;
        public const uint LoadE32Epilogue = 0x800199A4;
        public const uint LoadE32Err47E = 0x47E;
        public const uint LoadE32BadExe = 0xC1;
        public const uint E32RomPublicSize = 0x24;
        public const uint E32RomPackedSize = 0x5C;
        public const uint E32RomRetryOff = 0x44;
        // Dump nk.exe ImageBase 0x80010000 PE R4000 LE:
        // 0x8005730C jr ra; mfc0 v0,Count
        // 0x80057314 jr ra; mfc0 v0,Compare
        // 0x8005731C jr ra; mtc0 a0,Compare
        // 0x8002C070 jr ra; move v0,a0
        // 0x80055DB0 CurMSec (jal ReadCount; 0x803392B0 /
        // 0x80342C60 scale). 0x800557F4 tick vs 0x80338F70;
        // MMIO 0xB04007D4. 0x80059CE8 Count+Compare stall.
        public const uint OemCurMSec = 0x80055DB0;
        public const uint OemReadCount = 0x8005730C;
        public const uint OemReadCompare = 0x80057314;
        public const uint OemWriteCompare = 0x8005731C;
        public const uint OemTickDelta = 0x800557F4;
        public const uint OemCountStall = 0x80059CE8;
        public const uint NkMoveV0A0 = 0x8002C070;
        // Dump nk.exe wrapper at 0x8001E3E0:
        //   jal 0x800196E4 LoadE32
        //   bnez v0, 0x8001E538   # LoadE32RomRet 0x8001E3E8
        //   jal 0x800165DC        # LoadO32 a0=obj a1=s7 a2=s4 a3=0
        //   bnez v0, 0x8001E538
        // 0x8001637C is a 0x400 predicate, not heap alloc:
        //   **(obj) or obj+8; andi 0x400; 0 -> v0=1; busy -> v0=0.
        // ExtraROM e32 live0 0x212E0003 & 0x400 = 0, so v0=1.
        // 0x800165DC: fp=**(obj) LiveEntry first word (not e32
        // live0 unless they alias); jal predicate; andi fp,0x200;
        // beqz -> 0x80016830 skip jal 0x8003E660 kmode thunk;
        // 0x80016848 move v0,0 success, dest never written.
        // ExtraROM LiveEntry0 is dump TOC dwFileAttributes
        // (extract 0x807), not e32 0x212E0003. Dump nk.exe
        // already decompiled: 0x80016830 is not MapO32.
        // After andi fp,0x200 beqz: 0x8001662C sw zero,
        // 0x20(sp); skip never jal 0x8003E660; 0x80016830
        // lw v0,0x20(sp); beqz 0x80016848; move v0,0; jr ra.
        // Dest out (s4) is only sw when 0x20(sp) is the
        // thunk return. Skip leaves dest 0 and still
        // succeeds. 0x8003E660 only when fp&0x200
        // (a0=-1 a1=sp+0x20 a2=s7). ExtraROM 0x807 and
        // ddi_nop 0x807 both skip it. LoadO32 jal
        // CreateFileMapping 0x8003DA64 at 0x800167AC is
        // on the 0x200 TAKEN path after 0x8001665C
        // andi/beqz skip. ExtraROM dumpToc0 0x807 never
        // reaches it. ddi_nop dest is MapO32 0x8001AEB4
        // CreateFileMapping miss then 0x8001AECC
        // SetFilePointer (object+6>=2), not LoadO32
        // 0x800167AC. Do not set 0x200.
        // Wrapper after LoadO32 v0=0:
        //   0x8001E428 andi s5,2 then jal 0x800283FC
        //   a0=0x7E000000 a2=0x1102000 VirtualAlloc-like,
        //   not CEDecompressROM
        //   0x8001E45C andi s5,0x8000 then jal 0x8001AF20
        //   (NOT MapO32: lbu obj+4 bit4; walk o32 at
        //   LiveEntry+0x18; page-sum vsizes; sw delta
        //   module+0xC; jr ra)
        //   0x8001ACC4 jal 0x80028844 is MapO32 inner.
        //   0x8001AC9C is bnez flags&0x80002000, not that jal.
        //   0x8001E4A8 lw 0x24(sp); andi 0x2000; beqz
        //   0x8001E534 v0=0xC1. 0x24(sp) is LoadE32 out
        //   (e32_imageflags). ExtraROM e32 0x212E0003
        //   has 0x2000 DLL so C1 should not fire if that
        //   copy ran. Log 0x24(sp). Do not invent 0x2000.
        // Honest miss: after BuiltIn LoadO32 skip,
        // firmware never VirtualCopys ExtraROM o32.
        // ddi_nop dest remains OpenFile/LoadDriver
        // MapO32/CEDecompressROM object+6>=2 (c1c0bc4).
        // Do not write object+6. Do not invent dest.
        // Do not invent a map at 0x8178C000.
        public const uint LoadE32WrapJal = 0x8001E3E0;
        public const uint LoadO32ThunkLookup = 0x8003CA70;
        public const uint LoadO32ThunkTail = 0x8003CE44;
        public const uint LoadE32WrapFail = 0x8001E538;
        public const uint LoadO32Rom = 0x800165DC;
        public const uint LoadO32RomRet = 0x8001E420;
        public const uint LoadO32WrapAfter = 0x8001E428;
        public const uint LoadO32Pred = 0x8001637C;
        public const uint LoadO32PredFail = 0x80016810;
        public const uint LoadO32SkipStore = 0x8001662C;
        public const uint LoadO32Andi200 = 0x8001665C;
        public const uint LoadO32CreateFileMapping = 0x800167AC;
        public const uint LoadO32SkipValloc = 0x80016830;
        public const uint LoadO32OkRet = 0x80016848;
        public const uint LoadO32WrapValloc = 0x800283FC;
        public const uint LoadO32WrapO32Walk = 0x8001AF20;
        public const uint LoadO32WrapS5Hi = 0x8001E45C;
        public const uint LoadO32WrapFlagsChk = 0x8001E4A8;
        public const uint LoadO32WrapC1 = 0x8001E534;
        // Dump nk.exe: 0x8001AC9C is bnez flags&0x80002000.
        // jal 0x80028844 is at 0x8001ACC4. nleddrvr flags
        // 0x60002020 skip 28844 then 0x8001AD50 jal
        // 0x800283FC(o32.real, size, 0x40). 0x8001AE08
        // beqz v0 then 0x8001AD4C v0=0xE ERROR_OUTOFMEMORY.
        // Wrapper 0x8001E758 passes 0xE to 0x8001E538.
        // LoadO32 0x8001E420 v0=0. Do not treat 0xE as
        // LoadO32 fail. Do not treat 0x8001AC9C as jal 28844.
        public const uint MapO32FlagsBnez = 0x8001AC9C;
        public const uint MapO32InnerJal = 0x8001ACC4;
        public const uint MapO32VallocJal = 0x8001AD50;
        public const uint E32ImageDllBit = 0x2000;
        public const uint WrapS5Bit2 = 2;
        public const uint WrapS5CallDll = 0x8000;
        public const uint BcmuartImageBase = 0x02F20000;
        public const uint BcmuartPsizeSum = 13471;
        public const uint BcmuartRealSize = 31744;
        // Extract etc/rom_meta + load_graph.json (not a live
        // log). ExtraROM phys 0x80630000–0x8134EA18.
        // TOC[63] bcmuart load_va 0x8178C000 PAST physlast.
        // TOC[33] ddi_nop load_va 0x80C68000 in-ROM. Both
        // cerom_attributes 0x807; dumpToc0&0x200=0. NK
        // coredll/fsdmgr/ceddk 0x1007 also lacks 0x200.
        public const uint ExtraRomPhysFirst = 0x80630000;
        public const uint ExtraRomPhysLast = 0x8134EA18;
        public const uint DumpTocAttr807 = 0x00000807;
        public const uint NkTocAttr1007 = 0x00001007;
        public const uint DumpTocAttr1807 = 0x00001807;
        public const uint BcmuartLoadVa = 0x8178C000;
        public const uint DdiNopLoadVa = 0x80C68000;
        public const uint LoadO32VallocOpen = 0x8003E660;
        public const uint LoadO32LockBit = 0x400;
        public const uint LoadO32VallocBit = 0x200;
        public const uint LoadE32RomBit2 = 4;
        public const uint CopyO32Rom = 0x8001AFA4;
        public const uint MapO32Rom = 0x8001AC30;
        // 0x8001ACC4 jal 0x80028844 is MapO32 inner.
        // 0x8001AC9C is flags bnez, not that jal. Dump
        // nk.exe: 28844 is not on the LoadO32 skip path.
        // ddi_nop dest remains OpenFile/LoadDriver.
        // Do not invent dest. Do not write object+6.
        public const uint MapO32RomEpilogue = 0x8001AE50;
        public const uint MapO32CreateFileMapping = 0x8001AEB4;
        public const uint MapO32SetFilePointer = 0x8001AECC;
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
        // Live 404d06b BindImp-stall pc=0x8001F7D0.
        // Same nk cluster as LoadExeE32Ret 0x8001F870.
        // BindImp jal 0x8001F7BC (ordinal GetProc) from
        // 0x80019090; ret 0x80019098. lw v1,80(a0) is
        // MODULE+0x50 BasePtr. Observe only. Do not
        // invent COREDLL BasePtr or export bytes.
        public const uint BindImpOrdLookup = 0x8001F7BC;
        public const uint BindImpOrdBaseLw = 0x8001F7D0;
        public const uint BindImpOrdJalRet = 0x80019098;
        // Live d79cd40: after beq $a2,$v0 at 0x80019104
        // BindImp addiu $a3,$0,0x5800 sign-extends to
        // 0xFFFF5800, lw 0($a3), then sw $v0,0($v1) at
        // 0x80019124. v1 was *(fp+0x1C) at 0x800190FC.
        public const uint BindImpIatKdata = 0x8001910C;
        public const uint BindImpIatSw = 0x80019124;
        public const uint BindImpIatAfter = 0x80019128;
        // Live 19656e2: lw $v1,0x1C($fp) then sw $v0,0($v1).
        public const uint BindImpIatSlotLw = 0x800190FC;
        // Live 1c3b70a: after slot0, firmware +4 *(fp+0x1C)
        // here then loops GetProc. Keep VALLOC dest+n*4.
        public const uint BindImpIatNext = 0x800192EC;
        public const uint BindImpIatNextAfter = 0x800192F0;
        public const uint BindImpFpIatOff = 0x1C;
        // Live d19770c: after IAT slot7, stall at
        // 0x8001528C sw $t1,132($s0) in the exception
        // register-save. Observe Cause/EPC/BadVAddr.
        public const uint BindImpExnLo = 0x80015240;
        public const uint BindImpExnHi = 0x8001528C;
        public const int BindImpObserveMax = 24;
        public const uint ModuleExpRva = 0x8C;
        public const uint ModuleExpEnd = 0x90;
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
        // 0x8001DD6C skips CallDLL when module+0x50 is useg
        // or 0xC2xxxxxx. ExtraROM ddi_nop VALLOC 0x01980000
        // is useg, so firmware never jalrs startip. Force
        // the existing DLL jal (a1=1) for that module only.
        public const uint XipCallDllUsegChk = 0x8001DD6C;
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
        // wait104: leftover past CB34 dest-word
        // 0x02E01025 (or $v0,$s7,$0). Then ERET2
        // 0x80015B9C. Not leftover still mid
        // 0x8001586C as the after-cb14 trigger.
        // Not OEMIdle (later 600M DONE). Next
        // dest-live insn is CB38. Resume there
        // after dest peek. Do not rewrite
        // 0x80015B9C. Do not rewind CB34.
        public const uint LeftoverCb38 = 0x03F6CB38;
        // wait105: leftover past CB38 dest-word
        // 0x8FBE0010 (lw $fp,16($sp)). Then ERET2
        // 0x80015B9C. after-cb34 already one-shot.
        // Not leftover still mid 0x8001586C.
        // Not OEMIdle (later 600M DONE). Next
        // dest-live insn is CB3C. Resume there
        // after dest peek. Do not invent dest
        // at CB3C. Do not rewrite 0x80015B9C.
        // Do not rewind CB38.
        public const uint LeftoverCb3c = 0x03F6CB3C;
        // wait106: leftover past CB3C dest-word
        // 0x8FB70014 (lw $s7,20($sp)). Then ERET2
        // 0x80015B9C. after-cb38 already one-shot.
        // Not leftover still mid 0x8001586C.
        // Not OEMIdle (later 600M DONE). Next
        // dest-live insn is CB40. Resume there
        // after dest peek. Do not invent dest
        // at CB40. Do not rewrite 0x80015B9C.
        // Do not rewind CB3C.
        public const uint LeftoverCb40 = 0x03F6CB40;
        // wait107: leftover past CB40 dest-word
        // 0x8FB60018 (lw $s6,24($sp)). Then ERET2
        // 0x80015B9C. after-cb3c already one-shot.
        // Not leftover still mid 0x8001586C.
        // Not OEMIdle (later 600M DONE). Next
        // dest-live insn is CB44. Resume there
        // after dest peek. Do not invent dest
        // at CB44. Do not rewrite 0x80015B9C.
        // Do not rewind CB40.
        public const uint LeftoverCb44 = 0x03F6CB44;
        // wait108: leftover past CB44 dest-word
        // 0x8FBF001C (lw $ra,28($sp)). Then ERET2
        // 0x80015B9C. after-cb40 already one-shot.
        // Not leftover still mid 0x8001586C.
        // Not OEMIdle (later 600M DONE). Next
        // dest-live insn is CB48. Resume there
        // after dest peek. Do not invent dest
        // at CB48. Do not rewrite 0x80015B9C.
        // Do not rewind CB44. Do not skip leftover
        // 0x03F6CAC0 to 28($sp).
        public const uint LeftoverCb48 = 0x03F6CB48;
        // wait109: leftover past CB48 dest-word
        // 0x03E00008 (jr $ra). Then leftover left.
        // after-cb44 already one-shot. Next runner
        // is ERET2 0x80015B9C. Resume at dest-live
        // delay slot CB4C after dest peek, then
        // follow live $ra (lw $ra,28($sp) at CB44).
        // Do not invent dest at CB4C. Do not
        // rewrite 0x80015B9C. Do not rewind
        // leftover. Do not skip leftover 0x03F6CAC0
        // to 28($sp).
        public const uint LeftoverCb4c = 0x03F6CB4C;
        // wait111: leftover past leftover-jr-ra dest
        // 0x03F731E4 dest-word 0x1040000A
        // (beq $v0,$0,+10). leftover left.
        // after-cb4c already one-shot. Next
        // runner is ERET2 0x80015B9C /
        // leftover mid 0x8001588C. Resume at
        // dest-live next insn after that beq
        // after dest peek of fallthrough
        // 0x03F731E8 and taken 0x03F73210.
        // Follow live $v0. Do not invent dest.
        // Do not rewrite 0x80015B9C. Do not
        // rewind leftover. Do not invent dest
        // at 0x03F731E4.
        public const uint LeftoverBeqRaFt = 0x03F731E8;
        public const uint LeftoverBeqRaTk = 0x03F73210;
        // wait112: leftover past 0x03F73210 dest-word
        // 0x10000002 (b +2). leftover left.
        // after-jr-ra already one-shot (did not
        // fire). Next runner is ERET2 0x80015B9C
        // / leftover mid 0x8001588C. Resume at
        // dest-live next insn after that branch
        // after dest peek of delay 0x03F73214
        // and taken 0x03F7321C. Do not invent
        // dest. Do not rewrite 0x80015B9C. Do
        // not rewind leftover. Do not invent
        // dest at 0x03F731E4.
        public const uint LeftoverBPlus2Delay = 0x03F73214;
        public const uint LeftoverBPlus2Taken = 0x03F7321C;
        // wait113: leftover past 0x03F7321C dest-word
        // 0x03C0E825 (or $sp,$s8,$0). leftover left.
        // after-b+2 already one-shot (did not fire).
        // Next runner is ERET2 0x80015B9C /
        // leftover mid 0x8001588C. Resume at
        // dest-live next insn 0x03F73220 after
        // dest peek. Do not invent dest. Do not
        // rewrite 0x80015B9C. Do not rewind
        // leftover. Do not invent dest at
        // 0x03F731E4.
        public const uint LeftoverBPlus2Next = 0x03F73220;
        // wait114: leftover past 0x03F73220 dest-word
        // 0x8FBE0010 (lw $fp,16($sp)). leftover left.
        // after-taken already one-shot (did not
        // fire). Next runner is ERET2 0x80015B9C
        // / leftover mid 0x8001588C. Resume at
        // dest-live next insn 0x03F73224 after
        // dest peek. Do not invent dest. Do not
        // rewrite 0x80015B9C. Do not rewind
        // leftover. Do not invent dest at
        // 0x03F731E4.
        public const uint LeftoverFpNext = 0x03F73224;
        // wait115: leftover past 0x03F73224 dest-word
        // 0x8FB70014 (lw $s7,20($sp)). leftover left.
        // after-fp already one-shot. Next runner
        // is ERET2 0x80015B9C / leftover mid
        // 0x8001588C. Resume at dest-live next
        // insn 0x03F73228 after dest peek. Do
        // not invent dest. Do not rewrite
        // 0x80015B9C. Do not rewind leftover.
        // Do not invent dest at 0x03F731E4.
        public const uint LeftoverS7Next = 0x03F73228;
        // wait117: leftover dest after leftover-past-
        // 0x03F73228 (lw $s6,24($sp)). leftover left.
        // after-s7 already one-shot. Next runner
        // is ERET2 0x80015B9C / leftover mid
        // 0x8001588C. Resume at dest-live next
        // insn 0x03F7322C after dest peek. Do
        // not invent dest. Do not rewrite
        // 0x80015B9C. Do not rewind leftover.
        public const uint LeftoverS6Next = 0x03F7322C;
        // wait118: leftover dest after leftover-past-
        // 0x03F7322C (lw $s5,28($sp)). leftover left.
        // after-s6 already one-shot. Next runner
        // is ERET2 0x80015B9C / leftover mid
        // 0x8001588C. Resume at dest-live next
        // insn 0x03F73230 after dest peek. Do
        // not invent dest. Do not rewrite
        // 0x80015B9C. Do not rewind leftover.
        public const uint LeftoverS5Next = 0x03F73230;
        // wait119: leftover dest after leftover-past-
        // 0x03F73230 (lw $s4,32($sp)). leftover left.
        // after-s5 already one-shot. Next runner
        // is ERET2 0x80015B9C / leftover mid
        // 0x8001588C. Resume at dest-live next
        // insn 0x03F73234 after dest peek. Do
        // not invent dest. Do not rewrite
        // 0x80015B9C. Do not rewind leftover.
        public const uint LeftoverS4Next = 0x03F73234;
        // wait120: leftover dest-live next after leftover-past
        // 0x03F73234 (lw $ra,36($sp)). Peek first. Do not
        // invent dest. leftover dest-live continue, not a
        // one-shot-per-insn.
        public const uint LeftoverEpilogueNext = 0x03F73238;
        // leftover-drop: leftover dest-live resume hijacks
        // leftover mid / ERET2 I-fetch. leftover ERET
        // 0x80015A24 uses $v0 not leftover ctxPC. leftover
        // $v0 restore is one-shot leftover-CAE8 dest. After
        // leftover dest-live lw leftover $v0 stays leftover
        // mid. leftover ERET returns leftover mid / ERET2.
        // leftover dest-live continue leftover ERET $v0
        // restore dest-live next. leftover dest-live
        // continue stays live after leftover dest-live
        // delay. wait124: leftover dest-live ERET $v0
        // restore after dest-live delay wrote dest-live
        // $ra. leftover already past dest-live $ra
        // (leftover past jr $ra). dest-live $ra is
        // already walked. leftover I-fetch after
        // dest-live delay is leftover mid / leftover
        // dest-live delay's live leftover next first,
        // not dest-live $ra. leftover still I-fetches
        // ERET2 and PC+4. After dest-live delay,
        // dest-live next is leftover dest-live delay's
        // live leftover next (leftover $ra at dest-live
        // jr $ra if live leftover dest), not dest-live
        // $ra, not PC+4. leftover dest-live ERET $v0
        // restore writes leftover $v0 to leftover
        // dest-live delay's live leftover next. prior
        // peek named 0x03F731E4 as evidence only; do
        // not invent dest. Do not follow dest-live $ra
        // blindly. leftover DISPATCH after leftover
        // dest-live I-fetch must not yank leftover
        // ctxPC to ERET2. Do not hop 0x03F73238. Do
        // not rewrite 0x80015B9C.
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
        // Live 147e54f: I-fetch TLBL 0x03FB492C (IAT slot6).
        // ImageBase keep-imagebase=0x03F50000. MapCoredllSharedVa
        // still refuses >=0x03FA0000 until tv2 startip
        // (wait77 OEMIdle). After DllMain, demand-map any
        // remaining COREDLL page via slot-1 firmware PTE.
        // Live 1bba9df: filesys-slot4 mapped. Next
        // data-TLBL epc=0x0001E4DC badvaddr=0x09F574F8.
        // Slot 4 view of IB page 0x03F57000→0x8007B000.
        // Relative [0x01F50000, 0x01FF0000). Slot 0 is
        // IAT real 0x01F57000 — exclude. Do not rewrite
        // ImageBase. Do not lift MapCoredllSharedVa
        // 0x03FA0000 cap. Live bb6cdc7: BindImp-exn
        // cause=2 epc=0x800467E4 badvaddr=0x03FE135C
        // (page 0x03FE1000, rel 0x01FE1000) sat one
        // page past the old 0x01FE0000 hi.
        public const int CoredllImagePageCap = 32;
        public const uint CoredllImageRelLo = 0x01F50000;
        public const uint CoredllImageRelHi = 0x01FF0000;
        public const uint BindImpNameWalk = 0x80018580;
        // KDataNest 0xFFFFD885 is cNest at KData+0x85.
        // UserKData 0x5800 addiu sign-extends to this page.
        public const uint KDataBase = 0xFFFFD800;
        public const uint UserKPage = 0xFFFF5800;
        public const uint KDataSection = 0xFFFFD8C0;
        // Live 258ef59: coredll slot-4 aliased. Next
        // data-TLBL epc=0x000593C8 badvaddr=0xFFFFFCE1
        // a1=1 v0=0x00013320 v1=0x78 stores=24.
        // Live 674d704: lh t8,-800(s7) insn=0x86F8FCE0
        // rs=23 base=1 formed=0xFFFFFCE1. Page
        // 0xFFFFF000 is SharedUserData wrap, not
        // UserKPage 0xFFFF5800 / KData 0xFFFFD800.
        // Map only live firmware peek or TLB PFN.
        // Do not invent KData / TickCount.
        public const uint FfffF000Page = 0xFFFFF000;
        public const uint FfffFce1Fault = 0xFFFFFCE1;
        public const uint FfffFce1Epc = 0x000593C8;
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
        public const uint ThreadLastErr = 56;
        public const uint ThreadStack = 0x24;
        // Dump 0x800158C8 lw $t2,44($t3) then
        // 0x800158CC sw $t2,36($t3) and
        // 0x800158DC addiu $sp,$t2,-48. +0x2C is
        // the implicit-API stack cookie.
        // 0x80030210 sw $v0,44($fp) writes
        // (ThreadStack&0xFFFF)+$s0 there.
        // Live fb58a7e: that word plus -48 is
        // 0xC201FE88 (slot97 image-low).
        public const uint ThreadStackAlt = 0x2C;
        // Dump 0x800399A4 lw $s3,-688($v0) with
        // $v0=0x80340000, then 0x800399E8
        // or $v0,$s3. leftover 0x800159B4
        // or $ra,$v0; 0x80015A08 mtc0 $t4,$14.
        // That word is the 0x800397B0 resume
        // plant (wait99: -1 → EPC 0xFFFFFFFF).
        // Dump 0x800399A4 lw $s3,-688; branches to
        // 0x800399A8 skip that load so $s3 stays
        // stale. 0x800399E8 or $v0,$s3 returns it.
        // Sole ROM store 0x800370F8 is a GetProc
        // delay-slot (jal 0x8001C468 a1=6), not a
        // per-exception EPC. Live f66919d: plant
        // still -1 after adel-pc gone; leftover
        // dest hop then Code-10 spin. Replay
        // thread+0xEC or refuse leftover ERET.
        public const uint ExnContinueWord = 0x8033FD50;
        public const uint LeftoverDestLo = 0x03F6C000;
        public const uint LeftoverDestHi = 0x03F80000;
        // Live 8d10132: plant-fix +EC=0x800382F8
        // +DC=0x8003B05C hung LoadO32. Dump:
        // 0x8003B054 jal 0x80038294 (handle
        // lookup); +EC is mid that callee
        // (beq $t5,$0); +DC is the jal return
        // (bne $v0,$0). Replay +EC with $ra=
        // +EC then jr $ra loops. Poison mid.
        // Do not leftover hop. Do not invent dest.
        public const uint HandleLookupJal = 0x80038294;
        public const uint HandleLookupEnd = 0x80038340;
        public const uint HandleLookupRet = 0x8003B04C;
        public const uint HandleLookupRetEnd = 0x8003B080;
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
        public const uint RomHdrListPtr = 0x80342B10;
        public const uint RomHdrSrcChain = 0x803429C8;
        public const uint RomHdrWalk = 0x80016AFC;
        public const uint RomHdrLink = 0x8001728C;
        public const uint RomHdrLinkJal = 0x80014420;
        public const uint RomHdrLinkJalStatus = 0x8001442C;
        public const uint RomHdrSrcChainLw = 0x800172B8;
        public const uint RomHdrLinkPublish = 0x80017308;
        public const uint RomHdrLinkSplice = 0x8001731C;
        public const uint ExtraRomDumpHdr = 0x8134DA84;
        public const uint NkDumpHdr = 0x802808B4;
        public const uint NkRomHdrPtr = 0x8001101C;
        public const uint RomHdrCopyEntries = 0x20;
        public const uint RomHdrCopyOffset = 0x24;
        public const uint RomHdrExtensions = 0x48;
        public const uint NkPExtensions = 0x80011020;
        public const uint NkCopy0Src = 0x8021F8EC;
        public const uint NkCopy0Dst = 0x80320000;
        public const uint NkCopy0CopyLen = 0x5A4;
        public const uint NkCopy0DestLen = 0x22C88;
        public const uint RomHdrListBssOff = 0x22B10;
        public const uint CreateFileMappingObj6 = 0x8001D4F0;
        public const uint RomHdrListLoad0 = 0x80016B1C;
        public const uint RomHdrListLoad1 = 0x8001B670;
        public const uint RomHdrListLoad2 = 0x80022BEC;
        public const uint RomHdrListLoad3 = 0x80036F6C;
        public const uint RomHdrListLoad4 = 0x800458E8;
        public const uint RomHdrListLoad5 = 0x80045C74;
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
        // Scratch for ExtraROM FILE OpenFile after FILE[25].
        // FILE[11] 932864/356579 and FILE[26] 6398464/2612926.
        // After VallocHostKsegLim. FILE[25] dest stays
        // Tv2FileDest (5120). Not ExtraROM tail and not a
        // dump 0x81360000 map.
        public const uint ExtraRomFileDest = 0x8F400000;
        public const uint ExtraRomFileSrc = 0x8FC00000;
        public const uint ExtraRomFileCacheMax = 0x400000;
        public const uint ExtraRomFileDestMax = 0x800000;
        public const int ExtraRomFileMax = 48;
        public const uint O32RomSize = 0x18;
        public const uint O32LiteSize = 0x1C;
        // Public CE: e32_rom is 0x24, then o32_rom[objcnt].
        // Host dump e32 then dump o32 after that copy. Do not
        // pack o32 at +0x5C (that was leftover CurMSec a1).
        // coredll 0x03F7A960 bne v0,0 / delay sw v0, (0x01FFFFA0).
        // HeapCreate(0,0,0) returned 0 in device.exe and the delay
        // slot wrote that 0 over the heap filesys already stored.
        // 0x01FFF000 is one physical page here, so that wipe makes
        // LocalAlloc call HeapAlloc(0) and RegOpen returns 14.
        public const uint HeapCreateStore = 0x03F7A964;
        public const uint ProcessHeapPtr = 0x01FFFFA0;
        // Live edf15b0: after IAT stores=24, TLBL cause=2
        // epc=0x03F6C908 lw $v0,0($s5). $s5==BadVAddr==
        // 0x01FFFCA4. Same page as *0x01FFFFA0 / wait96.
        public const uint ProcessInfoPage = 0x01FFF000;
        public const uint ProcessInfoFaultVa = 0x01FFFCA4;
        // Live 6b8a9eb: after DllMain, I-fetch TLBL
        // epc==badvaddr==0x0005D2E0. In-tree gwes
        // Display 0x0005D250 (GwesVaDispAlloc) is the
        // same page. Not COREDLL RVA 0x5D2E0 — do not
        // invent 0x03FAD2E0.
        public const uint GwesDispFetchPage = 0x0005D000;
        public const uint GwesDispFetchFault = 0x0005D2E0;
        // Live 4f43fe4: after gwes-disp fetch map, data
        // TLBL epc=0x0005D310 badvaddr=0x000B6008.
        // In-tree GwesIatGetProc. v0=0x000B0000 is the
        // gwes IAT/data region. Same image as vbase
        // 0x00010000 / vsize 0xBB000. Do not invent dest.
        public const uint GwesDispDataPage = 0x000B6000;
        public const uint GwesDispDataFault = 0x000B6008;
        // Live 8623be5: after IAT data map, NK 0x80020174
        // (near ThreadContextSetup 0x80020BE4) data-TLBL
        // badvaddr=0x00011C10. Same page as gwes VA
        // 0x00011000 (GwesRomText) and FILESYS API table
        // 0x000111A8. Do not invent dest / sipcfg dest.
        public const uint GwesTextBasePage = 0x00011000;
        public const uint GwesTextBaseFault = 0x00011C10;
        // Live 04b8c34: after gwes-text map, Display
        // 0x0005D380 data-TLBL badvaddr=0x000B7CA8.
        // Same gwes data region as v0=0x000B0000 /
        // GwesInitFlag 0x000B7A1D (page 0x000B7000).
        // Adjacent to mapped IAT 0x000B6000. Do not
        // invent dest.
        public const uint GwesDispData2Page = 0x000B7000;
        public const uint GwesDispData2Fault = 0x000B7CA8;
        // Live 5db4c8e: after data2 map, Display 0x0005D38C
        // data-TLBL badvaddr=0x000BA954. In-tree GwesDispObj
        // (LocalAlloc 584 result). Page 0x000BA000 is still
        // in gwes image (vbase 0x00010000 / vsize 0xBB000).
        // Skipped B8000/B9000 - not an adjacent walk. v0=0
        // (unlike prior 0x000B0000 IAT base). Do not invent
        // dest.
        public const uint GwesDispData3Page = 0x000BA000;
        public const uint GwesDispData3Fault = 0x000BA954;
        // Live c36c2a4: after data3 map, I-fetch TLBL
        // epc==badvaddr==0x00014B3C. Next gwes .text page
        // after 0x00011000 (ROM 0x80149B3C = GwesRomText +
        // 0x3B3C). Before WinMain 0x00016014 / entry
        // 0x000163C8. Skipped 0x00012000/0x00013000 - not
        // a successive adjacent walk. v0=0x000E1700 is
        // leftover GwesDispObj dest-word. Do not invent
        // dest or steal tv2 PE 0x00014000.
        public const uint GwesText2Page = 0x00014000;
        public const uint GwesText2Fault = 0x00014B3C;
        // Live 831a196: o32-rom .text live (0x00026000
        // → 0x8015B000). Next data-TLBL epc=0x00026130
        // badvaddr=0x00010004 v0=0x00010000
        // a1=0x00011918 stores=24. TOC[7] ImageBase
        // 0x00010000 is headers / pre-.text. .text
        // realaddr starts 0x00011000 / dataptr
        // 0x80146000. Slot 0 page is shared with
        // filesys — TOC gwes dest only. Do not invent
        // PE bytes. Do not invent SharedUserData.
        public const uint GwesImageBasePage = 0x00010000;
        public const uint GwesImageBaseFault = 0x00010004;
        // Live 187f5be: I-fetch TLBL 0x000B4B80 (page
        // 0x000B4000). Same page as jal 0x000B4D20
        // (IAT LocalAlloc thunk 0x000B60D0). Same miss
        // class as prior gwes text/data pages. Image
        // vbase 0x00010000 / vsize 0xBB000. Named pages
        // keep their Hive tags; new pages demand-map
        // via firmware PTE only. ImageBase headers
        // are 0x00010000 (not this .text span).
        // Do not invent dest.
        public const uint GwesImageLo = 0x00011000;
        public const uint GwesImageHi = 0x000CB000;
        // Live 7214ee6: o32-sec correctly refused
        // (TOC[7] o32[1] .data real 0x000B6000 vsize
        // 0x50E4 psize 0xCF5 dataptr 0x802852C8
        // flags 0xC0002040 compressed; page-off 0x3000
        // >= psize). Naive 0x80288000 is invalid.
        // Decompressed page is all-zero (0x000B9FF4=0).
        // dest-word=0 is dump truth. 258ef59 won
        // 0x000B9000→0x86F35000 dest-word=0 via
        // firmware PTE. Do not invent XIP. Do not
        // stretch .text. Do not hard-done pte-miss.
        public const uint GwesDataB9Page = 0x000B9000;
        public const uint GwesDataB9Fault = 0x000B9FF4;
        // Live c0347e8: dest0 map won after a transient
        // TLBL / BindImp-exn. Hive then froze while the
        // host burned CPU. Do not consume BindImp-exn
        // on that dest0 refill. One spin-observe if PC
        // sticks after the map. Do not invent XIP.
        // Live 98db5d5: 256K/16K never fired. Page
        // changes reset the counter during the
        // exception storm. Count total steps after
        // the B9 map. Do not reset on page change.
        public const int GwesDataB9SpinSame = 65536;
        public const int GwesDataB9SpinVec = 4096;
        // Live 98db5d5: after B9 dest0 + B9 skip,
        // BindImp-exn cause=3 epc=0x00021ABC
        // badvaddr=0 (TLBS store to null). Observe
        // insn/rs/rt/base. Do not map VA 0. Do not
        // invent SharedUserData / KData / dest.
        public const uint GwesNullStoreEpc = 0x00021ABC;
        // Live 73486bc: after 0x04021000→0x80115000,
        // BindImp-exn cause=2 epc=0x80052010
        // badvaddr=0x50 a1=0x50 v0=0x74. Near-null
        // TLBL. Observe insn/rs/rt/base. Do not
        // map VA 0 / page 0. Do not invent
        // SharedUserData / KData / dest.
        public const uint NearNullTlblEpc = 0x80052010;
        public const uint NearNullTlblVaddr = 0x00000050;
        public const uint NearNullPageHi = 0x00001000;
        // Live f3c2d62: after near-null observe,
        // BindImp-exn cause=4 epc=badvaddr=0xFFFFFB2A
        // (AdEL; 0xB2A unaligned). Observe only.
        // Do not map 0xFFFFF000. Do not invent
        // SharedUserData / KData / dest. FFFF*
        // AdEL must not consume BindImp-exn.
        public const uint FfffFb2aEpc = 0xFFFFFB2A;
        public const uint FfffFb2aVaddr = 0xFFFFFB2A;
        // Live 3ac5ed9: after 0x03FE1000 map,
        // BindImp-exn cause=4 epc=badvaddr=0xC6FA7C9A
        // (AdEL; 0x7C9A unaligned; not a module
        // VA). Observe only. Do not map that VA.
        // Do not invent dest. All AdEL /
        // epc==badvaddr unaligned must not
        // consume BindImp-exn.
        public const uint AdelC6FaEpc = 0xC6FA7C9A;
        public const uint AdelC6FaVaddr = 0xC6FA7C9A;
        // Live 3275fe9: after adel-pc, BindImp-exn
        // cause=3 epc=0x80031D38 badvaddr=0xC201FE84
        // a1=0x8033FE1C (nk ROM). Observe insn /
        // rs / rt / base / formed. Do not invent
        // dest for 0xC2xxxxxx. WalkFirmwarePte
        // L1 ((va>>16)&0x1FF) aliases 0xC201xxxx
        // to 0x0001xxxx — not a C2 PTE. Do not
        // map. C2* TLBS does not consume
        // BindImp-exn.
        // Live 7827498: insn is sw ra,60(sp). Dump
        // nk.exe 0x80031D34 is addiu $sp,-64 then
        // that store (ThreadPtr 0xFFFFDAC0). Incoming
        // $sp was 0xC201FE88. Slot 97 (NK.EXE) +
        // 0x0001FE48 is image, not a thread stack.
        // a1=0x8033FE1C is past B000FF end
        // 0x8031B3BC (caller arg, not the C2 stack).
        // Do not map 0xC201F000. Observe first C2
        // $sp. Do not invent dest.
        public const uint C2TlbsEpc = 0x80031D38;
        public const uint C2TlbsVaddr = 0xC201FE84;
        public const uint C2VaPrefix = 0xC2000000;
        public const uint C2TlbsFunc = 0x80031D34;
        public const uint NkImageEnd = 0x8031B3BC;
        // Live 155d918 / fb58a7e: first C2 $sp at
        // 0x80015664. Dump: 0x80015660 lw $sp,
        // 212($s0) (thread+0xD4). 0x8001563C lw
        // $ra,220($s0) is 0x80030264 (saved $ra,
        // not EPC). 0x8001566C lw $k0,236($s0)
        // then ERET. +0xD4 writers: 0x80015264
        // sw $sp,212($t0) only when nest==1
        // (0xFFFFD885); nest!=1 takes 0x80015488
        // ($sp-248, not the thread). 0x80020BF4
        // ThreadContextSetup v0=a1+a2-256 → +0x24
        // / v1=v0-48 → +0xD4. 0x80030210 writes
        // +0x2C; implicit-API 0x800158DC does
        // $sp=+0x2C-48. 0xC201FE88 = slot97+
        // 0x1FE88 (image, not a stack). Adel-pc
        // $sp was not C2 (nested). Replay that
        // $sp into +0xD4 when +0xEC is a sane
        // aligned NK PC. Else refuse ERET. Do
        // not map 0xC201F000. Do not hop EPC
        // to 0x80030264. Do not leftover/ERET2.
        // Live 695e734: +EC=0x800373C0 is mid NK
        // idle (dump: jal 0x80031D34 poll at
        // 0x800373CC). After AdEL, COP0 EPC is
        // still 0xC6FA7C9A at 0x80015664. sp-fix
        // then ERET2 resumes that idle / re-AdEL.
        // Refuse ERET while EPC is adel-pc poison.
        // Live cf2477b: after sp-fix $sp is
        // 0x040DFE80 (not C2). Live PeekEpc is
        // already rewritten (plant / +EC idle).
        // C2-$sp gate and live-EPC==C6FA both
        // missed; zero epc-halt; silent CPU
        // burn. Latch adel-pc EPC. Refuse ERET
        // while that latch is set. Do not
        // leftover hop. Do not invent dest.
        // Live aa0b26c: epc-halt fired. This
        // Boot +D4=0x040DFE60 (not C2). Poison
        // is live EPC and +DC=0xC6FA7C9A.
        // Dump leftover: 0x800159A8 jal
        // 0x800397B0; 0x800159B4 or $ra,$v0;
        // 0x800159CC or $t4,$ra; 0x80015A08
        // mtc0 $t4,$14 (wait99). Exception
        // 0x800152CC sw $ra,220($s0) saves
        // that $ra to +DC. +EC this Boot is
        // 0x80015B9C (ExnAfterFetch2; aligned
        // NK leftover mid, not adel resume).
        // Replay +EC into COP0 EPC / +DC / $ra
        // only when that is a sane aligned
        // NK/useg PC, not leftover mid / idle.
        // Clear latch only then. Do not leftover
        // hop. Do not invent dest.
        // Live 3b847b7: plant-clr first-win
        // (+EC=0x80015B9C). Later C2 $sp
        // 0xC201FE88; sp-fix +EC=0x800373C0
        // (dump: or $a3,$s0 then jal
        // 0x80031D34 poll). Latch already
        // clear; ERET2 idle; silent CPU burn
        // (same as 695e734). Refuse ERET when
        // +EC is that idle mid-poll. Do not
        // leftover hop. Do not invent dest.
        public const uint C2SpLoadPc = 0x80015660;
        public const uint C2SpFirstPc = 0x80015664;
        public const uint ThreadCtxEret = 0x8001568C;
        public const uint NkIdleJal = 0x800373C0;
        // Live ac46757 leftover-frame +5C=
        // 0x800356FC. Dump: addiu $sp,-32 then
        // jal 0x80031D34 (same idle poll as
        // 0x800373CC). Thread startip, not a
        // leftover resume. leftover-frame
        // +18/v016=0; FD50/ra40 leftover dest.
        // leftover-halt stays. Do not leftover
        // hop. Do not invent dest.
        public const uint NkIdleStart = 0x800356FC;
        public const uint C2SlotImageHi = 0x00100000;
        public const int GwesImagePageCap = 32;
        // TOC[7] o32[0] dataptr. Same as HostHardDisk.
        // VA 0x00011000 → 0x80146000. Live d01f68a:
        // 0x00059000 → 0x86FA1000 dest-word=0 (RAM).
        // ROM page = 0x80146000+(0x59000-0x11000)
        // = 0x8018E000. Dump insn at 0x000593C8 is
        // 0x15400002, not 0x86F8FCE0. Dest-word=0
        // .text falls back to this o32 page.
        public const uint GwesRomText = 0x80146000;
        public const uint GwesRomTextEnd = 0x801EADE0;
        // Live a633b83: after ddi-data dest6-adj, NK
        // 0x8003D254 data-TLBL 0x040110FC (a1=1,
        // v0=0x86FA7800 next MODULE*). CE 32MB slot 2:
        // 0x04000000 + 0x000110FC. Same page as filesys
        // VA 0x00011000 / FILESYS API 0x000111A8.
        // HostHardDisk: slot 0 is filesys. MapFirmwareSlotVa:
        // slot 2 is filesys. TryGetTocO32ByVbase: ROM DLL
        // vbases are unique and < 0x04000000 — not a
        // BuiltIn preferred base. Do not walk all slot-2
        // (wait77 OEMIdle). Firmware PTE only. Do not
        // invent dest.
        public const uint FilesysSlot2Page = 0x04011000;
        public const uint FilesysSlot2Fault = 0x040110FC;
        // Live 5b54d07: filesys-48d pages mapped.
        // Next data-TLBL epc=0x0001E4DC
        // badvaddr=0x08011BE8 a1=0x80000002
        // v0=0x080DF61C stores=24. Same
        // relative FILESYS API page as
        // 0x04011000→0x80105000: slot 4
        // 0x08000000+0x11000. epc is filesys
        // (near 0x0001E534). Slot 0 is
        // gwes-text — other handlers. Do
        // not invent dest. Do not walk all
        // slot-4.
        public const uint FilesysSlot4Page = 0x08011000;
        public const uint FilesysSlot4Fault = 0x08011BE8;
        public const uint FilesysSlotRelPage = 0x00011000;
        public const uint FilesysSlotMask = 0x01FFFFFFu;
        // Live 725f2f4: 0x0405C000→0x86F95000 and
        // 0x0405D000→0x86F96000. Next BindImp-exn
        // cause=2 epc=0x800525D8 badvaddr=0x04021ABC
        // a1=1 v0=0x00021ABC (slot-2 view of gwes
        // null-store PC). Widen extra Lo down to
        // the page after FILESYS API 0x04011000.
        // [0x04012000, 0x04080000) includes
        // 0x04021000 / 0x0405C000 / 0x0407F000.
        // Per-page firmware PTE, own kseg. Do not
        // alias onto 0x80105000. Do not walk all
        // slot-2 (wait77 OEMIdle). Do not steal
        // slot-0 gwes (rel 0x00021000). Do not
        // invent dest. Do not map VA 0.
        public const uint FilesysSlot2ExtraLo = 0x04012000;
        public const uint FilesysSlot2ExtraHi = 0x04080000;
        public const int FilesysSlot2ExtraCap = 32;
        public const uint FilesysSlot27FPage = 0x0407F000;
        public const uint FilesysSlot27FFault = 0x0407FEC0;
        // Live 017b67e: filesys-slot2 mapped. Next miss is
        // data-TLBL epc=0x0001E534 badvaddr=0x48D000F0.
        // Slot 0 is filesys (HostHardDisk). ROM =
        // FilesysRomText+(0x0001E534-0x00011000)=0x80112534.
        // Between CreateFile 0x00019CB8 and RegOpen
        // 0x0001FEB0. Not kernel LoadO32WrapC1 0x8001E534.
        // 0x48D000F0>>25=36 — outside CE 32MB slots 0-31.
        // Equals 0x40000000|0x08D000F0 (PTE-flag bit;
        // WalkFirmwarePte wait77 l2 0x40002A1A). v0=
        // 0x080DF51C is gwes-slot VALLOC 0x080D0000
        // (wait42), not KData 0xFFFFD800.
        // Live 98b8fa6: insn 0x8D4B00F0 lw t3,0xF0(t2)
        // t2=0x48D00000. Same word is dest-word of
        // gwes-page 0x00081000→0x86F8C000 l2=0x001BE31E
        // (firmware PTE consistent; not a backing miss).
        // Tagged gwes-slot VA: clear bit30 → 0x08D00000
        // (GwesSlot|0x00D00000). Demand-map that page
        // via slot-4 firmware PTE only. Do not invent
        // 0x48D dest. Do not walk slot-2. Observe stays.
        public const uint Filesys48dEpc = 0x0001E534;
        public const uint Filesys48dPage = 0x48D00000;
        public const uint Filesys48dFault = 0x48D000F0;
        public const uint Filesys48dClearPage = 0x08D00000;
        public const uint Filesys48dGwesSlot = 4;
        // Live 82240a0: page0 mapped 0x48D00000→0x08D00000→
        // 0x87B63000 (valloc-dest-adj). Next data-TLBL
        // epc=0x00031A10 badvaddr=0x48D01000 v1=0x48D05000.
        // gwes dest-words 0x00081000=0x48D00000,
        // 0x00082000=0x48D01000. Inclusive through
        // 0x48D05000. Bit30 clear 0x48Dxxxxx→0x08Dxxxxx.
        // Do not invent dest. Do not walk slot-2.
        public const uint Filesys48dBit30 = 0x40000000;
        public const uint Filesys48dClearLo = 0x08D00000;
        public const uint Filesys48dClearHi = 0x08D06000;
        public const int Filesys48dPageCap = 6;
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
        // ExtraROM FILE OpenFile after FILE[25]+TOC[46]:
        // mscorlib.dll then tv2clientcorece.dll (and the
        // other ExtraROM FILE names of that class). Same
        // type-8 as FILE[25]. dest/cache sized for THAT
        // file. FILE[25] _tv2File* stays so leftover
        // dest-live is not hopped. Do not invent bytes.
        private static ExtraRomOpenFile[] _romFiles;
        private static int _romFileCount;
        private static ExtraRomOpenFile _romFile;
        private static uint _romFileDecompRa;
        private static uint _romFileSavedSp;
        private static uint _romFilePos;
        private static bool _romFileDestOn;
        private static bool _romFileIoLogged;
        private static bool _romFileAttach;
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
        private static bool _tv2LeftoverCb38Peeked;
        private static uint _tv2LeftoverCb38Word;
        private static bool _tv2LeftoverAfterCb34Logged;
        private static bool _tv2LeftoverPastCb38Logged;
        private static bool _tv2LeftoverCb3cPeeked;
        private static uint _tv2LeftoverCb3cWord;
        private static bool _tv2LeftoverAfterCb38Logged;
        private static bool _tv2LeftoverPastCb3cLogged;
        private static bool _tv2LeftoverCb40Peeked;
        private static uint _tv2LeftoverCb40Word;
        private static bool _tv2LeftoverAfterCb3cLogged;
        private static bool _tv2LeftoverPastCb40Logged;
        private static bool _tv2LeftoverCb44Peeked;
        private static uint _tv2LeftoverCb44Word;
        private static bool _tv2LeftoverAfterCb40Logged;
        private static bool _tv2LeftoverPastCb44Logged;
        private static bool _tv2LeftoverCb48Peeked;
        private static uint _tv2LeftoverCb48Word;
        private static bool _tv2LeftoverAfterCb44Logged;
        private static bool _tv2LeftoverPastCb48Logged;
        private static bool _tv2LeftoverCb4cPeeked;
        private static uint _tv2LeftoverCb4cWord;
        private static bool _tv2LeftoverAfterCb48Logged;
        private static bool _tv2LeftoverPastCb4cLogged;
        private static bool _tv2LeftoverAfterCb4cLogged;
        private static bool _tv2LeftoverPastJrRaLogged;
        private static uint _tv2LeftoverJrRaDest;
        private static bool _tv2LeftoverBeqRaV0Set;
        private static uint _tv2LeftoverBeqRaV0;
        private static bool _tv2LeftoverBeqRaFtPeeked;
        private static uint _tv2LeftoverBeqRaFtWord;
        private static bool _tv2LeftoverBeqRaTkPeeked;
        private static uint _tv2LeftoverBeqRaTkWord;
        private static bool _tv2LeftoverAfterJrRaLogged;
        private static bool _tv2LeftoverPastBeqRaFtLogged;
        private static bool _tv2LeftoverPastBeqRaTkLogged;
        private static bool _tv2LeftoverBPlus2DelayPeeked;
        private static uint _tv2LeftoverBPlus2DelayWord;
        private static bool _tv2LeftoverBPlus2TakenPeeked;
        private static uint _tv2LeftoverBPlus2TakenWord;
        private static bool _tv2LeftoverAfterBPlus2Logged;
        private static bool _tv2LeftoverPastBPlus2DelayLogged;
        private static bool _tv2LeftoverPastBPlus2TakenLogged;
        private static bool _tv2LeftoverBPlus2NextPeeked;
        private static uint _tv2LeftoverBPlus2NextWord;
        private static bool _tv2LeftoverAfterBPlus2TakenLogged;
        private static bool _tv2LeftoverPastBPlus2NextLogged;
        private static bool _tv2LeftoverFpNextPeeked;
        private static uint _tv2LeftoverFpNextWord;
        private static bool _tv2LeftoverAfterFpLogged;
        private static bool _tv2LeftoverPastFpNextLogged;
        private static bool _tv2LeftoverS7NextPeeked;
        private static uint _tv2LeftoverS7NextWord;
        private static bool _tv2LeftoverAfterS7Logged;
        private static bool _tv2LeftoverPastS7NextLogged;
        private static bool _tv2LeftoverS6NextPeeked;
        private static uint _tv2LeftoverS6NextWord;
        private static bool _tv2LeftoverAfterS6Logged;
        private static bool _tv2LeftoverPastS6NextLogged;
        private static bool _tv2LeftoverS5NextPeeked;
        private static uint _tv2LeftoverS5NextWord;
        private static bool _tv2LeftoverAfterS5Logged;
        private static bool _tv2LeftoverPastS5NextLogged;
        private static bool _tv2LeftoverS4NextPeeked;
        private static uint _tv2LeftoverS4NextWord;
        private static bool _tv2LeftoverAfterS4Logged;
        private static bool _tv2LeftoverPastS4NextLogged;
        private static bool _tv2LeftoverEpiloguePeeked;
        private static uint _tv2LeftoverEpilogueWord;
        private static bool _tv2LeftoverPastEpilogueLogged;
        private static bool _tv2LeftoverPastEpilogueDelayLogged;
        private static uint _tv2LeftoverDestLiveNext;
        private static bool _tv2LeftoverUserRaSet;
        private static uint _tv2LeftoverUserRa;
        private static bool _tv2LeftoverEretLogged;
        private static bool _tv2LeftoverDropLogged;
        private static bool _tv2LeftoverDestLiveEretLogged;
        private static bool _tv2LeftoverDispatchLogged;
        private static bool _tv2GwesFetchLogged;
        private static bool _tv2GwesContLogged;
        private static bool _tv2MscoreeSlotLogged;
        private static bool _coredllMapBusy;
        private static bool _tv2ProcSwitchLogged;
        private static bool _tv2CurThreadLogged;
        private static bool _tv2RestoreLogged;
        private static bool _tv2SwitchForced;
        private static bool _tv2SwitchStoreLogged;

        // ExtraROM TOC type-7 modules from the dump ROMHDR walk.
        // Firmware later reuses ExtraROM tail and zeros TOC words.
        // Cache every ExtraROM TOC name at map time so CreateFileFail
        // / OpenFile / LoadLibrary can attach ANY ExtraROM TOC module
        // (same type-7 as ddi_nop/mscoree/ole32). Do not invent a
        // name that is not in ExtraROM TOC. FILE type-8 dest/cache
        // stays on IsExtraRomOpenFile / FILE[25].
        private static ExtraRomTocMod[] _romTocMods;
        private static int _romTocCount;
        private static uint _e32HostPool = ExtraRomE32Host;
        private static bool _e32HostCommitted;
        private static uint _tocDestHostPool = ExtraRomTocDestHost;
        private static uint[] _tocDestSlot0;
        private static uint[] _tocDestDump;
        private static uint[] _tocDestVsize;
        private static uint[] _tocDestKseg;
        private static bool[] _tocDestReady;
        private static int _tocDestN;
        private static uint _tocSrcPool = ExtraRomTocSrc;
        private static uint[] _tocSrcPtr;
        private static uint[] _tocSrcLen;
        private static uint[] _tocSrcKseg;
        private static int _tocSrcN;
        private static ExtraRomTocMod _tocDecompSlot;
        private static uint _loadE32Obj;
        private static string _pendingLoadE32Name;
        private static int _pendingLoadE32Index;
        private static bool _loadE32Watch;
        private static string _loadE32WatchName;
        private static int _loadE32WatchIndex;
        private static uint _loadE32WatchA0;
        private static uint _loadE32WatchA1;
        private static uint _loadE32WatchA2;
        private static uint _loadE32WatchA3;
        private static uint _loadE32WatchErr0;
        private static uint _loadE32WatchErrNow;
        private static uint _loadE32WatchErrPc;
        private static uint _loadE32WatchErrNew;
        private static int _loadE32WatchErrHits;
        private static int _loadE32WatchJalN;
        private static string _loadE32WatchJal;
        private static int _loadE32WatchSteps;
        private static uint _loadE32CopyRa;
        private static uint _loadE32CopyV0;
        private static uint _loadE32CopyA0;
        private static uint _loadE32CopyA1;
        private static uint _loadE32CopyA2;
        private static uint _loadE32CopyWord;
        private static uint _loadE32ChkRa;
        private static uint _loadE32ChkV0;
        private static uint _loadE32ChkA0;
        private static uint _loadE32ChkA1;
        private static uint _loadE32ChkA2;
        private static uint _loadE32ChkWord;
        private static uint _loadE32ChkOff;
        private static string _loadE32ChkSpan;
        private static bool _loadE32CopySeen;
        private static bool _loadE32ChkSeen;
        private static uint _loadE32RomBit;
        private static uint _loadE32CmpPc;
        private static string _loadE32CmpOp;
        private static uint _loadE32CmpLhs;
        private static uint _loadE32CmpRhs;
        private static uint _loadE32CmpFirstPc;
        private static string _loadE32CmpFirstOp;
        private static uint _loadE32CmpFirstLhs;
        private static uint _loadE32CmpFirstRhs;
        private static uint _loadE32CmpAfterPc;
        private static string _loadE32CmpAfterOp;
        private static uint _loadE32CmpAfterLhs;
        private static uint _loadE32CmpAfterRhs;
        private static int _loadE32CmpN;
        private static string _loadE32CmpLog;
        private static uint _loadE32RetPc;
        private static uint _loadE32RetV0;
        private static bool _loadE32RetLogged;
        private static bool _loadE32OkWatch;
        private static string _loadE32OkName;
        private static int _loadE32OkIndex;
        private static uint _loadE32OkObj;
        private static uint _loadE32OkDest;
        private static uint _loadE32OkDest0;
        private static uint _loadE32OkWrapPc;
        private static bool _loadE32OkLoadO32;
        private static bool _loadE32OkCopyO32;
        private static bool _loadE32OkPred;
        private static bool _loadE32OkPredFail;
        private static bool _loadE32OkLoadO32Ret;
        private static bool _loadE32OkWrapFail;
        private static uint _loadE32OkPredRa;
        private static uint _loadE32OkPredV0;
        private static uint _loadE32OkLiveEntry;
        private static uint _loadE32OkLiveE32;
        private static uint _loadE32OkDumpToc0;
        private static uint _loadE32OkFp;
        private static bool _loadE32OkBit200;
        private static bool _loadE32OkBit200Seen;
        private static bool _loadE32OkSkip200;
        private static bool _loadE32OkValloc;
        private static uint _loadE32OkVallocRa;
        private static uint _loadE32OkVallocV0;
        private static uint _loadE32OkLoadVa;
        private static uint _loadE32OkObj6;
        private static bool _loadE32OkWrapAfter;
        private static bool _loadE32OkMapO32;
        private static bool _loadE32OkMapInner;
        private static bool _loadE32OkMap28844;
        private static bool _loadE32OkMapValloc;
        private static uint _loadE32OkMapVallocV0;
        private static uint _loadE32OkMapVallocA0;
        private static uint _loadE32OkMapVallocA2;
        private static uint _loadE32OkMapVallocA3;
        private static bool _loadE32OkWrapValloc;
        private static bool _loadE32OkO32Walk;
        private static bool _loadE32OkS5Hi;
        private static bool _loadE32OkFlagsChk;
        private static bool _loadE32OkC1;
        private static uint _loadE32OkS5;
        private static uint _loadE32OkSp24;
        private static uint _loadE32OkDestAfter;
        private static string _bcmSkipSnap;
        private static string _bcmMapSnap;
        private static string _ddiSkipSnap;
        private static bool _loadE32OkBindImp;
        private static bool _loadE32OkCallDll;
        private static bool _loadE32OkDecomp;
        private static bool _skipDisasmLogged;
        private static bool _wrapAfterDisasmLogged;
        private static bool _romHdrChainLogged;
        private static bool _romHdrListWalkLogged;
        private static bool _obj6ShLogged;
        private static int _romHdrLinkEnterCount;
        private static int _romHdrLinkPublishCount;
        private static int _romHdrLinkSpliceCount;
        private static bool _romHdrLinkJalLogged;
        private const int RomHdrLinkLogMax = 8;
        private static int _loadE32OkSteps;
        private static bool _nkLoadE32Watch;
        private static string _nkLoadE32Name;
        private static uint _nkLoadE32E32;
        private static uint _nkLoadE32O32;
        private static uint _nkLoadE32O32Vsize;
        private static uint _nkLoadE32O32Ptr;
        private static uint _nkChkRa;
        private static uint _nkChkA0;
        private static uint _nkChkA1;
        private static uint _nkChkA2;
        private static uint _nkChkWord;
        private static uint _nkChkV0;
        private static string _nkChkSpan;
        private static bool _nkChkSeen;
        private static uint _nkRomBit;
        private static uint _nkCmpPc;
        private static string _nkCmpOp;
        private static uint _nkCmpLhs;
        private static uint _nkCmpRhs;
        private static uint _nkCmpFirstPc;
        private static string _nkCmpFirstOp;
        private static uint _nkCmpFirstLhs;
        private static uint _nkCmpFirstRhs;
        private static uint _nkRetPc;
        private static int _nkLoadE32Logged;
        private static string _nkLoadE32Ok;
        private static uint _nkLoadE32Obj;
        private static uint _nkLoadE32Toc;
        private static uint _nkLoadE32DumpToc0;
        private static bool _nkLoadO32Watch;
        private static string _nkLoadO32Name;
        private static uint _nkLoadO32Obj;
        private static uint _nkLoadO32Toc;
        private static uint _nkLoadO32DumpToc0;
        private static uint _nkLoadO32Word0;
        private static uint _nkLoadO32Fp;
        private static bool _nkLoadO32Bit200;
        private static bool _nkLoadO32Entered;
        private static bool _nkLoadO32Skip200;
        private static bool _nkLoadO32Thunk;
        private static bool _nkLoadO32Ret;
        private static int _nkLoadO32Steps;
        private static bool _curMSecDisasmLogged;
        private const int LoadE32AfterMax = 8;
        private static readonly uint[] _afterRa = new uint[LoadE32AfterMax];
        private static readonly string[] _afterName = new string[LoadE32AfterMax];
        private static readonly uint[] _afterA0 = new uint[LoadE32AfterMax];
        private static readonly uint[] _afterA1 = new uint[LoadE32AfterMax];
        private static readonly uint[] _afterA2 = new uint[LoadE32AfterMax];
        private static readonly uint[] _afterWord = new uint[LoadE32AfterMax];
        private static readonly string[] _afterNeed = new string[LoadE32AfterMax];
        private static int _afterN;
        private static string _afterRets;
        private static string _afterDisasm;

        private static string _lastRomAttachKey;

        private static void LogRomAttach(string result, string source, string kind, int index,
            string name, int type, uint dest, uint real, uint comp, string why)
        {
            string key = (result ?? "") + "|" + (source ?? "") + "|" + (kind ?? "") + "|" + (name ?? "");
            if (key == _lastRomAttachKey)
                return;
            _lastRomAttachKey = key;
            BootLog.Rom(result, source, kind, index, name, type, dest, real, comp, why);
        }

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
            // FILE type-8 first so FILE[11] mscorlib / FILE[25]
            // tv2clientce / FILE[26] tv2clientcorece stay type-8
            // dest/cache. Do not turn those names into TOC type-7.
            // ExtraROM TOC type-7 attach is any dump ROMHDR TOC
            // name (ddi_nop/mscoree/ole32 plus bcmuart/ndis/sipcfg
            // /iptvhal_*/iptvdriver and the rest). Do not skip
            // those as "not ExtraROM FILE". Names not in ExtraROM
            // TOC or FILE skip. Display stays ddi_nop.dll.
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
                    LogRomAttach("fail", "ExtraROM", "FILE", 25, "tv2clientce.exe", 8, 0, 0, 0,
                        "CreateFileFail; FILESentry miss; do not invent 0x81360000");
                    return false;
                }
                _romFileAttach = false;
                attachType = FileAttachType;
                System.Console.WriteLine("[Hive] FILE-attach ExtraROM tv2clientce.exe entry=0x" +
                    tocEntry.ToString("X8") +
                    " type=8 attr=0x" + attr.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    " (FILESentry; firmware SetFilePointer/ReadFile; not a dump 0x81360000 map)");
                LogRomAttach("ok", "ExtraROM", "FILE", 25, "tv2clientce.exe", 8, load, real, comp,
                    "CreateFileFail type-8 FILESentry; firmware SetFilePointer/ReadFile; not a dump 0x81360000 map");
                _pendingRomFile = null;
                return true;
            }
            // mscoree OpenFile after FILE[25]+TOC[46] is
            // ExtraROM FILE mscorlib.dll then
            // tv2clientcorece.dll. Same type-8 as FILE[25]:
            // object+0=entry, +4=8, dump attr 0x807.
            // dest/cache sized for that file's real/comp.
            // Do not invent FILE[26] bytes or 0x81360000.
            // Do not set ROMMODULE. Do not attach TOC
            // type-7 names. leftover dest-live stays parked.
            if (IsExtraRomOpenFile(baseName))
            {
                string want = ExtraRomOpenFileName(baseName);
                TryRestoreExtraRomOpenFileIfClobbered(bus, want);
                uint real = 0;
                uint comp = 0;
                uint load = 0;
                if (!TrySelectExtraRomOpenFile(want, out tocEntry, out attr,
                    out real, out comp, out load)
                    && !TryFindExtraRomFile(bus, want, out tocEntry, out attr,
                        out real, out comp, out load))
                {
                    LogRomAttach("fail", "ExtraROM", "FILE", -1, want, 8, 0, 0, 0,
                        "OpenFile type-8 FILESentry miss; do not invent bytes or 0x81360000");
                    return false;
                }
                ExtraRomOpenFile fileSlot = FindExtraRomOpenFile(want);
                int fileIndex = fileSlot != null ? fileSlot.Index : -1;
                _romFileAttach = true;
                attachType = FileAttachType;
                System.Console.WriteLine("[Hive] FILE-attach ExtraROM " + want +
                    " entry=0x" + tocEntry.ToString("X8") +
                    " type=8 attr=0x" + attr.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    " (FILESentry; firmware SetFilePointer/ReadFile; not a dump 0x81360000 map)");
                LogRomAttach("ok", "ExtraROM", "FILE", fileIndex, want, 8, load, real, comp,
                    "CreateFileFail/OpenFile type-8 FILESentry; firmware SetFilePointer/ReadFile; not a dump 0x81360000 map");
                _pendingRomFile = null;
                return true;
            }
            if (TryFindTocModule(bus, 0, 80, baseName, out tocEntry, out attr))
            {
                LogRomAttach("ok", "NK", "TOC", -1, baseName, 7, 0, 0, 0,
                    "CreateFileFail NK ROMHDR attach type-7");
                return true;
            }
            int tocIndex;
            uint dest;
            uint e32;
            if (TrySelectExtraRomToc(bus, baseName, out tocEntry, out attr, out tocIndex, out dest, out e32))
            {
                attachType = TocAttachType;
                string why = "CreateFileFail/OpenFile type-7; ExtraROM TOC[" + tocIndex +
                    "]; e32=0x" + e32.ToString("X8") +
                    "; do not invent 0x81360000";
                if (BootLog.IsGuestIoName(baseName))
                    why = "CreateFileFail/OpenFile type-7; ExtraROM TOC[" + tocIndex +
                        "]; firmware probe; do not invent a NIC or UART";
                System.Console.WriteLine("[Hive] TOC-attach ExtraROM " + baseName +
                    " entry=0x" + tocEntry.ToString("X8") +
                    " type=7 attr=0x" + attr.ToString("X8") +
                    " e32=0x" + e32.ToString("X8") +
                    " (TOC[" + tocIndex + "]; not a FILE; do not invent 0x81360000)");
                LogRomAttach("ok", "ExtraROM", "TOC", tocIndex, baseName, 7, dest, 0, 0, why);
                TryMarkExtraRomO32Compressed(bus, tocEntry);
                NoteLoadE32(baseName, tocIndex);
                _pendingRomFile = null;
                return true;
            }
            uint nkReal = 0;
            uint nkComp = 0;
            uint nkLoad = 0;
            if (TryFindNkFile(bus, baseName, out tocEntry, out attr, out nkReal, out nkComp, out nkLoad))
            {
                _romFileAttach = false;
                attachType = FileAttachType;
                LogRomAttach("ok", "NK", "FILE", -1, baseName, 8, nkLoad, nkReal, nkComp,
                    "CreateFileFail NK ROMHDR FILE type-8; do not invent ExtraROM copy");
                _pendingRomFile = null;
                return true;
            }
            LogRomAttach("skip", "ExtraROM", "", -1, baseName, 0, 0, 0, 0,
                BootLog.IsGuestIoName(baseName)
                    ? "CreateFileFail/OpenFile; guest IO name; not ExtraROM FILE type-8 or TOC attach; do not invent a NIC or UART"
                    : "CreateFileFail/OpenFile; not ExtraROM FILE type-8 or TOC attach name; do not invent");
            return false;
        }

        // LoadDriver does not CreateFile. OpenExe 0x8001D6F0 calls
        // this walk at 0x8001DA58 for a bare name. NK modules hit
        // because they sit on *(0x80342B10). ExtraROM 0x8134DA84
        // is mapped but never linked. Host attach is a workaround
        // because the chain is unlinked. Do not invent a
        // ROMChain_t. Write the same object the hit path at
        // 0x80016B9C writes and return 0 so 0x800196E4 can
        // decompress/map.
        public static bool TryAttachExtraRomTocWalk(MipsBus bus, uint path, uint obj)
        {
            if (bus == null || path == 0 || obj == 0)
                return false;
            string baseName = Basename(bus, path);
            if (string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(_pendingRomFile))
                baseName = _pendingRomFile;
            if (string.IsNullOrEmpty(baseName))
                return false;
            if (IsTv2ClientCe(baseName) || IsExtraRomOpenFile(baseName))
                return false;
            uint tocEntry;
            uint attr;
            int tocIndex;
            uint dest;
            uint e32;
            if (!TrySelectExtraRomToc(bus, baseName, out tocEntry, out attr, out tocIndex, out dest, out e32))
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
                TryLogRomHdrListWalk(bus, "TOC-walk miss " + baseName);
                LogRomAttach("fail", "ExtraROM", "TOC", -1, baseName, 7, 0, 0, 0,
                    "TOC-walk miss toc=0x" + toc.ToString("X8") +
                    " nmods=" + nmods + "; " + NameChainMiss());
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
            TryLogRomHdrListWalk(bus, "TOC-walk host-attach " + baseName);
            System.Console.WriteLine("[Hive] TOC-walk ExtraROM " + baseName + " entry=0x" +
                tocEntry.ToString("X8") +
                " (TOC[" + tocIndex + "]; type-7 host attach; chain unlinked; do not invent a ROMChain_t; do not invent a FILE)");
            LogRomAttach("ok", "ExtraROM", "TOC", tocIndex, baseName, 7, dest, 0, 0,
                "TOC-walk type-7 host attach; TOC[" + tocIndex + "]; " + NameChainMiss());
            TryMarkExtraRomO32Compressed(bus, tocEntry);
            NoteLoadE32(baseName, tocIndex);
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
            ExtraRomTocMod cached = FindCachedTocByEntry(tocEntry);
            if (tocEntry != _ddiNopTocEntry && tocEntry != _mscoreeTocEntry
                && tocEntry != _ole32TocEntry && cached == null)
                return;
            if (tocEntry == _mscoreeTocEntry)
                TryRestoreExtraRomMscoreeIfClobbered(bus);
            else if (tocEntry == _ole32TocEntry)
                TryRestoreExtraRomOle32IfClobbered(bus);
            else if (tocEntry == _ddiNopTocEntry)
                TryRestoreExtraRomIfClobbered(bus, tocEntry);
            else if (cached != null)
                TryRestoreExtraRomTocModIfClobbered(bus, cached);
            uint e32 = 0;
            uint o32 = 0;
            try
            {
                uint attr = bus.Read32(tocEntry);
                uint name = bus.Read32(tocEntry + 0x10);
                e32 = bus.Read32(tocEntry + 0x14);
                o32 = bus.Read32(tocEntry + 0x18);
                string tag = ExtraRomTocTag(tocEntry, cached);
                uint cachedE32 = tocEntry == _ole32TocEntry ? _ole32E32
                    : (tocEntry == _mscoreeTocEntry ? _mscoreeE32
                        : (tocEntry == _ddiNopTocEntry ? _ddiNopE32
                            : (cached != null ? cached.E32 : 0)));
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
                string tag = ExtraRomTocTag(tocEntry, cached);
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
                    ExtraRomTocMod marked = cached;
                    if (marked != null && marked.O32Words != null && marked.O32Words.Length >= 3
                        && marked.O32Words[2] == 0)
                        continue;
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
                // ExtraROM type-7 destDump (o32.real) is what
                // firmware VirtualAllocs. Do not rewrite to dest0.
                // ddi_nop OpenFile dest stays the working Display.
                if (!IsExtraRomDdiNopDest(dest) && !IsExtraRomDdiNopData(dataptr))
                    return;
                uint slot = dest & SlotMask;
                if (slot == dest)
                    return;
                bus.Write32(o32Lite + 8, slot);
                System.Console.WriteLine("[Hive] ExtraROM MapO32 dest 0x" +
                    dest.ToString("X8") + " -> 0x" + slot.ToString("X8") +
                    " (ddi_nop Display dest; do not steer ExtraROM type-7 destDump)");
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
            // 0x8001AC9C is flags bnez, not jal 0x80028844.
            // Do not rewrite o32_lite flags. ExtraROM type-7
            // destDump is firmware VirtualAlloc of o32.real.
        }

        // 0x80028844 is a0=dest a1=dataptr a2=vsize.
        // Do not jal 0x8004DBF8 or rewrite a0/a1/a2/a3.
        // 6c001d9 stole sipcfg/shell dest 0x00011000 and
        // looped. Firmware MapO32/OpenFile/VALLOC owns this.
        public static bool TryRedirectExtraRomMapO32Decompress(
            MipsBus bus, uint[] regs, ref uint programCounter)
        {
            return false;
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
        // ExtraROM TOC/e32/o32 live at 0x8134xxxx / 0x80E99Cxx.
        // Firmware reuses that tail as RAM. Host dump e32+o32
        // so LoadE32 type-7 can take the ROM success path
        // (v0=0 at 0x80019990). Dest word 0 after that is
        // CopyO32/CEDecompressROM not running, not LoadE32
        // fail. NK TOC attach works because NK
        // ROMHDR e32_rom stays in XIP. Copy dump TOC+e32+o32
        // next to FILE[25] dest 0x8F140000 (CEDecompressROM
        // tv2clientce already uses that kseg0 window). After
        // FILE[25] 5120/0x2000. Not 0x81360000, not FILE dest,
        // not VallocHostKseg 0x8F200000.
        public const uint ExtraRomE32Host = 0x8F148000;
        public const uint ExtraRomE32HostLim = 0x8F168000;
        // Dump ExtraROM TOC o32 dataptr backing (not a0).
        // a0 is dump o32 dataptr (bcmuart 0x80B62B98).
        // Do not rewrite a0 to ExtraRomTocSrc. a2 is firmware
        // dest (o32.real 0x02F21000 / VALLOC). Do not rewrite
        // a2 to ExtraRomTocDestHost. Dump vbase/vsize only.
        // Not 0x81360000.
        public const uint ExtraRomTocSrc = 0x8E000000;
        public const uint ExtraRomTocSrcLim = 0x8E800000;
        public const uint ExtraRomTocDestHost = 0x8E800000;
        public const uint ExtraRomTocDestHostLim = 0x8F000000;
        public const uint CeAllocGranularity = 0x10000;

        public static bool TryReserveExtraRomValloc(uint[] regs)
        {
            if (regs == null || regs.Length <= 6)
                return false;
            uint dest = regs[4];
            ExtraRomTocMod type7 = FindCachedTocByDest(dest);
            if (type7 == null && (dest & 0xF0000000u) == 0x60000000u
                && !string.IsNullOrEmpty(_loadE32OkName))
                type7 = FindCachedExtraRomToc(_loadE32OkName);
            if (type7 != null && type7.Dest != 0
                && !NamesMatchRom(type7.Name, "ddi_nop.dll"))
                return TryReserveExtraRomType7DestDump(regs, type7);
            // ddi_nop Display dest stays the working OpenFile path.
            return TryReserveExtraRomVallocDdiNopTail(regs, dest);
        }

        // Live eaeb634: MEM_COMMIT of ExtraROM type-7 destDump
        // (nleddrvr 0x02F81000 / bcmuart 0x02F21000 / mscoree
        // 0x034B1000) returned 0. last-error 14. Wrapper v0=0xE
        // is that OOM. Same miss as ddi_nop slot-1 0x03981000:
        // current process has no reservation. destDump is the
        // VA (o32.real). dest0 is only destDump&SlotMask.
        // Live 0x800283FC one-liner a0=0x60002020 is o32 flags
        // sampled at jal 0x8001AD50; dump jal a0=s4=destDump
        // a2=0x1000 a3=0x40. Do not treat flags as the address.
        // Add MEM_RESERVE so firmware COMMIT can succeed.
        // Host-back zeros only. Do not invent dest bytes.
        // Do not host-CEDecompressROM slot-0. ddi_nop stays
        // on the OpenFile path below.
        private static bool TryReserveExtraRomType7DestDump(uint[] regs, ExtraRomTocMod type7)
        {
            if (regs == null || regs.Length <= 6 || type7 == null)
                return false;
            uint destDump = type7.Dest;
            if (destDump == 0 || destDump >= 0x80000000u)
                return false;
            if (NamesMatchRom(type7.Name, "ddi_nop.dll")
                || IsExtraRomDdiNopDest(destDump)
                || IsExtraRomDdiNopDest(type7.Dest & SlotMask))
                return false;

            uint a0 = regs[4];
            uint size = regs[5];
            uint type = regs[6];
            uint dest0 = destDump & SlotMask;

            if (a0 != destDump)
                regs[4] = destDump;

            if (size == 0 || size == 1 || size > 0x01000000u)
            {
                uint vsize = type7.O32Words != null && type7.O32Words.Length > 0
                    ? type7.O32Words[0] : 0;
                if (vsize == 0 || vsize > 0x01000000u)
                    vsize = 0x1000;
                size = (vsize + 0xFFFu) & ~0xFFFu;
                regs[5] = size;
            }

            if (type == 0 || type == 1 || (type & 0xF0000000u) == 0x60000000u)
                type = 0x1000u;
            if ((type & MemReserve) == 0)
                type |= MemReserve;
            regs[6] = type;

            if (MapVallocHostVa(destDump) == destDump)
                TryHostBackValloc(destDump, destDump, size, type, false);

            BootLog.Write(
                "[Hive] TOC[" + type7.Index + "] " + type7.Name
                + " destDump reserve destDump=0x" + destDump.ToString("X8")
                + " dest0=0x" + dest0.ToString("X8")
                + " a0was=0x" + a0.ToString("X8")
                + " size=0x" + size.ToString("X")
                + " type=0x" + type.ToString("X")
                + " slot-" + (destDump >> 25)
                + " MEM_RESERVE+COMMIT. no dest bytes.");
            return true;
        }

        private static bool TryReserveExtraRomVallocDdiNopTail(uint[] regs, uint dest)
        {
            if (!IsExtraRomDdiNopDest(dest))
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
            // Live 330f08b: dest0 back 0x01980000 size 0x1A000
            // then CEDecompressROM dest=0x01981000 dest-word=0.
            // ExtraRomDestKseg0 / TryHostBackValloc remapped
            // dest0 to a kseg alias firmware does not write.
            // dest0 stays useg. MapFirmwareSlotVa walks
            // firmware PTE (0x80040278) so stores land on
            // the VALLOC page. Do not copy destDump onto
            // dest0. Do not invent dest.
            uint dest0Base = dest & SlotMask;
            _ddiNopDestOn = true;
            _ddiNopSlot0 = dest0Base != 0 ? dest0Base : (DdiNopVbase & SlotMask);
            BootLog.Write("[Hive] ExtraROM ddi_nop dest0 useg dest0=0x" +
                dest0Base.ToString("X8") +
                " (firmware PTE walk; not ExtraRomDestKseg0)");
            if (!needReserve && header == 0)
                return true;
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
            ExtraRomTocMod slot = FindCachedTocByDest(dest);
            if (slot != null)
                BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                    " 0x800283FC-ret v0=0x" + v0.ToString("X") +
                    " destDump=0x" + slot.Dest.ToString("X8") +
                    " a0=0x" + dest.ToString("X8") +
                    (v0 == 0
                        ? " slot-" + (dest >> 25) + " destDump COMMIT no reserve last-error 14"
                        : ""));
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

        // Do not jal 0x8004DBF8 from VirtualCopy. 6c001d9
        // treated dest 0x00011000 (sipcfg/shell TOC) as
        // ddi_nop and looped 3012 hits. Firmware owns
        // OpenFile/VALLOC/CopyO32. Leave a0/a1/a2/a3.
        // dest 0x01981000 first nonzero is noted at the
        // firmware 0x8004DBF8 return, once.
        private static uint _ddiNopDecompRa;
        private static uint _ddiNopDecompSrc;
        private static uint _ddiNopDecompCb;
        private static uint _ddiNopDecompDest;
        private static uint _ddiNopDecompVsize;
        private static uint _ddiNopDecompHdr;
        private static bool _ddiNopInnerCap;
        private static int _ddiNopInnerPages;
        private static bool _ddiNopDestWordLogged;
        private static bool _ddiNopObserve;

        public static bool TryRedirectExtraRomVirtualCopyToDecompress(
            MipsBus bus, uint[] regs, ref uint programCounter)
        {
            return false;
        }

        // Firmware already at 0x8004DBF8. Leave registers
        // and PC. sipcfg/shell dest 0x00011000 is not
        // ddi_nop. Remember RA only for VALLOC dest
        // 0x01981000 so the first nonzero dest word logs
        // once.
        public static void TryNoteExtraRomDecompressEntry(MipsBus bus, uint[] regs)
        {
            if (regs == null || regs.Length <= 7)
                return;
            uint dest = regs[6];
            if (dest != 0x01981000u || _ddiNopDestWordLogged)
                return;
            uint src = regs[4];
            uint cb = regs[5];
            uint vsize = regs[7];
            _ddiNopObserve = true;
            _ddiNopDecompRa = regs.Length > 31 ? regs[31] : 0;
            _ddiNopDecompSrc = src;
            _ddiNopDecompCb = cb;
            _ddiNopDecompDest = dest;
            _ddiNopDecompVsize = vsize;
            _ddiNopDecompHdr = 0;
            try
            {
                if (bus != null && src != 0)
                    _ddiNopDecompHdr = bus.Read32(src);
            }
            catch
            {
            }
            // Live c710c07: dest-word 0 at dest0/dest6;
            // dest10 word 0x806F0000 is a kseg pointer, not MZ.
            // Count host stores from this jal until ret.
            BeginDdiNopDecompStoreWatch(bus);
            TryHuntDdiNopModuleFromRegs(bus, regs);
        }

        public static bool TryNoteExtraRomInnerDest(MipsBus bus, uint[] regs)
        {
            if (_ddiNopObserve
                || (_ddiNopDecompRa == 0 && _tv2FileDecompRa == 0 && _romFileDecompRa == 0)
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
            if (_ddiNopObserve
                || (_ddiNopDecompRa == 0 && _tv2FileDecompRa == 0 && _romFileDecompRa == 0)
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
            uint src = _ddiNopDecompSrc;
            uint cb = _ddiNopDecompCb;
            uint hdr = _ddiNopDecompHdr;
            LogDdiNopDecompStores();
            _ddiNopDecompWatch = false;
            _ddiNopDecompRa = 0;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            uint a3 = regs != null && regs.Length > 7 ? regs[7] : 0;
            uint word = 0;
            uint entry = 0;
            bool mapped = false;
            bool entryMapped = false;
            try
            {
                if (bus != null && dest != 0)
                {
                    // dest0 useg: do not remap to pfn6 before
                    // dest6/dest10/dest0/destDump compare.
                    if (dest == 0x01981000u)
                        word = PeekDestWordRaw(bus, dest, out _);
                    else
                        word = bus.Read32(dest);
                    mapped = true;
                }
            }
            catch
            {
            }
            if (dest == 0x01981000u)
            {
                TryMeasureDdiNopDestAfterDecomp(bus, hdr, v0);
                TryServeDdiNopAtDecompRet(bus, regs);
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
            if (mapped && word != 0 && hdr != 0 && word == hdr)
                note += " (dest is src header; not expanded)";
            string decompName = !string.IsNullOrEmpty(_pendingLoadE32Name)
                ? _pendingLoadE32Name : "";
            string decompWhy = v0 == 0xFFFFFFFFu
                ? "firmware CEDecompressROM miss"
                : (vsize != 0 && v0 == vsize)
                    ? "firmware expanded vsize; dest word=0x" + word.ToString("X8") +
                        "; a0=0x" + src.ToString("X8") +
                        " a1=0x" + cb.ToString("X8") +
                        " a2=0x" + dest.ToString("X8")
                    : (v0 == 0 ? "firmware returned 0" : "firmware CEDecompressROM");
            BootLog.DecompressRom(decompName, dest, v0, decompWhy);
            bool header = mapped && word != 0 && hdr != 0 && word == hdr;
            bool expanded = mapped && word != 0 && !header && v0 != 0xFFFFFFFFu;
            if (expanded)
                MarkExtraRomTocDecompressed(dest);
            // 0x8004DBF8 is not ddi_nop on every hit. sipcfg/shell
            // dest 0x00011000 stays firmware. One line when
            // VALLOC dest 0x01981000 first becomes nonzero.
            if ((dest == 0x01981000u || dest == _ddiNopDest0Pte)
                && mapped && word != 0 && !header
                && !_ddiNopDestWordLogged)
            {
                _ddiNopDestWordLogged = true;
                string first = "[Hive] ExtraROM ddi_nop dest 0x01981000 first nonzero word=0x" +
                    word.ToString("X8") +
                    (_ddiNopDest0Pte != 0
                        ? " pteDest=0x" + _ddiNopDest0Pte.ToString("X8") : "") +
                    " a0=0x" + src.ToString("X8") +
                    " a1=0x" + cb.ToString("X8") +
                    " a2=0x" + dest.ToString("X8") +
                    " a3=0x" + vsize.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " live-a0=0x" + a0.ToString("X8") +
                    " live-a1=0x" + a1.ToString("X8") +
                    " live-a2=0x" + a2.ToString("X8") +
                    " live-a3=0x" + a3.ToString("X8") +
                    (entryMapped ? " entry=0x" + entry.ToString("X8") : "") +
                    imp +
                    note +
                    " (firmware OpenFile/VALLOC/CopyO32 dest; not sipcfg 0x00011000; not host a2 rewrite)";
                System.Console.WriteLine(first);
                BootLog.Write(first);
            }
            _ddiNopObserve = false;
            _tocDecompSlot = null;
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
                BootLog.Write("[Hive] ExtraROM BindImp vbase=0x" +
                    vbase.ToString("X8") +
                    " hdr=0x" + hdr.ToString("X8") +
                    " e32=0x" + e32.ToString("X8") +
                    (dll.Length > 0 ? " \"" + dll + "\"" : " (name unread)"));
                NoteDdiNopWalkSeeds(regs);
                if (_ddiNopLandedBySig)
                    TrySetDdiNopRamStartip(bus, 0, regs);
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
                _ddiNopBindLibName = dll;
                LogRomAttach("ok", "ExtraROM", "", -1, dll.Length > 0 ? dll : "(empty)", 0, 0, 0, 0,
                    "BindImp LoadLibrary; do not invent the DLL");
                return false;
            }
            if (pc == BindImpLoadLibRet && _ddiNopBindLib && !_ddiNopBindLibRet)
            {
                _ddiNopBindLibRet = true;
                uint v0 = regs[2];
                System.Console.WriteLine("[Hive] ExtraROM BindImp LoadLibrary ret v0=0x" +
                    v0.ToString("X8") +
                    (v0 == 0 ? " (import miss; last-error 126)" : " (import loaded)"));
                LogRomAttach(v0 == 0 ? "miss" : "ok", "ExtraROM", "", -1, "", 0, 0, 0, 0,
                    v0 == 0
                        ? "BindImp LoadLibrary ret v0=0 import miss; last-error 126; do not invent the DLL"
                        : "BindImp LoadLibrary ret v0=0x" + v0.ToString("X8"));
                // Live 9183b83: serve dest6 + entry-word
                // 0x27BDFFD8; FindInFlight returned 0
                // (heap TOC-attach openexe, not obj-96).
                // Walk the MODULE list from live v0 / $fp.
                _ddiNopBindLibV0 = v0;
                if (v0 != 0 && NamesMatchRom(_ddiNopBindLibName, "coredll.dll"))
                {
                    _coredllModule = v0;
                    TryKeepCoredllImageBasePtr(bus, v0);
                }
                NoteDdiNopWalkSeeds(regs);
                if (_ddiNopLandedBySig)
                    TrySetDdiNopRamStartip(bus, 0, regs);
                if (_ddiNopModule == 0)
                    LogDdiNopBindWalkOnce(bus);
                else if (_ddiNopLandedBySig)
                {
                    // Live c231655: NK/filesys CallDLL during
                    // early boot set saw=true. Reset so this
                    // load's poll is a fresh window.
                    _ddiNopAwaitCallDll = true;
                    _ddiNopSawCallDllPc = false;
                    _ddiNopCallDllMissPoll = 0;
                    // Live b4b6454: BindImp vbase is VALLOC
                    // 0x01980000; IAT FT is .data RVA 0x19000.
                    // Only .text was CEDecompress'd. Observe
                    // the IAT page and serve the TOC .data
                    // o32 dest if it was never mapped. Do
                    // not force CallDLL here.
                    TryServeDdiNopDataO32(bus);
                    TryLogDdiNopIatPage(bus);
                    _ddiNopIatWatch = true;
                }
                return false;
            }
            TryNoteDdiNopOrdGetProc(bus, regs, pc);
            return false;
        }

        // Live 404d06b: stall at 0x8001F7D0 lw MODULE+0x50.
        // Rate-limit first + every 256th, max 5. Peek only.
        // Do not invent BasePtr / export dir / GetProc VA.
        public static void TryNoteDdiNopOrdGetProc(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_ddiNopAwaitCallDll || regs == null || regs.Length <= 5)
                return;
            if (pc == BindImpOrdBaseLw && regs.Length > 4)
                TryKeepCoredllImageBasePtr(bus, regs[4]);
            TryFixBindImpIatSlot(bus, regs, pc);
            TryNoteBindImpAfterGoodV0(bus, pc);
            TryNoteBindImpIatWindow(bus, regs, pc);
            TryNoteBindImpExnSave(bus, regs, pc);
            TryNoteDdiNopProcessInfo(bus, regs);
            TryNoteDdiNopDllMain(bus, regs, pc);
            TryNoteDdiNopAfterDllMain(bus, regs, pc);
            if (pc == BindImpOrdJalRet)
            {
                uint v0 = regs[2];
                uint a1 = regs[5];
                if (v0 != 0 && _ddiNopOrdGoodV0 == 0)
                {
                    _ddiNopOrdGoodV0 = v0;
                    _ddiNopOrdAfterN = 0;
                    TryArmUserKPageAlias(bus);
                    TryFixBindImpIatSlot(bus, regs, pc);
                }
                if (a1 == _ddiNopOrdRetLastA1 || _ddiNopOrdRetLog >= BindImpObserveMax)
                    return;
                _ddiNopOrdRetLastA1 = a1;
                _ddiNopOrdRetLog++;
                uint iat = 0;
                uint dest6 = 0;
                PeekDdiNopIatWord(bus, out iat, out dest6);
                uint fp = regs.Length > 30 ? regs[30] : 0;
                uint fp1c = 0;
                TryPeekWord(bus, fp + BindImpFpIatOff, out fp1c);
                BootLog.Write("[Hive] ExtraROM BindImp-ord ret v0=0x" +
                    v0.ToString("X8") +
                    " a1=0x" + a1.ToString("X8") +
                    " iat=0x" + iat.ToString("X8") +
                    " dest6=0x" + dest6.ToString("X8") +
                    " fp1c=0x" + fp1c.ToString("X8") +
                    " v1=0x" + regs[3].ToString("X8") +
                    (v0 == 0 ? " (unresolved)" : ""));
                return;
            }
            if (pc != BindImpOrdBaseLw)
                return;
            uint a0 = regs[4];
            uint a1o = regs[5];
            if (a1o == _ddiNopOrdLastA1 || _ddiNopOrdLog >= BindImpObserveMax)
                return;
            _ddiNopOrdLastA1 = a1o;
            _ddiNopOrdLog++;
            uint ra = regs.Length > 31 ? regs[31] : 0;
            uint p50 = 0;
            uint exp = 0;
            uint end = 0;
            bool p50ok = a0 != 0 && TryPeekWord(bus, a0 + ProcModule, out p50);
            bool expok = a0 != 0 && TryPeekWord(bus, a0 + ModuleExpRva, out exp);
            TryPeekWord(bus, a0 + ModuleExpEnd, out end);
            uint fp0 = regs.Length > 30 ? regs[30] : 0;
            uint fp1c0 = 0;
            TryPeekWord(bus, fp0 + BindImpFpIatOff, out fp1c0);
            uint kdata0 = 0;
            bool kOk0 = TryPeekWord(bus, UserKPage, out kdata0);
            BootLog.Write("[Hive] ExtraROM BindImp-ord a0=0x" +
                a0.ToString("X8") +
                " a1=0x" + a1o.ToString("X8") +
                " p50=0x" + p50.ToString("X8") +
                (p50ok ? "" : " unmapped") +
                " exp=0x" + exp.ToString("X") +
                (expok ? "" : " unread") +
                " +90=0x" + end.ToString("X") +
                " fp1c=0x" + fp1c0.ToString("X8") +
                (kOk0
                    ? " FFFF5800=0x" + kdata0.ToString("X8")
                    : " FFFF5800-unmapped") +
                " ra=0x" + ra.ToString("X8") +
                (_ddiNopIatStoreLogged ? " after-slot0" : ""));
            if (_ddiNopOrdExpLogged || !p50ok || !expok || p50 == 0 || exp == 0)
                return;
            uint expVa = p50 + exp;
            uint w0 = 0;
            uint w1 = 0;
            uint w2 = 0;
            uint w3 = 0;
            if (!TryPeekWord(bus, expVa, out w0)
                || !TryPeekWord(bus, expVa + 4, out w1)
                || !TryPeekWord(bus, expVa + 8, out w2)
                || !TryPeekWord(bus, expVa + 12, out w3))
                return;
            _ddiNopOrdExpLogged = true;
            BootLog.Write("[Hive] ExtraROM BindImp-ord expVA=0x" +
                expVa.ToString("X8") +
                " w0=0x" + w0.ToString("X8") +
                " w1=0x" + w1.ToString("X8") +
                " w2=0x" + w2.ToString("X8") +
                " w3=0x" + w3.ToString("X8"));
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
            {
                ExtraRomTocMod slot = FindCachedTocByDest(dest);
                if (slot != null)
                {
                    HostMapFirmwareTocDest(bus, slot, dest, vsize);
                    slot.DecompDest = dest;
                }
                return;
            }
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
            if (IsExtraRomDdiNopDest(dest) || IsExtraRomMscoreeDest(dest)
                || IsExtraRomOle32Dest(dest))
                return true;
            return false;
        }

        private static ExtraRomTocMod FindCachedTocByDest(uint dest)
        {
            if (_romTocMods == null || dest == 0)
                return null;
            if (dest >= ExtraRomTocDestHost && dest < ExtraRomTocDestHostLim)
                return null;
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod slot = _romTocMods[i];
                if (slot == null || slot.Dest == 0)
                    continue;
                uint vsize = slot.O32Words != null && slot.O32Words.Length > 0
                    ? slot.O32Words[0] : 0;
                if (vsize == 0)
                    vsize = 0x1000;
                uint dump = slot.Dest;
                uint slot0 = dump & SlotMask;
                if (dest >= dump && dest < dump + vsize)
                    return slot;
                if (dest >= slot0 && dest < slot0 + vsize)
                    return slot;
            }
            return null;
        }

        private static ExtraRomTocMod FindCachedTocByDataptr(uint dataptr)
        {
            if (_romTocMods == null || dataptr == 0)
                return null;
            if (IsExtraRomDdiNopData(dataptr))
                return FindCachedExtraRomToc("ddi_nop.dll");
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod slot = _romTocMods[i];
                if (slot == null)
                    continue;
                if (slot.DataPtr != null)
                {
                    for (int s = 0; s < slot.DataPtr.Length; s++)
                    {
                        if (slot.DataPtr[s] != 0 && slot.DataPtr[s] == dataptr)
                            return slot;
                    }
                }
                if (slot.O32Words == null)
                    continue;
                int nsec = slot.O32Words.Length / 6;
                for (int s = 0; s < nsec; s++)
                {
                    if (slot.O32Words[s * 6 + 3] == dataptr)
                        return slot;
                }
            }
            return null;
        }

        // MapO32 0x8001AC30 a0=obj a1=o32_lite; dest at +8,
        // dataptr at +0x18. 0x80028844 a0=dest a1=dataptr
        // a2=vsize. 0x8004DBF8 a0=src a2=dest. ddi_nop
        // VALLOC dest 0x01981000 and dataptr 0x80764CE0
        // are dump-real (c1c0bc4). Do not invent dest.
        private static ExtraRomTocMod FindExtraRomMapSlot(MipsBus bus, uint[] regs, uint pc)
        {
            if (regs == null || regs.Length <= 4)
                return null;
            ExtraRomTocMod slot = null;
            uint a0 = regs[4];
            uint a1 = regs.Length > 5 ? regs[5] : 0;
            uint a2 = regs.Length > 6 ? regs[6] : 0;
            if (pc == MapO32Rom || pc == MapO32FlagsBnez)
            {
                try
                {
                    if (bus != null && a0 != 0 && bus.Read8(a0 + 4) == TocAttachType)
                        slot = FindCachedTocByEntry(bus.Read32(a0));
                }
                catch
                {
                }
                uint dest = 0;
                uint dataptr = 0;
                if (bus != null && a1 != 0)
                {
                    dest = PeekDestWord(bus, a1 + 8);
                    dataptr = PeekDestWord(bus, a1 + 0x18);
                }
                if (slot == null && dest == 0x01981000u)
                    slot = FindCachedExtraRomToc("ddi_nop.dll");
                if (slot == null)
                    slot = FindCachedTocByDest(dest);
                if (slot == null)
                    slot = FindCachedTocByDataptr(dataptr);
            }
            else if (pc == MapO32InnerJal || pc == MapO32Decompress || pc == MapO32VallocJal)
            {
                if (a0 == 0x01981000u || a1 == 0x01981000u)
                    slot = FindCachedExtraRomToc("ddi_nop.dll");
                if (slot == null)
                    slot = FindCachedTocByDest(a0);
                if (slot == null)
                    slot = FindCachedTocByDataptr(a1);
            }
            else if (pc == BinaryDecompressRom)
            {
                if (a2 == 0x01981000u)
                    slot = FindCachedExtraRomToc("ddi_nop.dll");
                if (slot == null)
                    slot = FindCachedTocByDest(a2);
                if (slot == null)
                    slot = FindCachedTocByDataptr(a0);
            }
            else
            {
                try
                {
                    if (bus != null && a0 != 0 && bus.Read8(a0 + 4) == TocAttachType)
                        slot = FindCachedTocByEntry(bus.Read32(a0));
                }
                catch
                {
                }
            }
            return slot;
        }

        private static bool IsCompareExtraRom(ExtraRomTocMod slot)
        {
            return slot != null && (NamesMatchRom(slot.Name, "bcmuart.dll")
                || NamesMatchRom(slot.Name, "ddi_nop.dll"));
        }

        private static bool WatchMatchesExtraRom(MipsBus bus, uint[] regs, uint pc)
        {
            if (string.IsNullOrEmpty(_loadE32OkName))
                return false;
            uint obj = regs != null && regs.Length > 4 ? regs[4] : 0;
            if (_loadE32OkObj != 0 && obj == _loadE32OkObj)
                return true;
            ExtraRomTocMod hit = FindExtraRomMapSlot(bus, regs, pc);
            return hit != null && NamesMatchRom(hit.Name, _loadE32OkName);
        }

        private static bool IsExtraRomCompressedData(uint dataptr)
        {
            if (IsExtraRomDdiNopData(dataptr) || IsExtraRomMscoreeData(dataptr)
                || IsExtraRomOle32Data(dataptr))
                return true;
            if (_romTocMods == null)
                return false;
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod slot = _romTocMods[i];
                if (slot == null || slot.DataPtr == null)
                    continue;
                for (int s = 0; s < slot.DataPtr.Length; s++)
                {
                    if (slot.DataPtr[s] != 0 && slot.DataPtr[s] == dataptr)
                        return true;
                }
            }
            return false;
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
            if (bus == null || obj == 0)
                return false;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return false;
                uint toc = bus.Read32(obj);
                if (_ddiNopTocEntry != 0 && toc == _ddiNopTocEntry)
                    return true;
                ExtraRomTocMod slot = FindCachedTocByEntry(toc);
                return slot != null && NamesMatchRom(slot.Name, "ddi_nop.dll");
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
            if (bus == null || obj == 0)
                return false;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return false;
                uint toc = bus.Read32(obj);
                if (_mscoreeTocEntry != 0 && toc == _mscoreeTocEntry)
                    return true;
                ExtraRomTocMod slot = FindCachedTocByEntry(toc);
                return slot != null && IsMscoreeDll(slot.Name);
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
            if (bus == null || obj == 0)
                return false;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return false;
                uint toc = bus.Read32(obj);
                if (_ole32TocEntry != 0 && toc == _ole32TocEntry)
                    return true;
                ExtraRomTocMod slot = FindCachedTocByEntry(toc);
                return slot != null && IsOle32Dll(slot.Name);
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

        public static bool TryDescribeExtraRomTocObject(MipsBus bus, uint obj,
            out string name, out int index, out uint tocEntry, out uint e32)
        {
            name = "";
            index = -1;
            tocEntry = 0;
            e32 = 0;
            if (bus == null || obj == 0)
                return false;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return false;
                tocEntry = bus.Read32(obj);
            }
            catch
            {
                return false;
            }
            ExtraRomTocMod slot = FindCachedTocByEntry(tocEntry);
            if (slot != null)
            {
                name = slot.Name;
                index = slot.Index;
                e32 = slot.LiveE32 != 0 ? slot.LiveE32 : slot.E32;
                return true;
            }
            if (tocEntry == _ddiNopTocEntry && tocEntry != 0)
            {
                name = "ddi_nop.dll";
                index = 33;
                return true;
            }
            if (tocEntry == _mscoreeTocEntry && tocEntry != 0)
            {
                name = "mscoree.dll";
                index = 46;
                e32 = _mscoreeE32;
                return true;
            }
            if (tocEntry == _ole32TocEntry && tocEntry != 0)
            {
                name = "ole32.dll";
                index = 34;
                e32 = _ole32E32;
                return true;
            }
            return false;
        }

        public static void NoteLoadE32(string name, int index)
        {
            _pendingLoadE32Name = name;
            _pendingLoadE32Index = index;
        }

        public static bool TryPeekLoadE32(out string name, out int index)
        {
            name = _pendingLoadE32Name ?? "";
            index = _pendingLoadE32Index;
            return name.Length != 0;
        }

        public static bool TryGetCachedExtraRomToc(string name, out int index, out uint tocEntry, out uint dest)
        {
            ExtraRomTocMod slot = FindCachedExtraRomToc(name);
            if (slot == null)
            {
                index = -1;
                tocEntry = 0;
                dest = 0;
                return false;
            }
            index = slot.Index;
            tocEntry = slot.Entry;
            dest = slot.Dest;
            return true;
        }

        public static void LogExtraRomTocAttachCache()
        {
            BootLog.Write("[NkBinLoader] ExtraROM TOC type-7 attach cache count=" + _romTocCount +
                " FILE type-8 cache count=" + _romFileCount);
            string[] probe =
            {
                "bcmuart.dll", "NDIS.Dll", "ndisuio.dll", "sipcfg.exe",
                "timesvc.dll", "waveapi.dll", "AFD.Dll", "cfgrdr.dll",
                "credsvc.dll", "ehci.dll", "nleddrvr.dll", "ohci2.dll",
                "PPP.Dll", "uspce.dll", "serial.dll", "iptvdriver.dll",
                "ddi_nop.dll", "mscoree.dll", "ole32.dll", "RunOnce.exe",
                "mscorlib.dll", "tv2clientce.exe", "com16550.dll",
                "keybddr.dll", "ddcore.dll", "EVENTLOG.DLL", "LMemDebug.DLL"
            };
            for (int i = 0; i < probe.Length; i++)
            {
                string n = probe[i];
                int index;
                uint entry;
                uint dest;
                if (TryGetCachedExtraRomToc(n, out index, out entry, out dest))
                {
                    BootLog.Rom("ok", "ExtraROM", "TOC", index, n, 7, dest, 0, 0, "cached");
                    continue;
                }
                ExtraRomOpenFile file = FindExtraRomOpenFile(n);
                if (file != null || IsTv2ClientCe(n))
                {
                    int fi = file != null ? file.Index : 25;
                    BootLog.Rom("ok", "ExtraROM", "FILE", fi, n, 8, 0, 0, 0,
                        "FILE type-8 dest/cache; not ExtraROM TOC type-7");
                    continue;
                }
                BootLog.Rom("miss", "ExtraROM", "", -1, n, 0, 0, 0, 0,
                    "not in ExtraROM TOC/FILE; honest miss; do not invent");
            }
            LogCachedExtraRomFragment("iptvhal");
        }

        // ExtraROM has iptvhal_* TOC names, not a bare iptvhal.dll.
        // Log the dump names so CreateFile/LoadLibrary can attach
        // them type-7. Do not invent iptvhal.dll or a Display REG_SZ.
        private static void LogCachedExtraRomFragment(string fragment)
        {
            if (string.IsNullOrEmpty(fragment))
                return;
            int hits = 0;
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod m = _romTocMods[i];
                if (m == null || string.IsNullOrEmpty(m.Name)
                    || m.Name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                hits++;
                BootLog.Rom("ok", "ExtraROM", "TOC", m.Index, m.Name, 7, m.Dest, 0, 0,
                    "cached iptvhal_* for CreateFileFail/OpenFile/LoadLibrary type-7; Display stays ddi_nop.dll");
            }
            for (int i = 0; i < _romFileCount; i++)
            {
                ExtraRomOpenFile f = _romFiles[i];
                if (f == null || string.IsNullOrEmpty(f.Label)
                    || f.Label.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                hits++;
                BootLog.Rom("ok", "ExtraROM", "FILE", f.Index, f.Label, 8, f.Load, 0, 0,
                    "FILE type-8 dest/cache; not ExtraROM TOC type-7");
            }
            if (hits == 0)
                BootLog.Rom("miss", "ExtraROM", "", -1, fragment, 0, 0, 0, 0,
                    "no ExtraROM TOC/FILE *" + fragment + "* ; honest miss; do not invent iptvhal.dll or a Display REG_SZ");
        }

        public static void NoteExtraRom(uint imageStart)
        {
            _extraRomStart = imageStart;
            _extraRomHdr = 0;
            _romHdrChainLogged = false;
            _romHdrListWalkLogged = false;
            _obj6ShLogged = false;
            _romHdrLinkEnterCount = 0;
            _romHdrLinkPublishCount = 0;
            _romHdrLinkSpliceCount = 0;
            _romHdrLinkJalLogged = false;
            _pendingRomFile = null;
            _lastRomAttachKey = null;
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
            _ddiNopDest0PteLogged = false;
            _ddiNopDest0Pte = 0;
            _ddiNopDestPeekRaw = false;
            _ddiNopDestPteMeasured = false;
            _ddiNopLandedDest = 0;
            _ddiNopLandedWord = 0;
            _ddiNopLandedBySig = false;
            _ddiNopModule = 0;
            ResetDdiNopModuleHunt();
            _mscoreeDestOn = false;
            _mscoreeSlot0 = 0;
            _mscoreeVbase = 0;
            _ole32DestOn = false;
            _ole32Slot0 = 0;
            _ole32Vbase = 0;
            _ddiNopDecompRa = 0;
            _ddiNopDecompSrc = 0;
            _ddiNopDecompCb = 0;
            _ddiNopDecompDest = 0;
            _ddiNopDecompVsize = 0;
            _ddiNopDecompHdr = 0;
            ResetDdiNopDecompStores();
            _ddiNopDestWordLogged = false;
            _ddiNopObserve = false;
            _ddiNopInnerCap = false;
            _ddiNopInnerPages = 0;
            _ddiNopBindHdr = false;
            _ddiNopBindName = false;
            _ddiNopBindLib = false;
            _ddiNopBindLibRet = false;
            ResetDdiNopModuleHunt();
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
            _romFiles = null;
            _romFileCount = 0;
            _romFile = null;
            _romFileDecompRa = 0;
            _romFileSavedSp = 0;
            _romFilePos = 0;
            _romFileDestOn = false;
            _romFileIoLogged = false;
            _romFileAttach = false;
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
            _tv2LeftoverCb38Peeked = false;
            _tv2LeftoverCb38Word = 0;
            _tv2LeftoverAfterCb34Logged = false;
            _tv2LeftoverPastCb38Logged = false;
            _tv2LeftoverCb3cPeeked = false;
            _tv2LeftoverCb3cWord = 0;
            _tv2LeftoverAfterCb38Logged = false;
            _tv2LeftoverPastCb3cLogged = false;
            _tv2LeftoverCb40Peeked = false;
            _tv2LeftoverCb40Word = 0;
            _tv2LeftoverAfterCb3cLogged = false;
            _tv2LeftoverPastCb40Logged = false;
            _tv2LeftoverCb44Peeked = false;
            _tv2LeftoverCb44Word = 0;
            _tv2LeftoverAfterCb40Logged = false;
            _tv2LeftoverPastCb44Logged = false;
            _tv2LeftoverCb48Peeked = false;
            _tv2LeftoverCb48Word = 0;
            _tv2LeftoverAfterCb44Logged = false;
            _tv2LeftoverPastCb48Logged = false;
            _tv2LeftoverCb4cPeeked = false;
            _tv2LeftoverCb4cWord = 0;
            _tv2LeftoverAfterCb48Logged = false;
            _tv2LeftoverPastCb4cLogged = false;
            _tv2LeftoverAfterCb4cLogged = false;
            _tv2LeftoverPastJrRaLogged = false;
            _tv2LeftoverJrRaDest = 0;
            _tv2LeftoverBeqRaV0Set = false;
            _tv2LeftoverBeqRaV0 = 0;
            _tv2LeftoverBeqRaFtPeeked = false;
            _tv2LeftoverBeqRaFtWord = 0;
            _tv2LeftoverBeqRaTkPeeked = false;
            _tv2LeftoverBeqRaTkWord = 0;
            _tv2LeftoverAfterJrRaLogged = false;
            _tv2LeftoverPastBeqRaFtLogged = false;
            _tv2LeftoverPastBeqRaTkLogged = false;
            _tv2LeftoverBPlus2DelayPeeked = false;
            _tv2LeftoverBPlus2DelayWord = 0;
            _tv2LeftoverBPlus2TakenPeeked = false;
            _tv2LeftoverBPlus2TakenWord = 0;
            _tv2LeftoverAfterBPlus2Logged = false;
            _tv2LeftoverPastBPlus2DelayLogged = false;
            _tv2LeftoverPastBPlus2TakenLogged = false;
            _tv2LeftoverBPlus2NextPeeked = false;
            _tv2LeftoverBPlus2NextWord = 0;
            _tv2LeftoverAfterBPlus2TakenLogged = false;
            _tv2LeftoverPastBPlus2NextLogged = false;
            _tv2LeftoverFpNextPeeked = false;
            _tv2LeftoverFpNextWord = 0;
            _tv2LeftoverAfterFpLogged = false;
            _tv2LeftoverPastFpNextLogged = false;
            _tv2LeftoverS7NextPeeked = false;
            _tv2LeftoverS7NextWord = 0;
            _tv2LeftoverAfterS7Logged = false;
            _tv2LeftoverPastS7NextLogged = false;
            _tv2LeftoverS6NextPeeked = false;
            _tv2LeftoverS6NextWord = 0;
            _tv2LeftoverAfterS6Logged = false;
            _tv2LeftoverPastS6NextLogged = false;
            _tv2LeftoverS5NextPeeked = false;
            _tv2LeftoverS5NextWord = 0;
            _tv2LeftoverAfterS5Logged = false;
            _tv2LeftoverPastS5NextLogged = false;
            _tv2LeftoverS4NextPeeked = false;
            _tv2LeftoverS4NextWord = 0;
            _tv2LeftoverAfterS4Logged = false;
            _tv2LeftoverPastS4NextLogged = false;
            _tv2LeftoverEpiloguePeeked = false;
            _tv2LeftoverEpilogueWord = 0;
            _tv2LeftoverPastEpilogueLogged = false;
            _tv2LeftoverPastEpilogueDelayLogged = false;
            _tv2LeftoverDestLiveNext = 0;
            _tv2LeftoverUserRaSet = false;
            _tv2LeftoverUserRa = 0;
            _tv2LeftoverEretLogged = false;
            _tv2LeftoverDropLogged = false;
            _tv2LeftoverDestLiveEretLogged = false;
            _tv2LeftoverDispatchLogged = false;
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
            _romTocMods = null;
            _romTocCount = 0;
            _pendingLoadE32Name = null;
            _pendingLoadE32Index = -1;
            _e32HostPool = ExtraRomE32Host;
            _e32HostCommitted = false;
            _tocDestHostPool = ExtraRomTocDestHost;
            _tocDestSlot0 = null;
            _tocDestDump = null;
            _tocDestVsize = null;
            _tocDestKseg = null;
            _tocDestReady = null;
            _tocDestN = 0;
            _tocSrcPool = ExtraRomTocSrc;
            _tocSrcPtr = null;
            _tocSrcLen = null;
            _tocSrcKseg = null;
            _tocSrcN = 0;
            _tocDecompSlot = null;
            _loadE32Obj = 0;
            ClearLoadE32Watch();
            ClearNkLoadE32Watch();
            ClearNkLoadO32Watch();
            _nkLoadE32Logged = 0;
            _nkLoadE32Ok = null;
            _curMSecDisasmLogged = false;
            ClearAfterLoadE32();
            _afterDisasm = null;
            ClearLoadE32OkWatch();
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
        public static void CacheExtraRomOpenFile(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint filesEntry, string label)
        {
            CacheExtraRomOpenFile(memory, filesEntry, label, -1);
        }

        public static void CacheExtraRomOpenFile(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint filesEntry, string label, int index)
        {
            if (memory == null || filesEntry == 0 || string.IsNullOrEmpty(label))
                return;
            if (IsTv2ClientCe(label))
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
                    nameWords = new uint[16];
                    for (int i = 0; i < nameWords.Length; i++)
                        nameWords[i] = memory.ReadMemory32(name + (uint)(i * 4));
                }
                uint[] blob = null;
                if (comp > 0 && comp <= ExtraRomFileCacheMax)
                {
                    uint n = (comp + 3) / 4;
                    blob = new uint[n];
                    for (uint w = 0; w < n; w++)
                        blob[w] = memory.ReadMemory32(load + w * 4);
                }
                ExtraRomOpenFile slot = FindExtraRomOpenFile(label);
                if (slot == null)
                {
                    if (_romFileCount >= ExtraRomFileMax)
                        return;
                    if (_romFiles == null)
                        _romFiles = new ExtraRomOpenFile[ExtraRomFileMax];
                    slot = new ExtraRomOpenFile();
                    _romFiles[_romFileCount] = slot;
                    _romFileCount++;
                }
                slot.Index = index;
                slot.Entry = filesEntry;
                slot.Words = words;
                slot.Name = name;
                slot.NameWords = nameWords;
                slot.Real = real;
                slot.Comp = comp;
                slot.Load = load;
                slot.Data = blob;
                slot.Label = label;
                BootLog.Write("[NkBinLoader] ExtraROM FILE" +
                    (index >= 0 ? "[" + index + "]" : "") +
                    " cached " + slot.Label +
                    " entry=0x" + filesEntry.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    (blob != null ? "" : " (FILESentry only; dump LZX stays at load)") +
                    " (restore if firmware RAM reuses ExtraROM tail; do not invent 0x81360000)");
            }
            catch (System.Exception ex)
            {
                BootLog.Write("[NkBinLoader] ExtraROM FILE cache skipped " + label +
                    ": " + ex.Message);
            }
        }

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
                BootLog.Write("[NkBinLoader] ExtraROM FILE[25] cached entry=0x" +
                    filesEntry.ToString("X8") +
                    " real=" + real +
                    " comp=" + comp +
                    " load=0x" + load.ToString("X8") +
                    " (restore if firmware RAM reuses ExtraROM tail)");
            }
            catch (System.Exception ex)
            {
                BootLog.Write("[NkBinLoader] ExtraROM FILE[25] cache skipped: " + ex.Message);
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

        public static void CacheExtraRomTocModule(
            ProcessorEmulator.Core.Emulation.IMemoryManager memory,
            uint romhdr, uint tocEntry, int index, string name)
        {
            if (memory == null || tocEntry == 0 || string.IsNullOrEmpty(name))
                return;
            if (IsTv2ClientCe(name) || IsExtraRomOpenFile(name))
                return;
            try
            {
                if (romhdr != 0)
                    _extraRomHdr = romhdr;
                var toc = new uint[8];
                for (int i = 0; i < 8; i++)
                    toc[i] = memory.ReadMemory32(tocEntry + (uint)(i * 4));
                uint e32 = toc[5];
                uint o32 = toc[6];
                uint dest = 0;
                uint[] e32Words = null;
                uint[] o32Words = null;
                if (e32 != 0)
                {
                    e32Words = new uint[32];
                    for (int i = 0; i < e32Words.Length; i++)
                        e32Words[i] = memory.ReadMemory32(e32 + (uint)(i * 4));
                    uint objcnt = e32Words[0] & 0xFFFF;
                    if (o32 != 0 && objcnt > 0 && objcnt <= 16)
                    {
                        o32Words = new uint[objcnt * 6];
                        for (int i = 0; i < o32Words.Length; i++)
                            o32Words[i] = memory.ReadMemory32(o32 + (uint)(i * 4));
                        if (o32Words.Length >= 5)
                            dest = o32Words[4];
                    }
                }
                ExtraRomTocMod slot = FindCachedTocByEntry(tocEntry);
                if (slot == null)
                    slot = FindCachedExtraRomToc(name);
                if (slot == null)
                {
                    if (_romTocCount >= 128)
                        return;
                    if (_romTocMods == null)
                        _romTocMods = new ExtraRomTocMod[128];
                    slot = new ExtraRomTocMod();
                    _romTocMods[_romTocCount] = slot;
                    _romTocCount++;
                }
                slot.Index = index;
                slot.Name = name;
                slot.Entry = tocEntry;
                slot.Attr = toc[0];
                slot.LoadVa = toc.Length > 7 ? toc[7] : 0;
                slot.Dest = dest;
                slot.E32 = e32;
                slot.O32 = o32;
                slot.TocWords = toc;
                slot.E32Words = e32Words;
                slot.O32Words = o32Words;
                slot.Vbase = e32Words != null && e32Words.Length > 2 ? e32Words[2] : 0;
                slot.Decompressed = false;
                slot.DecompDest = 0;
                slot.LoadE32Ok = false;
                if (o32Words != null && o32Words.Length >= 6)
                {
                    int nsec = o32Words.Length / 6;
                    slot.DataPtr = new uint[nsec];
                    slot.DataLen = new uint[nsec];
                    slot.Data = new uint[nsec][];
                    for (int s = 0; s < nsec; s++)
                    {
                        uint psize = o32Words[s * 6 + 2];
                        uint dataptr = o32Words[s * 6 + 3];
                        if (dataptr == 0 || psize == 0 || psize > 0x40000)
                            continue;
                        uint n = (psize + 3) / 4;
                        var blob = new uint[n];
                        for (uint w = 0; w < n; w++)
                            blob[w] = memory.ReadMemory32(dataptr + w * 4);
                        slot.DataPtr[s] = dataptr;
                        slot.DataLen[s] = psize;
                        slot.Data[s] = blob;
                    }
                }
            }
            catch (System.Exception ex)
            {
                BootLog.Write("[NkBinLoader] ExtraROM TOC[" + index + "] " + name +
                    " cache skipped: " + ex.Message);
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

        private static ExtraRomOpenFile FindExtraRomOpenFile(string want)
        {
            if (_romFiles == null || string.IsNullOrEmpty(want))
                return null;
            string look = RomLookupName(want);
            for (int i = 0; i < _romFileCount; i++)
            {
                ExtraRomOpenFile slot = _romFiles[i];
                if (slot == null || string.IsNullOrEmpty(slot.Label))
                    continue;
                if (NamesMatchRom(want, slot.Label) || NamesMatchRom(look, slot.Label))
                    return slot;
            }
            return null;
        }

        private static bool TrySelectExtraRomOpenFile(string want, out uint filesEntry,
            out uint attr, out uint real, out uint comp, out uint load)
        {
            filesEntry = 0;
            attr = 0;
            real = 0;
            comp = 0;
            load = 0;
            ExtraRomOpenFile slot = FindExtraRomOpenFile(want);
            if (slot == null || slot.Entry == 0 || slot.Words == null)
                return false;
            _romFile = slot;
            _romFilePos = 0;
            _romFileDestOn = false;
            _romFileIoLogged = false;
            filesEntry = slot.Entry;
            attr = slot.Words[0];
            real = slot.Real;
            comp = slot.Comp;
            load = slot.Load;
            return true;
        }

        private static void TryRestoreExtraRomOpenFileIfClobbered(MipsBus bus, string want)
        {
            ExtraRomOpenFile slot = FindExtraRomOpenFile(want);
            if (bus == null || slot == null || slot.Entry == 0 || slot.Words == null)
                return;
            uint liveAttr = 0;
            uint liveName = 0;
            uint liveReal = 0;
            uint liveComp = 0;
            uint liveLoad = 0;
            try
            {
                liveAttr = bus.Read32(slot.Entry);
                liveName = bus.Read32(slot.Entry + FilesNameOff);
                liveReal = bus.Read32(slot.Entry + FilesRealSize);
                liveComp = bus.Read32(slot.Entry + FilesCompSize);
                liveLoad = bus.Read32(slot.Entry + FilesLoadOff);
            }
            catch
            {
            }
            if (liveAttr == slot.Words[0] && liveName == slot.Name
                && liveReal == slot.Real && liveComp == slot.Comp
                && liveLoad == slot.Load && liveReal != 0)
                return;
            try
            {
                for (int i = 0; i < slot.Words.Length; i++)
                    bus.Write32(slot.Entry + (uint)(i * 4), slot.Words[i]);
                if (slot.Name != 0 && slot.NameWords != null)
                {
                    for (int i = 0; i < slot.NameWords.Length; i++)
                        bus.Write32(slot.Name + (uint)(i * 4), slot.NameWords[i]);
                }
                uint liveLoad0 = 0;
                try
                {
                    if (slot.Load != 0)
                        liveLoad0 = bus.Read32(slot.Load);
                }
                catch
                {
                }
                if (slot.Data != null && slot.Load != 0 && liveLoad0 == 0)
                {
                    for (int w = 0; w < slot.Data.Length; w++)
                        bus.Write32(slot.Load + (uint)(w * 4), slot.Data[w]);
                }
                System.Console.WriteLine("[Hive] ExtraROM FILE restored " + slot.Label +
                    " entry=0x" + slot.Entry.ToString("X8") +
                    " real=" + slot.Real +
                    " load=0x" + slot.Load.ToString("X8") +
                    " (was attr=0x" + liveAttr.ToString("X8") +
                    " real=" + liveReal +
                    "; firmware RAM reused ExtraROM tail; do not invent 0x81360000)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM FILE restore-fail " +
                    (slot.Label ?? want) + " " + ex.Message);
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
            if (_romFileAttach)
                return TryStartExtraRomOpenFileDecompress(bus, regs, ref programCounter);
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
                " (firmware BinaryDecompressROM; dump FILE record; do not invent e32)");
            return true;
        }

        private static bool TryStartExtraRomOpenFileDecompress(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            ExtraRomOpenFile slot = _romFile;
            if (!_romFileAttach || slot == null || slot.Entry == 0 || slot.Real == 0 || slot.Comp == 0)
                return false;
            _romFileAttach = false;
            if (slot.Real > ExtraRomFileDestMax || slot.Comp > ExtraRomFileCacheMax)
                return false;
            uint src = ExtraRomFileSrc;
            uint dest = ExtraRomFileDest;
            try
            {
                uint n = (slot.Comp + 3) / 4;
                uint[] blob = slot.Data;
                for (uint w = 0; w < n; w++)
                {
                    uint word = blob != null && w < blob.Length
                        ? blob[w]
                        : bus.Read32(slot.Load + w * 4);
                    bus.Write32(src + w * 4, word);
                }
                uint pages = (slot.Real + 0x1FFFu) & ~0xFFFu;
                if (pages > ExtraRomFileDestMax)
                    pages = ExtraRomFileDestMax;
                for (uint i = 0; i < pages; i += 4)
                    bus.Write32(dest + i, 0);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM FILE dest-prep fail " +
                    slot.Label + " " + ex.Message +
                    " (do not invent 0x81360000)");
                return false;
            }
            regs[4] = src;
            regs[5] = slot.Comp;
            regs[6] = dest;
            regs[7] = slot.Real;
            _romFileSavedSp = regs[29];
            regs[29] = _romFileSavedSp - 32;
            try
            {
                bus.Write32(regs[29] + 16, 0);
                bus.Write32(regs[29] + 20, 1);
                bus.Write32(regs[29] + 24, 0x1000);
            }
            catch
            {
            }
            _romFileDecompRa = NameCopyContinue;
            _romFilePos = 0;
            _romFileDestOn = true;
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
            System.Console.WriteLine("[Hive] ExtraROM FILE CEDecompressROM " + slot.Label +
                " dest=0x" + dest.ToString("X8") + " src=0x" + src.ToString("X8") +
                " real=" + slot.Real +
                " comp=" + slot.Comp +
                " src0=0x" + src0.ToString("X8") +
                " (firmware BinaryDecompressROM; dump FILE record; do not invent e32)");
            return true;
        }

        private static bool TryFinishExtraRomOpenFileDecompress(MipsBus bus, uint[] regs, uint pc)
        {
            if (_romFileDecompRa == 0 || pc != _romFileDecompRa)
                return false;
            _romFileDecompRa = 0;
            if (regs != null && regs.Length > 29 && _romFileSavedSp != 0)
                regs[29] = _romFileSavedSp;
            _romFileSavedSp = 0;
            uint ret = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint dest0 = 0;
            ExtraRomOpenFile slot = _romFile;
            try
            {
                if (bus != null)
                    dest0 = bus.Read32(ExtraRomFileDest);
            }
            catch
            {
            }
            System.Console.WriteLine("[Hive] ExtraROM FILE CEDecompressROM ret " +
                (slot != null ? slot.Label : "") +
                " v0=0x" + ret.ToString("X8") +
                " dest=0x" + ExtraRomFileDest.ToString("X8") +
                " word=0x" + dest0.ToString("X8") +
                (slot != null && ret == slot.Real ? " (firmware expanded FILE real)" : "") +
                " (do not invent e32; FILE[26] tv2clientcorece.dll is 6398464)");
            BootLog.DecompressRom(slot != null ? slot.Label : "", ExtraRomFileDest, ret,
                slot != null && ret == slot.Real
                    ? "firmware expanded FILE real; dest 0x8F400000 class"
                    : "FILE type-8 CEDecompressROM; dest 0x8F400000 class");
            return true;
        }

        public static bool TryFinishTv2FileDecompress(MipsBus bus, uint[] regs, uint pc)
        {
            if (TryFinishExtraRomOpenFileDecompress(bus, regs, pc))
                return false;
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
            BootLog.DecompressRom("tv2clientce.exe", Tv2FileDest, v0,
                v0 == _tv2FileReal
                    ? "firmware expanded FILE real; FILE[25] dest 0x8F140000"
                    : "FILE[25] type-8 CEDecompressROM; dest 0x8F140000");
            return false;
        }

        public static bool IsTv2FileHandle(uint handle)
        {
            return _tv2FileDestOn && _tv2FileEntry != 0 && handle == _tv2FileEntry;
        }

        public static bool IsExtraRomOpenFileHandle(uint handle)
        {
            return _romFileDestOn && _romFile != null && _romFile.Entry != 0
                && handle == _romFile.Entry;
        }

        public static bool TryServeTv2SetFilePointer(uint[] regs, uint jalrTarget, ref uint target)
        {
            if (jalrTarget != Win32SetFilePointer || regs == null || regs.Length <= 7)
                return false;
            if (IsExtraRomOpenFileHandle(regs[4]))
                return ServeExtraRomOpenFilePointer(regs, ref target);
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
            if (IsExtraRomOpenFileHandle(regs[4]))
                return ServeExtraRomOpenFileRead(bus, regs, ref programCounter);
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
            if (IsExtraRomOpenFileHandle(regs[4]))
            {
                regs[2] = 0;
                programCounter = regs[31];
                ExtraRomOpenFile mapped = _romFile;
                System.Console.WriteLine("[Hive] ExtraROM FILE CreateFileMapping v0=0 " +
                    (mapped != null ? mapped.Label : "") +
                    " (firmware object+6=3; MapO32 ReadFile of dump PE; do not invent e32)");
                return true;
            }
            if (!IsTv2FileHandle(regs[4]))
                return false;
            regs[2] = 0;
            programCounter = regs[31];
            System.Console.WriteLine("[Hive] FILE[25] CreateFileMapping v0=0" +
                " (firmware object+6=3; MapO32 ReadFile of dump PE; do not invent e32)");
            return true;
        }

        private static bool ServeExtraRomOpenFilePointer(uint[] regs, ref uint target)
        {
            ExtraRomOpenFile slot = _romFile;
            if (slot == null || regs == null || regs.Length <= 7)
                return false;
            uint dist = regs[5];
            uint method = regs[7];
            uint pos = _romFilePos;
            if (method == 0)
                pos = dist;
            else if (method == 1)
                pos = _romFilePos + dist;
            else if (method == 2)
                pos = slot.Real + dist;
            if (pos > slot.Real)
                pos = slot.Real;
            _romFilePos = pos;
            regs[2] = pos;
            target = regs.Length > 31 ? regs[31] : target;
            if (!_romFileIoLogged)
            {
                _romFileIoLogged = true;
                System.Console.WriteLine("[Hive] ExtraROM FILE SetFilePointer " + slot.Label +
                    " pos=0x" + pos.ToString("X") + " method=" + method +
                    " (dump FILE bytes; do not invent e32)");
            }
            return true;
        }

        private static bool ServeExtraRomOpenFileRead(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            ExtraRomOpenFile slot = _romFile;
            if (bus == null || slot == null || regs == null || regs.Length <= 31)
                return false;
            uint dest = regs[5];
            uint count = regs[6];
            uint outN = regs[7];
            if (dest == 0 || count == 0 || count > ExtraRomFileDestMax)
                return false;
            uint left = slot.Real > _romFilePos ? slot.Real - _romFilePos : 0;
            if (count > left)
                count = left;
            uint srcPos = _romFilePos;
            try
            {
                for (uint i = 0; i < count; i += 4)
                {
                    uint word = bus.Read32(ExtraRomFileDest + _romFilePos + i);
                    if (i + 4 <= count)
                        bus.Write32((dest + i) & ~3u, word);
                    else
                    {
                        for (uint b = 0; b < count - i; b++)
                        {
                            uint src = ExtraRomFileDest + _romFilePos + i + b;
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
                System.Console.WriteLine("[Hive] ExtraROM FILE ReadFile fail " +
                    slot.Label + " " + ex.Message);
                return false;
            }
            _romFilePos += count;
            regs[2] = 1;
            programCounter = regs[31];
            if (count != 0 && srcPos == 0)
            {
                uint destWord = 0;
                uint fileWord = 0;
                try
                {
                    destWord = bus.Read32(dest);
                    fileWord = bus.Read32(ExtraRomFileDest + srcPos);
                }
                catch
                {
                }
                System.Console.WriteLine("[Hive] ExtraROM FILE ReadFile " + slot.Label +
                    " dest=0x" + dest.ToString("X8") + " pos=0x" + srcPos.ToString("X") +
                    " n=0x" + count.ToString("X") +
                    " dest-word=0x" + destWord.ToString("X8") +
                    " file-word=0x" + fileWord.ToString("X8") +
                    " (ExtraRomFileDest+raw; do not invent section bytes)");
            }
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

        private static bool TrySelectExtraRomToc(MipsBus bus, string baseName,
            out uint tocEntry, out uint attr, out int index, out uint dest, out uint e32)
        {
            tocEntry = 0;
            attr = 0;
            index = -1;
            dest = 0;
            e32 = 0;
            if (string.IsNullOrEmpty(baseName))
                return false;
            if (IsTv2ClientCe(baseName) || IsExtraRomOpenFile(baseName))
                return false;

            if (IsMscoreeDll(baseName))
                TryRestoreExtraRomMscoreeIfClobbered(bus);
            else if (IsOle32Dll(baseName))
                TryRestoreExtraRomOle32IfClobbered(bus);
            else if (NamesMatchRom(baseName, "ddi_nop.dll"))
                TryRestoreExtraRomIfClobbered(bus, _ddiNopTocEntry);

                ExtraRomTocMod slot = FindCachedExtraRomToc(baseName);
            if (slot != null)
            {
                if (!IsMscoreeDll(baseName) && !IsOle32Dll(baseName)
                    && !NamesMatchRom(baseName, "ddi_nop.dll"))
                    TryRestoreExtraRomTocModIfClobbered(bus, slot);
                TryHostExtraRomE32O32(bus, slot);
                tocEntry = slot.LiveEntry != 0 ? slot.LiveEntry : slot.Entry;
                attr = (slot.Attr & 0xFFFFEFFFu) | 0x2040u;
                if (IsMscoreeDll(baseName) && _mscoreeAttr != 0)
                    attr = _mscoreeAttr;
                else if (IsOle32Dll(baseName) && _ole32Attr != 0)
                    attr = _ole32Attr;
                index = slot.Index;
                dest = slot.Dest;
                e32 = slot.LiveE32 != 0 ? slot.LiveE32 : slot.E32;
                if (IsMscoreeDll(baseName) && _mscoreeE32 != 0 && slot.LiveE32 == 0)
                    e32 = _mscoreeE32;
                else if (IsOle32Dll(baseName) && _ole32E32 != 0 && slot.LiveE32 == 0)
                    e32 = _ole32E32;
                return tocEntry != 0;
            }

            string look = RomLookupName(baseName);
            if (bus != null
                && TryFindTocModule(bus, ExtraRomToc(bus), 128, look, out tocEntry, out attr))
            {
                ExtraRomTocMod live = FindCachedTocByEntry(tocEntry);
                index = live != null ? live.Index : ExtraRomTocIndex(tocEntry);
                dest = live != null ? live.Dest : 0;
                try
                {
                    e32 = bus.Read32(tocEntry + 0x14);
                }
                catch
                {
                }
                return true;
            }

            if (IsMscoreeDll(baseName) && _mscoreeTocEntry != 0)
            {
                tocEntry = _mscoreeTocEntry;
                attr = _mscoreeAttr != 0 ? _mscoreeAttr
                    : (_mscoreeTocWords != null ? _mscoreeTocWords[0] : 0);
                index = 46;
                e32 = _mscoreeE32;
                return true;
            }
            if (IsOle32Dll(baseName) && _ole32TocEntry != 0)
            {
                tocEntry = _ole32TocEntry;
                attr = _ole32Attr != 0 ? _ole32Attr
                    : (_ole32TocWords != null ? _ole32TocWords[0] : 0);
                index = 34;
                e32 = _ole32E32;
                return true;
            }
            if (NamesMatchRom(baseName, "ddi_nop.dll") && _ddiNopTocEntry != 0)
            {
                tocEntry = _ddiNopTocEntry;
                attr = (_ddiNopAttr & 0xFFFFEFFFu) | 0x2040u;
                index = 33;
                return true;
            }
            return false;
        }

        private static ExtraRomTocMod FindCachedExtraRomToc(string name)
        {
            if (_romTocMods == null || string.IsNullOrEmpty(name))
                return null;
            string look = RomLookupName(name);
            ExtraRomTocMod family = null;
            int familyHits = 0;
            bool wantIptvHal = IsIptvHalAsk(name) || IsIptvHalAsk(look);
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod slot = _romTocMods[i];
                if (slot == null || string.IsNullOrEmpty(slot.Name))
                    continue;
                if (NamesMatchRom(name, slot.Name) || NamesMatchRom(look, slot.Name))
                    return slot;
                if (wantIptvHal && IsIptvHalAsk(slot.Name))
                {
                    familyHits++;
                    if (family == null)
                        family = slot;
                }
            }
            // CE may say iptvhal.dll while ExtraROM only has
            // iptvhal_*. Attach the one dump name. Two hits stay
            // a miss so we do not pick a module CE did not name.
            if (familyHits == 1)
                return family;
            return null;
        }

        private static ExtraRomTocMod FindCachedTocByEntry(uint tocEntry)
        {
            if (_romTocMods == null || tocEntry == 0)
                return null;
            for (int i = 0; i < _romTocCount; i++)
            {
                ExtraRomTocMod slot = _romTocMods[i];
                if (slot != null && (slot.Entry == tocEntry || slot.LiveEntry == tocEntry))
                    return slot;
            }
            return null;
        }

        private static int ExtraRomTocIndex(uint tocEntry)
        {
            ExtraRomTocMod slot = FindCachedTocByEntry(tocEntry);
            if (slot != null)
                return slot.Index;
            if (tocEntry == _ddiNopTocEntry)
                return 33;
            if (tocEntry == _mscoreeTocEntry)
                return 46;
            if (tocEntry == _ole32TocEntry)
                return 34;
            if (_extraRomHdr != 0 && tocEntry >= _extraRomHdr + TocFirst)
            {
                uint off = tocEntry - (_extraRomHdr + TocFirst);
                if ((off % TocEntrySize) == 0)
                    return (int)(off / TocEntrySize);
            }
            return -1;
        }

        private static string ExtraRomTocTag(uint tocEntry, ExtraRomTocMod? cached)
        {
            int index = cached != null ? cached.Index : ExtraRomTocIndex(tocEntry);
            if (index >= 0)
                return "TOC[" + index + "]";
            return "TOC";
        }

        private static void TryRestoreExtraRomTocModIfClobbered(MipsBus bus, ExtraRomTocMod slot)
        {
            if (bus == null || slot == null || slot.Entry == 0 || slot.TocWords == null)
                return;
            uint liveE32 = 0;
            uint liveO32 = 0;
            uint liveObjcnt = 0;
            uint liveVsize = 0;
            try
            {
                liveE32 = bus.Read32(slot.Entry + 0x14);
                liveO32 = bus.Read32(slot.Entry + 0x18);
                if (liveE32 != 0)
                    liveObjcnt = bus.Read32(liveE32) & 0xFFFF;
                if (liveO32 != 0)
                    liveVsize = bus.Read32(liveO32);
            }
            catch
            {
            }
            if (liveE32 == slot.E32 && liveE32 != 0 && liveObjcnt != 0 && liveVsize != 0)
                return;
            try
            {
                for (int i = 0; i < slot.TocWords.Length; i++)
                    bus.Write32(slot.Entry + (uint)(i * 4), slot.TocWords[i]);
                if (slot.E32 != 0 && slot.E32Words != null)
                {
                    for (int i = 0; i < slot.E32Words.Length; i++)
                        bus.Write32(slot.E32 + (uint)(i * 4), slot.E32Words[i]);
                }
                if (slot.O32 != 0 && slot.O32Words != null)
                {
                    for (int i = 0; i < slot.O32Words.Length; i++)
                        bus.Write32(slot.O32 + (uint)(i * 4), slot.O32Words[i]);
                }
                System.Console.WriteLine("[Hive] ExtraROM TOC[" + slot.Index + "] " +
                    slot.Name + " restored e32=0x" + slot.E32.ToString("X8") +
                    " o32=0x" + slot.O32.ToString("X8") +
                    " (was 0x" + liveE32.ToString("X8") +
                    "; firmware RAM reused ExtraROM tail; do not invent 0x81360000)");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[" + slot.Index + "] " +
                    slot.Name + " restore-fail " + ex.Message);
            }
        }

        // NK LoadE32 copies e32_rom at TOC+0x14 and o32 at +0x18
        // because those VAs stay in NK XIP. ExtraROM tail does
        // not. Write dump-cached TOC/e32/o32 to ExtraRomE32Host
        // and point object+0 at that copy. Do not invent e32
        // bytes or 0x81360000.
        public static bool TryServeExtraRomLoadE32(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0)
                return false;
            ExtraRomTocMod slot;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return false;
                uint tocEntry = bus.Read32(obj);
                slot = FindCachedTocByEntry(tocEntry);
            }
            catch
            {
                return false;
            }
            if (slot == null || slot.E32Words == null)
                return false;
            TryRestoreExtraRomTocModIfClobbered(bus, slot);
            if (!TryHostExtraRomE32O32(bus, slot) || slot.LiveEntry == 0)
                return false;
            try
            {
                bus.Write32(obj, slot.LiveEntry);
            }
            catch
            {
                return false;
            }
            _loadE32Obj = obj;
            TryMarkExtraRomO32Compressed(bus, slot.LiveEntry);
            NoteLoadE32(slot.Name, slot.Index);
            return true;
        }

        private static bool TryHostExtraRomE32O32(MipsBus bus, ExtraRomTocMod slot)
        {
            if (bus == null || slot == null || slot.TocWords == null || slot.E32Words == null)
                return false;
            uint e32Bytes = ExtraRomHostE32Bytes(slot);
            uint o32Bytes = slot.O32Words != null ? (uint)slot.O32Words.Length * 4 : 0;
            string name = slot.Name ?? "";
            uint nameBytes = ((uint)name.Length + 4) & ~3u;
            uint tocBytes = 32;
            uint span = (tocBytes + e32Bytes + o32Bytes + nameBytes + 0xF) & ~0xFu;
            bool first = slot.LiveEntry == 0;
            if (first)
            {
                if (_e32HostPool < ExtraRomE32Host
                    || _e32HostPool + span > ExtraRomE32HostLim)
                    return false;
                slot.LiveEntry = _e32HostPool;
                _e32HostPool += span;
            }
            slot.LiveE32 = slot.LiveEntry + tocBytes;
            slot.LiveO32 = o32Bytes != 0 ? slot.LiveE32 + e32Bytes : 0;
            slot.LiveName = (o32Bytes != 0 ? slot.LiveO32 + o32Bytes : slot.LiveE32 + e32Bytes);
            CommitExtraRomE32Host(bus);
            if (!WriteHostExtraRomE32O32(bus, slot, name))
                return false;
            if (!first)
                return true;
            uint o32Real = slot.O32Words != null && slot.O32Words.Length > 4 ? slot.O32Words[4] : 0;
            BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                " e32_rom v0= dest-word=0 destDump=0x" + slot.Dest.ToString("X8") +
                " dest0=0x" + (slot.Dest & SlotMask).ToString("X8") +
                " object+6=0 0x80028844=False o32.real=0x" + o32Real.ToString("X8"));
            return true;
        }

        // FILE[25] dest 0x8F140000 is kseg0 RAM because
        // CEDecompressROM Write32 commits those pages. Do the
        // same for ExtraRomE32Host (next to that dest). Do not
        // invent a third B000FF at 0x81360000.
        private static void CommitExtraRomE32Host(MipsBus bus)
        {
            if (bus == null || _e32HostCommitted)
                return;
            try
            {
                for (uint i = ExtraRomE32Host; i < ExtraRomE32HostLim; i += 4)
                    bus.Write32(i, 0);
                _e32HostCommitted = true;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM e32-host commit-fail " + ex.Message +
                    " (0x8F148000 next to FILE dest 0x8F140000; do not invent 0x81360000)");
            }
        }

        public static void TryLogExtraRomLoadE32(MipsBus bus, uint[] regs, bool isRet, uint lastError)
        {
            if (bus == null || regs == null || regs.Length <= 4)
                return;
            uint obj = regs[4];
            if (obj == 0 || (isRet && _loadE32Obj != 0))
                obj = _loadE32Obj != 0 ? _loadE32Obj : obj;
            if (obj == 0)
                return;
            uint entry = 0;
            uint type = 0;
            uint obj6 = 0;
            try
            {
                type = bus.Read8(obj + 4);
                entry = bus.Read32(obj);
                obj6 = (uint)(bus.Read8(obj + 6) | (bus.Read8(obj + 7) << 8));
            }
            catch
            {
                return;
            }
            if (type != TocAttachType)
                return;
            ExtraRomTocMod slot = FindCachedTocByEntry(entry);
            if (slot == null || string.IsNullOrEmpty(slot.Name) || slot.Index < 0)
                return;
            uint v0 = isRet && regs.Length > 2 ? regs[2] : 0;
            uint dest0 = slot.Dest & SlotMask;
            uint destWord = PeekDestWord(bus, dest0);
            string line = "[Hive] TOC[" + slot.Index + "] " + slot.Name +
                (isRet ? " LoadE32-ret" : " LoadE32") +
                " v0=0x" + v0.ToString("X") +
                " dest-word=0x" + destWord.ToString("X") +
                " dest0=0x" + dest0.ToString("X8") +
                " object+6=" + obj6 +
                " 0x80028844=" + slot.FwMapO32;
            if (!isRet)
            {
                BeginLoadE32Watch(slot, regs, lastError);
                _loadE32RomBit = type & LoadE32RomBit;
                if (NamesMatchRom(slot.Name, "ddi_nop.dll"))
                    LatchDdiNopFileObj(obj);
            }
            else
            {
                if (IsLoadE32Success(v0, _loadE32RetPc))
                {
                    slot.LoadE32Ok = true;
                    BeginLoadE32OkWatch(slot, _loadE32WatchA0);
                }
                _loadE32Obj = 0;
                ClearLoadE32Watch();
            }
            BootLog.Write(line);
        }

        // NK TOC type-7 LoadE32 also returns v0=0 on success
        // (0x80019990 / 0x800199A4). Log ret-pc so ExtraROM
        // can match. Fail is v0=0xC1 / 0x47E only. CurMSec
        // leftover a1 is not o32. NK e32 bytes are not in-repo.
        public static void TryBeginNkLoadE32(MipsBus bus, uint[] regs)
        {
            if (_loadE32Watch || _nkLoadE32Watch || bus == null || regs == null || regs.Length <= 4)
                return;
            uint obj = regs[4];
            if (obj == 0)
                return;
            uint toc = 0;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return;
                toc = bus.Read32(obj);
            }
            catch
            {
                return;
            }
            if (toc == 0 || FindCachedTocByEntry(toc) != null)
                return;
            uint e32 = 0;
            uint o32 = 0;
            string name = "";
            try
            {
                e32 = bus.Read32(toc + 0x14);
                o32 = bus.Read32(toc + 0x18);
                uint np = bus.Read32(toc + 0x10);
                name = ReadAscii(bus, np);
            }
            catch
            {
                return;
            }
            if (string.IsNullOrEmpty(name))
                return;
            if (!WantNkLoadE32Log(name) && _nkLoadE32Logged >= 8)
                return;
            uint o32v = PeekLoadE32Word(bus, o32);
            uint o32p = PeekLoadE32Word(bus, o32 != 0 ? o32 + 0xC : 0);
            uint dumpToc0 = PeekDestWord(bus, toc);
            _nkLoadE32Watch = true;
            _nkLoadE32Name = name;
            _nkLoadE32E32 = e32;
            _nkLoadE32O32 = o32;
            _nkLoadE32O32Vsize = o32v;
            _nkLoadE32O32Ptr = o32p;
            _nkLoadE32Obj = obj;
            _nkLoadE32Toc = toc;
            _nkLoadE32DumpToc0 = dumpToc0;
            _nkChkRa = 0;
            _nkChkA0 = 0;
            _nkChkA1 = 0;
            _nkChkA2 = 0;
            _nkChkWord = 0;
            _nkChkV0 = 0xFFFFFFFFu;
            _nkChkSpan = null;
            _nkChkSeen = false;
            _nkRomBit = LoadE32RomBit;
            _nkCmpPc = 0;
            _nkCmpOp = null;
            _nkCmpLhs = 0;
            _nkCmpRhs = 0;
            _nkCmpFirstPc = 0;
            _nkCmpFirstOp = null;
            _nkCmpFirstLhs = 0;
            _nkCmpFirstRhs = 0;
            _nkRetPc = 0;
        }

        public static void TryFinishNkLoadE32(MipsBus bus, uint[] regs)
        {
            if (!_nkLoadE32Watch)
                return;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint destWord = PeekDestWord(bus, _nkLoadE32Toc);
            uint obj6 = PeekObj6(bus, _nkLoadE32Obj);
            BootLog.Write("[Hive] NK " + _nkLoadE32Name +
                " LoadE32-ret v0=0x" + v0.ToString("X") +
                " dest-word=0x" + destWord.ToString("X") +
                " dest0=0x" + _nkLoadE32Toc.ToString("X8") +
                " object+6=" + obj6 +
                " 0x80028844=False");
            if (IsLoadE32Success(v0, _nkRetPc))
            {
                _nkLoadE32Ok = _nkLoadE32Name +
                    " success=LoadE32 ret-pc=0x" + _nkRetPc.ToString("X8") +
                    " v0=0 rombit=" + _nkRomBit +
                    " dumpToc0=0x" + _nkLoadE32DumpToc0.ToString("X8") +
                    " dumpToc0&0x200=" + (_nkLoadE32DumpToc0 & LoadO32VallocBit).ToString("X");
                if (WantNkLoadO32Log(_nkLoadE32Name))
                    BeginNkLoadO32Watch();
            }
            _nkLoadE32Logged++;
            ClearNkLoadE32Watch();
        }

        private static bool WantNkLoadE32Log(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            return NamesMatchRom(name, "fsdmgr.dll")
                || NamesMatchRom(name, "coredll.dll")
                || NamesMatchRom(name, "ceddk.dll")
                || NamesMatchRom(name, "nk.exe")
                || NamesMatchRom(name, "filesys.exe");
        }

        private static bool WantNkLoadO32Log(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            return NamesMatchRom(name, "fsdmgr.dll")
                || NamesMatchRom(name, "coredll.dll")
                || NamesMatchRom(name, "ceddk.dll");
        }

        private static void BeginNkLoadO32Watch()
        {
            _nkLoadO32Watch = true;
            _nkLoadO32Name = _nkLoadE32Name;
            _nkLoadO32Obj = _nkLoadE32Obj;
            _nkLoadO32Toc = _nkLoadE32Toc;
            _nkLoadO32DumpToc0 = _nkLoadE32DumpToc0;
            _nkLoadO32Word0 = 0;
            _nkLoadO32Fp = 0;
            _nkLoadO32Bit200 = false;
            _nkLoadO32Entered = false;
            _nkLoadO32Skip200 = false;
            _nkLoadO32Thunk = false;
            _nkLoadO32Ret = false;
            _nkLoadO32Steps = 0;
            BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                " LoadO32-watch v0= dest-word=0 dest0=0x" +
                _nkLoadO32Toc.ToString("X8") +
                " object+6=0 0x80028844=False");
        }

        private static void ClearNkLoadO32Watch()
        {
            _nkLoadO32Watch = false;
            _nkLoadO32Name = null;
            _nkLoadO32Obj = 0;
            _nkLoadO32Toc = 0;
            _nkLoadO32DumpToc0 = 0;
            _nkLoadO32Word0 = 0;
            _nkLoadO32Fp = 0;
            _nkLoadO32Bit200 = false;
            _nkLoadO32Entered = false;
            _nkLoadO32Skip200 = false;
            _nkLoadO32Thunk = false;
            _nkLoadO32Ret = false;
            _nkLoadO32Steps = 0;
        }

        private static void NoteAfterNkLoadO32(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_nkLoadO32Watch)
                return;
            _nkLoadO32Steps++;
            if (pc == LoadLibSyscallRet || _nkLoadO32Steps > 200000)
            {
                if (!_nkLoadO32Entered)
                    BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                        " LoadO32-not-entered v0= dest-word=0 dest0=0x" +
                        _nkLoadO32Toc.ToString("X8") +
                        " object+6=0 0x80028844=False");
                ClearNkLoadO32Watch();
                return;
            }
            if (pc == LoadO32Rom && !_nkLoadO32Entered)
            {
                uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
                if (_nkLoadO32Obj != 0 && a0 != 0 && a0 != _nkLoadO32Obj)
                    return;
                _nkLoadO32Entered = true;
                uint toc = PeekDestWord(bus, a0 != 0 ? a0 : _nkLoadO32Obj);
                uint fp = toc != 0 ? PeekDestWord(bus, toc) : 0;
                uint live0 = _nkLoadO32Toc != 0
                    ? PeekDestWord(bus, _nkLoadO32Toc) : fp;
                _nkLoadO32Fp = fp;
                _nkLoadO32Word0 = live0;
                _nkLoadO32Bit200 = (fp & LoadO32VallocBit) != 0;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " LoadO32 v0= dest-word=0x" + live0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
                return;
            }
            if (pc == LoadO32SkipValloc && _nkLoadO32Entered && !_nkLoadO32Skip200)
            {
                _nkLoadO32Skip200 = true;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " LoadO32-skip200 v0= dest-word=0x" + _nkLoadO32Word0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
                return;
            }
            if (pc == LoadO32VallocOpen && _nkLoadO32Entered && !_nkLoadO32Thunk)
            {
                _nkLoadO32Thunk = true;
                uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " thunk-enter v0= dest-word=0x" + _nkLoadO32Word0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
                return;
            }
            if (pc == LoadO32RomRet && _nkLoadO32Entered && !_nkLoadO32Ret)
            {
                _nkLoadO32Ret = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint live0 = _nkLoadO32Toc != 0
                    ? PeekDestWord(bus, _nkLoadO32Toc) : _nkLoadO32Word0;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " LoadO32-ret v0=0x" + v0.ToString("X") +
                    " dest-word=0x" + live0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
                ClearNkLoadO32Watch();
                return;
            }
            if (!_nkLoadO32Entered || bus == null || regs == null)
                return;
            uint instr = 0;
            try
            {
                instr = bus.Read32(pc);
            }
            catch
            {
                return;
            }
            uint op = instr >> 26;
            if (!_nkLoadO32Bit200 && op == 0xC && (instr & 0xFFFF) == LoadO32VallocBit)
            {
                uint rs = (instr >> 21) & 31;
                uint lhs = regs.Length > (int)rs ? regs[(int)rs] : 0;
                _nkLoadO32Fp = lhs;
                _nkLoadO32Bit200 = (lhs & LoadO32VallocBit) != 0;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " andi-0x200 v0= dest-word=0x" + _nkLoadO32Word0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
            }
            uint target = 0;
            if (op == 3)
                target = (pc & 0xF0000000u) | ((instr & 0x3FFFFFFu) << 2);
            if (target == LoadO32VallocOpen && !_nkLoadO32Thunk)
            {
                _nkLoadO32Thunk = true;
                uint a0 = regs.Length > 4 ? regs[4] : 0;
                BootLog.Write("[Hive] NK " + _nkLoadO32Name +
                    " jal-pred v0= dest-word=0x" + _nkLoadO32Word0.ToString("X") +
                    " dest0=0x" + _nkLoadO32Toc.ToString("X8") +
                    " object+6=" + PeekObj6(bus, _nkLoadO32Obj) +
                    " 0x80028844=False");
            }
        }

        private static void NoteNkLoadE32FieldJal(MipsBus bus, uint[] regs, uint pc)
        {
            _nkChkSeen = true;
            _nkChkRa = pc + 8;
            _nkChkA0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            _nkChkA1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            _nkChkA2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            _nkChkWord = PeekLoadE32Word(bus, _nkChkA1);
            _nkChkSpan = FormatO32RomPeek(bus, _nkChkA1);
            TryLogCurMSecDecompile(bus);
        }

        // Observe firmware LoadE32 ExtraROM only. Poll last-error
        // and jal targets. Do not jal. Do not rewrite registers.
        // Do not force v0=1. Do not emit BinaryDecompressROM hex
        // (watchdog LOOP_KILL false-positive on that substring).
        public static void TryWatchExtraRomLoadE32(MipsBus bus, uint[] regs, uint pc)
        {
            TryWatchExtraRomFwMap(bus, regs, pc);
            if (pc == RomHdrLinkJal)
                TryLogRomHdrLinkJal(bus, regs);
            if (pc == RomHdrLink)
                TryLogRomHdrLinkEnter(bus, regs);
            if (pc == RomHdrLinkPublish)
                TryLogRomHdrLinkSw(bus, regs, "0x80017308 publish-head");
            if (pc == RomHdrLinkSplice)
                TryLogRomHdrLinkSw(bus, regs, "0x8001731C splice-front");
            if (pc == RomHdrWalk || pc == RomHdrListLoad0 || pc == RomHdrListLoad1
                || pc == RomHdrListLoad2 || pc == RomHdrListLoad3
                || pc == RomHdrListLoad4 || pc == RomHdrListLoad5)
                TryLogRomHdrListWalk(bus, "live pc=0x" + pc.ToString("X8"));
            if (pc == CreateFileMappingObj6 && !_obj6ShLogged)
            {
                _obj6ShLogged = true;
                uint fp = regs != null && regs.Length > 30 ? regs[30] : 0;
                uint s5 = PeekS5(regs);
                uint obj6 = PeekObj6(bus, fp);
                BootLog.Write("[Hive] 0x8001D4F0 object+6=" + obj6 +
                    " s5=0x" + s5.ToString("X") +
                    " dest-word= dest0=0x" + fp.ToString("X8") +
                    " 0x80028844=False");
            }
            if (_loadE32OkWatch)
                NoteAfterLoadE32Ok(bus, regs, pc);
            if (_nkLoadO32Watch)
                NoteAfterNkLoadO32(bus, regs, pc);
            if ((!_loadE32Watch && !_nkLoadE32Watch) || bus == null)
                return;
            NoteLoadE32RetPc(regs, pc);
            _loadE32WatchSteps++;
            if (_loadE32WatchSteps > 200000)
            {
                ClearLoadE32Watch();
                return;
            }
            uint err = ReadThreadLastError(bus);
            if (regs != null && _loadE32CopyRa != 0 && pc == _loadE32CopyRa)
            {
                _loadE32CopyV0 = regs.Length > 2 ? regs[2] : 0;
                _loadE32CopyRa = 0;
            }
            if (regs != null && _nkChkRa != 0 && pc == _nkChkRa)
            {
                _nkChkV0 = regs.Length > 2 ? regs[2] : 0;
                _nkChkRa = 0;
            }
            if (regs != null && _loadE32ChkRa != 0 && pc == _loadE32ChkRa)
            {
                _loadE32ChkV0 = regs.Length > 2 ? regs[2] : 0;
                _loadE32ChkRa = 0;
            }
            if (regs != null)
                FinishLoadE32AfterJal(regs, pc);
            if (_loadE32Watch && err != _loadE32WatchErrNow && _loadE32WatchErrHits < 4)
            {
                _loadE32WatchErrNow = err;
                if (_loadE32WatchErrHits == 0)
                {
                    _loadE32WatchErrPc = pc;
                    _loadE32WatchErrNew = err;
                }
                _loadE32WatchErrHits++;
            }
            if (regs == null)
                return;
            uint instr = 0;
            try
            {
                instr = bus.Read32(pc);
            }
            catch
            {
                return;
            }
            uint target = 0;
            uint op = instr >> 26;
            if (op == 3)
                target = (pc & 0xF0000000u) | ((instr & 0x3FFFFFFu) << 2);
            else if (op == 0 && (instr & 0x3Fu) == 9 && regs.Length > ((int)((instr >> 21) & 0x1F)))
                target = regs[(int)((instr >> 21) & 0x1F)];
            NoteLoadE32BodyCmp(bus, regs, pc, instr);
            if (target == 0)
                return;
            if (_nkLoadE32Watch && target == OemCurMSec && !_nkChkSeen)
                NoteNkLoadE32FieldJal(bus, regs, pc);
            if (!_loadE32Watch)
                return;
            if (target == LoadE32UnitCopy || target == OemCurMSec)
                NoteLoadE32FieldJal(bus, regs, pc, target);
            if (IsLoadE32OemTickJal(target))
                NoteLoadE32AfterJal(bus, regs, pc, target);
            string name = NameLoadE32Jal(target);
            if (string.IsNullOrEmpty(name))
                return;
            bool named = name.Length > 0 && name[0] != '0';
            if (!named && _loadE32WatchJalN >= 8)
                return;
            if (!string.IsNullOrEmpty(_loadE32WatchJal)
                && _loadE32WatchJal.IndexOf(name, System.StringComparison.Ordinal) >= 0)
                return;
            if (!named)
                _loadE32WatchJalN++;
            if (!string.IsNullOrEmpty(_loadE32WatchJal))
                _loadE32WatchJal += ",";
            _loadE32WatchJal += name;
        }

        private static void BeginLoadE32Watch(ExtraRomTocMod slot, uint[] regs, uint err)
        {
            ClearAfterLoadE32();
            _loadE32Watch = true;
            _loadE32WatchName = slot != null ? slot.Name : "";
            _loadE32WatchIndex = slot != null ? slot.Index : -1;
            _loadE32WatchA0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            _loadE32WatchA1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            _loadE32WatchA2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            _loadE32WatchA3 = regs != null && regs.Length > 7 ? regs[7] : 0;
            if (NamesMatchRom(_loadE32WatchName, "ddi_nop.dll"))
                LatchDdiNopFileObj(_loadE32WatchA0);
            _loadE32WatchErr0 = err;
            _loadE32WatchErrNow = err;
            _loadE32WatchErrPc = 0;
            _loadE32WatchErrNew = err;
            _loadE32WatchErrHits = 0;
            _loadE32WatchJalN = 0;
            _loadE32WatchJal = "";
            _loadE32WatchSteps = 0;
            _loadE32CopyRa = 0;
            _loadE32CopyV0 = 0xFFFFFFFFu;
            _loadE32CopyA0 = 0;
            _loadE32CopyA1 = 0;
            _loadE32CopyA2 = 0;
            _loadE32CopyWord = 0;
            _loadE32ChkRa = 0;
            _loadE32ChkV0 = 0xFFFFFFFFu;
            _loadE32ChkA0 = 0;
            _loadE32ChkA1 = 0;
            _loadE32ChkA2 = 0;
            _loadE32ChkWord = 0;
            _loadE32ChkOff = 0;
            _loadE32ChkSpan = null;
            _loadE32CopySeen = false;
            _loadE32ChkSeen = false;
            ClearLoadE32Cmp();
        }

        private static void ClearNkLoadE32Watch()
        {
            _nkLoadE32Watch = false;
            _nkLoadE32Name = null;
            _nkLoadE32E32 = 0;
            _nkLoadE32O32 = 0;
            _nkLoadE32O32Vsize = 0;
            _nkLoadE32O32Ptr = 0;
            _nkLoadE32Obj = 0;
            _nkLoadE32Toc = 0;
            _nkLoadE32DumpToc0 = 0;
            _nkChkRa = 0;
            _nkChkA0 = 0;
            _nkChkA1 = 0;
            _nkChkA2 = 0;
            _nkChkWord = 0;
            _nkChkV0 = 0xFFFFFFFFu;
            _nkChkSpan = null;
            _nkChkSeen = false;
            _nkRomBit = 0;
            _nkCmpPc = 0;
            _nkCmpOp = null;
            _nkCmpLhs = 0;
            _nkCmpRhs = 0;
            _nkCmpFirstPc = 0;
            _nkCmpFirstOp = null;
            _nkCmpFirstLhs = 0;
            _nkCmpFirstRhs = 0;
            _nkRetPc = 0;
        }

        private static void ClearLoadE32Watch()
        {
            ClearAfterLoadE32();
            _loadE32Watch = false;
            _loadE32WatchName = null;
            _loadE32WatchIndex = -1;
            _loadE32WatchA0 = 0;
            _loadE32WatchA1 = 0;
            _loadE32WatchA2 = 0;
            _loadE32WatchA3 = 0;
            _loadE32WatchErr0 = 0;
            _loadE32WatchErrNow = 0;
            _loadE32WatchErrPc = 0;
            _loadE32WatchErrNew = 0;
            _loadE32WatchErrHits = 0;
            _loadE32WatchJalN = 0;
            _loadE32WatchJal = null;
            _loadE32WatchSteps = 0;
            _loadE32CopyRa = 0;
            _loadE32CopyV0 = 0xFFFFFFFFu;
            _loadE32CopyA0 = 0;
            _loadE32CopyA1 = 0;
            _loadE32CopyA2 = 0;
            _loadE32CopyWord = 0;
            _loadE32ChkRa = 0;
            _loadE32ChkV0 = 0xFFFFFFFFu;
            _loadE32ChkA0 = 0;
            _loadE32ChkA1 = 0;
            _loadE32ChkA2 = 0;
            _loadE32ChkWord = 0;
            _loadE32ChkOff = 0;
            _loadE32ChkSpan = null;
            _loadE32CopySeen = false;
            _loadE32ChkSeen = false;
            ClearLoadE32Cmp();
        }

        private static void ClearLoadE32Cmp()
        {
            _loadE32RomBit = 0;
            _loadE32CmpPc = 0;
            _loadE32CmpOp = null;
            _loadE32CmpLhs = 0;
            _loadE32CmpRhs = 0;
            _loadE32CmpFirstPc = 0;
            _loadE32CmpFirstOp = null;
            _loadE32CmpFirstLhs = 0;
            _loadE32CmpFirstRhs = 0;
            _loadE32CmpAfterPc = 0;
            _loadE32CmpAfterOp = null;
            _loadE32CmpAfterLhs = 0;
            _loadE32CmpAfterRhs = 0;
            _loadE32CmpN = 0;
            _loadE32CmpLog = null;
            _loadE32RetPc = 0;
            _loadE32RetV0 = 0;
            _loadE32RetLogged = false;
        }

        private static void BeginLoadE32OkWatch(ExtraRomTocMod slot, uint obj)
        {
            _loadE32OkWatch = true;
            _loadE32OkName = slot != null ? slot.Name : "";
            _loadE32OkIndex = slot != null ? slot.Index : -1;
            _loadE32OkObj = obj;
            if (NamesMatchRom(_loadE32OkName, "ddi_nop.dll"))
                LatchDdiNopFileObj(obj);
            _loadE32OkDest = slot != null ? slot.Dest : 0;
            _loadE32OkDest0 = _loadE32OkDest & SlotMask;
            _loadE32OkLiveEntry = slot != null ? slot.LiveEntry : 0;
            _loadE32OkLiveE32 = slot != null ? slot.LiveE32 : 0;
            _loadE32OkDumpToc0 = DumpTocWord0(slot);
            _loadE32OkLoadVa = SlotLoadVa(slot);
            _loadE32OkObj6 = 0;
            _loadE32OkWrapPc = LoadE32RomRet;
            _loadE32OkLoadO32 = false;
            _loadE32OkCopyO32 = false;
            _loadE32OkPred = false;
            _loadE32OkPredFail = false;
            _loadE32OkLoadO32Ret = false;
            _loadE32OkWrapFail = false;
            _loadE32OkWrapAfter = false;
            _loadE32OkMapO32 = false;
            _loadE32OkMapInner = false;
            _loadE32OkMap28844 = false;
            _loadE32OkMapValloc = false;
            _loadE32OkMapVallocV0 = 0xFFFFFFFFu;
            _loadE32OkMapVallocA0 = 0;
            _loadE32OkMapVallocA2 = 0;
            _loadE32OkMapVallocA3 = 0;
            _loadE32OkWrapValloc = false;
            _loadE32OkO32Walk = false;
            _loadE32OkS5Hi = false;
            _loadE32OkFlagsChk = false;
            _loadE32OkC1 = false;
            _loadE32OkS5 = 0;
            _loadE32OkSp24 = 0;
            _loadE32OkDestAfter = 0;
            _loadE32OkBindImp = false;
            _loadE32OkCallDll = false;
            _loadE32OkDecomp = false;
            _skipDisasmLogged = false;
            _wrapAfterDisasmLogged = false;
            _loadE32OkPredRa = 0;
            _loadE32OkPredV0 = 0xFFFFFFFFu;
            _loadE32OkFp = 0;
            _loadE32OkBit200 = false;
            _loadE32OkBit200Seen = false;
            _loadE32OkSkip200 = false;
            _loadE32OkValloc = false;
            _loadE32OkVallocRa = 0;
            _loadE32OkVallocV0 = 0xFFFFFFFFu;
            _loadE32OkSteps = 0;
        }

        private static void ClearLoadE32OkWatch()
        {
            _loadE32OkWatch = false;
            _loadE32OkName = null;
            _loadE32OkIndex = -1;
            _loadE32OkObj = 0;
            _loadE32OkDest = 0;
            _loadE32OkDest0 = 0;
            _loadE32OkWrapPc = 0;
            _loadE32OkLiveEntry = 0;
            _loadE32OkLiveE32 = 0;
            _loadE32OkDumpToc0 = 0;
            _loadE32OkLoadVa = 0;
            _loadE32OkObj6 = 0;
            _loadE32OkLoadO32 = false;
            _loadE32OkCopyO32 = false;
            _loadE32OkPred = false;
            _loadE32OkPredFail = false;
            _loadE32OkLoadO32Ret = false;
            _loadE32OkWrapFail = false;
            _loadE32OkWrapAfter = false;
            _loadE32OkMapO32 = false;
            _loadE32OkMapInner = false;
            _loadE32OkMap28844 = false;
            _loadE32OkMapValloc = false;
            _loadE32OkMapVallocV0 = 0xFFFFFFFFu;
            _loadE32OkMapVallocA0 = 0;
            _loadE32OkMapVallocA2 = 0;
            _loadE32OkMapVallocA3 = 0;
            _loadE32OkWrapValloc = false;
            _loadE32OkO32Walk = false;
            _loadE32OkS5Hi = false;
            _loadE32OkFlagsChk = false;
            _loadE32OkC1 = false;
            _loadE32OkS5 = 0;
            _loadE32OkSp24 = 0;
            _loadE32OkDestAfter = 0;
            _loadE32OkBindImp = false;
            _loadE32OkCallDll = false;
            _loadE32OkDecomp = false;
            _skipDisasmLogged = false;
            _wrapAfterDisasmLogged = false;
            _loadE32OkPredRa = 0;
            _loadE32OkPredV0 = 0xFFFFFFFFu;
            _loadE32OkFp = 0;
            _loadE32OkBit200 = false;
            _loadE32OkBit200Seen = false;
            _loadE32OkSkip200 = false;
            _loadE32OkValloc = false;
            _loadE32OkVallocRa = 0;
            _loadE32OkVallocV0 = 0xFFFFFFFFu;
            _loadE32OkSteps = 0;
        }

        private static uint ReadThreadLastError(MipsBus bus)
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

        private static string FormatLastError(uint err)
        {
            if (err == 0xFFFFFFFF)
                return "unmapped";
            string name = err == 2 ? " FILE_NOT_FOUND"
                : err == 3 ? " PATH_NOT_FOUND"
                : err == 8 ? " NOT_ENOUGH_MEMORY"
                : err == 14 ? " OUTOFMEMORY"
                : err == 87 ? " INVALID_PARAMETER"
                : err == 126 ? " MOD_NOT_FOUND"
                : err == 193 ? " BAD_EXE_FORMAT"
                : err == LoadE32Err47E ? " LoadE32-0x47E"
                : err == 1114 ? " DLL_INIT_FAILED"
                : "";
            return err + name;
        }

        private static bool IsLoadE32Success(uint v0, uint retPc)
        {
            if (v0 == LoadE32BadExe || v0 == LoadE32Err47E)
                return false;
            if (retPc == LoadE32FailBadExe || retPc == LoadE32Fail47E)
                return false;
            if (retPc == LoadE32Ok || retPc == LoadE32Epilogue)
                return v0 == 0;
            return v0 == 0;
        }

        private static string NameLoadE32Ret(uint retPc, uint v0)
        {
            if (retPc == LoadE32FailBadExe || v0 == LoadE32BadExe)
                return "fail=ERROR_BAD_EXE_FORMAT v0=0xC1";
            if (retPc == LoadE32Fail47E || v0 == LoadE32Err47E)
                return "fail=0x47E";
            if ((retPc == LoadE32Ok || retPc == LoadE32Epilogue || retPc == 0) && v0 == 0)
                return "success=LoadE32";
            if (v0 == 0)
                return "success=LoadE32";
            return "fail=LoadE32 v0=0x" + v0.ToString("X8");
        }

        public static string NameLoadE32RetPublic(uint v0)
        {
            return NameLoadE32Ret(0, v0);
        }

        // Dump nk.exe: v0=0 at 0x80019990 / 0x800199A4 is
        // success. Dest word 0 after that is CopyO32 miss.
        private static string DescribeLoadE32Ret(MipsBus bus, ExtraRomTocMod slot,
            uint v0, uint err, bool liveMapped, uint live0, uint dump0)
        {
            string lite = DescribeE32Lite(bus, _loadE32WatchA1, slot);
            string jal = !string.IsNullOrEmpty(_loadE32WatchJal)
                ? " jal=" + _loadE32WatchJal : " jal=none";
            if (!string.IsNullOrEmpty(_afterRets))
                jal += " after=" + _afterRets;
            string errAt = _loadE32WatchErrHits > 0
                ? " last-error-set " + FormatLastError(_loadE32WatchErrNew) +
                    " at pc=0x" + _loadE32WatchErrPc.ToString("X8")
                : (err == _loadE32WatchErr0
                    ? (err == 2
                        ? " last-error stale FILE_NOT_FOUND (CreateFileFail leftover; LoadE32 did not SetLastError)"
                        : " last-error unchanged")
                    : " last-error-set " + FormatLastError(err));
            string copy = !liveMapped ? " LiveE32-unmapped"
                : (live0 == dump0 && dump0 != 0 ? " e32_rom dump-real" : " e32_rom mismatch");
            string dest = DescribeLoadE32Dest(bus, slot);
            string named = NameLoadE32Ret(_loadE32RetPc, v0);
            string retpc = " ret-pc=0x" + _loadE32RetPc.ToString("X8");
            string body;
            if (IsLoadE32Success(v0, _loadE32RetPc))
                body = " " + named + retpc +
                    " rombit=(obj+4)&2=" + _loadE32RomBit +
                    " " + NameLoadE32BodyNote(slot) + dest +
                    " (type-7 ROM path; memcpy then move v0,0; dest word 0 is CopyO32/CEDecompressROM/VALLOC not yet; not LoadE32 fail)";
            else if (lite.IndexOf("empty", System.StringComparison.Ordinal) >= 0)
                body = " " + named + retpc +
                    " fail=before-e32_rom-copy rombit=(obj+4)&2=" + _loadE32RomBit +
                    " first-cmp " +
                    FormatLoadE32Cmp(_loadE32CmpFirstPc, _loadE32CmpFirstOp,
                        _loadE32CmpFirstLhs, _loadE32CmpFirstRhs);
            else
                body = " " + named + retpc + " " + NameLoadE32BodyNote(slot) + dest;
            return lite + copy + jal + body + " " + errAt;
        }

        private static string DescribeLoadE32Dest(MipsBus bus, ExtraRomTocMod slot)
        {
            if (slot == null)
                return "";
            uint destDump = slot.Dest;
            uint dest0 = destDump & SlotMask;
            uint word0 = PeekDestWord(bus, dest0);
            uint wordDump = destDump != dest0 ? PeekDestWord(bus, destDump) : word0;
            return " dest0=0x" + dest0.ToString("X8") +
                " dest-word=0x" + word0.ToString("X8") +
                " destDump=0x" + destDump.ToString("X8") +
                " dump-word=0x" + wordDump.ToString("X8");
        }

        private static string DescribeE32Lite(MipsBus bus, uint lite, ExtraRomTocMod slot)
        {
            if (lite == 0)
                return " e32_lite=a1-0";
            uint w0 = 0;
            uint w1 = 0;
            uint w2 = 0;
            uint w3 = 0;
            bool mapped = false;
            try
            {
                if (bus != null)
                {
                    w0 = bus.Read32(lite);
                    w1 = bus.Read32(lite + 4);
                    w2 = bus.Read32(lite + 8);
                    w3 = bus.Read32(lite + 12);
                    mapped = true;
                }
            }
            catch
            {
            }
            if (!mapped)
                return " e32_lite=0x" + lite.ToString("X8") + "-unmapped";
            uint dumpVbase = slot != null && slot.E32Words != null && slot.E32Words.Length > 2
                ? slot.E32Words[2] : 0;
            uint dumpVsize = slot != null && slot.E32Words != null && slot.E32Words.Length > 5
                ? slot.E32Words[5] : 0;
            uint dump0 = slot != null && slot.E32Words != null && slot.E32Words.Length > 0
                ? slot.E32Words[0] : 0;
            bool empty = w0 == 0 && w1 == 0 && w2 == 0 && w3 == 0;
            bool hasVbase = dumpVbase != 0 && (w0 == dumpVbase || w1 == dumpVbase
                || w2 == dumpVbase || w3 == dumpVbase);
            bool hasVsize = dumpVsize != 0 && (w0 == dumpVsize || w1 == dumpVsize
                || w2 == dumpVsize || w3 == dumpVsize);
            bool hasObjcnt = dump0 != 0 && ((w0 & 0xFFFF) == (dump0 & 0xFFFF));
            string which = empty ? "empty"
                : ((hasObjcnt ? "objcnt" : "objcnt-miss") +
                    (hasVbase ? "+vbase" : "+vbase-miss") +
                    (hasVsize ? "+vsize" : "+vsize-miss"));
            return " e32_lite=0x" + lite.ToString("X8") +
                " w0=0x" + w0.ToString("X8") +
                " w1=0x" + w1.ToString("X8") +
                " w2=0x" + w2.ToString("X8") +
                " w3=0x" + w3.ToString("X8") +
                " " + which;
        }

        private static string NameLoadE32Jal(uint target)
        {
            if (target == 0)
                return "";
            if (target == BinaryDecompressRom || target == BinaryDecompressInner)
                return "BinaryDecompressROM";
            if (target == CreateFileFail)
                return "CreateFileFail";
            if (target == KernelReadFile)
                return "ReadFile";
            if (target == KernelCreateFileMapping)
                return "CreateFileMapping";
            if (target == 0x8001D3A0u)
                return "CreateFile";
            if (target == 0x800283FCu)
                return "VALLOC";
            if (target == MapO32VirtualCopy)
                return "VirtualCopy";
            if (target == MapO32Decompress)
                return "MapO32Decompress";
            if (target == LoadE32Rom)
                return "";
            if (target == LoadE32UnitCopy)
                return "e32_unit_copy";
            if (target == OemCurMSec)
                return "CurMSec";
            if (target == OemReadCount)
                return "ReadCount";
            if (target == OemTickDelta)
                return "TickDelta";
            if (target == OemCountStall)
                return "CountStall";
            if (target == OemReadCompare)
                return "ReadCompare";
            if (target == OemWriteCompare)
                return "WriteCompare";
            if (target == NkMoveV0A0)
                return "MoveV0A0";
            return "0x" + target.ToString("X8");
        }

        private static bool IsLoadE32OemTickJal(uint target)
        {
            return target == OemReadCount
                || target == OemTickDelta
                || target == OemCountStall
                || target == OemReadCompare
                || target == OemWriteCompare
                || target == NkMoveV0A0;
        }

        private static string NameLoadE32AfterNeed(uint target, uint a0, uint a1, uint word)
        {
            if (target == OemReadCount)
                return "mfc0 Count leftover; not a LoadE32 compare";
            if (target == OemReadCompare)
                return "mfc0 Compare leftover; not a LoadE32 compare";
            if (target == OemWriteCompare)
                return "mtc0 Compare leftover; not a LoadE32 compare";
            if (target == OemTickDelta)
                return "tick vs 0x80338F70 leftover; later MMIO 0xB04007D4; not OalLoadE32Arg dest";
            if (target == OemCountStall)
                return "Count+Compare stall leftover; not a LoadE32 compare";
            if (target == NkMoveV0A0)
                return "move v0,a0 leftover; not a LoadE32 compare";
            return "OEM tick leftover a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " word=0x" + word.ToString("X8") +
                "; not a LoadE32 compare";
        }

        private static void NoteLoadE32AfterJal(MipsBus bus, uint[] regs, uint pc, uint target)
        {
            if (_afterN >= LoadE32AfterMax)
                return;
            int i = _afterN;
            _afterN++;
            _afterRa[i] = pc + 8;
            _afterName[i] = NameLoadE32Jal(target);
            _afterA0[i] = regs != null && regs.Length > 4 ? regs[4] : 0;
            _afterA1[i] = regs != null && regs.Length > 5 ? regs[5] : 0;
            _afterA2[i] = regs != null && regs.Length > 6 ? regs[6] : 0;
            _afterWord[i] = PeekLoadE32Word(bus, _afterA1[i] != 0 ? _afterA1[i] : _afterA0[i]);
            _afterNeed[i] = NameLoadE32AfterNeed(target, _afterA0[i], _afterA1[i], _afterWord[i]);
            TryLogLoadE32JalDecompile(bus, target, _afterName[i]);
        }

        private static void FinishLoadE32AfterJal(uint[] regs, uint pc)
        {
            for (int i = 0; i < _afterN; i++)
            {
                if (_afterRa[i] == 0 || pc != _afterRa[i])
                    continue;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                _afterRa[i] = 0;
                string ret = _afterName[i] + " v0=0x" + v0.ToString("X8") +
                    " a0=0x" + _afterA0[i].ToString("X8") +
                    " a1=0x" + _afterA1[i].ToString("X8") +
                    " a2=0x" + _afterA2[i].ToString("X8") +
                    " word=0x" + _afterWord[i].ToString("X8");
                if (string.IsNullOrEmpty(_afterRets))
                    _afterRets = ret;
                else
                    _afterRets += "; " + ret;
            }
        }

        private static void ClearAfterLoadE32()
        {
            for (int i = 0; i < LoadE32AfterMax; i++)
            {
                _afterRa[i] = 0;
                _afterName[i] = null;
                _afterA0[i] = 0;
                _afterA1[i] = 0;
                _afterA2[i] = 0;
                _afterWord[i] = 0;
                _afterNeed[i] = null;
            }
            _afterN = 0;
            _afterRets = null;
        }

        private static void NoteLoadE32FieldJal(MipsBus bus, uint[] regs, uint pc, uint target)
        {
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            uint word = PeekLoadE32Word(bus, a1 != 0 ? a1 : a0);
            uint ra = pc + 8;
            if (target == LoadE32UnitCopy && !_loadE32CopySeen)
            {
                _loadE32CopySeen = true;
                _loadE32CopyRa = ra;
                _loadE32CopyA0 = a0;
                _loadE32CopyA1 = a1;
                _loadE32CopyA2 = a2;
                _loadE32CopyWord = word;
                return;
            }
            if (target != OemCurMSec || _loadE32ChkSeen)
                return;
            _loadE32ChkSeen = true;
            _loadE32ChkRa = ra;
            _loadE32ChkA0 = a0;
            _loadE32ChkA1 = a1;
            _loadE32ChkA2 = a2;
            _loadE32ChkWord = word;
            _loadE32ChkOff = 0;
            _loadE32ChkSpan = FormatO32RomPeek(bus, a1);
            ExtraRomTocMod slot = FindCachedExtraRomToc(_loadE32WatchName);
            uint live = slot != null ? slot.LiveE32 : 0;
            if (a1 != 0 && live != 0 && a1 >= live && a1 < live + 0x80)
                _loadE32ChkOff = a1 - live;
            TryLogCurMSecDecompile(bus);
        }

        private static uint PeekLoadE32Word(MipsBus bus, uint va)
        {
            if (bus == null || va == 0)
                return 0;
            try
            {
                return bus.Read32(va);
            }
            catch
            {
                return 0;
            }
        }

        private static string FormatLoadE32Cmp(uint pc, string op, uint lhs, uint rhs)
        {
            if (string.IsNullOrEmpty(op) || pc == 0)
                return "none";
            return "pc=0x" + pc.ToString("X8") + " " + op +
                " lhs=0x" + lhs.ToString("X8") +
                " rhs=0x" + rhs.ToString("X8");
        }

        private static bool TryDecodeLoadE32Cmp(uint[] regs, uint instr,
            out string op, out uint lhs, out uint rhs)
        {
            op = null;
            lhs = 0;
            rhs = 0;
            uint opcode = instr >> 26;
            uint rs = (instr >> 21) & 31;
            uint rt = (instr >> 16) & 31;
            uint fn = instr & 0x3F;
            int simm = (short)(instr & 0xFFFF);
            uint rsV = regs != null && regs.Length > (int)rs ? regs[(int)rs] : 0;
            uint rtV = regs != null && regs.Length > (int)rt ? regs[(int)rt] : 0;
            if (opcode == 4)
            {
                op = "beq";
                lhs = rsV;
                rhs = rtV;
                return true;
            }
            if (opcode == 5)
            {
                op = "bne";
                lhs = rsV;
                rhs = rtV;
                return true;
            }
            if (opcode == 6)
            {
                op = "blez";
                lhs = rsV;
                rhs = 0;
                return true;
            }
            if (opcode == 7)
            {
                op = "bgtz";
                lhs = rsV;
                rhs = 0;
                return true;
            }
            if (opcode == 0xA)
            {
                op = "slti";
                lhs = rsV;
                rhs = (uint)simm;
                return true;
            }
            if (opcode == 0xB)
            {
                op = "sltiu";
                lhs = rsV;
                rhs = (uint)simm;
                return true;
            }
            if (opcode == 0 && fn == 0x2A)
            {
                op = "slt";
                lhs = rsV;
                rhs = rtV;
                return true;
            }
            if (opcode == 0 && fn == 0x2B)
            {
                op = "sltu";
                lhs = rsV;
                rhs = rtV;
                return true;
            }
            if (opcode == 0xC && (instr & 0xFFFF) == LoadE32RomBit)
            {
                op = "andi-rombit";
                lhs = rsV;
                rhs = LoadE32RomBit;
                return true;
            }
            return false;
        }

        private static void NoteLoadE32BodyCmp(MipsBus bus, uint[] regs, uint pc, uint instr)
        {
            if (pc < LoadE32Rom || pc >= LoadE32BodyLim)
                return;
            string op;
            uint lhs;
            uint rhs;
            if (!TryDecodeLoadE32Cmp(regs, instr, out op, out lhs, out rhs))
                return;
            if (_nkLoadE32Watch)
            {
                if (string.IsNullOrEmpty(_nkCmpFirstOp))
                {
                    _nkCmpFirstPc = pc;
                    _nkCmpFirstOp = op;
                    _nkCmpFirstLhs = lhs;
                    _nkCmpFirstRhs = rhs;
                }
                _nkCmpPc = pc;
                _nkCmpOp = op;
                _nkCmpLhs = lhs;
                _nkCmpRhs = rhs;
            }
            if (!_loadE32Watch)
                return;
            if (string.IsNullOrEmpty(_loadE32CmpFirstOp))
            {
                _loadE32CmpFirstPc = pc;
                _loadE32CmpFirstOp = op;
                _loadE32CmpFirstLhs = lhs;
                _loadE32CmpFirstRhs = rhs;
            }
            _loadE32CmpPc = pc;
            _loadE32CmpOp = op;
            _loadE32CmpLhs = lhs;
            _loadE32CmpRhs = rhs;
            bool afterCopy = _loadE32CopySeen && _loadE32CopyRa == 0;
            if (!afterCopy)
                return;
            _loadE32CmpAfterPc = pc;
            _loadE32CmpAfterOp = op;
            _loadE32CmpAfterLhs = lhs;
            _loadE32CmpAfterRhs = rhs;
            string key = "0x" + pc.ToString("X8");
            if (!string.IsNullOrEmpty(_loadE32CmpLog)
                && _loadE32CmpLog.IndexOf(key, System.StringComparison.Ordinal) >= 0)
                return;
            if (_loadE32CmpN >= 8)
                return;
            if (string.IsNullOrEmpty(_loadE32CmpLog))
                _loadE32CmpLog = key;
            else
                _loadE32CmpLog += "," + key;
            _loadE32CmpN++;
            if (pc == LoadE32Ok || pc == LoadE32Fail47E
                || pc == LoadE32FailBadExe || pc == LoadE32Epilogue)
                return;
        }

        private static string FormatO32RomPeek(MipsBus bus, uint va)
        {
            if (va == 0)
                return "va=0";
            uint vsize = PeekLoadE32Word(bus, va);
            uint rva = PeekLoadE32Word(bus, va + 4);
            uint psize = PeekLoadE32Word(bus, va + 8);
            uint dataptr = PeekLoadE32Word(bus, va + 0xC);
            uint real = PeekLoadE32Word(bus, va + 0x10);
            uint flags = PeekLoadE32Word(bus, va + 0x14);
            return "va=0x" + va.ToString("X8") +
                " vsize=0x" + vsize.ToString("X") +
                " rva=0x" + rva.ToString("X") +
                " psize=0x" + psize.ToString("X") +
                " dataptr=0x" + dataptr.ToString("X8") +
                " real=0x" + real.ToString("X8") +
                " flags=0x" + flags.ToString("X");
        }

        private static string FormatDumpO32(ExtraRomTocMod? slot)
        {
            if (slot == null || slot.O32Words == null || slot.O32Words.Length < 4)
                return "";
            return " psize=0x" + slot.O32Words[2].ToString("X")
                + " dataptr=0x" + slot.O32Words[3].ToString("X8");
        }

        private static uint PeekSpWord(MipsBus bus, uint[] regs, uint off)
        {
            if (bus == null || regs == null || regs.Length <= 29)
                return 0;
            uint sp = regs[29];
            if (sp == 0)
                return 0;
            return PeekDestWord(bus, sp + off);
        }

        private static uint PeekS5(uint[] regs)
        {
            if (regs == null || regs.Length <= 21)
                return 0;
            return regs[21];
        }

        private static string FormatWrapBits(uint s5, uint sp24)
        {
            return " s5=0x" + s5.ToString("X8") +
                " s5&2=" + (s5 & WrapS5Bit2).ToString("X") +
                " s5&0x8000=" + (s5 & WrapS5CallDll).ToString("X") +
                " 0x24(sp)=0x" + sp24.ToString("X8") +
                " 0x24(sp)&0x2000=" + (sp24 & E32ImageDllBit).ToString("X") +
                " (0x24(sp) is LoadE32 e32_imageflags; ExtraROM 0x212E0003 has 0x2000 DLL; do not invent 0x2000)";
        }

        private static string FormatSkipVsDdiNop()
        {
            return FormatSkipVsDdiNop(_loadE32OkName);
        }

        private static string FormatSkipVsDdiNop(string name)
        {
            bool bcm = name != null && NamesMatchRom(name, "bcmuart.dll");
            bool ddi = name != null && NamesMatchRom(name, "ddi_nop.dll");
            if (bcm)
                return " vs ddi_nop OpenFile/LoadDriver object+6>=2 c1c0bc4 dest 0x01981000; BuiltIn bcmuart LoadLibrary skip dest 0";
            if (ddi)
                return " vs BuiltIn bcmuart LoadLibrary LoadO32 skip dest 0; this is OpenFile/LoadDriver MapO32/CEDecompressROM";
            return " vs ddi_nop OpenFile/LoadDriver (object+6>=2) vs BuiltIn LoadLibrary skip";
        }

        private static string FormatSkipWatchBits()
        {
            return " object+6=" + _loadE32OkObj6 +
                " 0x8001AC9c=" + _loadE32OkMapInner +
                " 0x80028844=" + _loadE32OkMap28844 +
                " 0x800283fc=" + _loadE32OkWrapValloc +
                " 0x8001AF20=" + _loadE32OkO32Walk +
                " 0x8001E45c=" + _loadE32OkS5Hi +
                " 0x8001E4a8=" + _loadE32OkFlagsChk +
                " C1=" + _loadE32OkC1 +
                " dest-after-0x80016848=0x" + _loadE32OkDestAfter.ToString("X8") +
                FormatWrapBits(_loadE32OkS5, _loadE32OkSp24);
        }

        private static void MarkFwMapO32()
        {
            ExtraRomTocMod slot = FindCachedExtraRomToc(_loadE32OkName);
            if (slot != null)
                slot.FwMapO32 = true;
        }

        private static void MarkBuiltInSkip()
        {
            ExtraRomTocMod slot = FindCachedExtraRomToc(_loadE32OkName);
            if (slot != null)
                slot.BuiltInSkip = true;
        }

        private static void PersistSkipCompare(MipsBus bus)
        {
            if (string.IsNullOrEmpty(_loadE32OkName))
                return;
            string snap = _loadE32OkName + FormatSkipWatchBits() + FormatSkipVsDdiNop();
            bool bcm = NamesMatchRom(_loadE32OkName, "bcmuart.dll");
            bool ddi = NamesMatchRom(_loadE32OkName, "ddi_nop.dll");
            if (bcm)
                _bcmSkipSnap = snap;
            if (ddi)
                _ddiSkipSnap = snap;
            if (!bcm && !ddi)
                return;
            HiveWatch(bus, "skip-compare", 0);
        }

        private static void PersistOpenFileMap(ExtraRomTocMod slot, uint obj6, uint dest, uint destWord)
        {
            if (slot == null || !IsCompareExtraRom(slot))
                return;
            string snap = slot.Name +
                " OpenFile/LoadDriver object+6=" + obj6 +
                " 0x8001AC9c=" + slot.LoggedFwMapInner +
                " 0x80028844=" + slot.LoggedFwMap28844 +
                " MapO32=" + slot.LoggedFwMapO32 +
                " dest=0x" + dest.ToString("X8") +
                " dest-word=0x" + destWord.ToString("X8") +
                " " + FormatDumpO32(slot) +
                FormatLoadVaPhys(slot.Name, SlotLoadVa(slot)) +
                " (firmware MapO32/CEDecompressROM; serve dest on this path; do not invent dest; do not invent a map at 0x8178C000)";
            bool ddi = NamesMatchRom(slot.Name, "ddi_nop.dll");
            if (ddi)
                _ddiSkipSnap = snap;
            else
                _bcmMapSnap = snap;
            BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                " openfile-map v0= dest-word=0x" + destWord.ToString("X") +
                " dest0=0x" + dest.ToString("X8") +
                " object+6=" + obj6 +
                " 0x80028844=" + slot.LoggedFwMap28844);
        }

        // ddi_nop dest is OpenFile/LoadDriver MapO32, not the
        // ExtraROM BuiltIn LoadE32-ok watch. Log the same
        // compare bits so Boot can set _ddiSkipSnap without
        // attributing that MapO32 to bcmuart skip.
        private static void TryWatchExtraRomFwMap(MipsBus bus, uint[] regs, uint pc)
        {
            if (bus == null || regs == null)
                return;
            if (pc != MapO32Rom && pc != MapO32InnerJal && pc != MapO32Decompress
                && pc != BinaryDecompressRom)
                return;
            ExtraRomTocMod slot = FindExtraRomMapSlot(bus, regs, pc);
            if (!IsCompareExtraRom(slot))
                return;
            bool first = false;
            if (pc == MapO32Rom && !slot.LoggedFwMapO32)
            {
                slot.LoggedFwMapO32 = true;
                first = true;
            }
            else if (pc == MapO32InnerJal && !slot.LoggedFwMapInner)
            {
                slot.LoggedFwMapInner = true;
                first = true;
            }
            else if (pc == MapO32Decompress && !slot.LoggedFwMap28844)
            {
                slot.LoggedFwMap28844 = true;
                first = true;
            }
            else if (pc == BinaryDecompressRom && !slot.FwMapO32)
                first = true;
            if (!first)
                return;
            slot.FwMapO32 = true;
            uint obj = regs.Length > 4 ? regs[4] : 0;
            uint obj6 = PeekObj6(bus, obj);
            uint dest = 0;
            uint destWord = 0;
            if (pc == MapO32Decompress)
                dest = regs[4];
            else if (pc == BinaryDecompressRom)
                dest = regs.Length > 6 ? regs[6] : 0;
            else if (regs.Length > 5 && regs[5] != 0)
                dest = PeekDestWord(bus, regs[5] + 8);
            if (dest == 0)
                dest = slot.DecompDest != 0 ? slot.DecompDest : (slot.Dest & SlotMask);
            destWord = PeekDestWord(bus, dest);
            PersistOpenFileMap(slot, obj6, dest, destWord);
            string ev = pc == MapO32InnerJal ? "0x8001ACC4"
                : pc == MapO32Decompress ? "0x80028844"
                : pc == BinaryDecompressRom ? "CEDecompressROM"
                : "MapO32";
            uint destDump = slot.Dest;
            uint wordDump = destDump != 0 ? PeekDestWord(bus, destDump) : destWord;
            string miss = (pc == MapO32Decompress || pc == MapO32Rom || pc == MapO32InnerJal)
                && destWord == 0 && wordDump == 0
                ? " destDump-word=0; 0x800283FC(o32.real) not memcpy"
                : "";
            BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                " " + ev +
                " v0= dest-word=0x" + destWord.ToString("X") +
                " dest0=0x" + dest.ToString("X8") +
                " object+6=" + obj6 +
                " 0x80028844=" + slot.LoggedFwMap28844 +
                miss);
        }

        // Dump nk.exe: CurMSec jal ReadCount then 0x803392B0 /
        // 0x80342C60. Guest bytes are not in-repo; later Boot
        // fills this. Incoming a1 is leftover LoadE32, not o32.
        private static void TryLogCurMSecDecompile(MipsBus bus)
        {
            _curMSecDisasmLogged = true;
        }

        // Guest NK/OAL bytes are not in-repo. Read them from the
        // live bus on the later Boot (no dump folder I/O).
        private static void TryLogLoadE32JalDecompile(MipsBus bus, uint va, string name)
        {
        }

        private static bool IsMipsJrRa(uint instr)
        {
            return (instr >> 26) == 0 && (instr & 0x3Fu) == 8 && ((instr >> 21) & 0x1Fu) == 31;
        }

        private static readonly string[] MipsRegName = {
            "0", "at", "v0", "v1", "a0", "a1", "a2", "a3",
            "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
            "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"
        };

        private static string MipsRn(uint r)
        {
            return MipsRegName[r & 31];
        }

        private static string FormatMipsOp(uint pc, uint instr)
        {
            uint op = instr >> 26;
            uint rs = (instr >> 21) & 31;
            uint rt = (instr >> 16) & 31;
            uint rd = (instr >> 11) & 31;
            uint sh = (instr >> 6) & 31;
            uint fn = instr & 0x3F;
            int simm = (short)(instr & 0xFFFF);
            uint uimm = instr & 0xFFFF;
            if (op == 0)
            {
                if (instr == 0)
                    return "nop";
                if (fn == 0)
                    return "sll " + MipsRn(rd) + "," + MipsRn(rt) + "," + sh;
                if (fn == 2)
                    return "srl " + MipsRn(rd) + "," + MipsRn(rt) + "," + sh;
                if (fn == 3)
                    return "sra " + MipsRn(rd) + "," + MipsRn(rt) + "," + sh;
                if (fn == 8)
                    return "jr " + MipsRn(rs);
                if (fn == 9)
                    return "jalr " + MipsRn(rd) + "," + MipsRn(rs);
                if (fn == 0x21)
                    return "addu " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x23)
                    return "subu " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x24)
                    return "and " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x25)
                    return "or " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x27)
                    return "nor " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x2A)
                    return "slt " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x2B)
                    return "sltu " + MipsRn(rd) + "," + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x1A)
                    return "div " + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x1B)
                    return "divu " + MipsRn(rs) + "," + MipsRn(rt);
                if (fn == 0x10)
                    return "mfhi " + MipsRn(rd);
                if (fn == 0x12)
                    return "mflo " + MipsRn(rd);
                return "spec fn=0x" + fn.ToString("X");
            }
            if (op == 0x10)
            {
                if (rs == 0)
                    return "mfc0 " + MipsRn(rt) + "," + rd;
                if (rs == 4)
                    return "mtc0 " + MipsRn(rt) + "," + rd;
            }
            if (op == 2 || op == 3)
            {
                uint t = (pc & 0xF0000000u) | ((instr & 0x3FFFFFFu) << 2);
                return (op == 2 ? "j " : "jal ") + "0x" + t.ToString("X8");
            }
            if (op == 4)
                return "beq " + MipsRn(rs) + "," + MipsRn(rt) + "," + simm;
            if (op == 5)
                return "bne " + MipsRn(rs) + "," + MipsRn(rt) + "," + simm;
            if (op == 6)
                return "blez " + MipsRn(rs) + "," + simm;
            if (op == 7)
                return "bgtz " + MipsRn(rs) + "," + simm;
            if (op == 8)
                return "addi " + MipsRn(rt) + "," + MipsRn(rs) + "," + simm;
            if (op == 9)
                return "addiu " + MipsRn(rt) + "," + MipsRn(rs) + "," + simm;
            if (op == 0xA)
                return "slti " + MipsRn(rt) + "," + MipsRn(rs) + "," + simm;
            if (op == 0xB)
                return "sltiu " + MipsRn(rt) + "," + MipsRn(rs) + "," + simm;
            if (op == 0xC)
                return "andi " + MipsRn(rt) + "," + MipsRn(rs) + ",0x" + uimm.ToString("X");
            if (op == 0xD)
                return "ori " + MipsRn(rt) + "," + MipsRn(rs) + ",0x" + uimm.ToString("X");
            if (op == 0xE)
                return "xori " + MipsRn(rt) + "," + MipsRn(rs) + ",0x" + uimm.ToString("X");
            if (op == 0xF)
                return "lui " + MipsRn(rt) + ",0x" + uimm.ToString("X");
            if (op == 0x20)
                return "lb " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            if (op == 0x23)
                return "lw " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            if (op == 0x24)
                return "lbu " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            if (op == 0x25)
                return "lhu " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            if (op == 0x28)
                return "sb " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            if (op == 0x2B)
                return "sw " + MipsRn(rt) + "," + simm + "(" + MipsRn(rs) + ")";
            return "op" + op.ToString("X") + "=0x" + instr.ToString("X8");
        }

        // Body notes only. v0=0 is success, not fail=LoadE32Cmp.
        private static string NameLoadE32BodyNote(ExtraRomTocMod slot)
        {
            uint live = slot != null ? slot.LiveE32 : 0;
            uint liveO32 = slot != null ? slot.LiveO32 : 0;
            string copy = _loadE32CopySeen
                ? " e32_unit_copy v0=0x" + _loadE32CopyV0.ToString("X8") +
                    " dest=0x" + _loadE32CopyA0.ToString("X8") +
                    " src=0x" + _loadE32CopyA1.ToString("X8") +
                    " a2=0x" + _loadE32CopyA2.ToString("X8")
                : " e32_unit_copy missed";
            string first = " first-cmp " +
                FormatLoadE32Cmp(_loadE32CmpFirstPc, _loadE32CmpFirstOp,
                    _loadE32CmpFirstLhs, _loadE32CmpFirstRhs);
            string lastAfter = !string.IsNullOrEmpty(_loadE32CmpAfterOp)
                ? " last-cmp " +
                    FormatLoadE32Cmp(_loadE32CmpAfterPc, _loadE32CmpAfterOp,
                        _loadE32CmpAfterLhs, _loadE32CmpAfterRhs)
                : " last-cmp " +
                    FormatLoadE32Cmp(_loadE32CmpPc, _loadE32CmpOp,
                        _loadE32CmpLhs, _loadE32CmpRhs);
            string leftover = _loadE32ChkSeen
                ? " CurMSec leftover a1=0x" + _loadE32ChkA1.ToString("X8") +
                    " tick-v0=0x" + _loadE32ChkV0.ToString("X8") +
                    " (not o32 ABI; not the fail)"
                : " CurMSec not observed";
            string dumpO32 = FormatDumpO32(slot);
            string nk = !string.IsNullOrEmpty(_nkLoadE32Ok)
                ? " NK-ok " + _nkLoadE32Ok
                : " NK-ok pending fsdmgr/coredll/ceddk (NK e32 not in-repo)";
            return lastAfter +
                first + copy + leftover +
                " liveE32=0x" + live.ToString("X8") +
                " LiveO32=0x" + liveO32.ToString("X8") +
                " " + dumpO32 +
                " (dump e32 then dump o32 after; do not invent +0x5C;" +
                nk + "; memcpy is 0x80058B24; do not force v0=1)";
        }

        private static void NoteLoadE32RetPc(uint[] regs, uint pc)
        {
            if (pc != LoadE32Ok && pc != LoadE32Fail47E
                && pc != LoadE32FailBadExe && pc != LoadE32Epilogue)
                return;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            if (pc == LoadE32Ok || pc == LoadE32Fail47E || pc == LoadE32FailBadExe)
            {
                if (_loadE32Watch)
                    _loadE32RetPc = pc;
                if (_nkLoadE32Watch)
                    _nkRetPc = pc;
            }
            else if (pc == LoadE32Epilogue)
            {
                if (_loadE32Watch && _loadE32RetPc == 0)
                    _loadE32RetPc = pc;
                if (_nkLoadE32Watch && _nkRetPc == 0)
                    _nkRetPc = pc;
            }
            if (_loadE32Watch)
                _loadE32RetV0 = v0;
            if (!_loadE32Watch || _loadE32RetLogged)
                return;
            _loadE32RetLogged = true;
        }

        private static string FormatLoadE32OkDest(MipsBus bus)
        {
            uint word0 = PeekDestWord(bus, _loadE32OkDest0);
            uint wordDump = _loadE32OkDest != 0 && _loadE32OkDest != _loadE32OkDest0
                ? PeekDestWord(bus, _loadE32OkDest) : word0;
            return " dest0=0x" + _loadE32OkDest0.ToString("X8") +
                " dest-word=0x" + word0.ToString("X8") +
                " destDump=0x" + _loadE32OkDest.ToString("X8") +
                " dump-word=0x" + wordDump.ToString("X8");
        }

        private static uint DumpTocWord0(ExtraRomTocMod slot)
        {
            if (slot == null)
                return 0;
            if (slot.TocWords != null && slot.TocWords.Length > 0)
                return slot.TocWords[0];
            return slot.Attr;
        }

        private static string FormatDumpLiveEntry0(uint dumpToc0, uint live0)
        {
            bool dumpReal = dumpToc0 != 0 && live0 == dumpToc0;
            return " dumpToc0=0x" + dumpToc0.ToString("X8") +
                " LiveEntry0=0x" + live0.ToString("X8") +
                (dumpReal ? " dump-real" : " LiveEntry0!=dumpToc0") +
                " dumpToc0&0x200=" + (dumpToc0 & LoadO32VallocBit).ToString("X") +
                " LiveEntry0&0x200=" + (live0 & LoadO32VallocBit).ToString("X");
        }

        // Extract: ExtraROM BuiltIn 0x807 and ddi_nop 0x807
        // both lack 0x200. ddi_nop dest was OpenFile/LoadDriver
        // MapO32/CEDecompressROM (object+6>=2), not the
        // LoadO32 thunk. BuiltIn LoadLibrary hits LoadE32
        // success then LoadO32 skip, so firmware never
        // VirtualCopys ExtraROM BuiltIn o32. Serve dest only
        // on the path firmware actually takes. Do not set
        // 0x200. Do not copy NK 0x1007. Do not invent dest.
        private static string NameBuiltInMiss()
        {
            return "dest-word 0; serve dest only if firmware wrote it";
        }

        // Dump-real: linker 0x8001728C has ONE caller,
        // 0x80014420 (early kernel, before mtc0 Status at
        // 0x8001442C). One-shot. nk.exe .text only
        // 0x800172B8 addiu/lw of 0x29c8, no sw; ExtraROM
        // extracted PEs: zero lui 0x8034 + imm 0x29c8.
        // If *0x803429C8 is 0 at 0x80014420, firmware never
        // links 0x8134DA84. Do not invent a ROMChain_t
        // before that jal. Do not host-write 0x803429C8.
        // Host attach is a workaround because ExtraROM is
        // unlinked. 86e51ea linker enter/sw logs stay.
        private static string NameChainMiss()
        {
            return "host attach; ExtraROM unlinked; dest only if firmware wrote it";
        }

        public static void LogExtraRomHdrAtMap(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint romhdr)
        {
            if (memory == null)
                return;
            if (romhdr != 0)
                _extraRomHdr = romhdr;
            uint hdr = romhdr != 0 ? romhdr : ExtraRomDumpHdr;
            uint copy = 0;
            uint ext = 0;
            uint physfirst = 0;
            uint physlast = 0;
            uint nmods = 0;
            bool dumpHdr = false;
            try
            {
                copy = memory.ReadMemory32(hdr + RomHdrCopyEntries);
                ext = memory.ReadMemory32(hdr + RomHdrExtensions);
                physfirst = memory.ReadMemory32(hdr + 8);
                physlast = memory.ReadMemory32(hdr + 0xC);
                nmods = memory.ReadMemory32(hdr + RomHdrNumMods);
                dumpHdr = true;
            }
            catch
            {
            }
            if (!dumpHdr && hdr != ExtraRomDumpHdr)
            {
                try
                {
                    hdr = ExtraRomDumpHdr;
                    copy = memory.ReadMemory32(hdr + RomHdrCopyEntries);
                    ext = memory.ReadMemory32(hdr + RomHdrExtensions);
                    physfirst = memory.ReadMemory32(hdr + 8);
                    physlast = memory.ReadMemory32(hdr + 0xC);
                    nmods = memory.ReadMemory32(hdr + RomHdrNumMods);
                    dumpHdr = true;
                }
                catch
                {
                }
            }
            if (!_romHdrChainLogged)
            {
                _romHdrChainLogged = true;
                uint head = 0;
                try { head = memory.ReadMemory32(RomHdrListPtr); }
                catch { }
                BootLog.Write("[Hive] ROMHDR at-map ExtraROM-hdr=0x" + hdr.ToString("X8") +
                    " *0x80342B10=0x" + head.ToString("X") +
                    " nmods=" + nmods);
            }
        }

        private static string FormatRomHdrListFromMemory(ProcessorEmulator.Core.Emulation.IMemoryManager memory, uint extraHdr)
        {
            if (memory == null)
                return "list-walk skipped";
            try
            {
                uint head = memory.ReadMemory32(RomHdrListPtr);
                return FormatRomHdrListWalk(va => memory.ReadMemory32(va), head, extraHdr);
            }
            catch
            {
                return "*(0x80342B10) unmapped (NK list not readable at ExtraROM map)";
            }
        }

        public static void TryLogRomHdrListWalk(MipsBus bus, string when)
        {
            if (bus == null)
                return;
            uint extraHdr = _extraRomHdr != 0 ? _extraRomHdr : ExtraRomDumpHdr;
            uint head = PeekDestWord(bus, RomHdrListPtr);
            if (_romHdrListWalkLogged && when != null && when.IndexOf("host-attach", System.StringComparison.Ordinal) < 0)
                return;
            _romHdrListWalkLogged = true;
            BootLog.Write("[Hive] ROMHDR list ExtraROM-hdr=0x" + extraHdr.ToString("X8") +
                " *0x80342B10=0x" + head.ToString("X"));
        }

        // Observe 0x80014420 jal of 0x8001728C only. Peek
        // *0x803429C8 and ExtraROM hdr mapped. Cite dump:
        // one caller, no sw of 0x803429C8 in nk or ExtraROM
        // PEs. Do not invent a ROMChain_t before that jal.
        // Do not host-write 0x803429C8. 86e51ea enter/sw stay.
        private static void TryLogRomHdrLinkJal(MipsBus bus, uint[] regs)
        {
            if (bus == null || _romHdrLinkJalLogged)
                return;
            _romHdrLinkJalLogged = true;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            uint a3 = regs != null && regs.Length > 7 ? regs[7] : 0;
            uint srcHead;
            TryRead32(va => bus.Read32(va), RomHdrSrcChain, out srcHead);
            uint word = 0;
            bool hdrMapped = TryRead32(va => bus.Read32(va), ExtraRomDumpHdr, out word);
            BootLog.Write("[Hive] ROMHDR jal 0x80014420 *0x803429C8=0x" + srcHead.ToString("X") +
                " ExtraROM-hdr=" + (hdrMapped ? "mapped" : "unmapped") +
                " a0=0x" + a0.ToString("X") +
                " a1=0x" + a1.ToString("X") +
                " a2=0x" + a2.ToString("X") +
                " a3=0x" + a3.ToString("X"));
        }

        private static string FormatExtraRomHdrMapped(MipsBus bus)
        {
            uint word = 0;
            bool mapped = bus != null
                && TryRead32(va => bus.Read32(va), ExtraRomDumpHdr, out word);
            string host = _extraRomHdr != 0
                ? " host-NkBinLoader-hdr=0x" + _extraRomHdr.ToString("X8")
                : " host-NkBinLoader-hdr-not-yet";
            if (!mapped)
                return "ExtraROM-hdr-0x8134DA84-unmapped" + host;
            return "ExtraROM-hdr-0x8134DA84-mapped word0=0x" + word.ToString("X8") + host;
        }

        // Observe 0x8001728C only. Peek *0x803429C8 and walk
        // node+4 vs ExtraROM 0x8134DA84 / NK 0x802808B4.
        // Do not host-write 0x803429C8 or 0x80342B10.
        private static void TryLogRomHdrLinkEnter(MipsBus bus, uint[] regs)
        {
            if (bus == null || _romHdrLinkEnterCount >= RomHdrLinkLogMax)
                return;
            _romHdrLinkEnterCount++;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
            uint a3 = regs != null && regs.Length > 7 ? regs[7] : 0;
            uint srcHead = PeekDestWord(bus, RomHdrSrcChain);
            uint listHead = PeekDestWord(bus, RomHdrListPtr);
            BootLog.Write("[Hive] ROMHDR enter 0x8001728C *0x803429C8=0x" + srcHead.ToString("X") +
                " *0x80342B10=0x" + listHead.ToString("X") +
                " a1=0x" + a1.ToString("X") +
                " a2=0x" + a2.ToString("X") +
                " a3=0x" + a3.ToString("X"));
        }

        private static void TryLogRomHdrLinkSw(MipsBus bus, uint[] regs, string which)
        {
            if (bus == null || string.IsNullOrEmpty(which))
                return;
            bool publish = which.IndexOf("0x80017308", System.StringComparison.Ordinal) >= 0;
            if (publish)
            {
                if (_romHdrLinkPublishCount >= RomHdrLinkLogMax)
                    return;
                _romHdrLinkPublishCount++;
            }
            else
            {
                if (_romHdrLinkSpliceCount >= RomHdrLinkLogMax)
                    return;
                _romHdrLinkSpliceCount++;
            }
            uint a3 = regs != null && regs.Length > 7 ? regs[7] : 0;
            uint srcHead = PeekDestWord(bus, RomHdrSrcChain);
            uint oldHead = PeekDestWord(bus, RomHdrListPtr);
            BootLog.Write("[Hive] ROMHDR sw " + which +
                " *0x803429C8=0x" + srcHead.ToString("X") +
                " *0x80342B10=0x" + oldHead.ToString("X") +
                " a3=0x" + a3.ToString("X"));
        }

        // Walk *0x803429C8. Each node+4 vs ExtraROM 0x8134DA84
        // and NK 0x802808B4 / live *0x8001101C. Peek only.
        private static string FormatSrcChainWalk(System.Func<uint, uint> read32)
        {
            uint src;
            if (!TryRead32(read32, RomHdrSrcChain, out src))
                return "live-*(0x803429C8)-unmapped";
            uint nkWord;
            bool gotNk = TryRead32(read32, NkRomHdrPtr, out nkWord);
            string nkCite = " *0x8001101C="
                + (gotNk ? "0x" + nkWord.ToString("X8") : "unmapped")
                + (gotNk && nkWord == NkDumpHdr
                    ? " dump-NK-romhdr-0x802808B4"
                    : gotNk && nkWord != 0
                        ? " !=dump-0x802808B4"
                        : "");
            if (src == 0)
                return "live-*(0x803429C8)=0 OEM-never-published-ExtraROM-onto-source-chain ExtraROM-hdr-unlinked"
                    + nkCite;
            var sb = new System.Text.StringBuilder();
            sb.Append("live-*(0x803429C8)=0x").Append(src.ToString("X8")).Append(nkCite);
            bool extra = false;
            bool matchNkWord = false;
            uint node = src;
            for (int i = 0; i < 16 && node != 0; i++)
            {
                uint next;
                uint hdr;
                if (!TryRead32(read32, node, out next) || !TryRead32(read32, node + 4, out hdr))
                {
                    sb.Append(" [").Append(i).Append("] node=0x").Append(node.ToString("X8"))
                        .Append(" unmapped");
                    break;
                }
                string vs = hdr == ExtraRomDumpHdr
                    ? " ExtraROM-hdr-0x8134DA84"
                    : hdr == NkDumpHdr
                        ? " NK-hdr-0x802808B4"
                        : " !=ExtraROM/NK";
                if (gotNk && hdr != 0 && hdr == nkWord)
                {
                    vs += " ==*0x8001101C";
                    matchNkWord = true;
                }
                sb.Append(" [").Append(i).Append("] node=0x").Append(node.ToString("X8"))
                    .Append(" node+4=0x").Append(hdr.ToString("X8")).Append(vs);
                if (hdr == ExtraRomDumpHdr)
                    extra = true;
                if (next == 0 || next == node)
                    break;
                node = next;
            }
            sb.Append(extra ? " ExtraROM-on-source-chain" : " ExtraROM-not-on-source-chain");
            sb.Append(matchNkWord ? " would-0x80017308-publish" : " source-walk-miss");
            sb.Append(" (do not invent a ROMChain_t; do not host-write 0x803429C8 or 0x80342B10)");
            return sb.ToString();
        }

        // Dump-named NK copy[0] vs live *(0x80342B10) and
        // source chain *(0x803429C8). List head is dst+0x22B10
        // (BSS tail past copy_len) until 0x8001728C sw. Peek
        // only; do not host-write. Do not invent a ROMChain_t.
        private static string FormatNkCopyVsList(System.Func<uint, uint> read32)
        {
            string nkCopy = "NK-copy[0]-dump src=0x"
                + NkCopy0Src.ToString("X8")
                + " dst=0x"
                + NkCopy0Dst.ToString("X8")
                + " copy_len=0x"
                + NkCopy0CopyLen.ToString("X")
                + " dest_len=0x"
                + NkCopy0DestLen.ToString("X")
                + " list=dst+0x"
                + RomHdrListBssOff.ToString("X");
            uint nkHdr;
            if (!TryRead32(read32, EcecTocPtr, out nkHdr))
            {
                nkCopy += " live-NK-hdr-unmapped";
            }
            else if (nkHdr == 0)
            {
                nkCopy += " live-NK-hdr=0";
            }
            else
            {
                uint nkEntries;
                uint nkCopyOff;
                bool gotEntries = TryRead32(read32, nkHdr + RomHdrCopyEntries, out nkEntries);
                bool gotOff = TryRead32(read32, nkHdr + RomHdrCopyOffset, out nkCopyOff);
                nkCopy += " live-NK-hdr=0x" + nkHdr.ToString("X8");
                if (gotEntries)
                    nkCopy += " ulCopyEntries=0x" + nkEntries.ToString("X");
                if (gotOff)
                    nkCopy += " ulCopyOffset=0x" + nkCopyOff.ToString("X8");
                if (gotEntries && gotOff && nkEntries != 0 && nkCopyOff != 0)
                {
                    uint liveSrc;
                    uint liveDst;
                    uint liveCopyLen;
                    uint liveDestLen;
                    if (TryRead32(read32, nkCopyOff, out liveSrc)
                        && TryRead32(read32, nkCopyOff + 4, out liveDst)
                        && TryRead32(read32, nkCopyOff + 8, out liveCopyLen)
                        && TryRead32(read32, nkCopyOff + 12, out liveDestLen))
                    {
                        nkCopy += " live-copy[0] src=0x"
                            + liveSrc.ToString("X8")
                            + " dst=0x"
                            + liveDst.ToString("X8")
                            + " copy_len=0x"
                            + liveCopyLen.ToString("X")
                            + " dest_len=0x"
                            + liveDestLen.ToString("X");
                    }
                }
            }

            uint extraCopy;
            string extraTable = TryRead32(read32, ExtraRomDumpHdr + RomHdrCopyEntries, out extraCopy)
                ? (extraCopy == 0
                    ? " ExtraROM-copy_table-empty"
                    : " ExtraROM-ulCopyEntries=0x" + extraCopy.ToString("X"))
                : " ExtraROM-copy_table-unmapped";

            uint[] pExt = new uint[8];
            bool pExtMapped = true;
            bool pExtZeros = true;
            for (int i = 0; i < 8; i++)
            {
                if (!TryRead32(read32, NkPExtensions + (uint)(i * 4), out pExt[i]))
                {
                    pExtMapped = false;
                    break;
                }
                if (pExt[i] != 0)
                    pExtZeros = false;
            }
            string pExtLive;
            if (!pExtMapped)
                pExtLive = "pExtensions-0x80011020-dump-.text-32-zeros pExtensions-0x80011020-unmapped";
            else if (pExtZeros)
                pExtLive = "pExtensions-0x80011020-dump-.text-32-zeros pExtensions-0x80011020-live-32-zeros";
            else
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("pExtensions-0x80011020-dump-.text-32-zeros pExtensions-0x80011020-live=");
                for (int i = 0; i < 8; i++)
                {
                    if (i != 0)
                        sb.Append(',');
                    sb.Append("0x").Append(pExt[i].ToString("X8"));
                }
                pExtLive = sb.ToString();
            }

            uint listWord;
            string list;
            if (!TryRead32(read32, RomHdrListPtr, out listWord))
            {
                list = "live-*(0x80342B10)-unmapped";
            }
            else if (listWord == 0)
            {
                list = "live-*(0x80342B10)=0 empty-after-NK-copy-BSS-tail-until-0x8001728C ExtraROM-no-dump-node";
            }
            else
            {
                uint nodeHdr;
                list = "live-*(0x80342B10)=0x" + listWord.ToString("X8");
                if (TryRead32(read32, listWord + 4, out nodeHdr))
                    list += " node+4=0x" + nodeHdr.ToString("X8");
                else
                    list += " node+4-unmapped";
            }

            return nkCopy + extraTable + " " + pExtLive + " " + FormatSrcChainWalk(read32) + " " + list;
        }

        private static bool TryRead32(System.Func<uint, uint> read32, uint va, out uint value)
        {
            value = 0;
            if (read32 == null)
                return false;
            try
            {
                value = read32(va);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string FormatRomHdrListWalk(System.Func<uint, uint> read32, uint head, uint extraHdr)
        {
            if (read32 == null)
                return "list-walk skipped";
            if (head == 0)
                return "*(0x80342B10)=0 BSS-tail-past-copy_len ExtraROM-no-dump-node; do not invent a ROMChain_t";
            var sb = new System.Text.StringBuilder();
            sb.Append("*(0x80342B10)=0x").Append(head.ToString("X8"));
            bool linked = false;
            uint node = head;
            for (int i = 0; i < 16 && node != 0; i++)
            {
                uint next = 0;
                uint hdr = 0;
                try
                {
                    next = read32(node);
                    hdr = read32(node + 4);
                }
                catch
                {
                    sb.Append(" [").Append(i).Append("] node=0x").Append(node.ToString("X8"))
                        .Append(" unmapped");
                    break;
                }
                sb.Append(" [").Append(i).Append("] node=0x").Append(node.ToString("X8"))
                    .Append(" hdr=0x").Append(hdr.ToString("X8"));
                if (hdr != 0 && (hdr == extraHdr || hdr == ExtraRomDumpHdr))
                    linked = true;
                if (next == 0 || next == node)
                    break;
                node = next;
            }
            sb.Append(linked
                ? " ExtraROM-hdr-on-list"
                : " ExtraROM-no-dump-node");
            sb.Append(" (do not invent a ROMChain_t)");
            return sb.ToString();
        }

        private static string NameLoadO32Path(uint dumpToc0, uint live0, bool destFilled)
        {
            bool has200 = ((live0 != 0 ? live0 : dumpToc0) & LoadO32VallocBit) != 0;
            if (destFilled)
                return "dest filled; serve dest only if firmware wrote it";
            if (!has200)
                return NameBuiltInMiss();
            return "dest-word 0; serve dest only if firmware wrote it";
        }

        private static uint SlotLoadVa(ExtraRomTocMod slot)
        {
            if (slot != null && slot.LoadVa != 0)
                return slot.LoadVa;
            if (slot != null && slot.TocWords != null && slot.TocWords.Length > 7
                && slot.TocWords[7] != 0)
                return slot.TocWords[7];
            return ExtractLoadVa(slot != null ? slot.Name : null);
        }

        private static uint ExtractLoadVa(string name)
        {
            if (NamesMatchRom(name, "bcmuart.dll"))
                return BcmuartLoadVa;
            if (NamesMatchRom(name, "ddi_nop.dll"))
                return DdiNopLoadVa;
            return 0;
        }

        private static string FormatLoadVaPhys(string name, uint loadVa)
        {
            uint va = loadVa != 0 ? loadVa : ExtractLoadVa(name);
            return va == 0 ? "" : " load_va=0x" + va.ToString("X8");
        }

        private static uint PeekObj6(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0)
                return 0;
            try
            {
                return (uint)(bus.Read8(obj + 6) | (bus.Read8(obj + 7) << 8));
            }
            catch
            {
                return 0;
            }
        }

        private static void TryLogNkRangeDecompile(MipsBus bus, uint va, string name, uint words, string why)
        {
        }

        private static string FormatLoadO32Fp(MipsBus bus, uint obj)
        {
            uint toc = PeekDestWord(bus, obj);
            uint fp = toc != 0 ? PeekDestWord(bus, toc) : 0;
            uint live0 = _loadE32OkLiveEntry != 0
                ? PeekDestWord(bus, _loadE32OkLiveEntry) : 0;
            uint e32live0 = _loadE32OkLiveE32 != 0
                ? PeekDestWord(bus, _loadE32OkLiveE32) : 0;
            uint dumpToc0 = _loadE32OkDumpToc0;
            _loadE32OkFp = fp;
            _loadE32OkBit200 = (fp & LoadO32VallocBit) != 0;
            bool alias = toc != 0 && toc == _loadE32OkLiveE32;
            string aliasName = alias
                ? " fp-aliases-e32"
                : " fp=LiveEntry-first-word not e32 live0";
            return " *obj=0x" + toc.ToString("X8") +
                " fp=**(obj)=0x" + fp.ToString("X8") +
                " LiveEntry=0x" + _loadE32OkLiveEntry.ToString("X8") +
                FormatDumpLiveEntry0(dumpToc0, live0) +
                " LiveE32=0x" + _loadE32OkLiveE32.ToString("X8") +
                " e32-live0=0x" + e32live0.ToString("X8") +
                " fp&0x200=" + (fp & LoadO32VallocBit).ToString("X") +
                " fp&0x400=" + (fp & LoadO32LockBit).ToString("X") +
                " e32&0x200=" + (e32live0 & LoadO32VallocBit).ToString("X") +
                " e32&0x400=" + (e32live0 & LoadO32LockBit).ToString("X") +
                aliasName;
        }

        // destDump is o32.real (nleddrvr 0x02F81000 / mscoree
        // 0x034B1000, CE slot 1). dest0 is destDump&0x01FFFFFF.
        // Live bfa911a: both words 0 after MapO32. jal
        // 0x800283FC a0=destDump a2=0x1000 (MEM_COMMIT).
        // Current process has no reservation on that slot-1
        // VA (same last-error 14 as ddi_nop slot-1 0x03981000).
        // MapO32 memcpy/decomp never run. Serve destDump only
        // if firmware wrote dump-word. Do not invent dest.
        private static void HiveWatch(MipsBus bus, string ev, uint v0)
        {
            uint destDump = _loadE32OkDest;
            uint dest0 = _loadE32OkDest0;
            uint wordDump = PeekDestWord(bus, destDump);
            uint word0 = PeekDestWord(bus, dest0);
            uint slot = destDump >> 25;
            string miss = "";
            if (_loadE32OkMapValloc && _loadE32OkMapVallocV0 == 0 && wordDump == 0 && word0 == 0)
                miss = " slot-" + slot + " destDump COMMIT no reserve last-error 14";
            else if (v0 == 0xE && ev != null && ev.IndexOf("wrapper", System.StringComparison.Ordinal) >= 0
                && word0 == 0)
                miss = " MapO32 v0=0xE after slot-" + slot + " COMMIT no reserve; LoadO32 was 0";
            else if ((_loadE32OkMap28844 || _loadE32OkMapO32) && wordDump == 0 && word0 == 0)
                miss = " destDump-word=0 dest0-word=0; slot-" + slot + " COMMIT miss";
            else if (wordDump == 0 && word0 != 0)
                miss = " dest0-word set; firmware dest is dest0";
            BootLog.Write("[Hive] TOC[" + _loadE32OkIndex + "] " + _loadE32OkName
                + " " + ev
                + " v0=0x" + v0.ToString("X")
                + " destDump=0x" + destDump.ToString("X8")
                + " dump-word=0x" + wordDump.ToString("X")
                + " dest0=0x" + dest0.ToString("X8")
                + " dest0-word=0x" + word0.ToString("X")
                + " object+6=" + _loadE32OkObj6
                + " 0x80028844=" + _loadE32OkMap28844
                + miss);
        }

        private static void NoteAfterLoadE32Ok(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_loadE32OkWatch)
                return;
            _loadE32OkSteps++;
            bool ddiOk = NamesMatchRom(_loadE32OkName, "ddi_nop.dll") || _ddiNopFileObj != 0;
            if (ddiOk && (pc == BinaryDecompressRom || (_loadE32OkSteps & 0xFFF) == 0))
                TryHuntDdiNopModuleFromRegs(bus, regs);
            if (pc == LoadLibSyscallRet || _loadE32OkSteps > 200000)
            {
                // Live e29762a: 200k cap cleared the watch
                // during CEDecompressROM, so startip saw
                // obj=0. Keep ddi_nop until one startip
                // attempt after the .text sig serve.
                if (ddiOk && pc != LoadLibSyscallRet
                    && (!_ddiNopLandedBySig || !_ddiNopStartipAttempted))
                    return;
                if (!_loadE32OkLoadO32)
                    HiveWatch(bus, "LoadO32-not-entered", 0);
                else if (!_loadE32OkMapInner && !_loadE32OkMap28844 && !_loadE32OkMapO32 && !_loadE32OkDecomp)
                    HiveWatch(bus, "LoadO32-skip-no-MapO32", 0);
                PersistSkipCompare(bus);
                ClearLoadE32OkWatch();
                return;
            }
            if (pc == LoadE32WrapFail && !_loadE32OkWrapFail)
            {
                _loadE32OkWrapFail = true;
                _loadE32OkWrapPc = pc;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                HiveWatch(bus, "wrapper-0x8001E538", v0);
                return;
            }
            if (pc == LoadO32Rom && !_loadE32OkLoadO32)
            {
                uint a0chk = regs != null && regs.Length > 4 ? regs[4] : 0;
                if (_loadE32OkObj != 0 && a0chk != 0 && a0chk != _loadE32OkObj)
                    return;
                _loadE32OkLoadO32 = true;
                uint a0 = a0chk;
                uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
                uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
                uint a3 = regs != null && regs.Length > 7 ? regs[7] : 0;
                uint type = 0;
                try
                {
                    if (bus != null && a0 != 0)
                        type = bus.Read8(a0 + 4);
                }
                catch
                {
                }
                uint obj = a0 != 0 ? a0 : _loadE32OkObj;
                _loadE32OkObj6 = PeekObj6(bus, obj);
                HiveWatch(bus, "LoadO32", 0);
                return;
            }
            if (pc == LoadO32PredFail && !_loadE32OkPredFail)
            {
                _loadE32OkPredFail = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                HiveWatch(bus, "LoadO32-pred-fail", v0);
                return;
            }
            if (pc == LoadO32SkipValloc && _loadE32OkLoadO32 && !_loadE32OkSkip200)
            {
                _loadE32OkSkip200 = true;
                MarkBuiltInSkip();
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                uint sp20 = PeekSpWord(bus, regs, 0x20);
                if (!_skipDisasmLogged)
                {
                    _skipDisasmLogged = true;
                    TryLogNkRangeDecompile(bus, LoadO32SkipValloc, "LoadO32-skip 0x80016830", 8,
                        "dump nk.exe: lw v0,0x20(sp); beqz 0x80016848; not MapO32; dest out only if thunk filled 0x20(sp); observe only; do not set 0x200");
                }
                HiveWatch(bus, "LoadO32-skip200", 0);
                return;
            }
            if (pc == LoadO32OkRet && _loadE32OkLoadO32 && !_loadE32OkLoadO32Ret)
            {
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint word0 = PeekDestWord(bus, _loadE32OkDest0);
                _loadE32OkDestAfter = word0;
                HiveWatch(bus, "LoadO32-ok", v0);
            }
            if (regs != null && _loadE32OkPredRa != 0 && pc == _loadE32OkPredRa)
            {
                _loadE32OkPredV0 = regs.Length > 2 ? regs[2] : 0;
                _loadE32OkPredRa = 0;
                HiveWatch(bus, "LoadO32-pred", _loadE32OkPredV0);
                return;
            }
            if (regs != null && _loadE32OkVallocRa != 0 && pc == _loadE32OkVallocRa)
            {
                _loadE32OkVallocV0 = regs.Length > 2 ? regs[2] : 0;
                _loadE32OkVallocRa = 0;
                HiveWatch(bus, "LoadO32-thunk", _loadE32OkVallocV0);
                return;
            }
            if (pc == LoadO32RomRet && _loadE32OkLoadO32 && !_loadE32OkLoadO32Ret)
            {
                _loadE32OkLoadO32Ret = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint word0 = PeekDestWord(bus, _loadE32OkDest0);
                uint live0 = _loadE32OkLiveEntry != 0
                    ? PeekDestWord(bus, _loadE32OkLiveEntry) : _loadE32OkFp;
                string destWhy = word0 != 0
                    ? NameLoadO32Path(_loadE32OkDumpToc0, live0, true)
                    : (_loadE32OkValloc
                        ? "dest word 0 after kmode thunk 0x8003E660 v0=0x" + _loadE32OkVallocV0.ToString("X8")
                        : NameLoadO32Path(_loadE32OkDumpToc0, live0, false));
                HiveWatch(bus, "LoadO32-ret", v0);
                return;
            }
            if (pc == LoadO32WrapAfter && _loadE32OkLoadO32 && !_loadE32OkWrapAfter)
            {
                _loadE32OkWrapAfter = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint word0 = PeekDestWord(bus, _loadE32OkDest0);
                if (_loadE32OkDestAfter == 0)
                    _loadE32OkDestAfter = word0;
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                if (!_wrapAfterDisasmLogged)
                {
                    _wrapAfterDisasmLogged = true;
                    TryLogNkRangeDecompile(bus, LoadO32WrapAfter, "LoadO32-wrap-after 0x8001E428", 16,
                        "dump nk.exe: andi s5,2 then jal 0x800283fc VirtualAlloc-like not CEDecompressROM; 0x8001E45c andi s5,0x8000 then jal 0x8001AF20 NOT MapO32; 0x8001AC9c/0x80028844 not on skip path; observe only; do not jal; do not invent dest; do not invent 0x2000");
                }
                HiveWatch(bus, "wrap-after", v0);
                return;
            }
            if (pc == LoadO32WrapValloc && _loadE32OkLoadO32 && !_loadE32OkWrapValloc)
            {
                _loadE32OkWrapValloc = true;
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
                uint a2 = regs != null && regs.Length > 6 ? regs[6] : 0;
                HiveWatch(bus, "wrap-valloc", 0);
                return;
            }
            if (pc == LoadO32WrapS5Hi && _loadE32OkLoadO32 && !_loadE32OkS5Hi)
            {
                _loadE32OkS5Hi = true;
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                HiveWatch(bus, "wrap-s5", 0);
                return;
            }
            if (pc == LoadO32WrapO32Walk && _loadE32OkLoadO32 && !_loadE32OkO32Walk)
            {
                _loadE32OkO32Walk = true;
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                HiveWatch(bus, "wrap-o32walk", 0);
                return;
            }
            if (pc == LoadO32WrapFlagsChk && _loadE32OkLoadO32 && !_loadE32OkFlagsChk)
            {
                _loadE32OkFlagsChk = true;
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                HiveWatch(bus, "wrap-flags", 0);
                return;
            }
            if (pc == LoadO32WrapC1 && _loadE32OkLoadO32 && !_loadE32OkC1)
            {
                _loadE32OkC1 = true;
                _loadE32OkS5 = PeekS5(regs);
                _loadE32OkSp24 = PeekSpWord(bus, regs, 0x24);
                HiveWatch(bus, "wrap-C1", 0);
                return;
            }
            if (pc == CopyO32Rom && !_loadE32OkCopyO32 && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkCopyO32 = true;
                HiveWatch(bus, "CopyO32", 0);
                return;
            }
            if (pc == MapO32Rom && _loadE32OkLoadO32 && !_loadE32OkMapO32
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkMapO32 = true;
                MarkFwMapO32();
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                HiveWatch(bus, "MapO32-0x8001AC30", 0);
                return;
            }
            if (pc == MapO32InnerJal && _loadE32OkLoadO32 && !_loadE32OkMapInner
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkMapInner = true;
                MarkFwMapO32();
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                HiveWatch(bus, "MapO32-inner", 0);
                return;
            }
            if (pc == MapO32Decompress && _loadE32OkLoadO32 && !_loadE32OkMap28844
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkMap28844 = true;
                MarkFwMapO32();
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                HiveWatch(bus, "0x80028844", 0);
                return;
            }
            if (_loadE32OkMapO32 && !_loadE32OkMapValloc
                && (pc == MapO32VallocJal
                    || (pc == LoadO32WrapValloc
                        && regs != null && regs.Length > 4
                        && (regs[4] == _loadE32OkDest || regs[4] == _loadE32OkDest0))))
            {
                _loadE32OkMapValloc = true;
                _loadE32OkMapVallocA0 = regs != null && regs.Length > 4 ? regs[4] : 0;
                _loadE32OkMapVallocA2 = regs != null && regs.Length > 6 ? regs[6] : 0;
                _loadE32OkMapVallocA3 = regs != null && regs.Length > 7 ? regs[7] : 0;
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                uint a0log = _loadE32OkMapVallocA0;
                if ((a0log & 0xF0000000u) == 0x60000000u && _loadE32OkDest != 0)
                    a0log = _loadE32OkDest;
                HiveWatch(bus, "0x800283FC a0=0x" + a0log.ToString("X8")
                    + ((_loadE32OkMapVallocA0 & 0xF0000000u) == 0x60000000u
                        ? " (flags; destDump)" : "")
                    + " a2=0x" + _loadE32OkMapVallocA2.ToString("X")
                    + " a3=0x" + _loadE32OkMapVallocA3.ToString("X"), 0);
                return;
            }
            if (pc == MapO32VallocRet && _loadE32OkMapValloc && _loadE32OkMapVallocV0 == 0xFFFFFFFFu)
            {
                _loadE32OkMapVallocV0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                _loadE32OkObj6 = PeekObj6(bus, _loadE32OkObj);
                HiveWatch(bus, "0x8001AE08", _loadE32OkMapVallocV0);
                return;
            }
            if (pc == BindImpHdr && _loadE32OkLoadO32 && !_loadE32OkBindImp
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkBindImp = true;
                uint word0 = PeekDestWord(bus, _loadE32OkDest0);
                HiveWatch(bus, "BindImp", 0);
                return;
            }
            if (pc == CallDllStartip && _loadE32OkLoadO32 && !_loadE32OkCallDll
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkCallDll = true;
                uint word0 = PeekDestWord(bus, _loadE32OkDest0);
                HiveWatch(bus, "CallDLL", 0);
                return;
            }
            if (pc == BinaryDecompressRom && _loadE32OkLoadO32 && !_loadE32OkDecomp
                && WatchMatchesExtraRom(bus, regs, pc))
            {
                _loadE32OkDecomp = true;
                MarkFwMapO32();
                HiveWatch(bus, "CEDecompressROM", 0);
                if (NamesMatchRom(_loadE32OkName, "ddi_nop.dll") || _ddiNopFileObj != 0)
                    TryHuntDdiNopModuleFromRegs(bus, regs);
                return;
            }
            if (bus == null || regs == null)
                return;
            uint instr = 0;
            try
            {
                instr = bus.Read32(pc);
            }
            catch
            {
                return;
            }
            uint op = instr >> 26;
            if (_loadE32OkLoadO32 && !_loadE32OkBit200Seen && op == 0xC
                && (instr & 0xFFFF) == LoadO32VallocBit)
            {
                _loadE32OkBit200Seen = true;
                uint rs = (instr >> 21) & 31;
                uint lhs = regs.Length > (int)rs ? regs[(int)rs] : 0;
                _loadE32OkFp = lhs;
                _loadE32OkBit200 = (lhs & LoadO32VallocBit) != 0;
                HiveWatch(bus, _loadE32OkBit200 ? "andi-0x200-taken" : "andi-0x200-skip", 0);
            }
            uint target = 0;
            if (op == 3)
                target = (pc & 0xF0000000u) | ((instr & 0x3FFFFFFu) << 2);
            if (target == LoadO32Pred && !_loadE32OkPred)
            {
                _loadE32OkPred = true;
                _loadE32OkPredRa = pc + 8;
                uint a0 = regs.Length > 4 ? regs[4] : 0;
                HiveWatch(bus, "jal-pred", 0);
            }
            if (target == LoadO32VallocOpen && _loadE32OkLoadO32 && !_loadE32OkValloc)
            {
                _loadE32OkValloc = true;
                _loadE32OkVallocRa = pc + 8;
                uint a0 = regs.Length > 4 ? regs[4] : 0;
                HiveWatch(bus, "thunk-enter", 0);
            }
        }

        // Same 0x8004DBF8 path gwes uses for ddi_nop after
        // LoadE32=0. Dump o32 dest/vsize/psize/dataptr only.
        // Do not invent e32 bytes. ddi_nop/mscoree/ole32 keep
        // their existing VALLOC+VirtualCopy redirect.
        // CreateFileFail a0/a1/a2/a3 are not CEDecompressROM
        // (src,cb,dest,vsize). Booted 2c926c7: a0=0x86F46220
        // (heap object) a1=1 a2=0 a3=0xFFFFFFFE. Leave
        // firmware registers. Type-7 attach still CreateFileOk.
        // BuiltIn LoadDriver uses OpenFile/VALLOC/CopyO32 like
        // ddi_nop. Do not jal 0x8004DBF8 from this PC.
        public static bool TryStartExtraRomTocDecompress(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (regs == null || regs.Length <= 7)
                return false;
            ExtraRomTocMod slot = null;
            try
            {
                uint obj = regs.Length > 30 ? regs[30] : 0;
                if (obj != 0 && bus != null && bus.Read8(obj + 4) == TocAttachType)
                    slot = FindCachedTocByEntry(bus.Read32(obj));
            }
            catch
            {
            }
            if (slot == null && !string.IsNullOrEmpty(_pendingLoadE32Name))
                slot = FindCachedExtraRomToc(_pendingLoadE32Name);
            if (slot == null || string.IsNullOrEmpty(slot.Name) || slot.Index < 0)
                return false;
            if (NamesMatchRom(slot.Name, "ddi_nop.dll") || IsMscoreeDll(slot.Name)
                || IsOle32Dll(slot.Name))
                return false;
            BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                " CreateFileFail v0= dest-word= dest0=0 object+6=0 0x80028844=False");
            return false;
        }

        // Firmware sh s5,6(fp) at 0x8001D4F0 only when
        // CreateFileMapping 0x8003DA64 returns 0. BuiltIn
        // LoadLibrary never takes that jal. Do not host-write
        // object+6 to match LoadDriver. Observe only.
        public static void TryPrepareExtraRomBuiltInLikeDdiNop(MipsBus bus, uint obj)
        {
            if (bus == null || obj == 0)
                return;
            try
            {
                if (bus.Read8(obj + 4) != TocAttachType)
                    return;
                ExtraRomTocMod slot = FindCachedTocByEntry(bus.Read32(obj));
                // NK attach names hit CreateFileFail type-7 with
                // no ExtraROM o32. destDump=0 dataptr=0 is not
                // ExtraROM TOC[-1]. Do not log those as o32.
                if (slot == null || string.IsNullOrEmpty(slot.Name) || slot.Index < 0)
                    return;
                uint obj6 = (uint)(bus.Read8(obj + 6) | (bus.Read8(obj + 7) << 8));
                uint dest = slot != null ? slot.Dest : 0;
                uint slot0 = dest & SlotMask;
                BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                    " CreateFileFail v0= dest-word=0 destDump=0x" + dest.ToString("X8") +
                    " dest0=0x" + slot0.ToString("X8") +
                    " object+6=" + obj6 +
                    " 0x80028844=False");
            }
            catch
            {
            }
        }

        private static bool IsFirmwareDecompressDest(uint dest, uint real)
        {
            if (dest == 0)
                return false;
            if (dest >= ExtraRomTocDestHost && dest < ExtraRomTocDestHostLim)
                return false;
            if (dest >= ExtraRomTocSrc && dest < ExtraRomTocSrcLim)
                return false;
            if (real != 0 && (dest == real || dest == (real & SlotMask)))
                return true;
            if (dest >= 0x01400000u && dest < 0x02000000u)
                return true;
            return false;
        }

        // Dump o32 dataptr bytes stay at dump dataptr VA. Backing
        // is ExtraRomTocSrc pool. a0 stays dump dataptr.
        private static bool HostSrcExtraRomToc(MipsBus bus, ExtraRomTocMod slot,
            uint dataptr, uint psize)
        {
            if (bus == null || dataptr == 0 || psize == 0)
                return false;
            uint pages = (psize + 0x1FFFu) & ~0xFFFu;
            if (_tocSrcPool + pages > ExtraRomTocSrcLim)
                return false;
            uint kseg = _tocSrcPool;
            try
            {
                uint[] blob = slot.Data != null && slot.Data.Length > 0 ? slot.Data[0] : null;
                uint n = (psize + 3) / 4;
                for (uint w = 0; w < n; w++)
                {
                    uint word = blob != null && w < blob.Length ? blob[w] : 0;
                    bus.Write32(kseg + w * 4, word);
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[" + slot.Index + "] " +
                    slot.Name + " src-host fail " + ex.Message +
                    " (do not invent 0x81360000)");
                return false;
            }
            if (_tocSrcPtr == null)
            {
                _tocSrcPtr = new uint[32];
                _tocSrcLen = new uint[32];
                _tocSrcKseg = new uint[32];
            }
            if (_tocSrcN >= _tocSrcPtr.Length)
                return false;
            _tocSrcPtr[_tocSrcN] = dataptr;
            _tocSrcLen[_tocSrcN] = psize;
            _tocSrcKseg[_tocSrcN] = kseg;
            _tocSrcN++;
            _tocSrcPool += pages;
            return true;
        }

        // Map firmware dest (o32.real 0x02F21000 / VALLOC) onto
        // ExtraRomTocDestHost backing. a2 stays firmware dest.
        private static bool HostMapFirmwareTocDest(MipsBus bus, ExtraRomTocMod slot,
            uint fwDest, uint vsize)
        {
            if (bus == null || fwDest == 0 || vsize == 0)
                return false;
            uint slot0 = fwDest & SlotMask;
            uint pages = (vsize + 0x1FFFu) & ~0xFFFu;
            if (_tocDestHostPool + pages > ExtraRomTocDestHostLim)
                return false;
            uint kseg = _tocDestHostPool;
            try
            {
                for (uint i = 0; i < pages; i += 4)
                    bus.Write32(kseg + i, 0);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC dest-host fail " +
                    slot.Name + " " + ex.Message +
                    " (do not invent 0x81360000)");
                return false;
            }
            if (_tocDestSlot0 == null)
            {
                _tocDestSlot0 = new uint[32];
                _tocDestDump = new uint[32];
                _tocDestVsize = new uint[32];
                _tocDestKseg = new uint[32];
                _tocDestReady = new bool[32];
            }
            if (_tocDestN >= _tocDestSlot0.Length)
                return false;
            _tocDestSlot0[_tocDestN] = slot0;
            _tocDestDump[_tocDestN] = slot.Dest != 0 ? slot.Dest : fwDest;
            _tocDestVsize[_tocDestN] = pages;
            _tocDestKseg[_tocDestN] = kseg;
            _tocDestReady[_tocDestN] = true;
            _tocDestN++;
            _tocDestHostPool += pages;
            return true;
        }

        public static uint MapExtraRomE32HostVa(uint va)
        {
            if (va >= ExtraRomE32Host && va < ExtraRomE32HostLim)
                return va;
            return va;
        }

        public static uint MapExtraRomTocSrcVa(uint va)
        {
            for (int i = 0; i < _tocSrcN; i++)
            {
                uint ptr = _tocSrcPtr[i];
                uint len = _tocSrcLen[i];
                uint kseg = _tocSrcKseg[i];
                if (ptr == 0 || len == 0 || kseg == 0)
                    continue;
                if (va >= ptr && va < ptr + len)
                    return kseg + (va - ptr);
                uint slot0 = ptr & SlotMask;
                uint off = va & SlotMask;
                if ((va >> 25) == (ptr >> 25)
                    && off >= slot0 && off < slot0 + len)
                    return kseg + (off - slot0);
            }
            return va;
        }

        public static uint MapExtraRomTocDestVa(uint va)
        {
            for (int i = 0; i < _tocDestN; i++)
            {
                uint slot0 = _tocDestSlot0[i];
                uint dump = _tocDestDump[i];
                uint vsize = _tocDestVsize[i];
                uint kseg = _tocDestKseg[i];
                if (kseg == 0 || vsize == 0)
                    continue;
                if (va >= kseg && va < kseg + vsize)
                    return va;
                if (_tocDestReady == null || !_tocDestReady[i])
                    continue;
                uint off = va & SlotMask;
                uint base0 = slot0 & SlotMask;
                if (slot0 != 0 && off >= base0 && off < base0 + vsize)
                    return kseg + (off - base0);
                if (dump != 0)
                {
                    uint dumpOff = dump & SlotMask;
                    if ((va & ~SlotMask) == (dump & ~SlotMask)
                        && off >= dumpOff && off < dumpOff + vsize)
                        return kseg + (off - dumpOff);
                }
            }
            return va;
        }

        private static void MarkExtraRomTocDecompressed(uint dest)
        {
            if (_tocDecompSlot != null
                && (_tocDecompSlot.DecompDest == dest || dest == 0))
                _tocDecompSlot.Decompressed = true;
            if (_romTocMods != null && dest != 0)
            {
                for (int i = 0; i < _romTocCount; i++)
                {
                    ExtraRomTocMod s = _romTocMods[i];
                    if (s != null && s.DecompDest != 0 && s.DecompDest == dest)
                        s.Decompressed = true;
                }
            }
            if (_tocDestReady == null || dest == 0)
                return;
            for (int i = 0; i < _tocDestN; i++)
            {
                if (_tocDestKseg[i] == dest)
                    _tocDestReady[i] = true;
            }
        }

        private static string FileBaseName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            int slash = path.LastIndexOf('\\');
            if (slash < 0)
                slash = path.LastIndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }

        private static uint PeekDestWord(MipsBus bus, uint va)
        {
            if (bus == null || va == 0)
                return 0;
            // Live 822671a: do not rewrite dest0 useg to pfn6
            // before comparing dest6/dest10/dest0/destDump.
            try
            {
                return bus.Read32(va);
            }
            catch
            {
                return 0;
            }
        }

        // dest0 useg / destDump peek must not go through
        // MapFirmwareSlotVa pfn6 remap (that hid dest0-useg).
        private static uint PeekDestWordRaw(MipsBus bus, uint va, out bool threw)
        {
            threw = false;
            if (bus == null || va == 0)
                return 0;
            _ddiNopDestPeekRaw = true;
            try
            {
                return bus.Read32(va);
            }
            catch
            {
                threw = true;
                return 0;
            }
            finally
            {
                _ddiNopDestPeekRaw = false;
            }
        }

        // Live ccb9552: VALLOC vbase 0x01980000, CEDecompressROM
        // dest 0x01981000 (o32 rva 0x1000). MZ is at vbase, not
        // the section first word. dest6 0x86F1C000 took 1038
        // stores; pfn6-word at section base stayed 0.
        private const uint DdiNopVbasePage = 0x01980000u;
        private const uint DdiNopDest0Page = 0x01981000u;
        private const uint DdiNopDestDumpPage = 0x03981000u;
        private const uint DdiNopDestKseg0Page = 0x81981000u;
        private const uint DdiNopDest6Live = 0x86F1C000u;
        private const uint DdiNopDest10Live = 0x806F1000u;
        // ExtraROM extract ddi_nop.dll .text RVA 0x2000
        // (file ptr 0x1200). Live 021a2eb dest+0x1000
        // 0x86F1D000 word. TOC cache is compressed o32,
        // not PE bytes. Do not invent this word.
        private const uint DdiNopTextSigRva2000 = 0x8C481B78u;
        private const uint DdiNopTextVsize = 0x1743Au;
        // ExtraROM extract ddi_nop.dll AddressOfEntryPoint.
        // Prefer slot e32[1] (e32_entryrva). Do not invent e32.
        private const uint DdiNopEntryRvaExtract = 0x18014u;
        // Extract IAT FirstThunk / .data VA. VALLOC IAT is
        // vbase+this. Do not invent dest10.
        private const uint DdiNopIatRva = 0x19000u;
        // Live 68b9567: data-TLBL 0x0199B050 after coredll-
        // page. o32[.data] dest 0x01999000 + vsz covers
        // past the IAT page. dest0 walk stopped at
        // 0x019B0000. Demand-map remaining VALLOC .data
        // via firmware PTE. Do not invent dest.
        private const uint DdiNopVallocLo = 0x01980000u;
        private const uint DdiNopVallocHi = 0x019B0000u;
        private const int DdiNopDataPageCap = 32;

        private static void ResetDdiNopDecompStores()
        {
            _ddiNopDecompWatch = false;
            _ddiNopWatchDest6 = DdiNopDest6Live;
            _ddiNopWatchDest10 = DdiNopDest10Live;
            _ddiNopWatchVbase6 = 0;
            _ddiNopStoreN0 = 0;
            _ddiNopStoreN6 = 0;
            _ddiNopStoreNV6 = 0;
            _ddiNopStoreN10 = 0;
            _ddiNopStoreND = 0;
            _ddiNopStoreNK = 0;
            _ddiNopStoreFirstVa = 0;
            _ddiNopStoreFirstVal = 0;
            _ddiNopStoreLastVa = 0;
            _ddiNopStoreLastVal = 0;
            _ddiNopStoreThrew0 = false;
            _ddiNopStoreThrew6 = false;
            _ddiNopStoreThrewV = false;
            _ddiNopStoreThrew10 = false;
            _ddiNopStoreThrewD = false;
            _ddiNopStoreThrewK = false;
        }

        // Live l2 only. Do not invent dest6 / dest10 / l2.
        private static bool WalkDdiNopPteDests(MipsBus bus, uint va,
            out uint l2, out uint dest6, out uint dest10)
        {
            l2 = 0;
            dest6 = 0;
            dest10 = 0;
            if (bus == null || va == 0)
                return false;
            uint sec = PeekSection(bus, 0);
            if (sec == 0 || sec == 1)
                return false;
            uint l1Ptr = sec + (((va >> 16) & 0x1FFu) * 4);
            uint l1;
            if (!TryPeekWord(bus, l1Ptr, out l1) || l1 == 0 || l1 == 1)
                return false;
            uint l2Ptr = l1 + ((((va >> 12) & 0xFu) + 3) * 4);
            if (!TryPeekWord(bus, l2Ptr, out l2) || l2 == 0 || (l2 & 2) == 0)
                return false;
            dest6 = 0x80000000u | ((((l2 >> 6) << 12) & 0x1FFFFFFFu));
            dest6 |= va & 0xFFFu;
            dest10 = 0x80000000u | ((((l2 >> 10) << 12) & 0x1FFFFFFFu));
            dest10 |= va & 0xFFFu;
            return dest6 != 0 || dest10 != 0;
        }

        private static void WalkDdiNopWatchDests(MipsBus bus)
        {
            _ddiNopWatchDest6 = DdiNopDest6Live;
            _ddiNopWatchDest10 = DdiNopDest10Live;
            _ddiNopWatchVbase6 = 0;
            if (bus == null)
                return;
            uint dest6;
            uint dest10;
            uint v6;
            uint unused;
            if (WalkDdiNopPteDests(bus, DdiNopDest0Page, out unused, out dest6, out dest10))
            {
                if (dest6 != 0)
                    _ddiNopWatchDest6 = dest6;
                if (dest10 != 0)
                    _ddiNopWatchDest10 = dest10;
            }
            if (WalkDdiNopPteDests(bus, DdiNopVbasePage, out unused, out v6, out dest10)
                && v6 != 0)
                _ddiNopWatchVbase6 = v6;
        }

        private static void BeginDdiNopDecompStoreWatch(MipsBus bus)
        {
            ResetDdiNopDecompStores();
            WalkDdiNopWatchDests(bus);
            _ddiNopDecompWatch = true;
        }

        private static int DdiNopDecompStoreSlot(uint mappedVa)
        {
            uint page = mappedVa & ~0xFFFu;
            if (page == DdiNopDest0Page)
                return 0;
            if (page == (_ddiNopWatchDest6 & ~0xFFFu))
                return 1;
            if (_ddiNopWatchVbase6 != 0
                && page == (_ddiNopWatchVbase6 & ~0xFFFu))
                return 5;
            if (page == (_ddiNopWatchDest10 & ~0xFFFu))
                return 2;
            if (page == DdiNopDestDumpPage)
                return 3;
            if (page == DdiNopDestKseg0Page)
                return 4;
            return -1;
        }

        // Count after Map* so dest0 remapped to dest6 counts
        // as dest6. Do not invent dest.
        public static bool TryNoteDdiNopDecompStore(uint mappedVa, uint value)
        {
            if (!_ddiNopDecompWatch)
                return false;
            int slot = DdiNopDecompStoreSlot(mappedVa);
            if (slot < 0)
                return false;
            if (slot == 0)
                _ddiNopStoreN0++;
            else if (slot == 1)
                _ddiNopStoreN6++;
            else if (slot == 5)
                _ddiNopStoreNV6++;
            else if (slot == 2)
                _ddiNopStoreN10++;
            else if (slot == 3)
                _ddiNopStoreND++;
            else
                _ddiNopStoreNK++;
            if (_ddiNopStoreFirstVa == 0)
            {
                _ddiNopStoreFirstVa = mappedVa;
                _ddiNopStoreFirstVal = value;
            }
            _ddiNopStoreLastVa = mappedVa;
            _ddiNopStoreLastVal = value;
            return true;
        }

        public static void TryNoteDdiNopDecompStoreThrow(uint mappedVa)
        {
            if (!_ddiNopDecompWatch)
                return;
            int slot = DdiNopDecompStoreSlot(mappedVa);
            if (slot == 0)
                _ddiNopStoreThrew0 = true;
            else if (slot == 1)
                _ddiNopStoreThrew6 = true;
            else if (slot == 5)
                _ddiNopStoreThrewV = true;
            else if (slot == 2)
                _ddiNopStoreThrew10 = true;
            else if (slot == 3)
                _ddiNopStoreThrewD = true;
            else if (slot == 4)
                _ddiNopStoreThrewK = true;
        }

        private static void LogDdiNopDecompStores()
        {
            BootLog.Write("[Hive] ExtraROM ddi_nop dest stores dest0=" +
                _ddiNopStoreN0 +
                " dest6=" + _ddiNopStoreN6 +
                " vbase6=" + _ddiNopStoreNV6 +
                " dest10=" + _ddiNopStoreN10 +
                " dump=" + _ddiNopStoreND +
                " kseg0=" + _ddiNopStoreNK +
                " first=0x" + _ddiNopStoreFirstVa.ToString("X8") +
                ":0x" + _ddiNopStoreFirstVal.ToString("X8") +
                " last=0x" + _ddiNopStoreLastVa.ToString("X8") +
                ":0x" + _ddiNopStoreLastVal.ToString("X8") +
                " threw=" + (_ddiNopStoreThrew6 ? "6" : "") +
                (_ddiNopStoreThrewV ? "V" : "") +
                (_ddiNopStoreThrew10 ? "A" : "") +
                (_ddiNopStoreThrew0 ? "0" : "") +
                (_ddiNopStoreThrewD ? "D" : "") +
                (_ddiNopStoreThrewK ? "K" : ""));
        }

        // Live ccb9552 dest10 pfn10-word 0x806F0000 is a
        // kseg0 page pointer, not MZ. Serve only MZ at vbase.
        private static bool IsMzWord(uint word)
        {
            return (word & 0xFFFFu) == 0x5A4D;
        }

        // TOC[33] ExtraROM extract ddi_nop.dll .text RVA 0x2000.
        // Cached o32 Data[] is compressed; not PE bytes.
        private static uint DdiNopTextSigExpected()
        {
            uint[] blob = null;
            if (_ddiNopData != null && _ddiNopData.Length > 0)
                blob = _ddiNopData[0];
            if (blob == null)
            {
                ExtraRomTocMod slot = FindCachedExtraRomToc("ddi_nop.dll");
                if (slot != null && slot.Data != null && slot.Data.Length > 0)
                    blob = slot.Data[0];
            }
            // File ptr 0x1200 / 4 = word 0x480. Only if
            // cached blob is an expanded PE (MZ).
            if (blob != null && blob.Length > 0x480
                && (blob[0] & 0xFFFFu) == 0x5A4D)
                return blob[0x480];
            return DdiNopTextSigRva2000;
        }

        private static bool DdiNopDestStoresAllowServe()
        {
            return _ddiNopStoreN0 != 0
                || _ddiNopStoreN6 != 0
                || _ddiNopStoreNV6 != 0;
        }

        // Live ccb9552: section dest6 0x86F1C000 took 1038
        // stores; pfn6-word at 0x86F1C000 stayed 0. MZ for a
        // CE TOC module is at VALLOC vbase 0x01980000
        // (dest - 0x1000), not the o32 section page.
        private static void TryMeasureDdiNopDestAfterDecomp(MipsBus bus, uint hdr, uint expanded)
        {
            if (_ddiNopDestPteMeasured || bus == null)
                return;
            _ddiNopDestPteMeasured = true;
            uint vbase = DdiNopVbasePage;
            uint dest0 = DdiNopDest0Page;
            uint vl2 = 0;
            uint sl2 = 0;
            uint vbase6 = 0;
            uint vbase10 = 0;
            uint dest6 = 0;
            uint dest10 = 0;
            WalkDdiNopPteDests(bus, vbase, out vl2, out vbase6, out vbase10);
            WalkDdiNopPteDests(bus, dest0, out sl2, out dest6, out dest10);
            bool tv6 = false;
            bool tv10 = false;
            bool ts6 = false;
            bool ts10 = false;
            uint vw6 = vbase6 != 0 ? PeekDestWordRaw(bus, vbase6, out tv6) : 0;
            uint vw10 = vbase10 != 0 ? PeekDestWordRaw(bus, vbase10, out tv10) : 0;
            uint dw6 = dest6 != 0 ? PeekDestWordRaw(bus, dest6, out ts6) : 0;
            uint dw10 = dest10 != 0 ? PeekDestWordRaw(bus, dest10, out ts10) : 0;
            bool tsig = false;
            uint sig = 0;
            if (dest6 != 0)
                sig = PeekDestWordRaw(bus, dest6 + 0x1000u, out tsig);
            bool tv0 = false;
            uint vw0 = PeekDestWordRaw(bus, vbase, out tv0);
            BootLog.Write("[Hive] ExtraROM ddi_nop dest vbase PTE vbase6=0x" +
                vbase6.ToString("X8") +
                " vw6=0x" + vw6.ToString("X8") +
                " vw10=0x" + vw10.ToString("X8") +
                " dest6=0x" + dest6.ToString("X8") +
                " dw6=0x" + dw6.ToString("X8") +
                " sig=0x" + sig.ToString("X8") +
                " dw10=0x" + dw10.ToString("X8") +
                " threw=" + (tv6 ? "V" : "") + (ts6 ? "6" : "") +
                (tv10 ? "A" : "") + (ts10 ? "S" : "") +
                (tsig ? "G" : "") + (tv0 ? "0" : ""));
            uint span = expanded != 0 && expanded != 0xFFFFFFFFu
                ? expanded : _ddiNopDecompVsize;
            uint end = vbase + 0x1000u + span;
            if (end <= vbase)
                end = vbase + 0x1000u;
            uint last = (end - 1u) & ~0xFFFu;
            uint mzVa = 0;
            uint mz6 = 0;
            uint mzW = 0;
            uint nzVa = 0;
            uint nz6 = 0;
            uint nzW = 0;
            int n = 0;
            for (uint va = vbase; va <= last && n < 32; va += 0x1000u, n++)
            {
                uint l2;
                uint page6;
                uint page10;
                if (!WalkDdiNopPteDests(bus, va, out l2, out page6, out page10)
                    || page6 == 0)
                    continue;
                bool threw;
                uint w = PeekDestWordRaw(bus, page6, out threw);
                if (threw || w == 0)
                    continue;
                if (nzVa == 0)
                {
                    nzVa = va;
                    nz6 = page6;
                    nzW = w;
                }
                if (mzVa == 0 && IsMzWord(w))
                {
                    mzVa = va;
                    mz6 = page6;
                    mzW = w;
                }
            }
            BootLog.Write("[Hive] ExtraROM ddi_nop dest MZ page mz=0x" +
                mzVa.ToString("X8") + "->0x" + mz6.ToString("X8") +
                " w=0x" + mzW.ToString("X8") +
                " nz=0x" + nzVa.ToString("X8") + "->0x" + nz6.ToString("X8") +
                " w=0x" + nzW.ToString("X8"));
            _ddiNopLandedDest = 0;
            _ddiNopLandedWord = 0;
            _ddiNopLandedBySig = false;
            // Live 021a2eb: dest-word 0 at dest6 is honest
            // (.text starts 0). dest+0x1000 word 0x8C481B78
            // matches extract .text RVA 0x2000. dest10
            // 0x806F0000 is not MZ. Do not invent dest.
            if (!DdiNopDestStoresAllowServe())
                return;
            uint expect = DdiNopTextSigExpected();
            bool sizeOk = expanded != 0 && expanded != 0xFFFFFFFFu
                && (expanded == _ddiNopDecompVsize || expanded == DdiNopTextVsize);
            if (sizeOk && dest6 != 0 && !tsig && sig == expect
                && (dest6 & ~0xFFFu) != DdiNopDest10Live)
            {
                _ddiNopLandedDest = dest6;
                _ddiNopLandedWord = sig;
                _ddiNopLandedBySig = true;
                return;
            }
            if (vbase6 != 0 && IsMzWord(vw6))
            {
                _ddiNopLandedDest = vbase6;
                _ddiNopLandedWord = vw6;
            }
            else if (IsMzWord(vw0))
            {
                _ddiNopLandedDest = vbase;
                _ddiNopLandedWord = vw0;
            }
        }

        // Live 37c4995: sig matched but LoadLibrary v0!=0
        // (firmware already MapO32'd; BindImp COREDLL). Serve
        // dest6 here, not on a LoadLibrary miss that will not
        // come. Do not serve dest10.
        private static void TryServeDdiNopAtDecompRet(MipsBus bus, uint[] regs)
        {
            if (!_ddiNopLandedBySig || _ddiNopLandedDest == 0)
                return;
            if ((_ddiNopLandedDest & ~0xFFFu) == DdiNopDest10Live)
                return;
            ExtraRomTocMod slot = FindCachedExtraRomToc("ddi_nop.dll");
            if (slot == null)
                return;
            uint dest6 = _ddiNopLandedDest;
            uint vbase = DdiNopVbasePage;
            slot.DecompDest = dest6;
            slot.Vbase = vbase;
            slot.Decompressed = true;
            MarkExtraRomTocDecompressed(dest6);
            TrySetDdiNopRamStartip(bus, 0, regs);
            BootLog.Write("[Hive] TOC[" + slot.Index + "] ddi_nop.dll serve dest6=0x" +
                dest6.ToString("X8") +
                " sig=0x" + _ddiNopLandedWord.ToString("X8") +
                " vbase=0x" + vbase.ToString("X8") +
                " (CEDecompressROM .text sig; not LoadLibrary miss)");
            BootLog.Rom("ok", "ExtraROM", "TOC", slot.Index, slot.Name, 7,
                dest6, _ddiNopLandedWord, vbase,
                "CEDecompressROM .text sig; serve dest6");
        }

        private static uint DdiNopEntryRvaFromSlot(ExtraRomTocMod slot)
        {
            if (slot != null && slot.E32Words != null
                && slot.E32Words.Length > 1 && slot.E32Words[1] != 0)
                return slot.E32Words[1];
            return DdiNopEntryRvaExtract;
        }

        // Live 9183b83: heap TOC-attach openexe, so
        // obj-96 is not a MODULE. Find the real in-flight
        // MODULE via pointer oe or a pmodNext walk from
        // live seeds. Do not invent a module.
        private static void TrySetDdiNopRamStartip(MipsBus bus, uint hintModule)
        {
            TrySetDdiNopRamStartip(bus, hintModule, null);
        }

        private static void TrySetDdiNopRamStartip(MipsBus bus, uint hintModule, uint[] regs)
        {
            if (regs != null)
                NoteDdiNopWalkSeeds(regs);
            ExtraRomTocMod slot = FindCachedExtraRomToc("ddi_nop.dll");
            uint entryrva = DdiNopEntryRvaFromSlot(slot);
            TrySetDdiNopModuleStartip(bus, DdiNopVbasePage, entryrva, hintModule);
        }

        private static void TrySetDdiNopModuleStartip(MipsBus bus, uint vbase, uint entryrva)
        {
            TrySetDdiNopModuleStartip(bus, vbase, entryrva, 0);
        }

        private static void TrySetDdiNopModuleStartip(MipsBus bus, uint vbase, uint entryrva, uint hintModule)
        {
            if (bus == null)
                return;
            if (vbase == 0)
                vbase = DdiNopVbasePage;
            if (entryrva == 0)
                entryrva = DdiNopEntryRvaExtract;
            uint want = vbase + entryrva;
            uint dumpXip = DdiNopVbase + entryrva;
            uint module = 0;
            uint p50 = 0;
            uint before = 0;
            string why;
            try
            {
                _ddiNopStartipAttempted = true;
                module = FindInFlightDdiNopModule(bus, hintModule);
                if (module == 0)
                {
                    why = "skip-no-mod";
                    LogDdiNopNoModOnce(bus);
                }
                else
                {
                    TryPeekWord(bus, module + ProcModule, out p50);
                    TryPeekWord(bus, module + ModuleStartip, out before);
                    if (before == 0)
                    {
                        bus.Write32(module + ModuleStartip, want);
                        why = "set-zero";
                    }
                    else if (before == dumpXip && _ddiNopLandedBySig)
                    {
                        bus.Write32(module + ModuleStartip, want);
                        why = "set-dump-xip";
                    }
                    else if (before == want)
                        why = "keep";
                    else
                        why = "skip-have";
                    TrySetDdiNopVallocBasePtr(bus, module);
                }
            }
            catch
            {
                why = "skip-throw";
            }
            uint entryWord = PeekDdiNopRamEntryWord(bus);
            BootLog.Write("[Hive] ExtraROM ddi_nop startip module=0x" +
                module.ToString("X8") +
                " mod+0x50=0x" + p50.ToString("X8") +
                " startip=0x" + before.ToString("X8") +
                " " + why +
                " startip=0x" + want.ToString("X8") +
                " entry-word=0x" + entryWord.ToString("X8"));
        }

        // Live e8489d0: module+0x50 stayed dump XIP
        // 0x03980000 while serve/startip used VALLOC
        // 0x01980000. BindImp then walked dump IAT.
        // Only this field, only ddi_nop, only dump XIP.
        private static void TrySetDdiNopVallocBasePtr(MipsBus bus, uint module)
        {
            if (!_ddiNopLandedBySig || bus == null || module == 0)
                return;
            if (!IsDdiNopModule(bus, module))
                return;
            uint p50;
            if (!TryPeekWord(bus, module + ProcModule, out p50))
                return;
            if (p50 != DdiNopVbase)
                return;
            bus.Write32(module + ProcModule, DdiNopVbasePage);
            BootLog.Write("[Hive] ExtraROM ddi_nop baseptr module=0x" +
                module.ToString("X8") +
                " was=0x" + p50.ToString("X8") +
                " set-valloc=0x" + DdiNopVbasePage.ToString("X8"));
        }

        // Live 5166cf2: set-xip 0x800B2000 made F7BC treat
        // MIPS prologues as the export dir (expVA words
        // 0xAFAA0014…). 94038eb GetProc was correct with
        // ImageBase 0x03F50000 → v0=0x03F57EB4. Keep that
        // BasePtr. Undo leftover XIP. Do not invent a
        // new BasePtr. Do not serve PE into 0x800B2000.
        private static void TryKeepCoredllImageBasePtr(MipsBus bus, uint module)
        {
            if (bus == null || module == 0)
                return;
            if (IsDdiNopModule(bus, module))
                return;
            uint p50;
            if (!TryPeekWord(bus, module + ProcModule, out p50))
                return;
            bool coredll = module == _coredllModule
                || p50 == CoredllSharedLo
                || (p50 >= 0x80000000u
                    && NamesMatchRom(_ddiNopBindLibName, "coredll.dll")
                    && module == _ddiNopBindLibV0);
            if (!coredll)
                return;
            if (p50 >= 0x80000000u)
            {
                bus.Write32(module + ProcModule, CoredllSharedLo);
                BootLog.Write("[Hive] ExtraROM coredll baseptr module=0x" +
                    module.ToString("X8") +
                    " was=0x" + p50.ToString("X8") +
                    " undo-xip=0x" + CoredllSharedLo.ToString("X8"));
                return;
            }
            if (p50 != CoredllSharedLo)
                return;
            if (_coredllBasePtrLogged)
                return;
            _coredllBasePtrLogged = true;
            BootLog.Write("[Hive] ExtraROM coredll baseptr keep-imagebase=0x" +
                CoredllSharedLo.ToString("X8") +
                " (XIP+exp was code, not export dir)");
        }

        private static void PeekDdiNopIatWord(MipsBus bus, out uint word, out uint dest6)
        {
            word = 0;
            dest6 = _ddiNopIatDest6;
            uint va = DdiNopVbasePage + DdiNopIatRva;
            uint l2 = 0;
            uint dest10 = 0;
            if (dest6 == 0)
                WalkDdiNopPteDests(bus, va, out l2, out dest6, out dest10);
            if (dest6 != 0 && !IsDdiNopDest10Page(dest6))
            {
                bool threw;
                word = PeekDestWordRaw(bus, dest6, out threw);
                if (!threw)
                    return;
            }
            TryPeekWord(bus, va, out word);
        }

        // Live 5166cf2: after a good GetProc v0 the IAT
        // stayed 0. Observe whether BindImp stores into
        // 0x01999000 / dest6. Log the next BindImp PCs
        // if it does not. Do not invent IAT fills.
        private static void TryNoteBindImpAfterGoodV0(MipsBus bus, uint pc)
        {
            if (_ddiNopOrdGoodV0 == 0 || _ddiNopOrdAfterDone)
                return;
            if (pc == BindImpOrdJalRet)
                return;
            if (pc >= 0x80000000u && pc < 0x80000400u)
                return;
            if (pc == _ddiNopOrdAfterLast)
                return;
            uint iat = 0;
            uint dest6 = 0;
            PeekDdiNopIatWord(bus, out iat, out dest6);
            uint va = DdiNopVbasePage + DdiNopIatRva;
            if (_ddiNopIatStoreLogged || iat == _ddiNopOrdGoodV0)
            {
                _ddiNopOrdAfterDone = true;
                BootLog.Write("[Hive] ExtraROM BindImp-after pc=0x" +
                    pc.ToString("X8") +
                    " iat=0x" + iat.ToString("X8") +
                    " dest6=0x" + dest6.ToString("X8") +
                    " va=0x" + va.ToString("X8") +
                    " (store)");
                return;
            }
            if (iat != 0)
            {
                _ddiNopOrdAfterDone = true;
                BootLog.Write("[Hive] ExtraROM BindImp-after pc=0x" +
                    pc.ToString("X8") +
                    " iat=0x" + iat.ToString("X8") +
                    " dest6=0x" + dest6.ToString("X8") +
                    " va=0x" + va.ToString("X8") +
                    " (iat-has)");
                return;
            }
            _ddiNopOrdAfterLast = pc;
            _ddiNopOrdAfterN++;
            if (_ddiNopOrdAfterN <= 6)
            {
                BootLog.Write("[Hive] ExtraROM BindImp-after pc=0x" +
                    pc.ToString("X8") +
                    " iat=0x00000000 dest6=0x" + dest6.ToString("X8") +
                    " va=0x" + va.ToString("X8") +
                    " (no-store)");
            }
            // Back at GetProc without an IAT write is the
            // drop. Do not conclude after four sequential
            // BindImp instructions — the store may be later.
            bool backGetProc = pc == BindImpOrdBaseLw
                || pc == BindImpOrdLookup;
            if (!backGetProc && _ddiNopOrdAfterN < 16)
                return;
            _ddiNopOrdAfterDone = true;
            BootLog.Write("[Hive] ExtraROM BindImp-ord drop-v0=0x" +
                _ddiNopOrdGoodV0.ToString("X8") +
                " no-IAT last=0x" + pc.ToString("X8") +
                (backGetProc ? " (back-GetProc)" : ""));
            TryNoteBindImpIatSwSkipped();
        }

        // Live d79cd40: BindImp touches 0xFFFF5800 before
        // sw $v0,0($v1) at 0x80019124. UserKData addiu
        // sign-extends; kernel KData is already live at
        // 0xFFFFD800 (nest/CurProc/ThreadPtr). Alias the
        // user page onto that KData. Do not invent bytes.
        public static uint MapUserKDataVa(uint va)
        {
            if (!_userKPageAlias)
                return va;
            if ((va & ~0xFFFu) != (UserKPage & ~0xFFFu))
                return va;
            return (KDataBase & ~0xFFFu) | (va & 0xFFFu);
        }

        // Live 674d704: lh at 0xFFFFFCE1 (base=1 +
        // sign_extend 0xFCE0). Same discipline as
        // MapUserKDataVa: rewrite onto live firmware
        // backing only. Peek 0xFFFFF000 or TLB PFN
        // (kseg0). Do not alias KData. Do not
        // zero-fill SharedUserData. Do not rewrite
        // GPR23.
        public static uint MapFfffF000Va(MipsBus bus, uint va)
        {
            if (_ffffF000Busy)
                return va;
            if (!IsFfffF000Armed())
                return va;
            if ((va & ~0xFFFu) != FfffF000Page)
                return va;
            if (_ffffF000Kseg != 0)
                return _ffffF000Kseg | (va & 0xFFFu);
            TryResolveFfffF000(bus, va);
            if (_ffffF000Kseg != 0)
                return _ffffF000Kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsFfffF000Armed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ffffFce1Logged || _ffffF000Demand;
        }

        private static void TryResolveFfffF000(MipsBus bus, uint va)
        {
            if (bus == null || _ffffF000Busy || _ffffF000Done)
                return;
            if ((va & ~0xFFFu) != FfffF000Page)
                return;
            try
            {
                _ffffF000Busy = true;
                _ffffF000Demand = true;
                uint word = 0;
                if (TryPeekWord(bus, FfffF000Page | (va & 0xFFFu), out word)
                    || TryPeekWord(bus, FfffF000Page, out word))
                {
                    RememberFfffF000Kseg(bus, FfffF000Page, va, word, "live-peek");
                    return;
                }
                uint pfn = 0;
                bool valid = false;
                bool tlbHit = bus.TryFindTlbPfn(FfffF000Page, out pfn, out valid);
                if (tlbHit && valid)
                {
                    uint dest = 0x80000000u | ((pfn << 12) & 0x1FFFFFFFu);
                    if ((dest & 0x1FFFFFFFu) >= 0x00010000u
                        && (TryPeekWord(bus, dest | (va & 0xFFFu), out word)
                            || TryPeekWord(bus, dest, out word)))
                    {
                        RememberFfffF000Kseg(bus, dest, va, word, "tlb-pfn");
                        return;
                    }
                }
                if (!_ffffF000Logged)
                {
                    _ffffF000Logged = true;
                    _ffffF000Done = true;
                    uint kd = 0;
                    bool kdOk = TryPeekWord(bus, KDataBase, out kd);
                    string tlbWhy = "none";
                    if (tlbHit)
                        tlbWhy = valid
                            ? "pfn=0x" + pfn.ToString("X") + "-unmapped"
                            : "inv-pfn=0x" + pfn.ToString("X");
                    BootLog.Write("[Hive] ExtraROM ddi_nop ffff-f000 map va=0x" +
                        FfffF000Page.ToString("X8") +
                        " pte-miss tlb=" + tlbWhy +
                        (kdOk ? " FFFFD800=0x" + kd.ToString("X8") : " FFFFD800-unmapped") +
                        " (SharedUserData; no dump page; not UserK/KData alias; do not invent dest)");
                }
            }
            finally
            {
                _ffffF000Busy = false;
            }
        }

        private static void RememberFfffF000Kseg(MipsBus bus, uint kseg,
            uint va, uint word, string via)
        {
            _ffffF000Kseg = kseg & ~0xFFFu;
            if (_ffffF000Logged)
                return;
            _ffffF000Logged = true;
            _ffffF000Done = true;
            if (via == null)
                via = "firmware";
            BootLog.Write("[Hive] ExtraROM ddi_nop ffff-f000 map va=0x" +
                FfffF000Page.ToString("X8") +
                " -> 0x" + _ffffF000Kseg.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " via=" + via +
                " (SharedUserData; firmware backing; do not invent dest)");
        }

        private static void TryArmUserKPageAlias(MipsBus bus)
        {
            if (_userKPageAliasNoted)
                return;
            _userKPageAliasNoted = true;
            uint userWord = 0;
            bool userMapped = TryPeekWord(bus, UserKPage, out userWord);
            uint kdataWord = 0;
            bool kdataMapped = TryPeekWord(bus, KDataBase, out kdataWord);
            if (!userMapped)
            {
                BootLog.Write("[Hive] ExtraROM BindImp-iat FFFF5800-unmapped" +
                    (kdataMapped
                        ? " kdata=0x" + kdataWord.ToString("X8")
                        : " KData-unmapped"));
            }
            else if (userWord == 0)
            {
                BootLog.Write("[Hive] ExtraROM BindImp-iat FFFF5800=0" +
                    (kdataMapped
                        ? " kdata=0x" + kdataWord.ToString("X8")
                        : " KData-unmapped"));
            }
            if (userMapped && userWord != 0)
                return;
            if (!kdataMapped)
                return;
            _userKPageAlias = true;
            BootLog.Write("[Hive] ExtraROM BindImp-iat alias 0x" +
                UserKPage.ToString("X8") +
                " -> 0x" + KDataBase.ToString("X8") +
                " (KData live; do not invent contents)");
        }

        // Live 19656e2: *(fp+0x1C) / v1 was o32[.data].real
        // 0x01F57000. sw 0x80019124 wrote the resolve there,
        // not VALLOC IAT 0x01999000. Same class as ddi_nop
        // set-valloc. Rewrite dump-real slot to the served
        // o32 dest. Do not invent dest or IAT fills.
        private static bool TryGetDdiNopIatBases(out uint real, out uint dest,
            out uint span)
        {
            real = _ddiNopIatReal;
            dest = _ddiNopIatValloc;
            span = _ddiNopIatSpan;
            if (real != 0 && dest != 0 && !IsDdiNopDest10Page(dest))
                return true;
            int sec;
            uint vsize;
            uint rva;
            uint psize;
            uint dataptr;
            uint flags;
            uint[] blob;
            if (!TryFindDdiNopDataO32(out sec, out vsize, out rva, out psize,
                out dataptr, out real, out flags, out blob))
            {
                dest = 0;
                span = 0;
                return false;
            }
            dest = DdiNopVbasePage + (rva != 0 ? rva : DdiNopIatRva);
            span = vsize;
            if (real == 0 || dest == 0 || IsDdiNopDest10Page(dest))
                return false;
            _ddiNopIatReal = real;
            _ddiNopIatValloc = dest;
            _ddiNopIatSpan = span;
            _ddiNopIatPsize = psize;
            return true;
        }

        private static bool TryMapDumpIatSlot(uint ptr, uint real, uint dest,
            uint span, out uint want)
        {
            want = 0;
            if (ptr == 0 || real == 0 || dest == 0)
                return false;
            if (IsDdiNopDest10Page(ptr) || IsDdiNopDest10Page(dest))
                return false;
            uint off;
            if (span == 0)
            {
                if (ptr != real)
                    return false;
                off = 0;
            }
            else
            {
                if (ptr < real || ptr >= real + span)
                    return false;
                off = ptr - real;
                if ((off & 3u) != 0)
                    return false;
            }
            want = dest + off;
            return want != 0 && want != ptr;
        }

        private static void TryFixBindImpIatSlot(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_ddiNopAwaitCallDll || !_ddiNopLandedBySig || !_ddiNopIatWatch)
                return;
            if (bus == null || regs == null || regs.Length <= 3)
                return;
            if (pc != BindImpOrdJalRet && pc != BindImpIatSlotLw
                && pc != BindImpIatNext && pc != BindImpIatNextAfter
                && (pc < BindImpIatKdata || pc > BindImpIatAfter))
                return;
            uint real;
            uint dest;
            uint span;
            if (!TryGetDdiNopIatBases(out real, out dest, out span))
                return;
            uint fp = regs.Length > 30 ? regs[30] : 0;
            uint fp1c = 0;
            bool fpOk = fp != 0 && TryPeekWord(bus, fp + BindImpFpIatOff, out fp1c);
            uint v1 = regs[3];
            uint want;
            uint written = 0;
            uint was = 0;
            if (fpOk && TryMapDumpIatSlot(fp1c, real, dest, span, out want))
            {
                was = fp1c;
                written = want;
                try
                {
                    bus.Write32(fp + BindImpFpIatOff, want);
                }
                catch
                {
                    return;
                }
                if (TryMapDumpIatSlot(v1, real, dest, span, out want))
                {
                    regs[3] = want;
                    written = want;
                }
            }
            else if (TryMapDumpIatSlot(v1, real, dest, span, out want))
            {
                was = v1;
                written = want;
                regs[3] = want;
            }
            else
            {
                TryNoteBindImpIatNext(pc, fp1c, dest, span);
                return;
            }
            if (written == 0)
                return;
            if (_bindImpIatSlotLog < BindImpObserveMax)
            {
                _bindImpIatSlotLog++;
                BootLog.Write("[Hive] ExtraROM BindImp-iat slot was=0x" +
                    was.ToString("X8") +
                    " set-valloc=0x" + written.ToString("X8"));
            }
            TryNoteBindImpIatNext(pc, written, dest, span);
        }

        private static void TryNoteBindImpIatNext(uint pc, uint fp1c, uint dest,
            uint span)
        {
            if (pc != BindImpIatNext && pc != BindImpIatNextAfter)
                return;
            if (fp1c == _bindImpIatNextLast || _bindImpIatNextLog >= BindImpObserveMax)
                return;
            _bindImpIatNextLast = fp1c;
            _bindImpIatNextLog++;
            bool valloc = dest != 0 && fp1c >= dest
                && (span == 0 ? fp1c == dest : fp1c < dest + span);
            BootLog.Write("[Hive] ExtraROM BindImp-iat next fp1c=0x" +
                fp1c.ToString("X8") +
                (valloc ? " (valloc)" : " (not-valloc)"));
        }

        private static void TryNoteBindImpIatWindow(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_ddiNopAwaitCallDll || regs == null || regs.Length <= 3)
                return;
            if (pc < BindImpIatKdata || pc > BindImpIatAfter)
                return;
            if (pc == BindImpIatSw)
                _bindImpIatSwExpect = true;
            TryArmUserKPageAlias(bus);
            if (pc == _bindImpIatWinLast)
                return;
            if (_bindImpIatWinLog >= 8)
                return;
            _bindImpIatWinLast = pc;
            _bindImpIatWinLog++;
            uint v0 = regs[2];
            uint v1 = regs[3];
            uint fp = regs.Length > 30 ? regs[30] : 0;
            uint fp1c = 0;
            bool fpOk = fp != 0 && TryPeekWord(bus, fp + BindImpFpIatOff, out fp1c);
            uint kdata = 0;
            bool kOk = TryPeekWord(bus, UserKPage, out kdata);
            uint iat = DdiNopVbasePage + DdiNopIatRva;
            bool slot = v1 == iat || fp1c == iat
                || (_ddiNopIatDest6 != 0
                    && ((v1 & ~0xFFFu) == (_ddiNopIatDest6 & ~0xFFFu)
                        || (fp1c & ~0xFFFu) == (_ddiNopIatDest6 & ~0xFFFu)));
            BootLog.Write("[Hive] ExtraROM BindImp-iat pc=0x" +
                pc.ToString("X8") +
                " v0=0x" + v0.ToString("X8") +
                " v1=0x" + v1.ToString("X8") +
                " fp1c=0x" + fp1c.ToString("X8") +
                (fpOk ? "" : " fp1c-unmapped") +
                (kOk
                    ? " FFFF5800=0x" + kdata.ToString("X8")
                    : " FFFF5800-unmapped") +
                (slot ? "" : " (not-IAT-slot)"));
        }

        public static void TryNoteBindImpIatSw(uint origVa, uint value)
        {
            if (!_bindImpIatSwExpect || _ddiNopDestPeekRaw)
                return;
            _bindImpIatSwExpect = false;
            _bindImpIatSwLogged = true;
            if (_bindImpIatSwLog >= BindImpObserveMax)
                return;
            _bindImpIatSwLog++;
            uint iat = DdiNopVbasePage + DdiNopIatRva;
            bool hit = (origVa & ~0xFFFu) == iat
                || (_ddiNopIatDest6 != 0
                    && !IsDdiNopDest10Page(_ddiNopIatDest6)
                    && (origVa & ~0xFFFu) == (_ddiNopIatDest6 & ~0xFFFu));
            BootLog.Write("[Hive] ExtraROM BindImp-iat sw va=0x" +
                origVa.ToString("X8") +
                " word=0x" + value.ToString("X8") +
                (hit ? " (IAT)" : " (not-IAT)"));
        }

        private static void TryNoteBindImpIatSwSkipped()
        {
            if (_bindImpIatSwLogged || _ddiNopOrdGoodV0 == 0)
                return;
            if (_bindImpIatWinLog == 0 && !_userKPageAliasNoted)
                return;
            _bindImpIatSwLogged = true;
            BootLog.Write("[Hive] ExtraROM BindImp-iat 19124-skipped");
        }

        // Live d19770c: after slot7, exception save at
        // 0x8001528C. Name Cause/EPC/BadVAddr. Do not
        // invent IAT fills.
        public static void TryNoteBindImpException(uint code, uint epc, uint vaddr,
            uint vector, uint[] regs)
        {
            TryNoteBindImpException(code, epc, vaddr, vector, regs, null);
        }

        public static void TryNoteBindImpException(uint code, uint epc, uint vaddr,
            uint vector, uint[] regs, MipsBus bus)
        {
            if (!_ddiNopAwaitCallDll || !_ddiNopIatWatch || code == 0)
                return;
            if (_ddiNopIatStoreN < 7 && !_ddiNopIatStoreLogged)
                return;
            TryNoteC2SpObserve(regs, epc);
            _bindImpExnCode = code;
            _bindImpExnEpc = epc;
            _bindImpExnVaddr = vaddr;
            if (code == 2
                && vaddr >= ProcessInfoPage && vaddr < 0x02000000u
                && (_ddiNopIatStoreN >= BindImpObserveMax
                    || _ddiNopIatStoreLogged
                    || _ddiNopSawCallDllPc))
            {
                _ddiNopInfoDemand = true;
                TryResolveDdiNopProcessInfo(bus);
            }
            if (code == 2
                && epc == vaddr
                && (vaddr & ~0xFFFu) == GwesDispFetchPage
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesDispFetchTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && (vaddr & ~0xFFFu) == GwesDispDataPage
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesDispDataTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && (vaddr & ~0xFFFu) == GwesTextBasePage
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesTextBaseTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && (vaddr & ~0xFFFu) == GwesDispData2Page
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesDispData2Tlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && (vaddr & ~0xFFFu) == GwesDispData3Page
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesDispData3Tlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc == vaddr
                && (vaddr & ~0xFFFu) == GwesText2Page
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesText2Tlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && IsDdiNopGwesImageVa(vaddr)
                && !IsNamedDdiNopGwesPage(vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopGwesImageTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && IsDdiNopCoredllImageVa(vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopCoredllImageTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && IsDdiNopFilesysSlotVa(vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopFilesysSlot2Tlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && epc != vaddr
                && IsDdiNopFilesys48dVa(vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopFilesys48dTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && IsDdiNopVallocDataVa(vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteDdiNopVallocDataTlbl(bus, regs, epc, vaddr, vector);
            }
            if (code == 2
                && IsFfffFce1ObserveVa(epc, vaddr)
                && (_ddiNopDllMainLogged || _ddiNopIatStoreN >= BindImpObserveMax))
            {
                TryNoteFfffFce1Observe(bus, regs, epc, vaddr, vector);
            }
            // Live c0347e8: B9 dest0 PTE fills after the
            // first TLBL. BindImp-exn on that refill hid
            // the next real miss and left Hive quiet.
            if (code == 2 && IsGwesDataB9Page(vaddr))
                return;
            // Live ddd472a: extra slot-2 pages are
            // demand-mapped. Do not consume the
            // one-shot on that refill.
            if (code == 2 && IsFilesysSlot2ExtraPage(vaddr))
                return;
            // Live bb6cdc7: this COREDLL page is now
            // demand-mapped. Do not consume the
            // one-shot on that refill.
            if (code == 2 && IsDdiNopCoredllImageVa(vaddr))
                return;
            // Live 98db5d5 / 73486bc: page-0 TLBS/TLBL
            // consumed the one-shot and hid later
            // real misses. Observe the named sites.
            // Do not map VA 0 / page 0.
            if (IsNearNullVa(vaddr))
            {
                if (code == 3 && epc == GwesNullStoreEpc && vaddr == 0)
                    TryNoteGwesNullStoreObserve(bus, regs, epc);
                else if (code == 2 && epc == NearNullTlblEpc
                    && vaddr == NearNullTlblVaddr)
                    TryNoteNearNullTlblObserve(bus, regs, epc, vaddr);
                return;
            }
            // Live f3c2d62 / 3ac5ed9: AdEL consumed
            // the one-shot. Observe 0xFFFFFB2A and
            // 0xC6FA7C9A. Do not map those VAs. Do
            // not invent dest. All AdEL /
            // epc==badvaddr unaligned skip the
            // one-shot so a later TLBL can name
            // itself.
            if (IsAdelSkip(code, epc, vaddr))
            {
                if (epc == FfffFb2aEpc && vaddr == FfffFb2aVaddr)
                    TryNoteFfffFb2aAdelObserve(bus, regs, epc, vaddr);
                else if (epc == AdelC6FaEpc && vaddr == AdelC6FaVaddr)
                    TryNoteAdelC6FaObserve(bus, regs, epc, vaddr);
                return;
            }
            // Live 3275fe9: C2* TLBS consumed the
            // one-shot. Observe 0xC201FE84. Do not
            // invent dest. Do not walk C2 as useg
            // (L1 alias). Skip so a later TLBL can
            // name itself.
            if (IsC2TlbsVa(code, vaddr))
            {
                if (epc == C2TlbsEpc && vaddr == C2TlbsVaddr)
                    TryNoteC2TlbsObserve(bus, regs, epc, vaddr);
                return;
            }
            if (_bindImpExnLogged)
                return;
            _bindImpExnLogged = true;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint v1 = regs != null && regs.Length > 3 ? regs[3] : 0;
            BootLog.Write("[Hive] ExtraROM BindImp-exn cause=" +
                code +
                " epc=0x" + epc.ToString("X8") +
                " badvaddr=0x" + vaddr.ToString("X8") +
                " vec=0x" + vector.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " v0=0x" + v0.ToString("X8") +
                " v1=0x" + v1.ToString("X8") +
                " stores=" + _ddiNopIatStoreN);
        }

        // Live 258ef59: page 0xFFFFF000 / 0xFFFFFCE1
        // or gwes epc 0x000593C8 chasing FFFF*.
        // One Hive line. Do not map. Do not invent.
        private static bool IsFfffFce1ObserveVa(uint epc, uint vaddr)
        {
            if ((vaddr & ~0xFFFu) == FfffF000Page)
                return true;
            return epc == FfffFce1Epc
                && (vaddr & 0xFF000000u) == 0xFF000000u;
        }

        private static uint PeekGpr(uint[] regs, int i)
        {
            if (regs == null || i < 0 || i >= regs.Length)
                return 0;
            return regs[i];
        }

        private static string GprHex(uint[] regs, int i)
        {
            return "0x" + PeekGpr(regs, i).ToString("X8");
        }

        private static void TryNoteFfffFce1Observe(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            if (_ffffFce1Logged)
                return;
            _ffffFce1Logged = true;
            uint insn = 0;
            TryPeekWord(bus, epc, out insn);
            string dis = insn != 0 ? FormatMipsOp(epc, insn) : "peek-miss";
            uint op = insn >> 26;
            uint rs = (insn >> 21) & 31;
            uint rt = (insn >> 16) & 31;
            uint uimm = insn & 0xFFFFu;
            int simm = (short)uimm;
            uint bas = PeekGpr(regs, (int)rs);
            uint formed = bas + (uint)simm;
            uint uk = 0;
            uint kd = 0;
            uint pg = 0;
            bool ukOk = TryPeekWord(bus, UserKPage, out uk);
            bool kdOk = TryPeekWord(bus, KDataBase, out kd);
            bool pgOk = TryPeekWord(bus, FfffF000Page, out pg);
            uint mapped = MapUserKDataVa(vaddr);
            BootLog.Write("[Hive] ExtraROM ddi_nop ffff-fce1 observe epc=0x" +
                epc.ToString("X8") +
                " badvaddr=0x" + vaddr.ToString("X8") +
                " vec=0x" + vector.ToString("X8") +
                " insn=0x" + insn.ToString("X8") +
                " " + dis +
                " op=0x" + op.ToString("X") +
                " rs=" + rs +
                " rt=" + rt +
                " imm=0x" + uimm.ToString("X4") +
                " base=0x" + bas.ToString("X8") +
                " formed=0x" + formed.ToString("X8") +
                " a0=" + GprHex(regs, 4) +
                " a1=" + GprHex(regs, 5) +
                " a2=" + GprHex(regs, 6) +
                " a3=" + GprHex(regs, 7) +
                " v0=" + GprHex(regs, 2) +
                " v1=" + GprHex(regs, 3) +
                " t0=" + GprHex(regs, 8) +
                " t1=" + GprHex(regs, 9) +
                " t2=" + GprHex(regs, 10) +
                " t3=" + GprHex(regs, 11) +
                " t4=" + GprHex(regs, 12) +
                " t5=" + GprHex(regs, 13) +
                " t6=" + GprHex(regs, 14) +
                " t7=" + GprHex(regs, 15) +
                " t8=" + GprHex(regs, 24) +
                " t9=" + GprHex(regs, 25) +
                " s0=" + GprHex(regs, 16) +
                " s1=" + GprHex(regs, 17) +
                " s2=" + GprHex(regs, 18) +
                " s3=" + GprHex(regs, 19) +
                " s4=" + GprHex(regs, 20) +
                " s5=" + GprHex(regs, 21) +
                " s6=" + GprHex(regs, 22) +
                " s7=" + GprHex(regs, 23) +
                " s8=" + GprHex(regs, 30) +
                " gp=" + GprHex(regs, 28) +
                " sp=" + GprHex(regs, 29) +
                " ra=" + GprHex(regs, 31) +
                (_userKPageAlias
                    ? " FFFF5800-alias=on->FFFFD800"
                    : " FFFF5800-alias=off") +
                (ukOk ? " FFFF5800=0x" + uk.ToString("X8") : " FFFF5800-unmapped") +
                (kdOk ? " FFFFD800=0x" + kd.ToString("X8") : " FFFFD800-unmapped") +
                (pgOk ? " FFFFF000=0x" + pg.ToString("X8") : " FFFFF000-unmapped") +
                (mapped != vaddr
                    ? " map-hit=0x" + mapped.ToString("X8")
                    : " no-FFFFF000-alias") +
                " (page 0xFFFFF000 off=0x" +
                (vaddr & 0xFFFu).ToString("X") +
                "; not UserKPage/KData; observe only; do not invent dest)");
            TryResolveFfffF000(bus, vaddr);
        }

        // Live 98db5d5: gwes 0x00021ABC store miss on
        // null. Peek insn / rs / rt / base. One Hive
        // line. Do not map VA 0. Do not invent dest.
        private static void TryNoteGwesNullStoreObserve(MipsBus bus, uint[] regs,
            uint epc)
        {
            if (_gwesNullStoreLogged)
                return;
            _gwesNullStoreLogged = true;
            uint insn = 0;
            string via = "peek-miss";
            if (TryPeekWord(bus, epc, out insn))
                via = "gwes";
            else
            {
                uint rom = GwesRomTextPage(epc);
                if (rom != 0 && TryPeekWord(bus, rom | (epc & 0xFFFu), out insn))
                    via = "rom";
            }
            string dis = via != "peek-miss" ? FormatMipsOp(epc, insn) : "peek-miss";
            uint rs = (insn >> 21) & 31;
            uint rt = (insn >> 16) & 31;
            int simm = (short)(insn & 0xFFFFu);
            uint bas = PeekGpr(regs, (int)rs);
            uint formed = bas + (uint)simm;
            string why;
            if (rs == 0)
                why = "rs0";
            else if (bas == 0)
                why = "base0";
            else if (formed == 0)
                why = "formed0";
            else
                why = "badv=0";
            string extra = via == "gwes" ? "" : " via=" + via;
            BootLog.Write("[Hive] ExtraROM ddi_nop null-store epc=0x" +
                epc.ToString("X8") +
                " insn=0x" + insn.ToString("X8") +
                " " + dis +
                " rs=" + rs +
                " rt=" + rt +
                " base=0x" + bas.ToString("X8") +
                " formed=0x" + formed.ToString("X8") +
                " why=" + why +
                extra +
                " v0=" + GprHex(regs, 2) +
                " (do not map VA 0)");
        }

        private static bool IsNearNullVa(uint va)
        {
            return va < NearNullPageHi;
        }

        // Live 73486bc: kernel 0x80052010 TLBL 0x50.
        // Peek insn / rs / rt / base. One Hive line.
        // Do not map page 0. Do not invent dest.
        private static void TryNoteNearNullTlblObserve(MipsBus bus, uint[] regs,
            uint epc, uint vaddr)
        {
            if (_nearNullTlblLogged)
                return;
            _nearNullTlblLogged = true;
            uint insn = 0;
            string via = "peek-miss";
            if (TryPeekWord(bus, epc, out insn))
                via = "kseg";
            string dis = via != "peek-miss" ? FormatMipsOp(epc, insn) : "peek-miss";
            uint rs = (insn >> 21) & 31;
            uint rt = (insn >> 16) & 31;
            int simm = (short)(insn & 0xFFFFu);
            uint bas = PeekGpr(regs, (int)rs);
            uint formed = bas + (uint)simm;
            string why;
            if (rs == 0)
                why = "rs0";
            else if (bas == 0)
                why = "base0";
            else if (IsNearNullVa(formed))
                why = "formed0";
            else
                why = "page0";
            string extra = via == "kseg" ? "" : " via=" + via;
            BootLog.Write("[Hive] ExtraROM ddi_nop near-null epc=0x" +
                epc.ToString("X8") +
                " insn=0x" + insn.ToString("X8") +
                " " + dis +
                " rs=" + rs +
                " rt=" + rt +
                " base=0x" + bas.ToString("X8") +
                " formed=0x" + formed.ToString("X8") +
                " why=" + why +
                extra +
                " v0=" + GprHex(regs, 2) +
                " (do not map page 0)");
        }

        // Live f3c2d62 skipped only FFFF* AdEL.
        // Live 3ac5ed9: cause=4 epc=badvaddr=
        // 0xC6FA7C9A is also AdEL (unaligned
        // corrupt PC). Skip all AdEL and
        // epc==badvaddr unaligned. Do not map.
        private static bool IsAdelSkip(uint code, uint epc, uint va)
        {
            if (code == 4)
                return true;
            return epc == va && (epc & 3) != 0;
        }

        // Live f3c2d62: AdEL epc=badvaddr=0xFFFFFB2A.
        // Peek insn if mapped. One Hive line. Do not
        // map 0xFFFFF000. Do not invent dest.
        private static void TryNoteFfffFb2aAdelObserve(MipsBus bus, uint[] regs,
            uint epc, uint vaddr)
        {
            if (_ffffFb2aAdelLogged)
                return;
            _ffffFb2aAdelLogged = true;
            uint insn = 0;
            bool peeked = TryPeekWord(bus, epc, out insn);
            string dis = peeked ? FormatMipsOp(epc, insn) : "peek-miss";
            string why = (epc & 3) != 0 ? "unaligned" : (peeked ? "adel" : "unmapped");
            BootLog.Write("[Hive] ExtraROM ddi_nop adel-ffff epc=0x" +
                epc.ToString("X8") +
                " badvaddr=0x" + vaddr.ToString("X8") +
                " insn=" + (peeked ? "0x" + insn.ToString("X8") : "peek-miss") +
                (peeked ? " " + dis : "") +
                " why=" + why +
                " a1=" + GprHex(regs, 5) +
                " v0=" + GprHex(regs, 2) +
                " v1=" + GprHex(regs, 3) +
                " (AdEL; do not map 0xFFFFF000)");
        }

        // Live 3ac5ed9 / fb58a7e: AdEL epc=badvaddr=
        // 0xC6FA7C9A (unaligned I-fetch). Dump:
        // leftover 0x800159A8 jal 0x800397B0 returns
        // *(0x8033FD50), then 0x80015A08 mtc0 EPC.
        // 0xC6FA7C9A = 0x86FA7C9A|0x40000000. Keep
        // adel-pc $sp (not C2; nested nest!=1). Do
        // not map that VA. Do not invent dest.
        private static void TryNoteAdelC6FaObserve(MipsBus bus, uint[] regs,
            uint epc, uint vaddr)
        {
            if (_adelC6FaLogged)
                return;
            _adelC6FaLogged = true;
            _adelPcEpc = epc;
            _adelPcSp = PeekGpr(regs, 29);
            uint plant = 0;
            TryPeekWord(bus, ExnContinueWord, out plant);
            _exnContinueWord = plant;
            uint insn = 0;
            bool peeked = TryPeekWord(bus, epc, out insn);
            string dis = peeked ? FormatMipsOp(epc, insn) : "peek-miss";
            string why;
            if ((epc & 3) != 0)
                why = "unaligned";
            else if (epc == vaddr)
                why = "corrupt-pc";
            else
                why = peeked ? "adel" : "unmapped";
            BootLog.Write("[Hive] ExtraROM ddi_nop adel-pc epc=0x" +
                epc.ToString("X8") +
                " badvaddr=0x" + vaddr.ToString("X8") +
                " insn=" + (peeked ? "0x" + insn.ToString("X8") : "peek-miss") +
                (peeked ? " " + dis : "") +
                " why=" + why +
                " sp=0x" + _adelPcSp.ToString("X8") +
                " a1=" + GprHex(regs, 5) +
                " v0=" + GprHex(regs, 2) +
                " (AdEL; do not map)");
        }

        private static bool IsC2TlbsVa(uint code, uint va)
        {
            return code == 3 && (va & 0xFF000000u) == C2VaPrefix;
        }

        private static bool IsC2Sp(uint sp)
        {
            return (sp & 0xFF000000u) == C2VaPrefix;
        }

        // Live 7827498: $sp=0xC201FE48 at sw ra,60(sp).
        // Dump 0x80031D34 is a kernel prologue, not a
        // missing page. Log the first C2 $sp (pc/sp/ra
        // /a1). Slot 97 image is not a stack. Do not
        // map. Do not invent dest.
        private static void TryNoteC2SpObserve(uint[] regs, uint pc)
        {
            if (_c2SpLogged)
                return;
            if (!_ddiNopAwaitCallDll)
                return;
            if (!_ddiNopDllMainLogged && _ddiNopIatStoreN < BindImpObserveMax)
                return;
            uint sp = PeekGpr(regs, 29);
            if (!IsC2Sp(sp))
                return;
            _c2SpLogged = true;
            uint low = sp & 0x01FFFFFFu;
            BootLog.Write("[Hive] ExtraROM ddi_nop sp-c2 pc=0x" +
                pc.ToString("X8") +
                " sp=0x" + sp.ToString("X8") +
                " low=0x" + low.ToString("X8") +
                " ra=" + GprHex(regs, 31) +
                " a1=" + GprHex(regs, 5) +
                " (NK slot97; not a stack; do not invent dest)");
        }

        private static bool IsC2ImageSp(uint sp)
        {
            return IsC2Sp(sp) && (sp & 0x01FFFFFFu) < C2SlotImageHi;
        }

        private static bool IsAdelPoisonEpc(uint pc)
        {
            if (pc == AdelC6FaEpc || (pc & 0xFF000000u) == 0xC6000000u)
                return true;
            return pc != 0 && (pc & 3) != 0;
        }

        private static bool IsSaneNkResumePc(uint pc)
        {
            return (pc & 3) == 0 && pc >= 0x80010000u && pc < NkImageEnd;
        }

        // Live 3b847b7 / 695e734: 0x800373C0 is
        // mid NK idle (jal 0x80031D34). Live
        // ac46757: +5C=0x800356FC is that
        // thread's start (dump jal 0x80031D34).
        // Not a leftover resume. Do not leftover
        // hop.
        private static bool IsNkIdleResumePc(uint pc)
        {
            return pc == NkIdleJal || pc == C2TlbsFunc
                || pc == NkIdleStart;
        }

        // Live e3cc519: +EC=0x80015B9C is leftover
        // mid (ExnAfterFetch2; dump addiu $sp,-304
        // then jal 0x80020FA0), not an adel resume.
        // plant-clr of that leftover then later
        // +EC idle. leftover dest / adel-pc /
        // near-null / NK idle / leftover mid are
        // not a resume. Do not invent dest.
        private static bool IsSaneAdelResumePc(uint pc)
        {
            if ((pc & 3) != 0 || IsAdelPoisonEpc(pc) || IsPoisonPlant(pc)
                || IsNearNullVa(pc) || IsLeftoverDestVa(pc)
                || IsNkIdleResumePc(pc) || pc == ExnAfterFetch
                || pc == ExnAfterFetch2)
                return false;
            if (IsSaneNkResumePc(pc))
                return true;
            if (IsC2ImageSp(pc))
                return false;
            return pc >= 0x00010000u && pc < 0x80000000u;
        }

        private static bool IsSaneReplaySp(uint sp)
        {
            if (sp == 0 || (sp & 3) != 0)
                return false;
            if (IsNearNullVa(sp) || IsC2ImageSp(sp))
                return false;
            if ((sp & 0xFF000000u) == 0xC6000000u)
                return false;
            if (sp >= 0xFFFFD000u && sp < 0xFFFFE000u)
                return true;
            if (sp >= 0x80000000u && sp < 0xC0000000u)
                return true;
            return sp >= 0x00010000u && sp < 0x80000000u;
        }

        // Live 155d918 / fb58a7e: adel-pc then
        // 0x80015664 $sp=0xC201FE88 from thread
        // +0xD4. Nested AdEL (0x80015488) does
        // not update +0xD4, so ERET2 reloads the
        // ThreadContextSetup / +0x2C-48 image
        // cookie. Replay adel-pc $sp into +0xD4
        // when +0xEC is a sane aligned NK PC
        // (firmware's own first-level save at
        // 0x80015264). Else refuse ERET. Do not
        // hop EPC to 0x80030264. Do not invent
        // dest. Do not map 0xC201F000.
        // Live cf2477b: after that replay, $sp
        // is adel-pc slot-2 (not C2) and live
        // PeekEpc is already rewritten. Refuse
        // ERET while the adel-pc latch is set,
        // even when live EPC is not C6FA. Observe
        // latch/live/+EC/pc. Do not leftover hop.
        // Live aa0b26c: +D4 is slot-2 (not C2);
        // leftover mtc0 / 0x800152CC left
        // EPC and +DC as adel-pc. 0x8001563C
        // already lw $ra,220($s0). Replay +EC
        // into EPC / +DC / $ra when +EC is a
        // sane aligned NK/useg PC, then clear
        // the latch. Do not invent dest.
        // Live 3b847b7: after that clear, later
        // C2 $sp sp-fix +EC=0x800373C0 idle.
        // Refuse that ERET (idle-halt). Do not
        // leftover hop. Do not invent dest.
        // Live e3cc519: plant-clr of leftover
        // mid 0x80015B9C cleared the latch;
        // later +EC idle. Keep latch / epc-halt
        // when +EC is leftover mid. Do not
        // invent dest.
        public static bool TryRefuseC2SpResume(MipsBus bus, uint[] regs,
            ref uint programCounter)
        {
            if (programCounter != C2SpFirstPc
                && programCounter != ThreadCtxRestore2
                && programCounter != ThreadCtxEret)
                return false;
            if (!_ddiNopAwaitCallDll)
                return false;
            if (!_ddiNopDllMainLogged && _ddiNopIatStoreN < BindImpObserveMax)
                return false;
            if (_adelPcEpc == 0 && !_adelC6FaLogged
                && !_nearNullTlblLogged && !_c2SpLogged)
                return false;
            uint sp = PeekGpr(regs, 29);
            bool c2 = IsC2ImageSp(sp);
            if (!c2 && _adelPcEpc == 0)
                return false;
            uint d4 = 0;
            uint t24 = 0;
            uint t2c = 0;
            uint ec = 0;
            uint dc = 0;
            uint plant = _exnContinueWord;
            uint thr = 0;
            if (bus != null)
            {
                try
                {
                    thr = bus.Read32(ThreadPtr);
                    if (thr != 0)
                    {
                        d4 = bus.Read32(thr + ThreadCtxSp);
                        t24 = bus.Read32(thr + ThreadStack);
                        t2c = bus.Read32(thr + ThreadStackAlt);
                        ec = bus.Read32(thr + ThreadCtxPc);
                        dc = bus.Read32(thr + ThreadCtxRa);
                    }
                    uint word;
                    if (TryPeekWord(bus, ExnContinueWord, out word))
                        plant = word;
                }
                catch
                {
                }
            }
            if (!_thrSpLogged)
            {
                _thrSpLogged = true;
                BootLog.Write("[Hive] ExtraROM ddi_nop thr-sp +D4=0x" +
                    d4.ToString("X8") +
                    " +24=0x" + t24.ToString("X8") +
                    " +2C=0x" + t2c.ToString("X8") +
                    " +EC=0x" + ec.ToString("X8") +
                    " +DC=0x" + dc.ToString("X8") +
                    " adel-sp=0x" + _adelPcSp.ToString("X8") +
                    " plant=0x" + plant.ToString("X8"));
            }
            bool fixedSp = false;
            if (c2 && thr != 0 && bus != null
                && IsSaneReplaySp(_adelPcSp) && IsSaneNkResumePc(ec))
            {
                try
                {
                    bus.Write32(thr + ThreadCtxSp, _adelPcSp);
                    if (regs != null && regs.Length > 29)
                        regs[29] = _adelPcSp;
                    fixedSp = true;
                }
                catch
                {
                    if (!_c2EretHaltLogged)
                    {
                        _c2EretHaltLogged = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop eret-c2-halt pc=0x" +
                            programCounter.ToString("X8") +
                            " sp=0x" + sp.ToString("X8") +
                            " ra=" + GprHex(regs, 31) +
                            " +EC=0x" + ec.ToString("X8") +
                            " (refuse ERET after adel-pc; do not invent dest)");
                    }
                    return true;
                }
                if (!_spFixLogged)
                {
                    _spFixLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop sp-fix +D4=0x" +
                        sp.ToString("X8") +
                        " to=0x" + _adelPcSp.ToString("X8") +
                        " +EC=0x" + ec.ToString("X8") +
                        " (replay adel-pc $sp; do not invent dest)");
                }
            }
            if (_adelPcEpc != 0)
            {
                uint live = 0;
                if (bus != null)
                    live = bus.PeekEpc();
                if (IsSaneAdelResumePc(ec))
                {
                    bool cleared = true;
                    if (bus != null)
                        bus.PokeEpc(ec);
                    if (thr != 0 && bus != null && IsAdelPoisonEpc(dc))
                    {
                        try
                        {
                            bus.Write32(thr + ThreadCtxRa, ec);
                        }
                        catch
                        {
                            cleared = false;
                        }
                    }
                    if (cleared && regs != null && regs.Length > 31
                        && IsAdelPoisonEpc(regs[31]))
                        regs[31] = ec;
                    if (cleared)
                    {
                        if (!_adelPlantClrLogged)
                        {
                            _adelPlantClrLogged = true;
                            BootLog.Write("[Hive] ExtraROM ddi_nop plant-clr latch=0x" +
                                _adelPcEpc.ToString("X8") +
                                " live=0x" + live.ToString("X8") +
                                " +DC=0x" + dc.ToString("X8") +
                                " +EC=0x" + ec.ToString("X8") +
                                " (replay +EC; do not invent dest)");
                        }
                        _adelPcEpc = 0;
                        return false;
                    }
                }
                if (!_epcHaltLogged)
                {
                    _epcHaltLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop epc-halt latch=0x" +
                        _adelPcEpc.ToString("X8") +
                        " live=0x" + live.ToString("X8") +
                        " +EC=0x" + ec.ToString("X8") +
                        " pc=0x" + programCounter.ToString("X8") +
                        " (refuse ERET; adel-pc latch; do not invent dest)");
                }
                return true;
            }
            if (IsNkIdleResumePc(ec))
            {
                if (!_idleHaltLogged)
                {
                    _idleHaltLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop idle-halt +EC=0x" +
                        ec.ToString("X8") +
                        " pc=0x" + programCounter.ToString("X8") +
                        " (refuse ERET; NK idle poll; do not invent dest)");
                }
                return true;
            }
            if (fixedSp || !c2)
                return false;
            if (!_c2EretHaltLogged)
            {
                _c2EretHaltLogged = true;
                BootLog.Write("[Hive] ExtraROM ddi_nop eret-c2-halt pc=0x" +
                    programCounter.ToString("X8") +
                    " sp=0x" + sp.ToString("X8") +
                    " ra=" + GprHex(regs, 31) +
                    " +EC=0x" + ec.ToString("X8") +
                    " (refuse ERET after adel-pc; do not invent dest)");
            }
            return true;
        }

        private static bool IsDdiNopDestLive()
        {
            return _ddiNopDllMainLogged || _ddiNopDestWordLogged;
        }

        private static bool IsPoisonPlant(uint pc)
        {
            if (pc == 0 || pc == 0xFFFFFFFFu)
                return true;
            if ((pc & 3) != 0)
                return true;
            return (pc & 0xFF000000u) == 0xC6000000u;
        }

        private static bool IsLeftoverDestVa(uint pc)
        {
            return pc >= LeftoverDestLo && pc < LeftoverDestHi;
        }

        // Live 8d10132: +EC mid 0x80038294 / +DC
        // mid 0x8003B04C. plant-fix $ra to that
        // PC then jr $ra spins. Not a leftover
        // resume. Do not invent dest.
        private static bool IsPoisonMidPlantResume(uint pc)
        {
            if (pc >= HandleLookupJal && pc < HandleLookupEnd)
                return true;
            return pc >= HandleLookupRet && pc < HandleLookupRetEnd;
        }

        private static bool IsSanePlantResumePc(uint pc)
        {
            if ((pc & 3) != 0 || IsPoisonPlant(pc) || IsNearNullVa(pc))
                return false;
            if (IsLeftoverDestVa(pc) || IsNkIdleResumePc(pc)
                || IsPoisonMidPlantResume(pc))
                return false;
            if (pc == LeftoverOrRa || pc == LeftoverMtc0Epc
                || pc == LeftoverJrRa || pc == LeftoverEret
                || pc == ExnAfterFetch || pc == ExnAfterFetch2
                || pc == ThreadCtxRestore || pc == ThreadCtxRestore2
                || pc == C2SpFirstPc || pc == 0x800397B0u)
                return false;
            if (pc >= 0x80010000u && pc < NkImageEnd)
                return true;
            return pc >= 0x00010000u && pc < 0x80000000u;
        }

        private static bool TryPeekThreadCtxPc(MipsBus bus, out uint thr, out uint ec,
            out uint dc, out uint plant)
        {
            thr = 0;
            ec = 0;
            dc = 0;
            plant = _exnContinueWord;
            if (bus == null)
                return false;
            try
            {
                thr = bus.Read32(ThreadPtr);
                if (thr != 0)
                {
                    ec = bus.Read32(thr + ThreadCtxPc);
                    dc = bus.Read32(thr + ThreadCtxRa);
                }
                uint word;
                if (TryPeekWord(bus, ExnContinueWord, out word))
                    plant = word;
                return thr != 0;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyPlantResume(uint[] regs, uint pc, uint dest)
        {
            if (regs == null || regs.Length <= 31)
                return;
            if (pc == LeftoverOrRa || pc == LeftoverEret)
                regs[2] = dest;
            if (pc == LeftoverMtc0Epc || pc == LeftoverEret || pc == LeftoverJrRa)
            {
                regs[12] = dest;
                regs[31] = dest;
            }
            if (pc == LeftoverEret)
                regs[2] = dest;
        }

        // Live f66919d: adel-pc gone; ddi_nop dest live;
        // leftover eret-restore was=0xFFFFFFFF. Dump
        // 0x800397B0 returns $s3; 0x800399A4 load of
        // *(0x8033FD50) is skipped by beq/bne to
        // 0x800399A8 so $s3 stays -1. leftover hop to
        // dest-live then ~2.8M Code-10. Replay
        // thread+0xEC when that is a sane aligned PC.
        // Else refuse leftover ERET. Do not leftover
        // dest hop. Do not invent dest.
        // Live 77ba8c0: adel-pc then epc-halt; +EC
        // leftover mid is not a resume. Dump wait99:
        // 0x800159B4 or $ra,$v0; 0x80015A08 mtc0
        // $t4,$14; user ERET / kernel jr $ra plants
        // that return. This Boot plant=0x03F74844
        // (leftover dest). ERET there then unaligned
        // I-fetch 0xC6FA7C9A (0x86FA7C9A|0x40000000).
        // +EC leftover mid / idle are not a resume.
        // Refuse leftover ERET when $v0/$t4/$ra is
        // leftover dest unless thread+0xEC is a
        // sane aligned NK/useg PC. Live 0332c87:
        // leftover-halt was=0x03F71740 +EC=
        // 0x800382F8 during NK coredll LoadO32.
        // Live 8d10132: plant-fix to that +EC
        // hung (jr $ra loop). Dump: +EC is mid
        // jal 0x80038294; +DC=0x8003B05C is the
        // jal return. Neither is a leftover
        // resume. leftover-halt when +EC is that
        // poison mid. Live 3d6387d: leftover-halt
        // again; plant=0x03F74844 leftover dest.
        // Dump leftover 0x800159A4 sw $v0,16($sp)
        // then jal 0x800397B0; 0x800397F8 lw
        // $s3,4(thread+0x18); 0x800399A4 may
        // skip FD50; 0x8001597C sw $ra,40($sp).
        // Observe those slots. Live ac46757:
        // leftover-frame +5C=0x800356FC is NK
        // idle start (jal 0x80031D34), not a
        // LoadO32 continue. +18/v016=0;
        // FD50/ra40 leftover dest. leftover-halt
        // stays. Live ee3e1af: leftover dest
        // $v0 at wait99 or $ra,$v0 plants
        // coredll mid-hash (0x03F71740). Dump
        // 0x800397B0 returns $s3 from frame+4.
        // Skip that or; leave $ra. leftover-halt
        // if dest later $t4/$ra. Do not leftover
        // hop. Do not invent dest.
        public static bool TryRefuseMinusOnePlant(MipsBus bus, uint[] regs,
            ref uint programCounter)
        {
            uint pc = programCounter;
            if (pc != LeftoverOrRa && pc != LeftoverMtc0Epc
                && pc != LeftoverJrRa && pc != LeftoverEret)
                return false;
            if (regs == null || regs.Length <= 31)
                return false;
            uint was = pc == LeftoverOrRa || pc == LeftoverEret
                ? regs[2]
                : (pc == LeftoverMtc0Epc ? regs[12] : regs[31]);
            bool adel = IsAdelPoisonEpc(was);
            bool destPlant = IsLeftoverDestVa(was);
            if (!adel && !destPlant && !(IsDdiNopDestLive() && IsPoisonPlant(was)))
                return false;
            uint thr;
            uint ec;
            uint dc;
            uint plant;
            TryPeekThreadCtxPc(bus, out thr, out ec, out dc, out plant);
            if (adel)
            {
                if (!_epcHaltLogged)
                {
                    _epcHaltLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop epc-halt was=0x" +
                        was.ToString("X8") +
                        " +EC=0x" + ec.ToString("X8") +
                        " plant=0x" + plant.ToString("X8") +
                        " (refuse leftover ERET adel-pc; do not invent dest)");
                }
                return true;
            }
            if (pc == LeftoverOrRa && destPlant)
            {
                programCounter = pc + 4;
                if (!_leftoverSkipLogged)
                {
                    _leftoverSkipLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop leftover-skip was=0x" +
                        was.ToString("X8") +
                        " ra=0x" + regs[31].ToString("X8") +
                        " (leave $ra; refuse leftover dest)");
                }
                return true;
            }
            if (destPlant && !IsSanePlantResumePc(ec))
            {
                TryNoteLeftoverFrameObserve(bus, regs, plant);
                if (!_leftoverHaltLogged)
                {
                    _leftoverHaltLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop leftover-halt was=0x" +
                        was.ToString("X8") +
                        " +EC=0x" + ec.ToString("X8") +
                        " +DC=0x" + dc.ToString("X8") +
                        " plant=0x" + plant.ToString("X8") +
                        " (refuse leftover ERET dest; do not invent dest)");
                }
                return true;
            }
            if (IsSanePlantResumePc(ec))
            {
                ApplyPlantResume(regs, pc, ec);
                if (!_plantFixLogged)
                {
                    _plantFixLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop plant-fix was=0x" +
                        was.ToString("X8") +
                        " +EC=0x" + ec.ToString("X8") +
                        " +DC=0x" + dc.ToString("X8") +
                        " plant=0x" + plant.ToString("X8") +
                        " (replay thread+0xEC; do not leftover dest)");
                }
                return false;
            }
            if (!_plantHaltLogged)
            {
                _plantHaltLogged = true;
                BootLog.Write("[Hive] ExtraROM ddi_nop plant-halt was=0x" +
                    was.ToString("X8") +
                    " +EC=0x" + ec.ToString("X8") +
                    " +DC=0x" + dc.ToString("X8") +
                    " plant=0x" + plant.ToString("X8") +
                    " (refuse leftover ERET; do not invent dest)");
            }
            return true;
        }

        // Live 3d6387d leftover-halt during NK
        // coredll LoadO32. Dump leftover:
        // 0x800158D4 lw thread+0x18; 0x800397F8
        // lw $s3,4($a0); 0x800399A4 lw FD50;
        // 0x800159A4 sw $v0,16($sp); 0x8001597C
        // sw $ra,40($sp). +5C startip. +F0 is
        // 3 kernel / 0x13 user. One Hive line.
        // Do not leftover hop. Do not invent dest.
        private static void TryNoteLeftoverFrameObserve(MipsBus bus, uint[] regs,
            uint plant)
        {
            if (_leftoverFrameLogged)
                return;
            _leftoverFrameLogged = true;
            uint frame = 0;
            uint frame4 = 0;
            uint fd50 = plant;
            uint ra40 = 0;
            uint v016 = 0;
            uint startip = 0;
            uint sr = 0;
            uint thr = 0;
            if (TryPeekWord(bus, ThreadPtr, out thr) && thr != 0
                && thr != 0xFFFFFFFFu)
            {
                TryPeekWord(bus, thr + ThreadSyscallFrame, out frame);
                TryPeekWord(bus, thr + ThreadStartip, out startip);
                TryPeekWord(bus, thr + ThreadCtxSr, out sr);
                if (frame != 0 && frame != 0xFFFFFFFFu)
                    TryPeekWord(bus, frame + 4, out frame4);
            }
            uint word;
            if (TryPeekWord(bus, ExnContinueWord, out word))
                fd50 = word;
            uint sp = PeekGpr(regs, 29);
            if (sp != 0 && sp != 0xFFFFFFFFu)
            {
                TryPeekWord(bus, sp + 16, out v016);
                TryPeekWord(bus, sp + 40, out ra40);
            }
            BootLog.Write("[Hive] ExtraROM ddi_nop leftover-frame +18=0x" +
                frame.ToString("X8") +
                " +4=0x" + frame4.ToString("X8") +
                " FD50=0x" + fd50.ToString("X8") +
                " ra40=0x" + ra40.ToString("X8") +
                " v016=0x" + v016.ToString("X8") +
                " +5C=0x" + startip.ToString("X8") +
                " +F0=0x" + sr.ToString("X8") +
                " (do not invent dest)");
        }

        // Live 3275fe9: kernel 0x80031D38 TLBS
        // 0xC201FE84. Peek insn / rs / rt / base.
        // a1 is nk ROM evidence, not a hop. One
        // Hive line. Do not invent dest. Do not
        // map 0xC2xxxxxx.
        private static void TryNoteC2TlbsObserve(MipsBus bus, uint[] regs,
            uint epc, uint vaddr)
        {
            if (_c2TlbsLogged)
                return;
            _c2TlbsLogged = true;
            uint insn = 0;
            string via = "peek-miss";
            if (TryPeekWord(bus, epc, out insn))
                via = "kseg";
            string dis = via != "peek-miss" ? FormatMipsOp(epc, insn) : "peek-miss";
            uint rs = (insn >> 21) & 31;
            uint rt = (insn >> 16) & 31;
            int simm = (short)(insn & 0xFFFFu);
            uint bas = PeekGpr(regs, (int)rs);
            uint formed = bas + (uint)simm;
            string extra = via == "kseg" ? "" : " via=" + via;
            BootLog.Write("[Hive] ExtraROM ddi_nop c2-tlbs epc=0x" +
                epc.ToString("X8") +
                " va=0x" + vaddr.ToString("X8") +
                " insn=" + (via != "peek-miss" ? "0x" + insn.ToString("X8") : "peek-miss") +
                (via != "peek-miss" ? " " + dis : "") +
                " rs=" + rs +
                " rt=" + rt +
                " base=0x" + bas.ToString("X8") +
                " formed=0x" + formed.ToString("X8") +
                extra +
                " a1=" + GprHex(regs, 5) +
                " (TLBS; do not invent dest)");
        }

        private static void TryNoteBindImpExnSave(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_ddiNopAwaitCallDll || !_ddiNopIatStoreLogged)
                return;
            if (pc < BindImpExnLo || pc > BindImpExnHi)
                return;
            if (_bindImpExnSaveLogged)
                return;
            if (IsGwesDataB9Page(_bindImpExnVaddr))
                return;
            if (IsFilesysSlot2ExtraPage(_bindImpExnVaddr))
                return;
            if (IsDdiNopCoredllImageVa(_bindImpExnVaddr))
                return;
            if (IsNearNullVa(_bindImpExnVaddr))
                return;
            if (IsAdelSkip(_bindImpExnCode, _bindImpExnEpc, _bindImpExnVaddr))
                return;
            if (IsC2TlbsVa(_bindImpExnCode, _bindImpExnVaddr))
                return;
            _bindImpExnSaveLogged = true;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            BootLog.Write("[Hive] ExtraROM BindImp-exn save pc=0x" +
                pc.ToString("X8") +
                " cause=" + _bindImpExnCode +
                " epc=0x" + _bindImpExnEpc.ToString("X8") +
                " badvaddr=0x" + _bindImpExnVaddr.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " stores=" + _ddiNopIatStoreN);
        }

        // Live edf15b0: after stores=24, coredll
        // 0x03F6C908 lw $v0,0($s5) TLBL on 0x01FFFCA4.
        // Observe $s5 / mappedness, then demand-map the
        // process-info page via firmware PTE, KData keep,
        // or a zero valloc host page. Do not invent heap.
        private static void TryNoteDdiNopProcessInfo(MipsBus bus, uint[] regs)
        {
            if (_ddiNopInfoObserved || !_ddiNopAwaitCallDll)
                return;
            if (_ddiNopIatStoreN < BindImpObserveMax && !_ddiNopSawCallDllPc)
                return;
            _ddiNopInfoObserved = true;
            uint s5 = regs != null && regs.Length > 21 ? regs[21] : 0;
            uint word = 0;
            bool mapped = false;
            _ddiNopInfoPeekRaw = true;
            try
            {
                mapped = TryPeekWord(bus, ProcessInfoFaultVa, out word);
            }
            finally
            {
                _ddiNopInfoPeekRaw = false;
            }
            BootLog.Write("[Hive] ExtraROM ddi_nop proc-info s5=0x" +
                s5.ToString("X8") +
                " va=0x" + ProcessInfoFaultVa.ToString("X8") +
                (mapped ? " mapped" : " unmapped") +
                " word=0x" + word.ToString("X8") +
                " stores=" + _ddiNopIatStoreN);
            TryResolveDdiNopProcessInfo(bus);
        }

        private static bool IsDdiNopProcessInfoArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            if (_ddiNopInfoDemand || _ddiNopSawCallDllPc)
                return true;
            return _ddiNopIatStoreN >= BindImpObserveMax;
        }

        public static uint MapDdiNopProcessInfoVa(MipsBus bus, uint va)
        {
            if (_ddiNopInfoPeekRaw || _ddiNopInfoBusy)
                return va;
            if (!IsDdiNopProcessInfoArmed())
                return va;
            if (va < ProcessInfoPage || va >= 0x02000000u)
                return va;
            if (_ddiNopInfoKseg != 0)
                return _ddiNopInfoKseg | (va & 0xFFFu);
            TryResolveDdiNopProcessInfo(bus);
            if (_ddiNopInfoKseg != 0)
                return _ddiNopInfoKseg | (va & 0xFFFu);
            return va;
        }

        private static void TryResolveDdiNopProcessInfo(MipsBus bus)
        {
            if (_ddiNopInfoKseg != 0 || _ddiNopInfoBusy || bus == null)
                return;
            try
            {
                _ddiNopInfoBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, ProcessInfoFaultVa,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopInfoKseg = kseg & ~0xFFFu;
                    if (!_ddiNopInfoMapLogged)
                    {
                        _ddiNopInfoMapLogged = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop proc-info map va=0x" +
                            ProcessInfoPage.ToString("X8") +
                            " -> 0x" + _ddiNopInfoKseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " (firmware PTE; same page as *0x01FFFFA0; do not invent heap bytes)");
                    }
                    return;
                }
                uint kdata = (KDataBase & ~0xFFFu) | (ProcessInfoFaultVa & 0xFFFu);
                uint word = 0;
                if (TryPeekWord(bus, kdata, out word))
                {
                    _ddiNopInfoKseg = KDataBase & ~0xFFFu;
                    if (!_ddiNopInfoMapLogged)
                    {
                        _ddiNopInfoMapLogged = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop proc-info map va=0x" +
                            ProcessInfoPage.ToString("X8") +
                            " -> 0x" + _ddiNopInfoKseg.ToString("X8") +
                            " (KData keep; same page as UserKPage alias; do not invent heap bytes)");
                    }
                    return;
                }
                if (TryHostBackProcessInfoPage() && !_ddiNopInfoMapLogged)
                {
                    _ddiNopInfoMapLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop proc-info map va=0x" +
                        ProcessInfoPage.ToString("X8") +
                        " -> 0x" + _ddiNopInfoKseg.ToString("X8") +
                        " (zero page; existing valloc host; do not invent heap bytes)");
                }
            }
            finally
            {
                _ddiNopInfoBusy = false;
            }
        }

        // Live 6b8a9eb: I-fetch TLBL at 0x0005D2E0 after
        // DllMain. Same page as GwesVaDispAlloc 0x0005D250.
        // Demand-map via firmware PTE only. Do not invent
        // dest / 0x03FAD2E0 / zero code bytes.
        public static uint MapDdiNopGwesDispFetchVa(MipsBus bus, uint va)
        {
            if (_ddiNopGwesFetchBusy)
                return va;
            if (!IsDdiNopGwesDispFetchArmed())
                return va;
            if ((va & ~0xFFFu) != GwesDispFetchPage)
                return va;
            if (_ddiNopGwesFetchKseg != 0)
                return _ddiNopGwesFetchKseg | (va & 0xFFFu);
            TryResolveDdiNopGwesDispFetch(bus);
            if (_ddiNopGwesFetchKseg != 0)
                return _ddiNopGwesFetchKseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesDispFetchArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesFetchDemand;
        }

        private static void TryNoteDdiNopGwesDispFetchTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesFetchDemand = true;
            if (!_ddiNopGwesFetchTlblLogged)
            {
                _ddiNopGwesFetchTlblLogged = true;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop fetch-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " dllmain-ra=0x" + _ddiNopDllMainRa.ToString("X8") +
                    " (I-fetch; gwes Display page 0x0005D000; not COREDLL 0x03FAD2E0)");
            }
            TryResolveDdiNopGwesDispFetch(bus);
        }

        private static void TryResolveDdiNopGwesDispFetch(MipsBus bus)
        {
            if (_ddiNopGwesFetchKseg != 0 || _ddiNopGwesFetchBusy || bus == null)
                return;
            try
            {
                _ddiNopGwesFetchBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesDispFetchFault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesFetchKseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesFetchLogged)
                    {
                        _ddiNopGwesFetchLogged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesDispFetchFault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp map va=0x" +
                            GwesDispFetchPage.ToString("X8") +
                            " -> 0x" + _ddiNopGwesFetchKseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; GwesVaDispAlloc page; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesFetchLogged)
                {
                    _ddiNopGwesFetchLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp map va=0x" +
                        GwesDispFetchPage.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (I-fetch 0x0005D2E0; do not invent dest or 0x03FAD2E0)");
                }
            }
            finally
            {
                _ddiNopGwesFetchBusy = false;
            }
        }

        // Live 4f43fe4: data TLBL epc=0x0005D310
        // badvaddr=0x000B6008 (GwesIatGetProc). Demand-map
        // that IAT page via firmware PTE only. Do not
        // invent dest-word or zero-fill.
        public static uint MapDdiNopGwesDispDataVa(MipsBus bus, uint va)
        {
            if (_ddiNopGwesDataBusy)
                return va;
            if (!IsDdiNopGwesDispDataArmed())
                return va;
            if ((va & ~0xFFFu) != GwesDispDataPage)
                return va;
            if (_ddiNopGwesDataKseg != 0)
                return _ddiNopGwesDataKseg | (va & 0xFFFu);
            TryResolveDdiNopGwesDispData(bus);
            if (_ddiNopGwesDataKseg != 0)
                return _ddiNopGwesDataKseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesDispDataArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesDataDemand;
        }

        private static void TryNoteDdiNopGwesDispDataTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesDataDemand = true;
            if (!_ddiNopGwesDataTlblLogged)
            {
                _ddiNopGwesDataTlblLogged = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop data-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (GwesIatGetProc; gwes Display IAT page; do not invent dest)");
            }
            TryResolveDdiNopGwesDispData(bus);
        }

        private static void TryResolveDdiNopGwesDispData(MipsBus bus)
        {
            if (_ddiNopGwesDataKseg != 0 || _ddiNopGwesDataBusy || bus == null)
                return;
            try
            {
                _ddiNopGwesDataBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesDispDataFault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesDataKseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesDataLogged)
                    {
                        _ddiNopGwesDataLogged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesDispDataFault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data map va=0x" +
                            GwesDispDataPage.ToString("X8") +
                            " -> 0x" + _ddiNopGwesDataKseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; GwesIatGetProc; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesDataLogged)
                {
                    _ddiNopGwesDataLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data map va=0x" +
                        GwesDispDataPage.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (data TLBL 0x000B6008; do not invent dest)");
                }
            }
            finally
            {
                _ddiNopGwesDataBusy = false;
            }
        }

        // Live 8623be5: NK 0x80020174 data-TLBL on
        // 0x00011C10. First gwes .text page (VA
        // 0x00011000 / FILESYS API 0x000111A8).
        // Firmware PTE only. Do not invent dest or
        // steal sipcfg 0x00011000.
        public static uint MapDdiNopGwesTextBaseVa(MipsBus bus, uint va)
        {
            if (_ddiNopGwesTextBusy)
                return va;
            if (!IsDdiNopGwesTextBaseArmed())
                return va;
            if ((va & ~0xFFFu) != GwesTextBasePage)
                return va;
            if (_ddiNopGwesTextKseg != 0)
                return _ddiNopGwesTextKseg | (va & 0xFFFu);
            TryResolveDdiNopGwesTextBase(bus);
            if (_ddiNopGwesTextKseg != 0)
                return _ddiNopGwesTextKseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesTextBaseArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesTextDemand;
        }

        private static void TryNoteDdiNopGwesTextBaseTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesTextDemand = true;
            if (!_ddiNopGwesTextTlblLogged)
            {
                _ddiNopGwesTextTlblLogged = true;
                uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop text-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " a1=0x" + a1.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " (nk near ThreadContextSetup; gwes .text 0x00011000; do not invent dest)");
            }
            TryResolveDdiNopGwesTextBase(bus);
        }

        private static void TryResolveDdiNopGwesTextBase(MipsBus bus)
        {
            if (_ddiNopGwesTextKseg != 0 || _ddiNopGwesTextBusy || bus == null)
                return;
            try
            {
                _ddiNopGwesTextBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesTextBaseFault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesTextKseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesTextLogged)
                    {
                        _ddiNopGwesTextLogged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesTextBaseFault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text map va=0x" +
                            GwesTextBasePage.ToString("X8") +
                            " -> 0x" + _ddiNopGwesTextKseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; gwes VA 0x00011000 / FILESYS API page; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesTextLogged)
                {
                    _ddiNopGwesTextLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text map va=0x" +
                        GwesTextBasePage.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (data TLBL 0x00011C10; do not invent dest or sipcfg dest)");
                }
            }
            finally
            {
                _ddiNopGwesTextBusy = false;
            }
        }

        // Live 04b8c34: Display 0x0005D380 data-TLBL
        // 0x000B7CA8. Page 0x000B7000 holds GwesInitFlag
        // 0x000B7A1D. Firmware PTE only. Do not invent dest.
        public static uint MapDdiNopGwesDispData2Va(MipsBus bus, uint va)
        {
            if (_ddiNopGwesData2Busy)
                return va;
            if (!IsDdiNopGwesDispData2Armed())
                return va;
            if ((va & ~0xFFFu) != GwesDispData2Page)
                return va;
            if (_ddiNopGwesData2Kseg != 0)
                return _ddiNopGwesData2Kseg | (va & 0xFFFu);
            TryResolveDdiNopGwesDispData2(bus);
            if (_ddiNopGwesData2Kseg != 0)
                return _ddiNopGwesData2Kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesDispData2Armed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesData2Demand;
        }

        private static void TryNoteDdiNopGwesDispData2Tlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesData2Demand = true;
            if (!_ddiNopGwesData2TlblLogged)
            {
                _ddiNopGwesData2TlblLogged = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop data2-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (GwesInitFlag page; gwes Display data 0x000B7000; do not invent dest)");
            }
            TryResolveDdiNopGwesDispData2(bus);
        }

        private static void TryResolveDdiNopGwesDispData2(MipsBus bus)
        {
            if (_ddiNopGwesData2Kseg != 0 || _ddiNopGwesData2Busy || bus == null)
                return;
            try
            {
                _ddiNopGwesData2Busy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesDispData2Fault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesData2Kseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesData2Logged)
                    {
                        _ddiNopGwesData2Logged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesDispData2Fault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data2 map va=0x" +
                            GwesDispData2Page.ToString("X8") +
                            " -> 0x" + _ddiNopGwesData2Kseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; GwesInitFlag page; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesData2Logged)
                {
                    _ddiNopGwesData2Logged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data2 map va=0x" +
                        GwesDispData2Page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (data TLBL 0x000B7CA8; do not invent dest)");
                }
            }
            finally
            {
                _ddiNopGwesData2Busy = false;
            }
        }

        // Live 5db4c8e: Display 0x0005D38C data-TLBL
        // 0x000BA954 (GwesDispObj). Firmware PTE only.
        // Do not invent dest.
        public static uint MapDdiNopGwesDispData3Va(MipsBus bus, uint va)
        {
            if (_ddiNopGwesData3Busy)
                return va;
            if (!IsDdiNopGwesDispData3Armed())
                return va;
            if ((va & ~0xFFFu) != GwesDispData3Page)
                return va;
            if (_ddiNopGwesData3Kseg != 0)
                return _ddiNopGwesData3Kseg | (va & 0xFFFu);
            TryResolveDdiNopGwesDispData3(bus);
            if (_ddiNopGwesData3Kseg != 0)
                return _ddiNopGwesData3Kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesDispData3Armed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesData3Demand;
        }

        private static void TryNoteDdiNopGwesDispData3Tlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesData3Demand = true;
            if (!_ddiNopGwesData3TlblLogged)
            {
                _ddiNopGwesData3TlblLogged = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop data3-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (GwesDispObj page; gwes Display data 0x000BA000; do not invent dest)");
            }
            TryResolveDdiNopGwesDispData3(bus);
        }

        private static void TryResolveDdiNopGwesDispData3(MipsBus bus)
        {
            if (_ddiNopGwesData3Kseg != 0 || _ddiNopGwesData3Busy || bus == null)
                return;
            try
            {
                _ddiNopGwesData3Busy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesDispData3Fault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesData3Kseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesData3Logged)
                    {
                        _ddiNopGwesData3Logged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesDispData3Fault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data3 map va=0x" +
                            GwesDispData3Page.ToString("X8") +
                            " -> 0x" + _ddiNopGwesData3Kseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; GwesDispObj; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesData3Logged)
                {
                    _ddiNopGwesData3Logged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data3 map va=0x" +
                        GwesDispData3Page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (data TLBL 0x000BA954; do not invent dest)");
                }
            }
            finally
            {
                _ddiNopGwesData3Busy = false;
            }
        }

        // Live c36c2a4: I-fetch TLBL 0x00014B3C after
        // data3 map. gwes .text page 0x00014000 (ROM
        // 0x80149B3C). Firmware PTE only. Do not invent
        // dest or steal tv2 PE 0x00014000.
        public static uint MapDdiNopGwesText2Va(MipsBus bus, uint va)
        {
            if (_ddiNopGwesText2Busy)
                return va;
            if (!IsDdiNopGwesText2Armed())
                return va;
            if ((va & ~0xFFFu) != GwesText2Page)
                return va;
            if (_ddiNopGwesText2Kseg != 0)
                return _ddiNopGwesText2Kseg | (va & 0xFFFu);
            TryResolveDdiNopGwesText2(bus);
            if (_ddiNopGwesText2Kseg != 0)
                return _ddiNopGwesText2Kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesText2Armed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiNopGwesText2Demand;
        }

        private static void TryNoteDdiNopGwesText2Tlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiNopGwesText2Demand = true;
            if (!_ddiNopGwesText2TlblLogged)
            {
                _ddiNopGwesText2TlblLogged = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop text2-TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (gwes .text 0x00014000 / ROM 0x80149B3C; do not invent dest)");
            }
            TryResolveDdiNopGwesText2(bus);
        }

        private static void TryResolveDdiNopGwesText2(MipsBus bus)
        {
            if (_ddiNopGwesText2Kseg != 0 || _ddiNopGwesText2Busy || bus == null)
                return;
            try
            {
                _ddiNopGwesText2Busy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, GwesText2Fault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _ddiNopGwesText2Kseg = kseg & ~0xFFFu;
                    if (!_ddiNopGwesText2Logged)
                    {
                        _ddiNopGwesText2Logged = true;
                        uint word = 0;
                        TryPeekWord(bus, kseg | (GwesText2Fault & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text2 map va=0x" +
                            GwesText2Page.ToString("X8") +
                            " -> 0x" + _ddiNopGwesText2Kseg.ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; gwes .text 0x00014000; do not invent dest)");
                    }
                    return;
                }
                if (!_ddiNopGwesText2Logged)
                {
                    _ddiNopGwesText2Logged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text2 map va=0x" +
                        GwesText2Page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (fetch TLBL 0x00014B3C; do not invent dest)");
                }
            }
            finally
            {
                _ddiNopGwesText2Busy = false;
            }
        }

        // Live 187f5be: fetch-TLBL 0x000B4B80 after text2.
        // Same firmware-PTE demand-map as 0x00011000 /
        // 0x00014000 / 0x0005D000 / 0x000B6000 / 0x000B7000
        // / 0x000BA000. Live d01f68a: 0x00059000 PTE
        // dest 0x86FA1000 dest-word=0 hid ROM 0x8018E000.
        // Dest-word=0 .text uses o32 dataptr. Live
        // 831a196: ImageBase 0x00010000 / +4 is headers;
        // TOC gwes o32/load, not slot-0 filesys PTE.
        // Live 7214ee6: 0x000B9000 is compressed .data;
        // dest-word=0 firmware PTE is dump zeros, not
        // a missing section. Named pages keep tags.
        public static uint MapDdiNopGwesImageVa(MipsBus bus, uint va)
        {
            if (_gwesImageBusy)
                return va;
            if (!IsDdiNopGwesImageArmed())
                return va;
            if (!IsDdiNopGwesImageVa(va) || IsNamedDdiNopGwesPage(va))
                return va;
            uint kseg = LookupGwesImageKseg(va);
            if (kseg != 0)
            {
                TryReplaceGwesDest0WithRom(bus, va, kseg);
                kseg = LookupGwesImageKseg(va);
                if (kseg != 0)
                    return kseg | (va & 0xFFFu);
            }
            TryResolveDdiNopGwesImage(bus, va);
            kseg = LookupGwesImageKseg(va);
            if (kseg != 0)
                return kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopGwesImageArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _gwesImageDemand;
        }

        private static bool IsDdiNopGwesImageVa(uint va)
        {
            if ((va >> 25) != 0)
                return false;
            uint page = va & ~0xFFFu;
            if (page == GwesImageBasePage)
                return true;
            return page >= GwesImageLo && page < GwesImageHi;
        }

        private static bool IsGwesImageBasePage(uint va)
        {
            return (va >> 25) == 0 && (va & ~0xFFFu) == GwesImageBasePage;
        }

        private static bool IsGwesDataB9Page(uint va)
        {
            return (va >> 25) == 0 && (va & ~0xFFFu) == GwesDataB9Page;
        }

        private static bool IsNamedDdiNopGwesPage(uint va)
        {
            uint page = va & ~0xFFFu;
            return page == GwesDispFetchPage
                || page == GwesDispDataPage
                || page == GwesTextBasePage
                || page == GwesDispData2Page
                || page == GwesDispData3Page
                || page == GwesText2Page;
        }

        private static void EnsureGwesImageMaps()
        {
            if (_gwesImagePage != null)
                return;
            _gwesImagePage = new uint[GwesImagePageCap];
            _gwesImageKseg = new uint[GwesImagePageCap];
            _gwesImageDone = new bool[GwesImagePageCap];
            _gwesImageTlbl = new bool[GwesImagePageCap];
        }

        private static int FindGwesImageSlot(uint page)
        {
            EnsureGwesImageMaps();
            for (int i = 0; i < _gwesImageN; i++)
            {
                if (_gwesImagePage[i] == page)
                    return i;
            }
            return -1;
        }

        private static int ClaimGwesImageSlot(uint page)
        {
            int i = FindGwesImageSlot(page);
            if (i >= 0)
                return i;
            if (_gwesImageN >= GwesImagePageCap)
                return -1;
            i = _gwesImageN;
            _gwesImageN++;
            _gwesImagePage[i] = page;
            return i;
        }

        private static uint LookupGwesImageKseg(uint va)
        {
            int i = FindGwesImageSlot(va & ~0xFFFu);
            if (i < 0)
                return 0;
            return _gwesImageKseg[i];
        }

        private static void RememberGwesImageKseg(uint va, uint dest)
        {
            if (!IsDdiNopGwesImageVa(va) || IsNamedDdiNopGwesPage(va))
                return;
            int i = ClaimGwesImageSlot(va & ~0xFFFu);
            if (i < 0)
                return;
            uint kseg = dest & ~0xFFFu;
            if (kseg != 0)
            {
                _gwesImageKseg[i] = kseg;
                _gwesImageDone[i] = true;
            }
        }

        // TOC[7] o32[0] dataptr + (page - 0x00011000).
        // Only .text through GwesRomTextEnd. Not data.
        // Not ImageBase 0x00010000 (headers).
        private static uint GwesRomTextPage(uint va)
        {
            uint page = va & ~0xFFFu;
            if (page < GwesImageLo)
                return 0;
            uint rom = GwesRomText + (page - GwesImageLo);
            if (rom < GwesRomText || rom >= GwesRomTextEnd)
                return 0;
            return rom;
        }

        // Live d01f68a: dest-word=0 at 0x86FA1000.
        // Prefer o32 ROM when that dest peeks.
        private static bool TryGwesRomTextDest(MipsBus bus, uint va,
            uint destWord, out uint rom, out uint romWord)
        {
            rom = 0;
            romWord = 0;
            if (destWord != 0)
                return false;
            rom = GwesRomTextPage(va);
            if (rom == 0 || bus == null)
                return false;
            uint off = va & 0xFFFu;
            if (TryPeekWord(bus, rom | off, out romWord)
                || TryPeekWord(bus, rom, out romWord))
                return true;
            rom = 0;
            return false;
        }

        // Live 7214ee6: o32[1] .data is compressed and
        // page-off 0x3000 >= psize 0xCF5. Do not emit
        // 0x80288000. B9 uses firmware PTE dest0.
        private static bool TryGwesO32SectionDest(MipsBus bus, uint va,
            uint destWord, out uint rom, out uint romWord, out uint o32Index)
        {
            rom = 0;
            romWord = 0;
            o32Index = 0;
            if (destWord != 0 || bus == null || !IsGwesDataB9Page(va))
                return false;
            uint tocEntry = 0;
            if (!TryFindGwesTocEntry(bus, out tocEntry))
                return false;
            try
            {
                uint e32 = bus.Read32(tocEntry + 0x14);
                uint o32 = bus.Read32(tocEntry + 0x18);
                if (e32 == 0 || o32 == 0)
                    return false;
                if (bus.Read32(e32 + 8) != ExeVbase)
                    return false;
                uint objcnt = bus.Read32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return false;
                uint page = va & ~0xFFFu;
                for (uint s = 0; s < objcnt; s++)
                {
                    uint src = o32 + s * O32RomSize;
                    uint vsize = bus.Read32(src);
                    uint rva = bus.Read32(src + 4);
                    uint psize = bus.Read32(src + 8);
                    uint dataptr = bus.Read32(src + 0xC);
                    uint real = bus.Read32(src + 0x10);
                    uint flags = bus.Read32(src + 0x14);
                    if (vsize == 0 || psize == 0)
                        continue;
                    if ((flags & O32Compressed) != 0)
                        continue;
                    if (dataptr < 0x80000000u || dataptr >= 0xA0000000u)
                        continue;
                    uint start = real != 0 ? real : (ExeVbase + rva);
                    if (va < start || va >= start + vsize)
                        continue;
                    uint startPage = start & ~0xFFFu;
                    if (page < startPage)
                        continue;
                    uint rel = page - startPage;
                    if (rel >= psize)
                        continue;
                    uint dest = (dataptr + rel) & ~0xFFFu;
                    if (dest == 0 || dest == GwesRomText)
                        continue;
                    if (dest >= GwesRomText && dest < GwesRomTextEnd)
                        continue;
                    uint off = va & 0xFFFu;
                    if (!TryPeekWord(bus, dest | off, out romWord)
                        && !TryPeekWord(bus, dest, out romWord))
                        continue;
                    rom = dest;
                    o32Index = s;
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        // Live 831a196: 0x00010004 is TOC[7] ImageBase
        // headers, not .text. Slot 0 PTE is filesys.
        // Dest from gwes o32 rva0 / TOCentry load /
        // peeked MZ immediately before o32[0] dataptr.
        // Do not invent PE bytes.
        private static bool TryGwesHeaderDest(MipsBus bus, uint va,
            out uint rom, out uint romWord, out string via)
        {
            rom = 0;
            romWord = 0;
            via = null;
            if (bus == null || !IsGwesImageBasePage(va))
                return false;
            uint dest = 0;
            string how = null;
            if (!TryFindGwesHeaderRom(bus, out dest, out how) || dest == 0)
                return false;
            uint off = va & 0xFFFu;
            if (!TryPeekWord(bus, dest | off, out romWord)
                && !TryPeekWord(bus, dest, out romWord))
                return false;
            rom = dest;
            via = how;
            return true;
        }

        private static bool TryFindGwesTocEntry(MipsBus bus, out uint tocEntry)
        {
            tocEntry = 0;
            uint attr = 0;
            if (bus == null)
                return false;
            try
            {
                if (TryFindTocModule(bus, 0, 80, "gwes.exe", out tocEntry, out attr)
                    && tocEntry != 0)
                    return true;
                uint extra = ExtraRomToc(bus);
                if (extra != 0
                    && TryFindTocModule(bus, extra, 128, "gwes.exe", out tocEntry, out attr)
                    && tocEntry != 0)
                    return true;
                if (TryFindGwesTocByTextO32(bus, 0, 80, out tocEntry))
                    return true;
                if (extra != 0
                    && TryFindGwesTocByTextO32(bus, extra, 128, out tocEntry))
                    return true;
            }
            catch
            {
            }
            tocEntry = 0;
            return false;
        }

        // nk TOC[7] identity: e32 vbase 0x00010000 and
        // o32[0] dataptr 0x80146000. filesys/device share
        // the EXE vbase.
        private static bool TryFindGwesTocByTextO32(MipsBus bus, uint tocOrZero,
            uint maxMods, out uint tocEntry)
        {
            tocEntry = 0;
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
                    uint e32 = bus.Read32(entry + 0x14);
                    uint o32 = bus.Read32(entry + 0x18);
                    if (e32 == 0 || o32 == 0)
                        continue;
                    if (bus.Read32(e32 + 8) != ExeVbase)
                        continue;
                    uint dataptr = bus.Read32(o32 + 0xC);
                    uint real = bus.Read32(o32 + 0x10);
                    if (dataptr != GwesRomText)
                        continue;
                    if (real != 0 && (real & ~0xFFFu) != GwesImageLo)
                        continue;
                    if (found != 0)
                        return false;
                    found = entry;
                }
                if (found == 0)
                    return false;
                tocEntry = found;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryFindGwesHeaderRom(MipsBus bus, out uint dest,
            out string via)
        {
            dest = 0;
            via = null;
            uint tocEntry = 0;
            if (!TryFindGwesTocEntry(bus, out tocEntry))
                return false;
            try
            {
                uint e32 = bus.Read32(tocEntry + 0x14);
                uint o32 = bus.Read32(tocEntry + 0x18);
                uint load = bus.Read32(tocEntry + 0x1C);
                if (e32 == 0 || o32 == 0)
                    return false;
                if (bus.Read32(e32 + 8) != ExeVbase)
                    return false;
                uint objcnt = bus.Read32(e32) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return false;
                for (uint s = 0; s < objcnt; s++)
                {
                    uint src = o32 + s * O32RomSize;
                    uint vsize = bus.Read32(src);
                    uint rva = bus.Read32(src + 4);
                    uint dataptr = bus.Read32(src + 0xC);
                    uint real = bus.Read32(src + 0x10);
                    if (vsize == 0)
                        continue;
                    if (dataptr < 0x80000000u || dataptr >= 0xA0000000u)
                        continue;
                    uint page = dataptr & ~0xFFFu;
                    if (page == 0 || page == GwesRomText)
                        continue;
                    bool covers = false;
                    if (real != 0
                        && real <= GwesImageBasePage
                        && GwesImageBasePage < real + vsize)
                        covers = true;
                    if ((ExeVbase + rva) <= GwesImageBasePage
                        && GwesImageBasePage < (ExeVbase + rva + vsize))
                        covers = true;
                    if (rva == 0)
                        covers = true;
                    if (!covers)
                        continue;
                    dest = page;
                    via = "o32-hdr";
                    return true;
                }
                uint loadPage = load & ~0xFFFu;
                if (load >= 0x80000000u && load < 0xA0000000u
                    && loadPage != 0 && loadPage != GwesRomText)
                {
                    dest = loadPage;
                    via = "toc-load";
                    return true;
                }
                uint o0ptr = bus.Read32(o32 + 0xC);
                uint o0real = bus.Read32(o32 + 0x10);
                if (o0ptr == GwesRomText
                    && (o0real == 0 || (o0real & ~0xFFFu) == GwesImageLo))
                {
                    uint hdr = GwesRomText - (GwesImageLo - GwesImageBasePage);
                    uint word = 0;
                    if (hdr < GwesRomText && hdr >= 0x80000000u
                        && (TryPeekWord(bus, hdr | (GwesImageBaseFault & 0xFFFu), out word)
                            || TryPeekWord(bus, hdr, out word))
                        && (word & 0xFFFFu) == 0x5A4Du)
                    {
                        dest = hdr;
                        via = "o32-pre";
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void TryReplaceGwesDest0WithRom(MipsBus bus, uint va,
            uint kseg)
        {
            uint rom = 0;
            uint romWord = 0;
            string via = null;
            if (IsGwesImageBasePage(va))
            {
                if (!TryGwesHeaderDest(bus, va, out rom, out romWord, out via))
                    return;
            }
            else if (IsGwesDataB9Page(va))
            {
                // dest-word=0 is decompressed .data zeros.
                // Do not replace firmware PTE with XIP.
                return;
            }
            else
            {
                uint word = 0;
                TryPeekWord(bus, (kseg & ~0xFFFu) | (va & 0xFFFu), out word);
                if (!TryGwesRomTextDest(bus, va, word, out rom, out romWord))
                    return;
                via = "o32-rom";
            }
            if ((kseg & ~0xFFFu) == rom)
                return;
            int i = FindGwesImageSlot(va & ~0xFFFu);
            if (i < 0)
                return;
            _gwesImageKseg[i] = rom;
            _gwesImageDone[i] = true;
            string why = IsGwesImageBasePage(va)
                ? " (ImageBase headers; TOC[7] gwes; do not invent dest)"
                : " (dest-word=0 .text; TOC[7] o32; do not invent dest)";
            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                (va & ~0xFFFu).ToString("X8") +
                " -> 0x" + rom.ToString("X8") +
                " dest-word=0x" + romWord.ToString("X8") +
                " via=" + via +
                " was=0x" + (kseg & ~0xFFFu).ToString("X8") +
                why);
        }

        private static void TryNoteDdiNopGwesImageTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _gwesImageDemand = true;
            uint page = vaddr & ~0xFFFu;
            int slot = ClaimGwesImageSlot(page);
            if (slot >= 0 && !_gwesImageTlbl[slot])
            {
                _gwesImageTlbl[slot] = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (gwes image page 0x" + page.ToString("X8") +
                    "; do not invent dest)");
            }
            TryResolveDdiNopGwesImage(bus, vaddr);
        }

        private static void TryResolveDdiNopGwesImage(MipsBus bus, uint va)
        {
            if (bus == null || _gwesImageBusy)
                return;
            if (!IsDdiNopGwesImageVa(va) || IsNamedDdiNopGwesPage(va))
                return;
            uint page = va & ~0xFFFu;
            int slot = FindGwesImageSlot(page);
            if (slot >= 0 && (_gwesImageKseg[slot] != 0 || _gwesImageDone[slot]))
                return;
            try
            {
                _gwesImageBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                slot = ClaimGwesImageSlot(page);
                if (slot < 0)
                    return;
                uint word = 0;
                uint rom = 0;
                uint romWord = 0;
                string via = null;
                if (TryGwesHeaderDest(bus, va, out rom, out romWord, out via))
                {
                    _gwesImageKseg[slot] = rom;
                    if (!_gwesImageDone[slot])
                    {
                        _gwesImageDone[slot] = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                            page.ToString("X8") +
                            " -> 0x" + rom.ToString("X8") +
                            " dest-word=0x" + romWord.ToString("X8") +
                            " via=" + via +
                            " (ImageBase headers; TOC[7] gwes; do not invent dest)");
                    }
                    return;
                }
                if (IsGwesImageBasePage(va))
                {
                    if (!_gwesImageDone[slot])
                    {
                        _gwesImageDone[slot] = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                            page.ToString("X8") +
                            " pte-miss sec=0x" + sec.ToString("X8") +
                            " (ImageBase headers; TOC gwes miss; do not invent dest)");
                    }
                    return;
                }
                if (IsGwesDataB9Page(va))
                {
                    if (sec != 0
                        && WalkFirmwarePte(bus, sec, va, out l1, out l2, out pfn, out kseg)
                        && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                    {
                        TryPeekWord(bus, (kseg & ~0xFFFu) | (va & 0xFFFu), out word);
                        _gwesImageKseg[slot] = kseg & ~0xFFFu;
                        if (!_gwesImageDone[slot])
                        {
                            _gwesImageDone[slot] = true;
                            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                                page.ToString("X8") +
                                " -> 0x" + _gwesImageKseg[slot].ToString("X8") +
                                " l2=0x" + l2.ToString("X8") +
                                " dest-word=0x" + word.ToString("X8") +
                                " (firmware PTE; compressed .data dest0; do not invent dest)");
                        }
                        return;
                    }
                    // 7214ee6: o32-sec miss then hard-done
                    // hid the later 0x80040278 PTE. Retry.
                    // Do not invent 0x80288000.
                    if (!_gwesImageTlbl[slot])
                    {
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                            page.ToString("X8") +
                            " pte-miss sec=0x" + sec.ToString("X8") +
                            " (compressed .data; wait firmware PTE; do not invent dest)");
                    }
                    return;
                }
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, va, out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    TryPeekWord(bus, (kseg & ~0xFFFu) | (va & 0xFFFu), out word);
                    if (TryGwesRomTextDest(bus, va, word, out rom, out romWord))
                    {
                        _gwesImageKseg[slot] = rom;
                        if (!_gwesImageDone[slot])
                        {
                            _gwesImageDone[slot] = true;
                            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                                page.ToString("X8") +
                                " -> 0x" + rom.ToString("X8") +
                                " l2=0x" + l2.ToString("X8") +
                                " dest-word=0x" + romWord.ToString("X8") +
                                " via=o32-rom was=0x" + (kseg & ~0xFFFu).ToString("X8") +
                                " (dest-word=0 .text; TOC[7] o32; do not invent dest)");
                        }
                        return;
                    }
                    _gwesImageKseg[slot] = kseg & ~0xFFFu;
                    if (!_gwesImageDone[slot])
                    {
                        _gwesImageDone[slot] = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                            page.ToString("X8") +
                            " -> 0x" + _gwesImageKseg[slot].ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; gwes image; do not invent dest)");
                    }
                    return;
                }
                if (TryGwesRomTextDest(bus, va, 0, out rom, out romWord))
                {
                    _gwesImageKseg[slot] = rom;
                    if (!_gwesImageDone[slot])
                    {
                        _gwesImageDone[slot] = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                            page.ToString("X8") +
                            " -> 0x" + rom.ToString("X8") +
                            " dest-word=0x" + romWord.ToString("X8") +
                            " via=o32-rom (pte-miss .text; TOC[7] o32; do not invent dest)");
                    }
                    return;
                }
                if (!_gwesImageDone[slot])
                {
                    _gwesImageDone[slot] = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page map va=0x" +
                        page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (gwes image TLBL; do not invent dest)");
                }
            }
            finally
            {
                _gwesImageBusy = false;
            }
        }

        // Live 147e54f: I-fetch TLBL 0x03FB492C after gwes
        // image generalize. IAT slot6 word. COREDLL
        // ImageBase 0x03F50000. MapCoredllSharedVa still
        // caps at 0x03FA0000 until tv2 (OEMIdle). Demand-
        // map remaining COREDLL pages after DllMain via
        // slot-1 firmware PTE only. Live 1bba9df: slot-4
        // view 0x09F574F8 ≡ 0x03F574F8. Canon to slot-1
        // IB VA; alias dest. Do not invent dest.
        public static uint MapDdiNopCoredllImageVa(MipsBus bus, uint va)
        {
            if (_coredllImageBusy)
                return va;
            if (!IsDdiNopCoredllImageArmed())
                return va;
            if (!IsDdiNopCoredllImageVa(va))
                return va;
            uint use = CoredllImageCanonVa(va);
            uint kseg = LookupCoredllImageKseg(use);
            if (kseg != 0)
            {
                TryLogCoredllSlotView(bus, va, use, kseg, 0);
                return kseg | (va & 0xFFFu);
            }
            TryResolveDdiNopCoredllImage(bus, va);
            kseg = LookupCoredllImageKseg(use);
            if (kseg != 0)
            {
                TryLogCoredllSlotView(bus, va, use, kseg, 0);
                return kseg | (va & 0xFFFu);
            }
            return va;
        }

        private static bool IsDdiNopCoredllImageArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _coredllImageDemand;
        }

        // Slot-relative COREDLL ImageBase pages.
        // Rel in [0x01F50000, 0x01FF0000). Slot 1 is
        // 0x03F5xxxx (keep ImageBase). Slot 4 is
        // 0x09F5xxxx (live 1bba9df). Slot 0 is IAT
        // real 0x01F57000 — exclude. Not a blanket
        // bit25 walk. Not MapCoredllSharedVa.
        private static bool IsDdiNopCoredllImageVa(uint va)
        {
            uint rel = va & 0x01FFFFFFu;
            if (rel < CoredllImageRelLo || rel >= CoredllImageRelHi)
                return false;
            uint slot = (va & ~0xFFFu) >> 25;
            return slot >= 1 && slot <= 31;
        }

        // Slot-1 ImageBase view. Keep 0x03F50000.
        private static uint CoredllImageCanonVa(uint va)
        {
            return (CoredllSharedLo & ~0x01FFFFFFu) | (va & 0x01FFFFFFu);
        }

        private static void EnsureCoredllImageMaps()
        {
            if (_coredllImagePage != null)
                return;
            _coredllImagePage = new uint[CoredllImagePageCap];
            _coredllImageKseg = new uint[CoredllImagePageCap];
            _coredllImageDone = new bool[CoredllImagePageCap];
            _coredllImageTlbl = new bool[CoredllImagePageCap];
        }

        private static int FindCoredllImageSlot(uint page)
        {
            EnsureCoredllImageMaps();
            for (int i = 0; i < _coredllImageN; i++)
            {
                if (_coredllImagePage[i] == page)
                    return i;
            }
            return -1;
        }

        private static int ClaimCoredllImageSlot(uint page)
        {
            int i = FindCoredllImageSlot(page);
            if (i >= 0)
                return i;
            if (_coredllImageN >= CoredllImagePageCap)
                return -1;
            i = _coredllImageN;
            _coredllImageN++;
            _coredllImagePage[i] = page;
            return i;
        }

        private static uint LookupCoredllImageKseg(uint va)
        {
            int i = FindCoredllImageSlot(CoredllImageCanonVa(va) & ~0xFFFu);
            if (i < 0)
                return 0;
            return _coredllImageKseg[i];
        }

        private static void TryNoteDdiNopCoredllImageTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _coredllImageDemand = true;
            uint page = vaddr & ~0xFFFu;
            uint use = CoredllImageCanonVa(vaddr);
            uint canon = use & ~0xFFFu;
            bool view = page != canon;
            int slot = ClaimCoredllImageSlot(canon);
            bool first = view
                ? !_coredllSlotViewTlbl
                : (slot >= 0 && !_coredllImageTlbl[slot]);
            if (first)
            {
                if (view)
                    _coredllSlotViewTlbl = true;
                else if (slot >= 0)
                    _coredllImageTlbl[slot] = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop coredll-page TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " (COREDLL ImageBase 0x03F50000 page 0x" +
                    page.ToString("X8") +
                    " canon=0x" + canon.ToString("X8") +
                    "; IAT slot6 class; do not invent dest)");
            }
            TryResolveDdiNopCoredllImage(bus, vaddr);
        }

        private static void TryResolveDdiNopCoredllImage(MipsBus bus, uint va)
        {
            if (bus == null || _coredllImageBusy)
                return;
            if (!IsDdiNopCoredllImageVa(va))
                return;
            uint use = CoredllImageCanonVa(va);
            uint page = use & ~0xFFFu;
            int slot = FindCoredllImageSlot(page);
            if (slot >= 0 && (_coredllImageKseg[slot] != 0 || _coredllImageDone[slot]))
                return;
            try
            {
                _coredllImageBusy = true;
                uint sec = _coredllLiveSec != 0 ? _coredllLiveSec : PeekSection(bus, 1);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                slot = ClaimCoredllImageSlot(page);
                if (slot < 0)
                    return;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, use, out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _coredllImageKseg[slot] = kseg & ~0xFFFu;
                    if (!_coredllImageDone[slot])
                    {
                        _coredllImageDone[slot] = true;
                        uint word = 0;
                        TryPeekWord(bus, _coredllImageKseg[slot] | (va & 0xFFFu), out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop coredll-page map va=0x" +
                            page.ToString("X8") +
                            " -> 0x" + _coredllImageKseg[slot].ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware PTE; COREDLL ImageBase; do not invent dest)");
                    }
                    return;
                }
                if (!_coredllImageDone[slot])
                {
                    _coredllImageDone[slot] = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop coredll-page map va=0x" +
                        page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (COREDLL image TLBL; do not invent dest)");
                }
            }
            finally
            {
                _coredllImageBusy = false;
            }
        }

        // Live 1bba9df: first process-slot view of an
        // already-mapped IB page. Same dest. Do not
        // invent dest. Do not rewrite ImageBase.
        private static void TryLogCoredllSlotView(MipsBus bus, uint va,
            uint canon, uint kseg, uint l2)
        {
            uint page = va & ~0xFFFu;
            uint ib = canon & ~0xFFFu;
            if (page == ib)
                return;
            if (_coredllSlotViewLogged)
                return;
            _coredllSlotViewLogged = true;
            uint word = 0;
            TryPeekWord(bus, (kseg & ~0xFFFu) | (va & 0xFFFu), out word);
            BootLog.Write("[Hive] ExtraROM ddi_nop coredll-page map va=0x" +
                page.ToString("X8") +
                " -> 0x" + (kseg & ~0xFFFu).ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " via=slot-1-alias canon=0x" + ib.ToString("X8") +
                " (COREDLL ImageBase slot view; do not invent dest)");
        }

        // Live a633b83: NK 0x8003D254 data-TLBL
        // 0x040110FC. One filesys slot-2 page after
        // DllMain. Slot-2 section first; slot-0
        // 0x00011000 is the same filesys page
        // (HostHardDisk). Live 5b54d07: same
        // relative page in slot 4 (0x08011000 /
        // 0x08011BE8). Alias to the already-mapped
        // FILESYS ROM dest when slot-4 PTE misses.
        // Do not walk all slot-2 / slot-4. Do not
        // invent dest or steal gwes ROM.
        public static uint MapDdiNopFilesysSlot2Va(MipsBus bus, uint va)
        {
            if (_filesysSlot2Busy || _filesysSlot2ExtraBusy)
                return va;
            if (!IsDdiNopFilesysSlot2Armed())
                return va;
            if (IsFilesysSlot2ExtraPage(va))
                return MapFilesysSlot2ExtraVa(bus, va);
            if (!IsDdiNopFilesysSlotVa(va))
                return va;
            if (_filesysSlot2Kseg != 0)
            {
                TryLogFilesysSlotMap(bus, va, _filesysSlot2Kseg, 0, "slot-2-alias");
                return _filesysSlot2Kseg | (va & 0xFFFu);
            }
            TryResolveDdiNopFilesysSlot2(bus, va);
            if (_filesysSlot2Kseg != 0)
                return _filesysSlot2Kseg | (va & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopFilesysSlot2Armed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _filesysSlot2Demand;
        }

        // FILESYS API page at slot+0x11000.
        // Slot 2 proven 017b67e; slot 4 live
        // 5b54d07. Slot 0 is gwes-text / filesys
        // ROM — other handlers. Not a blanket
        // bit25 slot walk.
        private static bool IsDdiNopFilesysSlotVa(uint va)
        {
            if (IsFilesysSlot2ExtraPage(va))
                return true;
            uint page = va & ~0xFFFu;
            if ((page & FilesysSlotMask) != FilesysSlotRelPage)
                return false;
            uint slot = page >> 25;
            return slot == 2 || slot == 4;
        }

        // Slot 2 extra pages only. Not FILESYS API
        // +0x11000 (0x04011000). Not slot-0 gwes
        // (rel 0x00021000 is gwes .text). Not a
        // blanket slot-2 walk.
        private static bool IsFilesysSlot2ExtraPage(uint va)
        {
            if ((va >> 25) != 2)
                return false;
            uint page = va & ~0xFFFu;
            if (page < FilesysSlot2ExtraLo || page >= FilesysSlot2ExtraHi)
                return false;
            return (page & FilesysSlotMask) != FilesysSlotRelPage;
        }

        private static string FilesysSlotHiveTag(uint va)
        {
            uint slot = (va & ~0xFFFu) >> 25;
            if (slot == 4)
                return "filesys-slot4";
            return "filesys-slot2";
        }

        private static void TryNoteDdiNopFilesysSlot2Tlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _filesysSlot2Demand = true;
            if (IsFilesysSlot2ExtraPage(vaddr))
            {
                TryNoteFilesysSlot2ExtraTlbl(bus, regs, epc, vaddr, vector);
                return;
            }
            uint page = vaddr & ~0xFFFu;
            bool first = page == FilesysSlot4Page
                ? !_filesysSlot4TlblLogged
                : !_filesysSlot2TlblLogged;
            if (first)
            {
                if (page == FilesysSlot4Page)
                    _filesysSlot4TlblLogged = true;
                else
                    _filesysSlot2TlblLogged = true;
                uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint insn = 0;
                TryPeekWord(bus, epc, out insn);
                string dis = insn != 0 ? FormatMipsOp(epc, insn) : "peek-miss";
                BootLog.Write("[Hive] ExtraROM ddi_nop " +
                    FilesysSlotHiveTag(vaddr) +
                    " TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " insn=0x" + insn.ToString("X8") +
                    " " + dis +
                    " a1=0x" + a1.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " (FILESYS API page slot+" +
                    FilesysSlotRelPage.ToString("X") +
                    "; do not invent dest)");
            }
            TryResolveDdiNopFilesysSlot2(bus, vaddr);
        }

        private static void TryResolveDdiNopFilesysSlot2(MipsBus bus, uint va)
        {
            if (_filesysSlot2Kseg != 0 || _filesysSlot2Busy || bus == null)
                return;
            try
            {
                _filesysSlot2Busy = true;
                uint page = va & ~0xFFFu;
                uint slot = page >> 25;
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                uint sec = PeekSection(bus, slot);
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, page | (va & 0xFFFu),
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    RememberFilesysSlot2Kseg(bus, kseg, l2,
                        "slot-" + slot, page, va);
                    return;
                }
                // Same FILESYS API page at slot 2
                // (proven 017b67e → 0x80105000).
                uint sec2 = slot == 2 ? sec : PeekSection(bus, 2);
                if (slot != 2 && sec2 != 0
                    && WalkFirmwarePte(bus, sec2, FilesysSlot2Fault,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    RememberFilesysSlot2Kseg(bus, kseg, l2, "slot-2", page, va);
                    return;
                }
                // Same filesys page at slot 0 (HostHardDisk:
                // slot 0 is filesys). Firmware PTE only.
                uint sec0 = PeekSection(bus, 0);
                if (sec0 != 0
                    && WalkFirmwarePte(bus, sec0, GwesTextBasePage,
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    RememberFilesysSlot2Kseg(bus, kseg, l2, "slot-0", page, va);
                    return;
                }
                TryLogFilesysSlotMiss(page, sec, sec2, sec0);
            }
            finally
            {
                _filesysSlot2Busy = false;
            }
        }

        private static void RememberFilesysSlot2Kseg(MipsBus bus, uint kseg,
            uint l2, string via, uint page, uint va)
        {
            _filesysSlot2Kseg = kseg & ~0xFFFu;
            TryLogFilesysSlotMap(bus, page | (va & 0xFFFu),
                _filesysSlot2Kseg, l2, via);
        }

        private static void TryLogFilesysSlotMap(MipsBus bus, uint va,
            uint kseg, uint l2, string via)
        {
            uint page = va & ~0xFFFu;
            if (page == FilesysSlot4Page)
            {
                if (_filesysSlot4Logged)
                    return;
                _filesysSlot4Logged = true;
            }
            else
            {
                if (_filesysSlot2Logged)
                    return;
                _filesysSlot2Logged = true;
            }
            uint word = 0;
            TryPeekWord(bus, (kseg & ~0xFFFu) | (va & 0xFFFu), out word);
            if (via == null)
                via = "firmware PTE";
            BootLog.Write("[Hive] ExtraROM ddi_nop " +
                FilesysSlotHiveTag(va) +
                " map va=0x" + page.ToString("X8") +
                " -> 0x" + (kseg & ~0xFFFu).ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " via=" + via +
                " (firmware PTE; FILESYS API page slot+" +
                FilesysSlotRelPage.ToString("X") +
                "; do not invent dest)");
        }

        private static void TryLogFilesysSlotMiss(uint page, uint sec,
            uint sec2, uint sec0)
        {
            if (page == FilesysSlot4Page)
            {
                if (_filesysSlot4Logged)
                    return;
                _filesysSlot4Logged = true;
            }
            else
            {
                if (_filesysSlot2Logged)
                    return;
                _filesysSlot2Logged = true;
            }
            BootLog.Write("[Hive] ExtraROM ddi_nop " +
                FilesysSlotHiveTag(page) +
                " map va=0x" + page.ToString("X8") +
                " pte-miss sec=0x" + sec.ToString("X8") +
                " sec2=0x" + sec2.ToString("X8") +
                " sec0=0x" + sec0.ToString("X8") +
                " (FILESYS API page; do not invent dest or walk slot-2/4)");
        }

        // Live 725f2f4: extra maps won through
        // 0x0405D000. Next miss 0x04021ABC.
        // Per-page firmware PTE after DllMain.
        // Do not alias FILESYS API dest 0x80105000.
        // Do not walk all slot-2. Do not invent dest.
        private static void EnsureFilesysSlot2ExtraMaps()
        {
            if (_filesysSlot2ExtraPage != null)
                return;
            _filesysSlot2ExtraPage = new uint[FilesysSlot2ExtraCap];
            _filesysSlot2ExtraKseg = new uint[FilesysSlot2ExtraCap];
            _filesysSlot2ExtraLogged = new bool[FilesysSlot2ExtraCap];
            _filesysSlot2ExtraTlbl = new bool[FilesysSlot2ExtraCap];
            _filesysSlot2ExtraMiss = new bool[FilesysSlot2ExtraCap];
        }

        private static int FindFilesysSlot2ExtraSlot(uint page)
        {
            EnsureFilesysSlot2ExtraMaps();
            for (int i = 0; i < _filesysSlot2ExtraN; i++)
            {
                if (_filesysSlot2ExtraPage[i] == page)
                    return i;
            }
            return -1;
        }

        private static int ClaimFilesysSlot2ExtraSlot(uint page)
        {
            int i = FindFilesysSlot2ExtraSlot(page);
            if (i >= 0)
                return i;
            if (_filesysSlot2ExtraN >= FilesysSlot2ExtraCap)
                return -1;
            i = _filesysSlot2ExtraN;
            _filesysSlot2ExtraN++;
            _filesysSlot2ExtraPage[i] = page;
            return i;
        }

        private static uint LookupFilesysSlot2ExtraKseg(uint va)
        {
            int i = FindFilesysSlot2ExtraSlot(va & ~0xFFFu);
            if (i < 0)
                return 0;
            return _filesysSlot2ExtraKseg[i];
        }

        private static uint MapFilesysSlot2ExtraVa(MipsBus bus, uint va)
        {
            uint kseg = LookupFilesysSlot2ExtraKseg(va);
            if (kseg != 0)
                return kseg | (va & 0xFFFu);
            TryResolveFilesysSlot2Extra(bus, va);
            kseg = LookupFilesysSlot2ExtraKseg(va);
            if (kseg != 0)
                return kseg | (va & 0xFFFu);
            return va;
        }

        private static void TryNoteFilesysSlot2ExtraTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            uint page = vaddr & ~0xFFFu;
            int i = ClaimFilesysSlot2ExtraSlot(page);
            if (i >= 0 && !_filesysSlot2ExtraTlbl[i])
            {
                _filesysSlot2ExtraTlbl[i] = true;
                uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint insn = 0;
                TryPeekWord(bus, epc, out insn);
                string dis = insn != 0 ? FormatMipsOp(epc, insn) : "peek-miss";
                BootLog.Write("[Hive] ExtraROM ddi_nop filesys-slot2 TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " insn=0x" + insn.ToString("X8") +
                    " " + dis +
                    " a1=0x" + a1.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " (filesys 0x" + (page & FilesysSlotMask).ToString("X") +
                    "; do not invent dest)");
            }
            TryResolveFilesysSlot2Extra(bus, vaddr);
        }

        private static void TryResolveFilesysSlot2Extra(MipsBus bus, uint va)
        {
            if (_filesysSlot2ExtraBusy || bus == null)
                return;
            if (!IsFilesysSlot2ExtraPage(va))
                return;
            uint page = va & ~0xFFFu;
            int i = ClaimFilesysSlot2ExtraSlot(page);
            if (i < 0)
                return;
            if (_filesysSlot2ExtraKseg[i] != 0)
                return;
            try
            {
                _filesysSlot2ExtraBusy = true;
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                uint sec = PeekSection(bus, 2);
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, page | (va & 0xFFFu),
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    _filesysSlot2ExtraKseg[i] = kseg & ~0xFFFu;
                    if (!_filesysSlot2ExtraLogged[i])
                    {
                        _filesysSlot2ExtraLogged[i] = true;
                        uint word = 0;
                        TryPeekWord(bus, _filesysSlot2ExtraKseg[i] | (va & 0xFFFu),
                            out word);
                        BootLog.Write("[Hive] ExtraROM ddi_nop filesys-slot2 map va=0x" +
                            page.ToString("X8") +
                            " -> 0x" + _filesysSlot2ExtraKseg[i].ToString("X8") +
                            " l2=0x" + l2.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " via=slot-2 (firmware PTE; filesys 0x" +
                            (page & FilesysSlotMask).ToString("X") +
                            "; do not invent dest)");
                    }
                    return;
                }
                if (!_filesysSlot2ExtraMiss[i])
                {
                    _filesysSlot2ExtraMiss[i] = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop filesys-slot2 map va=0x" +
                        page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " (filesys 0x" + (page & FilesysSlotMask).ToString("X") +
                        "; do not invent dest or walk slot-2)");
                }
            }
            finally
            {
                _filesysSlot2ExtraBusy = false;
            }
        }

        // Live 82240a0: page0 mapped. Next miss is filesys
        // 0x00031A10 data-TLBL 0x48D01000 v1=0x48D05000.
        // Per-page Hive. Do not invent dest.
        private static void TryNoteDdiNopFilesys48dTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _filesys48dLogged = true;
            EnsureFilesys48dMaps();
            int i = Filesys48dIndex(vaddr);
            if (i >= 0 && !_filesys48dTlbl[i])
            {
                _filesys48dTlbl[i] = true;
                uint insn = 0;
                TryPeekWord(bus, epc, out insn);
                string dis = insn != 0 ? FormatMipsOp(epc, insn) : "peek-miss";
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint v1 = regs != null && regs.Length > 3 ? regs[3] : 0;
                uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop filesys-48d TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " insn=0x" + insn.ToString("X8") +
                    " " + dis +
                    " v0=0x" + v0.ToString("X8") +
                    " v1=0x" + v1.ToString("X8") +
                    " ra=0x" + ra.ToString("X8") +
                    " strip=0x" + (vaddr & ~Filesys48dBit30).ToString("X8") +
                    " (tagged gwes-slot page; do not invent dest or walk slot-2)");
            }
            TryResolveDdiNopFilesys48d(bus, vaddr);
        }

        // Live 82240a0: page0 0x48D00000→0x08D00000→0x87B63000.
        // Live next: 0x48D01000 (gwes dest-word 0x00082000).
        // Bit30 clear; range [0x08D00000, 0x08D06000) from
        // v1=0x48D05000. Per-page neighbor/VALLOC-adj if
        // dest peeks, else zero-valloc 4K.
        public static uint MapDdiNopFilesys48dVa(MipsBus bus, uint va)
        {
            if (_filesys48dBusy)
                return va;
            if (!IsDdiNopFilesys48dArmed())
                return va;
            if (!IsDdiNopFilesys48dVa(va))
                return va;
            uint use = va & ~Filesys48dBit30;
            int i = Filesys48dIndex(use);
            if (i < 0)
                return va;
            EnsureFilesys48dMaps();
            if (_filesys48dKsegs[i] != 0)
                return _filesys48dKsegs[i] | (va & 0xFFFu);
            TryResolveDdiNopFilesys48d(bus, use);
            if (_filesys48dKsegs[i] != 0)
                return _filesys48dKsegs[i] | (va & 0xFFFu);
            return use;
        }

        private static bool IsDdiNopFilesys48dArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _filesys48dLogged;
        }

        // Tagged 0x48Dxxxxx or cleared 0x08Dxxxxx in
        // [0x08D00000, 0x08D06000). Not a blanket bit30 strip.
        private static bool IsDdiNopFilesys48dVa(uint va)
        {
            uint page = va & ~0xFFFu;
            uint use = page & ~Filesys48dBit30;
            if (use < Filesys48dClearLo || use >= Filesys48dClearHi)
                return false;
            return page == use || page == (use | Filesys48dBit30);
        }

        private static int Filesys48dIndex(uint va)
        {
            uint use = (va & ~Filesys48dBit30) & ~0xFFFu;
            if (use < Filesys48dClearLo || use >= Filesys48dClearHi)
                return -1;
            return (int)((use - Filesys48dClearLo) >> 12);
        }

        private static void EnsureFilesys48dMaps()
        {
            if (_filesys48dKsegs != null)
                return;
            _filesys48dKsegs = new uint[Filesys48dPageCap];
            _filesys48dDone = new bool[Filesys48dPageCap];
            _filesys48dTlbl = new bool[Filesys48dPageCap];
        }

        private static void TryResolveDdiNopFilesys48d(MipsBus bus, uint va)
        {
            if (bus == null || _filesys48dBusy)
                return;
            uint use = va & ~Filesys48dBit30;
            int i = Filesys48dIndex(use);
            if (i < 0)
                return;
            EnsureFilesys48dMaps();
            if (_filesys48dKsegs[i] != 0 || _filesys48dDone[i])
                return;
            try
            {
                _filesys48dBusy = true;
                uint page = use & ~0xFFFu;
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                uint sec = PeekSection(bus, Filesys48dGwesSlot);
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, page | (va & 0xFFFu),
                        out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    RememberFilesys48dKseg(bus, i, page, kseg, l2, va,
                        "tagged gwes-slot; bit30 clear; slot-4 firmware PTE");
                    return;
                }
                uint alias = 0;
                uint aliasL2 = 0;
                string why;
                if (TryAliasFilesys48dNeighbor(bus, sec, page, va,
                    out alias, out aliasL2, out why))
                {
                    RememberFilesys48dKseg(bus, i, page, alias, aliasL2, va, why);
                    return;
                }
                if (TryHostBackFilesys48dPage(i, page))
                {
                    RememberFilesys48dKseg(bus, i, page, _filesys48dKsegs[i], 0, va,
                        "zero-valloc; uncommitted gwes-slot page");
                    return;
                }
                if (!_filesys48dDone[i])
                {
                    _filesys48dDone[i] = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop filesys-48d map va=0x" +
                        (page | Filesys48dBit30).ToString("X8") +
                        " -> 0x" + page.ToString("X8") +
                        " pte-miss sec4=0x" + sec.ToString("X8") +
                        " (tagged gwes-slot; do not invent dest or walk slot-2)");
                }
            }
            finally
            {
                _filesys48dBusy = false;
            }
        }

        private static void RememberFilesys48dKseg(MipsBus bus, int i, uint page,
            uint kseg, uint l2, uint va, string why)
        {
            EnsureFilesys48dMaps();
            _filesys48dKsegs[i] = kseg & ~0xFFFu;
            if (_filesys48dDone[i])
                return;
            _filesys48dDone[i] = true;
            uint word = 0;
            TryPeekWord(bus, _filesys48dKsegs[i] | (va & 0xFFFu), out word);
            if (why == null)
                why = "tagged gwes-slot";
            BootLog.Write("[Hive] ExtraROM ddi_nop filesys-48d map va=0x" +
                (page | Filesys48dBit30).ToString("X8") +
                " -> 0x" + page.ToString("X8") +
                " -> 0x" + _filesys48dKsegs[i].ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " (" + why + "; do not invent dest)");
        }

        // Live 82240a0: page0 dest 0x87B63000. Next page
        // tries dest+0x1000 / VALLOC dest+delta when that
        // dest already peeks. Do not invent dest bytes.
        private static bool TryAliasFilesys48dNeighbor(MipsBus bus, uint sec,
            uint page, uint va, out uint dest, out uint l2, out string why)
        {
            dest = 0;
            l2 = 0;
            why = null;
            if (bus == null)
                return false;
            EnsureFilesys48dMaps();
            int i = Filesys48dIndex(page);
            uint off = va & 0xFFFu;
            if (i > 0 && _filesys48dKsegs[i - 1] != 0)
            {
                uint cand = _filesys48dKsegs[i - 1] + 0x1000u;
                uint word = 0;
                if (TryPeekWord(bus, cand | off, out word) || TryPeekWord(bus, cand, out word))
                {
                    dest = cand;
                    why = "neighbor-dest; peek-ok";
                    return true;
                }
            }
            if (i >= 0 && i + 1 < Filesys48dPageCap && _filesys48dKsegs[i + 1] != 0)
            {
                uint cand = _filesys48dKsegs[i + 1] - 0x1000u;
                uint word = 0;
                if (TryPeekWord(bus, cand | off, out word) || TryPeekWord(bus, cand, out word))
                {
                    dest = cand;
                    why = "neighbor-dest; peek-ok";
                    return true;
                }
            }
            uint[] nbr = { page - 0x1000u, page + 0x1000u, 0x080D0000u, 0x080DF000u };
            uint walkSec = sec != 0 ? sec : PeekSection(bus, Filesys48dGwesSlot);
            for (int n = 0; n < nbr.Length; n++)
            {
                uint l1 = 0;
                uint pfn = 0;
                uint kseg = 0;
                if (walkSec == 0
                    || !WalkFirmwarePte(bus, walkSec, nbr[n],
                        out l1, out l2, out pfn, out kseg)
                    || (kseg & 0x1FFFFFFFu) < 0x00010000u)
                    continue;
                uint cand = kseg & ~0xFFFu;
                if (nbr[n] < page)
                    cand += page - nbr[n];
                else
                    cand -= nbr[n] - page;
                uint word = 0;
                if (!TryPeekWord(bus, cand | off, out word)
                    && !TryPeekWord(bus, cand, out word))
                    continue;
                dest = cand;
                if (nbr[n] == 0x080D0000u || nbr[n] == 0x080DF000u)
                    why = "valloc-dest-adj; peek-ok";
                else
                    why = "neighbor-dest; peek-ok";
                return true;
            }
            return false;
        }

        // Uncommitted gwes-slot page. One zero 4K from the
        // valloc host pool (same as process-info). Do not
        // invent firmware payload.
        private static bool TryHostBackFilesys48dPage(int i, uint page)
        {
            EnsureFilesys48dMaps();
            uint lo = page;
            uint hi = page + 0x1000u;
            if (_filesys48dKsegs[i] != 0)
                return true;
            if (VallocHostCovers(lo, hi))
            {
                for (int n = 0; n < _vallocHostN; n++)
                {
                    if (_vallocHostLo[n] <= lo && _vallocHostHi[n] >= hi)
                    {
                        _filesys48dKsegs[i] = _vallocHostKseg[n];
                        return _filesys48dKsegs[i] != 0;
                    }
                }
                return false;
            }
            if (_vallocHostN >= _vallocHostLo.Length)
                return false;
            uint span = 0x1000u;
            uint kseg = _vallocHostPool;
            if (kseg < VallocHostKseg || kseg + span > VallocHostKsegLim)
                return false;
            _vallocHostLo[_vallocHostN] = lo;
            _vallocHostHi[_vallocHostN] = hi;
            _vallocHostKseg[_vallocHostN] = kseg;
            _vallocHostN++;
            _vallocHostPool += span;
            _filesys48dKsegs[i] = kseg;
            return true;
        }

        // Live 68b9567: data-TLBL epc=0x039833A4
        // badvaddr=0x0199B050. IAT page 0x01999000 was
        // mapped; .data vsz continues. 0x0398* is linked
        // preferred (MapDdiNopDestVa aliases fetch). Do
        // not rewrite PC. Firmware PTE only.
        public static uint MapDdiNopVallocDataVa(MipsBus bus, uint va)
        {
            if (_ddiDataBusy)
                return va;
            if (!IsDdiNopVallocDataArmed())
                return va;
            uint use = DdiNopVallocAlias(va);
            if (!IsDdiNopVallocDataVa(use))
                return va;
            uint kseg = LookupDdiDataKseg(use);
            if (kseg != 0)
                return kseg | (use & 0xFFFu);
            TryResolveDdiNopVallocData(bus, use);
            kseg = LookupDdiDataKseg(use);
            if (kseg != 0)
                return kseg | (use & 0xFFFu);
            return va;
        }

        private static bool IsDdiNopVallocDataArmed()
        {
            if (!_ddiNopAwaitCallDll)
                return false;
            return _ddiNopDllMainLogged || _ddiDataDemand;
        }

        private static uint DdiNopVallocAlias(uint va)
        {
            if (va >= DdiNopVbase && va < 0x039B0000u)
                return DdiNopVbasePage + (va - DdiNopVbase);
            return va;
        }

        private static uint DdiNopVallocDataHi()
        {
            uint hi = DdiNopVallocHi;
            if (_ddiNopIatSpan == 0)
                return hi;
            uint end = DdiNopVbasePage + DdiNopIatRva + _ddiNopIatSpan;
            uint pageHi = (end + 0xFFFu) & ~0xFFFu;
            if (pageHi > hi)
                return pageHi;
            return hi;
        }

        private static bool IsDdiNopVallocDataVa(uint va)
        {
            uint use = DdiNopVallocAlias(va);
            return use >= DdiNopVallocLo && use < DdiNopVallocDataHi();
        }

        private static void EnsureDdiDataMaps()
        {
            if (_ddiDataPage != null)
                return;
            _ddiDataPage = new uint[DdiNopDataPageCap];
            _ddiDataKseg = new uint[DdiNopDataPageCap];
            _ddiDataDone = new bool[DdiNopDataPageCap];
            _ddiDataTlbl = new bool[DdiNopDataPageCap];
        }

        private static int FindDdiDataSlot(uint page)
        {
            EnsureDdiDataMaps();
            for (int i = 0; i < _ddiDataN; i++)
            {
                if (_ddiDataPage[i] == page)
                    return i;
            }
            return -1;
        }

        private static int ClaimDdiDataSlot(uint page)
        {
            int i = FindDdiDataSlot(page);
            if (i >= 0)
                return i;
            if (_ddiDataN >= DdiNopDataPageCap)
                return -1;
            i = _ddiDataN;
            _ddiDataN++;
            _ddiDataPage[i] = page;
            return i;
        }

        private static uint LookupDdiDataKseg(uint va)
        {
            int i = FindDdiDataSlot(va & ~0xFFFu);
            if (i < 0)
                return 0;
            return _ddiDataKseg[i];
        }

        private static void TryNoteDdiNopPrefPc(uint epc, uint[] regs)
        {
            if (_ddiPrefPcLogged)
                return;
            if (epc < DdiNopVbase || epc >= 0x039B0000u)
                return;
            _ddiPrefPcLogged = true;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            BootLog.Write("[Hive] ExtraROM ddi_nop ddi-pref-pc epc=0x" +
                epc.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " valloc=0x" + DdiNopVbasePage.ToString("X8") +
                " alias=0x" + DdiNopVallocAlias(epc).ToString("X8") +
                " (linked preferred; dest alias; do not rewrite PC)");
        }

        private static void TryNoteDdiNopVallocDataTlbl(MipsBus bus, uint[] regs,
            uint epc, uint vaddr, uint vector)
        {
            _ddiDataDemand = true;
            TryNoteDdiNopPrefPc(epc, regs);
            uint use = DdiNopVallocAlias(vaddr);
            uint page = use & ~0xFFFu;
            int slot = ClaimDdiDataSlot(page);
            if (slot >= 0 && !_ddiDataTlbl[slot])
            {
                _ddiDataTlbl[slot] = true;
                uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
                uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
                BootLog.Write("[Hive] ExtraROM ddi_nop ddi-data TLBL epc=0x" +
                    epc.ToString("X8") +
                    " badvaddr=0x" + vaddr.ToString("X8") +
                    " vec=0x" + vector.ToString("X8") +
                    " a1=0x" + a1.ToString("X8") +
                    " v0=0x" + v0.ToString("X8") +
                    " (VALLOC .data page 0x" + page.ToString("X8") +
                    "; do not invent dest)");
            }
            TryResolveDdiNopVallocData(bus, use);
        }

        private static void TryResolveDdiNopVallocData(MipsBus bus, uint va)
        {
            if (bus == null || _ddiDataBusy)
                return;
            uint use = DdiNopVallocAlias(va);
            if (!IsDdiNopVallocDataVa(use))
                return;
            uint page = use & ~0xFFFu;
            int slot = FindDdiDataSlot(page);
            if (slot >= 0 && (_ddiDataKseg[slot] != 0 || _ddiDataDone[slot]))
                return;
            try
            {
                _ddiDataBusy = true;
                uint sec = PeekSection(bus, 0);
                uint l1 = 0;
                uint l2 = 0;
                uint pfn = 0;
                uint kseg = 0;
                slot = ClaimDdiDataSlot(page);
                if (slot < 0)
                    return;
                if (sec != 0
                    && WalkFirmwarePte(bus, sec, use, out l1, out l2, out pfn, out kseg)
                    && (kseg & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    RememberDdiDataMap(bus, slot, page, kseg, l2, use,
                        "firmware PTE; VALLOC .data");
                    return;
                }
                uint dest6 = 0;
                uint dest10 = 0;
                if (WalkDdiNopPteDests(bus, use, out l2, out dest6, out dest10)
                    && dest6 != 0
                    && !IsDdiNopDest10Page(dest6)
                    && (dest6 & 0x1FFFFFFFu) >= 0x00010000u)
                {
                    uint word = 0;
                    if (TryPeekWord(bus, dest6, out word))
                    {
                        RememberDdiDataMap(bus, slot, page, dest6, l2, use,
                            "firmware dest6; VALLOC .data");
                        return;
                    }
                }
                // Live 778120c: 0x0199B000 pte-miss dest6=0
                // while neighbors mapped. VALLOC commit ended
                // before this page. Offset 0x2050 < psize
                // 0x297A: file-backed. Alias to dest6-adj /
                // o32.real if those dests already peek. Do
                // not invent dest bytes.
                uint alias = 0;
                uint aliasL2 = 0;
                string why;
                if (TryAliasDdiDataFilePage(bus, page, use, out alias,
                    out aliasL2, out why))
                {
                    RememberDdiDataMap(bus, slot, page, alias, aliasL2, use, why);
                    return;
                }
                if (!_ddiDataDone[slot])
                {
                    _ddiDataDone[slot] = true;
                    uint off = 0;
                    uint dataDest = _ddiNopIatValloc != 0
                        ? _ddiNopIatValloc : (DdiNopVbasePage + DdiNopIatRva);
                    if (page >= dataDest)
                        off = page - dataDest;
                    BootLog.Write("[Hive] ExtraROM ddi_nop ddi-data map va=0x" +
                        page.ToString("X8") +
                        " pte-miss sec=0x" + sec.ToString("X8") +
                        " dest6=0x" + dest6.ToString("X8") +
                        " off=0x" + off.ToString("X") +
                        " psize=0x" + _ddiNopIatPsize.ToString("X") +
                        " real=0x" + _ddiNopIatReal.ToString("X8") +
                        " (VALLOC .data TLBL; do not invent dest)");
                }
            }
            finally
            {
                _ddiDataBusy = false;
            }
        }

        private static void RememberDdiDataMap(MipsBus bus, int slot, uint page,
            uint dest, uint l2, uint va, string why)
        {
            _ddiDataKseg[slot] = dest & ~0xFFFu;
            if (_ddiDataDone[slot])
                return;
            _ddiDataDone[slot] = true;
            uint word = 0;
            TryPeekWord(bus, (dest & ~0xFFFu) | (va & 0xFFFu), out word);
            if (why == null)
                why = "firmware PTE; VALLOC .data";
            BootLog.Write("[Hive] ExtraROM ddi_nop ddi-data map va=0x" +
                page.ToString("X8") +
                " -> 0x" + _ddiDataKseg[slot].ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " (" + why + "; do not invent dest)");
        }

        // Live 778120c: 0x0199B000 has no slot-0 PTE (VALLOC
        // ended at 0x0199B000). File-backed: dest+psize =
        // 0x0199B97A. Map from dest6-adjacent or o32.real
        // only when that dest already peeks.
        private static bool TryAliasDdiDataFilePage(MipsBus bus, uint page,
            uint use, out uint dest, out uint l2, out string why)
        {
            dest = 0;
            l2 = 0;
            why = null;
            int sec;
            uint vsize;
            uint rva;
            uint psize;
            uint dataptr;
            uint real;
            uint flags;
            uint[] blob;
            TryFindDdiNopDataO32(out sec, out vsize, out rva, out psize,
                out dataptr, out real, out flags, out blob);
            if (psize == 0)
                psize = _ddiNopIatPsize;
            if (vsize == 0)
                vsize = _ddiNopIatSpan;
            if (real == 0)
                real = _ddiNopIatReal;
            uint dataDest = _ddiNopIatValloc != 0
                ? _ddiNopIatValloc : (DdiNopVbasePage + DdiNopIatRva);
            if (page < dataDest)
                return false;
            uint off = page - dataDest;
            bool fileBacked = psize == 0 || off < psize;
            uint dest6 = _ddiNopIatDest6;
            uint dest10 = 0;
            if (dest6 == 0)
                WalkDdiNopPteDests(bus, dataDest, out l2, out dest6, out dest10);
            if (dest6 != 0 && !IsDdiNopDest10Page(dest6)
                && (dest6 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                uint cand = (dest6 & ~0xFFFu) + off;
                uint word = 0;
                if (TryPeekWord(bus, cand | (use & 0xFFFu), out word))
                {
                    dest = cand;
                    why = "dest6-adj; file-backed o32[.data]";
                    return true;
                }
            }
            if (page >= 0x1000u)
            {
                uint prevK = LookupDdiDataKseg(page - 0x1000u);
                if (prevK != 0)
                {
                    uint cand = prevK + 0x1000u;
                    uint word = 0;
                    if (TryPeekWord(bus, cand | (use & 0xFFFu), out word))
                    {
                        dest = cand;
                        why = "neighbor-dest; file-backed o32[.data]";
                        return true;
                    }
                }
            }
            if (fileBacked && real != 0)
            {
                uint realVa = (real & ~0xFFFu) + off;
                uint mapped = realVa;
                if (realVa >= 0x01F57000u && realVa < 0x01F67000u)
                    mapped = ExtraRomDestKseg1 + (realVa - 0x01F57000u);
                uint word = 0;
                if (TryPeekWord(bus, mapped | (use & 0xFFFu), out word))
                {
                    dest = mapped;
                    why = "o32.real; file-backed o32[.data]";
                    return true;
                }
            }
            return false;
        }

        // During BindImp, dump-real IAT (o32.real) is the
        // same bytes as VALLOC dest. MapDdiNopDestVa
        // otherwise sends 0x01F57000 to ExtraRomDestKseg1.
        // Do not invent dest.
        public static uint MapBindImpIatRealVa(uint va)
        {
            if (!_ddiNopAwaitCallDll || !_ddiNopIatWatch)
                return va;
            if (_ddiNopIatReal == 0 || _ddiNopIatValloc == 0)
                return va;
            if (IsDdiNopDest10Page(_ddiNopIatValloc))
                return va;
            if (va < _ddiNopIatReal)
                return va;
            uint span = _ddiNopIatSpan != 0 ? _ddiNopIatSpan : 0x1000u;
            if (va >= _ddiNopIatReal + span)
                return va;
            return _ddiNopIatValloc + (va - _ddiNopIatReal);
        }

        private static bool IsDdiNopDest10Page(uint dest)
        {
            if (dest == 0)
                return false;
            return (dest & ~0xFFFu) == (DdiNopDest10Live & ~0xFFFu);
        }

        // Live b4b6454: BindImp IAT stores at vbase+0x19000.
        // One observe. Do not invent PTE.
        private static void TryLogDdiNopIatPage(MipsBus bus)
        {
            if (_ddiNopIatLogged)
                return;
            _ddiNopIatLogged = true;
            uint va = DdiNopVbasePage + DdiNopIatRva;
            uint l2 = 0;
            uint dest6 = 0;
            uint dest10 = 0;
            WalkDdiNopPteDests(bus, va, out l2, out dest6, out dest10);
            if (dest6 != 0 && !IsDdiNopDest10Page(dest6))
                _ddiNopIatDest6 = dest6;
            bool threw;
            uint word = 0;
            bool mapped = false;
            if (dest6 != 0 && !IsDdiNopDest10Page(dest6))
            {
                word = PeekDestWordRaw(bus, dest6, out threw);
                mapped = !threw;
            }
            if (!mapped)
            {
                word = PeekDestWordRaw(bus, va, out threw);
                mapped = !threw;
            }
            bool writable = false;
            if (mapped)
            {
                uint poke = dest6 != 0 && !IsDdiNopDest10Page(dest6) ? dest6 : va;
                try
                {
                    bool raw = dest6 != 0 && !IsDdiNopDest10Page(dest6);
                    if (raw)
                        _ddiNopDestPeekRaw = true;
                    try
                    {
                        bus.Write32(poke, word);
                        writable = true;
                    }
                    finally
                    {
                        if (raw)
                            _ddiNopDestPeekRaw = false;
                    }
                }
                catch
                {
                }
            }
            BootLog.Write("[Hive] ExtraROM ddi_nop IAT va=0x" +
                va.ToString("X8") +
                " dest6=0x" + dest6.ToString("X8") +
                " l2=0x" + l2.ToString("X8") +
                " word=0x" + word.ToString("X8") +
                (mapped ? " mapped" : " unmapped") +
                (writable ? " writable" : " not-writable"));
        }

        // First guest store into VALLOC IAT 0x01999000 /
        // dest6. Host IAT poke sets destPeekRaw. Do not
        // invent the written word.
        public static void TryNoteDdiNopIatStore(uint origVa, uint mappedVa, uint value)
        {
            if (!_ddiNopIatWatch || _ddiNopDestPeekRaw)
                return;
            uint iat = DdiNopVbasePage + DdiNopIatRva;
            uint dest6 = _ddiNopIatDest6;
            uint page = origVa & ~0xFFFu;
            uint mappedPage = mappedVa & ~0xFFFu;
            bool hit = page == iat
                || mappedPage == iat
                || (dest6 != 0 && !IsDdiNopDest10Page(dest6)
                    && (page == (dest6 & ~0xFFFu)
                        || mappedPage == (dest6 & ~0xFFFu)));
            if (!hit)
                return;
            uint baseVa = page == iat ? iat
                : mappedPage == iat ? iat
                : (dest6 & ~0xFFFu);
            uint slotVa = page == iat ? origVa
                : mappedPage == iat ? mappedVa
                : (mappedPage == (dest6 & ~0xFFFu) ? mappedVa : origVa);
            uint slot = (slotVa - baseVa) / 4;
            _ddiNopIatStoreLogged = true;
            if (_ddiNopIatStoreN >= BindImpObserveMax)
                return;
            _ddiNopIatStoreN++;
            BootLog.Write("[Hive] ExtraROM ddi_nop IAT-store va=0x" +
                origVa.ToString("X8") +
                " dest6=0x" + dest6.ToString("X8") +
                " word=0x" + value.ToString("X8") +
                " slot=" + slot);
        }

        private static bool TryFindDdiNopDataO32(out int sec, out uint vsize,
            out uint rva, out uint psize, out uint dataptr, out uint real,
            out uint flags, out uint[] blob)
        {
            sec = -1;
            vsize = 0;
            rva = 0;
            psize = 0;
            dataptr = 0;
            real = 0;
            flags = 0;
            blob = null;
            uint[] words = _ddiNopO32Words;
            uint[][] data = _ddiNopData;
            if (words == null || words.Length < 6)
            {
                ExtraRomTocMod slot = FindCachedExtraRomToc("ddi_nop.dll");
                if (slot != null)
                {
                    words = slot.O32Words;
                    data = slot.Data;
                }
            }
            if (words == null || words.Length < 6)
                return false;
            int nsec = words.Length / 6;
            uint iatVa = DdiNopVbasePage + DdiNopIatRva;
            uint iatDump = DdiNopVbase + DdiNopIatRva;
            for (int s = 0; s < nsec; s++)
            {
                uint vs = words[s * 6];
                uint rv = words[s * 6 + 1];
                uint rl = words.Length > s * 6 + 4 ? words[s * 6 + 4] : 0;
                uint span = vs == 0 ? 0 : vs;
                bool covers = span != 0
                    && rv <= DdiNopIatRva
                    && DdiNopIatRva < rv + span;
                if (!covers && rl != 0)
                {
                    uint slotReal = rl & SlotMask;
                    covers = (rl & ~0xFFFu) == (iatDump & ~0xFFFu)
                        || (slotReal & ~0xFFFu) == (iatVa & ~0xFFFu);
                }
                if (!covers)
                    continue;
                sec = s;
                vsize = vs;
                rva = rv != 0 ? rv : DdiNopIatRva;
                psize = words[s * 6 + 2];
                dataptr = words[s * 6 + 3];
                real = rl;
                flags = words.Length > s * 6 + 5 ? words[s * 6 + 5] : 0;
                if (data != null && s < data.Length)
                    blob = data[s];
                return true;
            }
            return false;
        }

        private static bool DdiNopO32LooksCompressed(uint vsize, uint psize,
            uint flags, uint[] blob)
        {
            if (psize == 0)
                return false;
            if ((flags & O32Compressed) != 0)
                return true;
            if (psize < vsize)
                return true;
            if (blob == null || blob.Length == 0)
                return false;
            uint first = blob[0];
            uint declared = first & 0x00FFFFFFu;
            uint sig = first >> 24;
            return declared == vsize
                || sig == 0xB5 || sig == 0xB4
                || sig == 0x11 || sig == 0x0C;
        }

        private static bool TryWriteDdiNopVallocWord(MipsBus bus, uint va, uint word)
        {
            if (bus == null || va == 0 || IsDdiNopDest10Page(va))
                return false;
            uint l2;
            uint dest6;
            uint dest10;
            WalkDdiNopPteDests(bus, va, out l2, out dest6, out dest10);
            if (IsDdiNopDest10Page(dest6))
                return false;
            try
            {
                if (dest6 != 0)
                {
                    _ddiNopDestPeekRaw = true;
                    try
                    {
                        bus.Write32(dest6, word);
                    }
                    finally
                    {
                        _ddiNopDestPeekRaw = false;
                    }
                    return true;
                }
                bus.Write32(va, word);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Live b4b6454: .text CEDecompress only. IAT lives in
        // .data RVA 0x19000. Serve that o32's VALLOC dest the
        // same way (.text CopyO32 / dest6). Do not invent
        // dest10 or a new image. Honest skip-no-o32.
        private static void TryServeDdiNopDataO32(MipsBus bus)
        {
            if (_ddiNopDataO32Logged)
                return;
            _ddiNopDataO32Logged = true;
            int sec;
            uint vsize;
            uint rva;
            uint psize;
            uint dataptr;
            uint real;
            uint flags;
            uint[] blob;
            if (!TryFindDdiNopDataO32(out sec, out vsize, out rva, out psize,
                out dataptr, out real, out flags, out blob))
            {
                BootLog.Write("[Hive] ExtraROM o32[.data] skip-no-o32" +
                    " rva=0x" + DdiNopIatRva.ToString("X") +
                    " (TOC has no .data o32; do not invent)");
                return;
            }
            uint dest = DdiNopVbasePage + rva;
            if (IsDdiNopDest10Page(dest))
            {
                BootLog.Write("[Hive] ExtraROM o32[.data] s=" + sec +
                    " rva=0x" + rva.ToString("X") +
                    " dest=0x" + dest.ToString("X8") +
                    " skip-dest10");
                return;
            }
            uint l2;
            uint dest6;
            uint dest10;
            WalkDdiNopPteDests(bus, dest, out l2, out dest6, out dest10);
            if (IsDdiNopDest10Page(dest6))
                dest6 = 0;
            bool compressed = DdiNopO32LooksCompressed(vsize, psize, flags, blob);
            string why;
            uint filled = 0;
            if (dest6 == 0)
                TryHostBackValloc(dest, dest, 0x1000u, 0x1000u, false);
            if (psize > 0 && !compressed)
            {
                uint n = psize;
                if (n > 0x20000u)
                    n = 0x20000u;
                uint[] src = blob;
                for (uint i = 0; i < n; i += 4)
                {
                    uint word = 0;
                    uint w = i / 4;
                    if (src != null && w < (uint)src.Length)
                        word = src[w];
                    if (TryWriteDdiNopVallocWord(bus, dest + i, word))
                        filled += 4;
                }
                why = dest6 != 0 ? "set-copyo32" : "set-copyo32-host";
                if (filled == 0)
                    why = dest6 == 0 ? "skip-unmapped" : "skip-write";
            }
            else if (psize == 0)
            {
                // BSS CopyO32: zero the IAT page only.
                for (uint i = 0; i < 0x1000u; i += 4)
                {
                    if (TryWriteDdiNopVallocWord(bus, dest + i, 0))
                        filled += 4;
                }
                why = dest6 != 0 ? "set-bss-zero" : "set-bss-host";
                if (filled == 0)
                    why = dest6 == 0 ? "skip-unmapped" : "skip-write";
            }
            else if (dest6 != 0)
            {
                // Compressed TOC blob. Do not host-CEDecompress
                // ExtraROM (no host LZX; do not invent dest
                // bytes). dest6 is the firmware dest; BindImp
                // can store IAT. Dest10 never.
                why = "set-dest6";
            }
            else if (TryWriteDdiNopVallocWord(bus, dest, 0))
            {
                filled = 4;
                why = "set-valloc-commit";
            }
            else
                why = "skip-unmapped";
            if (real != 0 && dest != 0 && !IsDdiNopDest10Page(dest))
            {
                _ddiNopIatReal = real;
                _ddiNopIatValloc = dest;
                _ddiNopIatSpan = vsize;
                _ddiNopIatPsize = psize;
            }
            BootLog.Write("[Hive] ExtraROM o32[.data] s=" + sec +
                " rva=0x" + rva.ToString("X") +
                " vsz=0x" + vsize.ToString("X") +
                " psize=0x" + psize.ToString("X") +
                " dest=0x" + dest.ToString("X8") +
                " dest6=0x" + dest6.ToString("X8") +
                " " + why +
                " n=0x" + filled.ToString("X") +
                (dataptr != 0 ? " dp=0x" + dataptr.ToString("X8") : "") +
                (real != 0 ? " real=0x" + real.ToString("X8") : ""));
        }

        private const uint ModuleLpSelf = 0;
        private const uint ModulePmodNext = 4;
        private const int DdiNopWalkCap = 32;
        private const int DdiNopWalkSeedMax = 12;

        private static void ResetDdiNopModuleHunt()
        {
            _ddiNopBindLibV0 = 0;
            _ddiNopBindLibName = null;
            _coredllModule = 0;
            _coredllBasePtrLogged = false;
            _ddiNopFileObj = 0;
            _ddiNopStartipAttempted = false;
            _ddiNopAwaitCallDll = false;
            _ddiNopSawCallDllPc = false;
            _ddiNopCallDllMissLogged = false;
            _ddiNopCallDllMissPoll = 0;
            _ddiNopStallLogged = false;
            _ddiNopIatLogged = false;
            _ddiNopDataO32Logged = false;
            _ddiNopIatWatch = false;
            _ddiNopIatStoreLogged = false;
            _ddiNopIatStoreN = 0;
            _ddiNopIatDest6 = 0;
            _ddiNopIatReal = 0;
            _ddiNopIatValloc = 0;
            _ddiNopIatSpan = 0;
            _ddiNopIatPsize = 0;
            _bindImpIatSlotLog = 0;
            _ddiNopOrdLog = 0;
            _ddiNopOrdLastA1 = 0;
            _ddiNopOrdRetLog = 0;
            _ddiNopOrdRetLastA1 = 0;
            _ddiNopOrdExpLogged = false;
            _ddiNopOrdGoodV0 = 0;
            _ddiNopOrdAfterDone = false;
            _ddiNopOrdAfterN = 0;
            _ddiNopOrdAfterLast = 0;
            _userKPageAlias = false;
            _userKPageAliasNoted = false;
            _ffffFce1Logged = false;
            _ffffF000Kseg = 0;
            _ffffF000Logged = false;
            _ffffF000Busy = false;
            _ffffF000Demand = false;
            _ffffF000Done = false;
            _bindImpIatSwExpect = false;
            _bindImpIatSwLogged = false;
            _bindImpIatSwLog = 0;
            _bindImpIatWinLog = 0;
            _bindImpIatWinLast = 0;
            _bindImpIatNextLog = 0;
            _bindImpIatNextLast = 0;
            _bindImpExnLogged = false;
            _bindImpExnSaveLogged = false;
            _bindImpExnCode = 0;
            _bindImpExnEpc = 0;
            _bindImpExnVaddr = 0;
            _gwesB9SpinLogged = false;
            _gwesB9SpinPage = 0;
            _gwesB9SpinN = 0;
            _gwesNullStoreLogged = false;
            _nearNullTlblLogged = false;
            _ffffFb2aAdelLogged = false;
            _adelC6FaLogged = false;
            _adelPcEpc = 0;
            _adelPcSp = 0;
            _adelPlantClrLogged = false;
            _idleHaltLogged = false;
            _exnContinueWord = 0;
            _thrSpLogged = false;
            _spFixLogged = false;
            _plantFixLogged = false;
            _plantHaltLogged = false;
            _leftoverHaltLogged = false;
            _leftoverSkipLogged = false;
            _leftoverFrameLogged = false;
            _epcHaltLogged = false;
            _c2TlbsLogged = false;
            _c2SpLogged = false;
            _c2EretHaltLogged = false;
            _ddiNopInfoObserved = false;
            _ddiNopInfoDemand = false;
            _ddiNopInfoBusy = false;
            _ddiNopInfoPeekRaw = false;
            _ddiNopInfoMapLogged = false;
            _ddiNopInfoKseg = 0;
            _ddiNopCallDllHiveLogged = false;
            _ddiNopDllMainLogged = false;
            _ddiNopDllMainRa = 0;
            _ddiNopCallDllSite = 0;
            _ddiNopAfterDllMainLogged = false;
            _ddiNopGwesFetchKseg = 0;
            _ddiNopGwesFetchLogged = false;
            _ddiNopGwesFetchBusy = false;
            _ddiNopGwesFetchDemand = false;
            _ddiNopGwesFetchTlblLogged = false;
            _ddiNopGwesDataKseg = 0;
            _ddiNopGwesDataLogged = false;
            _ddiNopGwesDataBusy = false;
            _ddiNopGwesDataDemand = false;
            _ddiNopGwesDataTlblLogged = false;
            _ddiNopGwesTextKseg = 0;
            _ddiNopGwesTextLogged = false;
            _ddiNopGwesTextBusy = false;
            _ddiNopGwesTextDemand = false;
            _ddiNopGwesTextTlblLogged = false;
            _ddiNopGwesData2Kseg = 0;
            _ddiNopGwesData2Logged = false;
            _ddiNopGwesData2Busy = false;
            _ddiNopGwesData2Demand = false;
            _ddiNopGwesData2TlblLogged = false;
            _ddiNopGwesData3Kseg = 0;
            _ddiNopGwesData3Logged = false;
            _ddiNopGwesData3Busy = false;
            _ddiNopGwesData3Demand = false;
            _ddiNopGwesData3TlblLogged = false;
            _ddiNopGwesText2Kseg = 0;
            _ddiNopGwesText2Logged = false;
            _ddiNopGwesText2Busy = false;
            _ddiNopGwesText2Demand = false;
            _ddiNopGwesText2TlblLogged = false;
            _gwesImageDemand = false;
            _gwesImageBusy = false;
            _gwesImageN = 0;
            if (_gwesImagePage != null)
            {
                for (int i = 0; i < _gwesImagePage.Length; i++)
                {
                    _gwesImagePage[i] = 0;
                    _gwesImageKseg[i] = 0;
                    _gwesImageDone[i] = false;
                    _gwesImageTlbl[i] = false;
                }
            }
            _coredllImageDemand = false;
            _coredllImageBusy = false;
            _coredllImageN = 0;
            _coredllSlotViewLogged = false;
            _coredllSlotViewTlbl = false;
            if (_coredllImagePage != null)
            {
                for (int i = 0; i < _coredllImagePage.Length; i++)
                {
                    _coredllImagePage[i] = 0;
                    _coredllImageKseg[i] = 0;
                    _coredllImageDone[i] = false;
                    _coredllImageTlbl[i] = false;
                }
            }
            _filesysSlot2Kseg = 0;
            _filesysSlot2Logged = false;
            _filesysSlot2Busy = false;
            _filesysSlot2Demand = false;
            _filesysSlot2TlblLogged = false;
            _filesysSlot4Logged = false;
            _filesysSlot4TlblLogged = false;
            _filesysSlot2ExtraBusy = false;
            _filesysSlot2ExtraN = 0;
            if (_filesysSlot2ExtraPage != null)
            {
                for (int i = 0; i < _filesysSlot2ExtraPage.Length; i++)
                {
                    _filesysSlot2ExtraPage[i] = 0;
                    _filesysSlot2ExtraKseg[i] = 0;
                    _filesysSlot2ExtraLogged[i] = false;
                    _filesysSlot2ExtraTlbl[i] = false;
                    _filesysSlot2ExtraMiss[i] = false;
                }
            }
            _filesys48dLogged = false;
            _filesys48dBusy = false;
            if (_filesys48dKsegs != null)
            {
                for (int i = 0; i < _filesys48dKsegs.Length; i++)
                {
                    _filesys48dKsegs[i] = 0;
                    _filesys48dDone[i] = false;
                    _filesys48dTlbl[i] = false;
                }
            }
            _ddiDataDemand = false;
            _ddiDataBusy = false;
            _ddiDataN = 0;
            _ddiPrefPcLogged = false;
            if (_ddiDataPage != null)
            {
                for (int i = 0; i < _ddiDataPage.Length; i++)
                {
                    _ddiDataPage[i] = 0;
                    _ddiDataKseg[i] = 0;
                    _ddiDataDone[i] = false;
                    _ddiDataTlbl[i] = false;
                }
            }
            _ddiNopWalkSeedN = 0;
            _ddiNopNoModDiag = false;
            _ddiNopWalkDiag = false;
            if (_ddiNopWalkSeeds != null)
            {
                for (int i = 0; i < _ddiNopWalkSeeds.Length; i++)
                    _ddiNopWalkSeeds[i] = 0;
            }
        }

        // Sticky ddi_nop file object. ClearLoadE32OkWatch
        // must not drop this; first startip is after the
        // 200k cap. Reset only on Boot / hunt reset.
        private static void LatchDdiNopFileObj(uint obj)
        {
            if (obj == 0 || obj == 0xDEADBEEFu)
                return;
            if (_ddiNopFileObj == 0)
                _ddiNopFileObj = obj;
        }

        // Live e29762a: $fp is often the heap file object.
        // Accept only a trusted MODULE whose +96 is the
        // sticky TOC object. Do not invent obj-96.
        private static void TryHuntDdiNopModuleFromRegs(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null)
                return;
            if (_ddiNopModule != 0 && IsDdiNopModule(bus, _ddiNopModule))
                return;
            if (_ddiNopFileObj == 0
                && !NamesMatchRom(_loadE32OkName, "ddi_nop.dll")
                && !NamesMatchRom(_loadE32WatchName, "ddi_nop.dll"))
                return;
            NoteDdiNopWalkSeeds(regs);
            if (_ddiNopWalkSeeds == null)
                return;
            for (int i = 0; i < _ddiNopWalkSeedN; i++)
            {
                if (AcceptDdiNopModule(bus, _ddiNopWalkSeeds[i]) != 0)
                    return;
            }
        }

        private static void NoteDdiNopWalkSeed(uint va)
        {
            if (va == 0 || va == 0xDEADBEEFu)
                return;
            if (_ddiNopWalkSeeds == null)
                _ddiNopWalkSeeds = new uint[DdiNopWalkSeedMax];
            for (int i = 0; i < _ddiNopWalkSeedN; i++)
            {
                if (_ddiNopWalkSeeds[i] == va)
                    return;
            }
            if (_ddiNopWalkSeedN >= _ddiNopWalkSeeds.Length)
                return;
            _ddiNopWalkSeeds[_ddiNopWalkSeedN++] = va;
        }

        private static void NoteDdiNopWalkSeeds(uint[] regs)
        {
            if (regs == null)
                return;
            for (int r = 16; r <= 23 && r < regs.Length; r++)
                NoteDdiNopWalkSeed(regs[r]);
            if (regs.Length > 30)
                NoteDdiNopWalkSeed(regs[30]);
            if (regs.Length > 2)
                NoteDdiNopWalkSeed(regs[2]);
        }

        // Pointer oe: module+96 is either the embedded
        // openexe or a pointer to the heap TOC-attach
        // object. Do not invent obj-96.
        private static bool IsDdiNopModule(MipsBus bus, uint module)
        {
            if (bus == null || module == 0)
                return false;
            if (IsDdiNopTocObject(bus, module + ModuleFileObj))
                return true;
            uint p;
            if (!TryPeekWord(bus, module + ModuleFileObj, out p) || p == 0)
                return false;
            if (_ddiNopFileObj != 0 && p == _ddiNopFileObj)
                return true;
            if (IsDdiNopTocObject(bus, p))
                return true;
            if (p != _loadE32Obj && p != _loadE32OkObj && p != _loadE32WatchA0)
                return false;
            return IsDdiNopTocObject(bus, p);
        }

        private static bool IsTrustedModule(MipsBus bus, uint module)
        {
            if (module == 0 || module == 0xDEADBEEFu)
                return false;
            uint self;
            if (!TryPeekWord(bus, module + ModuleLpSelf, out self))
                return false;
            return self == module;
        }

        private static bool MatchesDdiNopRamOrDumpStartip(MipsBus bus, uint module)
        {
            uint ip;
            if (!TryPeekWord(bus, module + ModuleStartip, out ip) || ip == 0)
                return false;
            if (ip == DdiNopVbasePage + DdiNopEntryRvaExtract)
                return true;
            return ip == DdiNopVbase + DdiNopEntryRvaExtract;
        }

        private static uint AcceptDdiNopModule(MipsBus bus, uint module)
        {
            if (module == 0)
                return 0;
            if (IsDdiNopModule(bus, module))
            {
                _ddiNopModule = module;
                return module;
            }
            if (IsTrustedModule(bus, module) && MatchesDdiNopRamOrDumpStartip(bus, module))
            {
                _ddiNopModule = module;
                return module;
            }
            return 0;
        }

        private static uint WalkDdiNopModuleList(MipsBus bus, uint seed)
        {
            uint m = seed;
            for (int i = 0; i < DdiNopWalkCap && m != 0 && m != 0xDEADBEEFu; i++)
            {
                if (!IsTrustedModule(bus, m))
                    return 0;
                uint hit = AcceptDdiNopModule(bus, m);
                if (hit != 0)
                    return hit;
                uint next;
                if (!TryPeekWord(bus, m + ModulePmodNext, out next))
                    return 0;
                if (next == 0 || next == m)
                    return 0;
                m = next;
            }
            return 0;
        }

        // Live 9183b83: skip-no-mod. Heap TOC-attach
        // openexe is not an embedded MODULE+96. Walk
        // pmodNext from CurProc+0x50, BindImp LoadLibrary
        // ret v0, and $fp / callee-saved. No invent.
        private static uint FindInFlightDdiNopModule(MipsBus bus, uint hintModule)
        {
            uint hit = AcceptDdiNopModule(bus, hintModule);
            if (hit != 0)
                return hit;
            hit = AcceptDdiNopModule(bus, _ddiNopModule);
            if (hit != 0)
                return hit;
            uint fromObj = ModuleFromEmbeddedDdiNopFileObj(bus, _ddiNopFileObj);
            if (fromObj == 0)
                fromObj = ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32OkObj);
            if (fromObj == 0)
                fromObj = ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32Obj);
            if (fromObj == 0)
                fromObj = ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32WatchA0);
            if (fromObj != 0)
            {
                _ddiNopModule = fromObj;
                return fromObj;
            }
            uint proc = 0;
            uint p50 = 0;
            if (TryPeekWord(bus, CurProc, out proc) && proc != 0)
                TryPeekWord(bus, proc + ProcModule, out p50);
            hit = WalkDdiNopModuleList(bus, p50);
            if (hit != 0)
                return hit;
            hit = WalkDdiNopModuleList(bus, hintModule);
            if (hit != 0)
                return hit;
            hit = WalkDdiNopModuleList(bus, _ddiNopBindLibV0);
            if (hit != 0)
                return hit;
            if (_ddiNopWalkSeeds != null)
            {
                for (int i = 0; i < _ddiNopWalkSeedN; i++)
                {
                    uint seed = _ddiNopWalkSeeds[i];
                    hit = AcceptDdiNopModule(bus, seed);
                    if (hit != 0)
                        return hit;
                    hit = WalkDdiNopModuleList(bus, seed);
                    if (hit != 0)
                        return hit;
                }
            }
            return 0;
        }

        // obj-96 is the MODULE only when obj is the
        // embedded openexe. A standalone heap file
        // object minus 96 is a corrupt write.
        private static uint ModuleFromEmbeddedDdiNopFileObj(MipsBus bus, uint obj)
        {
            if (obj < ModuleFileObj || !IsDdiNopTocObject(bus, obj))
                return 0;
            uint module = obj - ModuleFileObj;
            if (module < 0x80000000u || module >= 0xC0000000u)
                return 0;
            if (!IsDdiNopTocObject(bus, module + ModuleFileObj))
                return 0;
            if (!IsTrustedModule(bus, module))
                return 0;
            uint unused;
            if (!TryPeekWord(bus, module + ModuleStartip, out unused))
                return 0;
            if (!TryPeekWord(bus, module + ProcModule, out unused))
                return 0;
            return module;
        }

        private static void LogDdiNopNoModOnce(MipsBus bus)
        {
            if (_ddiNopNoModDiag)
                return;
            _ddiNopNoModDiag = true;
            uint proc = 0;
            uint p50 = 0;
            TryPeekWord(bus, CurProc, out proc);
            if (proc != 0)
                TryPeekWord(bus, proc + ProcModule, out p50);
            bool emb = ModuleFromEmbeddedDdiNopFileObj(bus, _ddiNopFileObj) != 0
                || ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32OkObj) != 0
                || ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32Obj) != 0
                || ModuleFromEmbeddedDdiNopFileObj(bus, _loadE32WatchA0) != 0;
            bool p50Self = IsTrustedModule(bus, p50);
            BootLog.Write("[Hive] ExtraROM ddi_nop skip-no-mod obj=0x" +
                _loadE32Obj.ToString("X8") +
                " okObj=0x" + _loadE32OkObj.ToString("X8") +
                " sticky=0x" + _ddiNopFileObj.ToString("X8") +
                " emb=" + (emb ? "1" : "0") +
                " CurProc=0x" + proc.ToString("X8") +
                " +50=0x" + p50.ToString("X8") +
                " lpSelf=" + (p50Self ? "1" : "0"));
            LogDdiNopBindWalkOnce(bus);
        }

        // Always walk 3 nodes from the live BindImp
        // LoadLibrary ret MODULE, even when CurProc+50
        // failed lpSelf. Do not invent the list head.
        private static void LogDdiNopBindWalkOnce(MipsBus bus)
        {
            if (_ddiNopWalkDiag || _ddiNopBindLibV0 == 0)
                return;
            _ddiNopWalkDiag = true;
            string walk = FormatDdiNopWalk(bus, _ddiNopBindLibV0, 3);
            if (walk.Length == 0)
                walk = "v0=0x" + _ddiNopBindLibV0.ToString("X8") +
                    (IsTrustedModule(bus, _ddiNopBindLibV0) ? "" : " not-lpSelf");
            BootLog.Write("[Hive] ExtraROM ddi_nop walk " + walk);
        }

        private static string FormatDdiNopWalk(MipsBus bus, uint seed, int cap)
        {
            if (!IsTrustedModule(bus, seed))
                return "";
            string walk = "";
            uint m = seed;
            int n = 0;
            while (n < cap && m != 0 && m != 0xDEADBEEFu)
            {
                if (!IsTrustedModule(bus, m))
                    break;
                uint oe = 0;
                uint ip = 0;
                TryPeekWord(bus, m + ModuleFileObj, out oe);
                TryPeekWord(bus, m + ModuleStartip, out ip);
                if (walk.Length > 0)
                    walk += " ";
                walk += "m=0x" + m.ToString("X8") +
                    " +96=0x" + oe.ToString("X8") +
                    " ip=0x" + ip.ToString("X8");
                uint next;
                if (!TryPeekWord(bus, m + ModulePmodNext, out next) || next == 0 || next == m)
                    break;
                m = next;
                n++;
            }
            return walk;
        }

        // PTE dest6 at RAM entry 0x01998014. Peek 0 is
        // honest; do not invent 0x27BDFFD8.
        private static uint PeekDdiNopRamEntryWord(MipsBus bus)
        {
            uint va = DdiNopVbasePage + DdiNopEntryRvaExtract;
            uint l2;
            uint dest6;
            uint dest10;
            if (WalkDdiNopPteDests(bus, va, out l2, out dest6, out dest10)
                && dest6 != 0)
                return PeekDestWordRaw(bus, dest6, out _);
            return PeekDestWordRaw(bus, va, out _);
        }

        private static bool IsCallDllSkipUseg(uint p50)
        {
            if (p50 < 0x80000000u)
                return true;
            return (p50 & 0xFF000000u) == 0xC2000000u;
        }

        private static uint DumpTocVbase(ExtraRomTocMod slot)
        {
            if (slot == null)
                return 0;
            if (slot.Vbase != 0)
                return slot.Vbase;
            if (slot.E32Words != null && slot.E32Words.Length > 2 && slot.E32Words[2] != 0)
                return slot.E32Words[2];
            if (slot.O32Words != null && slot.O32Words.Length >= 5
                && slot.O32Words[1] == 0x1000 && slot.Dest >= 0x1000)
                return slot.Dest - 0x1000;
            return 0;
        }

        public static bool TryServeExtraRomLoadLibrary(MipsBus bus, string name, uint[] regs)
        {
            if (regs == null || regs.Length <= 2 || regs[2] != 0)
                return false;
            string baseName = FileBaseName(name);
            ExtraRomTocMod slot = FindCachedExtraRomToc(baseName);
            if (slot == null && !string.IsNullOrEmpty(baseName))
                slot = FindCachedExtraRomToc(name);
            if (slot == null)
                return false;
            uint destDump = slot.Dest;
            uint dest0 = destDump & SlotMask;
            uint hdr = 0;
            if (slot.Data != null && slot.Data.Length > 0
                && slot.Data[0] != null && slot.Data[0].Length > 0)
                hdr = slot.Data[0][0];
            if (NamesMatchRom(slot.Name, "ddi_nop.dll") && dest0 == 0x01981000u)
                TryMeasureDdiNopDestAfterDecomp(bus, hdr, _ddiNopDecompVsize);
            uint wordDump = PeekDestWordRaw(bus, destDump, out _);
            uint word0 = PeekDestWordRaw(bus, dest0, out _);
            // Live 021a2eb: dest6+0x1000 sig 0x8C481B78 is
            // dump .text RVA 0x2000. Serve dest6; vbase is
            // VALLOC 0x01980000. Do not require MZ at vbase.
            // Do not serve dest10. Do not invent dest.
            uint fwDest = 0;
            uint fwWord = 0;
            if (!DdiNopDestStoresAllowServe())
            {
                fwDest = 0;
                fwWord = 0;
            }
            else if (_ddiNopLandedBySig && _ddiNopLandedDest != 0
                && (_ddiNopLandedDest & ~0xFFFu) != DdiNopDest10Live)
            {
                fwDest = _ddiNopLandedDest;
                fwWord = _ddiNopLandedWord;
            }
            else if (_ddiNopLandedDest != 0 && IsMzWord(_ddiNopLandedWord)
                && (_ddiNopLandedDest & ~0xFFFu) != DdiNopDest10Live)
            {
                fwDest = _ddiNopLandedDest;
                fwWord = _ddiNopLandedWord;
            }
            uint vbase;
            if (_ddiNopLandedBySig && fwDest != 0)
                vbase = DdiNopVbasePage;
            else if (fwDest != 0)
                vbase = fwDest;
            else
                vbase = DumpTocVbase(slot);
            string why;
            if (fwDest == 0)
            {
                if (!DdiNopDestStoresAllowServe())
                    why = "dest0 dest6 vbase6 store-count=0; do not serve";
                else
                    why = "dest6 .text sig miss; do not serve dest10";
            }
            else if (_ddiNopLandedBySig)
                why = "dest6 .text sig; serve dest6";
            else
                why = "vbase MZ; serve vbase dest";
            BootLog.Write("[Hive] TOC[" + slot.Index + "] " + slot.Name +
                " LoadLibrary v0=0 destDump=0x" + destDump.ToString("X8") +
                " dump-word=0x" + wordDump.ToString("X") +
                " dest0=0x" + dest0.ToString("X8") +
                " dest0-word=0x" + word0.ToString("X") +
                " landed=0x" + fwDest.ToString("X8") +
                " landed-word=0x" + fwWord.ToString("X") +
                " " + why);
            if (fwDest == 0 || vbase == 0)
            {
                BootLog.Rom("miss", "ExtraROM", "TOC", slot.Index, slot.Name, 7, destDump, wordDump, vbase, why);
                return false;
            }
            slot.DecompDest = fwDest;
            slot.Vbase = vbase;
            regs[2] = fwDest;
            BootLog.Rom("ok", "ExtraROM", "TOC", slot.Index, slot.Name, 7, fwDest, fwWord, regs[2], why);
            return true;
        }

        // Dump-cached e32 size. o32 follows that copy at
        // LiveE32+e32Bytes (TOC+0x18). Do not pack o32 at
        // +0x5C: that was leftover CurMSec a1, not a pointer.
        private static uint ExtraRomHostE32Bytes(ExtraRomTocMod slot)
        {
            if (slot == null || slot.E32Words == null || slot.E32Words.Length == 0)
                return 0;
            return (uint)slot.E32Words.Length * 4;
        }

        private static bool WriteHostExtraRomE32O32(MipsBus bus, ExtraRomTocMod slot, string name)
        {
            try
            {
                for (int i = 0; i < slot.TocWords.Length && i < 8; i++)
                    bus.Write32(slot.LiveEntry + (uint)(i * 4), slot.TocWords[i]);
                bus.Write32(slot.LiveEntry + 0x14, slot.LiveE32);
                bus.Write32(slot.LiveEntry + 0x18, slot.LiveO32);
                if (slot.LiveName != 0)
                    bus.Write32(slot.LiveEntry + 0x10, slot.LiveName);
                uint e32Bytes = ExtraRomHostE32Bytes(slot);
                int e32n = (int)(e32Bytes / 4);
                if (e32n > slot.E32Words.Length)
                    e32n = slot.E32Words.Length;
                for (int i = 0; i < e32n; i++)
                    bus.Write32(slot.LiveE32 + (uint)(i * 4), slot.E32Words[i]);
                if (slot.O32Words != null && slot.LiveO32 != 0)
                {
                    for (int i = 0; i < slot.O32Words.Length; i++)
                        bus.Write32(slot.LiveO32 + (uint)(i * 4), slot.O32Words[i]);
                }
                if (slot.LiveName != 0 && !string.IsNullOrEmpty(name))
                {
                    for (int i = 0; i < name.Length; i++)
                        bus.Write8(slot.LiveName + (uint)i, (byte)name[i]);
                    bus.Write8(slot.LiveName + (uint)name.Length, 0);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[Hive] ExtraROM TOC[" + slot.Index + "] " +
                    slot.Name + " e32-host-fail " + ex.Message);
                return false;
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
                    if (!NamesMatchRom(baseName, ReadAscii(bus, name)))
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
                    if (!NamesMatchRom(baseName, ReadAscii(bus, name)))
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

        private static bool TryFindNkFile(MipsBus bus, string baseName,
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
                uint toc = bus.Read32(EcecTocPtr);
                if (toc == 0)
                    return false;
                uint nmods = bus.Read32(toc + RomHdrNumMods);
                uint nfiles = bus.Read32(toc + RomHdrNumFiles);
                if (nmods > 80 || nfiles == 0 || nfiles > 80)
                    return false;
                uint first = toc + TocFirst + nmods * TocEntrySize;
                for (uint i = 0; i < nfiles; i++)
                {
                    uint entry = first + i * FilesEntrySize;
                    uint name = bus.Read32(entry + FilesNameOff);
                    if (!NamesMatchRom(baseName, ReadAscii(bus, name)))
                        continue;
                    attr = bus.Read32(entry);
                    real = bus.Read32(entry + FilesRealSize);
                    comp = bus.Read32(entry + FilesCompSize);
                    load = bus.Read32(entry + FilesLoadOff);
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
                // RAM .text sig landed: do not fill dump XIP
                // 0x03998014. Set VALLOC startip on this module.
                if (_ddiNopLandedBySig && IsDdiNopModule(bus, module))
                {
                    TrySetDdiNopRamStartip(bus, module);
                    return;
                }
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

        public static void NoteDdiNopCallDllPc(MipsBus bus, uint[] regs, uint pc)
        {
            // Live c231655: any module's CallDLL PC set saw
            // before await armed, so the poll never logged.
            // Only this load, only ddi_nop.
            if (!_ddiNopAwaitCallDll || bus == null || regs == null
                || regs.Length <= 30)
                return;
            if (pc != XipCallDllUsegChk && pc != XipExeCallDllSkip
                && pc != CallDllStartip && pc != XipDllCallDllJal
                && pc != CallDllAfterJalr)
                return;
            bool hit = IsDdiNopModule(bus, regs[30]);
            if (!hit && (pc == CallDllStartip || pc == CallDllAfterJalr))
            {
                if (regs.Length > 23 && IsDdiNopModule(bus, regs[23]))
                    hit = true;
                else if (regs.Length > 4 && IsDdiNopModule(bus, regs[4]))
                    hit = true;
            }
            bool startipHit = false;
            if (!hit && (pc == CallDllStartip || pc == CallDllAfterJalr
                || pc == XipDllCallDllJal))
                startipHit = IsDdiNopStartipModule(bus, regs);
            if (hit || startipHit)
            {
                _ddiNopSawCallDllPc = true;
                if (_ddiNopCallDllSite == 0)
                    _ddiNopCallDllSite = pc;
                if (!_ddiNopCallDllHiveLogged)
                {
                    _ddiNopCallDllHiveLogged = true;
                    uint a1 = regs.Length > 5 ? regs[5] : 0;
                    uint ip = 0;
                    if (_ddiNopModule != 0)
                        TryPeekWord(bus, _ddiNopModule + ModuleStartip, out ip);
                    BootLog.Write("[Hive] ExtraROM ddi_nop CallDLL pc=0x" +
                        pc.ToString("X8") +
                        " module=0x" + regs[30].ToString("X8") +
                        " a1=0x" + a1.ToString("X8") +
                        " startip=0x" + ip.ToString("X8") +
                        (startipHit && !hit ? " (startip-site)" : ""));
                }
                TryNoteDdiNopProcessInfo(bus, regs);
            }
        }

        private static bool IsDdiNopStartipModule(MipsBus bus, uint[] regs)
        {
            if (bus == null || regs == null)
                return false;
            uint ip = 0;
            if (_ddiNopModule != 0
                && TryPeekWord(bus, _ddiNopModule + ModuleStartip, out ip)
                && IsDdiNopRamStartip(ip))
                return true;
            for (int i = 0; i < 3; i++)
            {
                uint mod = 0;
                if (i == 0 && regs.Length > 30)
                    mod = regs[30];
                else if (i == 1 && regs.Length > 23)
                    mod = regs[23];
                else if (i == 2 && regs.Length > 4)
                    mod = regs[4];
                if (mod == 0)
                    continue;
                if (!TryPeekWord(bus, mod + ModuleStartip, out ip))
                    continue;
                if (IsDdiNopRamStartip(ip))
                    return true;
            }
            return false;
        }

        private static bool IsDdiNopRamStartip(uint ip)
        {
            return ip == DdiNopVbasePage + DdiNopEntryRvaExtract
                || ip == DdiNopVbase + DdiNopEntryRvaExtract;
        }

        // Live edf15b0: DllMain / CallDLL already had
        // a1=1 and MODULE in v0, but died in coredll
        // lw $v0,0($s5) before any Hive. Log startip
        // when it actually runs. Do not invent CallDLL.
        private static void TryNoteDdiNopDllMain(MipsBus bus, uint[] regs, uint pc)
        {
            if (_ddiNopDllMainLogged || !_ddiNopAwaitCallDll)
                return;
            if (_ddiNopModule == 0 || bus == null)
                return;
            uint ip = 0;
            if (!TryPeekWord(bus, _ddiNopModule + ModuleStartip, out ip) || ip == 0)
                return;
            if (pc != ip)
                return;
            _ddiNopDllMainLogged = true;
            _ddiNopSawCallDllPc = true;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
            _ddiNopDllMainRa = ra;
            BootLog.Write("[Hive] ExtraROM ddi_nop DllMain startip=0x" +
                ip.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " module=0x" + _ddiNopModule.ToString("X8") +
                " calldll-site=" +
                (_ddiNopCallDllSite != 0
                    ? "0x" + _ddiNopCallDllSite.ToString("X8")
                    : "none"));
            TryNoteDdiNopProcessInfo(bus, regs);
            TryResolveDdiNopGwesDispFetch(bus);
            TryResolveDdiNopGwesDispData(bus);
            TryResolveDdiNopGwesTextBase(bus);
            TryResolveDdiNopGwesDispData2(bus);
            TryResolveDdiNopGwesDispData3(bus);
            TryResolveDdiNopGwesText2(bus);
        }

        // Live 6b8a9eb: after DllMain the next I-fetch
        // was 0x0005D2E0 (gwes Display page). Name that
        // PC/$ra once. Do not invent a jump.
        private static void TryNoteDdiNopAfterDllMain(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_ddiNopDllMainLogged || _ddiNopAfterDllMainLogged)
                return;
            if (pc >= DdiNopVbasePage && pc < 0x019B0000u)
                return;
            if (pc >= BindImpExnLo && pc <= BindImpExnHi)
                return;
            if (pc == 0 || pc == 0x80000000u || pc == 0x80000180u)
                return;
            _ddiNopAfterDllMainLogged = true;
            uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
            bool fetchPage = (pc & ~0xFFFu) == GwesDispFetchPage;
            BootLog.Write("[Hive] ExtraROM ddi_nop after-DllMain pc=0x" +
                pc.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " dllmain-ra=0x" + _ddiNopDllMainRa.ToString("X8") +
                (fetchPage ? " (gwes Display fetch page)" : "") +
                " calldll-site=" +
                (_ddiNopCallDllSite != 0
                    ? "0x" + _ddiNopCallDllSite.ToString("X8")
                    : "none"));
            if (fetchPage)
                TryResolveDdiNopGwesDispFetch(bus);
            TryResolveDdiNopGwesDispData(bus);
            TryResolveDdiNopGwesTextBase(bus);
            TryResolveDdiNopGwesDispData2(bus);
            TryResolveDdiNopGwesDispData3(bus);
            TryResolveDdiNopGwesText2(bus);
            if (IsDdiNopGwesImageVa(pc) && !IsNamedDdiNopGwesPage(pc))
                TryResolveDdiNopGwesImage(bus, pc);
            if (IsDdiNopCoredllImageVa(pc))
                TryResolveDdiNopCoredllImage(bus, pc);
            if (IsDdiNopVallocDataVa(pc))
                TryResolveDdiNopVallocData(bus, pc);
        }

        // Observe only. After BindImp, startip is set but
        // firmware may never reach 0x8001DD6C. Do not
        // invent a CallDLL site.
        public static void TryPollDdiNopCallDllMiss(MipsBus bus, uint pc)
        {
            TryPollDdiNopCallDllMiss(bus, null, pc);
        }

        private static bool IsBindImpIatWalkPc(uint pc)
        {
            if (pc >= BindImpHdr && pc <= BindImpIatNextAfter)
                return true;
            if (pc == BindImpOrdLookup || pc == BindImpOrdBaseLw)
                return true;
            if (pc == BindImpLoadLib || pc == BindImpLoadLibRet)
                return true;
            if (_ddiNopIatStoreLogged
                && pc >= BindImpExnLo && pc <= BindImpExnHi)
                return true;
            return false;
        }

        public static void TryPollDdiNopCallDllMiss(MipsBus bus, uint[] regs, uint pc)
        {
            TryNoteC2SpObserve(regs, pc);
            TryNoteDdiNopOrdGetProc(bus, regs, pc);
            NoteDdiNopCallDllPc(bus, regs, pc);
            TryNoteGwesB9SpinObserve(bus, regs, pc);
            if (!_ddiNopAwaitCallDll || _ddiNopCallDllMissLogged || _ddiNopSawCallDllPc)
                return;
            // Live edf15b0: CallDLL-miss still fired
            // mid-bind after slot0..23. Wait until the
            // IAT walk is done (24 stores).
            if (_ddiNopIatStoreN < BindImpObserveMax)
                return;
            // Live 1c3b70a: slot0 IAT-store won, then
            // CallDLL-miss fired while BindImp was still
            // at GetProc-ord for the next ordinal.
            if (IsBindImpIatWalkPc(pc))
                return;
            _ddiNopCallDllMissPoll++;
            if (_ddiNopCallDllMissPoll < 4096)
                return;
            TryLogDdiNopCallDllMiss(bus, regs, pc);
        }

        // Live c0347e8: after B9 dest0 map, Hive froze
        // (~84KB) while the host burned CPU. Observe
        // the stuck PC. Do not invent dest. Do not hop.
        // Live 98db5d5: same-page reset never reached
        // 256K/16K. Count total steps after B9 map.
        private static void TryNoteGwesB9SpinObserve(MipsBus bus, uint[] regs,
            uint pc)
        {
            if (_gwesB9SpinLogged || pc == 0)
                return;
            if (LookupGwesImageKseg(GwesDataB9Page) == 0)
                return;
            _gwesB9SpinPage = pc & ~0xFFFu;
            _gwesB9SpinN++;
            bool vec = (pc >= 0x80000000u && pc < 0x80000200u)
                || (pc >= BindImpExnLo && pc <= BindImpExnHi);
            int need = vec ? GwesDataB9SpinVec : GwesDataB9SpinSame;
            if (_gwesB9SpinN < need)
                return;
            _gwesB9SpinLogged = true;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint a0 = regs != null && regs.Length > 4 ? regs[4] : 0;
            uint a1 = regs != null && regs.Length > 5 ? regs[5] : 0;
            uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
            BootLog.Write("[Hive] ExtraROM ddi_nop spin-observe epc=0x" +
                pc.ToString("X8") +
                " badvaddr=0x" + _bindImpExnVaddr.ToString("X8") +
                " cause=" + _bindImpExnCode +
                " v0=0x" + v0.ToString("X8") +
                " a0=0x" + a0.ToString("X8") +
                " a1=0x" + a1.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " (after B9 dest0; do not invent dest)");
        }

        public static void TryLogDdiNopCallDllMiss(MipsBus bus)
        {
            TryLogDdiNopCallDllMiss(bus, null, 0);
        }

        public static void TryLogDdiNopCallDllMiss(MipsBus bus, uint pc)
        {
            TryLogDdiNopCallDllMiss(bus, null, pc);
        }

        public static void TryLogDdiNopCallDllMiss(MipsBus bus, uint[] regs, uint pc)
        {
            if (_ddiNopCallDllMissLogged || !_ddiNopAwaitCallDll || _ddiNopSawCallDllPc)
                return;
            if (_ddiNopIatStoreN < BindImpObserveMax)
                return;
            if (_ddiNopModule == 0)
                return;
            _ddiNopCallDllMissLogged = true;
            uint p50 = 0;
            uint ip = 0;
            TryPeekWord(bus, _ddiNopModule + ProcModule, out p50);
            TryPeekWord(bus, _ddiNopModule + ModuleStartip, out ip);
            BootLog.Write("[Hive] ExtraROM ddi_nop CallDLL-miss module=0x" +
                _ddiNopModule.ToString("X8") +
                " mod+0x50=0x" + p50.ToString("X8") +
                " startip=0x" + ip.ToString("X8") +
                " no-0x8001DD6C");
            TryLogDdiNopBindImpStall(bus, regs, pc);
        }

        // Observe only. Do not invent a CallDLL site. If
        // stall is BinaryDecompress/MapO32 for o32[1],
        // say so; do not host-CEDecompress .data.
        private static void TryLogDdiNopBindImpStall(MipsBus bus, uint[] regs, uint pc)
        {
            if (_ddiNopStallLogged)
                return;
            _ddiNopStallLogged = true;
            string why = "";
            if (pc == BinaryDecompressRom || pc == MapO32Decompress)
                why = " BinaryDecompress";
            else if (pc == MapO32Rom || pc == MapO32InnerJal
                || pc == MapO32FlagsBnez || pc == MapO32VallocJal)
                why = " MapO32";
            else if (pc == BindImpOrdBaseLw)
                why = " GetProc-ord";
            else if (pc >= BindImpExnLo && pc <= BindImpExnHi)
                why = " exception-save";
            uint dest = 0;
            if (regs != null && regs.Length > 4)
                dest = regs[4];
            uint iat = DdiNopVbasePage + DdiNopIatRva;
            uint l2 = 0;
            uint dest6 = 0;
            uint dest10 = 0;
            WalkDdiNopPteDests(bus, iat, out l2, out dest6, out dest10);
            if (dest != 0 && (dest == iat
                || (dest6 != 0 && (dest & ~0xFFFu) == (dest6 & ~0xFFFu))))
                why += " o32[1]";
            BootLog.Write("[Hive] ExtraROM BindImp-stall pc=0x" +
                pc.ToString("X8") + why);
            if (_ddiNopOrdGoodV0 == 0 || _ddiNopIatStoreLogged
                || _ddiNopOrdAfterDone)
                return;
            uint iatWord = 0;
            uint iatDest6 = 0;
            PeekDdiNopIatWord(bus, out iatWord, out iatDest6);
            if (iatWord != 0)
                return;
            _ddiNopOrdAfterDone = true;
            BootLog.Write("[Hive] ExtraROM BindImp-ord drop-v0=0x" +
                _ddiNopOrdGoodV0.ToString("X8") +
                " no-IAT last=0x" + pc.ToString("X8") +
                " (stall)");
            TryNoteBindImpIatSwSkipped();
        }

        public static bool TryForceDdiNopCallDll(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (bus == null || regs == null || regs.Length <= 30 || !_ddiNopLandedBySig)
                return false;
            // $fp is the CallDLL module. Do not steal
            // filesys/gwes by substituting a cached ddi_nop.
            uint module = regs[30];
            if (!IsDdiNopModule(bus, module))
                return false;
            try
            {
                TrySetDdiNopRamStartip(bus, module);
                uint p50 = 0;
                uint ip = 0;
                TryPeekWord(bus, module + ProcModule, out p50);
                TryPeekWord(bus, module + ModuleStartip, out ip);
                if (programCounter == XipCallDllUsegChk && !IsCallDllSkipUseg(p50))
                    return false;
                if (ip == 0)
                {
                    BootLog.Write("[Hive] ExtraROM ddi_nop CallDLL-skip module=0x" +
                        module.ToString("X8") +
                        " mod+0x50=0x" + p50.ToString("X8") +
                        " startip=0x00000000 skip-startip-0");
                    return false;
                }
                regs[4] = module;
                regs[5] = 1;
                programCounter = XipDllCallDllJal;
                _ddiNopSawCallDllPc = true;
                BootLog.Write("[Hive] force CallDLL ExtraROM ddi_nop module=0x" +
                    module.ToString("X8") +
                    " mod+0x50=0x" + p50.ToString("X8") +
                    " startip=0x" + ip.ToString("X8") +
                    " a1=1 (jal 0x80018B34; useg +0x50)");
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
            if (IsDdiNopDestLive())
                return;
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
            if (IsDdiNopDestLive())
                return;
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
            if (IsDdiNopDestLive())
            {
                if (target != 0xFFFFFFFFu)
                    return false;
                uint thr;
                uint ec;
                uint dc;
                uint plant;
                TryPeekThreadCtxPc(bus, out thr, out ec, out dc, out plant);
                if (!IsSanePlantResumePc(ec))
                    return false;
                target = ec;
                if (regs != null && regs.Length > 31)
                {
                    regs[12] = ec;
                    regs[31] = ec;
                    regs[2] = ec;
                }
                if (!_plantFixLogged)
                {
                    _plantFixLogged = true;
                    BootLog.Write("[Hive] ExtraROM ddi_nop plant-fix was=0xFFFFFFFF +EC=0x" +
                        ec.ToString("X8") +
                        " +DC=0x" + dc.ToString("X8") +
                        " plant=0x" + plant.ToString("X8") +
                        " (replay thread+0xEC; do not leftover dest)");
                }
                return true;
            }
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

        // wait104: leftover past CB34 then ERET2
        // 0x80015B9C. after-cb14 already one-shot.
        // Not leftover still mid 0x8001586C as the
        // immediate next. Not OEMIdle (later 600M
        // DONE). After leftover-CB34, I-fetch of
        // ERET2 or leftover 0x8001588C resumes at
        // CB38 after dest peek. Do not rewrite
        // 0x80015B9C. Do not rewind 0x03F6CAC0,
        // CB14, or CB34. Do not invent dest.
        public static void TryResumeTv2LeftoverAfterCb34(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb34Logged)
                return;
            if (!_tv2LeftoverPastCb34Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb38, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb34Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb34 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb34") +
                " (ERET2/leftover mid after leftover CB34; next dest-live 0x03F6CB38; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait105: leftover past CB38 then ERET2
        // 0x80015B9C. after-cb34 already one-shot.
        // Not leftover still mid 0x8001586C as the
        // immediate next. Not OEMIdle (later 600M
        // DONE). After leftover-CB38, I-fetch of
        // ERET2 or leftover 0x8001588C resumes at
        // CB3C after dest peek. Do not invent dest
        // at CB3C. Do not rewrite 0x80015B9C. Do
        // not rewind 0x03F6CAC0, CB34, or CB38.
        public static void TryResumeTv2LeftoverAfterCb38(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb38Logged)
                return;
            if (!_tv2LeftoverPastCb38Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb3c, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb38Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb38 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb38") +
                " (ERET2/leftover mid after leftover CB38; next dest-live 0x03F6CB3C; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait106: leftover past CB3C then ERET2
        // 0x80015B9C. after-cb38 already one-shot.
        // Not leftover still mid 0x8001586C as the
        // immediate next. Not OEMIdle (later 600M
        // DONE). After leftover-CB3C, I-fetch of
        // ERET2 or leftover 0x8001588C resumes at
        // CB40 after dest peek. Do not invent dest
        // at CB40. Do not rewrite 0x80015B9C. Do
        // not rewind 0x03F6CAC0, CB38, or CB3C.
        public static void TryResumeTv2LeftoverAfterCb3c(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb3cLogged)
                return;
            if (!_tv2LeftoverPastCb3cLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb40, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb3cLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb3c was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb3c") +
                " (ERET2/leftover mid after leftover CB3C; next dest-live 0x03F6CB40; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait107: leftover past CB40 then ERET2
        // 0x80015B9C. after-cb3c already one-shot.
        // Not leftover still mid 0x8001586C as the
        // immediate next. Not OEMIdle (later 600M
        // DONE). After leftover-CB40, I-fetch of
        // ERET2 or leftover 0x8001588C resumes at
        // CB44 after dest peek. Do not invent dest
        // at CB44. Do not rewrite 0x80015B9C. Do
        // not rewind 0x03F6CAC0, CB3C, or CB40.
        public static void TryResumeTv2LeftoverAfterCb40(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb40Logged)
                return;
            if (!_tv2LeftoverPastCb40Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb44, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb40Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb40 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb40") +
                " (ERET2/leftover mid after leftover CB40; next dest-live 0x03F6CB44; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait108: leftover past CB44 then ERET2
        // 0x80015B9C. after-cb40 already one-shot.
        // Not leftover still mid 0x8001586C as the
        // immediate next. Not OEMIdle (later 600M
        // DONE). After leftover-CB44, I-fetch of
        // ERET2 or leftover 0x8001588C resumes at
        // CB48 after dest peek. Do not invent dest
        // at CB48. Do not rewrite 0x80015B9C. Do
        // not rewind 0x03F6CAC0, CB40, or CB44.
        // Do not skip leftover 0x03F6CAC0 to 28($sp).
        public static void TryResumeTv2LeftoverAfterCb44(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb44Logged)
                return;
            if (!_tv2LeftoverPastCb44Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb48, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb44Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb44 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb44") +
                " (ERET2/leftover mid after leftover CB44; next dest-live 0x03F6CB48; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait109: leftover past CB48 (jr $ra) then
        // leftover left. after-cb44 already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // resumes at dest-live delay slot CB4C after
        // dest peek. Do not invent dest at CB4C.
        // Do not rewrite 0x80015B9C. Do not rewind
        // leftover. Do not skip leftover 0x03F6CAC0
        // to 28($sp).
        public static void TryResumeTv2LeftoverAfterCb48(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb48Logged)
                return;
            if (!_tv2LeftoverPastCb48Logged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverCb4c, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb48Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb48 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-cb48") +
                " (ERET2/leftover mid after leftover CB48; dest-live delay slot 0x03F6CB4C; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        // wait110: leftover past CB4C then leftover
        // left. after-cb48 already one-shot. I-fetch
        // of ERET2 or leftover 0x8001588C follows
        // dest-live user $ra from peeked 28($sp)
        // (CB44 lw $ra,28($sp)). Do not invent dest
        // at 0x03F731E4. Do not follow leftover-
        // dispatch $ra. Do not rewrite 0x80015B9C.
        // Do not rewind leftover to 0x03F6C8F4.
        public static void TryResumeTv2LeftoverAfterCb4c(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterCb4cLogged)
                return;
            if (!_tv2LeftoverPastCb4cLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            TryCaptureLeftoverUserRa(bus, regs);
            if (!_tv2LeftoverUserRaSet)
                return;
            uint ra = _tv2LeftoverUserRa;
            if (ra == 0 || ra == 0xFFFFFFFFu || ra == 0xFFFFFFECu)
                return;
            if ((ra & 0x1FFFFFFFu) < 0x00010000u)
                return;
            if (ra >= 0x03F6C8F4u && ra <= LeftoverCb4c)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, ra, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterCb4cLogged = true;
            _tv2LeftoverJrRaDest = dest;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-cb4c was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                (live ? " dest-live" : " dest-ra") +
                " (ERET2/leftover mid after leftover CB4C; follow dest-live user $ra from 28($sp); do not invent 0x03F731E4; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait111: leftover past leftover-jr-ra dest
        // 0x03F731E4 then leftover left. after-cb4c
        // already one-shot. I-fetch of ERET2 or
        // leftover 0x8001588C follows dest-live
        // next insn after beq $v0,$0,+10. Peek
        // fallthrough 0x03F731E8 and taken
        // 0x03F73210 first. Follow leftover $v0
        // captured at leftover-past 0x03F731E4.
        // Do not invent dest. Do not rewrite
        // 0x80015B9C. Do not rewind leftover.
        // Do not invent dest at 0x03F731E4.
        public static void TryResumeTv2LeftoverAfterJrRa(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterJrRaLogged)
                return;
            if (!_tv2LeftoverPastJrRaLogged)
                return;
            if (_tv2LeftoverPastBeqRaFtLogged || _tv2LeftoverPastBeqRaTkLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            if (!_tv2LeftoverBeqRaV0Set)
                return;
            uint v0 = _tv2LeftoverBeqRaV0;
            if (v0 >= 0x80010000u && v0 < 0x80020000u)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            uint prefer = v0 == 0 ? LeftoverBeqRaTk : LeftoverBeqRaFt;
            if (!TryAcceptLeftoverAfterDest(bus, prefer, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterJrRaLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-jr-ra was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-jr-ra") +
                " beq-ra-v0=0x" + v0.ToString("X8") +
                " (ERET2/leftover mid after leftover jr $ra dest; follow dest-live $v0 after beq $v0,$0,+10; peek 0x03F731E8/0x03F73210; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait112: leftover past 0x03F73210 (b +2)
        // then leftover left. after-jr-ra already
        // one-shot. I-fetch of ERET2 or leftover
        // 0x8001588C follows dest-live next insn
        // after that branch. Peek delay 0x03F73214
        // and taken 0x03F7321C first. b +2 is
        // unconditional. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover. Do not invent dest at
        // 0x03F731E4.
        public static void TryResumeTv2LeftoverAfterBPlus2(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterBPlus2Logged)
                return;
            if (!_tv2LeftoverPastBeqRaTkLogged)
                return;
            if (_tv2LeftoverPastBPlus2DelayLogged || _tv2LeftoverPastBPlus2TakenLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverBPlus2Delay, out dest, out word, out live)
                && !TryAcceptLeftoverAfterDest(bus, LeftoverBPlus2Taken, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterBPlus2Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-b+2 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-b+2") +
                " (ERET2/leftover mid after leftover b +2; dest-live next insn after 0x03F73210; peek 0x03F73214/0x03F7321C; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait113: leftover past 0x03F7321C then
        // leftover left. after-b+2 already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // follows dest-live next insn 0x03F73220
        // after dest peek. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover. Do not invent dest at
        // 0x03F731E4.
        public static void TryResumeTv2LeftoverAfterBPlus2Taken(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterBPlus2TakenLogged)
                return;
            if (!_tv2LeftoverPastBPlus2TakenLogged)
                return;
            if (_tv2LeftoverPastBPlus2NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverBPlus2Next, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterBPlus2TakenLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-taken was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-taken") +
                " (ERET2/leftover mid after leftover 0x03F7321C; dest-live next 0x03F73220; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait114: leftover past 0x03F73220 then
        // leftover left. after-taken already
        // one-shot. I-fetch of ERET2 or leftover
        // 0x8001588C follows dest-live next insn
        // 0x03F73224 after dest peek. Do not
        // invent dest. Do not rewrite 0x80015B9C.
        // Do not rewind leftover. Do not invent
        // dest at 0x03F731E4.
        public static void TryResumeTv2LeftoverAfterFp(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterFpLogged)
                return;
            if (!_tv2LeftoverPastBPlus2NextLogged)
                return;
            if (_tv2LeftoverPastFpNextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverFpNext, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterFpLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-fp was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-fp") +
                " (ERET2/leftover mid after leftover 0x03F73220; dest-live next 0x03F73224; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait115: leftover past 0x03F73224 then
        // leftover left. after-fp already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // follows dest-live next insn 0x03F73228
        // after dest peek. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover. Do not invent dest at
        // 0x03F731E4.
        public static void TryResumeTv2LeftoverAfterS7(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterS7Logged)
                return;
            if (!_tv2LeftoverPastFpNextLogged)
                return;
            if (_tv2LeftoverPastS7NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverS7Next, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterS7Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-s7 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-s7") +
                " (ERET2/leftover mid after leftover 0x03F73224; dest-live next 0x03F73228; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait117: leftover past 0x03F73228 then
        // leftover left. after-s7 already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // follows dest-live next insn 0x03F7322C
        // after dest peek. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover.
        public static void TryResumeTv2LeftoverAfterS6(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterS6Logged)
                return;
            if (!_tv2LeftoverPastS7NextLogged)
                return;
            if (_tv2LeftoverPastS6NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverS6Next, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterS6Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-s6 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-s6") +
                " (ERET2/leftover mid after leftover 0x03F73228; dest-live next 0x03F7322C; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait118: leftover past 0x03F7322C then
        // leftover left. after-s6 already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // follows dest-live next insn 0x03F73230
        // after dest peek. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover.
        public static void TryResumeTv2LeftoverAfterS5(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterS5Logged)
                return;
            if (!_tv2LeftoverPastS6NextLogged)
                return;
            if (_tv2LeftoverPastS5NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverS5Next, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterS5Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-s5 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-s5") +
                " (ERET2/leftover mid after leftover 0x03F7322C; dest-live next 0x03F73230; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait119: leftover past 0x03F73230 then
        // leftover left. after-s5 already one-shot.
        // I-fetch of ERET2 or leftover 0x8001588C
        // follows dest-live next insn 0x03F73234
        // after dest peek. Do not invent dest. Do
        // not rewrite 0x80015B9C. Do not rewind
        // leftover.
        public static void TryResumeTv2LeftoverAfterS4(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (_tv2LeftoverAfterS4Logged)
                return;
            if (!_tv2LeftoverPastS5NextLogged)
                return;
            if (_tv2LeftoverPastS4NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint dest = 0;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, LeftoverS4Next, out dest, out word, out live))
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverAfterS4Logged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover after-s4 was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-s4") +
                " (ERET2/leftover mid after leftover 0x03F73230; dest-live next 0x03F73234; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        // wait120: leftover dest-live continue. leftover
        // already past 0x03F73234. leftover-after-s4
        // stays off. leftover mid / ERET2 I-fetch
        // resumes dest-live next after dest peek.
        // leftover dest-live next starts at peeked
        // 0x03F73238 (jr $ra / delay). Write leftover
        // ctxPC to dest-live next so leftover does not
        // leave after every lw. Do not rewrite
        // 0x80015B9C. Do not invent dest.
        // wait121: dest-live continue stays live after
        // leftover-past dest-live delay. after-* only
        // rewrites one I-fetch. leftover-left /
        // leftover ctxPC / EPC yank leftover to ERET2
        // unless dest-live next keeps PC+4 after dest
        // peek. leftover DISPATCH after leftover dest-live
        // I-fetch must not yank leftover ctxPC to ERET2.
        // leftover restore re-apply is too early (restore
        // I-fetch, not dispatch). dest-live I-fetch stays
        // dest-live next, not ERET2. Do not follow
        // dest-live $ra before dest-live delay (already
        // walked; rewind). wait124: dest-live $ra is
        // still already walked after dest-live delay.
        // After dest-live delay, dest-live next is
        // leftover dest-live delay's live leftover next
        // (leftover $ra at dest-live jr $ra if live
        // leftover dest), not dest-live $ra, not PC+4.
        // prior peek named 0x03F731E4 as evidence only;
        // do not invent dest. Do not follow dest-live $ra
        // blindly. Do not add a one-shot hop at
        // 0x03F73238.
        public static void TryResumeTv2LeftoverDestLiveContinue(MipsBus bus, uint[] regs, ref uint pc)
        {
            if (IsDdiNopDestLive())
                return;
            if (!_tv2LeftoverPastS4NextLogged)
                return;
            // wait127: leftover dest-live I-fetch of dest-live
            // next / PC+4 after dest-live delay+4 walk takes
            // leftover interrupt. leftover exception handler
            // I-fetches leftover mid / ERET2. leftover dest-live
            // continue hops leftover exception / leftover mid /
            // ERET2 to dest-live next / PC+4 after dest-live
            // delay+4 walk. leftover after dest-live delay+4
            // walk stays dest-live next / PC+4, not leftover
            // mid / ERET2.
            // wait128: leftover dest-live keep hops leftover
            // I-fetch / leftover FetchInstruction leftover mid /
            // ERET2 0x80015B9C after dest-live delay+4 to
            // dest-live next / leftover dest-live next /
            // leftover ctxPC / leftover PC+4 before leftover
            // FetchInstruction. leftover dest-live continue
            // after leftover interrupt is too late. leftover
            // after dest-live delay+4, including leftover
            // interrupt, stays dest-live next / PC+4, not
            // leftover mid / ERET2.
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch
                && !(_tv2LeftoverPastEpilogueDelayLogged && pc == 0x80000180u))
                return;
            uint dest = _tv2LeftoverDestLiveNext;
            if (dest == 0)
                dest = LeftoverEpilogueNext;
            if (_tv2LeftoverPastEpilogueLogged && dest == LeftoverEpilogueNext)
                dest = LeftoverEpilogueNext + 4;
            if (_tv2LeftoverPastEpilogueDelayLogged)
            {
                // dest-live $ra is already walked (leftover
                // past dest-live $ra). dest-live next after
                // dest-live delay is leftover dest-live
                // delay's live leftover next, not dest-live
                // $ra, not PC+4. wait126: after leftover
                // dest-live continue hops leftover mid to
                // dest-live delay+4 / dest-live next /
                // PC+4, leftover dest-live next stays
                // dest-live next / PC+4. leftover dest-live
                // continue hops leftover mid / ERET2 to
                // dest-live next / PC+4, not dest-live
                // delay+4.
                if (dest == LeftoverEpilogueNext + 8
                    || dest == LeftoverEpilogueNext + 12
                    || dest > LeftoverEpilogueNext + 8)
                {
                    // dest-live delay+4 already walked.
                }
                else if (_tv2LeftoverPastJrRaLogged
                    && dest == _tv2LeftoverUserRa)
                    dest = LeftoverEpilogueNext + 8;
                else if (dest == LeftoverEpilogueNext + 4 || dest == 0)
                    dest = LeftoverEpilogueNext + 8;
            }
            if (dest == LeftoverS4Next)
                return;
            if (dest == 0x03F731E4u)
                return;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, dest, out dest, out word, out live))
                return;
            if (dest == pc)
                return;
            uint was = pc;
            pc = dest;
            _tv2LeftoverDestLiveNext = dest + 4;
            TryKeepLeftoverDestLiveCtx(bus, dest);
            System.Console.WriteLine("[Hive] FILE[25] leftover dest-live continue was=0x" +
                was.ToString("X8") +
                " now=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-epilogue") +
                " (leftover mid/ERET2 after leftover 0x03F73234; dest-live next peeked; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        private static void TryKeepLeftoverDestLiveCtx(MipsBus bus, uint dest)
        {
            if (bus == null || _tv2Thread == 0 || dest == 0)
                return;
            if ((dest & 0x1FFFFFFFu) < 0x00010000u)
                return;
            if (dest == LeftoverS4Next)
                return;
            if (dest == 0x03F731E4u)
                return;
            if (dest == ExnAfterFetch || dest == ExnAfterFetch2)
                return;
            if (!IsTv2CoredllShared(dest))
                return;
            try
            {
                uint ctx = bus.Read32(_tv2Thread + ThreadCtxPc);
                if (ctx == dest)
                    return;
                bool yanked = ctx == ExnAfterFetch || ctx == ExnAfterFetch2;
                bool destLive = IsTv2CoredllShared(ctx);
                if (!yanked && !destLive)
                    return;
                bus.Write32(_tv2Thread + ThreadCtxPc, dest);
            }
            catch
            {
            }
        }

        private static bool TryResolveLeftoverDestLiveNext(out uint dest)
        {
            dest = _tv2LeftoverDestLiveNext;
            if (dest == 0)
                dest = LeftoverEpilogueNext;
            if (_tv2LeftoverPastEpilogueLogged && dest == LeftoverEpilogueNext)
                dest = LeftoverEpilogueNext + 4;
            if (_tv2LeftoverPastEpilogueDelayLogged)
            {
                // wait126: after leftover dest-live continue
                // hops leftover mid to dest-live delay+4 /
                // dest-live next / PC+4, leftover dest-live
                // next stays dest-live next / PC+4. leftover
                // DISPATCH dest is dest-live next / PC+4,
                // not dest-live delay+4.
                if (dest == LeftoverEpilogueNext + 8
                    || dest == LeftoverEpilogueNext + 12
                    || dest > LeftoverEpilogueNext + 8)
                {
                    // dest-live delay+4 already walked.
                }
                else if (_tv2LeftoverPastJrRaLogged
                    && dest == _tv2LeftoverUserRa)
                    dest = LeftoverEpilogueNext + 8;
                else if (dest == LeftoverEpilogueNext + 4 || dest == 0)
                    dest = LeftoverEpilogueNext + 8;
            }
            if (dest == LeftoverS4Next)
                return false;
            if (dest == 0x03F731E4u)
                return false;
            return dest != 0 && (dest & 0x1FFFFFFFu) >= 0x00010000u;
        }

        // leftover DISPATCH after leftover dest-live I-fetch
        // must not yank leftover ctxPC to ERET2. dest-live
        // I-fetch stays dest-live next / PC+4, not ERET2.
        // leftover restore re-apply is too early (restore
        // I-fetch, not dispatch). wait126: leftover dest-live
        // continue hops leftover mid to dest-live delay+4 /
        // PC+4. leftover DISPATCH after leftover dest-live
        // I-fetch of dest-live delay+4 / PC+4 yanks leftover
        // ctxPC. leftover later I-fetches ERET2. leftover
        // DISPATCH after leftover dest-live I-fetch of
        // dest-live delay+4 / PC+4 writes leftover ctxPC
        // to dest-live next / PC+4, not ERET2. leftover
        // dest-live continue after leftover dest-live
        // I-fetch hops leftover mid / ERET2 to dest-live
        // next / PC+4. leftover dest-live next after
        // dest-live delay+4 walk stays dest-live next /
        // PC+4, not ERET2. Do not follow dest-live $ra.
        // Do not rewrite 0x80015B9C. Do not add a one-shot
        // hop at 0x03F73238.
        public static void TryKeepLeftoverDestLiveDispatch(MipsBus bus, uint pc)
        {
            if (IsDdiNopDestLive())
                return;
            if (!_tv2LeftoverPastS4NextLogged)
                return;
            uint dest;
            if (!TryResolveLeftoverDestLiveNext(out dest))
                return;
            if (pc != ExnAfterFetch && pc != ExnAfterFetch2
                && IsTv2CoredllShared(pc)
                && pc != LeftoverS4Next
                && pc != 0x03F731E4u)
            {
                // wait126: leftover dest-live continue hops
                // leftover mid to dest-live delay+4 /
                // dest-live next / PC+4. leftover DISPATCH
                // after leftover dest-live I-fetch of
                // dest-live delay+4 / dest-live next /
                // PC+4 must not yank leftover dest-live
                // next / leftover ctxPC back to dest-live
                // delay+4 / leftover dest-live delay's
                // live leftover next. leftover dest-live
                // next stays dest-live next / PC+4.
                if (_tv2LeftoverPastEpilogueDelayLogged
                    && dest != 0 && dest != pc && dest != pc + 4
                    && dest < pc
                    && dest != LeftoverS4Next
                    && dest != 0x03F731E4u)
                    dest = pc;
                if (dest == pc)
                    dest = pc + 4;
                if (dest != LeftoverS4Next
                    && dest != 0x03F731E4u
                    && dest != ExnAfterFetch && dest != ExnAfterFetch2
                    && (dest & 0x1FFFFFFFu) >= 0x00010000u)
                    _tv2LeftoverDestLiveNext = dest;
            }
            if (dest == LeftoverS4Next)
                return;
            if (dest == 0x03F731E4u)
                return;
            if (dest == ExnAfterFetch || dest == ExnAfterFetch2)
                return;
            TryKeepLeftoverDestLiveCtx(bus, dest);
            if (_tv2LeftoverDispatchLogged)
                return;
            _tv2LeftoverDispatchLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover dest-live dispatch keep dest=0x" +
                dest.ToString("X8") +
                " after=0x" + pc.ToString("X8") +
                " (leftover DISPATCH after leftover dest-live I-fetch; dest-live I-fetch stays dest-live next / PC+4, not ERET2; leftover restore re-apply is too early; do not invent dest; do not rewrite 0x80015B9C; do not rewind leftover $ra 0x03F731E4; not TV UI)");
        }

        // leftover-drop: leftover dest-live resume hijacks
        // leftover mid / ERET2 I-fetch. leftover ctxPC stays
        // leftover mid / ERET2. leftover ERET 0x80015A24
        // uses $v0 not leftover ctxPC. leftover $v0 restore
        // is one-shot leftover-CAE8 dest. After leftover
        // dest-live lw leftover $v0 stays leftover mid.
        // leftover ERET returns leftover mid / ERET2.
        // leftover dest-live continue leftover ERET $v0
        // restore dest-live next. Stay-off after leftover
        // dest-live delay. Do not rewind leftover $ra
        // 0x03F731E4. Do not rewrite 0x80015B9C.
        public static void TryNoteTv2LeftoverDrop(MipsBus bus, uint[] regs, uint pc)
        {
            if (_tv2LeftoverDropLogged)
                return;
            if (!_tv2LeftoverPastS4NextLogged)
                return;
            if (pc != ExnAfterFetch2 && pc != ExnAfterFetch)
                return;
            uint v0 = regs != null && regs.Length > 2 ? regs[2] : 0;
            uint t4 = regs != null && regs.Length > 12 ? regs[12] : 0;
            uint ra = regs != null && regs.Length > 31 ? regs[31] : 0;
            uint ctx = 0;
            uint cur = 0;
            uint curThr = 0;
            try
            {
                if (bus != null && _tv2Thread != 0)
                    ctx = bus.Read32(_tv2Thread + ThreadCtxPc);
                if (bus != null)
                    cur = bus.Read32(CurProc);
                if (bus != null)
                    curThr = bus.Read32(ThreadPtr);
            }
            catch
            {
            }
            _tv2LeftoverDropLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover-drop pc=0x" +
                pc.ToString("X8") +
                " v0=0x" + v0.ToString("X8") +
                " t4=0x" + t4.ToString("X8") +
                " ra=0x" + ra.ToString("X8") +
                " ctxPC=0x" + ctx.ToString("X8") +
                " dest-live-next=0x" + _tv2LeftoverDestLiveNext.ToString("X8") +
                " CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " (leftover mid/ERET2 after leftover dest-live; leftover ERET 0x80015A24 uses $v0 not leftover ctxPC; leftover $v0 restore is one-shot leftover-CAE8 dest; leftover dest-live continue leftover ERET $v0 restore dest-live next; do not invent dest; do not rewrite 0x80015B9C; do not rewind leftover $ra 0x03F731E4; not TV UI)");
        }

        // leftover dest-live continue leftover ERET $v0
        // restore dest-live next. leftover ERET 0x80015A24
        // mtc0 $t4,EPC ($t4=$ra=$v0). leftover dest-live
        // resume hijacks leftover mid / ERET2 I-fetch.
        // leftover dest-live continue leftover ERET $v0
        // restore dest-live next so leftover ERET returns
        // dest-live next. wait124: dest-live $ra is already
        // walked after dest-live delay. After dest-live
        // delay, dest-live next is leftover dest-live
        // delay's live leftover next, not dest-live $ra,
        // not PC+4. leftover dest-live ERET $v0 restore
        // after dest-live delay writes leftover $v0 to
        // leftover dest-live delay's live leftover next.
        // leftover mid / ERET2 after dest-live delay is
        // leftover ERET $v0 restore, not leftover ERET
        // path. prior peek named 0x03F731E4 as evidence
        // only; do not invent dest. Do not follow dest-live
        // $ra blindly. Do not hop 0x03F73238. Do not
        // rewrite 0x80015B9C.
        public static void TryRestoreTv2LeftoverDestLiveEret(MipsBus bus, uint[] regs, uint pc)
        {
            if (IsDdiNopDestLive())
                return;
            if (!_tv2LeftoverPastS4NextLogged)
                return;
            bool leftoverEret = pc == LeftoverOrRa || pc == LeftoverMtc0Epc
                || pc == LeftoverJrRa || pc == LeftoverEret;
            bool leftoverMidAfterDelay = _tv2LeftoverPastEpilogueDelayLogged
                && (pc == ExnAfterFetch2 || pc == ExnAfterFetch
                    || pc == LeftoverEpilogueNext + 4);
            if (!leftoverEret && !leftoverMidAfterDelay)
                return;
            if (regs == null || regs.Length <= 31)
                return;
            uint dest;
            if (!TryResolveLeftoverDestLiveNext(out dest))
                return;
            uint word = 0;
            bool live = false;
            if (!TryAcceptLeftoverAfterDest(bus, dest, out dest, out word, out live))
                return;
            if (dest == LeftoverS4Next)
                return;
            if (dest == 0x03F731E4u)
                return;
            uint was = leftoverMidAfterDelay
                ? regs[2]
                : (pc == LeftoverOrRa ? regs[2] : (pc == LeftoverMtc0Epc ? regs[12] : regs[31]));
            if (was == dest)
                return;
            if (leftoverMidAfterDelay)
            {
                regs[2] = dest;
                regs[12] = dest;
                regs[31] = dest;
            }
            else if (pc == LeftoverOrRa)
                regs[2] = dest;
            else
            {
                regs[12] = dest;
                regs[31] = dest;
                if (pc == LeftoverEret)
                    regs[2] = dest;
            }
            TryKeepLeftoverDestLiveCtx(bus, dest);
            if (_tv2LeftoverDestLiveEretLogged)
                return;
            _tv2LeftoverDestLiveEretLogged = true;
            System.Console.WriteLine("[Hive] FILE[25] leftover dest-live eret-restore was=0x" +
                was.ToString("X8") +
                " at=0x" + pc.ToString("X8") +
                " dest=0x" + dest.ToString("X8") +
                " dest-word=0x" + word.ToString("X8") +
                (live ? " dest-live" : " dest-epilogue") +
                " (leftover ERET 0x80015A24 uses $v0 not leftover ctxPC; leftover dest-live continue leftover ERET $v0 restore dest-live next; do not invent dest; do not rewrite 0x80015B9C; do not rewind leftover $ra 0x03F731E4; not TV UI)");
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
                else if (va == LeftoverCb38 && _tv2LeftoverCb38Peeked)
                    word = _tv2LeftoverCb38Word;
                else if (va == LeftoverCb3c && _tv2LeftoverCb3cPeeked)
                    word = _tv2LeftoverCb3cWord;
                else if (va == LeftoverCb40 && _tv2LeftoverCb40Peeked)
                    word = _tv2LeftoverCb40Word;
                else if (va == LeftoverCb44 && _tv2LeftoverCb44Peeked)
                    word = _tv2LeftoverCb44Word;
                else if (va == LeftoverCb48 && _tv2LeftoverCb48Peeked)
                    word = _tv2LeftoverCb48Word;
                else if (va == LeftoverCb4c && _tv2LeftoverCb4cPeeked)
                    word = _tv2LeftoverCb4cWord;
                else if (va == LeftoverBeqRaFt && _tv2LeftoverBeqRaFtPeeked)
                    word = _tv2LeftoverBeqRaFtWord;
                else if (va == LeftoverBeqRaTk && _tv2LeftoverBeqRaTkPeeked)
                    word = _tv2LeftoverBeqRaTkWord;
                else if (va == LeftoverBPlus2Delay && _tv2LeftoverBPlus2DelayPeeked)
                    word = _tv2LeftoverBPlus2DelayWord;
                else if (va == LeftoverBPlus2Taken && _tv2LeftoverBPlus2TakenPeeked)
                    word = _tv2LeftoverBPlus2TakenWord;
                else if (va == LeftoverBPlus2Next && _tv2LeftoverBPlus2NextPeeked)
                    word = _tv2LeftoverBPlus2NextWord;
                else if (va == LeftoverFpNext && _tv2LeftoverFpNextPeeked)
                    word = _tv2LeftoverFpNextWord;
                else if (va == LeftoverS7Next && _tv2LeftoverS7NextPeeked)
                    word = _tv2LeftoverS7NextWord;
                else if (va == LeftoverS6Next && _tv2LeftoverS6NextPeeked)
                    word = _tv2LeftoverS6NextWord;
                else if (va == LeftoverS5Next && _tv2LeftoverS5NextPeeked)
                    word = _tv2LeftoverS5NextWord;
                else if (va == LeftoverS4Next && _tv2LeftoverS4NextPeeked)
                    word = _tv2LeftoverS4NextWord;
                else if (va == LeftoverEpilogueNext && _tv2LeftoverEpiloguePeeked)
                    word = _tv2LeftoverEpilogueWord;
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
            if (_tv2LeftoverPastS4NextLogged && _tv2LeftoverDestLiveNext != 0
                && (ctxPc == ExnAfterFetch || ctxPc == ExnAfterFetch2
                    || IsExnDispatchLeftover(ctxPc)
                    || IsTv2CoredllShared(ctxPc)))
            {
                // leftover DISPATCH after leftover dest-live
                // I-fetch must not yank leftover ctxPC to
                // ERET2. dest-live I-fetch stays dest-live
                // next / PC+4. leftover restore re-apply is
                // too early (restore I-fetch, not dispatch).
                TryKeepLeftoverDestLiveDispatch(bus, ctxPc);
                if (ctxPc == ExnAfterFetch || ctxPc == ExnAfterFetch2
                    || IsExnDispatchLeftover(ctxPc))
                    return;
            }
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
            if (IsLeftoverUserRa(stacked))
            {
                _tv2LeftoverUserRa = stacked;
                _tv2LeftoverUserRaSet = true;
            }
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
            // wait121: leftover-left / leftover restore
            // overwrites leftover ctxPC to ERET2 after
            // leftover-past dest-live. leftover restore
            // re-apply is too early (restore I-fetch, not
            // dispatch). leftover DISPATCH after leftover
            // dest-live I-fetch must not yank leftover
            // ctxPC to ERET2. dest-live I-fetch stays
            // dest-live next / PC+4, not ERET2. Do not
            // follow dest-live $ra 0x03F731E4. Do not
            // rewrite ERET2. Do not add a one-shot hop
            // at 0x03F73238.
            if (_tv2LeftoverPastS4NextLogged && _tv2LeftoverDestLiveNext != 0)
                TryKeepLeftoverDestLiveDispatch(bus, pc);
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
            if (_ddiNopDestPeekRaw || _ddiNopInfoPeekRaw)
                return va;
            bool dest0 = _ddiNopDestOn
                && va >= DdiNopVallocLo && va < DdiNopVallocDataHi();
            bool ddiInfo = va >= ProcessInfoPage && va < 0x02000000u
                && IsDdiNopProcessInfoArmed();
            bool ddiFetch = (va & ~0xFFFu) == GwesDispFetchPage
                && IsDdiNopGwesDispFetchArmed();
            bool ddiData = (va & ~0xFFFu) == GwesDispDataPage
                && IsDdiNopGwesDispDataArmed();
            bool ddiText = (va & ~0xFFFu) == GwesTextBasePage
                && IsDdiNopGwesTextBaseArmed();
            bool ddiData2 = (va & ~0xFFFu) == GwesDispData2Page
                && IsDdiNopGwesDispData2Armed();
            bool ddiData3 = (va & ~0xFFFu) == GwesDispData3Page
                && IsDdiNopGwesDispData3Armed();
            bool ddiText2 = (va & ~0xFFFu) == GwesText2Page
                && IsDdiNopGwesText2Armed();
            bool ddiGwes = IsDdiNopGwesImageArmed()
                && IsDdiNopGwesImageVa(va)
                && !IsNamedDdiNopGwesPage(va);
            if (_pteMapBusy || bus == null || (_tv2ImplRa == 0 && !dest0 && !ddiInfo && !ddiFetch && !ddiData && !ddiText && !ddiData2 && !ddiData3 && !ddiText2 && !ddiGwes))
                return va;
            if (va >= 0x80000000u)
                return va;
            if (IsTv2CoredllShared(va))
                return va;
            uint slot = va >> 25;
            bool walkSlot2 = slot == 2 && _tv2LeftoverLiveLogged;
            bool walkSlot0Info = slot == 0
                && (_tv2LeftoverPastLogged || ddiInfo)
                && va >= ProcessInfoPage
                && va < 0x02000000u;
            bool walkSlot0Fetch = slot == 0
                && va >= 0x00010000u
                && va < 0x01FFF000u
                && (_tv2LeftoverCae8Logged || ddiFetch || ddiData || ddiText || ddiData2 || ddiData3 || ddiText2 || ddiGwes);
            if (slot != 1 && slot != 6 && !walkSlot2 && !walkSlot0Info
                && !walkSlot0Fetch && !dest0)
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
                if (dest0)
                {
                    if ((va & 0xFFFFF000u) == 0x01981000u)
                        _ddiNopDest0Pte = dest & ~0xFFFu;
                    if (!_ddiNopDest0PteLogged)
                    {
                        _ddiNopDest0PteLogged = true;
                        BootLog.Write("[Hive] ExtraROM ddi_nop dest0 PTE va=0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " (firmware 0x80040278; useg dest)");
                    }
                }
                else if (walkSlot0Info && !_slot0InfoMapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _slot0InfoMapLogged = true;
                    _pteMapLogged = true;
                    if (ddiInfo && !_tv2LeftoverPastLogged)
                    {
                        if (_ddiNopInfoKseg == 0)
                            _ddiNopInfoKseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop proc-info PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; same page as *0x01FFFFA0; do not invent heap bytes)");
                    }
                    else
                    {
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
                }
                else if (walkSlot0Fetch && !_slot0FetchMapLogged)
                {
                    uint word = 0;
                    TryPeekWord(bus, dest, out word);
                    _slot0FetchMapLogged = true;
                    _pteMapLogged = true;
                    if (ddiGwes && !_tv2LeftoverCae8Logged)
                    {
                        uint rom = 0;
                        uint romWord = 0;
                        string via = null;
                        if (IsGwesImageBasePage(va))
                        {
                            if (TryGwesHeaderDest(bus, va, out rom, out romWord, out via))
                            {
                                RememberGwesImageKseg(va, rom);
                                BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page PTE 0x" +
                                    va.ToString("X8") + " -> 0x" + rom.ToString("X8") +
                                    " dest-word=0x" + romWord.ToString("X8") +
                                    " via=" + via +
                                    " was=0x" + dest.ToString("X8") +
                                    " (ImageBase headers; TOC[7] gwes; do not invent dest)");
                            }
                        }
                        else if (IsGwesDataB9Page(va))
                        {
                            RememberGwesImageKseg(va, dest);
                            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page PTE 0x" +
                                va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                                " dest-word=0x" + word.ToString("X8") +
                                " (firmware 0x80040278; compressed .data dest0; do not invent dest)");
                        }
                        else if (TryGwesRomTextDest(bus, va, word, out rom, out romWord))
                        {
                            RememberGwesImageKseg(va, rom);
                            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page PTE 0x" +
                                va.ToString("X8") + " -> 0x" + rom.ToString("X8") +
                                " dest-word=0x" + romWord.ToString("X8") +
                                " via=o32-rom was=0x" + dest.ToString("X8") +
                                " (dest-word=0 .text; TOC[7] o32; do not invent dest)");
                        }
                        else
                        {
                            RememberGwesImageKseg(va, dest);
                            BootLog.Write("[Hive] ExtraROM ddi_nop gwes-page PTE 0x" +
                                va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                                " dest-word=0x" + word.ToString("X8") +
                                " (firmware 0x80040278; gwes image; do not invent dest)");
                        }
                    }
                    else if (ddiText2 && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesText2Kseg == 0)
                            _ddiNopGwesText2Kseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text2 PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; gwes .text 0x00014000; do not invent dest)");
                    }
                    else if (ddiData3 && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesData3Kseg == 0)
                            _ddiNopGwesData3Kseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data3 PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; GwesDispObj; do not invent dest)");
                    }
                    else if (ddiData2 && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesData2Kseg == 0)
                            _ddiNopGwesData2Kseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data2 PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; GwesInitFlag page; do not invent dest)");
                    }
                    else if (ddiText && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesTextKseg == 0)
                            _ddiNopGwesTextKseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-text PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; gwes VA 0x00011000; do not invent dest)");
                    }
                    else if (ddiData && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesDataKseg == 0)
                            _ddiNopGwesDataKseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp data PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; GwesIatGetProc page; do not invent dest)");
                    }
                    else if (ddiFetch && !_tv2LeftoverCae8Logged)
                    {
                        if (_ddiNopGwesFetchKseg == 0)
                            _ddiNopGwesFetchKseg = dest & ~0xFFFu;
                        BootLog.Write("[Hive] ExtraROM ddi_nop gwes-disp PTE 0x" +
                            va.ToString("X8") + " -> 0x" + dest.ToString("X8") +
                            " dest-word=0x" + word.ToString("X8") +
                            " (firmware 0x80040278; GwesVaDispAlloc page; do not invent dest)");
                    }
                    else
                    {
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
            uint cb38 = 0;
            if (TryPeekWord(bus, LeftoverCb38, out cb38)
                && (LeftoverCb38 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb38Peeked = true;
                _tv2LeftoverCb38Word = cb38;
            }
            uint cb3c = 0;
            if (TryPeekWord(bus, LeftoverCb3c, out cb3c)
                && (LeftoverCb3c & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb3cPeeked = true;
                _tv2LeftoverCb3cWord = cb3c;
            }
            uint cb40 = 0;
            if (TryPeekWord(bus, LeftoverCb40, out cb40)
                && (LeftoverCb40 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb40Peeked = true;
                _tv2LeftoverCb40Word = cb40;
            }
            uint cb44 = 0;
            if (TryPeekWord(bus, LeftoverCb44, out cb44)
                && (LeftoverCb44 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb44Peeked = true;
                _tv2LeftoverCb44Word = cb44;
            }
            uint cb48 = 0;
            if (TryPeekWord(bus, LeftoverCb48, out cb48)
                && (LeftoverCb48 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb48Peeked = true;
                _tv2LeftoverCb48Word = cb48;
            }
            uint cb4c = 0;
            if (TryPeekWord(bus, LeftoverCb4c, out cb4c)
                && (LeftoverCb4c & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb4cPeeked = true;
                _tv2LeftoverCb4cWord = cb4c;
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
                " cb38-word=0x" + cb38.ToString("X8") +
                " cb3c-word=0x" + cb3c.ToString("X8") +
                " cb40-word=0x" + cb40.ToString("X8") +
                " cb44-word=0x" + cb44.ToString("X8") +
                " cb48-word=0x" + cb48.ToString("X8") +
                " cb4c-word=0x" + cb4c.ToString("X8") +
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
            uint cb38 = 0;
            if (TryPeekWord(bus, LeftoverCb38, out cb38)
                && (LeftoverCb38 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb38Peeked = true;
                _tv2LeftoverCb38Word = cb38;
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
                " from=0x03F6CB14 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb38-word=0x" + cb38.ToString("X8") +
                " (past leftover beq $a0,$0,+7; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb38(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb34Logged || _tv2LeftoverPastCb38Logged)
                return;
            if (pc != LeftoverCb38)
                return;
            _tv2LeftoverPastCb38Logged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb3c = 0;
            if (TryPeekWord(bus, LeftoverCb3c, out cb3c)
                && (LeftoverCb3c & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb3cPeeked = true;
                _tv2LeftoverCb3cWord = cb3c;
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
                " from=0x03F6CB34 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb3c-word=0x" + cb3c.ToString("X8") +
                " (past leftover or $v0,$s7,$0; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb3c(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb38Logged || _tv2LeftoverPastCb3cLogged)
                return;
            if (pc != LeftoverCb3c)
                return;
            _tv2LeftoverPastCb3cLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb40 = 0;
            if (TryPeekWord(bus, LeftoverCb40, out cb40)
                && (LeftoverCb40 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb40Peeked = true;
                _tv2LeftoverCb40Word = cb40;
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
                " from=0x03F6CB38 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb40-word=0x" + cb40.ToString("X8") +
                " (past leftover lw $fp,16($sp); do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb40(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb3cLogged || _tv2LeftoverPastCb40Logged)
                return;
            if (pc != LeftoverCb40)
                return;
            _tv2LeftoverPastCb40Logged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb44 = 0;
            if (TryPeekWord(bus, LeftoverCb44, out cb44)
                && (LeftoverCb44 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb44Peeked = true;
                _tv2LeftoverCb44Word = cb44;
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
                " from=0x03F6CB3C CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb44-word=0x" + cb44.ToString("X8") +
                " (past leftover lw $s7,20($sp); do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb44(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastCb40Logged || _tv2LeftoverPastCb44Logged)
                return;
            if (pc != LeftoverCb44)
                return;
            _tv2LeftoverPastCb44Logged = true;
            TryCaptureLeftoverUserRa(bus, null);
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb48 = 0;
            if (TryPeekWord(bus, LeftoverCb48, out cb48)
                && (LeftoverCb48 & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb48Peeked = true;
                _tv2LeftoverCb48Word = cb48;
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
                " from=0x03F6CB40 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb48-word=0x" + cb48.ToString("X8") +
                " (past leftover lw $s6,24($sp); do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        private static bool IsLeftoverUserRa(uint ra)
        {
            if (ra == 0 || ra == 0xFFFFFFFFu || ra == 0xFFFFFFECu)
                return false;
            if ((ra & 0x1FFFFFFFu) < 0x00010000u)
                return false;
            if (ra >= 0x03F6C8F4u && ra <= LeftoverCb4c)
                return false;
            if ((ra & 0xFF000000u) == 0x0C000000u)
                return false;
            return IsFirmwareUserOrCoredllVa(ra);
        }

        private static void TryCaptureLeftoverUserRa(MipsBus bus, uint[] regs)
        {
            if (_tv2LeftoverUserRaSet)
                return;
            if (!IsFirmwareUserSlotVa(_tv2StoreSp))
                return;
            uint stacked = 0;
            if (TryPeekWord(bus, _tv2StoreSp + 28, out stacked)
                && IsLeftoverUserRa(stacked))
            {
                _tv2LeftoverUserRa = stacked;
                _tv2LeftoverUserRaSet = true;
            }
        }

        public static void TryNoteTv2LeftoverPastCb48(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverPastCb44Logged || _tv2LeftoverPastCb48Logged)
                return;
            if (pc != LeftoverCb48)
                return;
            _tv2LeftoverPastCb48Logged = true;
            TryCaptureLeftoverUserRa(bus, regs);
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint cb4c = 0;
            if (TryPeekWord(bus, LeftoverCb4c, out cb4c)
                && (LeftoverCb4c & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverCb4cPeeked = true;
                _tv2LeftoverCb4cWord = cb4c;
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
                " from=0x03F6CB44 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " cb4c-word=0x" + cb4c.ToString("X8") +
                " ra=0x" + (_tv2LeftoverUserRaSet ? _tv2LeftoverUserRa.ToString("X8") : "unset") +
                " (past leftover lw $ra,28($sp); do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastCb4c(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverPastCb48Logged || _tv2LeftoverPastCb4cLogged)
                return;
            if (pc != LeftoverCb4c)
                return;
            _tv2LeftoverPastCb4cLogged = true;
            TryCaptureLeftoverUserRa(bus, regs);
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
                " from=0x03F6CB48 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " ra=0x" + (_tv2LeftoverUserRaSet ? _tv2LeftoverUserRa.ToString("X8") : "unset") +
                " (past leftover jr $ra delay slot; peeked 28($sp); do not invent 0x03F731E4; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastJrRa(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverAfterCb4cLogged || _tv2LeftoverPastJrRaLogged)
                return;
            if (_tv2LeftoverJrRaDest == 0 || pc != _tv2LeftoverJrRaDest)
                return;
            _tv2LeftoverPastJrRaLogged = true;
            if (regs != null && regs.Length > 2)
            {
                uint v0 = regs[2];
                if (v0 < 0x80010000u || v0 >= 0x80020000u)
                {
                    _tv2LeftoverBeqRaV0Set = true;
                    _tv2LeftoverBeqRaV0 = v0;
                }
            }
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            uint ft = 0;
            if (TryPeekWord(bus, LeftoverBeqRaFt, out ft)
                && (LeftoverBeqRaFt & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBeqRaFtPeeked = true;
                _tv2LeftoverBeqRaFtWord = ft;
            }
            uint tk = 0;
            if (TryPeekWord(bus, LeftoverBeqRaTk, out tk)
                && (LeftoverBeqRaTk & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBeqRaTkPeeked = true;
                _tv2LeftoverBeqRaTkWord = tk;
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
                " from=0x03F6CB48 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " ft-word=0x" + ft.ToString("X8") +
                " tk-word=0x" + tk.ToString("X8") +
                " v0=0x" + (_tv2LeftoverBeqRaV0Set ? _tv2LeftoverBeqRaV0.ToString("X8") : "unset") +
                " (past leftover jr $ra; live $ra; peek 0x03F731E8/0x03F73210; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6CAC0; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastBeqRaFt(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastJrRaLogged || _tv2LeftoverPastBeqRaFtLogged)
                return;
            if (pc != LeftoverBeqRaFt)
                return;
            _tv2LeftoverPastBeqRaFtLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBeqRaFtPeeked = true;
                _tv2LeftoverBeqRaFtWord = word;
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
                " from=0x03F731E4 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover beq $v0,$0,+10 fallthrough; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastBeqRaTk(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastJrRaLogged || _tv2LeftoverPastBeqRaTkLogged)
                return;
            if (pc != LeftoverBeqRaTk)
                return;
            _tv2LeftoverPastBeqRaTkLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBeqRaTkPeeked = true;
                _tv2LeftoverBeqRaTkWord = word;
            }
            uint delay = 0;
            if (TryPeekWord(bus, LeftoverBPlus2Delay, out delay)
                && (LeftoverBPlus2Delay & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2DelayPeeked = true;
                _tv2LeftoverBPlus2DelayWord = delay;
            }
            uint taken = 0;
            if (TryPeekWord(bus, LeftoverBPlus2Taken, out taken)
                && (LeftoverBPlus2Taken & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2TakenPeeked = true;
                _tv2LeftoverBPlus2TakenWord = taken;
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
                " from=0x03F731E4 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " delay-word=0x" + delay.ToString("X8") +
                " taken-word=0x" + taken.ToString("X8") +
                " (past leftover beq $v0,$0,+10 taken; peek 0x03F73214/0x03F7321C; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastBPlus2Delay(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastBeqRaTkLogged || _tv2LeftoverPastBPlus2DelayLogged)
                return;
            if (pc != LeftoverBPlus2Delay)
                return;
            _tv2LeftoverPastBPlus2DelayLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2DelayPeeked = true;
                _tv2LeftoverBPlus2DelayWord = word;
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
                " from=0x03F73210 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover b +2 delay slot; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastBPlus2Taken(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastBeqRaTkLogged || _tv2LeftoverPastBPlus2TakenLogged)
                return;
            if (pc != LeftoverBPlus2Taken)
                return;
            _tv2LeftoverPastBPlus2TakenLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2TakenPeeked = true;
                _tv2LeftoverBPlus2TakenWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverBPlus2Next, out next)
                && (LeftoverBPlus2Next & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2NextPeeked = true;
                _tv2LeftoverBPlus2NextWord = next;
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
                " from=0x03F73210 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover b +2 taken; peek 0x03F73220; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastBPlus2Next(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastBPlus2TakenLogged || _tv2LeftoverPastBPlus2NextLogged)
                return;
            if (pc != LeftoverBPlus2Next)
                return;
            _tv2LeftoverPastBPlus2NextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverBPlus2NextPeeked = true;
                _tv2LeftoverBPlus2NextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverFpNext, out next)
                && (LeftoverFpNext & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverFpNextPeeked = true;
                _tv2LeftoverFpNextWord = next;
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
                " from=0x03F7321C CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover or $sp,$s8,$0; peek 0x03F73224; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastFpNext(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastBPlus2NextLogged || _tv2LeftoverPastFpNextLogged)
                return;
            if (pc != LeftoverFpNext)
                return;
            _tv2LeftoverPastFpNextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverFpNextPeeked = true;
                _tv2LeftoverFpNextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverS7Next, out next)
                && (LeftoverS7Next & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS7NextPeeked = true;
                _tv2LeftoverS7NextWord = next;
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
                " from=0x03F73220 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover lw $fp,16($sp); peek 0x03F73228; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastS7Next(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastFpNextLogged || _tv2LeftoverPastS7NextLogged)
                return;
            if (pc != LeftoverS7Next)
                return;
            _tv2LeftoverPastS7NextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS7NextPeeked = true;
                _tv2LeftoverS7NextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverS6Next, out next)
                && (LeftoverS6Next & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS6NextPeeked = true;
                _tv2LeftoverS6NextWord = next;
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
                " from=0x03F73224 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover lw $s7,20($sp); peek 0x03F7322C; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastS6Next(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastS7NextLogged || _tv2LeftoverPastS6NextLogged)
                return;
            if (pc != LeftoverS6Next)
                return;
            _tv2LeftoverPastS6NextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS6NextPeeked = true;
                _tv2LeftoverS6NextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverS5Next, out next)
                && (LeftoverS5Next & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS5NextPeeked = true;
                _tv2LeftoverS5NextWord = next;
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
                " from=0x03F73228 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover lw $s6,24($sp); peek 0x03F73230; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastS5Next(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastS6NextLogged || _tv2LeftoverPastS5NextLogged)
                return;
            if (pc != LeftoverS5Next)
                return;
            _tv2LeftoverPastS5NextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS5NextPeeked = true;
                _tv2LeftoverS5NextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverS4Next, out next)
                && (LeftoverS4Next & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS4NextPeeked = true;
                _tv2LeftoverS4NextWord = next;
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
                " from=0x03F7322C CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover lw $s5,28($sp); peek 0x03F73234; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastS4Next(MipsBus bus, uint pc)
        {
            if (!_tv2LeftoverPastS5NextLogged || _tv2LeftoverPastS4NextLogged)
                return;
            if (pc != LeftoverS4Next)
                return;
            _tv2LeftoverPastS4NextLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverS4NextPeeked = true;
                _tv2LeftoverS4NextWord = word;
            }
            uint next = 0;
            if (TryPeekWord(bus, LeftoverEpilogueNext, out next)
                && (LeftoverEpilogueNext & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverEpiloguePeeked = true;
                _tv2LeftoverEpilogueWord = next;
                if (_tv2LeftoverDestLiveNext == 0)
                    _tv2LeftoverDestLiveNext = LeftoverEpilogueNext;
            }
            if (_tv2LeftoverDestLiveNext != 0)
                TryKeepLeftoverDestLiveCtx(bus, _tv2LeftoverDestLiveNext);
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
                " from=0x03F73230 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " next-word=0x" + next.ToString("X8") +
                " (past leftover lw $ra,36($sp); peek 0x03F73238; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastEpilogue(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverPastS4NextLogged || _tv2LeftoverPastEpilogueLogged)
                return;
            if (pc != LeftoverEpilogueNext)
                return;
            _tv2LeftoverPastEpilogueLogged = true;
            uint word = 0;
            bool mapped = TryPeekWord(bus, pc, out word);
            if (mapped && (pc & 0x1FFFFFFFu) >= 0x00010000u)
            {
                _tv2LeftoverEpiloguePeeked = true;
                _tv2LeftoverEpilogueWord = word;
            }
            TryCaptureLeftoverEpilogueRa(bus, regs);
            if (IsFirmwareJrRa(word))
                _tv2LeftoverDestLiveNext = LeftoverEpilogueNext + 4;
            if (_tv2LeftoverDestLiveNext != 0)
                TryKeepLeftoverDestLiveCtx(bus, _tv2LeftoverDestLiveNext);
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
                " from=0x03F73234 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " (past leftover dest-live next; peek dest-live $ra; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        public static void TryNoteTv2LeftoverPastEpilogueDelay(MipsBus bus, uint[] regs, uint pc)
        {
            if (!_tv2LeftoverPastEpilogueLogged || _tv2LeftoverPastEpilogueDelayLogged)
                return;
            if (pc != LeftoverEpilogueNext + 4)
                return;
            _tv2LeftoverPastEpilogueDelayLogged = true;
            TryCaptureLeftoverEpilogueRa(bus, regs);
            // dest-live continue stays live. dest-live
            // $ra is already walked (leftover past dest-live
            // $ra). dest-live next after dest-live delay is
            // leftover dest-live delay's live leftover next
            // (leftover $ra at dest-live jr $ra if live
            // leftover dest), not dest-live $ra, not PC+4.
            // prior peek named 0x03F731E4 as evidence only;
            // do not invent dest. Do not follow dest-live
            // $ra blindly. Do not hop 0x03F73238.
            uint next = 0;
            if (regs != null && regs.Length > 31 && IsLeftoverUserRa(regs[31])
                && (!_tv2LeftoverPastJrRaLogged || regs[31] != _tv2LeftoverUserRa))
                next = regs[31];
            if (next == 0)
                next = pc + 4;
            _tv2LeftoverDestLiveNext = next;
            TryKeepLeftoverDestLiveCtx(bus, _tv2LeftoverDestLiveNext);
            // leftover ERET 0x80015A24 uses $v0, not leftover
            // ctxPC. leftover dest-live ERET $v0 restore after
            // dest-live delay writes leftover $v0 to leftover
            // dest-live delay's live leftover next so leftover
            // ERET / dest-live continue hops leftover mid /
            // ERET2 to leftover dest-live delay's live leftover
            // next, not dest-live $ra, not ERET2, not PC+4.
            // Do not invent dest. Do not hop 0x03F73238.
            // Do not rewrite 0x80015B9C.
            TryRestoreTv2LeftoverDestLiveEret(bus, regs, pc);
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
                " from=0x03F73238 CurThread=0x" + curThr.ToString("X8") +
                " CurProc=0x" + cur.ToString("X8") +
                " dest-" + (mapped ? "mapped" : "unmapped") +
                " dest-word=0x" + word.ToString("X8") +
                " ra=0x" + _tv2LeftoverUserRa.ToString("X8") +
                " (past leftover dest-live delay; do not invent dest; do not rewrite 0x80015B9C; do not rewind 0x03F6C8F4; not TV UI)");
        }

        private static void TryCaptureLeftoverEpilogueRa(MipsBus bus, uint[] regs)
        {
            if (_tv2LeftoverUserRaSet && IsLeftoverUserRa(_tv2LeftoverUserRa))
                return;
            uint stacked = 0;
            uint sp = 0;
            if (regs != null && regs.Length > 29 && IsFirmwareUserSlotVa(regs[29]))
                sp = regs[29];
            else if (IsFirmwareUserSlotVa(_tv2StoreSp))
                sp = _tv2StoreSp;
            if (sp != 0 && TryPeekWord(bus, sp + 36, out stacked) && IsLeftoverUserRa(stacked))
            {
                _tv2LeftoverUserRa = stacked;
                _tv2LeftoverUserRaSet = true;
                return;
            }
            if (regs != null && regs.Length > 31 && IsLeftoverUserRa(regs[31]))
            {
                _tv2LeftoverUserRa = regs[31];
                _tv2LeftoverUserRaSet = true;
            }
        }

        private static bool IsFirmwareJrRa(uint word)
        {
            return word == 0x03E00008u;
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
        private static bool _ddiNopDest0PteLogged;
        // Live 6f80c88: dest0 PTE 0x01981000 -> 0x86F1C000.
        // PeekDestWord(useg) stayed 0. Firmware dest is the
        // PTE result. Do not invent this; walk fills it.
        private static uint _ddiNopDest0Pte;
        private static bool _ddiNopDestPeekRaw;
        private static bool _ddiNopDestPteMeasured;
        private static uint _ddiNopLandedDest;
        private static uint _ddiNopLandedWord;
        private static bool _ddiNopLandedBySig;
        private static uint _ddiNopModule;
        private static uint _ddiNopBindLibV0;
        private static string _ddiNopBindLibName;
        private static uint _coredllModule;
        private static bool _coredllBasePtrLogged;
        private static uint _ddiNopFileObj;
        private static bool _ddiNopStartipAttempted;
        private static bool _ddiNopAwaitCallDll;
        private static bool _ddiNopSawCallDllPc;
        private static bool _ddiNopCallDllMissLogged;
        private static int _ddiNopCallDllMissPoll;
        private static bool _ddiNopStallLogged;
        private static bool _ddiNopIatLogged;
        private static bool _ddiNopDataO32Logged;
        private static bool _ddiNopIatWatch;
        private static bool _ddiNopIatStoreLogged;
        private static int _ddiNopIatStoreN;
        private static uint _ddiNopIatDest6;
        private static uint _ddiNopIatReal;
        private static uint _ddiNopIatValloc;
        private static uint _ddiNopIatSpan;
        private static uint _ddiNopIatPsize;
        private static int _bindImpIatSlotLog;
        private static int _ddiNopOrdLog;
        private static uint _ddiNopOrdLastA1;
        private static int _ddiNopOrdRetLog;
        private static uint _ddiNopOrdRetLastA1;
        private static bool _ddiNopOrdExpLogged;
        private static uint _ddiNopOrdGoodV0;
        private static bool _ddiNopOrdAfterDone;
        private static int _ddiNopOrdAfterN;
        private static uint _ddiNopOrdAfterLast;
        private static bool _userKPageAlias;
        private static bool _userKPageAliasNoted;
        private static bool _ffffFce1Logged;
        private static uint _ffffF000Kseg;
        private static bool _ffffF000Logged;
        private static bool _ffffF000Busy;
        private static bool _ffffF000Demand;
        private static bool _ffffF000Done;
        private static bool _bindImpIatSwExpect;
        private static bool _bindImpIatSwLogged;
        private static int _bindImpIatSwLog;
        private static int _bindImpIatWinLog;
        private static uint _bindImpIatWinLast;
        private static int _bindImpIatNextLog;
        private static uint _bindImpIatNextLast;
        private static bool _bindImpExnLogged;
        private static bool _bindImpExnSaveLogged;
        private static uint _bindImpExnCode;
        private static uint _bindImpExnEpc;
        private static uint _bindImpExnVaddr;
        private static bool _gwesB9SpinLogged;
        private static uint _gwesB9SpinPage;
        private static int _gwesB9SpinN;
        private static bool _gwesNullStoreLogged;
        private static bool _nearNullTlblLogged;
        private static bool _ffffFb2aAdelLogged;
        private static bool _adelC6FaLogged;
        private static uint _adelPcEpc;
        private static uint _adelPcSp;
        private static bool _adelPlantClrLogged;
        private static bool _idleHaltLogged;
        private static uint _exnContinueWord;
        private static bool _thrSpLogged;
        private static bool _spFixLogged;
        private static bool _plantFixLogged;
        private static bool _plantHaltLogged;
        private static bool _leftoverHaltLogged;
        private static bool _leftoverSkipLogged;
        private static bool _leftoverFrameLogged;
        private static bool _epcHaltLogged;
        private static bool _c2TlbsLogged;
        private static bool _c2SpLogged;
        private static bool _c2EretHaltLogged;
        private static bool _ddiNopInfoObserved;
        private static bool _ddiNopInfoDemand;
        private static bool _ddiNopInfoBusy;
        private static bool _ddiNopInfoPeekRaw;
        private static bool _ddiNopInfoMapLogged;
        private static uint _ddiNopInfoKseg;
        private static bool _ddiNopCallDllHiveLogged;
        private static bool _ddiNopDllMainLogged;
        private static uint _ddiNopDllMainRa;
        private static uint _ddiNopCallDllSite;
        private static bool _ddiNopAfterDllMainLogged;
        private static uint _ddiNopGwesFetchKseg;
        private static bool _ddiNopGwesFetchLogged;
        private static bool _ddiNopGwesFetchBusy;
        private static bool _ddiNopGwesFetchDemand;
        private static bool _ddiNopGwesFetchTlblLogged;
        private static uint _ddiNopGwesDataKseg;
        private static bool _ddiNopGwesDataLogged;
        private static bool _ddiNopGwesDataBusy;
        private static bool _ddiNopGwesDataDemand;
        private static bool _ddiNopGwesDataTlblLogged;
        private static uint _ddiNopGwesTextKseg;
        private static bool _ddiNopGwesTextLogged;
        private static bool _ddiNopGwesTextBusy;
        private static bool _ddiNopGwesTextDemand;
        private static bool _ddiNopGwesTextTlblLogged;
        private static uint _ddiNopGwesData2Kseg;
        private static bool _ddiNopGwesData2Logged;
        private static bool _ddiNopGwesData2Busy;
        private static bool _ddiNopGwesData2Demand;
        private static bool _ddiNopGwesData2TlblLogged;
        private static uint _ddiNopGwesData3Kseg;
        private static bool _ddiNopGwesData3Logged;
        private static bool _ddiNopGwesData3Busy;
        private static bool _ddiNopGwesData3Demand;
        private static bool _ddiNopGwesData3TlblLogged;
        private static uint _ddiNopGwesText2Kseg;
        private static bool _ddiNopGwesText2Logged;
        private static bool _ddiNopGwesText2Busy;
        private static bool _ddiNopGwesText2Demand;
        private static bool _ddiNopGwesText2TlblLogged;
        private static uint[] _gwesImagePage;
        private static uint[] _gwesImageKseg;
        private static bool[] _gwesImageDone;
        private static bool[] _gwesImageTlbl;
        private static int _gwesImageN;
        private static bool _gwesImageDemand;
        private static bool _gwesImageBusy;
        private static uint[] _coredllImagePage;
        private static uint[] _coredllImageKseg;
        private static bool[] _coredllImageDone;
        private static bool[] _coredllImageTlbl;
        private static int _coredllImageN;
        private static bool _coredllImageDemand;
        private static bool _coredllImageBusy;
        private static bool _coredllSlotViewLogged;
        private static bool _coredllSlotViewTlbl;
        private static uint _filesysSlot2Kseg;
        private static bool _filesysSlot2Logged;
        private static bool _filesysSlot2Busy;
        private static bool _filesysSlot2Demand;
        private static bool _filesysSlot2TlblLogged;
        private static bool _filesysSlot4Logged;
        private static bool _filesysSlot4TlblLogged;
        private static uint[] _filesysSlot2ExtraPage;
        private static uint[] _filesysSlot2ExtraKseg;
        private static bool[] _filesysSlot2ExtraLogged;
        private static bool[] _filesysSlot2ExtraTlbl;
        private static bool[] _filesysSlot2ExtraMiss;
        private static int _filesysSlot2ExtraN;
        private static bool _filesysSlot2ExtraBusy;
        private static bool _filesys48dLogged;
        private static uint[] _filesys48dKsegs;
        private static bool[] _filesys48dDone;
        private static bool[] _filesys48dTlbl;
        private static bool _filesys48dBusy;
        private static uint[] _ddiDataPage;
        private static uint[] _ddiDataKseg;
        private static bool[] _ddiDataDone;
        private static bool[] _ddiDataTlbl;
        private static int _ddiDataN;
        private static bool _ddiDataDemand;
        private static bool _ddiDataBusy;
        private static bool _ddiPrefPcLogged;
        private static uint[] _ddiNopWalkSeeds;
        private static int _ddiNopWalkSeedN;
        private static bool _ddiNopNoModDiag;
        private static bool _ddiNopWalkDiag;
        private static bool _ddiNopDecompWatch;
        private static uint _ddiNopWatchDest6;
        private static uint _ddiNopWatchDest10;
        private static uint _ddiNopWatchVbase6;
        private static int _ddiNopStoreN0;
        private static int _ddiNopStoreN6;
        private static int _ddiNopStoreNV6;
        private static int _ddiNopStoreN10;
        private static int _ddiNopStoreND;
        private static int _ddiNopStoreNK;
        private static uint _ddiNopStoreFirstVa;
        private static uint _ddiNopStoreFirstVal;
        private static uint _ddiNopStoreLastVa;
        private static uint _ddiNopStoreLastVal;
        private static bool _ddiNopStoreThrew0;
        private static bool _ddiNopStoreThrew6;
        private static bool _ddiNopStoreThrewV;
        private static bool _ddiNopStoreThrew10;
        private static bool _ddiNopStoreThrewD;
        private static bool _ddiNopStoreThrewK;
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
            _ddiNopDest0PteLogged = false;
            _ddiNopDest0Pte = 0;
            _ddiNopDestPeekRaw = false;
            _ddiNopDestPteMeasured = false;
            _ddiNopLandedDest = 0;
            _ddiNopLandedWord = 0;
            _ddiNopLandedBySig = false;
            _ddiNopModule = 0;
            ResetDdiNopModuleHunt();
            _mscoreeDestOn = false;
            _mscoreeSlot0 = 0;
            _ole32DestOn = false;
            _ole32Slot0 = 0;
            _ddiNopDecompRa = 0;
            _ddiNopDecompSrc = 0;
            _ddiNopDecompCb = 0;
            _ddiNopDecompDest = 0;
            _ddiNopDecompVsize = 0;
            _ddiNopDecompHdr = 0;
            ResetDdiNopDecompStores();
            _ddiNopDestWordLogged = false;
            _ddiNopObserve = false;
            _ddiNopInnerCap = false;
            _ddiNopInnerPages = 0;
            _ddiNopBindHdr = false;
            _ddiNopBindName = false;
            _ddiNopBindLib = false;
            _ddiNopBindLibRet = false;
            ResetDdiNopModuleHunt();
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
            if (_ddiNopDestPeekRaw)
                return va;
            if (_ddiNopDestOn && _ddiNopSlot0 != 0)
            {
                if (va >= DdiNopVbase && va < 0x039B0000u)
                    va = _ddiNopSlot0 + (va - DdiNopVbase);
                // Live 330f08b: ExtraRomDestKseg0 did not
                // receive dest=0x01981000 stores. dest0 stays
                // useg; MapFirmwareSlotVa walks firmware PTE.
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
            if (va >= ExtraRomE32Host && va < ExtraRomE32HostLim)
                return va;
            for (int i = 0; i < _vallocHostN; i++)
            {
                if (va >= _vallocHostLo[i] && va < _vallocHostHi[i])
                    return _vallocHostKseg[i] + (va - _vallocHostLo[i]);
            }
            return va;
        }

        // Live edf15b0: process-info page had no PTE and
        // KData peek missed. Host-back one zero 4K via
        // the existing valloc pool. Do not invent heap.
        private static bool TryHostBackProcessInfoPage()
        {
            uint lo = ProcessInfoPage;
            uint hi = ProcessInfoPage + 0x1000u;
            if (_ddiNopInfoKseg != 0)
                return true;
            if (VallocHostCovers(lo, hi))
            {
                for (int i = 0; i < _vallocHostN; i++)
                {
                    if (_vallocHostLo[i] <= lo && _vallocHostHi[i] >= hi)
                    {
                        _ddiNopInfoKseg = _vallocHostKseg[i];
                        return _ddiNopInfoKseg != 0;
                    }
                }
                return false;
            }
            if (_vallocHostN >= _vallocHostLo.Length)
                return false;
            uint span = 0x1000u;
            uint kseg = _vallocHostPool;
            if (kseg < VallocHostKseg || kseg + span > VallocHostKsegLim)
                return false;
            _vallocHostLo[_vallocHostN] = lo;
            _vallocHostHi[_vallocHostN] = hi;
            _vallocHostKseg[_vallocHostN] = kseg;
            _vallocHostN++;
            _vallocHostPool += span;
            _ddiNopInfoKseg = kseg;
            return true;
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

        // ExtraROM FILE type-8 OpenFile after FILE[25]+TOC[46].
        // FILE table names only. Do not match TOC type-7
        // (mscoree / ole32 / tv2engine / mscoree3_5 / zlib /
        // uspce / raswrap / crypt32 / toolhelp). Do not invent
        // xdrm.dll. FILE[25] stays IsTv2ClientCe. FILE[11]/[26]
        // dest/cache stay ExtraRomFileDest 0x8F400000 class.
        // Any other ExtraROM FILE[0..47] (RunOnce.exe FILE[32])
        // uses that same dest class when CE CreateFile/OpenFile
        // asks. Do not attach ExtraROM TOC names as type-8.
        private static readonly string[] ExtraRomOpenFileNames =
        {
            "mscorlib.dll",
            "tv2clientcorece.dll",
            "system.dll",
            "system.core.dll",
            "system.drawing.dll",
            "system.web.services.dll",
            "system.windows.forms.dll",
            "system.xml.dll",
            "broadcastservermanagedbridge_dvbs_ce.dll",
            "managednetworkclient_dvbs_ce.dll"
        };

        public static bool IsExtraRomOpenFile(string name)
        {
            if (IsTv2ClientCe(name))
                return false;
            if (FindCachedExtraRomToc(name) != null)
                return false;
            return ExtraRomOpenFileName(name).Length != 0;
        }

        private static string ExtraRomOpenFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            for (int i = 0; i < ExtraRomOpenFileNames.Length; i++)
            {
                string n = ExtraRomOpenFileNames[i];
                if (NamesEqual(name, n) || NamesMatchRom(name, n))
                    return n;
                if (NamesEqual(name, n + ".dll") || NamesEqual(name, n + ".exe"))
                    return n;
            }
            ExtraRomOpenFile slot = FindExtraRomOpenFile(name);
            if (slot != null && !string.IsNullOrEmpty(slot.Label))
                return slot.Label;
            return "";
        }

        private sealed class ExtraRomOpenFile
        {
            public int Index;
            public uint Entry;
            public uint[] Words;
            public uint Name;
            public uint[] NameWords;
            public uint Real;
            public uint Comp;
            public uint Load;
            public uint[] Data;
            public string Label;
        }

        private sealed class ExtraRomTocMod
        {
            public int Index;
            public string Name;
            public uint Entry;
            public uint Attr;
            public uint LoadVa;
            public uint Dest;
            public uint E32;
            public uint O32;
            public uint[] TocWords;
            public uint[] E32Words;
            public uint[] O32Words;
            public uint LiveEntry;
            public uint LiveE32;
            public uint LiveO32;
            public uint LiveName;
            public uint Vbase;
            public uint[] DataPtr;
            public uint[] DataLen;
            public uint[][] Data;
            public bool Decompressed;
            public uint DecompDest;
            public bool LoadE32Ok;
            public bool BuiltInSkip;
            public bool FwMapO32;
            public bool LoggedFwMapO32;
            public bool LoggedFwMapInner;
            public bool LoggedFwMap28844;
        }

        // OpenExe retries \mscoree.dll.dll. Same suffix on any
        // ExtraROM TOC name. Do not invent a second module.
        private static string RomLookupName(string name)
        {
            name = FileBaseName(name);
            if (string.IsNullOrEmpty(name) || name.Length < 8)
                return name;
            int n = name.Length;
            if (n >= 8
                && ((name[n - 8] == '.' && (name[n - 7] == 'd' || name[n - 7] == 'D')
                    && (name[n - 6] == 'l' || name[n - 6] == 'L')
                    && (name[n - 5] == 'l' || name[n - 5] == 'L')
                    && name[n - 4] == '.' && (name[n - 3] == 'd' || name[n - 3] == 'D')
                    && (name[n - 2] == 'l' || name[n - 2] == 'L')
                    && (name[n - 1] == 'l' || name[n - 1] == 'L'))
                    || (name[n - 8] == '.' && (name[n - 7] == 'e' || name[n - 7] == 'E')
                    && (name[n - 6] == 'x' || name[n - 6] == 'X')
                    && (name[n - 5] == 'e' || name[n - 5] == 'E')
                    && name[n - 4] == '.' && (name[n - 3] == 'e' || name[n - 3] == 'E')
                    && (name[n - 2] == 'x' || name[n - 2] == 'X')
                    && (name[n - 1] == 'e' || name[n - 1] == 'E'))))
                return name.Substring(0, n - 4);
            return name;
        }

        private static bool NamesMatchRom(string asked, string have)
        {
            if (NamesEqual(asked, have))
                return true;
            if (string.IsNullOrEmpty(asked) || string.IsNullOrEmpty(have))
                return false;
            if (NamesEqual(RomLookupName(asked), have))
                return true;
            string askStem = StripRomExt(RomLookupName(asked));
            string haveStem = StripRomExt(have);
            return NamesEqual(askStem, haveStem);
        }

        // ExtraROM iptvhal_* TOC names. CE may CreateFile/LoadLibrary
        // iptvhal.dll or iptvhal_*.dll. Do not invent a second GDI DDI.
        private static bool IsIptvHalAsk(string name)
        {
            string stem = StripRomExt(RomLookupName(name));
            if (string.IsNullOrEmpty(stem) || stem.Length < 7)
                return false;
            return (stem[0] == 'i' || stem[0] == 'I')
                && (stem[1] == 'p' || stem[1] == 'P')
                && (stem[2] == 't' || stem[2] == 'T')
                && (stem[3] == 'v' || stem[3] == 'V')
                && (stem[4] == 'h' || stem[4] == 'H')
                && (stem[5] == 'a' || stem[5] == 'A')
                && (stem[6] == 'l' || stem[6] == 'L');
        }

        private static string StripRomExt(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 5)
                return name;
            int n = name.Length;
            if (n >= 4 && name[n - 4] == '.'
                && ((name[n - 3] == 'd' || name[n - 3] == 'D')
                    && (name[n - 2] == 'l' || name[n - 2] == 'L')
                    && (name[n - 1] == 'l' || name[n - 1] == 'L')
                    || (name[n - 3] == 'e' || name[n - 3] == 'E')
                    && (name[n - 2] == 'x' || name[n - 2] == 'X')
                    && (name[n - 1] == 'e' || name[n - 1] == 'E')))
                return name.Substring(0, n - 4);
            return name;
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
