using System.Collections.Generic;

namespace ProcessorEmulator.Tools
{
    public static class ChipReferenceManager
    {
        // Known CPU/SoC reference descriptions
        private static readonly Dictionary<string, string> ChipInfo = new()
        {
            { "x86", "Intel/AMD x86 32-bit architecture" },
            { "x64", "AMD64 / Intel 64-bit architecture" },
            { "ARM", "ARMv7 32-bit architecture (common in set-top boxes)" },
            { "ARM64", "ARMv8 64-bit architecture (modern ARM SoCs)" },
            { "MIPS", "MIPS architecture (often Broadcom SoCs)" },
            { "SPARC", "SPARC architecture" },
            { "ELF", "Generic ELF format (unknown CPU)" },
            { "PE", "Generic PE format (Windows/WinCE)" },
            // Humax HR44/HR54 Receiver SoCs and components
            { "BCM7346DRKFEBB35G", "Broadcom BCM7346 SoC: main processor for Humax DIRECTV DVR" },
            { "BCM4506K0IE33G", "Broadcom BCM4506: satellite tuner and demodulator" },
            { "88W8801-NJR2", "Marvell 88W8801: Wi-Fi SoC for wireless connectivity" },
            { "MX65L1G80GAC1-10G", "Macronix MX65L1G80GAC1: 1Gb SPI NOR flash memory" },
            { "QL01GS1DHSSG", "Spansion QL01GS1DHSSG: 1Gb SPI NOR flash memory for firmware storage" }
        };

        // Retrieve human-friendly info for a given chip key
        public static string GetInfo(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return ChipInfo.TryGetValue(key, out var info) ? info : null;
        }

        // Encourage contributions for missing chips
        public static string GetContributionMessage(string key)
        {
            return $"Chip '{key}' not in reference. Please research and open an issue or PR at https://github.com/julerobb1/Processor-Emulator";
        }
    }
}
