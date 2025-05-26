using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace ProcessorEmulator.Tools
{
    public class UverseFileParser
    {
        private class SignatureEntry
        {
            public string Filename { get; set; }
            public string Size { get; set; }
            public string Hash { get; set; }
            public string Type { get; set; }
        }

        private Dictionary<string, SignatureEntry> signatureEntries = new Dictionary<string, SignatureEntry>();
        private Dictionary<string, byte[]> contentCache = new Dictionary<string, byte[]>();

        public void ParseSignatureFile(string sigPath)
        {
            string[] lines = File.ReadAllLines(sigPath);
            // Skip first line (signature header)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(' ');
                if (parts.Length >= 4)
                {
                    signatureEntries[parts[0].ToLowerInvariant()] = new SignatureEntry
                    {
                        Filename = parts[0],
                        Size = parts[1],
                        Hash = parts[2],
                        Type = parts[3]
                    };
                }
            }
        }

        public void ParseBzFile(string bzPath)
        {
            using (var stream = File.OpenRead(bzPath))
            using (var reader = new BinaryReader(stream))
            {
                // Parse .bz format (compressed binary format used by U-verse)
                byte[] header = reader.ReadBytes(4);
                uint compressionType = BitConverter.ToUInt32(header, 0);
                byte[] content = reader.ReadBytes((int)(stream.Length - 4));
                
                string filename = Path.GetFileName(bzPath).ToLowerInvariant();
                if (signatureEntries.TryGetValue(filename, out SignatureEntry entry))
                {
                    if (VerifyContent(content, entry.Hash))
                    {
                        contentCache[filename] = DecompressContent(content, compressionType);
                    }
                }
            }
        }

        public void ParseDatFile(string datPath)
        {
            string filename = Path.GetFileName(datPath).ToLowerInvariant();
            byte[] content = File.ReadAllBytes(datPath);

            if (signatureEntries.TryGetValue(filename, out SignatureEntry entry))
            {
                if (VerifyContent(content, entry.Hash))
                {
                    contentCache[filename] = content;
                }
            }
        }

        private bool VerifyContent(byte[] content, string expectedHash)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(content);
                string actualHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return actualHash == expectedHash.ToLowerInvariant();
            }
        }

        private byte[] DecompressContent(byte[] compressed, uint compressionType)
        {
            // Handle different compression types used by U-verse
            switch (compressionType)
            {
                case 0x1: // Standard compression
                    return DecompressStandard(compressed);
                case 0x2: // Custom compression
                    return DecompressCustom(compressed);
                default:
                    return compressed;
            }
        }

        private byte[] DecompressStandard(byte[] data)
        {
            // Standard decompression implementation
            return data; // Placeholder
        }

        private byte[] DecompressCustom(byte[] data)
        {
            // Custom U-verse decompression implementation
            return data; // Placeholder
        }

        public void ParseBootSignature(string bootSigPath)
        {
            ParseSignatureFile(bootSigPath);
            
            // Special handling for boot-specific files
            foreach (var entry in signatureEntries.Values)
            {
                if (entry.Type == "BOOTABLE" || entry.Type == "SELF")
                {
                    // Handle boot and self-verifying files
                    ProcessBootableFile(entry);
                }
            }
        }

        private void ProcessBootableFile(SignatureEntry entry)
        {
            // Process bootable files with special verification
        }

        public void ParseContentFiles(string baseDir)
        {
            // Parse all content files based on content.sig
            foreach (var entry in signatureEntries.Values)
            {
                string fullPath = Path.Combine(baseDir, entry.Filename);
                if (File.Exists(fullPath))
                {
                    string ext = Path.GetExtension(fullPath).ToLowerInvariant();
                    switch (ext)
                    {
                        case ".bz":
                            ParseBzFile(fullPath);
                            break;
                        case ".dat":
                            ParseDatFile(fullPath);
                            break;
                        case ".bin":
                            ProcessBinaryFile(fullPath, entry);
                            break;
                    }
                }
            }
        }

        private void ProcessBinaryFile(string path, SignatureEntry entry)
        {
            byte[] content = File.ReadAllBytes(path);
            if (VerifyContent(content, entry.Hash))
            {
                contentCache[entry.Filename] = content;
            }
        }

        public byte[] GetFileContent(string filename)
        {
            return contentCache.TryGetValue(filename.ToLowerInvariant(), out byte[] content) 
                ? content 
                : null;
        }
    }
}