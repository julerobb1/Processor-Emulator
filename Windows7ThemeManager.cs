using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Versioning;

namespace ProcessorEmulator
{
    /// <summary>
    /// Forces a Windows 7 style theme (glass + colorization) on Windows 8+ hosts by overriding resource brushes.
    /// Non-destructive: keeps a snapshot of original brushes so they can be restored.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class Windows7ThemeManager
    {
        private static bool applied;
        private static ResourceDictionary backup;

        public static bool IsApplied => applied;

        public static void Apply(Application app)
        {
            if (app == null || applied) return;
            // Snapshot original relevant resources
            backup = new ResourceDictionary();
            string[] keys = new[]
            {
                "AeroHeaderBrush","AeroBorderBrush","WindowBackgroundBrush","AeroAccentBrush",
                "GlassMenuBackgroundBrush","GlassStatusBackgroundBrush","GlassPanelBrush"
            };
            foreach (var k in keys)
            {
                if (app.Resources.Contains(k)) backup[k] = app.Resources[k];
            }

            var color = GetColorizationColor() ?? Color.FromRgb(30, 90, 150); // Classic blue tint
            // Semi-transparent tints for glass areas
            var menuBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
            var statusBrush = new SolidColorBrush(Color.FromArgb(160, color.R, color.G, color.B));
            // Content background must be opaque (Win7 app surfaces are not see-through)
            var panelBrush = new SolidColorBrush(Color.FromRgb(241, 245, 250)); // light bluish-white
            menuBrush.Freeze(); statusBrush.Freeze(); panelBrush.Freeze();

            // Header (tabs/menu) gradient similar to Win7 (light at top)
            var headerGradient = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(235, (byte)Math.Min(color.R+40,255), (byte)Math.Min(color.G+40,255), (byte)Math.Min(color.B+40,255)),0),
                    new GradientStop(Color.FromArgb(200, color.R, color.G, color.B),1)
                }, new Point(0,0), new Point(0,1));
            headerGradient.Freeze();

            // Accent brush for titles
            var accent = new SolidColorBrush(Color.FromArgb(255, (byte)Math.Min(color.R+10,255), (byte)Math.Min(color.G+10,255), (byte)Math.Min(color.B+10,255)));
            accent.Freeze();

            app.Resources["AeroHeaderBrush"] = headerGradient;
            app.Resources["AeroAccentBrush"] = accent;
            app.Resources["AeroBorderBrush"] = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255));
            app.Resources["WindowBackgroundBrush"] = panelBrush; // opaque content surface
            app.Resources["GlassMenuBackgroundBrush"] = menuBrush;
            app.Resources["GlassStatusBackgroundBrush"] = statusBrush;
            app.Resources["GlassPanelBrush"] = panelBrush;

            applied = true;
        }

        public static void Restore(Application app)
        {
            if (!applied || app == null || backup == null) return;
            foreach (var key in backup.Keys)
            {
                app.Resources[key] = backup[key];
            }
            applied = false;
        }

        private static Color? GetColorizationColor()
        {
            try
            {
                // Windows 7 & legacy: HKCU DWM ColorizationColor
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\DWM", false);
                if (key != null)
                {
                    object val = key.GetValue("ColorizationColor");
                    if (val is int argb)
                    {
                        byte[] bytes = BitConverter.GetBytes(argb);
                        // Value stored as ABGR
                        byte b = bytes[0];
                        byte g = bytes[1];
                        byte r = bytes[2];
                        // byte a = bytes[3]; // often 0xFF
                        return Color.FromRgb(r, g, b);
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
