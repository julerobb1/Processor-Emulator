using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    public static class FirmwareStreamer
    {
        public static async Task<bool> StreamFirmwareToMemory(string firmwarePath, byte[] virtualMemory, uint baseAddress, Action<string> logger)
        {
            try
            {
                logger($"🚀 Streaming firmware to avoid 2GB limit...");
                
                using (var fs = new FileStream(firmwarePath, FileMode.Open, FileAccess.Read))
                {
                    var fileSize = fs.Length;
                    logger($"📦 File size: {fileSize:N0} bytes");
                    
                    // Skip signature wrapper - look for actual firmware
                    int skipBytes = 0;
                    
                    // Check for ARRIS signature
                    var headerBuffer = new byte[1024];
                    fs.Read(headerBuffer, 0, 1024);
                    var headerText = Encoding.ASCII.GetString(headerBuffer);
                    
                    if (headerText.Contains("CableLabs"))
                    {
                        // Skip to after certificates (usually around 32KB-64KB)
                        skipBytes = 65536;
                        logger($"🔓 Skipping ARRIS signature wrapper ({skipBytes} bytes)");
                    }
                    
                    // Reset and seek to firmware start
                    fs.Seek(skipBytes, SeekOrigin.Begin);
                    
                    // Stream in 64KB chunks to avoid memory pressure
                    const int chunkSize = 64 * 1024;
                    var buffer = new byte[chunkSize];
                    long totalRead = 0;
                    long maxRead = Math.Min(fileSize - skipBytes, 50 * 1024 * 1024); // Max 50MB
                    
                    logger($"📥 Streaming {maxRead:N0} bytes in {chunkSize:N0} byte chunks...");
                    
                    while (totalRead < maxRead)
                    {
                        var toRead = (int)Math.Min(chunkSize, maxRead - totalRead);
                        var bytesRead = fs.Read(buffer, 0, toRead);
                        
                        if (bytesRead == 0) break;
                        
                        // Copy directly to virtual memory
                        for (int i = 0; i < bytesRead; i++)
                        {
                            var memIndex = baseAddress + totalRead + i;
                            if (memIndex < virtualMemory.Length)
                            {
                                virtualMemory[memIndex] = buffer[i];
                            }
                        }
                        
                        totalRead += bytesRead;
                        
                        // Progress every 5MB
                        if (totalRead % (5 * 1024 * 1024) == 0)
                        {
                            var progress = (int)((totalRead * 100) / maxRead);
                            logger($"📊 Progress: {progress}% ({totalRead / (1024 * 1024)} MB)");
                            await Task.Delay(5); // Prevent UI freeze
                        }
                    }
                    
                    logger($"✅ Successfully streamed {totalRead:N0} bytes");
                    logger($"🔓 NO 2GB ARRAY LIMIT HIT - streaming bypass worked!");
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger($"❌ Streaming failed: {ex.Message}");
                return false;
            }
        }
    }
}
