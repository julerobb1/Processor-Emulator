namespace ProcessorEmulator.Tools
{
    public interface IChipsetEmulator
    {
        string ChipsetName { get; }
        bool Initialize(string configPath);
        byte[] ReadRegister(uint address);
        void WriteRegister(uint address, byte[] data);
        // Add other chipset-specific functions here
    }
}
