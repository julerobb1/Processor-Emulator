namespace ProcessorEmulator.Emulation
{
    public interface IChipsetEmulator
    {
        string ChipsetName { get; }
        bool Initialize(string configPath);
    byte[] ReadRegister(long address);
    void WriteRegister(long address, byte[] data);
        // Add other chipset-specific functions here
    }
}
