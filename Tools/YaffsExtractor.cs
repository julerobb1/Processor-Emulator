using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Stub for YAFFS filesystem extraction.
    /// A pure C# implementation can be added without external licensing.
    /// </summary>
    public static class YaffsExtractor
    {
        /// <summary>
        /// Extracts a YAFFS1 filesystem image (raw dump with spare area) to the given output directory.
        /// </summary>
        public static void ExtractYaffs(string yaffsImagePath, string outputDir)
        {
            // Determine YAFFS page layout: support 2048+64 (YAFFS1) or 2048+16 (YAFFS2)
            byte[] raw = File.ReadAllBytes(yaffsImagePath);

            // Auto-detect page and spare sizes for YAFFS1 and YAFFS2
            int page = 2048;
            int spareSize = 64;
            var candidates = new List<(int p, int s)> { (512, 16), (512, 64), (2048, 16), (2048, 64) };
            var match = candidates.FirstOrDefault(c => raw.Length % (c.p + c.s) == 0);
            if (match != default)
            {
                page = match.p;
                spareSize = match.s;
            }

            int chunkSize = page;
            int offset = 0;

            Console.WriteLine($"[YaffsExtractor] Detected page size: {page}, spare size: {spareSize}");
            Console.WriteLine($"[YaffsExtractor] Extracting YAFFS image: {yaffsImagePath}");
            Directory.CreateDirectory(outputDir);

            // Map object ID to relative path
            var objectPaths = new Dictionary<int, string> { { 1, string.Empty } };
            // Collect file chunks by object ID
            var fileChunks = new Dictionary<int, List<(int chunkId, byte[] data)>>();

            while (offset + chunkSize + spareSize <= raw.Length)
            {
                // Data area
                byte[] data = new byte[chunkSize];
                Array.Copy(raw, offset, data, 0, chunkSize);
                // Spare/OOB area
                byte[] spare = new byte[spareSize];
                Array.Copy(raw, offset + chunkSize, spare, 0, spareSize);
                offset += chunkSize + spareSize;

                int objId = BitConverter.ToInt32(spare, 0);
                int chunkId = BitConverter.ToInt32(spare, 4);
                int bytesUsed = BitConverter.ToInt32(spare, 8);
                int parentId = BitConverter.ToInt32(spare, 12);
                int fileLength = BitConverter.ToInt32(spare, 16);

                if (chunkId == -1)
                {
                    // Object header
                    using var ms = new MemoryStream(data);
                    using var br = new BinaryReader(ms, Encoding.UTF8);
                    int objType = br.ReadInt32();          // 1=directory, 2=file
                    int hdrParent = br.ReadInt32();       // parent object ID
                    int hdrLength = br.ReadInt32();       // total file length
                    byte[] nameBytes = br.ReadBytes(256);
                    string name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');

                    string parentPath = objectPaths.ContainsKey(hdrParent) ? objectPaths[hdrParent] : string.Empty;
                    string objPath = Path.Combine(parentPath, name);
                    objectPaths[objId] = objPath;

                    string fullPath = Path.Combine(outputDir, objPath);
                    if (objType == 1)
                    {
                        // Directory
                        Directory.CreateDirectory(fullPath);
                    }
                    else if (objType == 2)
                    {
                        // File: prepare chunk list
                        fileChunks[objId] = new List<(int, byte[])>();
                    }
                }
                else if (fileChunks.ContainsKey(objId))
                {
                    // File data chunk
                    int useLen = (bytesUsed > 0 && bytesUsed <= chunkSize) ? bytesUsed : chunkSize;
                    byte[] chunkData = new byte[useLen];
                    Array.Copy(data, 0, chunkData, 0, useLen);
                    fileChunks[objId].Add((chunkId, chunkData));
                }
            }

            // Write out files in order
            foreach (var kv in fileChunks)
            {
                int id = kv.Key;
                var chunks = kv.Value.OrderBy(c => c.chunkId);
                string relPath = objectPaths.ContainsKey(id) ? objectPaths[id] : id.ToString();
                string outPath = Path.Combine(outputDir, relPath);
                string dir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                using var fs = File.Create(outPath);
                foreach (var c in chunks)
                {
                    fs.Write(c.data, 0, c.data.Length);
                }
                Console.WriteLine($"[YaffsExtractor] Wrote file: {relPath}");
            }

            Console.WriteLine("[YaffsExtractor] Extraction complete.");
        }
    }
}
