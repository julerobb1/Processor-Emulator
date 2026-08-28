using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace ProcessorEmulator
{
    // Force Win7-era visual styles on Win10/11. Does not rewrite the WPF host.
    // Common Controls 6 + EnableVisualStyles; square corners; no immersive dark.
    internal static class Win7VisualStyle
    {
        private const int DwmwaUseImmersiveDarkModeOld = 19;
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaWindowCornerPreference = 33;
        private const int DwmwcpDoNotRound = 1;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        public static void EnableHost()
        {
            try { WinForms.Application.EnableVisualStyles(); }
            catch { }
            try { WinForms.Application.SetCompatibleTextRenderingDefault(false); }
            catch { }
            try
            {
                WinForms.Application.VisualStyleState =
                    WinForms.VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled;
            }
            catch { }
        }

        public static void ApplyToHwnd(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;
            try
            {
                int off = 0;
                int square = DwmwcpDoNotRound;
                DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref off, sizeof(int));
                DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeOld, ref off, sizeof(int));
                DwmSetWindowAttribute(hwnd, DwmwaWindowCornerPreference, ref square, sizeof(int));
            }
            catch
            {
            }
        }

        public static void ApplyToWindow(Window window)
        {
            if (window == null)
                return;
            try
            {
                ApplyToHwnd(new WindowInteropHelper(window).Handle);
            }
            catch
            {
            }
        }
    }
}
