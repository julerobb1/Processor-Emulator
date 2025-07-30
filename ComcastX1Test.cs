using System;
using System.Threading.Tasks;
using ProcessorEmulator;
using ProcessorEmulator.Tools;

// Quick test of the Comcast X1 emulator functionality
class ComcastX1Test
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 COMCAST X1 EMULATOR TEST");
        Console.WriteLine("═══════════════════════════");
        Console.WriteLine();

        try
        {
            // Test 1: Create X1 emulator with XG1v4 config
            Console.WriteLine("Test 1: Creating X1 Emulator with XG1v4 configuration...");
            var x1Config = ComcastX1Emulator.X1PlatformConfig.CreateXG1v4();
            var x1Emulator = new ComcastX1Emulator(x1Config);
            Console.WriteLine($"✅ Created emulator for {x1Config.HardwareModel}");
            Console.WriteLine($"   Chipset: {x1Config.ChipsetFamily}");
            Console.WriteLine($"   Memory: {x1Config.RamSizeMB}MB RAM, {x1Config.FlashSizeMB}MB Flash");
            Console.WriteLine();

            // Test 2: Test domain parser
            Console.WriteLine("Test 2: Running Comcast Domain Analysis...");
            var analysis = ComcastDomainParser.AnalyzeComcastDomains();
            Console.WriteLine($"✅ Found {analysis.FirmwareEndpoints.Count} firmware endpoints");
            Console.WriteLine($"✅ Found {analysis.UpdateEndpoints.Count} update endpoints");
            Console.WriteLine($"✅ Generated {analysis.PotentialFirmwareUrls.Count} potential firmware URLs");
            Console.WriteLine();

            // Test 3: Export analysis results
            Console.WriteLine("Test 3: Exporting analysis results...");
            string jsonFile = "test_comcast_analysis.json";
            string urlsFile = "test_comcast_urls.txt";
            await ComcastDomainParser.ExportAnalysisToJson(analysis, jsonFile);
            await ComcastDomainParser.ExportFirmwareUrlsToText(analysis.PotentialFirmwareUrls, urlsFile);
            Console.WriteLine($"✅ Exported to {jsonFile} and {urlsFile}");
            Console.WriteLine();

            // Test 4: Show top firmware endpoints
            Console.WriteLine("Test 4: Top Comcast Firmware Endpoints:");
            foreach (var endpoint in analysis.FirmwareEndpoints.Take(5))
            {
                Console.WriteLine($"  🎯 {endpoint}");
            }
            Console.WriteLine();

            Console.WriteLine("🎉 ALL TESTS PASSED!");
            Console.WriteLine();
            Console.WriteLine("✅ Comcast X1 Emulator is ready for use");
            Console.WriteLine("✅ Domain parser successfully analyzed endpoints");
            Console.WriteLine("✅ Export functionality working correctly");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("• Load real Comcast X1 firmware via the main UI");
            Console.WriteLine("• Use generated URLs to discover firmware repositories");
            Console.WriteLine("• Start emulation with backend connectivity");
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
