using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProcessorEmulator.Emulation
{
    /// <summary>
    /// Display window for showing PXRenderer framebuffer output.
    /// This is the actual "boot screen" that shows firmware visual output.
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private Image displayImage;
        private PXRenderer renderer;
        private DispatcherTimer refreshTimer;

        public DisplayWindow(PXRenderer pxRenderer)
        {
            renderer = pxRenderer;
            InitializeWindow();
            SetupRefreshTimer();
        }

        private void InitializeWindow()
        {
            Title = "RDK-V Firmware Display - ARM Emulation";
            Width = PXRenderer.RDK_HD_WIDTH + 40;
            Height = PXRenderer.RDK_HD_HEIGHT + 80;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Colors.Black);

            // Create image control for framebuffer display
            displayImage = new Image
            {
                Stretch = Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create content panel
            var panel = new DockPanel();
            
            // Status bar
            var statusBar = new TextBlock
            {
                Text = "ARM Firmware Boot Display - Waiting for visual output...",
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.DarkBlue),
                Padding = new Thickness(10, 5, 10, 5),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12
            };
            DockPanel.SetDock(statusBar, Dock.Bottom);
            panel.Children.Add(statusBar);

            // Main display area
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(2),
                Child = displayImage
            };
            panel.Children.Add(border);

            Content = panel;

            // Update status bar reference
            StatusBar = statusBar;
        }

        public TextBlock StatusBar { get; private set; }

        private void SetupRefreshTimer()
        {
            // Refresh display at 60 FPS for smooth firmware boot animation
            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            refreshTimer.Tick += RefreshDisplay;
            refreshTimer.Start();
        }

        private void RefreshDisplay(object sender, EventArgs e)
        {
            try
            {
                if (renderer?.IsInitialized == true && renderer.Framebuffer != null)
                {
                    // Update display image with current framebuffer
                    displayImage.Source = renderer.Framebuffer;
                    
                    // Update status
                    StatusBar.Text = $"ARM Firmware Display - {renderer.GetFramebufferStats()} - Active";
                }
                else
                {
                    StatusBar.Text = "ARM Firmware Display - Initializing framebuffer...";
                }
            }
            catch (Exception ex)
            {
                StatusBar.Text = $"Display Error: {ex.Message}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer = null;
            base.OnClosed(e);
        }

        /// <summary>
        /// Show boot message on display
        /// </summary>
        public void ShowBootMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusBar.Text = $"ARM Boot: {message}";
            });
        }

        /// <summary>
        /// Flash the display border to indicate activity
        /// </summary>
        public void FlashActivity()
        {
            Dispatcher.Invoke(() =>
            {
                var border = (Border)((DockPanel)Content).Children[1];
                border.BorderBrush = new SolidColorBrush(Colors.Lime);
                
                // Reset border color after short flash
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += (s, e) =>
                {
                    border.BorderBrush = new SolidColorBrush(Colors.Gray);
                    timer.Stop();
                };
                timer.Start();
            });
        }
    }
}
