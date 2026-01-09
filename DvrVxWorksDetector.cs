using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ProcessorEmulator.Tools.FileSystems
{
    public class DvrVxWorksDetector
    {
        private struct VxWorksVersion
        {
            public string Version;
            public long Signature;
            public long BootSignature;
            public string DeviceType; // e.g., "Hopper", "Joey", "HR54", etc.
            public VxWorksVersion(string version, long signature, long bootSignature, string deviceType)
            {
                Version = version;
                Signature = signature;
                BootSignature = bootSignature;
                DeviceType = deviceType;
            }
        }

        private readonly Dictionary<long, VxWorksVersion> knownVersions = new()
        {
            { 0x27051956L, new VxWorksVersion("6.9", 0x27051956L, 0x0FF0AD12L, "Hopper3") },
            { 0x27051957L, new VxWorksVersion("6.9.4", 0x27051957L, 0x0FF0AD13L, "Hopper") },
            { 0x27051960L, new VxWorksVersion("6.7", 0x27051960L, 0x0FF0AD16L, "Generic") }
        };

        public struct EncryptionInfo
        {
            public string Algorithm;
            private int keySize;
            public int KeySize
            {
                get { return keySize; }
                set { keySize = value; }
            }
            public byte[] KeyMaterial;
            public byte[] IV;
        }

        public (string version, string deviceType, EncryptionInfo encInfo) DetectVersion(byte[] firmware)
        {
            // Check for VxWorks signatures at known offsets
            long[] commonOffsets = { 0x0, 0x200, 0x400, 0x800, 0x1000 };
            foreach (long offset in commonOffsets)
            {
                if (offset + 8 > firmware.Length) continue;
                long signature = BitConverter.ToInt64(firmware, (int)offset);
                if (knownVersions.TryGetValue(signature, out VxWorksVersion version))
                {
                    var encInfo = DetectEncryption(firmware, version);
                    return (version.Version, version.DeviceType, encInfo);
                }
            }

            // Secondary detection method - search for version strings
            string versionStr = SearchVersionString(firmware);
            if (!string.IsNullOrEmpty(versionStr))
            {
                var deviceType = DetermineDeviceType(firmware);
                var encInfo = DetectEncryptionFallback(firmware);
                return (versionStr, deviceType, encInfo);
            }

            throw new Exception("Unknown VxWorks version or not a VxWorks firmware");
        }
//Stupid Sexy Flanders
        private static string SearchVersionString(byte[] firmware)
        {
            // Common VxWorks version string patterns
            string[] patterns = {
                "VxWorks 6",
                "VXWORKS_",
                "Wind River VxWorks"
            };

            foreach (string pattern in patterns)
            {
                byte[] searchPattern = Encoding.ASCII.GetBytes(pattern);
                int index = SearchBytes(firmware, searchPattern);
                if (index >= 0)
                {
                    // Extract version information around the found pattern
                    return ExtractVersionInfo(firmware, index);
                }
            }

            return null;
        }

        private static int SearchBytes(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }

        private static string ExtractVersionInfo(byte[] firmware, int index)
        {
            // Read up to 32 bytes after the pattern for version info
            int maxLength = Math.Min(32, firmware.Length - index);
            byte[] versionBytes = new byte[maxLength];
            Array.Copy(firmware, index, versionBytes, 0, maxLength);
            
            string version = Encoding.ASCII.GetString(versionBytes);
            // Clean up the version string
            int nullChar = version.IndexOf('\0');
                if (nullChar >= 0)
                {
                    version = version.Substring(0, nullChar);
                }
            
            return version.Trim();
        }

        private static string DetermineDeviceType(byte[] firmware)
        {
            // Known device identification strings
            Dictionary<string, string> devicePatterns = new()
            {
                { "HOPPER", "Hopper" },
                { "JOEY", "Joey" },
                { "HR54", "HR54" },
                { "HR44", "HR44" },
                { "GENIE", "Genie" }
            };

            foreach (var pattern in devicePatterns)
            {
                if (SearchBytes(firmware, Encoding.ASCII.GetBytes(pattern.Key)) >= 0)
                    return pattern.Value;
            }

            return "Unknown DVR";
        }

        private EncryptionInfo DetectEncryption(byte[] firmware, VxWorksVersion version)
        {
            // Look for known encryption signatures based on device type
            return version.DeviceType switch
            {
                "Hopper3" => DetectHopperEncryption(firmware),
                "HR54" or "HR44" => DetectGenieDvrEncryption(firmware),
                _ => DetectEncryptionFallback(firmware),
            };
        }

        private static EncryptionInfo DetectHopperEncryption(byte[] firmware)
        {
            // Hopper-specific encryption detection
            return new EncryptionInfo
            {
                Algorithm = "AES-256",
                KeySize = 256,
                KeyMaterial = ExtractKeyMaterial(firmware, "HOPPER"),
                IV = new byte[16] // IV is typically derived from device-specific data
            };
        }

        private static EncryptionInfo DetectGenieDvrEncryption(byte[] firmware)
        {
            // Genie DVR-specific encryption detection
            return new EncryptionInfo
            {
                Algorithm = "AES-128",
                KeySize = 128,
                KeyMaterial = ExtractKeyMaterial(firmware, "GENIE"),
                IV = new byte[16]
            };
        }

        private static EncryptionInfo DetectEncryptionFallback(byte[] firmware)
        {
            // Generic encryption detection
            return new EncryptionInfo
            {
                Algorithm = "Unknown",
                KeySize = 0,
                KeyMaterial = new byte[32],
                IV = new byte[16]
            };
        }

        private static byte[] ExtractKeyMaterial(byte[] firmware, string deviceType)
        {
            // Implementation depends on specific device type
            byte[] keyMaterial = new byte[32];
            // Key material extraction logic here
            return keyMaterial;
        }
    }
}