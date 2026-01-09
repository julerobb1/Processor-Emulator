using System;
using System.Linq;
using System.Threading.Tasks;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Demo program to test Comcast domain parsing functionality
    /// This demonstrates how to use the ComcastDomainParser for firmware discovery
    /// </summary>
    public static class ComcastDomainParserDemo
    {
        /// <summary>
        /// Run the complete Comcast domain analysis demo
        /// </summary>
        public static async Task RunDemo()
        {
            Console.WriteLine("🚀 COMCAST X1 DOMAIN PARSER DEMO");
            Console.WriteLine("═════════════════════════════════");
            Console.WriteLine("This tool analyzes Comcast/Xfinity endpoints to discover firmware locations");
            Console.WriteLine();

            try
            {
                // Step 1: Analyze all domains
                Console.WriteLine("Step 1: Analyzing Comcast domains...");
                var analysis = ComcastDomainParser.AnalyzeComcastDomains();

                // Step 2: Print summary
                ComcastDomainParser.PrintAnalysisSummary(analysis);

                // Step 3: Export results (optional)
                Console.WriteLine("💾 EXPORTING RESULTS:");
                Console.WriteLine("════════════════════");
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string jsonFile = $"comcast_analysis_{timestamp}.json";
                string urlsFile = $"comcast_firmware_urls_{timestamp}.txt";

                await ComcastDomainParser.ExportAnalysisToJson(analysis, jsonFile);
                await ComcastDomainParser.ExportFirmwareUrlsToText(analysis.PotentialFirmwareUrls, urlsFile);

                // Step 4: Optional network probing (commented out for safety)
                Console.WriteLine();
                Console.WriteLine("🌐 NETWORK PROBING OPTIONS:");
                Console.WriteLine("═══════════════════════════");
                Console.WriteLine("⚠️  Network probing is disabled by default to avoid overwhelming Comcast servers");
                Console.WriteLine("   To enable, uncomment the ProbeForLiveFirmwareEndpoints call below");
                Console.WriteLine($"   This would test {analysis.PotentialFirmwareUrls.Count} potential URLs");
                
                // Uncomment the following lines to enable live network probing:
                // Console.WriteLine("\n🔍 Probing for live endpoints...");
                // var liveEndpoints = await ComcastDomainParser.ProbeForLiveFirmwareEndpoints(analysis.PotentialFirmwareUrls);
                // Console.WriteLine($"✅ Found {liveEndpoints.Count} potentially live endpoints");

                Console.WriteLine();
                Console.WriteLine("✅ DEMO COMPLETE!");
                Console.WriteLine($"📄 Results saved to: {jsonFile} and {urlsFile}");
                Console.WriteLine();
                Console.WriteLine("🎯 NEXT STEPS:");
                Console.WriteLine("• Use the generated URLs to download firmware");
                Console.WriteLine("• Load firmware into the Comcast X1 Emulator");
                Console.WriteLine("• Analyze firmware structure and extract partitions");
                Console.WriteLine("• Start real X1 emulation with backend connectivity");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Demo failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Quick demo that just shows the key firmware endpoints
        /// </summary>
        public static void RunQuickDemo()
        {
            Console.WriteLine("🎯 QUICK COMCAST FIRMWARE DISCOVERY");
            Console.WriteLine("═══════════════════════════════════");
            
            var analysis = ComcastDomainParser.AnalyzeComcastDomains();
            
            Console.WriteLine();
            Console.WriteLine("🔥 TOP FIRMWARE ENDPOINTS:");
            foreach (var endpoint in analysis.FirmwareEndpoints.Take(10))
            {
                Console.WriteLine($"  🎯 {endpoint}");
            }
            
            Console.WriteLine();
            Console.WriteLine("🔄 TOP UPDATE ENDPOINTS:");
            foreach (var endpoint in analysis.UpdateEndpoints.Take(5))
            {
                Console.WriteLine($"  🔄 {endpoint}");
            }
            
            Console.WriteLine();
            Console.WriteLine("📱 TOP RDK ENDPOINTS:");
            foreach (var endpoint in analysis.RDKEndpoints.Take(5))
            {
                Console.WriteLine($"  📱 {endpoint}");
            }
            
            Console.WriteLine();
            Console.WriteLine($"💡 Generated {analysis.PotentialFirmwareUrls.Count} potential firmware URLs");
            Console.WriteLine("   Run full demo to see complete analysis and export results");
        }
    }
}
