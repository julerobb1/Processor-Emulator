// You must cut down the mightiest tree in the forest with a herring!
using System;
using System.IO;

namespace ProcessorEmulator
{
    internal static class Program
    {
        /// <summary>
        /// Command-line entry point: extracts raw images and archives.
        /// Usage: ProcessorEmulator.exe <inputFile> <outputDirectory>
        /// </summary>
        private static int Main(string[] args)
        {
            // CLI supports two modes: extract and analyze
            // Usage:
            //   ProcessorEmulator.exe extract <inputFile> <outputDirectory>
            //   ProcessorEmulator.exe analyze <inputFile>
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  ProcessorEmulator.exe extract <inputFile> <outputDirectory>");
                Console.WriteLine("  ProcessorEmulator.exe analyze <inputFile>");
                return 1;
            }
            var cmd = args[0].ToLowerInvariant();
            if (cmd == "analyze")
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: ProcessorEmulator.exe analyze <inputFile>");
                    return 1;
                }
                try
                {
                    ArchiveExtractor.AnalyzeArchive(args[1]);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Analysis failed: {ex.Message}");
                    return 1;
                }
            }
            else if (cmd == "extract")
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: ProcessorEmulator.exe extract <inputFile> <outputDirectory>");
                    return 1;
                }
                var input = args[1];
                var outputDir = args[2];
                if (!File.Exists(input))
                {
                    Console.WriteLine($"Error: input file not found: {input}");
                    return 1;
                }
                try
                {
                    ArchiveExtractor.ExtractArchive(input, outputDir);
                    Console.WriteLine("Extraction complete.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Extraction failed: {ex.Message}");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine($"Unknown command: {cmd}");
                return 1;
            }

        }
    }
}
