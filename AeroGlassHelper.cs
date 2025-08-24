using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ProcessorEmulator
{
    internal static class AeroGlassHelper
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

    // Win10+ undocumented accent blur (for when classic Aero isn't available)
    [DllImport("user32.dll", EntryPoint = "SetWindowCompositionAttribute")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fEnable;
            public IntPtr hRgnBlur;
            [MarshalAs(UnmanagedType.Bool)] public bool fTransitionOnMaximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ACCENT_POLICY
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWCOMPOSITIONATTRIBDATA
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private const int WCA_ACCENT_POLICY = 19; // value used by modern Windows
        private const int ACCENT_ENABLE_BLURBEHIND = 3;
        private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4; // fallback if desired
        private const uint DWM_BB_ENABLE = 0x00000001;

        public static bool TryApplyGlass(Window window, int topHeight = 40, int bottomHeight = 22)
        {
            try
            {
                if (window == null) return false;
                if (!IsDwmEnabled()) return false;

                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return false;

                // Extend a band at top (menu area) & bottom (status bar) into glass.
                var margins = new MARGINS
                {
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyTopHeight = topHeight,
                    cyBottomHeight = bottomHeight
                };

                int hr = DwmExtendFrameIntoClientArea(hwnd, ref margins);
                return hr >= 0; // S_OK or success code
            }
            catch
            {
                return false;
            }
        }

    public static bool IsDwmEnabled()
        {
            try
            {
                if (Environment.OSVersion.Version.Major < 6) return false; // Pre-Vista
                bool enabled; DwmIsCompositionEnabled(out enabled);
                return enabled;
            }
            catch { return false; }
        }

        /// <summary>
        /// Attempts to apply true Windows 7 style blur glass to the entire window client area.
        /// On Win7: uses DwmEnableBlurBehindWindow + full negative margins.
        /// On Win10/11: attempts AccentPolicy blur behind (closest approximation).
        /// </summary>
        public static bool TryApplyTrueGlass(Window window)
        {
            try
            {
                if (window == null) return false;
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return false;

                bool didSomething = false;

                if (IsDwmEnabled())
                {
                    // Extend frame fully (Win7 authentic blur) – on Win10+ this alone may not show
                    var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
                    int hr1 = DwmExtendFrameIntoClientArea(hwnd, ref margins);

                    // Enable blur-behind region
                    var bb = new DWM_BLURBEHIND
                    {
                        dwFlags = DWM_BB_ENABLE,
                        fEnable = true,
                        hRgnBlur = IntPtr.Zero,
                        fTransitionOnMaximized = false
                    };
                    int hr2 = DwmEnableBlurBehindWindow(hwnd, ref bb);
                    didSomething |= (hr1 >= 0) || (hr2 >= 0);
                }

                // On Windows 10/11, also enable AccentPolicy blur for visible transparency
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    var accent = new ACCENT_POLICY
                    {
                        AccentState = ACCENT_ENABLE_BLURBEHIND,
                        AccentFlags = 0,
                        // ARGB color; low alpha lets content be seen while keeping Chrome-like tint
                        GradientColor = unchecked((int)0x66FFFFFF)
                    };
                    var size = Marshal.SizeOf(accent);
                    IntPtr pAccent = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(accent, pAccent, false);
                    var data = new WINDOWCOMPOSITIONATTRIBDATA
                    {
                        Attribute = WCA_ACCENT_POLICY,
                        Data = pAccent,
                        SizeOfData = size
                    };
                    int hr = SetWindowCompositionAttribute(hwnd, ref data);
                    Marshal.FreeHGlobal(pAccent);
                    didSomething |= (hr >= 0);
                }

                if (didSomething) return true;
            }
            catch { }
            return false;
        }
    }
}
