using System.Collections.Generic;

namespace ProcessorEmulator.Tools
{
    public interface IManifestProvider
    {
        bool IsManifestPresent(string imagePath);
        IEnumerable<ManifestEntry> ParseManifest(string manifestPath);
    }

    public class ManifestEntry
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public string Flags { get; set; }
    }
}
