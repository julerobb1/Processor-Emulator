using System;
using System.Drawing;
using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Windows.Threading; // Added for Dispatcher.Invoke
using ProcessorEmulator.Emulation; // Added for MipsCpuEmulator

namespace ProcessorEmulator
{
    // Use the real MipsBus and CP0 implementations from ProcessorEmulator.Emulation

    public partial class MainWindow : Window
    {
        // Our WinForms hardware components
        private WinForms.RichTextBox _serialConsole;
        private WindowsCEExecutor _ceExecutor;
        private string _selectedExecutablePath;
        private DispatcherTimer _uiUpdateTimer;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += (_, __) => Win7VisualStyle.ApplyToWindow(this);
            SetupClassicUI();

            _ceExecutor = new WindowsCEExecutor();

            // Timer to refresh UI components like the process list
            _uiUpdateTimer = new DispatcherTimer();
            _uiUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            _uiUpdateTimer.Start();
        }

        private void SetupClassicUI()
        {
            // Create the Serial Console Tab (The Terminal look)
            _serialConsole = new WinForms.RichTextBox {
                Dock = WinForms.DockStyle.Fill,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.Lime,
                Font = new System.Drawing.Font("Lucida Console", 9f),
                ReadOnly = true
            };
            
            // Add the console to the new WindowsFormsHost
            ConsoleHost.Child = _serialConsole;
        }

        private void AppendToSerialConsole(string message)
        {
            // Ensure UI update is on the correct thread
            this.Dispatcher.Invoke(() =>
            {
                _serialConsole.AppendText(message + "\n");
                _serialConsole.SelectionStart = _serialConsole.Text.Length;
                _serialConsole.ScrollToCaret(); // Auto-scroll to the latest message
            });
        }
        
        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateProcessList();
        }

        private void UpdateProcessList()
        {
            var processes = _ceExecutor.GetRunningProcesses();
            ProcessListView.ItemsSource = processes;
        }

        private void LoadExeButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Windows CE Executables (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "Load Windows CE Executable"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedExecutablePath = dialog.FileName;
                StatusText.Text = $"Loaded: {Path.GetFileName(_selectedExecutablePath)}";
                RunButton.IsEnabled = true;
                AppendToSerialConsole($"Selected executable: {_selectedExecutablePath}");
            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedExecutablePath))
            {
                MessageBox.Show("Please load an executable first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RunButton.IsEnabled = false;
            StatusText.Text = $"Executing: {Path.GetFileName(_selectedExecutablePath)}...";
            AppendToSerialConsole($"--- Starting Execution of {_selectedExecutablePath} ---");

            var result = await _ceExecutor.ExecuteAsync(_selectedExecutablePath);

            if (result.Success)
            {
                StatusText.Text = $"Execution finished. Exit Code: {result.ExitCode}";
                AppendToSerialConsole($"--- Execution Finished. Exit Code: {result.ExitCode} ---");
            }
            else
            {
                StatusText.Text = $"Execution failed: {result.Error}";
                AppendToSerialConsole($"--- Execution Failed: {result.Error} ---");
            }

            foreach (var log in result.Log)
            {
                AppendToSerialConsole(log);
            }
            
            _selectedExecutablePath = null;
            UpdateProcessList();
        }

        private void LoadBinary_Click(object sender, RoutedEventArgs e)
        {
            // Point the old menu item to the new button's logic
            LoadExeButton_Click(sender, e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Loaded event fired, window should be visible.");
            StatusText.Text = "UI Ready";
            MessageBox.Show("MainWindow loaded", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
