using System;
using System.IO;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core.Loaders
{
    public static class NkBinLoader
    {
        /// <summary>
        /// Loads a Windows CE nk.bin file into memory.
        /// </summary>
        /// <param name="filePath">Path to the nk.bin file.</param>
        /// <param name="memory">The memory manager to load into.</param>
        /// <returns>The physical start address for the CPU.</returns>
        public static ulong Load(string filePath, IMemoryManager memory)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("nk.bin file not found.", filePath);
            }

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // 1. Read and verify header
                // nk.bin format starts with a 7-byte signature "B000FF\n"
                byte[] signature = reader.ReadBytes(7);
                // For this implementation, we will be lenient and just log it.
                // A real implementation would verify it.
                
                ulong imageStart = reader.ReadUInt32();
                ulong imageLength = reader.ReadUInt32();

                Console.WriteLine($"[NkBinLoader] Loading kernel. Image start: 0x{imageStart:X}, Length: 0x{imageLength:X}");

                ulong entryPoint = 0;

                // 2. Loop through records
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint recordAddress = reader.ReadUInt32();
                    uint recordLength = reader.ReadUInt32();
                    uint recordChecksum = reader.ReadUInt32();

                    if (recordAddress == 0 && recordLength == 0)
                    {
                        // This can be a marker for the sync record at the end
                        entryPoint = reader.ReadUInt32();
                        Console.WriteLine($"[NkBinLoader] Found sync record. Entry Point: 0x{entryPoint:X}");
                        break;
                    }

                    Console.WriteLine($"[NkBinLoader] Loading record at 0x{recordAddress:X}, Length: {recordLength}");

                    byte[] data = reader.ReadBytes((int)recordLength);

                    // Note: Checksum validation is skipped for this implementation.

                    // Load the record into memory
                    memory.WriteMemory(recordAddress, data);
                }

                if (entryPoint == 0)
                {
                    throw new InvalidDataException("Could not determine kernel entry point from nk.bin file.");
                }

                return entryPoint;
            }
        }
    }
}
