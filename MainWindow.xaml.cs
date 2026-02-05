using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading; // Added for Dispatcher.Invoke
using ProcessorEmulator.Emulation; // Added for MipsCpuEmulator

namespace ProcessorEmulator
{
    // Placeholder for MipsBus - will be properly defined elsewhere
    public class MipsBus 
    { 
        public uint Read32(uint address) { return 0; } // Placeholder
        public void Write32(uint address, uint value) { /* Placeholder */ } // Placeholder
        public uint Translate(uint address) { return address; } // Placeholder
    } 

    // Placeholder for CP0 - will be properly defined elsewhere
    public class CP0 
    { 
        public bool ShouldTriggerInterrupt() { return false; } // Placeholder
        public void UpdateTimer(int cycles) { /* Placeholder */ } // Placeholder
        public uint EPC { get; set; } // Placeholder
        public uint Cause { get; set; } // Placeholder
        public uint Status { get; set; } // Placeholder
        public void ReadTLBEntry() { /* Placeholder */ } // Placeholder
        public void WriteTLBEntryIndexed() { /* Placeholder */ } // Placeholder
        public void WriteTLBEntryRandom() { /* Placeholder */ } // Placeholder
        public void ProbeTLB() { /* Placeholder */ } // Placeholder
    }

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

            // Instantiate MipsCpuEmulator and subscribe to its log event
            // Using placeholder MipsBus and CP0 for now
            _emulator = new MipsCpuEmulator(new MipsBus(), new CP0()); 
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                _serialConsole.AppendText(message);
                _serialConsole.ScrollToEnd(); // Auto-scroll to the latest message
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
