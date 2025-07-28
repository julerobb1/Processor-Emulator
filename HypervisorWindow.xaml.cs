using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class HypervisorWindow : Window
    {
        private readonly VirtualMachineHypervisor hypervisor;
        private TextBox logBox;
        private TextBlock statusText;
        private Button powerOnButton;
        private Button resetButton;

        public HypervisorWindow(VirtualMachineHypervisor hypervisor, string platformName)
        {
            this.hypervisor = hypervisor;
            this.Title = $"VMware-Style Hypervisor - {platformName}";
            
            CreateUI();
            this.hypervisor.OnBootMessage += AppendLog;
        }

        private void CreateUI()
        {
            // Set window properties directly
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
            statusText = new TextBlock
            {
                Text = "Initializing...",
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Yellow,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            mainStack.Children.Add(statusText);

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

            logBox = new TextBox
            {
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.LightGreen,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                BorderThickness = new Thickness(0),
                AcceptsReturn = true
            };
            scrollViewer.Content = logBox;

            // Buttons
            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            mainStack.Children.Add(buttonStack);

            powerOnButton = new Button
            {
                Content = "Power On",
                Width = 100,
                Margin = new Thickness(5)
            };
            powerOnButton.Click += PowerOn_Click;
            buttonStack.Children.Add(powerOnButton);

            resetButton = new Button
            {
                Content = "Reset",
                Width = 100,
                Margin = new Thickness(5),
                IsEnabled = false
            };
            resetButton.Click += Reset_Click;
            buttonStack.Children.Add(resetButton);
        }

        private void AppendLog(string message)
        {
            // Marshal to UI thread
            Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                logBox.AppendText($"[{timestamp}] {message}\n");
                logBox.ScrollToEnd();
                statusText.Text = message;
            });
        }

        private async void PowerOn_Click(object sender, RoutedEventArgs e)
        {
            powerOnButton.IsEnabled = false;
            resetButton.IsEnabled = true;
            await hypervisor.PowerOn();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            hypervisor.Reset();
        }
    }
}
