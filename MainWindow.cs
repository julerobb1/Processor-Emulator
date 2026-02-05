using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading; // Added for Dispatcher.Invoke
using ProcessorEmulator.Emulation; // Added for MipsCpuEmulator

namespace ProcessorEmulator
{
    // Use the real MipsBus and CP0 implementations from ProcessorEmulator.Emulation

    public partial class MainWindow : Window
    {
        // Our WinForms hardware components
        private RichTextBox _serialConsole;
        private PictureBox _videoDisplay;
        private MipsCpuEmulator _emulator; // Declared MipsCpuEmulator

        public MainWindow()
        {
            InitializeComponent();
            SetupClassicUI();

            // Instantiate real CP0 and MipsBus, then create the emulator and subscribe to logs
            var cp0 = new CP0();
            var bus = new MipsBus(cp0);
            _emulator = new MipsCpuEmulator(bus, cp0);
            _emulator.OnLogMessage += AppendToSerialConsole;
        }

        private void SetupClassicUI()
        {
            // 1. Create the Video Tab (The XP/7 Pro look)
            TabPage videoPage = new TabPage("Video Output");
            _videoDisplay = new PictureBox { 
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.Black,
                SizeMode = PictureBoxSizeMode.CenterImage 
            };
            videoPage.Controls.Add(_videoDisplay);

            // 2. Create the Serial Console Tab (The Terminal look)
            TabPage consolePage = new TabPage("System Console");
            _serialConsole = new RichTextBox { 
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.Black, 
                ForeColor = System.Drawing.Color.Lime,
                Font = new System.Drawing.Font("Lucida Console", 9f),
                ReadOnly = true
            };
            consolePage.Controls.Add(_serialConsole);

            // Add them to the WindowsFormsHost container
            MainTabs.TabPages.Add(videoPage);
            MainTabs.TabPages.Add(consolePage);
        }

        private void AppendToSerialConsole(string message)
        {
            // Ensure UI update is on the correct thread
            // Use the WPF Dispatcher from this Window to avoid ambiguous 'Application' type
            this.Dispatcher.Invoke(() =>
            {
                _serialConsole.AppendText(message);
                // WinForms.RichTextBox doesn't have ScrollToEnd; use ScrollToCaret after moving selection
                _serialConsole.SelectionStart = _serialConsole.Text.Length;
                _serialConsole.ScrollToCaret(); // Auto-scroll to the latest message
            });
        }


        private void LoadBinary_Click(object sender, RoutedEventArgs e)
        {
            // Trigger your Smart Loader logic here
            StatusText.Text = "Loading Binary...";
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => this.Close();
        
        private void ShowMemoryMap_Click(object sender, RoutedEventArgs e) => 
            System.Windows.MessageBox.Show("Displaying Resource Map...");

        private void ShowInterrupts_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Displaying Interrupts...");
        }
    }
}
