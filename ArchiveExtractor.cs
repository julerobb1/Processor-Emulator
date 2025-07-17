using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Processor_Emulator
{
    public static class ArchiveExtractor
    {
        /// <summary>
        /// Extracts an archive or raw disk image to the specified directory.
        /// For .bin images, splits MBR partitions and extracts firmware sections by signature.
        /// Otherwise invokes 7z.
        /// </summary>
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            if (Path.GetExtension(archivePath).Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
                ExtractPartitions(archivePath, outputDir);
                ExtractFirmwareSections(archivePath, outputDir);
                return;
            }
            var sevenZip = Resolve7zExecutable();
            if (string.IsNullOrEmpty(sevenZip))
                throw new InvalidOperationException("7z.exe not found. Please install 7-Zip.");
            var psi = new ProcessStartInfo
            {
                FileName = sevenZip,
                Arguments = $"x -y -o\"{outputDir}\" \"{archivePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
            }
            SanitizeExtraction(outputDir);
        }

        /// <summary>
        /// Extracts an archive then runs firmware analysis.
        /// </summary>
        public static void ExtractAndAnalyze(string archivePath, string outputDir)
        {
            ExtractArchive(archivePath, outputDir);
            FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, outputDir);
        }

        private static void ExtractPartitions(string imagePath, string outputDir)
        {
            var data = File.ReadAllBytes(imagePath);
            if (data.Length < 512 || data[510] != 0x55 || data[511] != 0xAA)
                throw new InvalidOperationException("Invalid MBR disk image.");
            for (int i = 0; i < 4; i++)
            {
                int entry = 0x1BE + i * 16;
                byte type = data[entry + 4];
                uint lba = BitConverter.ToUInt32(data, entry + 8);
                uint sectors = BitConverter.ToUInt32(data, entry + 12);
                if (type == 0 || sectors == 0) continue;
                long start = (long)lba * 512;
                long len = (long)sectors * 512;
                if (start + len > data.LongLength) len = data.LongLength - start;
                var partFile = Path.Combine(outputDir, $"partition{i+1}_type{type:X2}.bin");
                File.WriteAllBytes(partFile, data.Skip((int)start).Take((int)len).ToArray());
            }
        }

        private static void ExtractFirmwareSections(string imagePath, string outputDir)
        {
            var data = File.ReadAllBytes(imagePath);
            var signatures = new Dictionary<string, byte[]>
            {
                ["kernel"] = new byte[]{ 0x1F, 0x8B }, // gzip
                ["elf"] = new byte[]{ 0x7F, (byte)'E', (byte)'L', (byte)'F' }
            };
            var matches = new List<(string name, int offset)>();
            foreach (var kv in signatures)
            {
                int idx = IndexOf(data, kv.Value, 0);
                if (idx >= 0) matches.Add((kv.Key, idx));
            }
            matches = matches.OrderBy(m => m.offset).ToList();
            for (int i = 0; i < matches.Count; i++)
            {
                var (name, start) = matches[i];
                int end = (i + 1 < matches.Count) ? matches[i + 1].offset : data.Length;
                var section = data.Skip(start).Take(end - start).ToArray();
                var outFile = Path.Combine(outputDir, $"{name}_{start:X}.bin");
                File.WriteAllBytes(outFile, section);
            }
        }

        private static int IndexOf(byte[] data, byte[] pattern, int start)
        {
            for (int i = start; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                    if (data[i + j] != pattern[j]) { match = false; break; }
                if (match) return i;
            }
            return -1;
        }

        private static void SanitizeExtraction(string dir)
        {
            try
            {
                var full = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    var f = Path.GetFullPath(file);
                    if (!f.StartsWith(full, StringComparison.OrdinalIgnoreCase))
                        File.Delete(f);
                }
            }
            catch { }
        }

        private static string Resolve7zExecutable()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
            };
            foreach (var p in paths) if (File.Exists(p)) return p;
            var env = Environment.GetEnvironmentVariable("PATH");
            foreach (var d in env?.Split(';') ?? Array.Empty<string>())
            {
                try { var exe = Path.Combine(d.Trim(), "7z.exe"); if (File.Exists(exe)) return exe; } catch { }
            }
            return null;
        }
                var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? Array.Empty<string>();
                foreach (var dir in paths)
                {
                    try
                    {
                        var exe = Path.Combine(dir.Trim(), "7z.exe");
                        if (File.Exists(exe)) return exe;
                    }
                    catch { }
                }
                // Prompt user to locate 7z.exe
                try
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        Title = "Locate 7z.exe",
                        Filter = "7-Zip Executable (7z.exe)|7z.exe"
                    };
                    if (dlg.ShowDialog() == true)
                        return dlg.FileName;
                }
                catch { }
                return null;
            }
    }

    // Add new helper for .bin firmware segmentation
    private static void ExtractFirmwareSections(string imagePath, string outputDir)
    {
        byte[] data = File.ReadAllBytes(imagePath);
        var sigs = new Dictionary<string, byte[]>
        {
            { "ELF", new byte[]{0x7F, (byte)'E', (byte)'L', (byte)'F'} },
            { "SQUASHFS", new byte[]{0x68, 0x73, 0x71, 0x73} },
            { "GZIP", new byte[]{0x1F, 0x8B, 0x08} }
        };
        foreach (var kv in sigs)
        {
            string name = kv.Key;
            byte[] sig = kv.Value;
            int idx = IndexOfSequence(data, sig);
            if (idx >= 0)
            {
                string outFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(imagePath) + $"_{name}.bin");
                Directory.CreateDirectory(Path.GetDirectoryName(outFile));
                File.WriteAllBytes(outFile, data.Skip(idx).ToArray());
            }
        }
    }

    // Finds the first occurrence of a byte sequence in a byte array
    private static int IndexOfSequence(byte[] haystack, byte[] needle)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { found = false; break; }
            }
            if (found) return i;
        }
        return -1;
    }
}
