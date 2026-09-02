using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        // nk.bin chain table 0x8006B9DC (16-byte records). Julian's
        // dump etc.bin is B000FF at 0x80630000. Load every dump
        // B000FF at THAT file's imageStart. Do not invent a map for
        // a chain base with no matching dump B000FF (0x81360000 in
        // this dump). Firmware has no skip for that missing image.
        // Do not invent a map or a host skip. Do not zero-fill.
        public const uint ChainTable = 0x8006B9DC;
        public const int ChainRecords = 3;

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

            Log("[NkBinLoader] Loading kernel. Image start: 0x" + imageStart.ToString("X") + ", Length: 0x" + imageLength.ToString("X"));
            BootLog.Rom("ok", "NK", "image", -1, "nk.bin", 0, imageStart, 0, imageLength,
                "B000FF map");

            int records = WriteB000FfRecords(data, pos, imageLength, memory, "nk", out uint firstRecord, out ulong entryPoint, out bool truncated);

            if (entryPoint == 0)
                entryPoint = firstRecord != 0 ? firstRecord : imageStart;

            if (entryPoint == 0)
                throw new InvalidDataException("Could not determine kernel entry point from nk.bin file.");

            BinBlkMedia.Attach(data);
            HostHardDisk.Attach();
            var mapped = new HashSet<uint> { imageStart };
            TryLoadDumpB000Ff(HostHardDisk.ExtraRomPaths, memory, mapped);
            ReportMissingChainImages(memory, mapped);
            return new NkLoadResult(entryPoint, imageStart, imageLength, records, truncated, data);
        }

        // Hunt is HostHardDisk ExtraRomPaths: every etc.bin plus any
        // other B000FF sitting next to nk.bin. Read-only. Load each
        // file's records at THAT file's imageStart (same walk as nk).
        // Skip stubs and non-B000FF (sec.bin, raven_fw.bin). Does not
        // attach BINBlk. Does not invent a chain base with no dump
        // B000FF. Firmware CreateFile of ETC.bin / BOOT.PRF / sec.bin
        // stays the Hard Disk path, not a second XIP.
        public static int TryLoadDumpB000Ff(IEnumerable<string> paths, IMemoryManager memory, HashSet<uint> mappedStarts)
        {
            int loaded = 0;
            if (memory == null || paths == null)
                return 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path) || !seen.Add(Path.GetFullPath(path)))
                    continue;
                if (TryLoadOneDumpB000Ff(path, memory, mappedStarts))
                    loaded++;
            }
            return loaded;
        }

        public static bool TryLoadExtraRom(string path, IMemoryManager memory)
        {
            var mapped = new HashSet<uint>();
            return TryLoadOneDumpB000Ff(path, memory, mapped);
        }

        private static bool TryLoadOneDumpB000Ff(string path, IMemoryManager memory, HashSet<uint> mappedStarts)
        {
            if (memory == null || string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            long len;
            try { len = new FileInfo(path).Length; }
            catch { return false; }
            if (len < 15)
            {
                Log("[NkBinLoader] ExtraROM skip " + path + " (" + len + " bytes, stub)");
                BootLog.Rom("skip", "ExtraROM", "image", -1, Path.GetFileName(path), 0, 0, 0, 0,
                    "stub");
                return false;
            }

            byte[] header;
            try
            {
                header = new byte[15];
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Read(header, 0, 15) < 15)
                    {
                        Log("[NkBinLoader] ExtraROM skip " + path + " (short read, stub)");
                        BootLog.Rom("skip", "ExtraROM", "image", -1, Path.GetFileName(path), 0, 0, 0, 0,
                            "short read, stub");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("[NkBinLoader] ExtraROM read failed " + path + ": " + ex.Message);
                BootLog.Rom("fail", "ExtraROM", "image", -1, Path.GetFileName(path), 0, 0, 0, 0,
                    ex.Message);
                return false;
            }

            if (!IsB000Ff(header))
            {
                Log("[NkBinLoader] ExtraROM skip " + path + " (" + len + " bytes, not B000FF)");
                BootLog.Rom("skip", "ExtraROM", "image", -1, Path.GetFileName(path), 0, 0, 0, 0,
                    "not B000FF");
                return false;
            }

            uint imageStart = BitConverter.ToUInt32(header, 7);
            uint imageLength = BitConverter.ToUInt32(header, 11);
            if (mappedStarts != null && mappedStarts.Contains(imageStart))
            {
                Log("[NkBinLoader] ExtraROM skip " + path +
                    " imageStart=0x" + imageStart.ToString("X8") + " (already mapped)");
                BootLog.Rom("skip", "ExtraROM", "image", -1, Path.GetFileName(path), 0, imageStart, 0, 0,
                    "already mapped");
                return false;
            }

            byte[] data;
            try
            {
                data = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Log("[NkBinLoader] ExtraROM read failed " + path + ": " + ex.Message);
                BootLog.Rom("fail", "ExtraROM", "image", -1, Path.GetFileName(path), 0, 0, 0, 0,
                    ex.Message);
                return false;
            }

            string label = Path.GetFileName(path);
            int records = WriteB000FfRecords(data, 15, imageLength, memory, label, out _, out _, out bool truncated);
            if (records <= 0)
            {
                Log("[NkBinLoader] ExtraROM skip " + path + " (no records" + (truncated ? ", truncated" : "") + ")");
                BootLog.Rom("skip", "ExtraROM", "image", -1, label, 0, imageStart, 0, 0,
                    truncated ? "no records, truncated" : "no records");
                return false;
            }
            if (mappedStarts != null)
                mappedStarts.Add(imageStart);
            Log("[NkBinLoader] ExtraROM mapped records=" + records +
                " imageStart=0x" + imageStart.ToString("X8") +
                " path=" + path);
            BootLog.Rom("ok", "ExtraROM", "image", -1, label, 0, imageStart, 0, imageLength,
                "B000FF map; XIP at imageStart");
            CeRomTocFiles.NoteExtraRom(imageStart);
            LogMappedRomHdr(memory, imageStart);
            return true;
        }

        // After a real map, ExtraROM XIP (tv2clientce.exe and the
        // rest) lives in this ROMHDR/TOC. Firmware inherit does not
        // peek that VA; +14/+18 stay leftovers unless the overlay
        // compare matches. Log only.
        private static void LogMappedRomHdr(IMemoryManager memory, uint imageStart)
        {
            if (memory == null || imageStart == 0)
                return;
            try
            {
                uint sig = memory.ReadMemory32(imageStart + 0x40);
                uint romhdr = memory.ReadMemory32(imageStart + 0x44);
                if (sig != 0x43454345 || romhdr == 0)
                    romhdr = imageStart;
                uint dllfirst = memory.ReadMemory32(romhdr);
                uint dlllast = memory.ReadMemory32(romhdr + 4);
                uint nummods = memory.ReadMemory32(romhdr + 0x10);
                uint numfiles = memory.ReadMemory32(romhdr + 0x30);
                Log("[NkBinLoader] ExtraROM ROMHDR imageStart=0x" + imageStart.ToString("X8") +
                    " cece=0x" + sig.ToString("X8") +
                    " dllfirst=0x" + dllfirst.ToString("X8") +
                    " dlllast=0x" + dlllast.ToString("X8") +
                    " nummods=" + nummods +
                    " numfiles=" + numfiles);
                if (nummods > 0 && nummods <= 128)
                {
                    for (uint i = 0; i < nummods; i++)
                    {
                        uint entry = romhdr + 0x54 + i * 32;
                        uint namePtr = memory.ReadMemory32(entry + 0x10);
                        string name = ReadAscii(memory, namePtr);
                        if (string.IsNullOrEmpty(name))
                            continue;
                        uint dest = 0;
                        uint vsize = 0;
                        uint psize = 0;
                        uint e32 = memory.ReadMemory32(entry + 0x14);
                        uint o32 = memory.ReadMemory32(entry + 0x18);
                        if (o32 != 0)
                        {
                            try
                            {
                                vsize = memory.ReadMemory32(o32);
                                psize = memory.ReadMemory32(o32 + 8);
                                dest = memory.ReadMemory32(o32 + 0x10);
                            }
                            catch
                            {
                            }
                        }
                        string why = "TOCentry type-7; dump o32 if present; do not invent 0x81360000";
                        if (IsDdiNop(name))
                        {
                            uint tocAttr = memory.ReadMemory32(entry);
                            CeRomTocFiles.NoteExtraRomModule(romhdr, entry, tocAttr);
                            CeRomTocFiles.CacheExtraRomDdiNop(memory, entry);
                            why = "LoadDriver; TOC type-7; do not invent 0x81360000";
                        }
                        if (IsMscoree(name))
                        {
                            CeRomTocFiles.CacheExtraRomMscoree(memory, entry);
                            why = "OpenExe; TOC type-7; not a FILE; e32=0x" + e32.ToString("X8") +
                                "; do not invent 0x81360000";
                        }
                        if (IsOle32(name))
                        {
                            CeRomTocFiles.CacheExtraRomOle32(memory, entry);
                            why = "OpenExe; TOC type-7; not a FILE; e32=0x" + e32.ToString("X8") +
                                "; do not invent 0x81360000";
                        }
                        BootLog.Rom("ok", "ExtraROM", "TOC", (int)i, name, 7, dest, vsize, psize, why);
                    }
                }
                uint nfiles = memory.ReadMemory32(romhdr + 0x30);
                if (nfiles > 0 && nfiles <= 128 && nummods <= 128)
                {
                    uint first = romhdr + 0x54 + nummods * 32;
                    bool sawMscoreeFile = false;
                    bool sawOle32File = false;
                    for (uint i = 0; i < nfiles; i++)
                    {
                        uint entry = first + i * 28;
                        string fname = ReadAscii(memory, memory.ReadMemory32(entry + 0x14));
                        if (string.IsNullOrEmpty(fname))
                            continue;
                        uint realSz = memory.ReadMemory32(entry + 0x0C);
                        uint compSz = memory.ReadMemory32(entry + 0x10);
                        uint load = memory.ReadMemory32(entry + 0x18);
                        string why = "FILESentry type-8; dump record; do not invent 0x81360000";
                        if (IsOle32(fname))
                        {
                            sawOle32File = true;
                            why = "FILESentry; unexpected ole32 FILE; do not invent";
                        }
                        else if (IsMscoree(fname))
                        {
                            sawMscoreeFile = true;
                            why = "FILESentry; unexpected mscoree FILE; do not invent";
                        }
                        BootLog.Rom("ok", "ExtraROM", "FILE", (int)i, fname, 8, load, realSz, compSz, why);
                        bool openFile = CeRomTocFiles.IsExtraRomOpenFile(fname);
                        if (IsTv2ClientCeExe(fname))
                            CeRomTocFiles.CacheExtraRomTv2File(memory, entry);
                        else if (openFile)
                            CeRomTocFiles.CacheExtraRomOpenFile(memory, entry, fname);
                    }
                    if (!sawMscoreeFile)
                        Log("[NkBinLoader] ExtraROM FILE table has no mscoree.dll" +
                            " (TOC[46] is the dump module; do not invent a FILE)");
                    if (!sawOle32File)
                        Log("[NkBinLoader] ExtraROM FILE table has no ole32.dll" +
                            " (TOC[34] is the dump module; do not invent a FILE)");
                }
            }
            catch (Exception ex)
            {
                Log("[NkBinLoader] ExtraROM ROMHDR log skipped: " + ex.Message);
            }
        }

        private static bool IsOle32(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length != 9)
                return false;
            return (name[0] == 'o' || name[0] == 'O')
                && (name[1] == 'l' || name[1] == 'L')
                && (name[2] == 'e' || name[2] == 'E')
                && name[3] == '3'
                && name[4] == '2'
                && name[5] == '.'
                && (name[6] == 'd' || name[6] == 'D')
                && (name[7] == 'l' || name[7] == 'L')
                && (name[8] == 'l' || name[8] == 'L');
        }

        private static bool IsMscoree(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length != 11)
                return false;
            return (name[0] == 'm' || name[0] == 'M')
                && (name[1] == 's' || name[1] == 'S')
                && (name[2] == 'c' || name[2] == 'C')
                && (name[3] == 'o' || name[3] == 'O')
                && (name[4] == 'r' || name[4] == 'R')
                && (name[5] == 'e' || name[5] == 'E')
                && (name[6] == 'e' || name[6] == 'E')
                && name[7] == '.'
                && (name[8] == 'd' || name[8] == 'D')
                && (name[9] == 'l' || name[9] == 'L')
                && (name[10] == 'l' || name[10] == 'L');
        }

        private static bool IsDdiNop(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length != 11)
                return false;
            return (name[0] == 'd' || name[0] == 'D')
                && (name[1] == 'd' || name[1] == 'D')
                && (name[2] == 'i' || name[2] == 'I')
                && name[3] == '_'
                && (name[4] == 'n' || name[4] == 'N')
                && (name[5] == 'o' || name[5] == 'O')
                && (name[6] == 'p' || name[6] == 'P')
                && name[7] == '.'
                && (name[8] == 'd' || name[8] == 'D')
                && (name[9] == 'l' || name[9] == 'L')
                && (name[10] == 'l' || name[10] == 'L');
        }

        private static bool IsTv2ClientCeExe(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length != 15)
                return false;
            return (name[0] == 't' || name[0] == 'T')
                && (name[1] == 'v' || name[1] == 'V')
                && name[2] == '2'
                && (name[3] == 'c' || name[3] == 'C')
                && (name[4] == 'l' || name[4] == 'L')
                && (name[5] == 'i' || name[5] == 'I')
                && (name[6] == 'e' || name[6] == 'E')
                && (name[7] == 'n' || name[7] == 'N')
                && (name[8] == 't' || name[8] == 'T')
                && (name[9] == 'c' || name[9] == 'C')
                && (name[10] == 'e' || name[10] == 'E')
                && name[11] == '.'
                && (name[12] == 'e' || name[12] == 'E')
                && (name[13] == 'x' || name[13] == 'X')
                && (name[14] == 'e' || name[14] == 'E');
        }

        private static string ReadAscii(IMemoryManager memory, uint addr)
        {
            if (memory == null || addr == 0)
                return "";
            var sb = new StringBuilder();
            for (int i = 0; i < 64; i += 4)
            {
                uint w = memory.ReadMemory32(addr + (uint)i);
                for (int b = 0; b < 4; b++)
                {
                    byte c = (byte)(w >> (8 * b));
                    if (c == 0)
                        return sb.ToString();
                    if (c < 32 || c > 126)
                        return sb.ToString();
                    sb.Append((char)c);
                }
            }
            return sb.ToString();
        }

        // Report only. Do not write bytes for a chain base the dump
        // did not name as B000FF.
        private static void ReportMissingChainImages(IMemoryManager memory, HashSet<uint> mappedStarts)
        {
            if (memory == null || mappedStarts == null)
                return;
            try
            {
                for (int i = 0; i < ChainRecords; i++)
                {
                    uint rec = ChainTable + (uint)(i * 16);
                    uint imageStart = memory.ReadMemory32(rec);
                    uint imageLength = memory.ReadMemory32(rec + 4);
                    if (imageStart == 0)
                        continue;
                    if (mappedStarts.Contains(imageStart))
                        continue;
                    Log("[NkBinLoader] ExtraROM missing dump B000FF for chain base=0x" +
                        imageStart.ToString("X8") + " size=0x" + imageLength.ToString("X") +
                        " (firmware has no skip; do not invent a map)");
                    BootLog.Rom("miss", "ExtraROM", "image", -1, "", 0, imageStart, 0, imageLength,
                        "no dump B000FF for chain base; do not invent a map");
                }
            }
            catch (Exception ex)
            {
                Log("[NkBinLoader] ExtraROM chain report skipped: " + ex.Message);
            }
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
                        Log("[NkBinLoader] " + label + " sync record. Entry Point: 0x" + entryPoint.ToString("X"));
                    }
                    break;
                }

                if (recordLength == 0 || recordLength > imageLength || pos + recordLength > data.Length)
                {
                    Log("[NkBinLoader] " + label + " stop at record " + records +
                        ": addr=0x" + recordAddress.ToString("X") + " len=0x" + recordLength.ToString("X") +
                        " remaining=" + (data.Length - pos));
                    truncated = true;
                    break;
                }

                byte[] record = new byte[recordLength];
                Buffer.BlockCopy(data, pos, record, 0, (int)recordLength);
                pos += (int)recordLength;

                Log("[NkBinLoader] " + label + " record at 0x" + recordAddress.ToString("X") + ", Length: " + recordLength);
                memory.WriteMemory(recordAddress, record);

                if (records == 0)
                    firstRecord = recordAddress;
                records++;
            }
            return records;
        }

        private static void Log(string line)
        {
            BootLog.Write(line);
        }
    }
}
