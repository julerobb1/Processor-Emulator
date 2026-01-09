namespace ProcessorEmulator.Emulation
{
    public interface IBusDevice
    {
        uint StartAddress { get; }
        uint Size { get; }
        uint Read32(uint offset);
        void Write32(uint offset, uint value);
        byte Read8(uint offset);
        void Write8(uint offset, byte value);

    }
}
