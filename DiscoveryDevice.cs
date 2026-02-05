using System;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator
{
    public class DiscoveryDevice : IBusDevice
    {
        public uint StartAddress => 0x1FC00100; // Just after the Reset Vector
        public uint Size => 0x100;

        // A simple table defining the "Universe" of this machine
        private readonly uint[] _configTable = new uint[] {
            0x4D495053, // Magic: "MIPS"
            0x10400000, // UART Base
            0x10000000, // Framebuffer Base
            640,        // Screen Width
            480,        // Screen Height
            0x0002A000  // Chipset ID (BCM7405)
        };

        public uint Read32(uint offset)
        {
            int index = (int)(offset / 4);
            return (index < _configTable.Length) ? _configTable[index] : 0;
        }

        public void Write32(uint offset, uint value) { /* Read Only */ }
        public void Write16(uint address, ushort value) { /* Read Only */ }
        public void Write8(uint address, byte value) { /* Read Only */ }
        public ushort Read16(uint address) { return 0; }
        public byte Read8(uint address) { return 0; }
    }
}
