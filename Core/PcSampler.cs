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
        static readonly Dictionary<uint, int> _pages = new Dictionary<uint, int>();
        static uint _k0, _k1; static bool _kInit; static int _sw_n;
        static readonly HashSet<uint> _hit = new HashSet<uint>();
        static readonly uint[] Marks = { 0x8003EABC, 0x80034D2C, 0x8003EB48, 0x8003EB74, 0x80030604, 0x8003EB80, 0x800143E8, 0x80014408, 0x80014440, 0x80054F8C, 0x80020B98, 0x80055F90, 0x8005688C, 0x80015B64, 0x80014424, 0x80014428, 0x80014438, 0x800151E0, 0x8001728C, 0x80023500 };

        static uint P(MipsBus bus, uint va) { uint w; return CeRomTocFiles.PeekWordForSampler(bus, va, out w) ? w : 0xDEADDEADu; }

        // EXTRAROM_IM_RESTORE=1 (HOST-ASSISTED, firmware-computed value).
        // Kernel start 0x80014408 takes IM = *(0xFFFFD890) & 0xFF00 (set by
        // OEMInit HookInterrupt: hw0 PIC 0x400, hw5 timer 0x8000), clears it,
        // and restores template |= IM at 0x80014440 before first schedule.
        // In the emulator 0x80014440 never runs, so INTERRUPTS_ON 0x80014F1C
        // writes Status=1 with IM=0 and OEMIdle 0x80059E98 spins forever with
        // IP7/IP2 pending. Latch IM at 14408; if 14440 was never executed,
        // OR it back into the template once at the first INTERRUPTS_ON.
        static readonly bool ImOn = Environment.GetEnvironmentVariable("EXTRAROM_IM_RESTORE") == "1";
        static uint _imLatch; static bool _imSeen14440, _imDone;
        static void ImStep(MipsBus bus, uint pc)
        {
            if (!ImOn || _imDone) return;
            if (pc == 0x80014408u) { uint t = P(bus, 0xFFFFD890u); if (t != 0xDEADDEADu) _imLatch = t & 0xFF00u; return; }
            if (pc == 0x80014440u) { _imSeen14440 = true; _imDone = true; BootLog.Write("[ImRestore] firmware restored IM itself at 0x80014440; no host action"); return; }
            if (pc != 0x80014F1Cu || _imLatch == 0 || _imSeen14440) return;
            uint tmpl = P(bus, 0xFFFFD890u);
            if (tmpl == 0xDEADDEADu || (tmpl & 0xFF00u) != 0) return;
            try { bus.Write32(0xFFFFD890u, tmpl | _imLatch); } catch { return; }
            _imDone = true;
            BootLog.Write("[ImRestore] HOST-ASSISTED template 0xFFFFD890 0x" + tmpl.ToString("X8") + " -> 0x" + (tmpl | _imLatch).ToString("X8") + " (IM latched at 0x80014408 from OEMInit HookInterrupt; 0x80014440 restore never ran) " + bus.Cp0Snapshot());
        }
        public static void Step(MipsBus bus, uint[] r, uint pc)
        {
            ImStep(bus, pc);
            if (!On || r == null || r.Length < 32) return;
            _steps++;
            uint pg = pc >> 12;
            _pages.TryGetValue(pg, out int c); _pages[pg] = c + 1;
            for (int i = 0; i < Marks.Length; i++)
                if (pc == Marks[i] && _hit.Add(pc))
                    BootLog.Write("[PcSample] mark pc=0x" + pc.ToString("X8") + " a0=0x" + r[4].ToString("X8") + " a1=0x" + r[5].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " tmplD890=0x" + P(bus, 0xFFFFD890).ToString("X8") + " " + bus.Cp0Snapshot() + " step=" + _steps);
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
                    _pages.Clear();
                    BootLog.Write("[PcSample] t=" + (ms / 1000) + "s step=" + _steps + " pc=0x" + pc.ToString("X8") + " sp=0x" + r[29].ToString("X8") + " ra=0x" + r[31].ToString("X8") + " v0=0x" + r[2].ToString("X8") + " a0=0x" + r[4].ToString("X8") + " DAC0=0x" + k0.ToString("X8") + " DAC4=0x" + k1.ToString("X8") + " C800=0x" + P(bus, 0xFFFFC800).ToString("X8") + " " + bus.Cp0Snapshot() + " flag2C4C=0x" + P(bus, 0x80342C4C).ToString("X8") + " hot:" + hot);
                }
            }
        }
    }
}