using System;

namespace Unicorn
{
    public enum UnicornArch { X86, PPC, ARM, ARM64, MIPS, SPARC, RISCV }
    public enum UnicornMode { Bit32, Bit64, Arm, LittleEndian, Mips32LittleEndian, PPC32, PPC64, Sparc32, Sparc64, RiscV32, RiscV64 }
    public enum HookType { Code }

    public class UnicornEngine : IDisposable
    {
        public UnicornArch Arch { get; }
        public UnicornMode Mode { get; }
        public MemoryManager Memory { get; } = new MemoryManager();
        public RegisterManager Registers { get; } = new RegisterManager();
        public HookManager Hooks { get; } = new HookManager();
        public UnicornEngine(UnicornArch arch, UnicornMode mode) { Arch = arch; Mode = mode; }
        public void Start(ulong begin, ulong end) { /* Stub: no-op */ }
        public void Dispose() { /* Stub: no-op */ }
    }

    public class MemoryManager
    {
        public void Map(ulong addr, ulong size) { /* Stub: no-op */ }
        public void Write(ulong addr, byte[] data) { /* Stub: no-op */ }
        public byte[] Read(ulong addr, uint length) { return new byte[length]; }
    }

    public class RegisterManager { public ulong PC { get; set; } }

    public class HookManager
    {
        public void Add(HookType type, Action<UnicornEngine, ulong, uint, object> callback) { /* Stub: no-op */ }
    }
}
