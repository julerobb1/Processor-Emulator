using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class HypervisorWindow : Window
    {
        private readonly RealMipsHypervisor hypervisor;
        private TextBox logBox;
        private TextBlock statusText;
        private Button startButton;
        private Button stopButton;

        public HypervisorWindow(RealMipsHypervisor hypervisor, string platformName)
        {
            this.hypervisor = hypervisor;
            this.Title = $"Real MIPS Hypervisor - {platformName}";
            
            CreateUI();
            this.hypervisor.OnRealExecution += AppendLog;
        }
        
        // Constructor for backward compatibility
        public HypervisorWindow(object legacyHypervisor, string platformName)
        {
            // Create new real hypervisor if old one is passed
            this.hypervisor = new RealMipsHypervisor();
            this.Title = $"Real MIPS Hypervisor - {platformName}";
            
            CreateUI();
            this.hypervisor.OnRealExecution += AppendLog;
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
                Text = "REAL MIPS EMULATOR",
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
                Text = "AT&T U-verse / Microsoft Mediaroom",
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

            startButton = new Button
            {
                Content = "Start U-verse",
                Width = 120,
                Margin = new Thickness(5)
            };
            startButton.Click += Start_Click;
            buttonStack.Children.Add(startButton);

            stopButton = new Button
            {
                Content = "Stop",
                Width = 100,
                Margin = new Thickness(5),
                IsEnabled = false
            };
            stopButton.Click += Stop_Click;
            buttonStack.Children.Add(stopButton);
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

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            
            // Start real U-verse emulation
            bool success = await hypervisor.StartEmulation();
            if (!success)
            {
                AppendLog("❌ Failed to start U-verse emulation");
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            hypervisor.StopEmulation();
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
        }
    }
}
