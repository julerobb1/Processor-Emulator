using System;
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
        }

        /// <summary>
        /// Finds the offset of the GPT header ("EFI PART").
        /// </summary>
        private static long FindGptHeaderOffset(byte[] data, long startOffset)
        {
            byte[] gptMagic = Encoding.ASCII.GetBytes("EFI PART");
            long searchLimit = data.Length - gptMagic.Length;
            for (long i = startOffset; i <= searchLimit; i++)
            {
                if (data[i] != gptMagic[0]) continue;

                bool found = true;
                for (int j = 1; j < gptMagic.Length; j++)
                {
                    if (data[i + j] != gptMagic[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i - 8; // "EFI PART" is at offset 8 in the GPT header
                }
            }
            return -1;
        }

        public static System.Collections.Generic.List<PartitionEntry> ParsePartitionTable(byte[] unpackedData)
        {
            var partitions = new System.Collections.Generic.List<PartitionEntry>();
            const ulong lbaSize = 512; // Logical Block Address size

            // Search for GPT header, typically at LBA 1 (offset 512)
            long gptHeaderOffset = FindGptHeaderOffset(unpackedData, (long)lbaSize);

            if (gptHeaderOffset == -1)
            {
                // Fallback: search from the beginning of the file if not found at LBA 1
                gptHeaderOffset = FindGptHeaderOffset(unpackedData, 0);
            }

            if (gptHeaderOffset == -1)
            {
                throw new FirmwareUnpackException("GPT header (EFI PART) not found.");
            }

            using (var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(unpackedData)))
            {
                reader.BaseStream.Seek(gptHeaderOffset + 72, System.IO.SeekOrigin.Begin); // Offset to PartitionEntryLBA
                ulong partitionEntryLBA = reader.ReadUInt64();
                uint numberOfPartitions = reader.ReadUInt32();
                uint sizeOfPartitionEntry = reader.ReadUInt32();

                reader.BaseStream.Seek((long)(partitionEntryLBA * lbaSize), System.IO.SeekOrigin.Begin);

                for (int i = 0; i < numberOfPartitions; i++)
                {
                    byte[] entryBytes = reader.ReadBytes((int)sizeOfPartitionEntry);
                    if (entryBytes.Length < 56) continue; // Not a full entry

                    var partitionTypeGuid = new Guid(new ReadOnlySpan<byte>(entryBytes, 0, 16));
                    if (partitionTypeGuid == Guid.Empty) continue;

                    var startingLBA = BitConverter.ToUInt64(entryBytes, 32);
                    var endingLBA = BitConverter.ToUInt64(entryBytes, 40);
                    var nameBytes = new byte[72];
                    Array.Copy(entryBytes, 56, nameBytes, 0, Math.Min(72, entryBytes.Length - 56));
                    var name = System.Text.Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');

                    // Perform arithmetic using ulong to avoid ambiguity and overflow
                    ulong partitionSizeInLbas = endingLBA - startingLBA + 1;
                    ulong partitionSizeInBytes = partitionSizeInLbas * lbaSize;
                    ulong startAddress = startingLBA * lbaSize;

                    if (startAddress + partitionSizeInBytes > (ulong)unpackedData.Length)
                    {
                        // Partition is out of bounds
                        continue;
                    }
                    
                    byte[] partitionData = new byte[partitionSizeInBytes];
                    Array.Copy(unpackedData, (long)startAddress, partitionData, 0, (long)partitionSizeInBytes);

                    partitions.Add(new PartitionEntry
                    {
                        PartitionTypeGuid = partitionTypeGuid,
                        StartingLBA = startingLBA,
                        EndingLBA = endingLBA,
                        Name = name,
                        Data = partitionData
                    });
                }
            }

            return partitions;
        }
    }
}
