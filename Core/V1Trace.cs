using System;
using ProcessorEmulator.Emulation;
using ProcessorEmulator.Core;

namespace ProcessorEmulator
{
    // v1 provenance trace for ExtraROM GetProc / iat-stub path.
    // Always-on observe (capped). Opt-in PE_IATSTUB_KEEPV1=1 makes the
    // iat-stub hooks (0x8001F7BC / 0x8001F7D0) return to $ra keeping
    // entry $v1. That redirect is HOST-ASSISTED, not firmware behavior.
    internal static class V1Trace
    {
        const uint WinLo = 0x80037178, WinHi = 0x80037188;
        const uint StubA = 0x8001F7BC, StubB = 0x8001F7D0;
        const uint JalGp1 = 0x8003FB0C, JalGp2 = 0x8003FB8C;
        const int Max = 200;
        static int _n;
        static uint _lastV1, _lastPc, _gpEntry;
        static bool _init;
        static int _redirects;
        static readonly bool KeepV1 =
            Environment.GetEnvironmentVariable("PE_IATSTUB_KEEPV1") == "1";

        static readonly System.Collections.Generic.HashSet<uint> _seen = new System.Collections.Generic.HashSet<uint>();
        static int _t;
        static void LogT(string s)
        {
            if (_t >= 400) return;
            _t++;
            BootLog.Write("[V1Trace] " + s);
        }
        static void Log(string s)
        {
            if (_n >= Max) return;
            _n++;
            BootLog.Write("[V1Trace] " + s);
        }

        static string R(uint[] r, uint pc) =>
            "pc=0x" + pc.ToString("X8") + " v0=0x" + r[2].ToString("X") +
            " v1=0x" + r[3].ToString("X") + " a0=0x" + r[4].ToString("X") +
            " a1=0x" + r[5].ToString("X") + " ra=0x" + r[31].ToString("X8") +
            " sp=0x" + r[29].ToString("X8");

        // Call ring: last guest jal/jalr sites + last non-sequential PC hops.
        struct CallRec { public uint Src, Tgt, Ra, Sp, A0, A1, A2, A3; public bool Jalr; }
        static readonly CallRec[] _cr = new CallRec[24];
        static int _crN;
        static readonly uint[] _hopFrom = new uint[24], _hopTo = new uint[24];
        static int _hopN;
        static uint _prevPc;
        static int _ringDumps;
        static void Ring(MipsBus bus, uint[] r, uint pc)
        {
            if (_prevPc != 0 && pc != _prevPc + 4 && pc != _prevPc + 8)
            { _hopFrom[_hopN % 24] = _prevPc; _hopTo[_hopN % 24] = pc; _hopN++; }
            _prevPc = pc;
            if (bus != null)
            {
                uint w = 0;
                try { w = bus.Read32(pc); } catch { }
                bool jal = (w >> 26) == 3;
                bool jalr = (w >> 26) == 0 && (w & 0x3F) == 9;
                if (jal || jalr)
                {
                    uint tgt = jal ? (((pc + 4) & 0xF0000000u) | ((w & 0x03FFFFFFu) << 2)) : r[(int)((w >> 21) & 31)];
                    _cr[_crN % 24] = new CallRec { Src = pc, Tgt = tgt, Ra = r[31], Sp = r[29], A0 = r[4], A1 = r[5], A2 = r[6], A3 = r[7], Jalr = jalr };
                    _crN++;
                }
            }
            if ((pc == 0x8003FA34 || pc == 0x8004266C || pc == 0x800426E0) && r[29] >= 0x90000000u && _ringDumps < 4)
            {
                _ringDumps++;
                BootLog.Write("[CallRing] at pc=0x" + pc.ToString("X8") + " ra=0x" + r[31].ToString("X8") + " sp=0x" + r[29].ToString("X8") + " calls=" + _crN + " hops=" + _hopN);
                int s = Math.Max(0, _crN - 24);
                for (int i = s; i < _crN; i++)
                {
                    var c = _cr[i % 24];
                    BootLog.Write("[CallRing] " + (c.Jalr ? "jalr" : "jal ") + " src=0x" + c.Src.ToString("X8") + " tgt=0x" + c.Tgt.ToString("X8") + " ra=0x" + c.Ra.ToString("X8") + " sp=0x" + c.Sp.ToString("X8") + " a0=0x" + c.A0.ToString("X") + " a1=0x" + c.A1.ToString("X") + " a2=0x" + c.A2.ToString("X") + " a3=0x" + c.A3.ToString("X"));
                }
                s = Math.Max(0, _hopN - 24);
                for (int i = s; i < _hopN; i++)
                    BootLog.Write("[CallRing] hop 0x" + _hopFrom[i % 24].ToString("X8") + " -> 0x" + _hopTo[i % 24].ToString("X8"));
            }
        }

        public static void Step(MipsBus bus, uint[] r, ref uint pc)
        {
            if (r == null || r.Length < 32) return;
            Ring(bus, r, pc);
            uint v1 = r[3];
            if (!_init) { _init = true; Log("armed keepv1=" + (KeepV1 ? "1" : "0")); }
            else if (v1 != _lastV1 && v1 == 1 && _seen.Add(_lastPc))
                Log("v1->1 after pc=0x" + _lastPc.ToString("X8") + " was=0x" + _lastV1.ToString("X") + " now " + R(r, pc));
            if (pc >= WinLo && pc <= WinHi)
                LogT("win " + R(r, pc));
            if ((pc == JalGp1 || pc == JalGp2) && bus != null)
            {
                try
                {
                    uint w = bus.Read32(pc);
                    if ((w >> 26) == 3)
                    {
                        _gpEntry = ((pc + 4) & 0xF0000000u) | ((w & 0x03FFFFFFu) << 2);
                        LogT("jal-getproc " + R(r, pc) + " target=0x" + _gpEntry.ToString("X8"));
                    }
                }
                catch { }
            }
            if (_gpEntry != 0 && pc == _gpEntry)
                LogT("getproc-entry " + R(r, pc));
            if (pc == StubA || pc == StubB)
            {
                LogT("iat-stub " + R(r, pc));
                if (KeepV1 && _redirects < 64 && r[31] != 0 && (r[31] & 3) == 0)
                {
                    _redirects++;
                    LogT("HOST-ASSISTED keepv1 return pc=0x" + pc.ToString("X8") + " -> ra=0x" + r[31].ToString("X8") + " v1=0x" + v1.ToString("X") + " (v0 left as-is)");
                    pc = r[31];
                }
            }
            _lastV1 = r[3];
            _lastPc = pc;
        }
    }
}
