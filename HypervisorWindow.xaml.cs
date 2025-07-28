using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class HypervisorWindow : Window
    {
        private readonly VirtualMachineHypervisor hypervisor;
        private TextBox LogBox;
        private TextBlock StatusText;
        private Button PowerOnButton;
        private Button ResetButton;

        public HypervisorWindow(VirtualMachineHypervisor hypervisor, string platformName)
        {
            this.hypervisor = hypervisor;
            this.Title = $"VMware-Style Hypervisor - {platformName}";
            
            // Manual UI creation instead of XAML
            CreateUI();
            
            // Subscribe to boot messages
            this.hypervisor.OnBootMessage += AppendLog;
        }

        private void CreateUI()
        {
            Width = 800;
            Height = 600;
            Background = System.Windows.Media.Brushes.Black;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var mainStack = new StackPanel();
            Content = mainStack;

            // Title
            var titleText = new TextBlock
            {
                Text = "X1 PLATFORM",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            mainStack.Children.Add(titleText);

            // Subtitle
            var subtitleText = new TextBlock
            {
                Text = "BIOS Version 3.2.1 - ARM Cortex-A15",
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainStack.Children.Add(subtitleText);

            // Status
            StatusText = new TextBlock
            {
                Text = "Initializing...",
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Yellow,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            mainStack.Children.Add(StatusText);

            // Log area
            var border = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.Black
            };
            mainStack.Children.Add(border);

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            border.Child = scrollViewer;

            LogBox = new TextBox
            {
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.LightGreen,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                BorderThickness = new Thickness(0),
                AcceptsReturn = true
            };
            scrollViewer.Content = LogBox;

            // Buttons
            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            mainStack.Children.Add(buttonStack);

            PowerOnButton = new Button
            {
                Content = "Power On",
                Width = 100,
                Margin = new Thickness(5)
            };
            PowerOnButton.Click += PowerOn_Click;
            buttonStack.Children.Add(PowerOnButton);

            ResetButton = new Button
            {
                Content = "Reset",
                Width = 100,
                Margin = new Thickness(5),
                IsEnabled = false
            };
            ResetButton.Click += Reset_Click;
            buttonStack.Children.Add(ResetButton);
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
