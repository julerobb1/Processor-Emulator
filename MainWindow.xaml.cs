using ProcessorEmulator.Emulation;
using ProcessorEmulator.Tools;
using ProcessorEmulator.Architectures;
using System.Windows;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;

namespace ProcessorEmulator
{
    public partial class MainWindow : Window
    {
        private IEmulator currentEmulator;
        private ArchitectureDetector archDetector = new ArchitectureDetector();
        private PartitionAnalyzer partitionAnalyzer = new PartitionAnalyzer();
        private Disassembler disassembler = new Disassembler();
        private Recompiler recompiler = new Recompiler();
        private ExoticFilesystemManager fsManager = new ExoticFilesystemManager();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartEmulation_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported Files|*.exe;*.dll;*.img;*.bin|WinCE Applications|*.exe|Firmware Images|*.img;*.bin|All Files|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            byte[] binary = File.ReadAllBytes(openFileDialog.FileName);
            string arch = archDetector.Detect(binary);
            bool isWinCE = IsWinCEBinary(binary);

            try
            {
                if (isWinCE)
                {
                    currentEmulator = new WinCEEmulator();
                }
                else
                {
                    switch (arch)
                    {
                        case "MIPS32": currentEmulator = new Mips32Emulator(); break;
                        case "MIPS64": currentEmulator = new Mips64Emulator(); break;
                        case "ARM": currentEmulator = new ArmEmulator(); break;
                        case "ARM64": currentEmulator = new Arm64Emulator(); break;
                        case "PowerPC": currentEmulator = new PowerPcEmulator(); break;
                        case "x86": currentEmulator = new X86Emulator(); break;
                        case "x86-64": currentEmulator = new X64Emulator(); break;
                        default: throw new Exception("Unknown architecture");
                    }
                }

                StatusBarText($"Loading {Path.GetFileName(openFileDialog.FileName)}...");
                currentEmulator.LoadBinary(binary);
                StatusBarText("Running emulation...");
                await Task.Run(() => currentEmulator.Run());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Emulation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Emulation failed.");
            }
        }

        private bool IsWinCEBinary(byte[] binary)
        {
            // Check PE header and subsystem for WinCE
            if (binary.Length < 0x40) return false;
            // Check for PE signature
            if (binary[0] != 0x4D || binary[1] != 0x5A) return false;
            // More detailed PE header checks would go here
            return true;
        }

        private void StatusBarText(string text)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => StatusBarText(text));
                return;
            }
            statusBar.Text = text;
        }

        private async void ExtractAndAnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Firmware Archives (*.csw;*.tar;*.img;*.bin)|*.csw;*.tar;*.img;*.bin|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string archivePath = openFileDialog.FileName;
                string extractDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(archivePath),
                    System.IO.Path.GetFileNameWithoutExtension(archivePath) + "_extracted");
                StatusBarText("Extracting and analyzing...");
                await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(archivePath, extractDir));
                StatusBarText("Done. See console for results.");
            }
        }

        private void StatusBarText(string text)
        {
            // Update your status bar here, e.g.:
            // statusBarItem.Content = text;
        }
    }
}
