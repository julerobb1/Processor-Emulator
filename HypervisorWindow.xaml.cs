using System;
using System.Windows;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class HypervisorWindow : Window
    {
        private readonly VirtualMachineHypervisor hypervisor;

        public HypervisorWindow(VirtualMachineHypervisor hypervisor, string platformName)
        {
            InitializeComponent();
            this.hypervisor = hypervisor;
            this.Title = $"VMware-Style Hypervisor - {platformName}";
            // Subscribe to boot messages
            this.hypervisor.OnBootMessage += AppendLog;
        }

        private void AppendLog(string message)
        {
            // Marshal to UI thread
            Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                LogBox.AppendText($"[{timestamp}] {message}\n");
                LogBox.ScrollToEnd();
                StatusText.Text = message;
            });
        }

        private async void PowerOn_Click(object sender, RoutedEventArgs e)
        {
            PowerOnButton.IsEnabled = false;
            ResetButton.IsEnabled = true;
            await hypervisor.PowerOn();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            hypervisor.Reset();
        }
    }
}
