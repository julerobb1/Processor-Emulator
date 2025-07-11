using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Unicorn;
using Unicorn.X86;
using Unicorn.ARM;
using Unicorn.ARM64;
using Unicorn.MIPS;
using Unicorn.PPC;
using Unicorn.SPARC;
using Unicorn.RISCV;

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
        public static byte[] Translate(string fromArch, string toArch, byte[] input)
        {
            // If no translation needed
            if (string.Equals(fromArch, toArch, StringComparison.OrdinalIgnoreCase))
                return input;
            // Determine Unicorn arch/mode
            UnicornEngine uc;
            switch (fromArch.ToLowerInvariant())
            {
                case "x86":
                    uc = new UnicornEngine(UnicornArch.X86, UnicornMode.Bit32);
                    break;
                case "x64":
                case "x86_64":
                    uc = new UnicornEngine(UnicornArch.X86, UnicornMode.Bit64);
                    break;
                case "arm":
                    uc = new UnicornEngine(UnicornArch.ARM, UnicornMode.Arm);
                    break;
                case "arm64":
                    uc = new UnicornEngine(UnicornArch.ARM64, UnicornMode.LittleEndian);
                    break;
                case "mips":
                    uc = new UnicornEngine(UnicornArch.MIPS, UnicornMode.Mips32LittleEndian);
                    break;
                case "ppc":
                case "ppc32":
                    uc = new UnicornEngine(UnicornArch.PPC, UnicornMode.PPC32);
                    break;
                case "ppc64":
                    uc = new UnicornEngine(UnicornArch.PPC, UnicornMode.PPC64);
                    break;
                case "sparc":
                    uc = new UnicornEngine(UnicornArch.SPARC, UnicornMode.Sparc32);
                    break;
                case "sparc64":
                    uc = new UnicornEngine(UnicornArch.SPARC, UnicornMode.Sparc64);
                    break;
                case "riscv":
                case "riscv32":
                    uc = new UnicornEngine(UnicornArch.RISCV, UnicornMode.RiscV32);
                    break;
                case "riscv64":
                    uc = new UnicornEngine(UnicornArch.RISCV, UnicornMode.RiscV64);
                    break;
                default:
                    MessageBox.Show($"Unsupported ISA: {fromArch}", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return input;
            }
            const ulong BASE = 0x100000;
            ulong size = (ulong)input.Length;
            // Map memory and write code
            uc.Memory.Map(BASE, size);
            uc.Memory.Write(BASE, input);
            // Initialize PC
            uc.Registers.PC = BASE;
            var output = new List<byte>();
            // Hook instructions to capture translated bytes
            uc.Hooks.Add(HookType.Code, (ucEngine, address, length, user) =>
            {
                var bytes = ucEngine.Memory.Read(address, length);
                output.AddRange(bytes);
            });
            try
            {
                // Run through the region once
                uc.Start(BASE, BASE + size);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Emulation error: {ex.Message}", "Translate Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            uc.Dispose();
            return output.ToArray();
        }
    }
}
