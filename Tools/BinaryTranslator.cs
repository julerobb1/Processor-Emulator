using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using StubUnicorn = ProcessorEmulator.Tools.StubUnicorn;

// Stub Unicorn namespace definitions for in-process translation
namespace ProcessorEmulator.Tools.StubUnicorn
{
    public enum UnicornArch { X86, PPC, ARM, ARM64, MIPS, SPARC, RISCV }
    public enum UnicornMode { Bit32, Bit64, Arm, LittleEndian, Mips32LittleEndian, PPC32, PPC64, Sparc32, Sparc64, RiscV32, RiscV64 }
    public enum HookType { Code }

    // Stub: memory manager for in-process translation
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
    // Stub: memory manager for in-process translation
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

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Provides cross-architecture binary translation (e.g. static recompilation) between supported ISAs.
    /// </summary>
    public static class BinaryTranslator
    {
        /// <summary>
        /// Translates a raw binary image from one architecture to another.
        /// </summary>
        /// <param name="fromArch">Source architecture (e.g. "x86", "ARM").</param>
        /// <param name="toArch">Target architecture (e.g. "x64", "MIPS").</param>
        /// <param name="input">Original binary data.</param>
        /// <returns>Translated binary data for the target ISA.</returns>
        /// <summary>
        /// Flag to enable in-process Unicorn stub emulation instead of RetDec.
        /// </summary>
        public static bool UseUnicornEngine { get; set; } = false;
        public static byte[] Translate(string fromArch, string toArch, byte[] input)
        {
            // If no translation needed
            if (string.Equals(fromArch, toArch, StringComparison.OrdinalIgnoreCase))
                return input;
            // Use in-process Unicorn stub engine if toggled
            if (UseUnicornEngine)
                return TranslateWithUnicornEngine(fromArch, toArch, input);
            // Otherwise use external RetDec tool for full cross-ISA translation
            return TranslateWithRetDec(fromArch, toArch, input);
        }

        /// <summary>
        /// In-process translation stub using Unicorn to execute and capture bytes. Only supports same-architecture emulation.
        /// </summary>
        private static byte[] TranslateWithUnicornEngine(string fromArch, string toArch, byte[] input)
        {
            // Only emulate if architectures match; otherwise fallback to RetDec
            if (!string.Equals(fromArch, toArch, StringComparison.OrdinalIgnoreCase))
                return TranslateWithRetDec(fromArch, toArch, input);
            // Determine Unicorn arch/mode
            StubUnicorn.UnicornEngine uc;
            switch (fromArch.ToLowerInvariant())
            {
                case "x86":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.X86, StubUnicorn.UnicornMode.Bit32);
                    break;
                case "x64":
                case "x86_64":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.X86, StubUnicorn.UnicornMode.Bit64);
                    break;
                case "arm":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.ARM, StubUnicorn.UnicornMode.Arm);
                    break;
                case "arm64":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.ARM64, StubUnicorn.UnicornMode.LittleEndian);
                    break;
                case "mips":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.MIPS, StubUnicorn.UnicornMode.Mips32LittleEndian);
                    break;
                case "ppc":
                case "ppc32":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.PPC, StubUnicorn.UnicornMode.PPC32);
                    break;
                case "ppc64":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.PPC, StubUnicorn.UnicornMode.PPC64);
                    break;
                case "sparc":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.SPARC, StubUnicorn.UnicornMode.Sparc32);
                    break;
                case "sparc64":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.SPARC, StubUnicorn.UnicornMode.Sparc64);
                    break;
                case "riscv":
                case "riscv32":
                    uc = new StubUnicorn.UnicornEngine(StubUnicorn.UnicornArch.RISCV, StubUnicorn.UnicornMode.RiscV32);
                    break;
                default:
                    return input;
            }
            const ulong BASE = 0x100000;
            ulong size = (ulong)input.Length;
            uc.Memory.Map(BASE, size);
            uc.Memory.Write(BASE, input);
            uc.Registers.PC = BASE;
            var output = new List<byte>();
            uc.Hooks.Add(StubUnicorn.HookType.Code, (engine, address, length, user) =>
            {
                var bytes = engine.Memory.Read(address, length);
                output.AddRange(bytes);
            });
            try
            {
                uc.Start(BASE, BASE + size);
            }
            catch
            {
                uc.Dispose();
                return input;
            }
            uc.Dispose();
            return output.ToArray();
        }
        /// <summary>
        /// Fallback translation using RetDec CLI.
        /// </summary>
        private static byte[] TranslateWithRetDec(string fromArch, string toArch, byte[] input)
        {
            string tempInput = null;
            string tempOutput = null;
            try
            {
                tempInput = Path.GetTempFileName();
                tempOutput = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                File.WriteAllBytes(tempInput, input);
                var args = $"--mode raw -e {fromArch} -t {toArch} -o {tempOutput} {tempInput}";
                var psi = new ProcessStartInfo("retdec-decompiler", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    var err = proc.StandardError.ReadToEnd();
                    MessageBox.Show("RetDec did not produce output.", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return input;
                }
                if (!File.Exists(tempOutput))
                {
                    MessageBox.Show("RetDec did not produce output file.", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return input;
                }
                return File.ReadAllBytes(tempOutput);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RetDec invocation error: {ex.Message}", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return input;
            }
            finally
            {
                try { if (tempInput != null) File.Delete(tempInput); if (tempOutput != null) File.Delete(tempOutput); } catch { }
            }
        }
    }
}
