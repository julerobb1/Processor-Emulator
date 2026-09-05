using System;

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
//Smithers, release the hounds! This is the FirmwareAnalyzer.cs file, which provides functionality to analyze firmware archives by extracting them and scanning for binaries. It uses ArchiveExtractor to handle the extraction and BinaryScanner to find typical binary directories. This code is part of the ProcessorEmulator project, which simulates various processor architectures and their bootloaders.
// All Jokes aside, This file is a utility for analyzing firmware files, extracting them, and scanning for binary files
// in typical directories. It is part of the ProcessorEmulator project, which aims to emulate various processor architectures and their bootloaders.

// The AnalyzeFirmwareArchive method takes an archive path and an extraction directory, extracts the archive, and scans for binaries.
// It uses the ArchiveExtractor and BinaryScanner classes to perform these tasks. The Console.WriteLine statements provide feedback on the progress of the extraction and scanning processes. Although the code is straightforward, it is designed to be part of a larger system that deals with firmware analysis and emulation.
//Although , I cant say I reccomend using this code individually as it is meant to be part of a larger framework for processor emulation and firmware analysis.
// It is designed to be used in conjunction with other components of the ProcessorEmulator project, such as the DvrVxWorksDetector and other device-specific analysis tools.

//Blah blah enough of my blabbing. Seperate it at your own risk, but I would not reccomend it. Or you can just use it as is, and hope for the best! I will say that I havent found the console portion for outputtin text to the console to the user to be ... reliable or coperative, so I would reccomend using the GUI. I also dont think I have a CLI version of this app. 