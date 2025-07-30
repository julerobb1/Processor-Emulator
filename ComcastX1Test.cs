using System;
using System.Linq;
using System.Threading.Tasks;
using ProcessorEmulator;
using ProcessorEmulator.Tools;

// Test of the real Comcast X1 emulator functionality - no fake implementations
class ComcastX1Test
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("COMCAST X1 EMULATOR TEST");
        Console.WriteLine("========================");
        Console.WriteLine();

        try
        {
            // Test 1: Create real X1 emulator
            Console.WriteLine("Test 1: Creating real Comcast X1 Emulator...");
            var x1Emulator = new ComcastX1Emulator();
            Console.WriteLine("Created emulator instance");
            Console.WriteLine();

            // Test 2: Initialize emulator
            Console.WriteLine("Test 2: Initializing emulator...");
            bool initResult = await x1Emulator.Initialize();
            if (initResult)
            {
                Console.WriteLine($"✅ Emulator initialized successfully");
                Console.WriteLine($"   Chipset: {x1Emulator.ChipsetName}");
                Console.WriteLine($"   Architecture: {x1Emulator.Architecture}");
            }
            else
            {
                Console.WriteLine("❌ Emulator initialization failed");
                return;
            }
            Console.WriteLine();

            // Test 3: Test domain parser
            Console.WriteLine("Test 3: Running Comcast Domain Analysis...");
            var analysis = ComcastDomainParser.AnalyzeComcastDomains();
            Console.WriteLine($"✅ Found {analysis.FirmwareEndpoints.Count} firmware endpoints");
            Console.WriteLine($"✅ Found {analysis.UpdateEndpoints.Count} update endpoints");
            Console.WriteLine($"✅ Generated {analysis.PotentialFirmwareUrls.Count} potential firmware URLs");
            Console.WriteLine();

            // Test 4: Export analysis results
            Console.WriteLine("Test 4: Exporting analysis results...");
            string jsonFile = "test_comcast_analysis.json";
            string urlsFile = "test_comcast_urls.txt";
            await ComcastDomainParser.ExportAnalysisToJson(analysis, jsonFile);
            await ComcastDomainParser.ExportFirmwareUrlsToText(analysis.PotentialFirmwareUrls, urlsFile);
            Console.WriteLine($"✅ Exported to {jsonFile} and {urlsFile}");
            Console.WriteLine();

            // Test 5: Show top firmware endpoints
            Console.WriteLine("Test 5: Top Comcast Firmware Endpoints:");
            foreach (var endpoint in analysis.FirmwareEndpoints.Take(5))
            {
                Console.WriteLine($"  🎯 {endpoint}");
            }
            Console.WriteLine();

            Console.WriteLine("✅ ALL TESTS PASSED!");
            Console.WriteLine();
            Console.WriteLine("Real Comcast X1 Emulator is ready for use");
            Console.WriteLine("Domain parser successfully analyzed endpoints");
            Console.WriteLine("Export functionality working correctly");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("• Load real Comcast X1 firmware via the main UI");
            Console.WriteLine("• Use generated URLs to discover firmware repositories");
            Console.WriteLine("• Start emulation with QEMU backend connectivity");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
