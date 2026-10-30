using System;
using System.Collections.Generic;
using ProcessorEmulator.Emulation;
using ProcessorEmulator.Core;

namespace ProcessorEmulator
{
    // EXTRAROM_PCSAMPLE=1: observe-only PC/thread sampler (no state change).
    // Every EXTRAROM_PCSAMPLE_SEC (default 5) s logs pc/sp/ra, step count,
    // hottest 4 KiB code pages since last sample, and KData words.
    // KData 0xFFFFDAC0/0xFFFFDAC4 are the words nk uses as cur thread/proc
    // (lw -0x2540/-0x253C($zero)); changes are logged (capped) as switches.
    // One-shot milestones: RunApps 0x8003EABC, CreateProc m[53] 0x80034D2C
    // ("filesys.exe" a0=0x800122D4), its return 0x8003EB48, FSReady wait
    // return 0x8003EB74. Kernel init Status template 0xFFFFD890 path (143E8/14408/14440),
    // OEMInit 0x80054F8C, HookInterrupt 0x80020B98 (+callers 55F90 timer hw5, 5688C PIC hw0), mask-off 0x80015B64.
    internal static class PcSampler
    {
        static readonly bool On = Environment.GetEnvironmentVariable("EXTRAROM_PCSAMPLE") == "1";
        static readonly int Sec = int.TryParse(Environment.GetEnvironmentVariable("EXTRAROM_PCSAMPLE_SEC"), out var s) && s > 0 ? s : 5;
        static readonly System.Diagnostics.Stopwatch _sw = System.Diagnostics.Stopwatch.StartNew();
        static long _next, _steps; public static long Steps => _steps;
        static readonly Dictionary<uint, int> _pages = new Dictionary<uint, int>(); static readonly Dictionary<uint, int> _pcs = new Dictionary<uint, int>();
        static uint _k0, _k1; static bool _kInit; static int _sw_n;
        static readonly HashSet<uint> _hit = new HashSet<uint>(); static int _ckt, _api, _exn, _gpa;
        static readonly uint[] Marks = { 0x8003EABC, 0x80034D2C, 0x8003EB48, 0x8003EB74, 0x80030604, 0x8003EB80, 0x800143E8, 0x80014408, 0x80014440, 0x80054F8C, 0x80020B98, 0x80055F90, 0x8005688C, 0x80015B64, 0x80014424, 0x80014428, 0x80014438, 0x800151E0, 0x8001728C, 0x80023500, 0x80037338, 0x80037340, 0x80037360, 0x80037368, 0x80037370, 0x80037380, 0x80037388, 0x800373C0, 0x800356FC, 0x80040CA8, 0x80030664, 0x80030688, 0x800306A4, 0x800306D8, 0x8002FF98, 0x8003EAF8, 0x8003EB10, 0x80034D30, 0x8001597C, 0x80015980 };

        static uint P(MipsBus bus, uint va) { uint w; return CeRomTocFiles.PeekWordForSampler(bus, va, out w) ? w : 0xDEADDEADu; }

