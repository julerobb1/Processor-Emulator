using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Runtime.Versioning;

namespace ProcessorEmulator
{
    internal enum AppTheme
    {
        Windows7Aero,
        CarlMode
    }

    [SupportedOSPlatform("windows")]
    internal static class AppThemeManager
    {
        private const string ThemeRegPath = "Software/ProcessorEmulator";
        private const string ThemeRegName = "AppTheme";

        private static ResourceDictionary currentCarlDict;

        public static AppTheme Current { get; private set; } = AppTheme.Windows7Aero;

        public static void LoadAndApplySaved(Application app)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(ThemeRegPath);
                var val = key?.GetValue(ThemeRegName) as string;
                if (Enum.TryParse<AppTheme>(val, out var theme))
                {
                    Apply(theme, app);
                }
                else
                {
                    // default to Win7 on first run
                    Apply(AppTheme.Windows7Aero, app);
                }
            }
            catch
            {
                Apply(AppTheme.Windows7Aero, app);
            }
        }

        public static void Apply(AppTheme theme, Application app)
        {
            if (app == null) return;

            // Remove Carl dictionary if present
            if (currentCarlDict != null && app.Resources.MergedDictionaries.Contains(currentCarlDict))
                app.Resources.MergedDictionaries.Remove(currentCarlDict);

            switch (theme)
            {
                case AppTheme.Windows7Aero:
                    Windows7ThemeManager.Apply(app);
                    break;
                case AppTheme.CarlMode:
                    Windows7ThemeManager.Restore(app); // ensure we start clean
                    currentCarlDict = new ResourceDictionary { Source = new Uri("/ProcessorEmulator;component/CarlMode.xaml", UriKind.Relative) };
                    app.Resources.MergedDictionaries.Add(currentCarlDict);
                    break;
            }

            Current = theme;

            // Persist
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(ThemeRegPath);
                key?.SetValue(ThemeRegName, theme.ToString());
            }
            catch { }
        }
    }
}
