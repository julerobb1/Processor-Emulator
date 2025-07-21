using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace ProcessorEmulator.Emulation
{
    public partial class EmulatorWindow : Window
    {
        private WriteableBitmap framebuffer;
        private Timer displayTimer;
        private IEmulator emulator;
        private volatile bool isRunning = false;
        
        public EmulatorWindow(IEmulator emulator)
        {
            InitializeComponent();
            this.emulator = emulator;
            InitializeFramebuffer();
            SetupDisplay();
        }
        
        private void InitializeFramebuffer()
        {
            // Create a 640x480 RGB framebuffer (common STB resolution)
            framebuffer = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
            EmulatorImage.Source = framebuffer;
            
            // Clear to black initially
            var rect = new Int32Rect(0, 0, 640, 480);
            var pixels = new byte[640 * 480 * 4]; // BGRA
            framebuffer.WritePixels(rect, pixels, 640 * 4, 0);
        }
        
        private void SetupDisplay()
        {
            // Update display at 30 FPS
            displayTimer = new Timer(UpdateDisplay, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(33));
        }
        
        private void UpdateDisplay(object state)
        {
            if (!isRunning) return;
            
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Generate some test pattern for RDK-V boot sequence
                    var rect = new Int32Rect(0, 0, 640, 480);
                    var pixels = GenerateBootPattern();
                    framebuffer.WritePixels(rect, pixels, 640 * 4, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Display update error: {ex.Message}");
                }
            });
        }
        
        private byte[] GenerateBootPattern()
        {
            var pixels = new byte[640 * 480 * 4];
            var time = DateTime.Now.Millisecond;
            
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    int offset = (y * 640 + x) * 4;
                    
                    // Create a simple boot animation pattern
                    byte intensity = (byte)((Math.Sin(x * 0.01 + time * 0.01) + 1) * 127);
                    
                    pixels[offset] = intensity;     // Blue
                    pixels[offset + 1] = 0;         // Green  
                    pixels[offset + 2] = 0;         // Red
                    pixels[offset + 3] = 255;       // Alpha
                }
            }
            
            return pixels;
        }
        
        public void StartEmulation()
        {
            isRunning = true;
            Title = "RDK-V Emulator - Running";
            
            // Start emulation in background thread
            Task.Run(() =>
            {
                try
                {
                    while (isRunning)
                    {
                        emulator.Step();
                        Thread.Sleep(1); // Slow down for debugging
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Emulation error: {ex.Message}");
                    Dispatcher.BeginInvoke(() =>
                    {
                        Title = "RDK-V Emulator - Error";
                        MessageBox.Show($"Emulation stopped: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }
        
        public void StopEmulation()
        {
            isRunning = false;
            Title = "RDK-V Emulator - Stopped";
        }
        
        protected override void OnClosed(EventArgs e)
        {
            StopEmulation();
            displayTimer?.Dispose();
            base.OnClosed(e);
        }
    }
}
