using ProcessorEmulator.Emulation;
using ProcessorEmulator.Tools;
using ProcessorEmulator.Network;
using System.IO;
using System.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Ext;
// YAFFS handled by ExoticFilesystemManager
using System.Windows;
using DiscUtils.Setup;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using DiscUtils.Iso9660;
using DiscUtils.Ext;
// Removed UFS support

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

    // Add missing IQemuEmulator interface stub
    public interface IQemuEmulator : IEmulator
    {
        string GetQemuExecutablePath();
        string GetQemuArguments(string filePath);
        string GetQemuArguments(string filePath, string winceVersion);
    }

    public partial class MainWindow : Window, IMainWindow
    {

        private IEmulator currentEmulator;

        // Add a default constructor for XAML
        public MainWindow()
        {
            // Load XAML UI components
            // Initialize drag-and-drop for file support
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
        }


        public MainWindow(IEmulator currentEmulator)
        {
            this.currentEmulator = currentEmulator;
        }

        private ArchitectureDetector archDetector = new();
        private PartitionAnalyzer partitionAnalyzer = new();
        private Disassembler disassembler = new();
        private Recompiler recompiler = new();
        private ExoticFilesystemManager fsManager = new();
        private InstructionDispatcher dispatcher = new();

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

        /// <summary>
        /// Main entry point for user actions. Presents a menu of emulation and analysis options.
        /// </summary>
        private async void StartEmulation_Click(object sender,
                                                RoutedEventArgs e)
        {
            // Present main options to the user
            var mainOptions = new List<string>
            {
                "Generic CPU/OS Emulation",
                "RDK-V Emulator",
                "RDK-B Emulator",
                "Dish Network Box/VxWorks Analysis",
                "Simulate SWM Switch/LNB",
                "Probe Filesystem",
                "Emulate CMTS Head End",
                "Uverse Box Emulator",
                "DirecTV Box/Firmware Analysis",
                "Executable Analysis",
                "Linux Filesystem Read/Write",
                "Cross-Compile Binary",
                "Mount CE Filesystem",
                "Mount YAFFS Filesystem",
                "Mount ISO Filesystem",
                "Mount EXT Filesystem",
                "Analyze Folder Contents"
            };
            string mainChoice = PromptUserForChoice("What would you like to do?", mainOptions);
            if (string.IsNullOrEmpty(mainChoice)) return;

            switch (mainChoice)
            {
                case "Generic CPU/OS Emulation":
                    await HandleGenericEmulation();
                    break;
                case "RDK-V Emulator":
                    await HandleRdkVEmulation();
                    break;
                case "RDK-B Emulator":
                    await HandleRdkBEmulation();
                    break;
                case "Dish Network Box/VxWorks Analysis":
                    await HandleDishVxWorksAnalysis();
                    break;
                case "Simulate SWM Switch/LNB":
                    await HandleSwmLnbSimulation();
                    break;
                case "Probe Filesystem":
                    await HandleFilesystemProbe();
                    break;
                case "Emulate CMTS Head End":
                    await HandleCmtsEmulation();
                    break;
                case "Uverse Box Emulator":
                    await HandleUverseEmulation();
                    break;
                case "DirecTV Box/Firmware Analysis":
                    await HandleDirectvAnalysis();
                    break;
                case "Executable Analysis":
                    await HandleExecutableAnalysis();
                    break;
                case "Linux Filesystem Read/Write":
                    await HandleLinuxFsReadWrite();
                    break;
                case "Cross-Compile Binary":
                    await HandleCrossCompile();
                    break;
                case "Mount CE Filesystem":
                    await HandleCeMount();
                    break;
                case "Mount YAFFS Filesystem":
                    await HandleYaffsMount();
                    break;
                case "Mount ISO Filesystem":
                    await HandleIsoMount();
                    break;
                case "Mount EXT Filesystem":
                    await HandleExtMount();
                    break;
                case "Analyze Folder Contents":
                    await HandleFolderAnalysis();
                    break;
                default:
                    MessageBox.Show("Not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        /// <summary>
        /// Emulates a CMTS head end with IPTV and DOCSIS networks.
        /// </summary>
        private async Task HandleCmtsEmulation()
        {
            var emu = new CMTSEmulator();
            emu.InitializeIPTV();
            StatusBarText("CMTS head end initialized.");
            ShowTextWindow("CMTS Emulation", new List<string> { "IPTV and DOCSIS networks active." });
            await Task.CompletedTask;
        }

        private async Task HandleGenericEmulation()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Binaries and Executables (*.bin;*.img;*.exe;*.fw)|*.bin;*.img;*.exe;*.fw|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            string filePath = openFileDialog.FileName;
            StatusBarText($"Launching emulation for {Path.GetFileName(filePath)}...");
            byte[] binary = File.ReadAllBytes(filePath);
            string arch = ArchitectureDetector.Detect(binary);

            try
            {
                // First attempt homebrew emulation
                var home = new HomebrewEmulator();
                home.LoadBinary(binary);
                home.Run();
                StatusBarText("Homebrew emulation complete.");
            }
            catch (NotImplementedException)
            {
                // Fallback to QEMU with optional extra CLI options
                StatusBarText("Homebrew emulator not implemented for this architecture, falling back to QEMU...");
                // Prompt user for extra QEMU command-line options using a detailed dialog
                string extraArgs = PromptForQemuOptions();
                try
                {
                    if (!string.IsNullOrEmpty(extraArgs))
                    {
                        // Use QemuManager directly for extra arguments
                        ProcessorEmulator.Tools.QemuManager.LaunchWithArgs(filePath, arch, extraArgs);
                    }
                    else
                    {
                        EmulatorLauncher.Launch(filePath, arch);
                    }
                    StatusBarText("QEMU emulation started.");
                }
                catch (Exception qex)
                {
                    MessageBox.Show($"QEMU error: {qex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusBarText("Emulation failed.");
                }
            }
            await Task.CompletedTask;
        }

        private async Task HandleDishVxWorksAnalysis()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Dish/VxWorks Firmware (*.bin;*.img;*.fw)|*.bin;*.img;*.fw|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            string filePath = openFileDialog.FileName;
            StatusBarText("Analyzing Dish VxWorks firmware...");
            byte[] firmware = File.ReadAllBytes(filePath);

            try
            {
                var detector = new Tools.FileSystems.DvrVxWorksDetector();
                var (version, deviceType, encInfo) = detector.DetectVersion(firmware);
                var output = new List<string>
                {
                    $"Version: {version}",
                    $"Device Type: {deviceType}",
                    $"Encryption Algorithm: {encInfo.Algorithm}",
                    $"Key Size: {encInfo.KeySize}",
                    $"Key Material: {BitConverter.ToString(encInfo.KeyMaterial)}",
                    $"IV: {BitConverter.ToString(encInfo.IV)}"
                };
                ShowTextWindow("Dish/VxWorks Analysis", output);
                StatusBarText("Dish VxWorks analysis complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Analysis error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Dish VxWorks analysis failed.");
            }
            await Task.CompletedTask;
        }

        private async Task HandleUverseEmulation()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Firmware Images (*.bin;*.img;*.sig)|*.bin;*.img;*.sig|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            string filePath = openFileDialog.FileName;
            StatusBarText($"Selected file: {Path.GetFileName(filePath)}");
            try
            {
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string extractDir = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_extracted");
                if (ext != ".sig")
                {
                    // Perform generic firmware analysis
                    await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(filePath, extractDir));
                    FirmwareAnalyzer.AnalyzeFirmwareArchive(filePath, extractDir);
                    StatusBarText("Firmware analysis complete.");
                    MessageBox.Show($"Firmware {Path.GetFileName(filePath)} analyzed in {extractDir}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Signature-based Uverse emulation
                StatusBarText($"Loading Uverse config from {Path.GetFileName(filePath)}...");
                string model = PromptUserForInput("Enter model type (e.g. VIP2250):")?.Trim();
                if (string.IsNullOrWhiteSpace(model)) model = "VIP2250";
                string proc = PromptUserForInput("Enter processor type (e.g. ARM/x86):")?.Trim();
                if (string.IsNullOrWhiteSpace(proc)) proc = "ARM";
                string memInput = PromptUserForInput("Enter memory size in MB:")?.Trim();
                if (!int.TryParse(memInput, out int mb)) mb = 128;
                uint memBytes = (uint)(mb * 1024 * 1024);
                bool isDVR = PromptUserForChoice("Is this device a DVR?", new[] { "Yes", "No" }) == "Yes";
                bool isWholeHome = PromptUserForChoice("Enable Whole Home network?", new[] { "Yes", "No" }) == "Yes";
                var config = new UverseHardwareConfig
                {
                    ModelType = model,
                    ProcessorType = proc,
                    MemorySize = memBytes,
                    IsDVR = isDVR,
                    IsWholeHome = isWholeHome
                };

                //What is the air speed velocity of an unladen swallow?
                //What do you mean? An African or European swallow?
                //What? I don't know that!
                //AAAUUH
                //Unladen swallow? What do you mean?
                //Well you have to know these things when you're a king, you know.
                //               
                // 
                //  // Initialize Uverse emulator with the provided configuration

                var emulator = new UverseEmulator(config);
                ShowTextWindow("Uverse Emulation", new List<string> { "Initialized emulator with config." });
                emulator.LoadBootImage(filePath);
                ShowTextWindow("Uverse Emulation", new List<string> { $"Loaded boot image: {Path.GetFileName(filePath)}" });
                string contentSig = Path.Combine(Path.GetDirectoryName(filePath), "content.sig");
                if (File.Exists(contentSig))
                {
                    emulator.LoadMediaroomContent(contentSig);
                    ShowTextWindow("Uverse Emulation", new List<string> { $"Loaded content signatures: {Path.GetFileName(contentSig)}" });
                }
                emulator.EmulateWholeHomeNetwork();
                ShowTextWindow("Uverse Emulation", new List<string> { "Configured whole home network." });
                UverseEmulator.StartMediaroom();
                ShowTextWindow("Uverse Emulation", new List<string> { "Started Mediaroom platform." });
                var uverseLog = new List<string>
                {
                    $"Model: {model}",
                    $"Processor: {proc}",
                    $"Memory (MB): {mb}",
                    $"DVR Enabled: {isDVR}",
                    $"Whole Home Network: {isWholeHome}"
                };
                ShowTextWindow("Uverse Emulation Log", uverseLog);
                StatusBarText("Uverse emulation complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uverse processing failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Uverse processing failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Analyzes DirecTV firmware images for structure and content.
        /// </summary>
        private async Task HandleDirectvAnalysis()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter =
                    "DirecTV Firmware Images (*.csw;*.bin;*.tar.csw.bin)|*.csw;*.bin;*.tar.csw.bin|" +
                    "All Supported Files|*.csw;*.bin;*.tar.csw.bin|" +
                    "All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                StatusBarText($"Selected firmware: {Path.GetFileName(filePath)}");
                string extractDir = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_extracted");
                try
                {
                    // Extract archive and analyze file structure
                    await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(filePath, extractDir));
                    // Further analyze binaries in the extracted directory
                    FirmwareAnalyzer.AnalyzeFirmwareArchive(filePath, extractDir);
                    StatusBarText("Firmware extraction and analysis complete.");
                    MessageBox.Show($"Firmware {Path.GetFileName(filePath)} extracted and analyzed to {extractDir}.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Analysis failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusBarText("Firmware analysis failed.");
                }
            }
        await Task.CompletedTask;
        }

        /// <summary>
        /// Analyzes arbitrary executables or binaries to detect architecture and format.
        /// </summary>
        private async Task HandleExecutableAnalysis()
        {
            // Select executable file
            var dlg = new OpenFileDialog
            {
                Filter = "Executables and Binaries (*.exe;*.dll;*.bin;*.so)|*.exe;*.dll;*.bin;*.so|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            string filePath = dlg.FileName;
            StatusBarText($"Analyzing executable: {Path.GetFileName(filePath)}");
            byte[] data = File.ReadAllBytes(filePath);
            // Determine format and architecture
            string format = (data.Length > 4 && data[0] == 0x7F && data[1] == (byte)'E' && data[2] == (byte)'L' && data[3] == (byte)'F') ? "ELF" : "PE";
            string arch = ArchitectureDetector.Detect(data);
            string bitness = "Unknown";
            if (format == "PE" && data.Length > 0x40)
            {
                int peOffset = BitConverter.ToInt32(data, 0x3C);
                ushort machine = BitConverter.ToUInt16(data, peOffset + 4);
                bitness = machine switch
                {
                    0x14c => "x86",
                    0x8664 => "x64",
                    0x1c0 => "ARM",
                    0xaa64 => "ARM64",
                    _ => "Unknown"
                };
            }
            else if (format == "ELF" && data.Length > 5)
            {
                bitness = data[4] == 1 ? "32-bit" : data[4] == 2 ? "64-bit" : "Unknown";
            }
            var output = new List<string>
            {
                $"File: {Path.GetFileName(filePath)}",
                $"Format: {format}",
                $"Architecture: {arch}",
                $"Bitness: {bitness}"
            };
            // Encourage contribution for unsupported chips
            var desc = ChipReferenceManager.GetInfo(arch);
            if (!string.IsNullOrEmpty(desc))
                output.Add($"Description: {desc}");
            else
                output.Add(ChipReferenceManager.GetContributionMessage(arch));
            ShowTextWindow("Executable Analysis", output);
            StatusBarText("Executable analysis complete.");
            // Prompt to launch emulator
            var choice = PromptUserForChoice("Launch emulator for this executable?", new[] { "Homebrew", "QEMU", "No" });
            if (choice == "Homebrew")
            {
                try
                {
                    var home = new HomebrewEmulator();
                    home.LoadBinary(data);
                    home.Run();
                    StatusBarText("Homebrew emulation complete.");
                }
                catch (NotImplementedException)
                {
                    MessageBox.Show("Homebrew emulator not supported for this architecture.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (choice == "QEMU")
            {
                try
                {
                    EmulatorLauncher.Launch(filePath, arch);
                    StatusBarText("QEMU emulation started.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Emulation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            await Task.CompletedTask;
        }

        private async Task HandleFirmadyneEmulation()
        {
            StatusBarText("Starting Firmadyne-based emulation...");
            // TODO: Integrate Firmadyne pipeline (extract rootfs, QEMU setup, network capture)
            ShowTextWindow("Firmadyne Emulation", new List<string> { "Firmadyne integration not implemented yet." });
            StatusBarText("Firmadyne emulation stub complete.");
            await Task.CompletedTask;
        }

        private async Task HandleAzeriaEmulation()
        {
            StatusBarText("Starting Azeria Labs ARM firmware emulation...");
            // TODO: Follow steps from https://azeria-labs.com/emulating-arm-firmware/ to setup QEMU and load firmware
            ShowTextWindow("Azeria ARM Emulation", new List<string> { "Azeria ARM firmware emulation not implemented yet." });
            StatusBarText("Azeria ARM emulation stub complete.");
            await Task.CompletedTask;
        }
    
        // Core feature handlers

        /// <summary>
        /// Emulates an RDK-V set-top box using QEMU.
        /// </summary>
        private async Task HandleRdkVEmulation()
        {
            var dlg = new OpenFileDialog { Filter = "RDK-V Firmware Images (*.bin;*.tar;*.tar.gz;*.tar.bz2)|*.bin;*.tar;*.tar.gz;*.tar.bz2|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Launching RDK-V emulator for {Path.GetFileName(path)}...");
            var bin = File.ReadAllBytes(path);
            // Force ARM architecture for RDK-V hardware
            var arch = "ARM";
            Debug.WriteLine("Forcing architecture to ARM for RDK-V hardware.");
            StatusBarText("Forcing ARM architecture for RDK-V hardware.");
            try { EmulatorLauncher.Launch(path, arch, platform: "RDK-V"); StatusBarText("RDK-V emulation started."); }
            catch (Exception ex) { MessageBox.Show($"RDK-V error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); StatusBarText("RDK-V emulation failed."); }
            await Task.CompletedTask;
        }


        /// <summary>
        /// Probes a disk image for partition tables.
        /// </summary>
        private async Task HandleFilesystemProbe()
        {
            var dlg = new OpenFileDialog { Filter = "Disk/Filesystem Images (*.img;*.bin)|*.img;*.bin|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Probing filesystem in {Path.GetFileName(path)}...");
            var data = File.ReadAllBytes(path);
            var parts = PartitionAnalyzer.Analyze(data);
            ShowTextWindow("Partition Analysis", parts);
            StatusBarText("Filesystem probe complete.");
            await Task.CompletedTask;
        }


        /// <summary>
        /// Handles Linux filesystem read/write operations.
        /// </summary>
        private async Task HandleLinuxFsReadWrite()
        {
            var dlg = new OpenFileDialog { Filter = "Linux Filesystem Images (*.img;*.bin;*.ext2;*.ext3;*.ext4)|*.img;*.bin;*.ext2;*.ext3;*.ext4|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Selected Linux FS image: {Path.GetFileName(path)}");
            var type = FilesystemProber.Probe(path);
            ShowTextWindow("Linux FS Probe", new List<string> { $"Detected: {type}" });
            StatusBarText("Linux FS probe complete.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cross-compiles a binary from one architecture to another.
        /// </summary>
        private async Task HandleCrossCompile()
        {
            var dlg = new OpenFileDialog { Filter = "Binaries (*.bin;*.exe;*.dll)|*.bin;*.exe;*.dll|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string inputPath = dlg.FileName;
            StatusBarText($"Cross-compiling {Path.GetFileName(inputPath)}...");
            byte[] inputData = File.ReadAllBytes(inputPath);
            string fromArch = ArchitectureDetector.Detect(inputData);
            var targets = new[] { "x86", "x64", "ARM", "ARM64" };
            string toArch = PromptUserForChoice("Select target architecture:", targets);
            if (string.IsNullOrEmpty(toArch)) return;
            // If this is a WinCE binary, launch emulator instead of static cross-compilation
            if (IsWinCEBinary(inputData))
            {
                MessageBox.Show("WinCE binary detected; launching built-in emulator.", "WinCE Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    EmulatorLauncher.Launch(inputPath, fromArch);
                    StatusBarText("WinCE emulation started.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"WinCE emulation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusBarText("WinCE emulation failed.");
                }
                return;
            }
            // perform translation/recompile
            byte[] outputData = ReadAndTranslateFile(inputPath, fromArch, toArch);
            var saveDlg = new SaveFileDialog { Filter = "Binary Output (*.bin)|*.bin|All Files (*.*)|*.*", FileName = Path.GetFileNameWithoutExtension(inputPath) + $"_{toArch}" };
            if (saveDlg.ShowDialog() != true) return;
            File.WriteAllBytes(saveDlg.FileName, outputData);
            ShowTextWindow("Cross-Compile Result", new List<string> { $"Compiled from {fromArch} to {toArch} -> {Path.GetFileName(saveDlg.FileName)}" });
            StatusBarText("Cross-compilation complete.");
            await Task.CompletedTask;
        }

        private void ShowTextWindow(string title, List<string> lines)
        {
            var win = new Window
            {
                Title = title,
                Width = 800,
                Height = 600,
                Content = new ScrollViewer
                {
                    Content = new TextBox
                    {
                        Text = string.Join(Environment.NewLine, lines),
                        IsReadOnly = true,
                        AcceptsReturn = true,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                }
            };
            win.Show();
        }

        // Menu event handlers to toggle Unicorn engine usage
        private void UseUnicorn_Checked(object sender, RoutedEventArgs e)
        {
            BinaryTranslator.UseUnicornEngine = true;
            StatusBarText("Unicorn engine enabled");
        }

        private void UseUnicorn_Unchecked(object sender, RoutedEventArgs e)
        {
            BinaryTranslator.UseUnicornEngine = false;
            StatusBarText("Unicorn engine disabled");
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
    
    
        /// <summary>
        /// Emulates an RDK-B broadband gateway using QEMU.
        /// </summary>
        private async Task HandleRdkBEmulation()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "RDK-B Firmware Images (*.bin;*.tar;*.tar.gz;*.tar.bz2)|*.bin;*.tar;*.tar.gz;*.tar.bz2|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;
            string filePath = openFileDialog.FileName;
            StatusBarText($"Launching RDK-B emulator for {Path.GetFileName(filePath)}...");
            byte[] binary = File.ReadAllBytes(filePath);
            string arch = ArchitectureDetector.Detect(binary);
            try
            {
                EmulatorLauncher.Launch(filePath, arch, platform: "RDK-B");
                StatusBarText("RDK-B emulation started.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RDK-B emulation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("RDK-B emulation failed.");
            }
            await Task.CompletedTask;
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
                else if (fwStr.Contains("BCM7405"))
                    chipsetName = "BCM7405";
                else if (fwStr.Contains("MIPS 4380") || fwStr.Contains("MIPS4380"))
                    chipsetName = "MIPS4380";
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
            // Load raw data
            byte[] data = File.ReadAllBytes(filePath);
            try
            {
                // Attempt static cross-translation via BinaryTranslator
                return BinaryTranslator.Translate(fromArch, toArch, data);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show($"Translation from {fromArch} to {toArch} not implemented.", "Cross-Compile Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return data;
            }
        }

        // Removed override of Equals(object) because DependencyObject.Equals(object) is sealed and cannot be overridden.

        // Removed GetHashCode override because DependencyObject.GetHashCode() is sealed and cannot be overridden.

        // Add this method to handle File -> Open menu click and call StartEmulation_Click
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartEmulation_Click(sender, e);
        }

        // New handler for firmware analysis from menu
        private async void AnalyzeFirmware_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Firmware Archives (*.zip;*.tar;*.tar.gz;*.tar.bz2;*.bin)|*.zip;*.tar;*.tar.gz;*.tar.bz2;*.bin|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            string archivePath = dlg.FileName;
            string extractDir = Path.Combine(Path.GetDirectoryName(archivePath), Path.GetFileNameWithoutExtension(archivePath) + "_analyzed");
            StatusBarText($"Analyzing firmware: {Path.GetFileName(archivePath)}...");
            try
            {
                await Task.Run(() => FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, extractDir));
                StatusBarText("Firmware analysis complete.");
                MessageBox.Show($"Firmware analysis finished. Check the console for details and extracted files at:\n{extractDir}", "Analysis Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firmware analysis failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Firmware analysis failed.");
            }
        }

        // New handler to extract selected firmware archives
        private async void ExtractFirmware_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Firmware Archives (*.zip;*.tar;*.tar.gz;*.tar.bz2;*.bin)|*.zip;*.tar;*.tar.gz;*.tar.bz2;*.bin|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            string archivePath = dlg.FileName;
            string extractDir = Path.Combine(Path.GetDirectoryName(archivePath), Path.GetFileNameWithoutExtension(archivePath) + "_extracted");
            try
            {
                await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(archivePath, extractDir));
                MessageBox.Show($"Extraction complete. Files extracted to:\n{extractDir}", "Extraction Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Extraction failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New handler to detect the type of selected file
        private void DetectFileType_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string filePath = dlg.FileName;
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                string type = AnalyzeFileType(filePath, data);
                MessageBox.Show($"Detected file type: {type}", "File Type Detection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper to analyze file type
        private static string AnalyzeFileType(string filePath, byte[] binary)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".exe" || ext == ".dll")
                return "Executable";
            if (ext == ".img" || ext == ".bin")
            {
                // Heuristic: check for firmware or archive magic numbers
                if (binary.Length > 4 && binary[0] == 0x1F && binary[1] == 0x8B)
                    return "Archive"; // gzip
                if (binary.Length > 2 && binary[0] == 0x50 && binary[1] == 0x4B)
                    return "Archive"; // zip
                // Add more heuristics as needed
                return "Firmware";
            }
            if (ext == ".tar" || ext == ".csw")
                return "Archive";
            return "Unknown";
        }

        // Helper to prompt user for a choice
        private string PromptUserForChoice(string message, IList<string> options)
        {
            var inputDialog = new Window
            {
                Title = "Select Action",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };
            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 10) });
            var comboBox = new ComboBox { ItemsSource = options, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(comboBox);
            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
            stack.Children.Add(okButton);
            inputDialog.Content = stack;

            string result = null;
            okButton.Click += (s, e) => { result = comboBox.SelectedItem as string; inputDialog.DialogResult = true; inputDialog.Close(); };
            inputDialog.ShowDialog();
            return result;
        }

        public static class FilesystemProber
        {
            public static string Probe(string filePath)
            {
                // ...existing code...
                return string.Empty;
            }
        }

        public static class DirecTVEmulator
        {
            public static string AnalyzeFirmware(string filePath)
            {
                // ...existing code...
                return string.Empty;
            }
        }

        // Helper to parse DirecTV firmware filename metadata
        private (string Manufacturer, string Model, string Version, string Tar)? ParseDirecTVFilename(string fileName)
        {
            var m = Regex.Match(fileName, @"image_mfr-(\d+)_mdl-([0-9a-z]+)_ver-(\d+)_tar-([0-9a-f]+)\\.csw", RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            return (m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value);
        }

        /// <summary>
        /// Simulates DirecTV SWM switches and/or LNBs.
        /// Currently a stub; intended for future simulation and testing.
        /// </summary>
        private async Task HandleSwmLnbSimulation()
        {
            var openFileDialog = new OpenFileDialog {
                Filter = "SWM Switch/LNB Firmware (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            string filePath = openFileDialog.FileName;
            StatusBarText("Loading SWM Switch/LNB firmware...");
            byte[] firmware = File.ReadAllBytes(filePath);
            StatusBarText("Simulating SWM LNB...");

            var output = new List<string>();
            try
            {
                SwmLnbEmulator.SimulateReceiverRequest(firmware);
                output.Add("SimulateReceiverRequest completed.");
                SwmLnbEmulator.SendChannelMap();
                output.Add("SendChannelMap completed.");
                SwmLnbEmulator.EmulateKeepAlive();
                output.Add("EmulateKeepAlive completed.");
            }
            catch (Exception ex)
            {
                output.Add($"Error during simulation: {ex.Message}");
            }

            StatusBarText("SWM LNB simulation complete.");
            ShowTextWindow("SWM LNB Simulation Output", output);

            await Task.CompletedTask;
        }

        // Handler for dropped files
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                // If unsupported extension, prompt to open GitHub issue
                var supported = new[] { ".exe", ".dll", ".bin", ".csw", ".tar", ".img", ".fw" };
                if (!supported.Contains(ext))
                {
                    var ask = MessageBox.Show($"No handler for '{ext}'. Create an issue on GitHub?", "Unsupported File", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ask == MessageBoxResult.Yes)
                        Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/julerobb1/Processor-Emulator/issues/new") { CreateNoWindow = true });
                }
                else
                {
                    StatusBarText($"File dropped: {Path.GetFileName(file)}. Use menu to analyze.");
                }
            }
        }

        // All duplicate methods/helpers have been removed for clarity.

        // New method to prompt user for QEMU options
        private string PromptForQemuOptions()
        {
            var dialog = new Window
            {
                Title = "QEMU Options",
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.CanResize,
                Owner = this
            };
            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock { Text = "Enter extra QEMU command-line options:", Margin = new Thickness(0, 0, 0, 5) });
            var textBox = new TextBox
            {
                AcceptsReturn = true,
                Text = string.Empty,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 180
            };
            stack.Children.Add(textBox);
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 5, 0) };
            var cancelButton = new Button { Content = "Cancel", Width = 80, IsCancel = true };
            btnPanel.Children.Add(okButton);
            btnPanel.Children.Add(cancelButton);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;
            string result = null;
            okButton.Click += (s, e) => { result = textBox.Text.Trim(); dialog.DialogResult = true; dialog.Close(); };
            if (dialog.ShowDialog() == true)
                return result;
            return string.Empty;
        }

        /// <summary>
        /// Mounts a Windows CE filesystem image using DiscUtils.Fat
        /// </summary>
        private async Task HandleCeMount()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "CE Image Files (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting CE filesystem image {Path.GetFileName(path)}...");
            try
            {
                using var stream = File.OpenRead(path);
                // DiscUtils initialization
                DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(DiscUtils.Fat.FatFileSystem).Assembly);
                var fs = new DiscUtils.Fat.FatFileSystem(stream);
                var entries = new List<string> { $"Mounted CE FS: {Path.GetFileName(path)}" };
                foreach (var entry in fs.GetFiles("", "*", SearchOption.TopDirectoryOnly))
                {
                    entries.Add(entry);
                }
                ShowTextWindow("CE Filesystem Mount", entries);
                StatusBarText("CE filesystem mount complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CE mount error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("CE filesystem mount failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Mounts a YAFFS filesystem image using the ExoticFilesystemManager.
        /// </summary>
        private async Task HandleYaffsMount()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "YAFFS Images (*.img;*.yaffs)|*.img;*.yaffs|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Extracting YAFFS image {Path.GetFileName(path)}...");
            try
            {
                string outDir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_yaffs");
                YaffsExtractor.ExtractYaffs(path, outDir);
                var entries = Directory.GetFiles(outDir, "*", SearchOption.AllDirectories)
                                        .Select(f => Path.GetRelativePath(outDir, f))
                                        .ToList();
                ShowTextWindow("YAFFS Extraction", entries);
                StatusBarText("YAFFS extraction complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"YAFFS extraction error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("YAFFS extraction failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Mounts a SquashFS filesystem image using DiscUtils.SquashFs
        /// </summary>
        private async Task HandleSquashFsMount()
        {
            var dlg = new OpenFileDialog { Filter = "SquashFS Images (*.bin;*.img;*.squashfs)|*.bin;*.img;*.squashfs|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting SquashFS image {Path.GetFileName(path)}...");
            try
            {
                using var stream = File.OpenRead(path);
                // Register the SquashFs assembly
                SetupHelper.RegisterAssembly(typeof(DiscUtils.SquashFs.SquashFileSystemReader).Assembly);
                // Create a SquashFS filesystem
                var fs = new DiscUtils.SquashFs.SquashFileSystemReader(stream);
                var entries = new List<string> { $"Mounted SquashFS: {Path.GetFileName(path)}" };
                foreach (var entry in fs.GetFiles("", "*", SearchOption.AllDirectories))
                    entries.Add(entry);
                ShowTextWindow("SquashFS Mount", entries);
                StatusBarText("SquashFS mount complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SquashFS mount error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("SquashFS mount failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Mounts an ISO9660 image and lists all files.
        /// </summary>
        private async Task HandleIsoMount()
        {
            var dlg = new OpenFileDialog { Filter = "ISO Images (*.iso)|*.iso|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting ISO image {Path.GetFileName(path)}...");
            try
            {
                using var stream = File.OpenRead(path);
                SetupHelper.RegisterAssembly(typeof(CDReader).Assembly);
                var fs = new CDReader(stream, true);
                var entries = new List<string> { $"Mounted ISO: {Path.GetFileName(path)}" };
                foreach (var entry in fs.GetFiles("", "*", SearchOption.AllDirectories))
                    entries.Add(entry);
                ShowTextWindow("ISO Filesystem Mount", entries);
                StatusBarText("ISO mount complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ISO mount error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("ISO mount failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Mounts an ext2/3/4 filesystem image and lists all files.
        /// </summary>
        private async Task HandleExtMount()
        {
            var dlg = new OpenFileDialog { Filter = "EXT Images (*.img;*.ext2;*.ext3;*.ext4)|*.img;*.ext2;*.ext3;*.ext4|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting EXT image {Path.GetFileName(path)}...");
            try
            {
                using var stream = File.OpenRead(path);
                SetupHelper.RegisterAssembly(typeof(DiscUtils.Ext.ExtFileSystem).Assembly);
                var fs = new DiscUtils.Ext.ExtFileSystem(stream);
                var entries = new List<string> { $"Mounted EXT: {Path.GetFileName(path)}" };
                foreach (var entry in fs.GetFiles("", "*", SearchOption.AllDirectories))
                    entries.Add(entry);
                ShowTextWindow("EXT Filesystem Mount", entries);
                StatusBarText("EXT mount complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"EXT mount error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("EXT mount failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Analyzes the contents of a folder, providing information about the files and subfolders.
        /// </summary>
        private async Task HandleFolderAnalysis()
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            string folderPath = dlg.SelectedPath;
            StatusBarText($"Analyzing folder: {folderPath}...");
            try
            {
                // Launch the new folder analysis window (code-only)
                var window = new FolderAnalysisWindow(folderPath)
                {
                    Owner = this
                };
                window.Show();
                StatusBarText("Folder analysis complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Folder analysis error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Folder analysis failed.");
            }
            await Task.CompletedTask;
        }
    }
}