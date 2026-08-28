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

            ulong entryPoint = 0;
            uint firstRecord = 0;
            int records = 0;
            bool truncated = false;

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
                        Console.WriteLine($"[NkBinLoader] Found sync record. Entry Point: 0x{entryPoint:X}");
                    }
                    break;
                }

                if (recordLength == 0 || recordLength > imageLength || pos + recordLength > data.Length)
                {
                    Console.WriteLine($"[NkBinLoader] Stopping at record {records}: addr=0x{recordAddress:X} len=0x{recordLength:X} remaining={data.Length - pos}");
                    truncated = true;
                    break;
                }

                byte[] record = new byte[recordLength];
                Buffer.BlockCopy(data, pos, record, 0, (int)recordLength);
                pos += (int)recordLength;

                Console.WriteLine($"[NkBinLoader] Loading record at 0x{recordAddress:X}, Length: {recordLength}");
                memory.WriteMemory(recordAddress, record);

                if (records == 0)
                    firstRecord = recordAddress;
                records++;
            }

            if (entryPoint == 0)
                entryPoint = firstRecord != 0 ? firstRecord : imageStart;

            if (entryPoint == 0)
                throw new InvalidDataException("Could not determine kernel entry point from nk.bin file.");

            BinBlkMedia.Attach(data);
            HostHardDisk.Attach();
            return new NkLoadResult(entryPoint, imageStart, imageLength, records, truncated, data);
        }
    }
}
