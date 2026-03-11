using System;
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

            // Show main window explicitly
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
