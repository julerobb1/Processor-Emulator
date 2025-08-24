using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ProcessorEmulator
{
    public partial class App : Application
    {
        private static string StartupLogPath => Path.Combine(Path.GetTempPath(), "ProcessorEmulator_startup.log");
        private static void Log(string line)
        {
            try { File.AppendAllText(StartupLogPath, DateTime.Now.ToString("o") + " " + line + Environment.NewLine); } catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log("OnStartup begin; args=[" + string.Join(" ", e.Args ?? Array.Empty<string>()) + "]");
            // If CLI args provided, run extract/analyze logic instead of WPF UI
            if (e.Args.Length > 0)
            {
                int exitCode = 0;
                try
                {
                    var args = e.Args;
                    var cmd = args[0].ToLowerInvariant();
                    switch (cmd)
                    {
                        case "analyze":
                            if (args.Length != 2)
                                throw new ArgumentException("Usage: analyze <inputFile>");
                            ArchiveExtractor.AnalyzeArchive(args[1]);
                            break;
                        case "extract":
                            if (args.Length != 3)
                                throw new ArgumentException("Usage: extract <inputFile> <outputDirectory>");
                            var input = args[1];
                            var outputDir = args[2];
                            if (!File.Exists(input))
                                throw new FileNotFoundException(input);
                            ArchiveExtractor.ExtractArchive(input, outputDir);
                            Console.WriteLine("Extraction complete.");
                            break;
                        default:
                            throw new ArgumentException($"Unknown command: {cmd}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    exitCode = 1;
                }
                // Exit immediately with code
                Environment.Exit(exitCode);
                return;
            }
            // Otherwise start normal WPF UI
            // Create MainWindow manually so we can safely assign resources at runtime
            base.OnStartup(e);
            try
            {
                // Attempt to load UI resource dictionary (ClassicStyle) at runtime. Guard against WIC/XAML failures.
                try
                {
                    Log("Loading ClassicStyle.xaml candidates...");
                    // Try several URI formats to locate the compiled ResourceDictionary reliably.
                    // Some build configurations embed pages under the pack:// application component path,
                    // so attempt the simple relative path first and fall back to explicit pack URIs.
                    string[] candidates = new[] {
                        "ClassicStyle.xaml",
                        "/ProcessorEmulator;component/ClassicStyle.xaml",
                        "pack://application:,,,/ProcessorEmulator;component/ClassicStyle.xaml"
                    };

                    ResourceDictionary dict = null;
                    Exception lastEx = null;
                    foreach (var c in candidates)
                    {
                        try
                        {
                            var uriKind = c.StartsWith("pack://") ? UriKind.Absolute : UriKind.Relative;
                            var rd = new ResourceDictionary() { Source = new Uri(c, uriKind) };
                            // If no exception thrown, assume success and use this dictionary
                            dict = rd;
                            System.Diagnostics.Debug.WriteLine($"[App] Loaded ClassicStyle from '{c}'");
                            break;
                        }
                        catch (Exception ex)
                        {
                            lastEx = ex;
                            System.Diagnostics.Debug.WriteLine($"[App] ClassicStyle try '{c}' failed: {ex.Message}");
                            Log($"ClassicStyle try '{c}' failed: {ex.Message}");
                        }
                    }

                    if (dict != null)
                    {
                        Current.Resources.MergedDictionaries.Add(dict);
                        Log($"ClassicStyle loaded from candidate; merged into Application resources.");
                    }
                    else
                    {
                        // If all attempts failed, log the last exception and install a minimal fallback.
                        System.Diagnostics.Debug.WriteLine($"[App] Failed to load ClassicStyle.xaml (all candidates). Last error: {lastEx}");
                        Log($"Failed to load ClassicStyle.xaml (all candidates). Last error: {lastEx}");
                        var fallback = new ResourceDictionary();
                        fallback["WindowBackgroundBrush"] = SystemColors.WindowBrush;
                        fallback["ControlBackgroundBrush"] = SystemColors.ControlBrush;
                        Current.Resources.MergedDictionaries.Add(fallback);
                    }
                }
                catch (Exception rex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] Unexpected failure loading ClassicStyle.xaml: {rex}");
                    Log($"Unexpected failure loading ClassicStyle.xaml: {rex.Message}");
                    var fallback = new ResourceDictionary();
                    fallback["WindowBackgroundBrush"] = SystemColors.WindowBrush;
                    fallback["ControlBackgroundBrush"] = SystemColors.ControlBrush;
                    Current.Resources.MergedDictionaries.Add(fallback);
                }

                // Try to merge Win7Styles.xaml dictionary (ported 7.css look)
                try
                {
                    Log("Merging Win7Styles.xaml...");
                    var w7 = new ResourceDictionary { Source = new Uri("/ProcessorEmulator;component/Win7Styles.xaml", UriKind.Relative) };
                    Current.Resources.MergedDictionaries.Add(w7);
                    Log("Win7Styles merged successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] Failed to load Win7Styles.xaml: {ex.Message}");
                    Log($"Failed to load Win7Styles.xaml: {ex.Message}");
                }

                Log("Creating MainWindow instance...");
                var main = new MainWindow();
                Log("MainWindow created.");
                // Force Win7 style overrides for Win8+ hosts
                try {
                    if (OperatingSystem.IsWindows()) {
                        AppThemeManager.LoadAndApplySaved(Current);
                        Log($"Theme applied: {AppThemeManager.Current}");
                    }
                    else {
                        Log("Skipping theme apply; non-Windows platform");
                    }
                }
                catch (Exception apx) {
                    Log("Theme apply failed: " + apx.Message);
                }
                // Try to set icon from Resources, but swallow any imaging errors
                try
                {
                    var iconUri = new Uri("pack://application:,,,/Resources/wow64_microsoft-windows-htmlhelp_31bf3856ad364e35_6_ICONDISK350.ico", UriKind.Absolute);
                    main.Icon = BitmapFrame.Create(iconUri);
                    Log("Loaded ICO resource successfully.");
                }
                catch (Exception ex)
                {
                    // Log and continue without icon
                    System.Diagnostics.Debug.WriteLine($"[App] Failed to load window icon: {ex.Message}");
                    Log("Failed to load ICO: " + ex.Message);
                }
                // After merging resources, write a short snapshot of resource keys so we can inspect them
                try
                {
                    var keys = Current.Resources.Keys;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine(DateTime.Now.ToString("o") + " Resource snapshot:");
                    int count = 0;
                    foreach (var k in keys)
                    {
                        sb.AppendLine(" - " + (k?.ToString() ?? "(null)"));
                        count++;
                        if (count > 200) { sb.AppendLine("  ...truncated"); break; }
                    }
                    File.AppendAllText(StartupLogPath, sb.ToString());
                }
                catch { }
                Log("Showing MainWindow...");
                MainWindow = main;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                main.Show();
                Log("MainWindow shown.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Failed to create MainWindow: {ex.Message}");
                Log("Failed to create/show MainWindow: " + ex.Message + "\n" + ex);
                // Fallback to default startup behavior
                base.OnStartup(e);
            }
        }
    }
}