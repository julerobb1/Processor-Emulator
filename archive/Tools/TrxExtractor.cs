using System;
using System.IO;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Extracts Broadcom TRX container images (used by some embedded routers and STBs).
    /// TRX header format: 4-byte magic 'HDR0', 4-byte length (including header), CRC32, flags, version, 3*uint32 padding.
    /// </summary>
    public static class TrxExtractor
    {
        public static void ExtractTrx(string trxPath, string outputDir)
        {
            Console.WriteLine($"[TrxExtractor] Extracting TRX: {trxPath}");
            Directory.CreateDirectory(outputDir);

            using var fs = File.OpenRead(trxPath);
            using var br = new BinaryReader(fs);
            // Read header
            var magic = new string(br.ReadChars(4));
            if (magic != "HDR0")
                throw new InvalidDataException("Not a valid TRX image (missing HDR0 magic)");
            uint totalLen = br.ReadUInt32();
            uint crc = br.ReadUInt32();
            uint flags = br.ReadUInt32();
            uint version = br.ReadUInt32();
            // Skip padding (12 bytes)
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();

            long headerSize = fs.Position;
            long payloadLen = (totalLen > headerSize) ? (totalLen - headerSize) : (fs.Length - headerSize);

            var payload = br.ReadBytes((int)payloadLen);
            var outFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(trxPath) + "_payload.bin");
            File.WriteAllBytes(outFile, payload);
            Console.WriteLine($"[TrxExtractor] Payload extracted to: {outFile}");
        }
    }
}
