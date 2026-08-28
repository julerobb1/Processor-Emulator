using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Win7VisualStyle.EnableHost();
            base.OnStartup(e);

            // global exception handlers for debugging
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
                var ex = ev.ExceptionObject as Exception;
                MessageBox.Show($"Unhandled domain exception: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            DispatcherUnhandledException += (s, ev) =>
            {
                MessageBox.Show($"Dispatcher exception: {ev.Exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ev.Handled = true;
            };

            // Attempt to manually load classic style if present
            try
            {
                var uri = new Uri("ClassicStyle.xaml", UriKind.Relative);
                var dict = new ResourceDictionary { Source = uri };
                Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load ClassicStyle.xaml: {ex.Message}");
            }
            try
            {
                var w7 = new ResourceDictionary { Source = new Uri("Win7Styles.xaml", UriKind.Relative) };
                Resources.MergedDictionaries.Add(w7);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load Win7Styles.xaml: {ex.Message}");
            }
            try { Windows7ThemeManager.Apply(this); }
            catch (Exception ex) { Debug.WriteLine($"Win7 theme: {ex.Message}"); }

            // handle command-line test mode before showing GUI
            if (e.Args != null && e.Args.Length > 0 && e.Args[0] == "--test-uverse")
            {
                // run the async test synchronously for simplicity
                UverseEmulatorTest.RunTest().GetAwaiter().GetResult();
                // after test exit application immediately
                Current.Shutdown();
                return;
            }

        }
    }
}
