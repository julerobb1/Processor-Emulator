namespace ProcessorEmulator.Core.Emulation
{
    public enum MemoryRegionType
    {
        RAM,
        ROM,
        MMIO
    }

    public class MemoryRegion
    {
        public string Name { get; }
        public ulong StartAddress { get; }
        public ulong Size { get; }
        public MemoryRegionType Type { get; }
        public ulong EndAddress => StartAddress + Size - 1;

        public MemoryRegion(string name, ulong startAddress, ulong size, MemoryRegionType type)
        {
            Name = name;
            StartAddress = startAddress;
            Size = size;
            Type = type;
        }

        public bool Contains(ulong address)
        {
            return address >= StartAddress && address <= EndAddress;
        }
    }
}
