using System;

namespace ProcessorEmulator
{
    public class PartitionEntry
    {
        public Guid PartitionTypeGuid { get; set; }
        public ulong StartingLBA { get; set; }
        public ulong EndingLBA { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
