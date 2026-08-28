using System;
using System.IO;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core;

namespace ProcessorEmulator.Core.Loaders
{
    public readonly struct NkLoadResult
    {
        public NkLoadResult(ulong entryPoint, uint imageStart, uint imageLength, int recordsLoaded, bool truncated, byte[] image)
        {
            EntryPoint = entryPoint;
            ImageStart = imageStart;
            ImageLength = imageLength;
            RecordsLoaded = recordsLoaded;
            Truncated = truncated;
            Image = image ?? Array.Empty<byte>();
        }

        public ulong EntryPoint { get; }
        public uint ImageStart { get; }
        public uint ImageLength { get; }
        public int RecordsLoaded { get; }
        public bool Truncated { get; }
        public byte[] Image { get; }
    }

    public static class NkBinLoader
    {
        // Chain table 0x8006B9EC: ExtraROM base 0x80630000 / size 0xD30000.
        // Julian's etc.bin B000FF imageStart matches that base. Do not
        // invent a map for chain 0x81360000 — this dump has no B000FF
        // for that slot.
        public const uint ExtraRomImageStart = 0x80630000;

        public static bool IsB000Ff(byte[] data)
        {
            return data != null
                && data.Length >= 15
                && data[0] == (byte)'B'
                && data[1] == (byte)'0'
                && data[2] == (byte)'0'
                && data[3] == (byte)'0'
                && data[4] == (byte)'F'
                && data[5] == (byte)'F'
                && data[6] == (byte)'\n';
        }

        public static ulong Load(string filePath, IMemoryManager memory)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("nk.bin file not found.", filePath);
            }

            string feed = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrEmpty(feed))
                HostHardDisk.OfferFeed(feed);
            NkLoadResult result = Load(File.ReadAllBytes(filePath), memory);
            return result.EntryPoint;
        }

        public static NkLoadResult Load(byte[] data, IMemoryManager memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            if (data == null || data.Length < 15)
            {
                throw new InvalidDataException("nk.bin is too small to contain a CE image header.");
            }

            int pos = IsB000Ff(data) ? 7 : 0;
            uint imageStart = BitConverter.ToUInt32(data, pos);
            pos += 4;
            uint imageLength = BitConverter.ToUInt32(data, pos);
            pos += 4;

            Console.WriteLine($"[NkBinLoader] Loading kernel. Image start: 0x{imageStart:X}, Length: 0x{imageLength:X}");

            int records = WriteB000FfRecords(data, pos, imageLength, memory, "nk", out uint firstRecord, out ulong entryPoint, out bool truncated);

            if (entryPoint == 0)
                entryPoint = firstRecord != 0 ? firstRecord : imageStart;

            if (entryPoint == 0)
                throw new InvalidDataException("Could not determine kernel entry point from nk.bin file.");

            BinBlkMedia.Attach(data);
            HostHardDisk.Attach();
            TryLoadExtraRom(HostHardDisk.ExtraRomPath, memory);
            return new NkLoadResult(entryPoint, imageStart, imageLength, records, truncated, data);
        }

        // Hunt path is HostHardDisk ExtraRomPath (filename etc.bin).
        // Read-only. Reject hunt stubs and any B000FF whose imageStart
        // is not the chain-1 base. Does not attach BINBlk. Does not
        // invent 0x81360000.
        public static bool TryLoadExtraRom(string path, IMemoryManager memory)
        {
            if (memory == null || string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            byte[] data;
            try
            {
                data = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[NkBinLoader] ExtraROM read failed " + path + ": " + ex.Message);
                return false;
            }

            if (!IsB000Ff(data))
            {
                Console.WriteLine("[NkBinLoader] ExtraROM skip " + path + " (" + data.Length + " bytes, not B000FF)");
                return false;
            }

            uint imageStart = BitConverter.ToUInt32(data, 7);
            uint imageLength = BitConverter.ToUInt32(data, 11);
            if (imageStart != ExtraRomImageStart)
            {
                Console.WriteLine("[NkBinLoader] ExtraROM skip " + path +
                    " imageStart=0x" + imageStart.ToString("X") +
                    " (want 0x" + ExtraRomImageStart.ToString("X") + "; do not invent 0x81360000)");
                return false;
            }

            int records = WriteB000FfRecords(data, 15, imageLength, memory, "etc", out _, out _, out bool truncated);
            if (records <= 0)
            {
                Console.WriteLine("[NkBinLoader] ExtraROM skip " + path + " (no records" + (truncated ? ", truncated" : "") + ")");
                return false;
            }
            Console.WriteLine("[NkBinLoader] ExtraROM mapped records=" + records +
                " imageStart=0x" + imageStart.ToString("X8"));
            return true;
        }

        private static int WriteB000FfRecords(byte[] data, int pos, uint imageLength, IMemoryManager memory, string label, out uint firstRecord, out ulong entryPoint, out bool truncated)
        {
            firstRecord = 0;
            entryPoint = 0;
            truncated = false;
            int records = 0;
            while (pos + 12 <= data.Length)
            {
                uint recordAddress = BitConverter.ToUInt32(data, pos);
                uint recordLength = BitConverter.ToUInt32(data, pos + 4);
                pos += 12;

                if (recordAddress == 0 && recordLength == 0)
                {
                    if (pos + 4 <= data.Length)
                    {
                        entryPoint = BitConverter.ToUInt32(data, pos);
                        Console.WriteLine("[NkBinLoader] " + label + " sync record. Entry Point: 0x" + entryPoint.ToString("X"));
                    }
                    break;
                }

                if (recordLength == 0 || recordLength > imageLength || pos + recordLength > data.Length)
                {
                    Console.WriteLine("[NkBinLoader] " + label + " stop at record " + records +
                        ": addr=0x" + recordAddress.ToString("X") + " len=0x" + recordLength.ToString("X") +
                        " remaining=" + (data.Length - pos));
                    truncated = true;
                    break;
                }

                byte[] record = new byte[recordLength];
                Buffer.BlockCopy(data, pos, record, 0, (int)recordLength);
                pos += (int)recordLength;

                Console.WriteLine("[NkBinLoader] " + label + " record at 0x" + recordAddress.ToString("X") + ", Length: " + recordLength);
                memory.WriteMemory(recordAddress, record);

                if (records == 0)
                    firstRecord = recordAddress;
                records++;
            }
            return records;
        }
    }
}
