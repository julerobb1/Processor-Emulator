using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Core
{
    // 0x8001D3A0 CreateFile has no TOC fallback: INVALID_HANDLE returns 2.
    // LoadLibraryExW and CreateProcess already map TOC modules on that miss.
    // device.exe CreateFile of DEVMGR.dll does not. Helper 0 with type 2
    // left object+0 as INVALID_HANDLE, and 0x80016584/0x8003DFD8 then
    // failed that handle (ERROR_BAD_EXE_FORMAT). 0x80016AFC already
    // attaches a TOC module as object+0=TOCentry, object+4=7 so
    // 0x800196E4 uses e32 at TOC+0x14. DEVMGR and TOC[31]
    // iptvcryptohal.dll (packed at 0x8028E000, not a FILESentry)
    // get that attach. Image bytes stay in RAM; no stub HAL.
    public static class CeRomTocFiles
    {
        public const uint CreateFileFail = 0x8001D400;
        public const uint NameCopyContinue = 0x8001D464;
        public const uint BindImpMiss = 0x80018F9C;
        public const uint BindImpWalk = 0x80018F3C;
        public const uint CurProc = 0xFFFFDAC4;
        public const uint EcecTocPtr = 0x80010044;
        public const uint RomHdrNumMods = 0x10;
        public const uint TocFirst = 0x54;
        public const uint TocEntrySize = 32;
        public const byte TocAttachType = 7;
        public const uint O32RomSize = 0x18;
        public const uint O32LiteSize = 0x1C;
        // coredll 0x03F7A960 bne v0,0 / delay sw v0, (0x01FFFFA0).
        // HeapCreate(0,0,0) returned 0 in device.exe and the delay
        // slot wrote that 0 over the heap filesys already stored.
        // 0x01FFF000 is one physical page here, so that wipe makes
        // LocalAlloc call HeapAlloc(0) and RegOpen returns 14.
        public const uint HeapCreateStore = 0x03F7A964;
        public const uint ProcessHeapPtr = 0x01FFFFA0;

        public static bool TryContinueRomModule(MipsBus bus, uint path, out uint attr, out uint tocEntry)
        {
            attr = 0;
            tocEntry = 0;
            if (bus == null || path == 0)
                return false;

            string baseName = Basename(bus, path);
            if (string.IsNullOrEmpty(baseName))
                return false;
            // LoadLibraryExW and CreateProcess already map TOC modules when
            // this helper returns 2. DEVMGR CreateFile treats that miss as
            // fatal. sigcheckfilter's CreateFile of IPTVCryptoHAL.dll is
            // the same: TOC[31] is already in this image (not FILESentry).
            if (!NamesEqual(baseName, "devmgr.dll")
                && !NamesEqual(baseName, "iptvcryptohal.dll"))
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
                    tocEntry = entry;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // 0x80018F9C walks o32_lite at 180($fp). device.exe PROCESS stores
        // that pointer at e32_lite+0x54 (0x06012008) but the list stays
        // zero. TOC o32_rom already has FirstThunk 0x2000 in section 1.
        // Copy those bytes into the firmware dest; do not invent a slot.
        public static bool TryFillEmptyO32Lite(MipsBus bus, uint e32Lite, uint o32List, uint lookup)
        {
            if (bus == null || e32Lite == 0 || o32List == 0)
                return false;

            uint objcnt;
            uint tocEntry;
            try
            {
                objcnt = bus.Read32(e32Lite) & 0xFFFF;
                if (objcnt == 0 || objcnt > 16)
                    return false;
                if (bus.Read32(o32List) != 0 || bus.Read32(o32List + 4) != 0)
                    return false;
                uint proc = bus.Read32(CurProc);
                if (proc == 0)
                    return false;
                tocEntry = bus.Read32(proc + 0x50);
            }
            catch
            {
                return false;
            }

            if (!TryGetTocO32(bus, tocEntry, objcnt, out uint o32Rom))
                return false;

            try
            {
                bool hit = false;
                for (uint s = 0; s < objcnt; s++)
                {
                    uint src = o32Rom + s * O32RomSize;
                    uint dst = o32List + s * O32LiteSize;
                    uint vsize = bus.Read32(src);
                    uint rva = bus.Read32(src + 4);
                    uint psize = bus.Read32(src + 8);
                    uint dataptr = bus.Read32(src + 0xC);
                    uint real = bus.Read32(src + 0x10);
                    uint flags = bus.Read32(src + 0x14);
                    bus.Write32(dst, vsize);
                    bus.Write32(dst + 4, rva);
                    bus.Write32(dst + 8, real);
                    bus.Write32(dst + 0xC, 0);
                    bus.Write32(dst + 0x10, flags);
                    bus.Write32(dst + 0x14, psize);
                    bus.Write32(dst + 0x18, dataptr);
                    if (lookup >= rva && lookup < rva + vsize)
                        hit = true;
                }
                return hit;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetTocO32(MipsBus bus, uint tocEntry, uint objcnt, out uint o32Rom)
        {
            o32Rom = 0;
            if (tocEntry == 0)
                return false;
            try
            {
                uint toc = bus.Read32(EcecTocPtr);
                uint nmods = bus.Read32(toc + RomHdrNumMods);
                if (nmods == 0 || nmods > 64)
                    return false;
                for (uint i = 0; i < nmods; i++)
                {
                    uint entry = toc + TocFirst + i * TocEntrySize;
                    if (entry != tocEntry)
                        continue;
                    uint e32 = bus.Read32(entry + 0x14);
                    uint o32 = bus.Read32(entry + 0x18);
                    if (e32 == 0 || o32 == 0)
                        return false;
                    if ((bus.Read32(e32) & 0xFFFF) != objcnt)
                        return false;
                    o32Rom = o32;
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

        public static uint KeepProcessHeapIfCreateFailed(MipsBus bus, uint created, uint dest)
        {
            if (created != 0 || dest != ProcessHeapPtr || bus == null)
                return created;
            try
            {
                uint old = bus.Read32(ProcessHeapPtr);
                if (old != 0)
                    return old;
            }
            catch
            {
            }
            return created;
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
