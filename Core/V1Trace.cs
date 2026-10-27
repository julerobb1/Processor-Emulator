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

        public static void Step(MipsBus bus, uint[] r, ref uint pc)
        {
            if (r == null || r.Length < 32) return;
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
