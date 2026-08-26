using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // 0x8001D3A0 CreateFile has no TOC fallback: INVALID_HANDLE returns 2.
    // LoadLibraryExW already resolves TOC basenames. When CreateFile misses
    // and the path basename is a TOC module, continue at the firmware
    // ROM-module success site (0x8001D44C) with FILE_ATTRIBUTE_ROMMODULE.
    // The image is already in RAM; no handle and no extra bytes are created.
    public static class CeRomTocFiles
    {
        public const uint CreateFileFail = 0x8001D400;
        public const uint RomModuleContinue = 0x8001D44C;
        public const uint EcecTocPtr = 0x80010044;
        public const uint RomHdrNumMods = 0x10;
        public const uint TocFirst = 0x54;
        public const uint TocEntrySize = 32;

        public static bool TryContinueRomModule(MipsBus bus, uint path, out uint attr)
        {
            attr = 0;
            if (bus == null || path == 0)
                return false;

            string baseName = Basename(bus, path);
            if (string.IsNullOrEmpty(baseName))
                return false;
            // LoadLibraryExW and CreateProcess already map TOC modules when
            // this helper returns 2. Only the DEVMGR CreateFile caller treats
            // that miss as fatal, so only that basename continues at 0x8001D44C.
            if (!NamesEqual(baseName, "devmgr.dll"))
                return false;

            uint toc;
            uint nmods;
            try
            {
                toc = bus.Read32(EcecTocPtr);
                if (toc == 0)
                    return false;
                nmods = bus.Read32(toc + RomHdrNumMods);
            }
            catch
            {
                return false;
            }

            if (nmods == 0 || nmods > 64)
                return false;

            try
            {
                for (uint i = 0; i < nmods; i++)
                {
                    uint entry = toc + TocFirst + i * TocEntrySize;
                    uint name = bus.Read32(entry + 0x10);
                    if (!NamesEqual(baseName, ReadAscii(bus, name)))
                        continue;
                    uint tocAttr = bus.Read32(entry);
                    attr = (tocAttr & 0xFFFFEFFFu) | 0x2040u;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static string Basename(MipsBus bus, uint path)
        {
            var sb = new System.Text.StringBuilder();
            int start = 0;
            for (int i = 0; i < 260; i++)
            {
                uint p = path + (uint)(i * 2);
                uint word = bus.Read32(p & ~3u);
                uint ch = ((p & 2) == 0) ? (word & 0xFFFF) : (word >> 16);
                if (ch == 0)
                    break;
                if (ch == '\\' || ch == '/')
                {
                    sb.Length = 0;
                    start = i + 1;
                    continue;
                }
                if (ch < 0x20 || ch > 0x7E)
                    return "";
                sb.Append((char)ch);
            }
            return start >= 0 ? sb.ToString() : "";
        }

        private static string ReadAscii(MipsBus bus, uint addr)
        {
            if (addr == 0)
                return "";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 80; i++)
            {
                uint src = addr + (uint)i;
                uint word = bus.Read32(src & ~3u);
                uint ch = (word >> (8 * (int)(src & 3))) & 0xFF;
                if (ch == 0)
                    break;
                if (ch < 0x20 || ch > 0x7E)
                    return "";
                sb.Append((char)ch);
            }
            return sb.ToString();
        }

        private static bool NamesEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                char ca = a[i];
                char cb = b[i];
                if (ca >= 'A' && ca <= 'Z') ca = (char)(ca + 32);
                if (cb >= 'A' && cb <= 'Z') cb = (char)(cb + 32);
                if (ca != cb)
                    return false;
            }
            return true;
        }
    }
}
