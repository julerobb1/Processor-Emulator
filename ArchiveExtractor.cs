using System;
using System.Diagnostics;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public static class ArchiveExtractor
    {
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            // Resolve 7z executable path
            string sevenZip = Resolve7zExecutable();
            if (string.IsNullOrEmpty(sevenZip))
                throw new InvalidOperationException("7z.exe not found. Please install 7-Zip or locate 7z.exe manually.");
            // Use -spf to allow extraction of full paths
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZip,
                    Arguments = $"x -spf \"{archivePath}\" -o\"{outputDir}\" -y",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            // Remove any files extracted outside of the output directory (prevent overwriting host paths)
            SanitizeExtraction(outputDir);
        }

        public static void ExtractAndAnalyze(string archivePath, string outputDir)
        {
            ExtractArchive(archivePath, outputDir);
            // Further sanitize before analysis
            FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, outputDir);
        }

        /// <summary>
        /// Deletes any files or directories unintentionally placed outside the extraction root.
        /// </summary>
        private static void SanitizeExtraction(string outputDir)
        {
            try
            {
                var root = Path.GetFullPath(outputDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                // Check all files under the extraction parent path
                var parent = Path.GetDirectoryName(root);
                foreach (var file in Directory.GetFiles(parent, "*", SearchOption.AllDirectories))
                {
                    var full = Path.GetFullPath(file);
                    if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(full);
                    }
                }
                // Optionally, similar cleanup for directories can be added
            }
            catch { /* Ignore cleanup errors */ }
        }
        
            /// <summary>
            /// Locates 7z.exe via PATH, common install folders, or user prompt.
            /// </summary>
            private static string Resolve7zExecutable()
            {
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
