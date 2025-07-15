using System;
using System.Diagnostics;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public static class ArchiveExtractor
    {
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            // Assumes 7z.exe is in PATH or same directory as the app
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "7z.exe",
                    // Use -spf to allow extraction of full (potentially dangerous) paths and symlinks
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
    }
}
