using System;
using ProcessorEmulator.Tools;
using System.IO;
using Unicorn;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Implements IChipsetEmulator using Unicorn.NET stubs.
    /// </summary>
    public class UnicornChipsetEmulator : IChipsetEmulator
    {
        // Key to lookup chip info from database
        private string chipKey;
        private global::Unicorn.UnicornEngine engine;

        /// <summary>
        /// Friendly name for the emulator instance.
        /// </summary>
        public string ChipsetName => engine != null 
            ? $"Unicorn_{engine.Arch}_{engine.Mode}" 
            : "Unicorn_Unknown";

        /// <summary>
        /// Initializes the Unicorn engine with default settings or from a config file.
        /// If configPath matches a known chip key, lookup reference info.
        /// </summary>
        public bool Initialize(string configPath)
        {
            try
            {
                // Store chip key for reference lookup
                chipKey = Path.GetFileNameWithoutExtension(configPath);
                // TODO: parse configPath to customize arch and mode
                engine = new global::Unicorn.UnicornEngine(global::Unicorn.UnicornArch.X86, global::Unicorn.UnicornMode.Bit32);
                // Map a default 4MB memory region at 0x1000
                engine.Memory.Map(0x1000, 4 * 1024 * 1024);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns human-friendly chip reference or contribution message.
        /// </summary>
        public string ChipInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(chipKey))
                    return null;
                return ChipReferenceManager.GetInfo(chipKey) ?? ChipReferenceManager.GetContributionMessage(chipKey);
            }
        }

        /// <summary>
        /// Reads 4 bytes from the specified memory address.
        /// </summary>
        public byte[] ReadRegister(uint address)
        {
            if (engine == null)
                throw new InvalidOperationException("Engine not initialized");
            return engine.Memory.Read(address, 4);
        }

        /// <summary>
        /// Writes data to the specified memory address.
        /// </summary>
        public void WriteRegister(uint address, byte[] data)
        {
            if (engine == null)
                throw new InvalidOperationException("Engine not initialized");
            engine.Memory.Write(address, data);
        }
    }
}
