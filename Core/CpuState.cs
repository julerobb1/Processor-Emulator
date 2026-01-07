using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using ProcessorEmulator.Core.Emulation;

namespace ProcessorEmulator.Core
{
    public class CpuState : ICpuState, IMemoryManager
    {
        private readonly Dictionary<string, ulong> _registers = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        private readonly byte[] _ram;

        public int PrivilegeLevel { get; set; }
        public ulong PC { get; set; }
        public IReadOnlyList<MemoryRegion> MemoryMap { get; }
        public bool IsLittleEndian { get; }

        public CpuState(List<MemoryRegion> memoryMap, bool isLittleEndian, ulong startPC = 0)
        {
            MemoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
            IsLittleEndian = isLittleEndian;
            PC = startPC;

            var ramRegion = MemoryMap.FirstOrDefault(r => r.Type == MemoryRegionType.RAM);
            if (ramRegion == null) throw new ArgumentException("Memory map must contain at least one RAM region.");
            _ram = new byte[ramRegion.Size];
        }

        public ulong GetRegister(string name, BitWidth width)
        {
            _registers.TryGetValue(name, out ulong value);
            return value;
        }

        public void SetRegister(string name, ulong value, BitWidth width)
        {
            _registers[name] = value;
        }

        private MemoryRegion GetRegionForAddress(ulong address)
        {
            foreach (var region in MemoryMap)
            {
                if (region.Contains(address)) return region;
            }
            throw new BusErrorException($"Access to unmapped address 0x{address:X}");
        }

        public uint ReadMemory32(ulong address)
        {
            var region = GetRegionForAddress(address);
            switch (region.Type)
            {
                case MemoryRegionType.RAM:
                    ulong offset = address - region.StartAddress;
                    if (IsLittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(_ram.AsSpan((int)offset));
                    return BinaryPrimitives.ReadUInt32BigEndian(_ram.AsSpan((int)offset));
                
                case MemoryRegionType.MMIO:
                    throw new NotImplementedException($"MMIO Read at 0x{address:X} not implemented.");
                
                case MemoryRegionType.ROM: // Assuming ROM is also backed by the RAM array for now
                default:
                    throw new BusErrorException($"Read from unhandled region type {region.Type} at 0x{address:X}");
            }
        }

        public void WriteMemory32(ulong address, uint value)
        {
            var region = GetRegionForAddress(address);
            switch (region.Type)
            {
                case MemoryRegionType.RAM:
                    ulong offset = address - region.StartAddress;
                    if (IsLittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(_ram.AsSpan((int)offset), value);
                    else BinaryPrimitives.WriteUInt32BigEndian(_ram.AsSpan((int)offset), value);
                    break;
                
                case MemoryRegionType.MMIO:
                    throw new NotImplementedException($"MMIO Write at 0x{address:X} not implemented.");
                
                case MemoryRegionType.ROM:
                    throw new BusErrorException($"Attempted to write to ROM region at 0x{address:X}");

                default:
                    throw new BusErrorException($"Write to unhandled region type {region.Type} at 0x{address:X}");
            }
        }
    }
}
