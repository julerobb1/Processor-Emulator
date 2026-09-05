using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;

namespace ProcessorEmulator.Emulation.SyncEngine
{
    /// <summary>
    /// Fetches TV guide data from various sources (Pluto TV, free EPG feeds) and normalizes to XMLTV format
    /// </summary>
    public class GuideFetcher
    {
        private readonly HttpClient httpClient;
        private readonly string cacheDirectory;
        private DateTime lastSync = DateTime.MinValue;
        
        public GuideFetcher()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "RDK-V/1.0 STB Guide Client");
            cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessorEmulator", "GuideCache");
            Directory.CreateDirectory(cacheDirectory);
        }
        
        public async Task<GuideData> FetchGuideAsync()
        {
            Debug.WriteLine("[GuideFetcher] Starting guide fetch...");
            
            var guideData = new GuideData();
            
            // Fetch from multiple sources
            await FetchPlutoTVGuide(guideData);
            await FetchTizenFreeGuide(guideData);
            await FetchXMLTVSample(guideData);
            
            // Cache the results
            await CacheGuideData(guideData);
            lastSync = DateTime.Now;
            
            Debug.WriteLine($"[GuideFetcher] Guide fetch complete: {guideData.Channels.Count} channels, {guideData.Programs.Count} programs");
            return guideData;
        }
        
        private async Task FetchPlutoTVGuide(GuideData guideData)
        {
            try
            {
                Debug.WriteLine("[GuideFetcher] Fetching Pluto TV guide...");
                
                // Pluto TV API endpoints
                var channelsUrl = "https://api.pluto.tv/v2/channels?sort=name";
                
                var response = await httpClient.GetStringAsync(channelsUrl);
                
                // Parse JSON more robustly - handle API changes
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    JsonElement root = doc.RootElement;
                    
                    // Try different possible JSON structures
                    JsonElement channelsArray;
                    if (root.TryGetProperty("data", out channelsArray))
                    {
                        // Structure: { "data": [...] }
                        ParsePlutoChannels(channelsArray, guideData);
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        // Structure: [...] (direct array)
                        ParsePlutoChannels(root, guideData);
                    }
                    else
                    {
                        Debug.WriteLine("[GuideFetcher] Unknown Pluto TV API structure - using fallback");
                        AddFallbackChannels(guideData, "PlutoTV");
                    }
                }
                
                Debug.WriteLine($"[GuideFetcher] Added Pluto TV channels successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GuideFetcher] Pluto TV fetch failed: {ex.Message}");
                // Add fallback channels
                AddFallbackChannels(guideData, "PlutoTV");
            }
        }

        private void ParsePlutoChannels(JsonElement channelsArray, GuideData guideData)
        {
            int channelCount = 0;
            foreach (JsonElement channel in channelsArray.EnumerateArray())
            {
                try
                {
                    string id = channel.TryGetProperty("id", out var idProp) ? idProp.GetString() : $"pluto_{channelCount}";
                    string name = channel.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Unknown Channel";
                    int number = channel.TryGetProperty("number", out var numProp) ? numProp.GetInt32() : channelCount + 1000;
                    string category = channel.TryGetProperty("category", out var catProp) ? catProp.GetString() : "General";
                    
                    string logoUrl = null;
                    if (channel.TryGetProperty("images", out var images) && 
                        images.TryGetProperty("logo", out var logo) &&
                        logo.TryGetProperty("path", out var path))
                    {
                        logoUrl = path.GetString();
                    }

                    var guideChannel = new GuideChannel
                    {
                        Id = $"pluto_{id}",
                        Name = name,
                        Number = $"10{number:D2}", // Map to 1000+ range
                        LogoUrl = logoUrl,
                        Category = category,
                        Source = "PlutoTV"
                    };
                    
                    guideData.Channels.Add(guideChannel);
                    AddSamplePrograms(guideData, guideChannel, name);
                    channelCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GuideFetcher] Failed to parse Pluto channel: {ex.Message}");
                }
            }
            
            Debug.WriteLine($"[GuideFetcher] Added {channelCount} Pluto TV channels");
        }
        
        private async Task FetchTizenFreeGuide(GuideData guideData)
        {
            try
            {
                Debug.WriteLine("[GuideFetcher] Fetching Tizen free channels...");
                
                // Samsung Tizen free channel list (public API)
                var tizenUrl = "https://i.mjh.nz/SamsungTVPlus/all.m3u8";
                var response = await httpClient.GetStringAsync(tizenUrl);
                
                // Parse M3U format
                var lines = response.Split('\n');
                string currentName = "";
                int channelNum = 2000; // Start at 2000 range
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("#EXTINF:"))
                    {
                        // Extract channel name from #EXTINF line
                        var nameStart = line.LastIndexOf(',') + 1;
                        if (nameStart > 0 && nameStart < line.Length)
                        {
                            currentName = line.Substring(nameStart).Trim();
                        }
                    }
                    else if (line.StartsWith("http") && !string.IsNullOrEmpty(currentName))
                    {
                        var guideChannel = new GuideChannel
                        {
                            Id = $"tizen_{channelNum}",
                            Name = currentName,
                            Number = channelNum.ToString(),
                            StreamUrl = line.Trim(),
                            Category = "Free",
                            Source = "TizenTV"
                        };
                        
                        guideData.Channels.Add(guideChannel);
                        AddSamplePrograms(guideData, guideChannel, currentName);
                        
                        channelNum++;
                        currentName = "";
                    }
                }
                
                Debug.WriteLine($"[GuideFetcher] Added {channelNum - 2000} Tizen free channels");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GuideFetcher] Tizen fetch failed: {ex.Message}");
                AddFallbackChannels(guideData, "TizenTV");
            }
        }
        
        private Task FetchXMLTVSample(GuideData guideData)
        {
            try
            {
                Debug.WriteLine("[GuideFetcher] Creating XMLTV sample data...");
                
                // Add some standard cable channels that RDK-V boxes expect
                var standardChannels = new[]
                {
                    new { num = "2", name = "CBS", category = "Broadcast" },
                    new { num = "4", name = "NBC", category = "Broadcast" },
                    new { num = "7", name = "ABC", category = "Broadcast" },
                    new { num = "11", name = "FOX", category = "Broadcast" },
                    new { num = "101", name = "CNN", category = "News" },
                    new { num = "102", name = "ESPN", category = "Sports" },
                    new { num = "103", name = "Discovery", category = "Documentary" },
                    new { num = "104", name = "Comedy Central", category = "Entertainment" },
                    new { num = "105", name = "History", category = "Documentary" },
                    new { num = "106", name = "National Geographic", category = "Documentary" }
                };
                
                foreach (var ch in standardChannels)
                {
                    var guideChannel = new GuideChannel
                    {
                        Id = $"cable_{ch.num}",
                        Name = ch.name,
                        Number = ch.num,
                        Category = ch.category,
                        Source = "Cable"
                    };
                    
                    guideData.Channels.Add(guideChannel);
                    AddSamplePrograms(guideData, guideChannel, ch.name);
                }
                
                Debug.WriteLine($"[GuideFetcher] Added {standardChannels.Length} standard cable channels");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GuideFetcher] XMLTV sample creation failed: {ex.Message}");
            }
            return Task.CompletedTask;
        }
        
        private void AddSamplePrograms(GuideData guideData, GuideChannel channel, string channelName)
        {
            var now = DateTime.Now;
            var programs = new[]
            {
                new GuideProgram
                {
                    Id = $"{channel.Id}_prog1",
                    ChannelId = channel.Id,
                    Title = $"{channelName} Morning Show",
                    Description = $"Morning programming on {channelName}",
                    StartTime = now.Date.AddHours(6),
                    EndTime = now.Date.AddHours(9),
                    Category = "News"
                },
                new GuideProgram
                {
                    Id = $"{channel.Id}_prog2", 
                    ChannelId = channel.Id,
                    Title = $"{channelName} Midday",
                    Description = $"Midday content on {channelName}",
                    StartTime = now.Date.AddHours(12),
                    EndTime = now.Date.AddHours(14),
                    Category = "Entertainment"
                },
                new GuideProgram
                {
                    Id = $"{channel.Id}_prog3",
                    ChannelId = channel.Id,
                    Title = $"{channelName} Prime Time",
                    Description = $"Prime time programming on {channelName}",
                    StartTime = now.Date.AddHours(20),
                    EndTime = now.Date.AddHours(22),
                    Category = "Entertainment"
                }
            };
            
            foreach (var program in programs)
            {
                guideData.Programs.Add(program);
            }
        }
        
        private void AddFallbackChannels(GuideData guideData, string source)
        {
            // Add basic fallback channels when API fails
            var fallbackChannels = new[]
            {
                new { num = "900", name = $"{source} News", category = "News" },
                new { num = "901", name = $"{source} Movies", category = "Movies" },
                new { num = "902", name = $"{source} Sports", category = "Sports" }
            };
            
            foreach (var ch in fallbackChannels)
            {
                var guideChannel = new GuideChannel
                {
                    Id = $"{source.ToLower()}_{ch.num}",
                    Name = ch.name,
                    Number = ch.num,
                    Category = ch.category,
                    Source = source
                };
                
                guideData.Channels.Add(guideChannel);
                AddSamplePrograms(guideData, guideChannel, ch.name);
            }
        }
        
        private async Task CacheGuideData(GuideData guideData)
        {
            try
            {
                var cacheFile = Path.Combine(cacheDirectory, $"guide_{DateTime.Now:yyyyMMdd}.json");
                var json = JsonSerializer.Serialize(guideData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(cacheFile, json);
                
                Debug.WriteLine($"[GuideFetcher] Guide cached to: {cacheFile}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GuideFetcher] Cache failed: {ex.Message}");
            }
        }
        
        public async Task<GuideData> LoadCachedGuideAsync()
        {
            try
            {
                var cacheFile = Path.Combine(cacheDirectory, $"guide_{DateTime.Now:yyyyMMdd}.json");
                if (File.Exists(cacheFile))
                {
                    var json = await File.ReadAllTextAsync(cacheFile);
                    return JsonSerializer.Deserialize<GuideData>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GuideFetcher] Cache load failed: {ex.Message}");
            }
            
            return new GuideData();
        }
        
        public TimeSpan TimeSinceLastSync => DateTime.Now - lastSync;
        public bool NeedsSync => TimeSinceLastSync > TimeSpan.FromHours(6); // Sync every 6 hours
    }
    
    // Data models
    public class GuideData
    {
        public List<GuideChannel> Channels { get; set; } = new List<GuideChannel>();
        public List<GuideProgram> Programs { get; set; } = new List<GuideProgram>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
    
    public class GuideChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string LogoUrl { get; set; }
        public string Category { get; set; }
        public string StreamUrl { get; set; }
        public string Source { get; set; }
    }
    
    public class GuideProgram
    {
        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Category { get; set; }
    }
    
    // Pluto TV API models
    public class PlutoTVResponse
    {
        public PlutoTVChannel[] data { get; set; }
    }
    
    public class PlutoTVChannel
    {
        public string id { get; set; }
        public string name { get; set; }
        public int number { get; set; }
        public string category { get; set; }
        public PlutoTVImages images { get; set; }
    }
    
    public class PlutoTVImages
    {
        public PlutoTVLogo logo { get; set; }
    }
    
    public class PlutoTVLogo
    {
        public string path { get; set; }
    }
}
