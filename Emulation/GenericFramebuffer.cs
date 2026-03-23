using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProcessorEmulator.Emulation
{
    public enum PixelFormat
    {
        RGB565, // 16-bit
        ARGB8888 // 32-bit
    }

    /// <summary>
    /// A generic memory-mapped framebuffer device that simulates the VRAM of a MIPS-based SoC.
    /// It handles memory access from the emulated CPU and provides a renderable bitmap for the host UI.
    /// </summary>
    public class GenericFramebuffer : IBusDevice
    {
        public uint StartAddress { get; }
        public uint Size { get; }

        private readonly byte[] _vram;
        private readonly int _width;
        private readonly int _height;
        private readonly PixelFormat _pixelFormat;
        private readonly bool _isBigEndian;

        // WPF-specific bitmap for rendering on the host UI
        public WriteableBitmap BackBuffer { get; }

        public GenericFramebuffer(uint startAddress, int width, int height, PixelFormat format = PixelFormat.ARGB8888, bool isBigEndian = true)
        {
            _width = width;
            _height = height;
            _pixelFormat = format;
            _isBigEndian = isBigEndian;

            int bytesPerPixel = _pixelFormat == PixelFormat.RGB565 ? 2 : 4;
            Size = (uint)(_width * _height * bytesPerPixel);
            StartAddress = startAddress;

            _vram = new byte[Size];
            BackBuffer = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgra32, null);
        }

        public byte Read8(uint offset)
        {
            return _vram[offset];
        }

        public void Write8(uint offset, byte value)
        {
            _vram[offset] = value;
        }

        public uint Read32(uint offset)
        {
            if (_isBigEndian)
            {
                return (uint)(_vram[offset] << 24 | _vram[offset + 1] << 16 | _vram[offset + 2] << 8 | _vram[offset + 3]);
            }
            return BitConverter.ToUInt32(_vram, (int)offset);
        }

        public void Write32(uint offset, uint value)
        {
            if (_isBigEndian)
            {
                _vram[offset] = (byte)(value >> 24);
                _vram[offset + 1] = (byte)(value >> 16);
                _vram[offset + 2] = (byte)(value >> 8);
                _vram[offset + 3] = (byte)value;
            }
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Copy(bytes, 0, _vram, offset, 4);
            }
        }

        /// <summary>
        /// Renders the internal VRAM buffer to the public-facing WriteableBitmap.
        /// This method should be called periodically by the emulator's display loop (e.g., on V-Sync).
        /// </summary>
        public void RenderFrame()
        {
            try
            {
                BackBuffer.Lock();

                IntPtr pBackBuffer = BackBuffer.BackBuffer;
                int stride = BackBuffer.BackBufferStride;

                if (_pixelFormat == PixelFormat.ARGB8888 && !_isBigEndian)
                {
                    // If the format is already host-compatible (32-bit little-endian), we can copy directly.
                    System.Runtime.InteropServices.Marshal.Copy(_vram, 0, pBackBuffer, (int)Size);
                }
                else
                {
                    // Otherwise, we must convert pixel by pixel.
                    for (int y = 0; y < _height; y++)
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            IntPtr pPixel = pBackBuffer + y * stride + x * 4;
                            int vramIndex = (y * _width + x) * (_pixelFormat == PixelFormat.RGB565 ? 2 : 4);
                            uint color = ConvertPixelToBgra32(vramIndex);
                            System.Runtime.InteropServices.Marshal.WriteInt32(pPixel, (int)color);
                        }
                    }
                }
            }
            finally
            {
                BackBuffer.Unlock();
            }
        }

        private uint ConvertPixelToBgra32(int vramIndex)
        {
            if (_pixelFormat == PixelFormat.RGB565)
            {
                // Read 16-bit value, accounting for endianness
                ushort pixelData = _isBigEndian
                    ? (ushort)((_vram[vramIndex] << 8) | _vram[vramIndex + 1])
                    : BitConverter.ToUInt16(_vram, vramIndex);

                // Convert RGB565 to BGRA8888
                int r = (pixelData >> 11) & 0x1F;
                int g = (pixelData >> 5) & 0x3F;
                int b = pixelData & 0x1F;

                r = (r * 255) / 31;
                g = (g * 255) / 63;
                b = (b * 255) / 31;

                return (uint)((255 << 24) | (r << 16) | (g << 8) | b);
            }
            else // ARGB8888
            {
                // Read 32-bit value, accounting for endianness (ARGB -> BGRA)
                uint pixelData = Read32((uint)vramIndex);
                byte a = (byte)(pixelData >> 24);
                byte r = (byte)(pixelData >> 16);
                byte g = (byte)(pixelData >> 8);
                byte b = (byte)pixelData;
                return (uint)((a << 24) | (b << 16) | (g << 8) | r);
            }
        }
    }
}
