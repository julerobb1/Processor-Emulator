using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace ProcessorEmulator
{
    internal static class Win7Chrome
    {
        public static void ApplyChrome(Window window)
        {
            if (window == null) return;

            // Ensure window style is None so we can draw custom frame
            window.WindowStyle = WindowStyle.None;
            // Keep resize capability via WindowChrome
            var chrome = new WindowChrome
            {
                CaptionHeight = 28,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(window, chrome);

            window.Loaded += (_, __) => InstallTitleInteractions(window);
        }

        private static void InstallTitleInteractions(Window window)
        {
            if (window.Template == null) return; // We'll attach via visual tree search instead
            // Allow dragging from our custom title bar (named PART_CustomTitleBar if present)
            if (FindChild<Grid>(window, "PART_CustomTitleBar") is Grid title)
            {
                title.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        ToggleMaxRestore(window);
                    }
                    else
                    {
                        try { window.DragMove(); } catch { }
                    }
                };
            }
            if (FindChild<Button>(window, "PART_CloseButton") is Button closeBtn)
                closeBtn.Click += (_, __) => window.Close();
            if (FindChild<Button>(window, "PART_MinButton") is Button minBtn)
                minBtn.Click += (_, __) => window.WindowState = WindowState.Minimized;
            if (FindChild<Button>(window, "PART_MaxButton") is Button maxBtn)
                maxBtn.Click += (_, __) => ToggleMaxRestore(window);
        }

        private static void ToggleMaxRestore(Window w)
        {
            w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public static System.Windows.Media.Brush GenerateNoiseBrush(int size = 64, byte alpha = 18)
        {
            var wb = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            int stride = size * 4;
            byte[] pixels = new byte[size * stride];
            var rand = new Random(7);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = y * stride + x * 4;
                    byte v = (byte)rand.Next(180, 255); // light speckle
                    pixels[i + 0] = v; // B
                    pixels[i + 1] = v; // G
                    pixels[i + 2] = v; // R
                    pixels[i + 3] = alpha; // A
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
            var brush = new ImageBrush(wb)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 16.0 / size, 16.0 / size),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Stretch = Stretch.None,
                Opacity = 0.9
            };
            brush.Freeze();
            return brush;
        }

        private static T FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && (name == null || fe.Name == name)) return fe;
                var result = FindChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
