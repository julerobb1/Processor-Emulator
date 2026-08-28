using System;
using System.IO;
using System.Text;
using System.Threading;
using ProcessorEmulator.Core;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Loaders;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator
{
    // Honest dump -> NkBinLoader -> MIPS/CE step. No synthetic
    // firmware, no CreateProcess, no ExtraROM map, no SetEvent.
    public sealed class MediaroomSession
    {
        private const uint RamSize = 256u * 1024u * 1024u;
        private const uint UartBase = 0xB0000000;
        private const uint UartSize = 0x1000;
        private const int HuntDepth = 3;

        private readonly Action<string> _log;
        private MipsBus _bus;
        private CP0 _cp0;
        private MipsCpuEmulator _cpu;
        private volatile bool _stop;

        public uint ProgramCounter => _cpu != null ? _cpu.ProgramCounter : 0;
        public long Steps { get; private set; }
        public string DumpRoot { get; private set; } = "";
        public string NkPath { get; private set; } = "";
        public bool KernelLoaded { get; private set; }

        public MediaroomSession(Action<string> log)
        {
            _log = log ?? (_ => { });
        }

        public void RequestStop()
        {
            _stop = true;
        }

        public bool Run(string feed, int maxSteps)
        {
            _stop = false;
            Steps = 0;
            KernelLoaded = false;
            DumpRoot = "";
            NkPath = "";

            if (!string.IsNullOrWhiteSpace(feed))
                HostHardDisk.OfferFeed(feed.Trim());

            string huntRoot = NormalizeFeed(feed);
            if (string.IsNullOrEmpty(huntRoot))
                huntRoot = Environment.GetEnvironmentVariable(HostHardDisk.EnvName);
            if (string.IsNullOrEmpty(huntRoot))
                huntRoot = Environment.GetEnvironmentVariable(HostHardDisk.EnvNameAlt);

            string nk = FindNkBin(huntRoot);
            if (string.IsNullOrEmpty(nk))
            {
                _log("No nk.bin under the dump root (hunt is by filename, not Uverse in the path).");
                return false;
            }

            NkPath = nk;
            string nkDir = Path.GetDirectoryName(nk);
            if (!string.IsNullOrEmpty(nkDir))
                HostHardDisk.OfferFeed(nkDir);
            _log("nk.bin " + nk);

            _cp0 = new CP0();
            _bus = new MipsBus(_cp0);
            _bus.IsBigEndian = false;
            _bus.AddDevice(new RamDevice(0x00000000, RamSize));
            var pic1000 = new BcmStickyMmio(0x10001000, 0x1000, "MMIO1000");
            _bus.AddDevice(new BcmSysControlRegs(_cp0, pic1000));
            _bus.AddDevice(new BcmStickyMmio(0x11F00000, 0x1000, "MMIO11F"));
            _bus.AddDevice(new BcmStickyMmio(0x10500000, 0x1000, "MMIO1050"));
            _bus.AddDevice(pic1000);
            _bus.AddDevice(new BcmStickyMmio(0x10104000, 0x1000, "MMIO1010"));
            _bus.AddDevice(new BcmStickyMmio(0x10080000, 0x1000, "MMIO1008"));
            _bus.AddDevice(new BcmStickyMmio(0x10090000, 0x1000, "MMIO1009"));
            _bus.AddDevice(new BcmStickyMmio(0x10480000, 0x1000, "MMIO1048"));
            _bus.AddDevice(new BcmStickyMmio(0xF0600000, 0x1000, "MMIOF060"));
            _cpu = new MipsCpuEmulator(_bus, _cp0);
            _bus.AddDevice(new MipsUart(UartBase, UartSize));
            _cpu.OnLogMessage += s => _log(s);
            _cpu.OnConsoleOutput += s => _log(s);

            NkLoadResult loaded;
            try
            {
                loaded = NkBinLoader.Load(File.ReadAllBytes(nk), new BusMemoryAdapter(_bus));
            }
            catch (Exception ex)
            {
                _log("NkBinLoader failed: " + ex.Message);
                return false;
            }

            KernelLoaded = true;
            DumpRoot = HostHardDisk.Root;
            _log("Hard Disk root=" + (string.IsNullOrEmpty(DumpRoot) ? "(none)" : DumpRoot));
            if (!string.IsNullOrEmpty(HostHardDisk.ExtraRomPath))
                _log("etc.bin at " + HostHardDisk.ExtraRomPath + " (firmware names ETC.BIN; not mapped here)");
            _log("entry=0x" + loaded.EntryPoint.ToString("X8") + " records=" + loaded.RecordsLoaded);

            _cpu.SetRegister(MipsCpuEmulator.Register.PC, (uint)loaded.EntryPoint);
            _cpu.SetRegister(MipsCpuEmulator.Register.SP, 0x80000000u + RamSize - 0x1000u);

            const int batch = 50000;
            try
            {
                while (!_stop && Steps < maxSteps)
                {
                    int n = (int)Math.Min(batch, (long)maxSteps - Steps);
                    _cpu.Step(n);
                    Steps += n;
                    if (Steps % 1000000L == 0 || Steps == n)
                        _log("steps=" + Steps + " PC=0x" + _cpu.ProgramCounter.ToString("X8"));
                }
            }
            catch (Exception ex)
            {
                _log("CPU stop: " + ex.GetType().Name + ": " + ex.Message + " PC=0x" + _cpu.ProgramCounter.ToString("X8") + " steps=" + Steps);
                return KernelLoaded;
            }

            _log((_stop ? "stopped" : "step limit") + " steps=" + Steps + " PC=0x" + _cpu.ProgramCounter.ToString("X8"));
            _log("tv2clientce not started (firmware never CreateProcess).");
            return true;
        }

        private static string NormalizeFeed(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "";
            string path = raw.Trim().Trim('"');
            try
            {
                if (File.Exists(path))
                    return Path.GetFullPath(Path.GetDirectoryName(path) ?? "");
                if (Directory.Exists(path))
                    return Path.GetFullPath(path);
            }
            catch
            {
            }
            return "";
        }

        private static string FindNkBin(string root)
        {
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
            {
                string hit = FindNamed(root, "nk.bin", 0);
                if (!string.IsNullOrEmpty(hit))
                    return hit;
                string vol = HostHardDisk.HuntAttach(root);
                if (!string.IsNullOrEmpty(vol))
                {
                    hit = FindNamed(vol, "nk.bin", 0);
                    if (!string.IsNullOrEmpty(hit))
                        return hit;
                }
            }

            HostHardDisk.Attach();
            if (!string.IsNullOrEmpty(HostHardDisk.Root))
                return FindNamed(HostHardDisk.Root, "nk.bin", 0);
            return "";
        }

        private static string FindNamed(string dir, string name, int depth)
        {
            if (depth > HuntDepth || string.IsNullOrEmpty(dir))
                return "";
            try
            {
                foreach (string p in Directory.GetFileSystemEntries(dir))
                {
                    string n = Path.GetFileName(p);
                    if (string.IsNullOrEmpty(n) || n[0] == '.')
                        continue;
                    if (n.Equals(name, StringComparison.OrdinalIgnoreCase) && File.Exists(p))
                        return p;
                    if (Directory.Exists(p))
                    {
                        string inner = FindNamed(p, name, depth + 1);
                        if (!string.IsNullOrEmpty(inner))
                            return inner;
                    }
                }
            }
            catch
            {
            }
            return "";
        }

        private sealed class BusMemoryAdapter : IMemoryManager
        {
            private readonly MipsBus _bus;
            public BusMemoryAdapter(MipsBus bus) { _bus = bus; }
            public bool IsLittleEndian => !_bus.IsBigEndian;
            public uint ReadMemory32(ulong address) => _bus.Read32((uint)address);
            public void WriteMemory32(ulong address, uint value) => _bus.Write32((uint)address, value);
            public void WriteMemory(ulong address, byte[] data) => _bus.WriteBytes((uint)address, data);
        }

        internal sealed class ConsoleTap : TextWriter
        {
            private readonly TextWriter _inner;
            private readonly Action<string> _log;
            public ConsoleTap(TextWriter inner, Action<string> log)
            {
                _inner = inner;
                _log = log;
            }
            public override Encoding Encoding => _inner != null ? _inner.Encoding : Encoding.UTF8;
            public override void WriteLine(string value)
            {
                if (!string.IsNullOrEmpty(value))
                    _log(value);
                _inner?.WriteLine(value);
            }
            public override void Write(char value) => _inner?.Write(value);
        }
    }
}
