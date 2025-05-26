using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ProcessorEmulator.Tools.FileSystems
{
    public class UverseFileSystem
    {
        private class SignatureFile
        {
            public string Signature { get; set; }
            public Dictionary<string, FileEntry> Entries { get; set; }
        }

        private class FileEntry
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string Hash { get; set; }
            public string Type { get; set; }  // BOOTABLE, NONE, SELF
        }

        private Dictionary<string, SignatureFile> signatureFiles = new Dictionary<string, SignatureFile>();
        private Dictionary<string, byte[]> fileCache = new Dictionary<string, byte[]>();
        private Dictionary<string, string> bootComponents = new Dictionary<string, string>();

        public void ParseSignatureFile(string sigPath)
        {
            string content = File.ReadAllText(sigPath);
            var sigFile = new SignatureFile
            {
                Entries = new Dictionary<string, FileEntry>()
            };

            string[] lines = content.Split('\n');
            sigFile.Signature = lines[0].Trim();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var entry = new FileEntry
                    {
                        Name = parts[0],
                        Size = parts[1],
                        Hash = parts[2],
                        Type = parts.Length > 3 ? parts[3] : "NONE"
                    };
                    sigFile.Entries[entry.Name.ToLowerInvariant()] = entry;

                    if (entry.Type == "BOOTABLE")
                    {
                        bootComponents[entry.Name] = entry.Hash;
                    }
                }
            }

            signatureFiles[Path.GetFileName(sigPath).ToLowerInvariant()] = sigFile;
        }

        public void LoadBzFile(string bzPath)
        {
            string fileName = Path.GetFileName(bzPath).ToLowerInvariant();
            byte[] content = File.ReadAllBytes(bzPath);
            fileCache[fileName] = content;
        }

        public bool VerifyFile(string fileName, byte[] content)
        {
            foreach (var sigFile in signatureFiles.Values)
            {
                if (sigFile.Entries.TryGetValue(fileName.ToLowerInvariant(), out FileEntry entry))
                {
                    using (var sha1 = SHA1.Create())
                    {
                        byte[] hash = sha1.ComputeHash(content);
                        string actualHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        return actualHash == entry.Hash.ToLowerInvariant();
                    }
                }
            }
            return false;
        }

        public void ParseMediaroomStructure(string basePath)
        {
            // Parse TV2ClientCE structure
            if (Directory.Exists(Path.Combine(basePath, "TV2ClientCE")))
            {
                ParseTV2ClientCE(Path.Combine(basePath, "TV2ClientCE"));
            }

            // Parse certificate and private data files
            foreach (var file in Directory.GetFiles(basePath, "*.dat"))
            {
                string fileName = Path.GetFileName(file).ToLowerInvariant();
                if (fileName.Contains("cert") || fileName.Contains("priv"))
                {
                    LoadSecureFile(file);
                }
            }
        }

        private void ParseTV2ClientCE(string tv2Path)
        {
            // Handle content folder
            string contentPath = Path.Combine(tv2Path, "content");
            if (Directory.Exists(contentPath))
            {
                foreach (var file in Directory.GetFiles(contentPath, "*.*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file).ToLowerInvariant();
                    fileCache[fileName] = File.ReadAllBytes(file);
                }
            }
        }

        private void LoadSecureFile(string filePath)
        {
            byte[] content = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath).ToLowerInvariant();
            fileCache[fileName] = content;
        }

        public byte[] GetBootableComponent(string name)
        {
            if (bootComponents.ContainsKey(name) && fileCache.ContainsKey(name.ToLowerInvariant()))
            {
                return fileCache[name.ToLowerInvariant()];
            }
            throw new FileNotFoundException($"Bootable component {name} not found");
        }

        public byte[] GetFile(string name)
        {
            if (fileCache.TryGetValue(name.ToLowerInvariant(), out byte[] content))
            {
                return content;
            }
            throw new FileNotFoundException($"File {name} not found");
        }

        public bool IsBootableComponent(string name)
        {
            return bootComponents.ContainsKey(name);
        }

        public IEnumerable<string> GetBootableComponents()
        {
            return bootComponents.Keys;
        }
    }
}