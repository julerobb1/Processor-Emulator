using System.Diagnostics;

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
                    Arguments = $"x \"{archivePath}\" -o\"{outputDir}\" -y",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public static void ExtractAndAnalyze(string archivePath, string outputDir)
        {
            ExtractArchive(archivePath, outputDir);
            FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, outputDir);
        }
    }
}