        // EXTRAROM_IM_RESTORE=1 (HOST-ASSISTED, firmware-computed value).
        // Kernel start: 0x80014410/14 s0 = *(0xFFFFD890) & 0xFF00 (IM set by
        // OEMInit HookInterrupt: hw0 PIC 0x400 + hw5 timer 0x8000 = 0x8400),
        // then jal 0x8001728C / 0x80023500, then 0x8001443C or s1,s1,s0 and
        // 0x80014440 template = s1. Live run: s0 came back 0 from 0x80023500
        // (callee-saved $s0 lost on a host-assisted path), template stayed 0,
        // Status IM=0 and OEMIdle 0x80059E98 spun with IP7+IP2 pending.
        // 0x80014408 can be reached again with template already 0; keep the first nonzero IM.
        // Latch IM at 0x80014408 and put it back in $s0 at 0x8001443C.
        static readonly bool ImOn = Environment.GetEnvironmentVariable("EXTRAROM_IM_RESTORE") == "1";
        static uint _imLatch; static bool _imDone; static int _imHits;
        static void ImStep(MipsBus bus, uint[] r, uint pc)
        {
            if (!ImOn || _imDone || r == null || r.Length < 32) return;
            if (pc == 0x80014408u) { uint t = P(bus, 0xFFFFD890u); _imHits++; if (t != 0xDEADDEADu && (t & 0xFF00u) != 0) _imLatch = t & 0xFF00u; return; }
            if (pc != 0x8001443Cu) return;
            _imDone = true;
            uint s0 = r[16];
            if (_imLatch == 0 || (s0 & 0xFF00u) == _imLatch)
            {
                BootLog.Write("[ImRestore] at 0x8001443C s0=0x" + s0.ToString("X8") + " latch=0x" + _imLatch.ToString("X8") + " hits14408=" + _imHits + " (firmware s0 intact; no host action)");
                return;
            }
            r[16] = s0 | _imLatch;
            BootLog.Write("[ImRestore] HOST-ASSISTED at 0x8001443C s0 0x" + s0.ToString("X8") + " -> 0x" + r[16].ToString("X8") + " (IM latched at 0x80014408 from OEMInit HookInterrupt; callee-saved $s0 lost across jal 0x80023500) " + bus.Cp0Snapshot());
        }
        // Observe-only: who fills 0x80347850 (+0x30 io fn) and what it returns. No writes.
        static int _io; static uint _ioTgt;
        static string Hex16(MipsBus bus, uint va) { var sb = new System.Text.StringBuilder(); for (uint k = 0; k < 16; k += 4) sb.Append(P(bus, va + k).ToString("X8")).Append(' '); return sb.ToString(); }
        static void IoProbe(MipsBus bus, uint[] r, uint pc)
        {
            if (_io >= 40) return;
            string tag = null;
            if (pc == 0x80061BD0u) tag = "init a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a1+30=0x" + P(bus, r[5] + 0x30).ToString("X8") + " a1+90=0x" + P(bus, r[5] + 0x90).ToString("X8") + " a0[]=" + Hex16(bus, r[4]);
            else if (pc == 0x800657ECu) tag = "F-entry a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " ra=0x" + r[31].ToString("X8");
            else if (pc == 0x80065844u || pc == 0x80061E44u || pc == 0x80061A40u) { _ioTgt = (pc == 0x80065844u) ? r[2] : (pc == 0x80061E44u ? r[8] : r[3]); tag = "io-call tgt=0x" + _ioTgt.ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " sp10..18=" + Hex16(bus, r[29] + 0x10) + " ra=0x" + r[31].ToString("X8"); }
            else if (pc == 0x8006584Cu || pc == 0x80061E4Cu) tag = "io-ret v0=0x" + r[2].ToString("X8") + " s6buf@0x" + r[22].ToString("X8") + "=" + Hex16(bus, r[22]) + " fp=0x" + r[30].ToString("X8");
            else if (_ioTgt != 0 && pc == _ioTgt) tag = "io-tgt-entry a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " ra=0x" + r[31].ToString("X8");
            if (tag == null) return;
            _io++;
            BootLog.Write("[PcSample] io " + tag + " pc=0x" + pc.ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps);
        }
        // Observe-only (EXTRAROM_PCSAMPLE): kernel debug-print format text, OsAxsT0 handler chain, thread state, exceptions by EPC. No writes.
        static int _dbgN, _chain, _thrN, _exnN; static readonly System.Collections.Generic.Dictionary<uint, int> _fmt = new System.Collections.Generic.Dictionary<uint, int>(); static readonly System.Collections.Generic.Dictionary<uint, int> _exnEpc = new System.Collections.Generic.Dictionary<uint, int>();
        static string WStr(MipsBus bus, uint va, int max)
        {
            var sb = new System.Text.StringBuilder();
            for (int k = 0; k < max; k += 2) { uint w = P(bus, va + (uint)(k * 2)); if (w == 0xDEADDEADu) break; char c0 = (char)(w & 0xFFFF), c1 = (char)(w >> 16); if (c0 == 0) break; sb.Append(c0 < 32 ? '.' : c0); if (c1 == 0) break; sb.Append(c1 < 32 ? '.' : c1); }
            return sb.ToString();
        }
        static string Words(MipsBus bus, uint va, int n) { var sb = new System.Text.StringBuilder(); for (int k = 0; k < n; k++) sb.Append(P(bus, va + (uint)(4 * k)).ToString("X8")).Append(' '); return sb.ToString(); }
        static int _poolN, _poolC; static uint _poolW = 0xFFFFFFFFu;
        static int _hcN;
        static int _shaN, _cpN, _raN;
        static int _apiDump; static readonly System.Collections.Generic.Dictionary<uint, int> _apiCnt = new System.Collections.Generic.Dictionary<uint, int>(), _exWin = new System.Collections.Generic.Dictionary<uint, int>(); static readonly System.Collections.Generic.Dictionary<uint, uint> _apiRa = new System.Collections.Generic.Dictionary<uint, uint>();
        static int _gwN, _uwN, _uw2, _gwStk;
        static uint _prevPc; static int _ri, _ringDumps; static readonly uint[] _ring = new uint[128], _ring2 = new uint[128], _ringRa = new uint[128];
        static int _rwN; static uint _rwLast = 0x12345678u;
        static string W(MipsBus bus, uint a, int n) { var sb = new System.Text.StringBuilder(); for (int i = 0; i < n; i++) sb.Append(P(bus, a + (uint)(i * 4)).ToString("X8")).Append(i % 8 == 7 ? "|" : " "); return sb.ToString(); }
        static bool _sipDump; static int _trN, _gwTr, _o32N, _o32wN, _sipU, _gw8Stk; static uint _sipLast; static bool _sipEntry, _waitDump; static int _e14N, _e14W, _o32aN, _ipTr, _dwN, _smN, _apiN, _wfN, _bbN, _alN; static uint _smThr; static uint _dwLast; static uint _o32Pa; static int _hpN, _ldN, _ld2N, _hwN, _hvN; static uint _hpPa = 1, _hpVal = 1; static MipsBus _sbus; public static uint LastInsn, LastFetchPc, LastPcAfterFetch; static int _nwN, _nrN; static uint _slotPa = 0xFFFFFFFFu;
        public static void NoteW(MipsBus bus, uint origVa, uint va, uint value)
        {
            if (value == 14u && _steps > 48000000 && _steps < 48200000 && _e14W < 40 && va >= 0x86000000u && va < 0x88000000u) { _e14W++; BootLog.Write("[PcSample] err14-w va=0x" + va.ToString("X8") + " pc=0x" + LastFetchPc.ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " step=" + _steps); }
            if (On && _o32wN < 40 && _steps > 80600000)
            {
                uint lo = origVa & 0x01FFFFFFu, lo2 = va & 0x01FFFFFFu;
                bool hit = (lo >= 0x00014998u && lo < 0x000149F0u) || (lo2 >= 0x00014998u && lo2 < 0x000149F0u) || (va >= 0x86000000u && va < 0x90000000u && _o32Pa != 0 && (va & 0x1FFFFFFFu) >= _o32Pa && (va & 0x1FFFFFFFu) < _o32Pa + 0x58u);
                if (hit) { _o32wN++; uint opa = 0xFFFFFFFFu; try { opa = bus.Translate(va, true); } catch { } if ((origVa & 0xFE000000u) == 0x06000000u && _o32Pa == 0 && opa != 0xFFFFFFFFu) _o32Pa = opa; BootLog.Write("[PcSample] o32W orig=0x" + origVa.ToString("X8") + " va=0x" + va.ToString("X8") + " pa=0x" + opa.ToString("X8") + " val=0x" + value.ToString("X8") + " pc=0x" + LastFetchPc.ToString("X8") + " insn=0x" + LastInsn.ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " step=" + _steps + " st=" + StkNames()); }
            }            if (!On || _nwN >= 30) return;
            if ((origVa & 0x01FFFFFFu) != 0x000DF384u && (va & 0x01FFFFFFu) != 0x000DF384u) { return; }
            uint pa = 0xFFFFFFFFu; try { pa = bus.Translate(va, true); } catch { }
            _nwN++; if (_slotPa == 0xFFFFFFFFu && origVa == 0x080DF384u) _slotPa = pa;
            BootLog.Write("[PcSample] slotW orig=0x" + origVa.ToString("X8") + " va=0x" + va.ToString("X8") + " pa=0x" + pa.ToString("X8") + " val=0x" + value.ToString("X8") + " step=" + _steps + " prevpc=0x" + _prevPc.ToString("X8") + " fpc=0x" + LastFetchPc.ToString("X8") + " pcAfter=0x" + LastPcAfterFetch.ToString("X8") + " insn=0x" + LastInsn.ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " alias=0x" + CeRomTocFiles.AliasProcPeek.ToString("X8") + " thr24=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x24u).ToString("X8") + " thr08=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x08u).ToString("X8") + " thr0C=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x0Cu).ToString("X8") + " host=" + (System.Environment.StackTrace.Contains("CeRomTocFiles") || System.Environment.StackTrace.Contains("HostHardDisk") ? "Y" : "n") + " st=" + StkNames());
        }
        static string StkNames() { var f = new System.Diagnostics.StackTrace(2, false).GetFrames(); var sb = new System.Text.StringBuilder(); for (int k = 0; k < f.Length && k < 9; k++) { var m = f[k].GetMethod(); sb.Append(m == null ? "?" : (m.DeclaringType == null ? "" : m.DeclaringType.Name) + "." + m.Name).Append('/'); } return sb.ToString(); }
        public static void NoteR(uint pa, uint va, uint val, uint origVa)
        {
            if (On && _hpN < 40 && (origVa & 0x01FFFFFCu) == 0x01FFFFA0u && _sbus != null && (pa != _hpPa || val != _hpVal))
            { _hpN++; _hpPa = pa; _hpVal = val; BootLog.Write("[PcSample] hheap orig=0x" + origVa.ToString("X8") + " va=0x" + va.ToString("X8") + " pa=0x" + pa.ToString("X8") + " val=0x" + val.ToString("X8") + " DAC4=0x" + P(_sbus, 0xFFFFDAC4u).ToString("X8") + " DAC0=0x" + P(_sbus, 0xFFFFDAC0u).ToString("X8") + " pc=0x" + _prevPc.ToString("X8") + " step=" + _steps); }
            if (!On || _nrN >= 20 || _steps < 96270000 || _steps > 96290000) return;
            if ((va & 0x01FFFFFFu) != 0x000DF384u) return;
            _nrN++; BootLog.Write("[PcSample] slotR va=0x" + va.ToString("X8") + " pa=0x" + pa.ToString("X8") + " val=0x" + val.ToString("X8") + " step=" + _steps);
        }        static void DbgProbe(MipsBus bus, uint[] r, uint pc)
        { _sbus = bus;
            if (_steps > 70000000 && _steps < 72030000 && _alN < 400 && r[31] == _prevPc + 8u && _prevPc >= 0x03E80000u && _prevPc < 0x03EB0000u && P(bus, 0xFFFFDAC0u) == 0x86F66AE0u) { _alN++; string s0 = "", s1 = ""; try { s0 = WStr(bus, r[4], 48); s1 = WStr(bus, r[5], 48); } catch { } BootLog.Write("[PcSample] al-tr " + _prevPc.ToString("X8") + "->" + pc.ToString("X8") + " a0=" + r[4].ToString("X8") + " a1=" + r[5].ToString("X8") + " a2=" + r[6].ToString("X8") + (s0.Length > 2 ? " s0=\"" + s0 + "\"" : "") + (s1.Length > 2 ? " s1=\"" + s1 + "\"" : "") + " step=" + _steps); }
            if (_bbN < 400 && pc != _prevPc + 4u && pc != _prevPc + 8u && ((pc >= 0x03DC1000u && pc < 0x03DC7000u) != (_prevPc >= 0x03DC1000u && _prevPc < 0x03DC7000u))) { _bbN++; string ws = ""; try { ws = WStr(bus, r[4], 40); } catch { } BootLog.Write("[PcSample] bb-x " + _prevPc.ToString("X8") + "->" + pc.ToString("X8") + " a0=" + r[4].ToString("X8") + " a1=" + r[5].ToString("X8") + " a2=" + r[6].ToString("X8") + " a3=" + r[7].ToString("X8") + " v0=" + r[2].ToString("X8") + " ra=" + r[31].ToString("X8") + " thr=" + P(bus, 0xFFFFDAC0u).ToString("X8") + (ws.Length > 1 ? " s=\"" + ws + "\"" : "") + " step=" + _steps); }
            if (_steps > 48040000 && _steps < 48070000 && _o32aN < 80 && P(bus, 0xFFFFDAC4u) == 0x803401E0u && (pc == 0x8001AE00u || pc == 0x8001AE08u || pc == 0x8001AD1Cu || pc == 0x8001AD24u || pc == 0x8001AD38u || pc == 0x8001AD40u || pc == 0x8001ACC4u || (r[31] == _prevPc + 8u && _prevPc >= 0x8001A800u && _prevPc < 0x8001B000u))) { _o32aN++; BootLog.Write("[PcSample] o32-ld pc=0x" + pc.ToString("X8") + " prev=0x" + _prevPc.ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " s4=0x" + r[20].ToString("X8") + " s6=0x" + r[22].ToString("X8") + " s7=0x" + r[23].ToString("X8") + " o32=" + (r[23] >= 0x80000000u ? W(bus, r[23], 7) : "") + " step=" + _steps); }
            if (_steps > 47800000 && _steps < 49000000 && _e14N < 120) { uint[] e14 = { 0x80019328u, 0x80019324u, 0x8001AD4Cu, 0x8001AD48u, 0x8001BBF4u, 0x8001BBF0u, 0x8001D564u, 0x8001D560u, 0x8001E914u, 0x8001E910u, 0x8001EC9Cu, 0x8001EC98u, 0x8001EE2Cu, 0x8001EE28u, 0x8001EE64u, 0x8001EE60u, 0x8001EF7Cu, 0x8001EF78u, 0x8001F180u, 0x8001F17Cu, 0x8001FF04u, 0x8001FF00u, 0x80020790u, 0x8002078Cu, 0x800207F8u, 0x800207F4u, 0x8002436Cu, 0x80024368u, 0x80025040u, 0x8002503Cu, 0x8002AA18u, 0x8002AA14u, 0x8002D7FCu, 0x8002D7F8u, 0x80031FC4u, 0x80031FC0u, 0x80032124u, 0x80032120u, 0x80034584u, 0x80034580u, 0x8003493Cu, 0x80034938u, 0x8003502Cu, 0x80035028u, 0x8003505Cu, 0x80035058u, 0x8003508Cu, 0x80035088u, 0x80035160u, 0x8003515Cu, 0x80035354u, 0x80035350u, 0x800355A8u, 0x800355A4u, 0x80037144u, 0x80037140u, 0x8003B650u, 0x8003B64Cu, 0x8003C2ECu, 0x8003C2E8u, 0x800451D4u, 0x800451D0u, 0x800451E4u, 0x800451E0u, 0x8004872Cu, 0x80048728u, 0x80048DD8u, 0x80048DD4u, 0x80049968u, 0x80049964u, 0x800499C4u, 0x800499C0u, 0x80049DECu, 0x80049DE8u, 0x80049EE0u, 0x80049EDCu, 0x8004A468u, 0x8004A464u, 0x8004A550u, 0x8004A54Cu, 0x8004A6DCu, 0x8004A6D8u, 0x8004B704u, 0x8004B700u, 0x8004B738u, 0x8004B734u, 0x8004B960u, 0x8004B95Cu, 0x8004BAB0u, 0x8004BAACu, 0x8004BB70u, 0x8004BB6Cu, 0x8004C054u, 0x8004C050u, 0x8004C298u, 0x8004C294u, 0x8004C6E8u, 0x8004C6E4u, 0x8004CBA8u, 0x8004CBA4u, 0x8004D0ACu, 0x8004D0A8u, 0x8004D0DCu, 0x8004D0D8u, 0x8004D6D8u, 0x8004D6D4u, 0x8004DAD4u, 0x8004DAD0u, 0x800563B0u, 0x800563ACu, 0x80016B9Cu, 0x80016B74u, 0x8001D8E4u, 0x8001DA58u, 0x8001D958u }; if (System.Array.IndexOf(e14, pc) >= 0) { _e14N++; BootLog.Write("[PcSample] cp-dev pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " s5=0x" + r[21].ToString("X8") + " thr=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " step=" + _steps); } }
            if (pc == 0x03E83918u && _apiN < 20) { _apiN++; BootLog.Write("[PcSample] fsd-IsAPIReady(0x16) v0=" + r[2] + " kinfoCB48=0x" + P(bus, 0xFFFFCB48u).ToString("X8") + " step=" + _steps); }
            if (pc == 0x03E88E00u && _wfN < 40) { _wfN++; BootLog.Write("[PcSample] fsd-wfmo ret v0=0x" + r[2].ToString("X8") + " kinfoCB48=0x" + P(bus, 0xFFFFCB48u).ToString("X8") + " step=" + _steps); }
            if (pc == 0x03E88FB4u && _smThr == 0) { _smThr = P(bus, 0xFFFFDAC0u); BootLog.Write("[PcSample] sm-start thr=0x" + _smThr.ToString("X8") + " v0=0x" + r[2].ToString("X8") + " s2=0x" + r[18].ToString("X8") + " step=" + _steps); }
            if (_smThr != 0 && _smN < 600 && ((r[31] == _prevPc + 8u && _prevPc >= 0x03E80000u && _prevPc < 0x03EB0000u) || (pc >= 0xF0000000u && r[31] >= 0x03E80000u && r[31] < 0x03EB0000u)) && pc != _prevPc + 4u && !(pc >= 0x80000000u && pc < 0xF0000000u) && P(bus, 0xFFFFDAC0u) == _smThr) { _smN++; string ws = ""; try { ws = WStr(bus, r[4], 40); } catch { } BootLog.Write("[PcSample] sm-tr " + _prevPc.ToString("X8") + "->" + pc.ToString("X8") + " a0=" + r[4].ToString("X8") + " a1=" + r[5].ToString("X8") + " a2=" + r[6].ToString("X8") + " a3=" + r[7].ToString("X8") + " ra=" + r[31].ToString("X8") + (ws.Length > 1 ? " s=\"" + ws + "\"" : "") + " step=" + _steps); }
            if (_steps > 40000000 && _dwN < 12) { uint wa = P(bus, 0x86F66AE0u) & 0x1000u, wb = P(bus, 0x86F66800u) & 0x1000u; uint cur = wa | (wb << 1); if (cur != _dwLast) { _dwN++; BootLog.Write("[PcSample] dbgwait-bit fsMain=" + (wa != 0) + " fs2=" + (wb != 0) + " pc=0x" + pc.ToString("X8") + " prev=0x" + _prevPc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " w0=" + P(bus, 0x86F66AE0u).ToString("X8") + "/" + P(bus, 0x86F66800u).ToString("X8") + " step=" + _steps); _dwLast = cur; } }
            if (pc == 0x80018E94u && r[20] == 0x03C4E464u && _ipTr == 0) { _ipTr = 1; BootLog.Write("[PcSample] ip-start o32@86F008E4=" + W(bus, 0x86F008E4u, 84) + " mod@86F1AA00=" + W(bus, 0x86F1AA00u, 128) + " step=" + _steps); }
            if (_ipTr > 0 && _ipTr < 700 && pc != _prevPc + 4u && pc != _prevPc + 8u && !(pc >= 0x80055000u && pc < 0x8005A000u) && !(_prevPc >= 0x80055000u && _prevPc < 0x8005A000u) && P(bus, 0xFFFFDAC0u) == 0x86F68DA8u) { _ipTr++; BootLog.Write("[PcSample] ip-tr " + _prevPc.ToString("X8") + "->" + pc.ToString("X8") + (r[31] == _prevPc + 8u ? " CALL" : "") + " a0=" + r[4].ToString("X8") + " a1=" + r[5].ToString("X8") + " a2=" + r[6].ToString("X8") + " a3=" + r[7].ToString("X8") + " v0=" + r[2].ToString("X8") + " v1=" + r[3].ToString("X8") + " ra=" + r[31].ToString("X8") + " step=" + _steps); if (pc == 0x8004294Cu) _ipTr = 700; }
            if (_steps > 80700000 && _sipU < 12 && pc < 0x80000000u && P(bus, 0xFFFFDAC0u) == 0x86F68DA8u && (pc & 0xFFF00000u) != (_sipLast & 0xFFF00000u)) { _sipU++; _sipLast = pc; BootLog.Write("[PcSample] sip-user pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " step=" + _steps); }
            if (pc == 0x000124D8u && P(bus, 0xFFFFDAC4u) == 0x803401E0u && !_sipEntry) { _sipEntry = true; BootLog.Write("[PcSample] sip-ENTRY reached pc=0x000124D8 a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " step=" + _steps); }
            if (_steps >= 100000000 && !_waitDump) { _waitDump = true; BootLog.Write("[PcSample] wait-dump fsThr=" + W(bus, 0x86F66AE0u, 56) + " gwThr=" + W(bus, 0x86F0C570u, 56) + " sipThr=" + W(bus, 0x86F68DA8u, 56) + " obj86F68800=" + W(bus, 0x86F68800u, 16) + " obj86F66800=" + W(bus, 0x86F66800u, 16) + " fsStk=" + W(bus, 0x0407FD80u, 128) + " pnpStk=" + W(bus, 0x040CFA00u, 96) + " gwSp=0x" + r[29].ToString("X8") + " pc=0x" + pc.ToString("X8") + " cur=0x" + P(bus, 0xFFFFDAC0u).ToString("X8")); }
            if (pc == 0x800308E0u && _steps > 99000000 && _gw8Stk < 3) { _gw8Stk++; uint sp = r[29]; BootLog.Write("[PcSample] gw8E0-stk a0=0x" + r[4].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + sp.ToString("X8") + " s0-s7=" + r[16].ToString("X8") + " " + r[17].ToString("X8") + " " + r[18].ToString("X8") + " " + r[19].ToString("X8") + " " + r[20].ToString("X8") + " " + r[21].ToString("X8") + " " + r[22].ToString("X8") + " " + r[23].ToString("X8") + " stk=" + W(bus, sp, 48) + " step=" + _steps); }
            if (pc == 0x8001634Cu && _o32N < 40 && _steps > 80000000) { _o32N++; BootLog.Write("[PcSample] o32chk a0=0x" + r[4].ToString("X8") + " vsize(t6)=0x" + r[14].ToString("X8") + " real(v0)=0x" + r[2].ToString("X8") + " page(a2)=0x" + r[6].ToString("X8") + " left(a1)=" + r[5] + " rec=" + W(bus, r[4], 7) + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " step=" + _steps); }
            if (pc == 0x80018E94u && r[20] == 0x00012804u && _trN == 0) _trN = 1;
            if (_trN > 0 && _trN < 4000 && pc != _prevPc + 4u && pc != _prevPc + 8u && !(pc >= 0x80055000u && pc < 0x8005A000u) && !(_prevPc >= 0x80055000u && _prevPc < 0x8005A000u) && pc != 0x8002C070u)
            {
                uint thr = P(bus, 0xFFFFDAC0u);
                bool sip = thr == 0x86F68DA8u;
                bool gw = thr == 0x86F0C570u && r[31] == _prevPc + 8u && _gwTr < 600;
                if (sip || gw)
                {
                    if (sip) _trN++; else _gwTr++;
                    BootLog.Write("[PcSample] " + (sip ? "pf-tr " : "gw-tr ") + _prevPc.ToString("X8") + "->" + pc.ToString("X8") + (r[31] == _prevPc + 8u ? " CALL" : "") + " a0=" + r[4].ToString("X8") + " a1=" + r[5].ToString("X8") + " a2=" + r[6].ToString("X8") + " a3=" + r[7].ToString("X8") + " v0=" + r[2].ToString("X8") + " ra=" + r[31].ToString("X8") + " step=" + _steps);
                    if (sip && pc == 0x8004294Cu) { _trN = 4000; BootLog.Write("[PcSample] at-print fsThr=" + W(bus, 0x86F66AE0u, 48) + " gwThr=" + W(bus, 0x86F0C570u, 48) + " cs8033FEC0=" + W(bus, 0x8033FEC0u, 8) + " sec2=" + W(bus, 0x80AD6F5Cu, 8) + " attach=" + W(bus, 0x8F148000u, 48)); }
                }
            }
            if (pc == 0x80018E94u && r[20] == 0x00012804u && !_sipDump) { _sipDump = true; BootLog.Write("[PcSample] sip-dump o32data0=" + W(bus, 0x80AD626Cu, 8) + " o32data1=" + W(bus, 0x80AD6E84u, 4) + " o32data2=" + W(bus, 0x80AD6F5Cu, 4) + " e32@80E9ADF0=" + W(bus, 0x80E9ADF0u, 26) + " o32@80E9AE60=" + W(bus, 0x80E9AE60u, 18) + " toc@8134E1B8=" + W(bus, 0x8134E1B8u, 8) + " attach@8F148000=" + W(bus, 0x8F148000u, 64) + " step=" + _steps); }
            if (pc == 0x80018D78u && _steps > 80000000 && _ldN < 12) { _ldN++; BootLog.Write("[PcSample] ld-entry a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " alias=0x" + CeRomTocFiles.AliasProcPeek.ToString("X8") + " e32=" + W(bus, r[4], 16) + " proc-60=" + W(bus, P(bus, 0xFFFFDAC4u), 40) + " step=" + _steps); }
            if (pc == 0x80018E94u && _steps > 80000000 && _ld2N < 12) { _ld2N++; uint s4 = r[20]; BootLog.Write("[PcSample] ld-e94 s4=0x" + s4.ToString("X8") + " *s4=0x" + P(bus, s4).ToString("X8") + " s5=0x" + r[21].ToString("X8") + " s2=0x" + r[18].ToString("X8") + " fp=0x" + r[30].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " alias=0x" + CeRomTocFiles.AliasProcPeek.ToString("X8") + " slot04=0x" + P(bus, 0x04000000u | (s4 & 0x01FFFFFFu)).ToString("X8") + " slot06=0x" + P(bus, 0x06000000u | (s4 & 0x01FFFFFFu)).ToString("X8") + " slot08=0x" + P(bus, 0x08000000u | (s4 & 0x01FFFFFFu)).ToString("X8") + " slot0A=0x" + P(bus, 0x0A000000u | (s4 & 0x01FFFFFFu)).ToString("X8") + " slot0C=0x" + P(bus, 0x0C000000u | (s4 & 0x01FFFFFFu)).ToString("X8") + " step=" + _steps); }
            if (pc == 0x800308E0u && _steps > 95000000 && _hwN < 8) { _hwN++; uint h = r[4]; BootLog.Write("[PcSample] h8E0 a0=0x" + h.ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0u).ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " step=" + _steps); }
            if (pc == 0x80030934u && _steps > 95000000 && _hvN < 8) { _hvN++; uint o = r[2]; BootLog.Write("[PcSample] h934 v0=0x" + o.ToString("X8") + " obj=" + P(bus, o).ToString("X8") + " " + P(bus, o + 4).ToString("X8") + " " + P(bus, o + 8).ToString("X8") + " " + P(bus, o + 12).ToString("X8") + " " + P(bus, o + 16).ToString("X8") + " " + P(bus, o + 20).ToString("X8") + " " + P(bus, o + 24).ToString("X8") + " " + P(bus, o + 28).ToString("X8") + " step=" + _steps); }
            if (_steps > 94000000 && _steps < 96300000 && _rwN < 60)
            {
                uint w = P(bus, 0x080DF384u);
                if (w != _rwLast) { _rwN++; BootLog.Write("[PcSample] ra-slot 0x080DF384 0x" + _rwLast.ToString("X8") + "->0x" + w.ToString("X8") + " pc=0x" + pc.ToString("X8") + " prev=0x" + _prevPc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " cur=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); _rwLast = w; }
            }
            if (pc != _prevPc + 4u) { _ring[_ri & 127] = _prevPc; _ring2[_ri & 127] = pc; _ringRa[_ri & 127] = r[31]; _ri++; }
            _prevPc = pc;
            if ((pc == 0x80000000u || pc == 0x80000180u) && _ringDumps < 2 && bus.Cp0Snapshot().Contains("EPC=0x0037D220"))
            {
                _ringDumps++;
                var sb = new System.Text.StringBuilder("[PcSample] trail-to-0x0037D220 " + bus.Cp0Snapshot() + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " t9=0x" + r[25].ToString("X8") + " cur=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps + " jumps(from->to,ra):");
                for (int k = 128; k >= 1; k--) { int ix = (_ri - k) & 127; sb.Append(" " + _ring[ix].ToString("X8") + "->" + _ring2[ix].ToString("X8") + "," + _ringRa[ix].ToString("X8")); }
                BootLog.Write(sb.ToString());
            }
            if (_steps > 80600000)
            {
                uint cur = P(bus, 0xFFFFDAC0);
                if ((pc == 0x80000180u || pc == 0x80000000u) && cur == 0x86F0C570u && _gwN < 40) { _gwN++; BootLog.Write("[PcSample] gwes-exn vec=0x" + pc.ToString("X8") + " " + bus.Cp0Snapshot() + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " step=" + _steps); }
                if (pc == 0x800466D0u) { _uwN++; if (_uwN <= 12 || _uwN % 5000 == 0) BootLog.Write("[PcSample] uw466D0 #" + _uwN + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " cur=0x" + cur.ToString("X8") + " step=" + _steps); }
                if (pc == 0x80047C90u || pc == 0x80047CB8u) { _uw2++; if (_uw2 <= 16 || _uw2 % 5000 == 0) BootLog.Write("[PcSample] uw47C #" + _uw2 + " pc=0x" + pc.ToString("X8") + " s4=0x" + r[20].ToString("X8") + " s6=0x" + r[22].ToString("X8") + " sp184=0x" + P(bus, r[29] + 0x184).ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " step=" + _steps); }
                if (cur == 0x86F0C570u && (_steps & 0x3FFFFF) == 0x200000 && _gwStk < 10) { _gwStk++; BootLog.Write("[PcSample] gwes-stk pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " words=" + Words(bus, r[29], 96) + " step=" + _steps); }
            }
            if (pc >= 0xFFFFF000u) { _apiCnt.TryGetValue(pc, out int ac); _apiCnt[pc] = ac + 1; _apiRa[pc] = r[31]; }
            if (pc == 0x80000180u) { uint ex = (System.Convert.ToUInt32(bus.Cp0Snapshot().Substring(bus.Cp0Snapshot().IndexOf("Cause=0x") + 8, 8), 16) >> 2) & 0x1F; if (ex != 0) { string s = bus.Cp0Snapshot(); uint bva = System.Convert.ToUInt32(s.Substring(s.IndexOf("BadVA=0x") + 8, 8), 16); uint key = (ex << 27) ^ (bva & 0x07FFFFFF); _exWin.TryGetValue(key, out int ec); _exWin[key] = ec + 1; } }
            if ((_steps & 0x3FFFFF) == 0 && _steps > 60000000 && _apiDump < 60)
            {
                _apiDump++;
                var sb = new System.Text.StringBuilder("[PcSample] api-hist step=" + _steps + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " :");
                foreach (var kv in System.Linq.Enumerable.Take(System.Linq.Enumerable.OrderByDescending(_apiCnt, x => x.Value), 6)) sb.Append(" api" + ((int)((0xFFFFFC02u - kv.Key) / 4)).ToString() + "(pc=0x" + kv.Key.ToString("X8") + ")x" + kv.Value + " ra=0x" + _apiRa[kv.Key].ToString("X8") + ";");
                sb.Append(" exn:");
                foreach (var kv in System.Linq.Enumerable.Take(System.Linq.Enumerable.OrderByDescending(_exWin, x => x.Value), 4)) sb.Append(" code" + (kv.Key >> 27) + "/bva~0x" + (kv.Key & 0x07FFFFFF).ToString("X") + "x" + kv.Value + ";");
                BootLog.Write(sb.ToString()); _apiCnt.Clear(); _exWin.Clear();
            }
            if (pc == 0x03F65C1Cu) { _shaN++; if (_shaN <= 6 || _shaN % 2000 == 0) BootLog.Write("[PcSample] sha-caller #" + _shaN + " ra=0x" + r[31].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " stk=" + Words(bus, r[29], 12) + " step=" + _steps); }
            if (pc == 0x80034D2Cu && _cpN < 30) { _cpN++; BootLog.Write("[PcSample] CreateProcess #" + _cpN + " name=\"" + WStr(bus, r[4], 60) + "\" args=\"" + WStr(bus, r[5], 60) + "\" ra=0x" + r[31].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); }
            if ((pc == 0x8003EB10u || pc == 0x8003EB48u || pc == 0x8003EB74u || pc == 0x8003EB80u) && _raN < 40) { _raN++; BootLog.Write("[PcSample] runapps pc=0x" + pc.ToString("X8") + " v0=0x" + r[2].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " step=" + _steps); }
            if ((pc == 0x8003CF0Cu || pc == 0x8003CF00u) && _hcN < 8) { _hcN++; BootLog.Write("[PcSample] hcheck pc=0x" + pc.ToString("X8") + " s6=0x" + r[22].ToString("X8") + " *s6=0x" + P(bus, r[22]).ToString("X8") + " a0=0x" + r[4].ToString("X8") + " *a0=0x" + P(bus, r[4]).ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " stk=" + Words(bus, r[29], 16) + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); }
            if (pc == 0x80041B88u || pc == 0x8004231Cu && _poolN < 6) { if (pc == 0x8004231Cu) _poolN++; BootLog.Write("[PcSample] pool pc=0x" + pc.ToString("X8") + " v1=0x" + r[3].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " A30..A3C=" + Words(bus, 0x80320A30u, 4) + " ra=0x" + r[31].ToString("X8") + " step=" + _steps); }
            if ((_steps & 0x3FF) == 0) { uint pw = P(bus, 0x80320A34u); if (pw != _poolW && _poolC < 40) { _poolC++; BootLog.Write("[PcSample] pool-watch 0x80320A34 0x" + _poolW.ToString("X8") + "->0x" + pw.ToString("X8") + " pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " A30..A3C=" + Words(bus, 0x80320A30u, 4) + " step=" + _steps); _poolW = pw; } }
            if (pc == 0x8004294Cu)
            {
                uint f = r[5]; _fmt.TryGetValue(f, out int n); _fmt[f] = ++n;
                if ((n == 1 || n == 2 || n == 3 || n == 10 || n == 100 || n == 1000 || n == 10000) && _dbgN < 200) { _dbgN++; BootLog.Write("[PcSample] dbgfmt #" + n + " fmt@0x" + f.ToString("X8") + " \"" + WStr(bus, f, 120) + "\" a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " va=" + Words(bus, r[6], 10) + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); }
            }
            else if ((pc == 0x800659D0u || pc == 0x03F78B5Cu) && _chain < 24)
            { _chain++; BootLog.Write("[PcSample] chain pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " stk=" + Words(bus, r[29], 24) + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); }
            else if (pc == 0x80000180u && _exnN < 60 && ((bus.Cp0Snapshot().Contains("Cause=0x") ? System.Convert.ToUInt32(bus.Cp0Snapshot().Substring(bus.Cp0Snapshot().IndexOf("Cause=0x") + 8, 8), 16) : 0u) & 0x7Cu) != 0)
            {
                uint epc = bus.Cp0Snapshot() is string s && s.Contains("EPC=0x") ? System.Convert.ToUInt32(s.Substring(s.IndexOf("EPC=0x") + 6, 8), 16) : 0; _exnEpc.TryGetValue(epc, out int n); _exnEpc[epc] = ++n;
                if (n == 1 || n == 100 || n == 10000) { _exnN++; BootLog.Write("[PcSample] exn #" + n + " " + bus.Cp0Snapshot() + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " DAC0=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps); }
            }
            if ((_steps & 0x3FFFFF) == 0 && _steps > 36000000 && _thrN < 12)
            {
                _thrN++;
                foreach (uint th in new uint[] { 0x86F7E028u, 0x86F66AE0u, 0x86F66800u, 0x86F68800u, 0x86F68DA8u })
                    BootLog.Write("[PcSample] thr 0x" + th.ToString("X8") + " +00..+5C=" + Words(bus, th, 24) + " cur=0x" + P(bus, 0xFFFFDAC0).ToString("X8") + " step=" + _steps);
            }
        }
        public static void Step(MipsBus bus, uint[] r, uint pc)
        {
            ImStep(bus, r, pc);
            if (!On || r == null || r.Length < 32) return;
            _steps++;
            uint pg = pc >> 12;
            _pages.TryGetValue(pg, out int c); _pages[pg] = c + 1; _pcs.TryGetValue(pc, out int pcn); _pcs[pc] = pcn + 1; IoProbe(bus, r, pc); DbgProbe(bus, r, pc);
            for (int i = 0; i < Marks.Length; i++)
                if (pc == Marks[i] && (_hit.Add(pc) || ((pc == 0x80030604u || pc == 0x80030664u || pc == 0x80030688u || pc == 0x800306A4u || pc == 0x800306D8u || pc == 0x8002FF98u) && _ckt++ < 60)))
                    BootLog.Write("[PcSample] mark pc=0x" + pc.ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " tmplD890=0x" + P(bus, 0xFFFFD890).ToString("X8") + " " + bus.Cp0Snapshot() + " step=" + _steps);
            // GetProcAddress-by-ordinal (m[144] -> 0x8001F7BC) on the kernel's
            // coredll handle: TBFf = ordinal 19 (0x13). DoCreateThread 0x8002FF98
            // fails (ERROR_NOT_ENOUGH_MEMORY) when *(0x8033FF00) is 0.
            if ((pc == 0x8001F7E4u || pc == 0x800201B4u || pc == 0x8001F794u || pc == 0x8001F7F8u) && _gpa++ < 60)
            {
                uint mod = r[4];
                BootLog.Write("[PcSample] gpa pc=0x" + pc.ToString("X8") + " v0=0x" + r[2].ToString("X8") + " v1=0x" + r[3].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " a2=0x" + r[6].ToString("X8") + " a3=0x" + r[7].ToString("X8") + " ra=0x" + r[31].ToString("X8")
                    + (pc == 0x8001F7E4u ? " exp+10=0x" + P(bus, r[2] + 0x10).ToString("X8") + " exp+14=0x" + P(bus, r[2] + 0x14).ToString("X8") + " exp+1C=0x" + P(bus, r[2] + 0x1C).ToString("X8") + " eat[a1-base]=0x" + P(bus, r[7] + 4 * (r[5] - P(bus, r[2] + 0x10))).ToString("X8") : "")
                    + " FF00=0x" + P(bus, 0x8033FF00u).ToString("X8") + " step=" + _steps);
            }            if (pc >= 0xFFFFF000u && _api++ < 24)
                BootLog.Write("[PcSample] api-fetch pc=0x" + pc.ToString("X8") + " api=" + ((0xFFFFFC02u - pc) / 4) + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " " + bus.Cp0Snapshot() + " step=" + _steps);
            if (pc == 0x80000180u && _exn++ < 40)
                BootLog.Write("[PcSample] exn-vec ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " epc=0x" + bus.PeekEpc().ToString("X8") + " " + bus.Cp0Snapshot() + " step=" + _steps);
            if ((_steps & 0xFFF) == 0)
            {
                uint k0 = P(bus, 0xFFFFDAC0), k1 = P(bus, 0xFFFFDAC4);
                if (!_kInit) { _kInit = true; _k0 = k0; _k1 = k1; }
                else if ((k0 != _k0 || k1 != _k1) && _sw_n < 80)
                {
                    _sw_n++;
                    BootLog.Write("[PcSample] kdata-switch DAC0 0x" + _k0.ToString("X8") + "->0x" + k0.ToString("X8") + " DAC4 0x" + _k1.ToString("X8") + "->0x" + k1.ToString("X8") + " pc=0x" + pc.ToString("X8") + " step=" + _steps);
                    _k0 = k0; _k1 = k1;
                }
                long ms = _sw.ElapsedMilliseconds;
                if (ms >= _next)
                {
                    _next = ms + Sec * 1000L;
                    var top = new List<KeyValuePair<uint, int>>(_pages);
                    top.Sort((a, b) => b.Value.CompareTo(a.Value));
                    string hot = "";
                    for (int i = 0; i < top.Count && i < 4; i++) hot += " 0x" + (top[i].Key << 12).ToString("X8") + ":" + top[i].Value;
                    _pages.Clear(); var tp = new List<KeyValuePair<uint, int>>(_pcs); tp.Sort((a, b) => b.Value.CompareTo(a.Value)); for (int i = 0; i < tp.Count && i < 10; i++) hot += " pc0x" + tp[i].Key.ToString("X8") + ":" + tp[i].Value; _pcs.Clear();
                    BootLog.Write("[PcSample] t=" + (ms / 1000) + "s step=" + _steps + " pc=0x" + pc.ToString("X8") + " sp=0x" + r[29].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " DAC0=0x" + k0.ToString("X8") + " DAC4=0x" + k1.ToString("X8") + " C800=0x" + P(bus, 0xFFFFC800).ToString("X8") + " " + bus.Cp0Snapshot() + " flag2C4C=0x" + P(bus, 0x80342C4C).ToString("X8") + " hot:" + hot);
                }
            }
        }
    }
}