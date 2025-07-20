// You must cut down the mightiest tree in the forest with a herring!
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using ProcessorEmulator.Tools;

namespace ProcessorEmulator
{
    public static class ArchiveExtractor
    {
        // Central repository of all known signatures for carving and analysis
        private static Dictionary<string, byte[]> GetSignatures()
        {
            return new Dictionary<string, byte[]>
            {
                ["kernel"]   = new byte[]{0x1F,0x8B},
                ["elf"]      = new byte[]{0x7F,(byte)'E',(byte)'L',(byte)'F'},
                ["uboot"]    = new byte[]{0x27,0x05,0x19,0x56},
                ["squashfs"] = System.Text.Encoding.ASCII.GetBytes("hsqs"),
                ["jffs2"]    = new byte[]{0xEF,0x53},
                ["lzma"]     = new byte[]{0x5D,0x00,0x00},
                ["tar"]      = System.Text.Encoding.ASCII.GetBytes("ustar"),
                ["zip"]      = new byte[]{0x50,0x4B,0x03,0x04},
                ["jpeg"]     = new byte[]{0xFF,0xD8,0xFF},
                ["png"]      = new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A},
                ["xz"]       = new byte[]{0xFD,(byte)'7',(byte)'z',(byte)'X',(byte)'Z',0x00},
                ["lz4"]      = new byte[]{0x04,0x22,0x4D,0x18},
                ["bzip2"]    = new byte[]{0x42,0x5A,0x68},
                ["rar"]      = new byte[]{0x52,0x61,0x72,0x21,0x1A,0x07,0x00},
                ["zstd"]     = new byte[]{0x28,0xB5,0x2F,0xFD},
                ["lzop"]     = new byte[]{0x89,0x4C,0x5A,0x4F,0x00,0x0D,0x0A,0x1A,0x0A},
                ["7z"]       = new byte[]{0x37,0x7A,0xBC,0xAF,0x27,0x1C},
                ["exe"]      = new byte[]{0x4D,0x5A},
                ["pdf"]      = System.Text.Encoding.ASCII.GetBytes("%PDF"),
                ["bmp"]      = new byte[]{0x42,0x4D},
                ["macho32"]  = new byte[]{0xCE,0xFA,0xED,0xFE},
                ["macho64"]  = new byte[]{0xCF,0xFA,0xED,0xFE},
                ["iso9660"]  = System.Text.Encoding.ASCII.GetBytes("CD001"),
                ["cab"]      = System.Text.Encoding.ASCII.GetBytes("MSCF"),
                ["trx"]      = System.Text.Encoding.ASCII.GetBytes("HDR0"),    // Broadcom TRX container
                ["xmi"]      = System.Text.Encoding.ASCII.GetBytes("XMI\0"),   // Mediaroom XMI package header
                // Additional kernel and filesystem-specific headers
                ["bzImage"]  = System.Text.Encoding.ASCII.GetBytes("HdrB"),            // Linux bzImage header
                ["yaffs"]    = System.Text.Encoding.ASCII.GetBytes("Yaffs")            // YAFFS filesystem superblock
            };
        }
        /// <summary>
        /// Extracts an archive or raw disk image to the specified directory.
        /// For .bin images, splits MBR partitions and extracts firmware sections by signature.
        /// Otherwise invokes 7z.
        /// </summary>
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var ext = Path.GetExtension(archivePath);
            if (string.IsNullOrEmpty(ext) || ext.Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
                // Use binwalk for extraction if available
                try
                {
                    var binwalk = new ProcessStartInfo("binwalk", $"-eM \"{archivePath}\" -C \"{outputDir}\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var bw = Process.Start(binwalk);
                    bw.WaitForExit();
                    Console.WriteLine(bw.StandardOutput.ReadToEnd());
                    Console.Error.WriteLine(bw.StandardError.ReadToEnd());
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ArchiveExtractor] binwalk extraction failed: {ex.Message}");
                }

                // Fallback: partition split and signature carving
                ExtractPartitions(archivePath, outputDir);
                ExtractFirmwareSections(archivePath, outputDir);
                return;
            }

            string exe7z = Resolve7zExecutable();
            if (string.IsNullOrEmpty(exe7z))
                throw new InvalidOperationException("7z.exe not found. Please install 7-Zip.");

