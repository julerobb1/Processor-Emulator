using System.Collections.Generic;

namespace ProcessorEmulator.Architectures
{
    public class ArchitectureInfo
    {
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public int Bitness { get; set; }
        public string Endianness { get; set; }
        public string Description { get; set; }
    }

    public static class ArchitectureDatabase
    {
        public static List<ArchitectureInfo> Architectures = new List<ArchitectureInfo>
        {
            new ArchitectureInfo { Name = "MIPS32", Aliases = new[]{"MIPS"}, Bitness = 32, Endianness = "Big/Little", Description = "MIPS 32-bit" },
            new ArchitectureInfo { Name = "MIPS64", Aliases = new[]{"MIPS64"}, Bitness = 64, Endianness = "Big/Little", Description = "MIPS 64-bit" },
            new ArchitectureInfo { Name = "ARM", Aliases = new[]{"ARMv7"}, Bitness = 32, Endianness = "Little", Description = "ARM 32-bit" },
            new ArchitectureInfo { Name = "ARM64", Aliases = new[]{"AArch64"}, Bitness = 64, Endianness = "Little", Description = "ARM 64-bit" },
            new ArchitectureInfo { Name = "PowerPC", Aliases = new[]{"PPC"}, Bitness = 32, Endianness = "Big/Little", Description = "PowerPC 32-bit" },
            new ArchitectureInfo { Name = "x86", Aliases = new[]{"i386"}, Bitness = 32, Endianness = "Little", Description = "Intel x86 32-bit" },
            new ArchitectureInfo { Name = "x86-64", Aliases = new[]{"amd64"}, Bitness = 64, Endianness = "Little", Description = "Intel/AMD 64-bit" },
        };
    }
}
