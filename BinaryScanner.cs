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
}
