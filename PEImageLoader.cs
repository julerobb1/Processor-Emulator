using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    public enum PEArchitecture
    {
        Unknown = 0,
        x86 = 0x14c,
        ARM = 0x166,     // ARM (little endian) - corrected value
        ARMThumb = 0x1c0, // ARM Thumb
        ARM64 = 0x1c2,   // ARM64 (was ARMThumb)
        MIPS = 0x366,    // MIPS
        MIPS16 = 0x266,  // MIPS16
        x64 = 0x8664
    }

    public enum PESubsystem
    {
        Unknown = 0,
        Native = 1,
        WindowsGUI = 2,
        WindowsCUI = 3,
        WindowsCE = 9
    }

    public enum MemoryProtection
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
        ReadWrite = Read | Write,
        ReadExecute = Read | Execute,
        ReadWriteExecute = Read | Write | Execute
    }

    public class PEImageInfo
    {
        public PEArchitecture Architecture { get; set; }
        public PESubsystem Subsystem { get; set; }
        public uint ImageBase { get; set; }
        public uint EntryPoint { get; set; }
        public uint SizeOfImage { get; set; }
        public List<SectionInfo> Sections { get; set; } = new List<SectionInfo>();
        public List<ImportInfo> Imports { get; set; } = new List<ImportInfo>();
        public byte[] RawData { get; set; }
    }

    public class SectionInfo
    {
        public string Name { get; set; }
        public uint VirtualAddress { get; set; }
        public uint VirtualSize { get; set; }
        public uint RawSize { get; set; }
        public uint RawOffset { get; set; }
        public uint Characteristics { get; set; }
        public byte[] RawData { get; set; }
    }

    public class ImportInfo
    {
        public string DllName { get; set; }
        public List<FunctionImport> Functions { get; set; } = new List<FunctionImport>();
    }

    public class FunctionImport
    {
        public string Name { get; set; }
        public ushort Ordinal { get; set; }
        public uint IATAddress { get; set; }
    }

    /// <summary>
    /// Loads and parses Windows CE PE executables
    /// </summary>
    public class PEImageLoader
    {
        public async Task<PEImageInfo> LoadPEImageAsync(string filePath)
        {
            try
            {
                var data = await File.ReadAllBytesAsync(filePath);
                return ParsePE(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PE Load Error: {ex.Message}");
                return null;
            }
        }

        private PEImageInfo ParsePE(byte[] data)
        {
            if (data.Length < 64)
                throw new InvalidDataException("File too small to be a PE");

            // Check DOS header
            if (BitConverter.ToUInt16(data, 0) != 0x5A4D) // "MZ"
                throw new InvalidDataException("Invalid DOS header");

            // Get PE header offset with bounds checking
            var peOffset = BitConverter.ToUInt32(data, 0x3C);
            if (peOffset >= data.Length - 24) // Need at least 24 bytes for PE signature + COFF header
                throw new InvalidDataException($"Invalid PE offset: 0x{peOffset:X} (file size: {data.Length})");

            // Check PE signature
            if (BitConverter.ToUInt32(data, (int)peOffset) != 0x00004550) // "PE\0\0"
                throw new InvalidDataException("Invalid PE signature");

            var pe = new PEImageInfo { RawData = data };

            // Parse COFF header with bounds checking
            var coffOffset = (int)peOffset + 4;
            if (coffOffset + 20 > data.Length)
                throw new InvalidDataException("COFF header extends beyond file");

            var machine = BitConverter.ToUInt16(data, coffOffset);
            pe.Architecture = (PEArchitecture)machine;

            var numberOfSections = BitConverter.ToUInt16(data, coffOffset + 2);
            var sizeOfOptionalHeader = BitConverter.ToUInt16(data, coffOffset + 16);

            Console.WriteLine($"🔍 Machine Type: 0x{machine:X4} ({pe.Architecture})");
            Console.WriteLine($"📦 Sections: {numberOfSections}");

            // Validate section count and optional header size
            if (numberOfSections > 96) // Reasonable limit
                throw new InvalidDataException($"Too many sections: {numberOfSections}");

            // Parse optional header with bounds checking
            var optionalHeaderOffset = coffOffset + 20;
            if (sizeOfOptionalHeader > 0)
            {
                if (optionalHeaderOffset + sizeOfOptionalHeader > data.Length)
                    throw new InvalidDataException("Optional header extends beyond file");

                if (sizeOfOptionalHeader >= 2) // At least magic number
                {
                    var magic = BitConverter.ToUInt16(data, optionalHeaderOffset);
                    var is32Bit = magic == 0x10B;
                    
                    if (is32Bit && sizeOfOptionalHeader >= 96) // Standard PE32 optional header size
                    {
                        pe.EntryPoint = BitConverter.ToUInt32(data, optionalHeaderOffset + 16);
                        pe.ImageBase = BitConverter.ToUInt32(data, optionalHeaderOffset + 28);
                        pe.SizeOfImage = BitConverter.ToUInt32(data, optionalHeaderOffset + 56);
                        pe.Subsystem = (PESubsystem)BitConverter.ToUInt16(data, optionalHeaderOffset + 68);
                    }
                    else if (!is32Bit && sizeOfOptionalHeader >= 112) // Standard PE32+ optional header size
                    {
                        // PE32+ format
                        pe.EntryPoint = BitConverter.ToUInt32(data, optionalHeaderOffset + 16);
                        pe.ImageBase = (uint)BitConverter.ToUInt64(data, optionalHeaderOffset + 24); // Truncate for 32-bit compat
                        pe.SizeOfImage = BitConverter.ToUInt32(data, optionalHeaderOffset + 56);
                        pe.Subsystem = (PESubsystem)BitConverter.ToUInt16(data, optionalHeaderOffset + 68);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Unusual optional header size: {sizeOfOptionalHeader} (magic: 0x{magic:X})");
                    }
                }
            }

            // Parse sections with bounds checking
            var sectionHeaderOffset = optionalHeaderOffset + sizeOfOptionalHeader;
            if (sectionHeaderOffset + (numberOfSections * 40) > data.Length)
                throw new InvalidDataException("Section headers extend beyond file");

            for (int i = 0; i < numberOfSections; i++)
            {
                var sectionOffset = sectionHeaderOffset + (i * 40);
                var section = new SectionInfo
                {
                    Name = System.Text.Encoding.ASCII.GetString(data, sectionOffset, 8).TrimEnd('\0'),
                    VirtualSize = BitConverter.ToUInt32(data, sectionOffset + 8),
                    VirtualAddress = BitConverter.ToUInt32(data, sectionOffset + 12),
                    RawSize = BitConverter.ToUInt32(data, sectionOffset + 16),
                    RawOffset = BitConverter.ToUInt32(data, sectionOffset + 20),
                    Characteristics = BitConverter.ToUInt32(data, sectionOffset + 36)
                };

                // Copy section data with bounds checking
                if (section.RawSize > 0 && section.RawOffset < data.Length && section.RawOffset + section.RawSize <= data.Length)
                {
                    var dataSize = Math.Min(section.RawSize, (uint)(data.Length - section.RawOffset));
                    section.RawData = new byte[dataSize];
                    Array.Copy(data, section.RawOffset, section.RawData, 0, dataSize);
                }
                else if (section.RawSize > 0)
                {
                    Console.WriteLine($"⚠️ Section {section.Name} has invalid data offset/size: 0x{section.RawOffset:X}/0x{section.RawSize:X}");
                }

                pe.Sections.Add(section);
            }

            // Parse imports (simplified)
            ParseImports(pe, data);

            return pe;
        }

        private void ParseImports(PEImageInfo pe, byte[] data)
        {
            try
            {
                // Find import directory from data directories
                // This is a simplified implementation
                var importSection = pe.Sections.FirstOrDefault(s => s.Name.StartsWith(".idata") || s.Name.StartsWith(".rdata"));
                if (importSection != null && importSection.RawData != null)
                {
                    Console.WriteLine($"📚 Found import section: {importSection.Name}");
                    
                    // Add dummy imports for common Windows CE DLLs
                    var commonDlls = new[] { "coredll.dll", "kernel32.dll", "user32.dll", "gdi32.dll" };
                    foreach (var dll in commonDlls)
                    {
                        var import = new ImportInfo { DllName = dll };
                        
                        // Add common functions
                        var commonFunctions = dll switch
                        {
                            "coredll.dll" => new[] { "CreateThread", "ExitProcess", "GetLastError", "CloseHandle" },
                            "kernel32.dll" => new[] { "GetModuleHandle", "LoadLibrary", "GetProcAddress" },
                            "user32.dll" => new[] { "MessageBox", "CreateWindow", "ShowWindow" },
                            "gdi32.dll" => new[] { "CreateDC", "DeleteDC", "BitBlt" },
                            _ => new[] { "DummyFunction" }
                        };

                        foreach (var func in commonFunctions)
                        {
                            import.Functions.Add(new FunctionImport
                            {
                                Name = func,
                                IATAddress = pe.ImageBase + 0x2000 + (uint)(pe.Imports.Count * 0x100 + import.Functions.Count * 4)
                            });
                        }
                        
                        pe.Imports.Add(import);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Import parsing failed: {ex.Message}");
            }
        }
    }
}
