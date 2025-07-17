using System;
using System.Diagnostics;
using System.IO;
using System;
using System.Diagnostics;
using System.IO;
    public static class ArchiveExtractor
    public static class ArchiveExtractor
        public static void ExtractArchive(string archivePath, string outputDir)
    public static class ArchiveExtractor
    {
        /// <summary>
        /// Extracts an archive or raw disk image to the specified directory.
        /// </summary>
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            // Raw disk image: split MBR partitions
            if (Path.GetExtension(archivePath).Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
                ExtractPartitions(archivePath, outputDir);
                return;
            }
            // Use 7z CLI for archive extraction
            string sevenZip = Resolve7zExecutable();
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

        private static void SanitizeExtraction(string outputDir)
        {
            try
            {
                string root = Path.GetFullPath(outputDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!Directory.Exists(outputDir)) return;
                string parent = Path.GetDirectoryName(root);
                if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent)) return;
                foreach (var file in Directory.GetFiles(parent, "*", SearchOption.AllDirectories))
                {
                    string full = Path.GetFullPath(file);
                    if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                        File.Delete(full);
                }
            }
            catch { }
        }

        private static string Resolve7zExecutable()
        {
            string[] locations = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
            };
            foreach (var loc in locations)
                if (File.Exists(loc)) return loc;
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(';'))
                {
                    try
                    {
                        var exe = Path.Combine(dir.Trim(), "7z.exe");
                        if (File.Exists(exe)) return exe;
                    }
                    catch { }
                }
            }
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

        private static void ExtractPartitions(string imagePath, string outputDir)
        {
            byte[] data = File.ReadAllBytes(imagePath);
            if (data.Length < 512 || data[510] != 0x55 || data[511] != 0xAA)
                throw new InvalidOperationException("Invalid MBR disk image.");
            for (int i = 0; i < 4; i++)
            {
                int offset = 0x1BE + i * 16;
                byte type = data[offset + 4];
                uint lba = BitConverter.ToUInt32(data, offset + 8);
                uint sectors = BitConverter.ToUInt32(data, offset + 12);
                if (type == 0 || sectors == 0) continue;
                long start = lba * 512L;
                long len = sectors * 512L;
                if (start + len > data.LongLength) len = data.LongLength - start;
                string fileName = Path.Combine(outputDir, $"partition{i+1}_type{type:X2}.bin");
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                File.WriteAllBytes(fileName, new ArraySegment<byte>(data, (int)start, (int)len).ToArray());
            }
        }
                if (Path.GetExtension(archivePath).Equals(".bin", StringComparison.OrdinalIgnoreCase))
                {
                    // Split raw disk image into partition files
                    ExtractPartitions(archivePath, outputDir);
                    return;
                }
            string sevenZip = Resolve7zExecutable();
            if (string.IsNullOrEmpty(sevenZip))
                throw new InvalidOperationException("7z.exe not found. Please install 7-Zip.");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZip,
                    Arguments = $"x -y -o\"{outputDir}\" \"{archivePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
            try { SanitizeExtraction(outputDir); } catch { }
        }

        /// <summary>
        /// Extracts an archive or raw disk image to the specified directory.
        /// </summary>
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            // Raw disk image: split MBR partitions
            if (Path.GetExtension(archivePath).Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
                ExtractPartitions(archivePath, outputDir);
                return;
            }
            // Use 7z CLI for archives
            string sevenZip = Resolve7zExecutable();
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
            TrySanitize(outputDir);
        }
        public static void ExtractAndAnalyze(string archivePath, string outputDir)
        /// <summary>
        /// Extracts an archive then runs firmware analysis.
        /// </summary>
        public static void ExtractAndAnalyze(string archivePath, string outputDir)
            ExtractArchive(archivePath, outputDir);
            ExtractArchive(archivePath, outputDir);
            FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, outputDir);
        }

        /// <summary>
        private static void TrySanitize(string outputDir)
        /// </summary>
            try { SanitizeExtraction(outputDir); } catch { }
        }

        private static void SanitizeExtraction(string outputDir)
        {
            string root = Path.GetFullPath(outputDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!Directory.Exists(outputDir)) return;
            string parent = Path.GetDirectoryName(root);
            if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent)) return;
            foreach (var file in Directory.GetFiles(parent, "*", SearchOption.AllDirectories))
            {
                string full = Path.GetFullPath(file);
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    File.Delete(full);
            }
        }
            }
        /// <summary>
        /// Locates 7z.exe via common install paths or PATH.
        /// </summary>
        private static string Resolve7zExecutable()
        }
            string[] locations = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
            };
            foreach (string loc in locations)
                if (File.Exists(loc)) return loc;
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (string dir in pathEnv.Split(';'))
                {
                    string exe = Path.Combine(dir.Trim(), "7z.exe");
                    if (File.Exists(exe)) return exe;
                }
            }
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
            /// <summary>
        /// <summary>
        /// Splits an MBR raw disk image into partition .bin files.
        /// </summary>
        private static void ExtractPartitions(string imagePath, string outputDir)
        {
            byte[] data = File.ReadAllBytes(imagePath);
            if (data.Length < 512 || data[510] != 0x55 || data[511] != 0xAA)
                throw new InvalidOperationException("Invalid MBR disk image.");
            for (int i = 0; i < 4; i++)
            {
                int offset = 0x1BE + i * 16;
                byte type = data[offset + 4];
                uint lba = BitConverter.ToUInt32(data, offset + 8);
                uint sectors = BitConverter.ToUInt32(data, offset + 12);
                if (type == 0 || sectors == 0) continue;
                long start = lba * 512L;
                long len = sectors * 512L;
                if (start + len > data.LongLength) len = data.LongLength - start;
                string fileName = Path.Combine(outputDir, $"partition{i+1}_type{type:X2}.bin");
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using var ms = new MemoryStream(data, (int)start, (int)len);
                using var fs = File.Create(fileName);
                ms.CopyTo(fs);
            }
        }
            {
        
            /// <summary>
            /// Reads an MBR disk image and writes each non-empty partition to its own .bin file.
            /// </summary>
            private static void ExtractPartitions(string imagePath, string outputDir)
            {
                byte[] data = File.ReadAllBytes(imagePath);
                // Verify MBR signature
                if (data.Length < 512 || data[510] != 0x55 || data[511] != 0xAA)
                    throw new InvalidOperationException("Invalid MBR disk image.");
                // Four partition entries at offset 0x1BE
                for (int i = 0; i < 4; i++)
                {
                    int entry = 0x1BE + i * 16;
                    byte type = data[entry + 4];
                    uint lba = BitConverter.ToUInt32(data, entry + 8);
                    uint sectors = BitConverter.ToUInt32(data, entry + 12);
                    if (type == 0 || sectors == 0)
                        continue;
                    long start = lba * 512L;
                    long length = sectors * 512L;
                    if (start + length > data.LongLength)
                        length = data.LongLength - start;
                    string fileName = Path.Combine(outputDir, $"partition{i + 1}_type{type:X2}.bin");
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    using var fs = File.Create(fileName);
                    fs.Write(data, (int)start, (int)length);
                }
            }
                // Check common Program Files locations
                string[] candidates = new[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
                };
                foreach (var path in candidates)
                {
                    if (File.Exists(path)) return path;
                }
                // Search in PATH
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
}
