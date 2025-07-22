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
            
            // Check if emulator has real framebuffer data
            if (emulator != null && emulator is HomebrewEmulator homebrewEmulator)
            {
                // Try to get actual video memory from the emulator
                var videoMemory = GetEmulatorVideoMemory(homebrewEmulator);
                if (videoMemory != null && videoMemory.Length > 0)
                {
                    // Convert emulator video memory to display format
                    return ConvertVideoMemoryToPixels(videoMemory);
                }
            }
            
            // If no video memory available, show firmware execution status
            return GenerateFirmwareExecutionDisplay();
        }
        
        private byte[] GetEmulatorVideoMemory(HomebrewEmulator emulator)
        {
            // TODO: Add method to HomebrewEmulator to expose video framebuffer
            // For now, return null to indicate no video memory available
            return null;
        }
        
        private byte[] ConvertVideoMemoryToPixels(byte[] videoMemory)
        {
            var pixels = new byte[640 * 480 * 4];
            
            // Convert various firmware video formats to BGRA
            // Common STB formats: RGB565, RGB888, YUV420, etc.
            
            // Simple RGB888 conversion (assuming videoMemory is RGB888)
            int pixelIndex = 0;
            for (int i = 0; i < Math.Min(videoMemory.Length / 3, pixels.Length / 4); i++)
            {
                if (i * 3 + 2 < videoMemory.Length)
                {
                    pixels[pixelIndex] = videoMemory[i * 3 + 2];     // Blue
                    pixels[pixelIndex + 1] = videoMemory[i * 3 + 1]; // Green
                    pixels[pixelIndex + 2] = videoMemory[i * 3];     // Red
                    pixels[pixelIndex + 3] = 255;                    // Alpha
                    pixelIndex += 4;
                }
            }
            
            return pixels;
        }
        
        private byte[] GenerateFirmwareExecutionDisplay()
        {
            var pixels = new byte[640 * 480 * 4];
            
            // Display actual firmware execution information
            if (emulator != null)
            {
                // Get real execution state from emulator
                var executionInfo = GetExecutionInfo();
                
                // Render execution state as text-like display
                RenderExecutionState(pixels, executionInfo);
            }
            
            return pixels;
        }
        
        private ExecutionInfo GetExecutionInfo()
        {
            if (emulator is HomebrewEmulator homebrewEmulator)
            {
                // TODO: Expose these properties from HomebrewEmulator
                return new ExecutionInfo
                {
                    ProgramCounter = GetProgramCounter(homebrewEmulator),
                    InstructionCount = GetInstructionCount(homebrewEmulator),
                    CurrentInstruction = GetCurrentInstruction(homebrewEmulator),
                    RegisterState = GetRegisterState(homebrewEmulator),
                    MemoryState = GetMemorySnapshot(homebrewEmulator)
                };
            }
            
            return new ExecutionInfo();
        }
        
        private void RenderExecutionState(byte[] pixels, ExecutionInfo info)
        {
            // Clear to dark background
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 16;     // Blue
                pixels[i + 1] = 16; // Green
                pixels[i + 2] = 16; // Red
                pixels[i + 3] = 255; // Alpha
            }
            
            // Render firmware execution state in console-like format
            int y = 20;
            
            // Title
            RenderText(pixels, "ARRIS XG1V4 - RDK-V Firmware Execution", 20, y, 255, 255, 255);
            y += 30;
            
            // Execution details
            RenderText(pixels, $"PC: 0x{info.ProgramCounter:X8}", 20, y, 0, 255, 0);
            y += 20;
            RenderText(pixels, $"Instructions: {info.InstructionCount}", 20, y, 0, 255, 0);
            y += 20;
            RenderText(pixels, $"Current: 0x{info.CurrentInstruction:X8}", 20, y, 0, 255, 0);
            y += 30;
            
            // Register state
            RenderText(pixels, "ARM Registers:", 20, y, 255, 255, 0);
            y += 20;
            for (int i = 0; i < Math.Min(8, info.RegisterState.Length); i++)
            {
                RenderText(pixels, $"R{i}: 0x{info.RegisterState[i]:X8}", 20 + (i % 4) * 150, y + (i / 4) * 20, 128, 255, 128);
            }
            y += 60;
            
            // Memory access log
            RenderText(pixels, "Recent Memory Access:", 20, y, 255, 255, 0);
            y += 20;
            if (info.MemoryState != null && info.MemoryState.Length > 0)
            {
                for (int i = 0; i < Math.Min(8, info.MemoryState.Length); i++)
                {
                    RenderText(pixels, $"0x{info.MemoryState[i]:X8}", 20, y, 192, 192, 255);
                    y += 15;
                }
            }
        }
        
        private void RenderText(byte[] pixels, string text, int x, int y, byte r, byte g, byte b)
        {
            // Simple bitmap font rendering for firmware execution display
            for (int i = 0; i < text.Length; i++)
            {
                RenderCharacter(pixels, text[i], x + i * 8, y, r, g, b);
            }
        }
        
        private void RenderCharacter(byte[] pixels, char c, int x, int y, byte r, byte g, byte b)
        {
            // Simple 8x8 character rendering
            var charPattern = GetCharacterPattern(c);
            
            for (int py = 0; py < 8; py++)
            {
                for (int px = 0; px < 8; px++)
                {
                    if (charPattern[py] & (1 << (7 - px)))
                    {
                        int pixelX = x + px;
                        int pixelY = y + py;
                        
                        if (pixelX >= 0 && pixelX < 640 && pixelY >= 0 && pixelY < 480)
                        {
                            int offset = (pixelY * 640 + pixelX) * 4;
                            pixels[offset] = b;     // Blue
                            pixels[offset + 1] = g; // Green
                            pixels[offset + 2] = r; // Red
                            pixels[offset + 3] = 255; // Alpha
                        }
                    }
                }
            }
        }
        
        private byte[] GetCharacterPattern(char c)
        {
            // Simple 8x8 bitmap font patterns (just a few characters for demo)
            switch (c)
            {
                case 'A': return new byte[] { 0x18, 0x3C, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x00 };
                case 'R': return new byte[] { 0x7C, 0x66, 0x66, 0x7C, 0x78, 0x6C, 0x66, 0x00 };
                case 'I': return new byte[] { 0x3C, 0x18, 0x18, 0x18, 0x18, 0x18, 0x3C, 0x00 };
                case 'S': return new byte[] { 0x3E, 0x60, 0x60, 0x3C, 0x06, 0x06, 0x7C, 0x00 };
                case 'X': return new byte[] { 0x66, 0x3C, 0x18, 0x18, 0x3C, 0x66, 0x66, 0x00 };
                case 'G': return new byte[] { 0x3C, 0x66, 0x60, 0x6E, 0x66, 0x66, 0x3C, 0x00 };
                case '1': return new byte[] { 0x18, 0x38, 0x18, 0x18, 0x18, 0x18, 0x7E, 0x00 };
                case 'V': return new byte[] { 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x00 };
                case '4': return new byte[] { 0x0C, 0x1C, 0x3C, 0x6C, 0x7E, 0x0C, 0x0C, 0x00 };
                case '-': return new byte[] { 0x00, 0x00, 0x00, 0x7E, 0x00, 0x00, 0x00, 0x00 };
                case ':': return new byte[] { 0x00, 0x18, 0x18, 0x00, 0x18, 0x18, 0x00, 0x00 };
                case '0': return new byte[] { 0x3C, 0x66, 0x6E, 0x76, 0x66, 0x66, 0x3C, 0x00 };
                case 'x': return new byte[] { 0x00, 0x00, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x00 };
                case 'P': return new byte[] { 0x7C, 0x66, 0x66, 0x7C, 0x60, 0x60, 0x60, 0x00 };
                case 'C': return new byte[] { 0x3C, 0x66, 0x60, 0x60, 0x60, 0x66, 0x3C, 0x00 };
                case ' ': return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                default:  return new byte[] { 0xFF, 0x81, 0x81, 0x81, 0x81, 0x81, 0xFF, 0x00 }; // Box for unknown chars
            }
        }
        
        // Helper methods to extract execution state from emulator
        private uint GetProgramCounter(HomebrewEmulator emulator)
        {
            return emulator.ProgramCounter;
        }
        
        private int GetInstructionCount(HomebrewEmulator emulator)
        {
            return emulator.InstructionCount;
        }
        
        private uint GetCurrentInstruction(HomebrewEmulator emulator)
        {
            return emulator.CurrentInstruction;
        }
        
        private uint[] GetRegisterState(HomebrewEmulator emulator)
        {
            return emulator.RegisterState ?? new uint[16];
        }
        
        private uint[] GetMemorySnapshot(HomebrewEmulator emulator)
        {
            var memory = emulator.MemoryState;
            if (memory == null || memory.Length < 32) return new uint[8];
            
            // Convert recent memory bytes to uint array for display
            var snapshot = new uint[8];
            for (int i = 0; i < 8 && i * 4 < memory.Length; i++)
            {
                snapshot[i] = BitConverter.ToUInt32(memory, i * 4);
            }
            return snapshot;
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
    
    public class ExecutionInfo
    {
        public uint ProgramCounter { get; set; }
        public int InstructionCount { get; set; }
        public uint CurrentInstruction { get; set; }
        public uint[] RegisterState { get; set; } = new uint[16];
        public uint[] MemoryState { get; set; } = new uint[8];
    }
}
