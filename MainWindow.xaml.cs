using ProcessorEmulator.Emulation;
using ProcessorEmulator.Tools;
using System.Windows;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ProcessorEmulator
{
    public interface IMainWindow
    {
        TextBlock StatusBar { get; set; }
        PartitionAnalyzer PartitionAnalyzer { get; set; }
        InstructionDispatcher Dispatcher1 { get; set; }

        bool Equals(object obj);
        int GetHashCode();
    }

    public partial class MainWindow : Window, IMainWindow
    {
        private IEmulator currentEmulator;
        private ArchitectureDetector archDetector = new ArchitectureDetector();
        private PartitionAnalyzer partitionAnalyzer = new PartitionAnalyzer();
        private Disassembler disassembler = new Disassembler();
        private Recompiler recompiler = new Recompiler();
        private ExoticFilesystemManager fsManager = new ExoticFilesystemManager();

        public MainWindow(TextBlock statusBar)
        {
            this.StatusBar = statusBar;
        }

        /// <summary>
        /// Handles the dispatching and execution of instructions within the emulator.
        /// </summary>
        private InstructionDispatcher dispatcher = new InstructionDispatcher();

        public TextBlock StatusBar { get; set; } = new TextBlock();
        public PartitionAnalyzer PartitionAnalyzer { get => partitionAnalyzer; set => partitionAnalyzer = value; }
        public InstructionDispatcher Dispatcher1 { get => dispatcher; set => dispatcher = value; }

        private void StatusBarText(string text)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => StatusBarText(text));
                return;
            }
            StatusBar.Text = text;
        }

        private IEmulator GetCurrentEmulator()
        {
            return currentEmulator;
        }

        private async void StartEmulation_Click(object sender,
                                                RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported Files|*.exe;*.dll;*.img;*.bin|WinCE Applications|*.exe|Firmware Images|*.img;*.bin|All Files|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            byte[] binary = File.ReadAllBytes(openFileDialog.FileName);
            string arch = ArchitectureDetector.Detect(binary);
            bool isWinCE = IsWinCEBinary(binary);

            // Show detected architecture for debugging
            MessageBox.Show($"Detected architecture: {arch ?? "(none)"}", "Architecture Detection", MessageBoxButton.OK, MessageBoxImage.Information);

            // If unknown, prompt user
            if (string.IsNullOrEmpty(arch) || arch == "Unknown")
            {
                arch = PromptUserForInput("Architecture could not be detected. Please enter architecture (e.g., MIPS32, ARM, x86, etc.):");
                if (string.IsNullOrWhiteSpace(arch))
                {
                    MessageBox.Show("Architecture is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                if (isWinCE)
                {
                    currentEmulator = new WinCEEmulator();
                }
                else
                {
                    currentEmulator = arch switch
                    {
                        "MIPS32" => new Mips32Emulator(),
                        "MIPS64" => new Mips64Emulator(),
                        "ARM" => new ArmEmulator(),
                        "ARM64" => new Arm64Emulator(),
                        "PowerPC" => new PowerPcEmulator(),
                        "x86" => new X86Emulator(),
                        "x86-64" => new X64Emulator(),
                        _ => throw new Exception("Unknown architecture"),
                    };
                }

                StatusBarText($"Loading {Path.GetFileName(openFileDialog.FileName)}...");
                currentEmulator.LoadBinary(binary);
                StatusBarText("Running emulation...");
                // Use dispatcher for unified instruction dispatching
                await Task.Run(() =>
                {
                    // Example: for each instruction, dispatch to the correct emulator
                    // This is a placeholder; real implementation would parse instructions from binary
                    // and call dispatcher.Dispatch(instruction, arch, hostArch)
                    currentEmulator.Run();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Emulation error: {ex.Message}\n\nIf this is a QEMU error, ensure qemu-system-mips.exe is installed and in your PATH or in the application directory.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Emulation failed.");
            }
        }

        private static bool IsWinCEBinary(byte[] binary)
        {
            // Check PE header and subsystem for WinCE
            if (binary.Length < 0x40) return false;
            // Check for PE signature
            if (binary[0] != 0x4D || binary[1] != 0x5A) return false;
            // More detailed PE header checks would go here
            return true;
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

        private void LoadFirmwareImage(string imagePath, string signaturePath)
        {
            // Copy firmware image to temp folder to avoid modifying originals
            string tempDir = Path.Combine(Path.GetTempPath(), "ProcessorEmulator", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string tempImagePath = Path.Combine(tempDir, Path.GetFileName(imagePath));
            File.Copy(imagePath, tempImagePath, overwrite: true);

            string chipsetName = null;
            string rootFilesystemType = null;

            // Only handle signature file if it is provided and exists
            string tempSignaturePath = null;
            if (!string.IsNullOrEmpty(signaturePath) && File.Exists(signaturePath))
            {
                tempSignaturePath = Path.Combine(tempDir, Path.GetFileName(signaturePath));
                File.Copy(signaturePath, tempSignaturePath, overwrite: true);

                // Try to parse signature/config file if present
                foreach (var line in File.ReadAllLines(tempSignaturePath))
                {
                    if (line.StartsWith("CHIPSET=", StringComparison.OrdinalIgnoreCase))
                        chipsetName = line.Substring("CHIPSET=".Length).Trim();
                    if (line.StartsWith("FS=", StringComparison.OrdinalIgnoreCase))
                        rootFilesystemType = line.Substring("FS=".Length).Trim();
                }
            }

            // If not found, use heuristics (scan firmware image for known patterns)
            if (chipsetName == null || rootFilesystemType == null)
            {
                byte[] fw = File.ReadAllBytes(tempImagePath);
                string fwStr = System.Text.Encoding.ASCII.GetString(fw);

                // Example heuristic: look for known chipset names
                if (fwStr.Contains("Contoso6311"))
                    chipsetName = "Contoso6311";
                else if (fwStr.Contains("FooChip9000"))
                    chipsetName = "FooChip9000";
                // Add more heuristics as needed

                // Example heuristic: look for filesystem markers
                if (fwStr.Contains("JFFS2"))
                    rootFilesystemType = "JFFS2";
                else if (fwStr.Contains("UBIFS"))
                    rootFilesystemType = "UBIFS";
                // Add more heuristics as needed
            }

            // If still not found, prompt user
            if (chipsetName == null)
            {
                chipsetName = PromptUserForInput("Chipset not detected. Please enter chipset name:");
                if (string.IsNullOrWhiteSpace(chipsetName))
                {
                    MessageBox.Show("Chipset is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (rootFilesystemType == null)
            {
                rootFilesystemType = PromptUserForInput("Filesystem type not detected. Please enter filesystem type (e.g., JFFS2):");
                if (string.IsNullOrWhiteSpace(rootFilesystemType))
                {
                    MessageBox.Show("Filesystem type is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Load Chipset Emulator
            string chipsetConfigPath = $"Configs/{chipsetName}.json";
            if (!fsManager.LoadChipsetEmulator(chipsetName, chipsetConfigPath))
            {
                MessageBox.Show($"Failed to load chipset emulator for {chipsetName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mount Filesystem (example only supports JFFS2)
            string mountPoint = "/mnt/firmware";
            if (rootFilesystemType.Equals("JFFS2", StringComparison.OrdinalIgnoreCase))
            {
                fsManager.MountJFFS2(tempImagePath, mountPoint);
            }
            else
            {
                MessageBox.Show($"Filesystem type '{rootFilesystemType}' is not supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper to prompt user for input (simple dialog)
        private string PromptUserForInput(string message)
        {
            var inputDialog = new Window
            {
                Title = "Input Required",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };
            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 10) });
            var textBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(textBox);
            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
            stack.Children.Add(okButton);
            inputDialog.Content = stack;

            string result = null;
            okButton.Click += (s, e) => { result = textBox.Text; inputDialog.DialogResult = true; inputDialog.Close(); };
            inputDialog.ShowDialog();
            return result;
        }

        private byte[] ReadAndTranslateFile(string filePath, string fromArch, string toArch)
        {
            // Read a file and translate/virtualize (example)
            byte[] fileData = fsManager.ReadFile(filePath);

            // Translate and Virtualize
            byte[] result = fsManager.RunTranslatedAndVirtualized(fileData, fromArch, toArch);
            return result;
        }

        // Removed override of Equals(object) because DependencyObject.Equals(object) is sealed and cannot be overridden.

        // Removed GetHashCode override because DependencyObject.GetHashCode() is sealed and cannot be overridden.
    }
}

// Note: The actual entrypoint for the application is defined in App.xaml/App.xaml.cs,
// which launches MainWindow. All emulation logic is triggered from MainWindow event handlers.
// Note: The actual entrypoint for the application is defined in App.xaml/App.xaml.cs,
// which launches MainWindow. All emulation logic is triggered from MainWindow event handlers.
// which launches MainWindow. All emulation logic is triggered from MainWindow event handlers.
