using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator
{
    // PACE_HOG: flag-gated (--pace-hog) little-endian loader for the Pace
    // XG1v3 (PX013AN, BCM7435) "Hog" boot stub. Dump-true: the whole signed
    // image is placed in RAM unmodified; the stub at file+0x44 (bal 0xB0)
    // derives its own base (s0=ra-8), copies itself to its link address
    // 0x86400338..0x8641E6F0, sets sp/gp and calls hog_in_c. No decrypted
    // data is invented. HOST-ASSISTED parts (labelled in the log):
    //   - image load address (CFE's real choice is unknown)
    //   - CFE args a0..a3 = 0 (no CFE API seal)
    //   - catch-all sticky MMIO for 0x10000000..0x1FFFFFFF (reads 0 until written)
    //   - UPG UART at 0x10406B40 (Hog putc writes THR 0xB0406B40, polls LSR 0xB0406B54 bit5); LSR reads 0x60
    public static class PaceHogSession
    {
        public const uint ImagePhys = 0x0A000000; // HOST-ASSISTED load address
        public const uint EntryOff = 0x44;
        public const uint KernLo = 0x80001000, KernHi = 0x8086B260, KernEntry = 0x806608C0;

        sealed class HogMmio : IBusDevice
        {
            public uint StartAddress => 0x10000000;
            public uint Size => 0x10000000;
            public const uint Uart = 0x406B40; // from Hog putc: sw 0x6B40 / poll lw 0x6B54 &0x20
            readonly Dictionary<uint, uint> _regs = new();
            public readonly Dictionary<uint, long> Reads = new();
            public readonly Dictionary<uint, long> Writes = new();
            public readonly StringBuilder UartOut = new();
            readonly Action<string> _log;
            int _logged;
            public HogMmio(Action<string> log) { _log = log; }
            void Note(bool w, uint off, uint v)
            {
                var d = w ? Writes : Reads;
                d.TryGetValue(off, out long n); d[off] = n + 1;
                if (n == 0 && _logged < 400)
                {
                    _logged++;
                    _log($"mmio {(w ? "W" : "R")} 0x{0x10000000 + off:X8} = 0x{v:X8} pc=0x{PcSampler.LastFetchPc:X8}");
                }
            }
            public uint Read32(uint off)
            {
                off &= ~3u;
                uint v;
                if (off == Uart + 0x14) v = 0x60; // LSR: THRE|TEMT
                else _regs.TryGetValue(off, out v);
                // HOST-ASSISTED: MEMC0/1 DDR-PHY op-done bit. Hog writes 1 to
                // 0x103C00FC / 0x103C03B8 then spins on bit0 of 0x103C0100 /
                // 0x103C03BC (reShmoo tail). Real PHY sets it when the op ends.
                if (off == 0x3C0100 || off == 0x3C03BC) v |= 1;
                Note(false, off, v);
                return v;
            }
            public void Write32(uint off, uint v)
            {
                off &= ~3u;
                if (off == Uart) { UartOut.Append((char)(v & 0xFF)); }
                _regs[off] = v;
                Note(true, off, v);
            }
            public byte Read8(uint off) => (byte)(Read32(off) >> (int)((off & 3) * 8));
            public void Write8(uint off, byte b)
            {
                uint a = off & ~3u; int sh = (int)((off & 3) * 8);
                _regs.TryGetValue(a, out uint w);
                w = (w & ~(0xFFu << sh)) | ((uint)b << sh);
                if (a == Uart && sh == 0) { UartOut.Append((char)b); _regs[a] = w; Note(true, a, w); return; }
                Write32(a, w);
            }
        }

        public static int Run(string image, int capSeconds, string logPath)
        {
            using var lw = new StreamWriter(logPath, false) { AutoFlush = true };
            void Log(string s) => lw.WriteLine($"{DateTime.Now:HH:mm:ss} {s}");
            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }
            byte[] img = File.ReadAllBytes(image);
            Log($"PACE_HOG image={image} len={img.Length} cap={capSeconds}s");
            if (img.Length < 0x200 || Encoding.ASCII.GetString(img, 0, 4) != "Pace" || Encoding.ASCII.GetString(img, 0x28, 4) != "Pimg")
            { Log("not a Pace/Pimg image"); return 2; }
            Log($"HOST-ASSISTED: image at phys 0x{ImagePhys:X8}; entry 0x{0x80000000 + ImagePhys + EntryOff:X8}; a0..a3=0; sticky MMIO; UART 0x10406B40");

            MipsCpuEmulator.Mips32Ext = true;
            CP0.HonorPageMask = true;
            MipsBus.NoCeHooks = true;
            var cp0 = new CP0();
            var bus = new MipsBus(cp0) { IsBigEndian = false };
            var ram = new RamDevice(0, 0x10000000);
            bus.AddDevice(ram);
            bus.AddDevice(new RamDevice(0x90000000, 0x08000000)); // MEMC1 window: stub TLB maps VA 0xE0000000 (2x64MB) -> phys 0x90000000
            var mmio = new HogMmio(Log);
            bus.AddDevice(mmio);
            ram.LoadData(ImagePhys, img);
            var cpu = new MipsCpuEmulator(bus, cp0);
            for (int r = 1; r < 32; r++) cpu.SetRegister(r, 0);
            cpu.SetRegister(MipsCpuEmulator.Register.PC, 0x80000000 + ImagePhys + EntryOff);
            cp0.Status = 0x10000000; // CU0, BEV=0, EXL=0, IE=0

            var sw = Stopwatch.StartNew();
            long steps = 0; int uartSeen = 0; long stopAt = -1; string why = "cap";
            long nextStatus = 5000;
            const int batch = 20000;
            while (sw.Elapsed.TotalSeconds < capSeconds)
            {
                try { cpu.Step(batch); }
                catch (Exception ex) { why = $"host exception {ex.GetType().Name}: {ex.Message} lastFetch=0x{PcSampler.LastFetchPc:X8}"; break; }
                steps += batch;
                uint pc = cpu.ProgramCounter;
                if (mmio.UartOut.Length > uartSeen)
                {
                    string add = mmio.UartOut.ToString(uartSeen, mmio.UartOut.Length - uartSeen);
                    uartSeen = mmio.UartOut.Length;
                    foreach (var line in add.Split('\n')) if (line.Length > 0) Log("UART| " + line.TrimEnd('\r'));
                    string all = mmio.UartOut.ToString();
                    if (stopAt < 0 && (all.Contains("Bad LZMA") || all.Contains("LzmaDecode returns") || all.Contains("Unexpected lzmaStatus") || all.Contains("Fatal Error") || all.Contains("Out of Memory")))
                        stopAt = steps + 5_000_000;
                }
                if ((cp0.Status & 2) != 0)
                {
                    why = $"exception EXL Cause=0x{cp0.Cause:X8} EPC=0x{cp0.EPC:X8} BadVA=0x{cp0.BadVAddr:X8} pc=0x{pc:X8}";
                    break;
                }
                if (pc >= KernLo && pc < KernHi)
                {
                    why = $"KERNEL JUMP pc=0x{pc:X8} (entry 0x{KernEntry:X8}); first words at 0x80001000: " +
                          string.Join(" ", Enumerable.Range(0, 8).Select(i => bus.Read32(KernLo + (uint)i * 4).ToString("X8")));
                    break;
                }
                if (stopAt >= 0 && steps >= stopAt) { why = "uart error string seen"; break; }
                if (sw.ElapsedMilliseconds >= nextStatus)
                {
                    nextStatus += 15000;
                    var hot = mmio.Reads.OrderByDescending(k => k.Value).Take(3).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
                    Log($"status steps={steps} pc=0x{pc:X8} uart={uartSeen} hotR=[{string.Join(",", hot)}]");
                }
            }
            Log($"STOP {why} steps={steps} secs={sw.Elapsed.TotalSeconds:F0}");
            string regs = string.Join(" ", Enumerable.Range(0, 32).Select(i => $"r{i}={cpu.GetRegister(i):X8}"));
            Log("regs " + regs);
            Log("UART-ALL<<" + mmio.UartOut.ToString().Replace("\r", "") + ">>");
            var topR = mmio.Reads.OrderByDescending(k => k.Value).Take(8).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
            var topW = mmio.Writes.OrderByDescending(k => k.Value).Take(8).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
            Log($"topR {string.Join(",", topR)} | topW {string.Join(",", topW)}");
            return 0;
        }
    }
}
