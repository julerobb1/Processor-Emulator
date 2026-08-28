using System;
using System.IO;
using ProcessorEmulator.Core;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Loaders;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator
{
    // Honest dump -> NkBinLoader -> MIPS/CE step. Every dump B000FF
    // next to nk.bin (etc.bin and any other) is loaded at that
    // file's imageStart. No invented 0x81360000 map, no
    // CreateProcess, no SetEvent.
    public sealed class MediaroomSession
    {
        private const uint RamSize = 256u * 1024u * 1024u;
        private const uint UartBase = 0xB0000000;
        private const uint UartSize = 0x1000;
        private const int HuntDepth = 3;

        private const uint MemsetSw = 0x80014200;
        private const uint MemsetDelay = 0x8001420C;
        private const int T1 = 9;

        private readonly Action<string> _status;
        private MipsBus _bus;
        private CP0 _cp0;
        private MipsCpuEmulator _cpu;
        private volatile bool _stop;
        private volatile uint _lastPc;
        private volatile int _hz;
        private string _memsetNote = "";

        public uint ProgramCounter => _lastPc;
        public long Steps { get; private set; }
        public int Hertz => _hz;
        public string MemsetNote => _memsetNote;
        public string DumpRoot { get; private set; } = "";
        public string NkPath { get; private set; } = "";
        public bool KernelLoaded { get; private set; }
        public bool GuestVideoWrote { get; private set; }

        public MediaroomSession(Action<string> status)
        {
            _status = status ?? (_ => { });
        }

        public void RequestStop()
        {
            _stop = true;
        }

        public bool Run(string feed)
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
                _status("no nk.bin");
                return false;
            }

            NkPath = nk;
            string nkDir = Path.GetDirectoryName(nk);
            if (!string.IsNullOrEmpty(nkDir))
                HostHardDisk.OfferFeed(nkDir);
            _status("loading");

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

            NkLoadResult loaded;
            try
            {
                loaded = NkBinLoader.Load(File.ReadAllBytes(nk), new BusMemoryAdapter(_bus));
            }
            catch (Exception ex)
            {
                _status("NkBinLoader: " + ex.Message);
                return false;
            }

            KernelLoaded = true;
            DumpRoot = HostHardDisk.Root;
            GuestVideoWrote = false;
            _cpu.SetRegister(MipsCpuEmulator.Register.PC, (uint)loaded.EntryPoint);
            _cpu.SetRegister(MipsCpuEmulator.Register.SP, 0x80000000u + RamSize - 0x1000u);
            _lastPc = (uint)loaded.EntryPoint;
            _status("running");

            const int batch = 50000;
            long lastHzSteps = 0;
            int lastHzMs = Environment.TickCount;
            uint memsetT1First = 0;
            long memsetFirstStep = -1;
            long memsetSameT1 = 0;
            uint memsetLastT1 = 0;
            try
            {
                while (!_stop)
                {
                    _cpu.Step(batch);
                    Steps += batch;
                    uint pc = _cpu.ProgramCounter;
                    _lastPc = pc;

                    int now = Environment.TickCount;
                    int dt = now - lastHzMs;
                    if (dt >= 250)
                    {
                        _hz = (int)((Steps - lastHzSteps) * 1000L / Math.Max(1, dt));
                        lastHzSteps = Steps;
                        lastHzMs = now;
                    }

                    if (pc >= MemsetSw && pc <= MemsetDelay)
                    {
                        uint t1 = _cpu.GetRegister(T1);
                        if (memsetFirstStep < 0)
                        {
                            memsetFirstStep = Steps;
                            memsetT1First = t1;
                            memsetLastT1 = t1;
                            memsetSameT1 = 0;
                        }
                        else if (t1 == memsetLastT1)
                            memsetSameT1 += batch;
                        else
                        {
                            memsetSameT1 = 0;
                            memsetLastT1 = t1;
                        }

                        if (memsetSameT1 >= 2000000 && t1 != 0)
                            _memsetNote = "memset 0x80014200 stuck t1=0x" + t1.ToString("X8") + " (not skipped)";
                        else if (memsetT1First > 0x01000000)
                            _memsetNote = "memset 0x80014200 t1=0x" + t1.ToString("X8") + " from 0x" + memsetT1First.ToString("X8") + " (running)";
                        else
                            _memsetNote = "memset 0x80014200 t1=0x" + t1.ToString("X8");
                    }
                    else if (memsetFirstStep >= 0 && string.IsNullOrEmpty(_memsetNote))
                        _memsetNote = "memset 0x80014200 left after " + (Steps - memsetFirstStep) + " steps";

                    if (pc == 0x80059E98 && (_memsetNote == null || _memsetNote.IndexOf("OEMIdle", StringComparison.Ordinal) < 0))
                        _memsetNote = (string.IsNullOrEmpty(_memsetNote) ? "" : _memsetNote + "; ")
                            + "OEMIdle 0x80059E98 (running; not a halt)";
                }
            }
            catch (Exception ex)
            {
                _lastPc = _cpu != null ? _cpu.ProgramCounter : _lastPc;
                _status("CPU " + ex.GetType().Name + " PC=0x" + _lastPc.ToString("X8"));
                return KernelLoaded;
            }

            _status("stopped");
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
    }
}
