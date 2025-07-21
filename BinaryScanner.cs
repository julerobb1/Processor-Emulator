using System;
using System.IO;
using System.Linq;

namespace ProcessorEmulator.Tools
{
    public static class BinaryScanner
    {
        public static void ScanTypicalBinaryDirs(string rootDir)
        {
            var dirs = new[] {"bin", "sbin", Path.Combine("usr", "bin"), Path.Combine("usr", "sbin")};
            foreach (var dir in dirs)
            {
                string fullPath = Path.Combine(rootDir, dir);
                if (Directory.Exists(fullPath))
                    ScanDirectory(fullPath);
            }
        }

        public static void ScanDirectory(string directory)
        {
            var detector = new ArchitectureDetector();
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    byte[] data = File.ReadAllBytes(file);
                    string arch = ArchitectureDetector.Detect(data);
                    if (arch != "Unknown")
                    {
                        Console.WriteLine($"{file}: {arch}");
                        var disasm = ProcessorEmulator.Tools.Disassembler.Disassemble(data, arch);
                        foreach (var line in disasm.Take(5)) // Preview first 5 lines
                            Console.WriteLine("    " + line);
                    }
                }
                catch { /* Ignore unreadable files */ }
            }
        }
    }

    internal static class Disassembler
    {
        public static string[] Disassemble(byte[] data, string arch)
        {
            // Stub implementation: return dummy lines for now
            return new[]
            {
                $"Disassembly preview for arch: {arch}",
                "0x0000: NOP",
                "0x0001: MOV R1, R2",
                "0x0002: ADD R3, R4",
                "0x0003: JMP 0x0000"
            };
        }
    }
}
