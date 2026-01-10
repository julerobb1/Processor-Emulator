using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace ProcessorEmulator
{
    public partial class MainWindow : Window
    {
        // Our WinForms hardware components
        private RichTextBox _serialConsole;
        private PictureBox _videoDisplay;

        public MainWindow()
        {
            InitializeComponent();
            SetupClassicUI();
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
