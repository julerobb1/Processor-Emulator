using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ProcessorEmulator.Core;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator.Emulation
{
    // ExtraROM Phase B: dump-grounded BTV1: IOCTLs + 97405 IR MMIO.
    // Off unless EXTRAROM_BTV=1. No invented pixels or keycodes.
    //
    // Display: format 0x13 is NEXUS/BPXL_eA8_R8_G8_B8, 32 bpp, from
    // iptvdriver table va 0x27BB080 (12-byte records, word1 descriptor
    // 0x07E48888 matches BPXL name table {desc,name} at 0x28C5EF8).
    // GetDesktopSurface default is 856x480, settings+0 = 0x13
    // (iptvdriver 0x2AE1EC4). IOCTL codes from halfword switch at
    // 0x2ACC434 (dwCode = 0x80032000 + index).
    //
    // Input: iptvhal_97405 bit table at 0x1E9C2C8 (recovered after
    // LZX decompress of data psize 0x206 -> 0x739). 0-bit at
    // 0xB0400140/0144 packed halfword (reg140<<8)|reg144 means pressed.
    public sealed class ExtraRomBtv
    {
        public const string EnvName = "EXTRAROM_BTV";
        public const uint DeviceHandle = 0xB7100001;
        public const uint SurfaceVa = 0x8F000000;
        public const uint WrapperVa = 0x8EFFFF80;
        public const int WrapperSize = 0x7C;
        public const int DefaultWidth = 856;
        public const int DefaultHeight = 480;
        public const int BytesPerPixel = 4;
        public const uint PixelFormatId = 0x13; // BPXL_eA8_R8_G8_B8

        public const uint IoctlLockBitmapBuffer = 0x8003201C;
        public const uint IoctlUnlockBitmapBuffer = 0x80032020;
        public const uint IoctlGetFrameBufferBackbuffer = 0x80032024;
        public const uint IoctlUpdateFrameBuffer = 0x80032028;
        public const uint IoctlGetVidMem = 0x80032050;
        public const uint IoctlGetDesktopSurface = 0x80032064;
        public const uint IoctlDisplayInit2 = 0x80032068;
        public const uint IoctlDisplaySetMode2 = 0x8003206C;
        public const uint IoctlDisplayGetMode2 = 0x80032070;
        public const uint IoctlGfxInit = 0x80032084;

        public const uint CoredllDeviceIoControl = 0x3F780E4;
        // Guest programs kseg1 0xB0400140/0144; bus Translate maps that to
        // phys 0x10400140/0144 (same 64KB page as BcmSysControlRegs).
        public const uint IrMmioBase = 0x10400140;
        public const uint IrMmioSize = 8; // 0x10400140 and 0x10400144 only
        public const uint IrReg140 = 0x0;
        public const uint IrReg144 = 0x4;

        // Dump table at 0x1E9C2C8, 16 bytes. 0 = unused bit.
        public static readonly byte[] BitKeycodes =
        {
            0xE9, 0x28, 0x9E, 0x00,
            0x25, 0x27, 0x9D, 0x00,
            0x26, 0xAF, 0x99, 0x00,
            0x2B, 0x12, 0xAE, 0x00
        };

        private static readonly object Gate = new object();
        private static ExtraRomBtv _live;

        public static bool IsEnabled
        {
            get
            {
                string v = Environment.GetEnvironmentVariable(EnvName);
                return v == "1" || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static ExtraRomBtv Live
        {
            get
            {
                lock (Gate)
                    return _live;
            }
        }

        public static ExtraRomBtv Ensure()
        {
            lock (Gate)
            {
                if (_live == null)
                    _live = new ExtraRomBtv();
                return _live;
            }
        }

        public static void Reset()
        {
            lock (Gate)
                _live = null;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Stride { get; private set; }
        public uint ModeId { get; private set; }
        public bool GuestVideoWrote { get; private set; }
        public bool SurfaceReady { get; private set; }

        // packed 16-bit state, 1 = released (idle). 0-bit = pressed.
        private int _irPacked = 0xFFFF;
        private int _createFileHits;
        private int _ioctlTotal;
        private int _updateCount;
        private int _ir140Reads;
        private int _ir144Reads;
        private string _framePath = "";
        private readonly Dictionary<uint, int> _ioctlCounts = new Dictionary<uint, int>();
        private readonly Dictionary<uint, string> _ioctlLast = new Dictionary<uint, string>();

        public event Action<byte[], int, int, int> FrameReady;

        private ExtraRomBtv()
        {
            Width = DefaultWidth;
            Height = DefaultHeight;
            Stride = DefaultWidth * BytesPerPixel;
            ModeId = 0;
        }

        public static bool TryOpenCreateFile(uint[] regs, ref uint programCounter)
        {
            if (!IsEnabled || regs == null || regs.Length <= 31)
                return false;
            ExtraRomBtv btv = Ensure();
            btv._createFileHits++;
            BootLog.Write("[BTV] CreateFile(BTV1:) handle 0x" + DeviceHandle.ToString("X8") + " count=" + btv._createFileHits);
            regs[2] = DeviceHandle;
            programCounter = regs[31];
            return true;
        }

        public static bool TryDeviceIoControl(MipsBus bus, uint[] regs, ref uint programCounter)
        {
            if (!IsEnabled || bus == null || regs == null || regs.Length <= 31)
                return false;
            if (regs[4] != DeviceHandle)
                return false;
            ExtraRomBtv btv = Ensure();
            uint code = regs[5];
            uint inPtr = regs[6];
            uint inLen = regs[7];
            uint sp = regs[29];
            uint outPtr = 0, outLen = 0, bytesRetPtr = 0;
            try
            {
                outPtr = bus.Read32(sp + 0x10);
                outLen = bus.Read32(sp + 0x14);
                bytesRetPtr = bus.Read32(sp + 0x18);
            }
            catch
            {
            }

            uint written = 0;
            bool ok = btv.HandleIoctl(bus, code, inPtr, inLen, outPtr, outLen, out written);
            try
            {
                if (bytesRetPtr != 0)
                    bus.Write32(bytesRetPtr, written);
            }
            catch
            {
            }
            regs[2] = ok ? 1u : 0u;
            programCounter = regs[31];
            return true;
        }

        private bool HandleIoctl(MipsBus bus, uint code, uint inPtr, uint inLen, uint outPtr, uint outLen, out uint written)
        {
            written = 0;
            _ioctlTotal++;
            int ncode;
            if (!_ioctlCounts.TryGetValue(code, out ncode))
                ncode = 0;
            ncode++;
            _ioctlCounts[code] = ncode;
            string last = "in=0x" + inLen.ToString("X") + " out=0x" + outLen.ToString("X")
                + " inPtr=0x" + inPtr.ToString("X8") + " outPtr=0x" + outPtr.ToString("X8");
            _ioctlLast[code] = last;
            if (ncode <= 8 || (ncode % 50) == 0)
                BootLog.Write("[BTV] IOCTL 0x" + code.ToString("X8") + " #" + ncode + " " + last);
            try
            {
                switch (code)
                {
                    case IoctlGfxInit:
                    case IoctlUnlockBitmapBuffer:
                        return true;

                    case IoctlDisplayInit2:
                        ApplyModeStruct(bus, inPtr, inLen);
                        EnsureSurface(bus);
                        return true;

                    case IoctlDisplaySetMode2:
                        ApplyModeStruct(bus, inPtr, inLen);
                        EnsureSurface(bus);
                        return true;

                    case IoctlDisplayGetMode2:
                        if (outPtr == 0 || outLen < 0x18)
                            return false;
                        // Out layout proven only for +0 mode DWORD and
                        // +4/+6 uint16 width/height (same 0x18 in-struct).
                        bus.Write32(outPtr, ModeId);
                        bus.Write32(outPtr + 4, (uint)(Width & 0xFFFF) | ((uint)(Height & 0xFFFF) << 16));
                        bus.Write32(outPtr + 8, 0);
                        bus.Write32(outPtr + 0xC, 0);
                        bus.Write32(outPtr + 0x10, 0);
                        bus.Write32(outPtr + 0x14, 0);
                        written = 8;
                        return true;

                    case IoctlGetDesktopSurface:
                    case IoctlGetFrameBufferBackbuffer:
                        EnsureSurface(bus);
                        if (outPtr == 0 || outLen < 4)
                            return false;
                        bus.Write32(outPtr, WrapperVa);
                        written = 4;
                        return true;

                    case IoctlLockBitmapBuffer:
                        return LockBuffer(bus, inPtr, inLen);

                    case IoctlUpdateFrameBuffer:
                        return UpdateFrame(bus);

                    case IoctlGetVidMem:
                        // Driver returns a size (end-start), not a pointer.
                        // Surface byte count is the only size this host
                        // allocation can honestly report.
                        if (outPtr != 0 && outLen >= 4)
                        {
                            bus.Write32(outPtr, (uint)(Stride * Height));
                            written = 4;
                        }
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ApplyModeStruct(MipsBus bus, uint inPtr, uint inLen)
        {
            if (inPtr == 0 || inLen < 8)
                return;
            ModeId = bus.Read32(inPtr);
            uint packed = bus.Read32(inPtr + 4);
            int w = (int)(packed & 0xFFFF);
            int h = (int)((packed >> 16) & 0xFFFF);
            if (w > 0 && w <= 1920 && h > 0 && h <= 1080)
            {
                Width = w;
                Height = h;
                Stride = w * BytesPerPixel;
            }
        }

        private void EnsureSurface(MipsBus bus)
        {
            if (bus == null)
                return;
            // wrapper: word0 lock flag, +4 "Nexus handle" (we store SurfaceVa)
            bus.Write32(WrapperVa, 0);
            bus.Write32(WrapperVa + 4, SurfaceVa);
            SurfaceReady = true;
        }

        private bool LockBuffer(MipsBus bus, uint inPtr, uint inLen)
        {
            EnsureSurface(bus);
            if (inPtr == 0 || inLen < 0xC)
                return false;
            // in 0xC: wrapper, void**, uint16*
            uint pixSlot = bus.Read32(inPtr + 4);
            uint strideSlot = bus.Read32(inPtr + 8);
            if (pixSlot == 0 || strideSlot == 0)
                return false;
            bus.Write32(pixSlot, SurfaceVa);
            // uint16 stride
            uint existing = bus.Read32(strideSlot);
            bus.Write32(strideSlot, (existing & 0xFFFF0000u) | ((uint)Stride & 0xFFFF));
            bus.Write32(WrapperVa, 1);
            return true;
        }

        private bool UpdateFrame(MipsBus bus)
        {
            if (!SurfaceReady || Width <= 0 || Height <= 0 || Stride <= 0)
                return false;
            int size = Stride * Height;
            if (size <= 0 || size > 1920 * 1080 * 4)
                return false;
            byte[] pixels = new byte[size];
            for (int off = 0; off < size; off += 4)
            {
                uint w = bus.Read32((uint)(SurfaceVa + off));
                pixels[off] = (byte)(w);
                pixels[off + 1] = (byte)(w >> 8);
                pixels[off + 2] = (byte)(w >> 16);
                pixels[off + 3] = (byte)(w >> 24);
            }
            _updateCount++;
            GuestVideoWrote = true;
            TrySaveFrame(pixels, Width, Height, Stride);
            var ev = FrameReady;
            ev?.Invoke(pixels, Width, Height, Stride);
            BootLog.Write("[BTV] UpdateFrameBuffer #" + _updateCount + " " + Width + "x" + Height + " stride=" + Stride + " path=" + _framePath);
            return true;
        }

        public void SetKey(int vk, bool pressed)
        {
            if (vk <= 0 || vk > 0xFF)
                return;
            byte code = (byte)vk;
            int packed = _irPacked;
            for (int bit = 0; bit < 16; bit++)
            {
                if (BitKeycodes[bit] != code)
                    continue;
                if (pressed)
                    packed &= ~(1 << bit);
                else
                    packed |= (1 << bit);
            }
            _irPacked = packed;
        }

        public uint ReadIr32(uint offset)
        {
            if (offset == IrReg140)
            {
                _ir140Reads++;
                if (_ir140Reads <= 4 || (_ir140Reads % 1000) == 0)
                    BootLog.Write("[BTV] IR MMIO read phys 0x10400140 (kseg1 0xB0400140) #" + _ir140Reads + " packed=0x" + _irPacked.ToString("X4"));
                return (uint)((_irPacked >> 8) & 0xFF);
            }
            if (offset == IrReg144)
            {
                _ir144Reads++;
                if (_ir144Reads <= 4 || (_ir144Reads % 1000) == 0)
                    BootLog.Write("[BTV] IR MMIO read phys 0x10400144 (kseg1 0xB0400144) #" + _ir144Reads + " packed=0x" + _irPacked.ToString("X4"));
                return (uint)(_irPacked & 0xFF);
            }
            return 0;
        }

        public void WriteIr32(uint offset, uint value)
        {
            // 0x140/0x144 are read-side packed halfwords; do not let guest writes
            // clobber the 0-bit=pressed idle state. Other offsets are not ours.
            _ = value;
        }

        public static void WriteCaptureSummary()
        {
            ExtraRomBtv btv = Live;
            if (btv == null)
            {
                BootLog.Write("[BTV] capture: no ExtraRomBtv instance (flag off or never opened)");
                return;
            }
            BootLog.Write("[BTV] capture CreateFileHits=" + btv._createFileHits
                + " ioctlTotal=" + btv._ioctlTotal
                + " UpdateFrameBuffer=" + btv._updateCount
                + " GuestVideoWrote=" + btv.GuestVideoWrote
                + " surface=" + btv.SurfaceReady
                + " " + btv.Width + "x" + btv.Height
                + " ir140=" + btv._ir140Reads
                + " ir144=" + btv._ir144Reads
                + " frame=" + (string.IsNullOrEmpty(btv._framePath) ? "(none)" : btv._framePath));
            foreach (KeyValuePair<uint, int> kv in btv._ioctlCounts)
            {
                string last;
                if (!btv._ioctlLast.TryGetValue(kv.Key, out last))
                    last = "";
                BootLog.Write("[BTV] capture IOCTL 0x" + kv.Key.ToString("X8") + " count=" + kv.Value + " last " + last);
            }
        }

        private void TrySaveFrame(byte[] pixels, int width, int height, int stride)
        {
            if (pixels == null || width <= 0 || height <= 0 || stride <= 0)
                return;
            try
            {
                string dest = Environment.GetEnvironmentVariable("EXTRAROM_BTV_FRAME");
                if (string.IsNullOrWhiteSpace(dest))
                {
                    string dir = "";
                    try { dir = Path.GetDirectoryName(BootLog.FilePath); } catch { }
                    if (string.IsNullOrEmpty(dir))
                        dir = Path.GetTempPath();
                    dest = Path.Combine(dir, "extrarom-btv-frame.png");
                }
                using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        if (data.Stride == stride)
                            Marshal.Copy(pixels, 0, data.Scan0, stride * height);
                        else
                        {
                            int row = Math.Min(stride, Math.Abs(data.Stride));
                            for (int y = 0; y < height; y++)
                            {
                                IntPtr destRow = data.Scan0 + y * data.Stride;
                                Marshal.Copy(pixels, y * stride, destRow, row);
                            }
                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(data);
                    }
                    string tmp = dest + ".tmp";
                    bmp.Save(tmp, ImageFormat.Png);
                    if (File.Exists(dest))
                        File.Delete(dest);
                    File.Move(tmp, dest);
                }
                _framePath = dest;
            }
            catch (Exception ex)
            {
                BootLog.Write("[BTV] frame save failed: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

    }

    public sealed class ExtraRomIrMmio : IBusDevice
    {
        public uint StartAddress => ExtraRomBtv.IrMmioBase;
        public uint Size => ExtraRomBtv.IrMmioSize;

        public uint Read32(uint offset)
        {
            ExtraRomBtv btv = ExtraRomBtv.Live ?? ExtraRomBtv.Ensure();
            return btv.ReadIr32(offset);
        }

        public void Write32(uint offset, uint value)
        {
            ExtraRomBtv btv = ExtraRomBtv.Live ?? ExtraRomBtv.Ensure();
            btv.WriteIr32(offset, value);
        }

        public byte Read8(uint offset)
        {
            uint word = Read32(offset & ~3u);
            return (byte)(word >> (int)((offset & 3) * 8));
        }

        public void Write8(uint offset, byte value)
        {
            uint aligned = offset & ~3u;
            uint shift = (offset & 3) * 8;
            ExtraRomBtv btv = ExtraRomBtv.Live ?? ExtraRomBtv.Ensure();
            uint word = btv.ReadIr32(aligned);
            word = (word & ~(0xFFu << (int)shift)) | ((uint)value << (int)shift);
            btv.WriteIr32(aligned, word);
        }
    }
}
