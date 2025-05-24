using System;
using System.IO;

namespace ProcessorEmulator.Tools
{
    public static class FirmwareAnalyzer
    {
        public static void AnalyzeFirmwareArchive(string archivePath, string extractDir)
        {
            Console.WriteLine($"Extracting {archivePath} to {extractDir}...");
            ArchiveExtractor.ExtractArchive(archivePath, extractDir);
            Console.WriteLine("Extraction complete. Scanning for binaries...");
            BinaryScanner.ScanTypicalBinaryDirs(extractDir);
            Console.WriteLine("Analysis complete.");
        }
    }
}
