using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    public static class FirmwareUnpacker
    {
        private const string Pack1Magic = "PACK1";

        /// <summary>
        /// Finds the offset of the PACK1 header in a firmware file.
        /// </summary>
        /// <param name="stream">The firmware file stream.</param>
        /// <returns>The offset of the PACK1 header, or -1 if not found.</returns>
        private static long FindPack1Offset(Stream stream)
        {
            const int searchLimit = 0x200000; // 2MB
            byte[] buffer = new byte[searchLimit];
            stream.Seek(0, SeekOrigin.Begin); // Ensure we start from the beginning
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] magicBytes = Encoding.ASCII.GetBytes(Pack1Magic);

            for (int i = 0; i <= bytesRead - magicBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < magicBytes.Length; j++)
                {
                    if (buffer[i + j] != magicBytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        public static byte[] UnpackARRISFirmware(byte[] firmwareImage)
        {
            using (var stream = new MemoryStream(firmwareImage))
            {
                long pack1Offset = FindPack1Offset(stream);
                if (pack1Offset == -1)
                {
                    throw new FirmwareUnpackException("PACK1 magic not found in firmware image.");
                }
                stream.Seek(pack1Offset, SeekOrigin.Begin);

                using (var reader = new BinaryReader(stream))
                {
                    // Read and verify PACK1 magic
                    string magic = new string(reader.ReadChars(5));
                    if (magic != Pack1Magic)
                    {
                        // This should not happen if FindPack1Offset is correct
                        throw new FirmwareUnpackException($"Invalid PACK1 magic: {magic}");
                    }

                    // The rest of the logic to unpack the firmware would go here.
                    // For now, we'll just return the rest of the stream.
                    long remainingLength = stream.Length - stream.Position;
                    return reader.ReadBytes((int)remainingLength);
                }
            }
        }namespace ProcessorEmulator
{
    public static class FirmwareUnpacker
    {
        private const string Pack1Magic = "PACK1";

        /// <summary>
        /// Finds the offset of the PACK1 header in a firmware file.
        /// </summary>
        /// <param name="stream">The firmware file stream.</param>
        /// <returns>The offset of the PACK1 header, or -1 if not found.</returns>
        private static long FindPack1Offset(Stream stream)
        {
            const int searchLimit = 0x200000; // 2MB
            byte[] buffer = new byte[searchLimit];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] magicBytes = Encoding.ASCII.GetBytes(Pack1Magic);

            for (int i = 0; i <= bytesRead - magicBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < magicBytes.Length; j++)
                {
                    if (buffer[i + j] != magicBytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        
                stream.Seek(pack1Offset, SeekOrigin.Begin);

                using (var reader = new BinaryReader(stream))
                {
                    // Read and verify PACK1 magic
                    string magic = new string(reader.ReadChars(5));
                    if (magic != Pack1Magic)
                    {
                        // This should not happen if FindPack1Offset is correct
                        throw new FirmwareUnpackException($"Invalid PACK1 magic: {magic}");
                    }

                    // The rest of the logic to unpack the firmware would go here.
                    // For now, we'll just return the rest of the stream.
                    long remainingLength = stream.Length - stream.Position;
                    return reader.ReadBytes((int)remainingLength);
                }
            }
        }

        public class FirmwareSection
        {
            public string Name { get; set; }
            public uint Offset { get; set; }
            public uint Size { get; set; }
            public byte[] Data { get; set; }
            public string Platform { get; set; }
        }

        public static List<FirmwareSection> UnpackARRISFirmware(string firmwarePath)
        {
            var sections = new List<FirmwareSection>();
            
            using (var fs = new FileStream(firmwarePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                var fileSize = fs.Length;
                Console.WriteLine($"Firmware file size: {fileSize:N0} bytes");
                
                // Look for PACK1 magic in small chunks to avoid memory issues
                const int searchChunkSize = 8192;
                byte[] searchBuffer = new byte[searchChunkSize];
                long pack1Offset = -1;
                
                for (long pos = 0; pos < Math.Min(fileSize, 1024 * 1024); pos += searchChunkSize / 2) // Search first 1MB only
                {
                    fs.Seek(pos, SeekOrigin.Begin);
                    int bytesRead = fs.Read(searchBuffer, 0, searchChunkSize);
                    
                    var pack1Pos = FindPattern(searchBuffer, Encoding.ASCII.GetBytes("PACK1"));
                    if (pack1Pos != -1)
                    {
                        pack1Offset = pos + pack1Pos;
                        Console.WriteLine($"Found PACK1 at offset: 0x{pack1Offset:X}");
                        break;
                    }
                }
                
                if (pack1Offset == -1)
                {
                    throw new Exception("PACK1 magic not found in first 1MB");
                }
                
                fs.Seek(pack1Offset, SeekOrigin.Begin);
                
                // Read PACK1 header
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(5)); // "PACK1"
                var version = reader.ReadByte(); // Usually 7
                var headerSize = reader.ReadUInt16(); // Usually 0x8000
                
                Console.WriteLine($"Found PACK1 v{version}, header size: 0x{headerSize:X}");
                
                // Skip to directory entries
                fs.Seek(pack1Offset + 16, SeekOrigin.Begin);
                
                while (fs.Position < pack1Offset + headerSize && fs.Position < fileSize)
                {
                    try
                    {
                        var entryType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                        
                        if (entryType == "PDIR")
                        {
                            var dirSize = reader.ReadUInt32();
                            var dirEntries = reader.ReadUInt32();
                            
                            Console.WriteLine($"Directory with {dirEntries} entries");
                            
                            for (int i = 0; i < dirEntries && i < 20; i++) // Limit to 20 entries max
                            {
                                var sectionName = ReadNullTerminatedString(reader, 4);
                                var sectionOffset = reader.ReadUInt32();
                                var sectionSize = reader.ReadUInt32();
                                var dataMarker = Encoding.ASCII.GetString(reader.ReadBytes(4)); // "DATA"
                                var platform = ReadNullTerminatedString(reader, 32);
                                
                                Console.WriteLine($"Section: {sectionName}, Offset: 0x{sectionOffset:X}, Size: 0x{sectionSize:X}, Platform: {platform}");
                                
                                // Only extract sections that are reasonable size (< 50MB)
                                if (sectionSize > 0 && sectionSize < 50 * 1024 * 1024)
                                {
                                    // Extract the actual data using streaming
                                    var currentPos = fs.Position;
                                    fs.Seek(pack1Offset + sectionOffset, SeekOrigin.Begin);
                                    
                                    var sectionData = new byte[sectionSize];
                                    int totalRead = 0;
                                    
                                    // Read in chunks to avoid memory pressure
                                    const int readChunkSize = 1024 * 1024; // 1MB chunks
                                    while (totalRead < sectionSize)
                                    {
                                        int toRead = Math.Min(readChunkSize, (int)(sectionSize - totalRead));
                                        int bytesRead = fs.Read(sectionData, totalRead, toRead);
                                        if (bytesRead == 0) break;
                                        totalRead += bytesRead;
                                    }
                                    
                                    sections.Add(new FirmwareSection
                                    {
                                        Name = sectionName,
                                        Offset = sectionOffset,
                                        Size = sectionSize,
                                        Data = sectionData,
                                        Platform = platform
                                    });
                                    
                                    fs.Seek(currentPos, SeekOrigin.Begin);
                                }
                                else
                                {
                                    Console.WriteLine($"Skipping section {sectionName} - size too large: {sectionSize:N0} bytes");
                                }
                            }
                            break; // Found directory, exit
                        }
                        else if (entryType == "\0\0\0\0")
                        {
                            break; // End of directory
                        }
                        else
                        {
                            // Skip unknown entry
                            fs.Seek(-3, SeekOrigin.Current); // Back up 3 bytes and try again
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading directory entry: {ex.Message}");
                        break;
                    }
                }
            }
            
            return sections;
        }
        
        private static int FindPattern(byte[] data, byte[] pattern)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }
        
        private static string ReadNullTerminatedString(BinaryReader reader, int maxLength)
        {
            var bytes = reader.ReadBytes(maxLength);
            var nullIndex = Array.IndexOf(bytes, (byte)0);
            if (nullIndex >= 0)
            {
                return Encoding.ASCII.GetString(bytes, 0, nullIndex);
            }
            return Encoding.ASCII.GetString(bytes);
        }
        
        public static void ExtractKernelFromSection(FirmwareSection kernelSection, string outputPath)
        {
            // Look for common ARM Linux kernel signatures
            var data = kernelSection.Data;
            
            // Check for uImage header (0x27051956)
            if (data.Length >= 64 && 
                data[0] == 0x27 && data[1] == 0x05 && data[2] == 0x19 && data[3] == 0x56)
            {
                Console.WriteLine("Found uImage kernel!");
                
                // uImage header is 64 bytes, kernel follows
                var kernelData = new byte[data.Length - 64];
                Array.Copy(data, 64, kernelData, 0, kernelData.Length);
                
                File.WriteAllBytes(outputPath, kernelData);
                return;
            }
            
            // Check for zImage signature
            var zImagePattern = Encoding.ASCII.GetBytes("Linux");
            var linuxOffset = FindPattern(data, zImagePattern);
            if (linuxOffset > 0)
            {
                Console.WriteLine($"Found Linux signature at offset 0x{linuxOffset:X}");
                
                // Extract from Linux signature onwards
                var kernelData = new byte[data.Length - linuxOffset];
                Array.Copy(data, linuxOffset, kernelData, 0, kernelData.Length);
                
                File.WriteAllBytes(outputPath, kernelData);
                return;
            }
            
            // If no specific signature found, save the whole section
            Console.WriteLine("No specific kernel signature found, saving entire section");
            File.WriteAllBytes(outputPath, data);
        }
    }
}

