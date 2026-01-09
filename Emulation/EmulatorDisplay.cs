using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProcessorEmulator.Emulation
{
    /// <summary>
    /// A placeholder class representing the MIPS VRAM. In a real implementation,
    /// this would be part of the MipsBus or a dedicated Video Controller device.
    /// </summary>
    public class GenericFramebuffer
    {
        // 640x480 resolution, 32 bits per pixel (4 bytes)
        public readonly byte[] FrameData = new byte[640 * 480 * 4];
    }

    /// <summary>
    /// A WinForms-based display that renders a framebuffer using high-performance GDI+.
    /// Styled to look like a classic Windows XP/7 professional diagnostic tool.
    /// </summary>
    public class EmulatorDisplay : Form
    {
        private PictureBox _screen;
        private Bitmap _backBuffer;
        private GenericFramebuffer _vram;

        public EmulatorDisplay(GenericFramebuffer vram)
        {
            _vram = vram;

            // XP/7 Professional Look
            this.Text = "Hardware Video Buffer - Broadcom BCM7405 Target";
            this.BackColor = SystemColors.Control;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Prevents modern resizing
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application; // Classic generic app icon
            this.ShowInTaskbar = true;

            // Main Video Container
            _screen = new PictureBox
            {
                Size = new Size(640, 480),
                Location = new Point(10, 10), // Margin like classic Win32 apps
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D, // Gives it that "sunken" CRT look
                SizeMode = PictureBoxSizeMode.Normal
            };

            // Status Bar (Very XP/7 style)
            StatusStrip statusBar = new StatusStrip();
            statusBar.Items.Add(new ToolStripStatusLabel("Emulation: Running"));
            statusBar.Items.Add(new ToolStripStatusLabel(" | VRAM: 0x10000000")); // Example VRAM address

            this.Controls.Add(_screen);
            this.Controls.Add(statusBar);
            this.ClientSize = new Size(_screen.Width + 20, _screen.Height + statusBar.Height + 20);

            _backBuffer = new Bitmap(640, 480, PixelFormat.Format32bppRgb);
            _screen.Image = _backBuffer;

            InitializeHardwareMonitor();
        }

        private void InitializeHardwareMonitor()
        {
            // Creating a Windows 7 style 'Property Grid' or 'ListView'
            ListView hardwareList = new ListView
            {
                //View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(660, 12),
                Size = new Size(250, 482),
                //BackColor = SystemColors.Window,
                //Font = new Font("Segoe UI", 9) // The Windows 7 standard font
            };

            hardwareList.Columns.Add("Component", 100);
            hardwareList.Columns.Add("Address", 120);

            hardwareList.Items.Add(new ListViewItem(new[] { "CPU Core", "MIPS32 R2" }));
            hardwareList.Items.Add(new ListViewItem(new[] { "UART (Serial)", "0x10400000" }));
            hardwareList.Items.Add(new ListViewItem(new[] { "Video RAM", "0x10000000" }));
    
            this.Controls.Add(hardwareList);
            // Expand the window to fit the monitor
            this.Width += 270; 
        }

        public void RenderFrame()
        {
            // 1. Lock the Windows Bitmap memory
            BitmapData data = _backBuffer.LockBits(
                new Rectangle(0, 0, 640, 480),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppRgb);

            try
            {
                // 2. Perform a direct memory copy from MIPS VRAM to Win32 Bitmap
                // This is extremely fast as it bypasses the GDI abstraction layer
                System.Runtime.InteropServices.Marshal.Copy(_vram.FrameData, 0, data.Scan0, _vram.FrameData.Length);
            }
            finally
            {
                // 3. Unlock and signal the PictureBox to repaint
                _backBuffer.UnlockBits(data);
                if (!_screen.IsDisposed && _screen.IsHandleCreated)
                {
                    _screen.Invalidate();
                }
            }
        }
    }
}
