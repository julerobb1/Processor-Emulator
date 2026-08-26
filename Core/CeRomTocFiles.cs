using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // GetRomFileInfo type 1 already copies a TOC module into WIN32_FIND_DATA.
    // Firmware filesys/coredll only walks type 2 (FILESentry). After those
    // eight names, continue the same walk with TOC modules so \Windows\*.dll
    // sees what is already in the image. No extra bytes are written to ROM.
    public static class CeRomTocFiles
    {
        public const uint GetRomFileInfo = 0x80045C4C;
        public const uint EcecTocPtr = 0x80010044;
        public const uint TypeFiles = 2;
        public const uint RomHdrNumMods = 0x10;
        public const uint RomHdrType2FileCount = 0x30;
        public const uint TocFirst = 0x54;
        public const uint TocEntrySize = 32;
        public const uint FindNameOff = 0x28;

        public static bool TryServeType2Module(MipsBus bus, uint type, uint findData, uint index, out uint v0)
        {
            v0 = 0;
            if (bus == null || type != TypeFiles || findData == 0)
                return false;

            uint toc;
            uint nfiles;
            uint nmods;
            try
            {
                toc = bus.Read32(EcecTocPtr);
                if (toc == 0)
                    return false;
                nfiles = bus.Read32(toc + RomHdrType2FileCount);
                nmods = bus.Read32(toc + RomHdrNumMods);
            }
            catch
            {
                return false;
            }

            if (index < nfiles)
                return false;
            uint mod = index - nfiles;
            if (mod >= nmods)
            {
                v0 = 0;
                return true;
            }

            try
            {
                uint entry = toc + TocFirst + mod * TocEntrySize;
                uint attr = bus.Read32(entry);
                uint ftLo = bus.Read32(entry + 4);
                uint ftHi = bus.Read32(entry + 8);
                uint size = bus.Read32(entry + 0xC);
                uint name = bus.Read32(entry + 0x10);
                uint outAttr = (attr & 0xFFFFEFFFu) | 0x2040u;
                bus.Write32(findData, outAttr);
                bus.Write32(findData + 4, ftLo);
                bus.Write32(findData + 8, ftHi);
                bus.Write32(findData + 0xC, ftLo);
                bus.Write32(findData + 0x10, ftHi);
                bus.Write32(findData + 0x14, ftLo);
                bus.Write32(findData + 0x18, ftHi);
                bus.Write32(findData + 0x1C, 0);
                bus.Write32(findData + 0x20, size);

                uint dst = findData + FindNameOff;
                for (int i = 0; i < 259; i++)
                {
                    uint src = name + (uint)i;
                    uint word = bus.Read32(src & ~3u);
                    uint ch = (word >> (8 * (int)(src & 3))) & 0xFF;
                    WriteU16(bus, dst, ch);
                    if (ch == 0)
                        break;
                    dst += 2;
                }

                v0 = 1;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void WriteU16(MipsBus bus, uint addr, uint ch)
        {
            uint aligned = addr & ~3u;
            uint word = bus.Read32(aligned);
            if ((addr & 2) == 0)
                word = (word & 0xFFFF0000u) | (ch & 0xFFFF);
            else
                word = (word & 0x0000FFFFu) | (ch << 16);
            bus.Write32(aligned, word);
        }
    }
}
