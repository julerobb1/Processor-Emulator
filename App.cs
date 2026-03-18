using System;
using System.Diagnostics;
using System.Windows;

namespace ProcessorEmulator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // global exception handlers for debugging
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
                var ex = ev.ExceptionObject as Exception;
                MessageBox.Show($"Unhandled domain exception: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            this.DispatcherUnhandledException += (s, ev) =>
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

            // handle command-line test mode before showing GUI
            if (e.Args != null && e.Args.Length > 0 && e.Args[0] == "--test-uverse")
            {
                // run the async test synchronously for simplicity
                ProcessorEmulator.UverseEmulatorTest.RunTest().GetAwaiter().GetResult();
                // after test exit application immediately
                Current.Shutdown();
                return;
            }

            // Show main window explicitly
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
        }
    }
}
