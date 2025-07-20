using System;
using System.IO;
using System.Windows;

namespace ProcessorEmulator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // If CLI args provided, run extract/analyze logic instead of WPF UI
            if (e.Args.Length > 0)
            {
                int exitCode = 0;
                try
                {
                    var args = e.Args;
                    var cmd = args[0].ToLowerInvariant();
                    switch (cmd)
                    {
                        case "analyze":
                            if (args.Length != 2)
                                throw new ArgumentException("Usage: analyze <inputFile>");
                            ArchiveExtractor.AnalyzeArchive(args[1]);
                            break;
                        case "extract":
                            if (args.Length != 3)
                                throw new ArgumentException("Usage: extract <inputFile> <outputDirectory>");
                            var input = args[1];
                            var outputDir = args[2];
                            if (!File.Exists(input))
                                throw new FileNotFoundException(input);
                            ArchiveExtractor.ExtractArchive(input, outputDir);
                            Console.WriteLine("Extraction complete.");
                            break;
                        default:
                            throw new ArgumentException($"Unknown command: {cmd}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    exitCode = 1;
                }
                // Exit immediately with code
                Environment.Exit(exitCode);
                return;
            }
            // Otherwise start normal WPF UI
            base.OnStartup(e);
        }
    }
}