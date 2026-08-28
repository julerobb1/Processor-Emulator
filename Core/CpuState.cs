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
        private readonly List<ProcessorEmulator.Core.Emulation.MemoryRegion> _memoryMap;
        private readonly Dictionary<ProcessorEmulator.Core.Emulation.MemoryRegion, byte[]> _ramRegions = new Dictionary<ProcessorEmulator.Core.Emulation.MemoryRegion, byte[]>();

        public int PrivilegeLevel { get; set; }
        public ulong PC { get; set; }
        public IReadOnlyList<ProcessorEmulator.Core.Emulation.MemoryRegion> MemoryMap => _memoryMap;
        public bool IsLittleEndian { get; }
        public ulong? LinkedAddress { get; set; }

        public CpuState(List<ProcessorEmulator.Core.Emulation.MemoryRegion> memoryMap, bool isLittleEndian, ulong startPC = 0)
        {
            _memoryMap = memoryMap ?? new List<ProcessorEmulator.Core.Emulation.MemoryRegion>();
            IsLittleEndian = isLittleEndian;
            PC = startPC;

            // Pre-allocate memory for all initial RAM regions
            foreach (var ramRegion in _memoryMap.Where(r => r.Type == MemoryRegionType.RAM))
            {
                _ramRegions[ramRegion] = new byte[ramRegion.Size];
            }
        }

        // Compatibility overload: accept legacy ProcessorEmulator.MemoryRegion array
        public CpuState(ProcessorEmulator.MemoryRegion[] legacyMemoryMap, bool isLittleEndian, ulong startPC = 0)
        {
            var converted = new List<ProcessorEmulator.Core.Emulation.MemoryRegion>();
            if (legacyMemoryMap != null)
            {
                foreach (var lm in legacyMemoryMap)
                {
                    ulong start = lm.BaseAddress;
                    ulong size = lm.Size;
                    var name = $"LEGACY_{start:X}";
                    converted.Add(new ProcessorEmulator.Core.Emulation.MemoryRegion(name, start, size, ProcessorEmulator.Core.Emulation.MemoryRegionType.RAM));
                }
            }

            _memoryMap = converted;
            IsLittleEndian = isLittleEndian;
            PC = startPC;

            foreach (var ramRegion in _memoryMap.Where(r => r.Type == MemoryRegionType.RAM))
            {
                _ramRegions[ramRegion] = new byte[ramRegion.Size];
            }
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

        private ProcessorEmulator.Core.Emulation.MemoryRegion HandleLazyAllocation(ulong address)
        {
            const ulong pageSize = 4096;
            ulong startAddress = address / pageSize * pageSize; // Align to 4KB boundary
            
            Console.WriteLine($"[ADAPTIVE BUS]: New memory region discovered at 0x{address:X}. Mapping temporary 4KB RAM at 0x{startAddress:X}.");

            var newRegion = new ProcessorEmulator.Core.Emulation.MemoryRegion($"RAM_Lazy_{startAddress:X}", startAddress, pageSize, ProcessorEmulator.Core.Emulation.MemoryRegionType.RAM);
            _memoryMap.Add(newRegion);
            _ramRegions[newRegion] = new byte[pageSize];
            
            return newRegion;
        }

        private (ProcessorEmulator.Core.Emulation.MemoryRegion region, byte[] buffer) GetRegionAndBufferForAddress(ulong address)
        {
            foreach (var region in _memoryMap)
            {
                if (region.Contains(address))
                {
                    if (region.Type == ProcessorEmulator.Core.Emulation.MemoryRegionType.RAM)
                    {
                        return (region, _ramRegions[region]);
                    }
                    return (region, null); // For ROM/MMIO
                }
            }
            // If we get here, no region was found. Lazily allocate it.
            var newRegion = HandleLazyAllocation(address);
            return (newRegion, _ramRegions[newRegion]);
        }

        public uint ReadMemory32(ulong address)
        {
            var (region, buffer) = GetRegionAndBufferForAddress(address);
            switch (region.Type)
            {
                case MemoryRegionType.RAM:
                    ulong offset = address - region.StartAddress;
                    if (IsLittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan((int)offset));
                    return BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan((int)offset));
                
                case MemoryRegionType.MMIO:
                    Console.WriteLine($"[Warning] Unhandled MMIO Read at 0x{address:X}. Returning 0.");
                    return 0;
                
                case MemoryRegionType.ROM:
                default:
                    throw new BusErrorException($"Read from unhandled region type {region.Type} at 0x{address:X}");
            }
        }

        public void WriteMemory32(ulong address, uint value)
        {
            var (region, buffer) = GetRegionAndBufferForAddress(address);
            switch (region.Type)
            {
                case MemoryRegionType.RAM:
                    ulong offset = address - region.StartAddress;
                    if (IsLittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan((int)offset), value);
                    else BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan((int)offset), value);
                    break;
                
                case MemoryRegionType.MMIO:
                     Console.WriteLine($"[Warning] Unhandled MMIO Write at 0x{address:X} with value 0x{value:X8}.");
                     break;
                
                case MemoryRegionType.ROM:
                    throw new BusErrorException($"Attempted to write to ROM region at 0x{address:X}");

                default:
                    throw new BusErrorException($"Write to unhandled region type {region.Type} at 0x{address:X}");
            }
        }

        public void WriteMemory(ulong address, byte[] data)
        {
            var (region, buffer) = GetRegionAndBufferForAddress(address);
            switch (region.Type)
            {
                case MemoryRegionType.RAM:
                    ulong offset = address - region.StartAddress;
                    if (offset + (ulong)data.Length > (ulong)buffer.Length)
                    {
                        throw new BusErrorException($"Write at 0x{address:X} with length {data.Length} would cross memory region boundary.");
                    }
                    data.CopyTo(buffer, (long)offset);
                    break;
                
                case MemoryRegionType.ROM:
                     throw new BusErrorException($"Attempted to write to ROM region at 0x{address:X}");

                default:
                    // Defer MMIO or other types for now
                    throw new NotImplementedException($"Block write to region type {region.Type} not implemented.");
            }
        }
    }
}