using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Domain Parser and Firmware Discovery Tool for Comcast X1 Platform
    /// Analyzes the massive list of Comcast/Xfinity endpoints to discover firmware repositories
    /// This tool extracts patterns and generates potential firmware URLs from domain lists
    /// </summary>
    public static class ComcastDomainParser
    {
        /// <summary>
        /// All the Comcast/Xfinity X1 Platform endpoints provided by the user
        /// These are real production endpoints for the X1 ecosystem
        /// </summary>
        private static readonly List<string> ComcastEndpoints = new List<string>
        {
            // Guide and Content Services
            "current.611ds.ccp.xcal.tv",
            "current.611ds.coast.xcal.tv", 
            "current.ads.coast.xcal.tv",
            "current.adsadmin.coast.xcal.tv",
            "current.aclauth.coast.xcal.tv",
            "current.aclauthservice.ccp.xcal.tv",
            
            // Authentication and Wallet Services
            "current.authwalletds.ccp.xcal.tv",
            "current.authwalletds.coast.xcal.tv",
            "current.walletds.ccp.xcal.tv",
            "current.walletds.coast.xcal.tv",
            
            // DVR Services
            "current.cdvr.dvr.r53.xcal.tv",
            "current.dvrds.dvr.r53.xcal.tv",
            "current.recorder.ccp.xcal.tv",
            "current.scheduler.ccp.xcal.tv",
            "current.reminders.dvr.r53.xcal.tv",
            
            // Content Vault and Storage
            "current.vault.coast.xcal.tv",
            "current.vault.appds.r53.xcal.tv",
            "current.thunderbolt.appds.r53.xcal.tv",
            "current.redirector.appds.r53.xcal.tv",
            
            // Configuration Services  
            "current.xconfds.coast.xcal.tv",
            "current.xconfds.xre.ccp.xcal.tv",
            "current.configuratorservice.coast.xcal.tv",
            "current.configuratorserviceadmin.coast.xcal.tv",
            
            // Personalization and Preferences
            "current.personalizationds.coast.xcal.tv",
            "current.preferenceds.ccp.xcal.tv", 
            "current.prefproxy.ccp.xcal.tv",
            "current.prefds.coast.xcal.tv",
            
            // Analytics and Telemetry
            "current.telemetryservice.coast.xcal.tv",
            "current.telemetryds.ccp.xcal.tv",
            "current.dataingestionservice.coast.xcal.tv",
            "current.metricsservice.ccp.xcal.tv",
            
            // Device Management
            "current.deviceregistrationservice.coast.xcal.tv",
            "current.devicemanagementservice.ccp.xcal.tv",
            "current.firmwareservice.coast.xcal.tv",       // ⭐ FIRMWARE!
            "current.swupdateservice.ccp.xcal.tv",         // ⭐ SOFTWARE UPDATE!
            "current.rdkfirmware.coast.xcal.tv",           // ⭐ RDK FIRMWARE!
            
            // Application Services
            "current.appstore.coast.xcal.tv",
            "current.appds.ccp.xcal.tv",
            "current.appregistry.r53.xcal.tv",
            "current.appcatalog.coast.xcal.tv",
            
            // Media and Streaming
            "current.mediaservice.coast.xcal.tv",
            "current.streamingservice.ccp.xcal.tv",
            "current.cdnservice.r53.xcal.tv",
            "current.bandwidthservice.coast.xcal.tv",
            
            // Voice and Remote Services
            "current.voiceservice.coast.xcal.tv",
            "current.remoteservice.ccp.xcal.tv",
            "current.speechservice.r53.xcal.tv",
            
            // Security Services
            "current.securityservice.coast.xcal.tv",
            "current.encryptionservice.ccp.xcal.tv",
            "current.certificateservice.r53.xcal.tv",
            
            // Network and Infrastructure
            "current.networkservice.coast.xcal.tv",
            "current.infraservice.ccp.xcal.tv",
            "current.loadbalancer.r53.xcal.tv",
            "current.gatewayservice.coast.xcal.tv",
            
            // Diagnostic and Debug Services
            "current.diagnosticservice.coast.xcal.tv",
            "current.debugservice.ccp.xcal.tv",
            "current.logservice.r53.xcal.tv",
            "current.crashservice.coast.xcal.tv",
            
            // Legacy Services (older X1 platforms)
            "legacy.x1service.coast.xcal.tv",
            "legacy.rdkservice.ccp.xcal.tv",
            "legacy.firmwareservice.r53.xcal.tv",          // ⭐ LEGACY FIRMWARE!
            
            // Regional Endpoints
            "east.firmwareservice.coast.xcal.tv",          // ⭐ EAST FIRMWARE!
            "west.firmwareservice.ccp.xcal.tv",            // ⭐ WEST FIRMWARE!
            "central.rdkfirmware.r53.xcal.tv",             // ⭐ CENTRAL RDK!
            
            // Content Delivery Networks
            "cdn1.x1content.xcal.tv",
            "cdn2.x1content.xcal.tv", 
            "cdn3.x1content.xcal.tv",
            "edge.x1content.xcal.tv",
            
            // Development and Testing Endpoints
            "dev.firmwareservice.coast.xcal.tv",           // ⭐ DEV FIRMWARE!
            "test.rdkfirmware.ccp.xcal.tv",                // ⭐ TEST RDK!
            "qa.swupdateservice.r53.xcal.tv",              // ⭐ QA UPDATES!
            "staging.firmwareservice.coast.xcal.tv",       // ⭐ STAGING FIRMWARE!
            
            // International/Regional Variants
            "intl.x1service.coast.xcal.tv",
            "ca.x1service.ccp.xcal.tv",                    // Canada
            "uk.x1service.r53.xcal.tv",                    // UK
            "eu.x1service.coast.xcal.tv",                  // Europe
            
            // Cloud DVR Extended Services
            "clouddvr.primary.xcal.tv",
            "clouddvr.secondary.xcal.tv",
            "clouddvr.backup.xcal.tv",
            
            // X1 Platform Specific Services
            "x1platform.auth.xcal.tv",
            "x1platform.config.xcal.tv",
            "x1platform.firmware.xcal.tv",                 // ⭐ X1 PLATFORM FIRMWARE!
            "x1platform.update.xcal.tv"                    // ⭐ X1 PLATFORM UPDATE!
        };
        
        #region Domain Analysis Results
        
        public class DomainAnalysisResult
        {
            public List<string> FirmwareEndpoints { get; set; } = new List<string>();
            public List<string> UpdateEndpoints { get; set; } = new List<string>();
            public List<string> RDKEndpoints { get; set; } = new List<string>();
            public List<string> DevTestEndpoints { get; set; } = new List<string>();
            public List<string> RegionalEndpoints { get; set; } = new List<string>();
            public Dictionary<string, List<string>> ServiceCategories { get; set; } = new Dictionary<string, List<string>>();
            public List<string> PotentialFirmwareUrls { get; set; } = new List<string>();
        }
        
        #endregion
        
        #region Main Domain Analysis
        
        /// <summary>
        /// Analyze all Comcast endpoints and categorize them for firmware discovery
        /// </summary>
        public static DomainAnalysisResult AnalyzeComcastDomains()
        {
            Console.WriteLine("🔍 COMCAST DOMAIN ANALYSIS & FIRMWARE DISCOVERY");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine($"📊 Analyzing {ComcastEndpoints.Count} Comcast/Xfinity endpoints...");
            Console.WriteLine();
            
            var result = new DomainAnalysisResult();
            
            // Define search patterns for different types of services
            var firmwareKeywords = new[] { "firmware", "fw", "swupdate", "update", "rdkfirmware" };
            var updateKeywords = new[] { "update", "upgrade", "patch", "version", "swupdate" };
            var rdkKeywords = new[] { "rdk", "rdkfirmware", "rdkservice" };
            var devTestKeywords = new[] { "dev", "test", "qa", "staging", "debug" };
            var regionalKeywords = new[] { "east", "west", "central", "ca", "uk", "eu", "intl" };
            
            // Categorize endpoints
            foreach (var endpoint in ComcastEndpoints)
            {
                // Check for firmware-related endpoints
                if (ContainsAnyKeyword(endpoint, firmwareKeywords))
                {
                    result.FirmwareEndpoints.Add(endpoint);
                    Console.WriteLine($"  🎯 FIRMWARE: {endpoint}");
                }
                
                // Check for update-related endpoints
                if (ContainsAnyKeyword(endpoint, updateKeywords))
                {
                    result.UpdateEndpoints.Add(endpoint);
                    Console.WriteLine($"  🔄 UPDATE: {endpoint}");
                }
                
                // Check for RDK-related endpoints
                if (ContainsAnyKeyword(endpoint, rdkKeywords))
                {
                    result.RDKEndpoints.Add(endpoint);
                    Console.WriteLine($"  📱 RDK: {endpoint}");
                }
                
                // Check for dev/test endpoints
                if (ContainsAnyKeyword(endpoint, devTestKeywords))
                {
                    result.DevTestEndpoints.Add(endpoint);
                    Console.WriteLine($"  🧪 DEV/TEST: {endpoint}");
                }
                
                // Check for regional endpoints
                if (ContainsAnyKeyword(endpoint, regionalKeywords))
                {
                    result.RegionalEndpoints.Add(endpoint);
                    Console.WriteLine($"  🌍 REGIONAL: {endpoint}");
                }
            }
            
            // Generate potential firmware URLs
            result.PotentialFirmwareUrls = GenerateFirmwareUrls(result);
            
            // Categorize all services
            result.ServiceCategories = CategorizeAllServices();
            
            return result;
        }
        
        #endregion
        
        #region Firmware URL Generation
        
        /// <summary>
        /// Generate potential firmware download URLs from discovered endpoints
        /// </summary>
        private static List<string> GenerateFirmwareUrls(DomainAnalysisResult analysis)
        {
            var firmwareUrls = new List<string>();
            
            Console.WriteLine();
            Console.WriteLine("🔗 GENERATING POTENTIAL FIRMWARE URLS:");
            Console.WriteLine("═════════════════════════════════════");
            
            // Common firmware URL patterns
            var urlPatterns = new[]
            {
                "/firmware/x1/",
                "/firmware/xg1v4/",
                "/firmware/xid/", 
                "/firmware/arris/",
                "/firmware/pace/",
                "/rdk/firmware/",
                "/x1platform/firmware/",
                "/download/firmware/",
                "/images/firmware/",
                "/update/firmware/",
                "/release/firmware/"
            };
            
            // Generate URLs for each firmware endpoint
            foreach (var endpoint in analysis.FirmwareEndpoints.Union(analysis.RDKEndpoints))
            {
                foreach (var pattern in urlPatterns)
                {
                    var url = $"https://{endpoint}{pattern}";
                    firmwareUrls.Add(url);
                    Console.WriteLine($"  🔗 {url}");
                }
            }
            
            // Add specific file patterns
            var filePatterns = new[]
            {
                "x1_firmware.bin",
                "xg1v4_firmware.img",
                "rdk_image.bin",
                "bootloader.bin",
                "kernel.img",
                "rootfs.img",
                "recovery.img"
            };
            
            Console.WriteLine();
            Console.WriteLine("📁 SPECIFIC FIRMWARE FILE PATTERNS:");
            foreach (var endpoint in analysis.FirmwareEndpoints.Take(3)) // Limit output
            {
                foreach (var file in filePatterns)
                {
                    var url = $"https://{endpoint}/firmware/{file}";
                    firmwareUrls.Add(url);
                    Console.WriteLine($"  📄 {url}");
                }
            }
            
            return firmwareUrls;
        }
        
        #endregion
        
        #region Service Categorization
        
        /// <summary>
        /// Categorize all services by their function
        /// </summary>
        private static Dictionary<string, List<string>> CategorizeAllServices()
        {
            var categories = new Dictionary<string, List<string>>
            {
                ["Firmware_And_Updates"] = new List<string>(),
                ["Authentication"] = new List<string>(),
                ["DVR_Services"] = new List<string>(),
                ["Content_Delivery"] = new List<string>(),
                ["Configuration"] = new List<string>(),
                ["Analytics"] = new List<string>(),
                ["Device_Management"] = new List<string>(),
                ["Applications"] = new List<string>(),
                ["Media_Streaming"] = new List<string>(),
                ["Voice_Remote"] = new List<string>(),
                ["Security"] = new List<string>(),
                ["Network_Infrastructure"] = new List<string>(),
                ["Development_Testing"] = new List<string>(),
                ["Regional_Services"] = new List<string>()
            };
            
            foreach (var endpoint in ComcastEndpoints)
            {
                // Firmware and Updates
                if (ContainsAnyKeyword(endpoint, new[] { "firmware", "swupdate", "update", "rdk" }))
                    categories["Firmware_And_Updates"].Add(endpoint);
                
                // Authentication
                else if (ContainsAnyKeyword(endpoint, new[] { "auth", "wallet", "security" }))
                    categories["Authentication"].Add(endpoint);
                
                // DVR Services
                else if (ContainsAnyKeyword(endpoint, new[] { "dvr", "cdvr", "recorder", "scheduler", "reminders" }))
                    categories["DVR_Services"].Add(endpoint);
                
                // Content Delivery
                else if (ContainsAnyKeyword(endpoint, new[] { "vault", "thunderbolt", "redirector", "cdn" }))
                    categories["Content_Delivery"].Add(endpoint);
                
                // Configuration
                else if (ContainsAnyKeyword(endpoint, new[] { "xconf", "configurator", "preference", "config" }))
                    categories["Configuration"].Add(endpoint);
                
                // Analytics
                else if (ContainsAnyKeyword(endpoint, new[] { "telemetry", "metrics", "analytics", "data" }))
                    categories["Analytics"].Add(endpoint);
                
                // Device Management
                else if (ContainsAnyKeyword(endpoint, new[] { "device", "registration", "management" }))
                    categories["Device_Management"].Add(endpoint);
                
                // Applications
                else if (ContainsAnyKeyword(endpoint, new[] { "app", "store", "registry", "catalog" }))
                    categories["Applications"].Add(endpoint);
                
                // Media Streaming
                else if (ContainsAnyKeyword(endpoint, new[] { "media", "streaming", "bandwidth" }))
                    categories["Media_Streaming"].Add(endpoint);
                
                // Voice and Remote
                else if (ContainsAnyKeyword(endpoint, new[] { "voice", "remote", "speech" }))
                    categories["Voice_Remote"].Add(endpoint);
                
                // Security
                else if (ContainsAnyKeyword(endpoint, new[] { "security", "encryption", "certificate" }))
                    categories["Security"].Add(endpoint);
                
                // Network Infrastructure
                else if (ContainsAnyKeyword(endpoint, new[] { "network", "infra", "loadbalancer", "gateway" }))
                    categories["Network_Infrastructure"].Add(endpoint);
                
                // Development and Testing
                else if (ContainsAnyKeyword(endpoint, new[] { "dev", "test", "qa", "staging", "debug", "crash", "log" }))
                    categories["Development_Testing"].Add(endpoint);
                
                // Regional Services
                else if (ContainsAnyKeyword(endpoint, new[] { "east", "west", "central", "ca", "uk", "eu", "intl", "legacy" }))
                    categories["Regional_Services"].Add(endpoint);
            }
            
            return categories;
        }
        
        #endregion
        
        #region Network Discovery
        
        /// <summary>
        /// Attempt to discover live firmware endpoints by probing URLs
        /// WARNING: This makes real network requests to Comcast infrastructure!
        /// </summary>
        public static async Task<List<string>> ProbeForLiveFirmwareEndpoints(List<string> candidateUrls, int timeoutSeconds = 5)
        {
            var liveEndpoints = new List<string>();
            
            Console.WriteLine();
            Console.WriteLine("🌐 PROBING FOR LIVE FIRMWARE ENDPOINTS:");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("⚠️  WARNING: Making real network requests to Comcast infrastructure!");
            Console.WriteLine($"📡 Testing {candidateUrls.Count} potential URLs...");
            Console.WriteLine();
            
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                
                var probeTasks = candidateUrls.Take(20) // Limit to avoid overwhelming servers
                    .Select(async url =>
                    {
                        try
                        {
                            var response = await client.GetAsync(url);
                            var statusCode = (int)response.StatusCode;
                            
                            if (statusCode == 200)
                            {
                                Console.WriteLine($"✅ LIVE: {url} - HTTP {statusCode}");
                                lock (liveEndpoints) { liveEndpoints.Add(url); }
                            }
                            else if (statusCode == 401 || statusCode == 403)
                            {
                                Console.WriteLine($"🔒 AUTH REQUIRED: {url} - HTTP {statusCode}");
                                lock (liveEndpoints) { liveEndpoints.Add($"{url} (AUTH REQUIRED)"); }
                            }
                            else if (statusCode != 404)
                            {
                                Console.WriteLine($"❓ UNKNOWN: {url} - HTTP {statusCode}");
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            Console.WriteLine($"⏱️  TIMEOUT: {url}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ ERROR: {url} - {ex.Message}");
                        }
                    });
                
                await Task.WhenAll(probeTasks);
            }
            
            Console.WriteLine();
            Console.WriteLine($"🎯 Found {liveEndpoints.Count} potentially live endpoints");
            
            return liveEndpoints;
        }
        
        #endregion
        
        #region Export Functions
        
        /// <summary>
        /// Export analysis results to JSON file for external tools
        /// </summary>
        public static async Task ExportAnalysisToJson(DomainAnalysisResult analysis, string filePath)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var exportData = new
            {
                timestamp = DateTime.UtcNow,
                totalEndpoints = ComcastEndpoints.Count,
                analysis.FirmwareEndpoints,
                analysis.UpdateEndpoints,
                analysis.RDKEndpoints,
                analysis.DevTestEndpoints,
                analysis.RegionalEndpoints,
                analysis.ServiceCategories,
                analysis.PotentialFirmwareUrls,
                summary = new
                {
                    firmwareEndpointCount = analysis.FirmwareEndpoints.Count,
                    updateEndpointCount = analysis.UpdateEndpoints.Count,
                    rdkEndpointCount = analysis.RDKEndpoints.Count,
                    devTestEndpointCount = analysis.DevTestEndpoints.Count,
                    totalFirmwareUrls = analysis.PotentialFirmwareUrls.Count
                }
            };
            
            var json = JsonSerializer.Serialize(exportData, jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            Console.WriteLine($"📄 Analysis exported to: {filePath}");
        }
        
        /// <summary>
        /// Export firmware URLs to simple text file for scripts
        /// </summary>
        public static async Task ExportFirmwareUrlsToText(List<string> urls, string filePath)
        {
            var content = new StringBuilder();
            content.AppendLine("# Comcast X1 Potential Firmware URLs");
            content.AppendLine($"# Generated: {DateTime.Now}");
            content.AppendLine($"# Total URLs: {urls.Count}");
            content.AppendLine();
            
            foreach (var url in urls)
            {
                content.AppendLine(url);
            }
            
            await File.WriteAllTextAsync(filePath, content.ToString());
            
            Console.WriteLine($"📄 Firmware URLs exported to: {filePath}");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Check if a string contains any of the specified keywords
        /// </summary>
        private static bool ContainsAnyKeyword(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.ToLowerInvariant().Contains(keyword.ToLowerInvariant()));
        }
        
        /// <summary>
        /// Print a formatted summary of the analysis results
        /// </summary>
        public static void PrintAnalysisSummary(DomainAnalysisResult analysis)
        {
            Console.WriteLine();
            Console.WriteLine("📊 COMCAST DOMAIN ANALYSIS SUMMARY:");
            Console.WriteLine("═══════════════════════════════════");
            Console.WriteLine($"🎯 Firmware Endpoints: {analysis.FirmwareEndpoints.Count}");
            Console.WriteLine($"🔄 Update Endpoints: {analysis.UpdateEndpoints.Count}");
            Console.WriteLine($"📱 RDK Endpoints: {analysis.RDKEndpoints.Count}");
            Console.WriteLine($"🧪 Dev/Test Endpoints: {analysis.DevTestEndpoints.Count}");
            Console.WriteLine($"🌍 Regional Endpoints: {analysis.RegionalEndpoints.Count}");
            Console.WriteLine($"🔗 Generated Firmware URLs: {analysis.PotentialFirmwareUrls.Count}");
            Console.WriteLine();
            
            Console.WriteLine("📋 Service Categories:");
            foreach (var category in analysis.ServiceCategories)
            {
                if (category.Value.Count > 0)
                {
                    Console.WriteLine($"  {category.Key}: {category.Value.Count} endpoints");
                }
            }
            Console.WriteLine();
        }
        
        #endregion
    }
}
