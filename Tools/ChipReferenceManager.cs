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
            { "BCM7445",        "Broadcom BCM7445 (Brahma15) quad-core ARM Cortex-A15 SoC used in X1 set-top boxes" },
            { "BCM4506K0IE33G", "Broadcom BCM4506: satellite tuner and demodulator" },
            { "88W8801-NJR2", "Marvell 88W8801: Wi-Fi SoC for wireless connectivity" },
            { "MX65L1G80GAC1-10G", "Macronix MX65L1G80GAC1: 1Gb SPI NOR flash memory" },
            { "QL01GS1DHSSG", "Spansion QL01GS1DHSSG: 1Gb SPI NOR flash memory for firmware storage" },
            // Added entries for BCM7405 and MIPS4380 based on hardware references
            { "BCM7405", "Broadcom BCM7405: MIPS 4380 class SoC commonly used in set-top boxes" },
            { "MIPS4380", "MIPS 4380 class CPU: core used in Broadcom SoCs such as BCM7405" },
            // STMicroelectronics SoCs (cooler running than Broadcom)
            { "STi7101", "STMicro STi7101 SoC: DVB multimedia decoder and processing unit" },
            { "STi7200", "STMicro STi7200 SoC: high-performance DVB multimedia processor" },
            { "STi5500", "STMicro STi5500 SoC: common DVB set-top box processor" }
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
