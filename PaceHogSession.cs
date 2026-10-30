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
        public const uint BoardInfoPhys = 0x09F00000; // HOST-ASSISTED zero block (CFE a2)
        public const uint KernLo = 0x80001000, KernHi = 0x8086B260, KernEntry = 0x806608C0;

        sealed class HogMmio : IBusDevice
        {
            public uint StartAddress => 0x10000000;
            public uint Size => 0x10000000;
            public const uint Uart = 0x406B40; // from Hog putc: sw 0x6B40 / poll lw 0x6B54 &0x20
            public readonly Dictionary<uint, uint> _regs = new();
            public int LogCap = 400;
            // HOST-ASSISTED WKTMR (0x10409580): 27 MHz ticks derived from CP0 Count at 202 MHz
            // (Hog's udelay rate). COUNTER (+4) = seconds, PRESCALER_VAL (+0x10) counts down.
            public Func<ulong>? Ticks27;
            // BCM7435 UPG UARTs A/B/C (16550, 4-byte stride). Hog uses B; the kernel's
            // ttyS port is whichever it probes. THR captured, LSR reads 0x60 (HOST-ASSISTED).
            static bool IsThr(uint off) => off == 0x406B00 || off == 0x406B40 || off == 0x406B80;
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
                if (n == 0 && _logged < LogCap)
                {
                    _logged++;
                    _log($"mmio {(w ? "W" : "R")} 0x{0x10000000 + off:X8} = 0x{v:X8} pc=0x{PcSampler.LastFetchPc:X8}");
                }
            }
            public uint Read32(uint off)
            {
                off &= ~3u;
                uint v;
                if (IsThr(off - 0x14)) v = 0x60; // LSR: THRE|TEMT
                else if (Ticks27 != null && (off == 0x409584 || off == 0x409590))
                {
                    ulong t = Ticks27();
                    v = off == 0x409584 ? (uint)(t / 27000000UL) : (uint)(27000000UL - 1 - t % 27000000UL);
                }
                else _regs.TryGetValue(off, out v);
                // HOST-ASSISTED: MEMC0/1 DDR-PHY op-done bit. Hog writes 1 to
                // 0x103C00FC / 0x103C03B8 then spins on bit0 of 0x103C0100 /
                // 0x103C03BC (reShmoo tail). Real PHY sets it when the op ends.
                if ((off >> 16) >= 0x3A && (off >> 16) <= 0x3C && ((off & 0xFFFF) == 0x0100 || (off & 0xFFFF) == 0x03BC)) v |= 1; // MEMC0..2
                Note(false, off, v);
                return v;
            }
            public void Write32(uint off, uint v)
            {
                off &= ~3u;
                if (IsThr(off)) { UartOut.Append((char)(v & 0xFF)); }
                _regs[off] = v;
                Note(true, off, v);
            }
            public byte Read8(uint off) => (byte)(Read32(off) >> (int)((off & 3) * 8));
            public void Write8(uint off, byte b)
            {
                uint a = off & ~3u; int sh = (int)((off & 3) * 8);
                _regs.TryGetValue(a, out uint w);
                w = (w & ~(0xFFu << sh)) | ((uint)b << sh);
                if (IsThr(a) && sh == 0) { UartOut.Append((char)b); _regs[a] = w; Note(true, a, w); return; }
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
            Log($"HOST-ASSISTED: image at phys 0x{ImagePhys:X8}; entry 0x{0x80000000 + ImagePhys + EntryOff:X8}; a0=a1=a3=0, a2=zero board-info @0x{0x80000000 + BoardInfoPhys:X8}; sticky MMIO; UART 0x10406B40");

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
            // HOST-ASSISTED: CFE a2 = board-info block. hog_in_c keeps it at sp+0x8c and
            // reads MAC bytes at +5..+9 and a u16 at +0x4E. Real CFE fills it from
            // flash nonvol; we pass an all-zero 4 KB block (MACs 00:00:..). Not firmware data.
            cpu.SetRegister(6, 0x80000000 + BoardInfoPhys);
            cp0.Status = 0x10000000; // CU0, BEV=0, EXL=0, IE=0

            var sw = Stopwatch.StartNew();
            long steps = 0, delaySkips = 0; int uartSeen = 0; long stopAt = -1; string why = "cap";
            long nextStatus = 5000;
            const int batch = 20000;
            while (sw.Elapsed.TotalSeconds < capSeconds)
            {
                try { cpu.Step(batch); }
                catch (Exception ex) { why = $"host exception {ex.GetType().Name}: {ex.Message} lastFetch=0x{PcSampler.LastFetchPc:X8}"; break; }
                steps += batch;
                // HOST-ASSISTED timing skip: Hog udelay (0x86400F54) spins on CP0 Count
                // until Count reaches a0. Jump Count to a0; no other state changes.
                { uint upc = cpu.ProgramCounter; if (upc >= 0x86400F64 && upc <= 0x86400F70) { uint tgt = cpu.GetRegister(4); if ((int)(tgt - cp0.Count) > 0) { cp0.Count = tgt; delaySkips++; } } }
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
                    try
                    {
                        // Dump the unpacked kernel exactly as Hog's own LZMA code left it (no host data written here).
                        var dumpPath = logPath + ".kernel.bin";
                        var buf = new byte[KernHi - KernLo];
                        for (uint a = 0; a < buf.Length; a += 4)
                            BitConverter.GetBytes(bus.Read32(KernLo + a)).CopyTo(buf, (int)a);
                        File.WriteAllBytes(dumpPath, buf);
                        Log($"DUMP 0x{KernLo:X8}..0x{KernHi:X8} -> {dumpPath} ({buf.Length} bytes)");
                    }
                    catch (Exception dx) { Log("DUMP failed: " + dx.Message); }
                    try
                    {
                        // Snapshot for --pace-kernel: emulator state exactly as Hog left it.
                        var sd = logPath + ".snap"; Directory.CreateDirectory(sd);
                        File.WriteAllBytes(Path.Combine(sd, "ram.bin"), ram.Memory);
                        var st = new StringBuilder();
                        st.AppendLine($"pc {cpu.ProgramCounter:X8}");
                        for (int r = 0; r < 32; r++) st.AppendLine($"r{r} {cpu.GetRegister(r):X8}");
                        for (int c = 0; c < 32; c++) st.AppendLine($"c{c} {cp0.ReadRegister(c):X8}");
                        foreach (var kv in mmio._regs) st.AppendLine($"m{kv.Key:X8} {kv.Value:X8}");
                        File.WriteAllText(Path.Combine(sd, "state.txt"), st.ToString());
                        Log($"SNAP -> {sd}");
                    }
                    catch (Exception sx) { Log("SNAP failed: " + sx.Message); }
                    break;
                }
                if (stopAt >= 0 && steps >= stopAt) { why = "uart error string seen"; break; }
                if (sw.ElapsedMilliseconds >= nextStatus)
                {
                    nextStatus += 15000;
                    var hot = mmio.Reads.OrderByDescending(k => k.Value).Take(3).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
                    Log($"status steps={steps} pc=0x{pc:X8} uart={uartSeen} dskip={delaySkips} hotR=[{string.Join(",", hot)}]");
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

        // PACE_KERNEL (--pace-kernel <snapdir>): resume the Linux 3.3.8 kernel from the
        // snapshot taken at Hog's kernel jump. All RAM/regs/CP0 come from the emulator's own
        // run of Hog (dump-true). HOST-ASSISTED: PRId BMIPS5000 value, UART LSR, sticky MMIO,
        // CP0 Count-based timer (1 tick per instruction).
        public const uint HostPrid = 0x00025A11; // HOST-ASSISTED guess: Broadcom BMIPS5000 (impl 0x5A)
        public static int RunKernel(string snapDir, int capSeconds, string logPath)
        {
            using var lw = new StreamWriter(logPath, false) { AutoFlush = true };
            void Log(string s) => lw.WriteLine($"{DateTime.Now:HH:mm:ss} {s}");
            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }
            MipsCpuEmulator.Mips32Ext = true; CP0.HonorPageMask = true; MipsBus.NoCeHooks = true;
            var cp0 = new CP0();
            var bus = new MipsBus(cp0) { IsBigEndian = false };
            var ram = new RamDevice(0, 0x10000000);
            bus.AddDevice(ram);
            // Board RAM per the kernel's own map: MEMC0 0..256M + 0x20000000..+256M, MEMC1 at 0x90000000..+512M
            bus.AddDevice(new RamDevice(0x20000000, 0x10000000));
            bus.AddDevice(new RamDevice(0x90000000, 0x20000000));
            Console.SetOut(TextWriter.Null); // CP0/exception Console spam costs time
            var mmio = new HogMmio(Log) { LogCap = 1500 };
            bus.AddDevice(mmio);
            byte[] snapRam = File.ReadAllBytes(Path.Combine(snapDir, "ram.bin"));
            // HOST-ASSISTED boot-arg edit: only one emulated core, so "maxcpus=2" -> "maxcpus=1"
            // in Hog's command line (otherwise __cpu_up spins forever waiting for CPU1).
            var pat = Encoding.ASCII.GetBytes("maxcpus=2"); int patched = 0;
            for (int i = 0; i + pat.Length <= snapRam.Length; i++)
            {
                if (snapRam[i] != (byte)'m') continue;
                int j = 1; while (j < pat.Length && snapRam[i + j] == pat[j]) j++;
                if (j == pat.Length) { snapRam[i + 8] = (byte)'1'; patched++; }
            }
            ram.LoadData(0, snapRam);
            Log($"HOST-ASSISTED maxcpus=2->1 patched {patched} copies");
            var cpu = new MipsCpuEmulator(bus, cp0);
            foreach (var line in File.ReadAllLines(Path.Combine(snapDir, "state.txt")))
            {
                var f = line.Split(' '); if (f.Length != 2) continue;
                uint v = Convert.ToUInt32(f[1], 16);
                if (f[0] == "pc") cpu.SetRegister(MipsCpuEmulator.Register.PC, v);
                else if (f[0][0] == 'r') cpu.SetRegister(int.Parse(f[0].Substring(1)), v);
                else if (f[0][0] == 'c') { int c = int.Parse(f[0].Substring(1)); if (c == 13) cp0.Cause = v; else if (c != 15) cp0.WriteRegister(c, v); }
                else if (f[0][0] == 'm') mmio._regs[Convert.ToUInt32(f[0].Substring(1), 16)] = v;
            }
            cp0.PRId = HostPrid;
            ulong t0 = (ulong)cp0.Count * 27UL / 202UL;
            mmio.Ticks27 = () => t0 + cp0.Ticks * 27UL / 202UL;
            // HOST-ASSISTED Config0/Config1 (BMIPS5000-like): M=1, MT=TLB, K0=3; 32-entry TLB,
            // 32 KB 2-way 64 B I/D caches. The real values come from the core, not the image.
            cp0.WriteRegister(16, (cp0.ReadRegister(16) & ~0x7u) | 0x80000083u);
            CP0.SelRegs[16 * 8 + 1] = 0xBEA95480u; // M=1 -> Config2
            CP0.SelRegs[16 * 8 + 2] = 0x00000463u; // HOST-ASSISTED L2: 1024 sets x 128 B x 4-way = 512 KB
            // HOST-ASSISTED SUN_TOP_CTRL chip family/product ID = BCM7435 B0 (kernel checks it).
            mmio._regs[0x404000] = 0x74350010u; mmio._regs[0x404004] = 0x74350010u;
            Log($"PACE_KERNEL snap={snapDir} pc=0x{cpu.ProgramCounter:X8} cap={capSeconds}s HOST-ASSISTED PRId=0x{HostPrid:X8} Config0=0x{cp0.ReadRegister(16):X8} Config1=0xBEA95480 Config2=0x463 chipid=0x74350010");
            var excCount = new Dictionary<uint, long>(); int excLogged = 0;
            MipsCpuEmulator.ExcHook = (code, epc, bad) =>
            {
                excCount.TryGetValue(code, out long n); excCount[code] = n + 1;
                if (code != 0 && excLogged < 300)
                {
                    excLogged++; Log($"EXC code={code} epc=0x{epc:X8} badva=0x{bad:X8} ra=0x{cpu.GetRegister(31):X8} status=0x{cp0.Status:X8} ctx=0x{cp0.Context:X8} k0=0x{cpu.GetRegister(26):X8} k1=0x{cpu.GetRegister(27):X8}");
                    if (excLogged <= 3 && (epc >> 29) == 4) // kseg0: dump code around EPC (runtime-generated handlers are not in the image dump)
                        Log($"  code@0x{epc - 0x40:X8}: " + string.Join(" ", Enumerable.Range(0, 24).Select(i => ram.Read32((epc & 0x1FFFFFFF) - 0x40 + (uint)i * 4).ToString("X8"))));
                    if (excLogged == 1)
                        Log($"  vec@0x80000000: " + string.Join(" ", Enumerable.Range(0, 8).Select(i => ram.Read32((uint)i * 4).ToString("X8"))) + " | vec@0x80000180: " + string.Join(" ", Enumerable.Range(0, 8).Select(i => ram.Read32(0x180 + (uint)i * 4).ToString("X8"))));
                }
            };
            var pcHist = new Dictionary<uint, long>();
            var sw = Stopwatch.StartNew();
            long steps = 0; int uartSeen = 0; long stopAt = -1; string why = "cap"; long nextStatus = 15000;
            const int batch = 20000;
            while (sw.Elapsed.TotalSeconds < capSeconds)
            {
                try { cpu.Step(batch); }
                catch (Exception ex) { why = $"host exception {ex.GetType().Name}: {ex.Message} lastFetch=0x{PcSampler.LastFetchPc:X8}"; break; }
                steps += batch;
                uint pc = cpu.ProgramCounter;
                uint bucket = pc & ~0xFFu; pcHist.TryGetValue(bucket, out long hn); pcHist[bucket] = hn + 1;
                if (mmio.UartOut.Length > uartSeen)
                {
                    string add = mmio.UartOut.ToString(uartSeen, mmio.UartOut.Length - uartSeen);
                    uartSeen = mmio.UartOut.Length;
                    foreach (var l in add.Split('\n')) if (l.Length > 0) Log("UART| " + l.TrimEnd('\r'));
                    if (stopAt < 0 && mmio.UartOut.ToString().Contains("Kernel panic")) stopAt = steps + 20_000_000;
                }
                if (stopAt >= 0 && steps >= stopAt) { why = "kernel panic seen"; break; }
                if (sw.ElapsedMilliseconds >= nextStatus)
                {
                    nextStatus += 15000;
                    Log($"status steps={steps} pc=0x{pc:X8} ra=0x{cpu.GetRegister(31):X8} st=0x{cp0.Status:X8} cause=0x{cp0.Cause:X8} epc=0x{cp0.EPC:X8} cnt=0x{cp0.Count:X8} cmp=0x{cp0.Compare:X8} uart={uartSeen} exc=[{string.Join(",", excCount.Select(k => $"{k.Key}:{k.Value}"))}]");
                }
            }
            MipsCpuEmulator.ExcHook = null;
            Log($"STOP {why} steps={steps} secs={sw.Elapsed.TotalSeconds:F0}");
            Log("regs " + string.Join(" ", Enumerable.Range(0, 32).Select(i => $"r{i}={cpu.GetRegister(i):X8}")) + $" pc={cpu.ProgramCounter:X8}");
            Log("hotPC " + string.Join(",", pcHist.OrderByDescending(k => k.Value).Take(12).Select(k => $"0x{k.Key:X8}x{k.Value}")));
            Log("UART-ALL<<" + mmio.UartOut.ToString().Replace("\r", "") + ">>");
            var topR = mmio.Reads.OrderByDescending(k => k.Value).Take(10).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
            var topW = mmio.Writes.OrderByDescending(k => k.Value).Take(10).Select(k => $"0x{0x10000000 + k.Key:X8}x{k.Value}");
            Log($"topR {string.Join(",", topR)} | topW {string.Join(",", topW)}");
            return 0;
        }
    }
}