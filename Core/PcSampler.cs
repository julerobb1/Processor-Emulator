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
        static long _next, _steps;
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
        public static uint LastInsn, LastFetchPc, LastPcAfterFetch; static int _nwN, _nrN; static uint _slotPa = 0xFFFFFFFFu;
        public static void NoteW(MipsBus bus, uint origVa, uint va, uint value)
        {
            if (!On || _nwN >= 30) return;
            if ((origVa & 0x01FFFFFFu) != 0x000DF384u && (va & 0x01FFFFFFu) != 0x000DF384u) { return; }
            uint pa = 0xFFFFFFFFu; try { pa = bus.Translate(va, true); } catch { }
            _nwN++; if (_slotPa == 0xFFFFFFFFu && origVa == 0x080DF384u) _slotPa = pa;
            BootLog.Write("[PcSample] slotW orig=0x" + origVa.ToString("X8") + " va=0x" + va.ToString("X8") + " pa=0x" + pa.ToString("X8") + " val=0x" + value.ToString("X8") + " step=" + _steps + " prevpc=0x" + _prevPc.ToString("X8") + " fpc=0x" + LastFetchPc.ToString("X8") + " pcAfter=0x" + LastPcAfterFetch.ToString("X8") + " insn=0x" + LastInsn.ToString("X8") + " DAC4=0x" + P(bus, 0xFFFFDAC4u).ToString("X8") + " alias=0x" + CeRomTocFiles.AliasProcPeek.ToString("X8") + " thr24=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x24u).ToString("X8") + " thr08=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x08u).ToString("X8") + " thr0C=0x" + P(bus, P(bus, 0xFFFFDAC0u) + 0x0Cu).ToString("X8") + " host=" + (System.Environment.StackTrace.Contains("CeRomTocFiles") || System.Environment.StackTrace.Contains("HostHardDisk") ? "Y" : "n") + " st=" + StkNames());
        }
        static string StkNames() { var f = new System.Diagnostics.StackTrace(2, false).GetFrames(); var sb = new System.Text.StringBuilder(); for (int k = 0; k < f.Length && k < 9; k++) { var m = f[k].GetMethod(); sb.Append(m == null ? "?" : (m.DeclaringType == null ? "" : m.DeclaringType.Name) + "." + m.Name).Append('/'); } return sb.ToString(); }
        public static void NoteR(uint pa, uint va, uint val)
        {
            if (!On || _nrN >= 20 || _steps < 96270000 || _steps > 96290000) return;
            if ((va & 0x01FFFFFFu) != 0x000DF384u) return;
            _nrN++; BootLog.Write("[PcSample] slotR va=0x" + va.ToString("X8") + " pa=0x" + pa.ToString("X8") + " val=0x" + val.ToString("X8") + " step=" + _steps);
        }        static void DbgProbe(MipsBus bus, uint[] r, uint pc)
        {
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