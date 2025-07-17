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
        /// <summary>
        /// Extracts an archive or raw disk image to the specified directory.
        /// For .bin images, splits MBR partitions and extracts firmware sections by signature.
        /// Otherwise invokes 7z.
        /// </summary>
        public static void ExtractArchive(string archivePath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            if (Path.GetExtension(archivePath).Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
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

        private static void ExtractPartitions(string img, string outDir)
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

                string partFile = Path.Combine(outDir, $"part{i+1}_type{type:X2}.bin");
                File.WriteAllBytes(partFile, buf.Skip((int)start).Take((int)length).ToArray());
            }
        }

        private static void ExtractFirmwareSections(string img, string outDir)
        {
            var buf = File.ReadAllBytes(img);
            var sigs = new Dictionary<string, byte[]>
            {
                ["kernel"] = new byte[]{0x1F,0x8B},
                ["elf"]    = new byte[]{0x7F,(byte)'E',(byte)'L',(byte)'F'}
            };

            var found = sigs.Select(kv=>(kv.Key, off:FindPattern(buf,kv.Value,0)))
                           .Where(x=>x.off>=0)
                           .OrderBy(x=>x.off)
                           .ToList();
            for(int i=0;i<found.Count;i++)
            {
                var (name,off)=found[i];
                int end=(i+1<found.Count)?found[i+1].off:buf.Length;
                File.WriteAllBytes(Path.Combine(outDir,$"{name}_{off:X}.bin"),
                    buf.Skip(off).Take(end-off).ToArray());
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