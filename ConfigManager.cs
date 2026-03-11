

using System.Text.Json;

namespace ProcessorEmulator
{
    /// <summary>
    /// Small helper for reading/writing a local JSON configuration file.
    /// The file lives next to the executable and is intentionally not
    /// checked in to source control (add to .gitignore if necessary).
    ///
    /// <para>Example <c>config.json</c> contents:</para>
    /// <code>
    /// {
    ///   "FirmwarePath": "C:\\UverseFirmware\\nk.exe",
    ///   "UseLiveNetwork": false,
    ///   "DnsRedirects": {
    ///       "xcal.tv": "127.0.0.1",
    ///       "xconf.comcast.net": "127.0.0.1"
    ///   }
    /// }
    /// </code>
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private static AppConfig _config;

        public static AppConfig Config => _config ??= Load();

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigFilePath))
                return new AppConfig();

            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                // if the file is corrupt for any reason we just return defaults
                return new AppConfig();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch
            {
                // best effort only
            }
        }
    }

    public class AppConfig
    {
        /// <summary>
        /// Path to the folder or file containing the firmware/kernel image.
        /// This value is provided by the user and is never committed to git.
        /// </summary>
        public string FirmwarePath { get; set; } = string.Empty;

        /// <summary>
        /// When true the emulator will allow network traffic to reach the real
        /// Comcast infrastructure rather than intercepting it locally.  This
        /// is useful for exercising live authentication or retrieving real
        /// guide data.
        /// </summary>
        public bool UseLiveNetwork { get; set; } = false;

        /// <summary>
        /// Optional DNS overrides used when the emulator is running in "local"
        /// mode.  Keys are hostnames and values are IP addresses (e.g.
        /// "xcal.tv": "127.0.0.1").  The <see cref="NetworkRedirector" />
        /// will apply these mappings to the Windows hosts file or dnsmasq.
        /// </summary>
        public Dictionary<string,string> DnsRedirects { get; set; } = new();
    }
}