            var psi = new ProcessStartInfo(exe7z, $"x -y -o\"{outputDir}\" \"{archivePath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc.WaitForExit();
            Sanitize(outputDir);
        }

        public static void ExtractAndAnalyze(string archivePath, string outputDir)
        {
            ExtractArchive(archivePath, outputDir);
            FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, outputDir);
        }
        /// <summary>
        /// Analyzes a file buffer for known signatures and reports offsets without extracting.
        /// </summary>
        public static void AnalyzeArchive(string archivePath)
        {
            var buf = File.ReadAllBytes(archivePath);
            var sigs = GetSignatures();
            Console.WriteLine($"Analyzing {archivePath} for known signatures...");
            foreach (var kv in sigs)
            {
                var name = kv.Key;
                var patt = kv.Value;
                int pos = 0;
                bool foundAny = false;
                while (pos < buf.Length)
                {
                    int off = FindPattern(buf, patt, pos);
                    if (off < 0) break;
                    Console.WriteLine($"  Found '{name}' at 0x{off:X}");
                    foundAny = true;
                    pos = off + 1;
                }
                if (!foundAny)
                    Console.WriteLine($"  No '{name}' signatures found.");
            }
        }
        //suppose that little things behave very differently than anything big
        //nothing's really as it seems , its so wonderfully different than anything big
        // the world is a dynmic mess of jiggling things its hard to believe
        
        public static void ExtractPartitions(string img, string outDir)
        {
            var buf = File.ReadAllBytes(img);
            if (buf.Length < 512 || buf[510] != 0x55 || buf[511] != 0xAA)
                return;

            for (int i = 0; i < 4; i++)
            {
                int off = 0x1BE + i * 16;
                byte type = buf[off + 4];
                uint lba = BitConverter.ToUInt32(buf, off + 8);
                uint sec = BitConverter.ToUInt32(buf, off + 12);
                if (type == 0 || sec == 0) continue;

                long start = (long)lba * 512;
                long length = (long)sec * 512;
                if (start + length > buf.LongLength) length = buf.LongLength - start;

                string partFile = Path.Combine(outDir, $"part{i + 1}_type{type:X2}.bin");
                File.WriteAllBytes(partFile, buf.Skip((int)start).Take((int)length).ToArray());
            }
        }

    public static void ExtractFirmwareSections(string img, string outDir)
        {
            var buf = File.ReadAllBytes(img);
            var sigs = new Dictionary<string, byte[]>
            {
                ["kernel"]   = new byte[]{0x1F,0x8B},                                       // GZIP kernel
                ["elf"]      = new byte[]{0x7F,(byte)'E',(byte)'L',(byte)'F'},               // ELF executable
                ["uboot"]    = new byte[]{0x27,0x05,0x19,0x56},                              // U-Boot image header
                ["squashfs"] = System.Text.Encoding.ASCII.GetBytes("hsqs"),                // SquashFS superblock
                ["jffs2"]    = new byte[]{0xEF,0x53},                                      // JFFS2 magic
                ["lzma"]     = new byte[]{0x5D,0x00,0x00},                                  // LZMA header
                ["tar"]      = System.Text.Encoding.ASCII.GetBytes("ustar"),              // POSIX tar
                ["zip"]      = new byte[]{0x50,0x4B,0x03,0x04},                            // ZIP archive
                ["jpeg"]     = new byte[]{0xFF,0xD8,0xFF},                                 // JPEG image
                ["png"]      = new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A},       // PNG image
                ["xz"]       = new byte[]{0xFD,(byte)'7',(byte)'z',(byte)'X',(byte)'Z',0x00}, // XZ compressed
                ["lz4"]      = new byte[]{0x04,0x22,0x4D,0x18},                             // LZ4 frame
                ["bzip2"]    = new byte[]{0x42,0x5A,0x68},                                  // BZIP2 stream
                ["rar"]      = new byte[]{0x52,0x61,0x72,0x21,0x1A,0x07,0x00},              // RAR archive
                ["zstd"]     = new byte[]{0x28,0xB5,0x2F,0xFD},                             // Zstandard frame
                ["lzop"]     = new byte[]{0x89,0x4C,0x5A,0x4F,0x00,0x0D,0x0A,0x1A,0x0A},   // LZOP header
                // Additional common magics
                ["7z"]       = new byte[]{0x37,0x7A,0xBC,0xAF,0x27,0x1C},               // 7-Zip archive header
                ["exe"]      = new byte[]{0x4D,0x5A},                                     // Windows MZ executable
                ["pdf"]      = System.Text.Encoding.ASCII.GetBytes("%PDF"),             // PDF document
                ["bmp"]      = new byte[]{0x42,0x4D},                                     // BMP image
                ["macho32"]  = new byte[]{0xCE,0xFA,0xED,0xFE},                            // Mach-O 32-bit
                ["macho64"]  = new byte[]{0xCF,0xFA,0xED,0xFE},                            // Mach-O 64-bit
                ["iso9660"]  = System.Text.Encoding.ASCII.GetBytes("CD001"),             // ISO9660 volume descriptor
                ["cab"]      = System.Text.Encoding.ASCII.GetBytes("MSCF")               // CAB archive header
            };

            var found = sigs.Select(kv=>(kv.Key, off:FindPattern(buf,kv.Value,0)))
                           .Where(x=>x.off>=0)
                           .OrderBy(x=>x.off)
                           .ToList();
            // If there is data before the first signature, extract it as preamble
            if (found.Count > 0 && found[0].off > 0)
            {
                int len0 = found[0].off;
                Console.WriteLine($"[ArchiveExtractor] extracting 'preamble' at 0x0 ({len0} bytes)");
                File.WriteAllBytes(Path.Combine(outDir, $"preamble_0x0.bin"), buf.Take(len0).ToArray());
            }
            for (int i = 0; i < found.Count; i++)
            {
                var (name, off) = found[i];
                int end = (i + 1 < found.Count) ? found[i + 1].off : buf.Length;
                int length = end - off;
                Console.WriteLine($"[ArchiveExtractor] extracting '{name}' at 0x{off:X} ({length} bytes)");
                var outputFile = Path.Combine(outDir, $"{name}_{off:X}.bin");
                File.WriteAllBytes(outputFile, buf.Skip(off).Take(length).ToArray());
                // If YAFFS filesystem detected, run YAFFS extractor stub
                if (name.Equals("yaffs", StringComparison.OrdinalIgnoreCase))
                {
                    var yaffsOut = Path.Combine(outDir, $"{name}_{off:X}_extracted");
                    YaffsExtractor.ExtractYaffs(outputFile, yaffsOut);
                }
                // If TRX container detected, run TRX extractor
                if (name.Equals("trx", StringComparison.OrdinalIgnoreCase))
                {
                    var trxOut = Path.Combine(outDir, $"{name}_{off:X}_extracted");
                    ProcessorEmulator.Tools.TrxExtractor.ExtractTrx(outputFile, trxOut);
                }
                // If Mediaroom XMI detected, run XMI extractor stub
                if (name.Equals("xmi", StringComparison.OrdinalIgnoreCase))
                {
                    var xmiOut = Path.Combine(outDir, $"{name}_{off:X}_extracted");
                    ProcessorEmulator.Tools.XmiExtractor.ExtractXmi(outputFile, xmiOut);
                }
            }
        }

        private static int FindPattern(byte[] data, byte[] patt, int start)
        {
            for(int i=start;i<=data.Length-patt.Length;i++)
            {
                bool ok=true;
                for(int j=0;j<patt.Length;j++) if(data[i+j]!=patt[j]){ok=false;break;}
                if(ok) return i;
            }
            return -1;
        }

        private static void Sanitize(string dir)
        {
            try
            {
                string root=Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar)+"\\";
                foreach(var f in Directory.GetFiles(dir,"*",SearchOption.AllDirectories))
                    if(!Path.GetFullPath(f).StartsWith(root,StringComparison.OrdinalIgnoreCase))
                        File.Delete(f);
            }catch{}
        }

        private static string Resolve7zExecutable()
        {
            string[] cand={
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"7-Zip","7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"7-Zip","7z.exe")
            };
            foreach(var p in cand) if(File.Exists(p)) return p;
            var env=Environment.GetEnvironmentVariable("PATH");
            foreach(var d in (env??"").Split(';'))
                try{var ex=Path.Combine(d.Trim(),"7z.exe");if(File.Exists(ex))return ex;}catch{}
            var dlg=new OpenFileDialog{Title="Locate 7z.exe",Filter="7z.exe"};
            return dlg.ShowDialog()==true?dlg.FileName:null;
        }
    }
}