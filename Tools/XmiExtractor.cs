using System;
using System.IO;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Extracts Mediaroom XMI packages via signature analysis, partition carving, and recursive unpacking.
    /// </summary>
    public static class XmiExtractor
    {
        public static void ExtractXmi(string xmiPath, string outputDir)
        {
            Console.WriteLine($"[XmiExtractor] Starting XMI container scan: {xmiPath}");
            Directory.CreateDirectory(outputDir);

            // Analyze container for known signatures
            ArchiveExtractor.AnalyzeArchive(xmiPath);

            // Extract partitions (if any) and firmware sections
            var partsDir = Path.Combine(outputDir, "partitions");
            ArchiveExtractor.ExtractPartitions(xmiPath, partsDir);

            var sectionsDir = Path.Combine(outputDir, "sections");
            ArchiveExtractor.ExtractFirmwareSections(xmiPath, sectionsDir);

            // Recursively unpack any embedded archives or firmware blobs
            foreach (var file in Directory.GetFiles(sectionsDir))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var subOut = Path.Combine(sectionsDir, name);
                ArchiveExtractor.ExtractArchive(file, subOut);
            }

            Console.WriteLine($"[XmiExtractor] XMI extraction complete. Output at: {outputDir}");
        }
    }
}
