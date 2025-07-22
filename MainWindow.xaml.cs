using ProcessorEmulator.Emulation;
using ProcessorEmulator.Tools;
using ProcessorEmulator.Network;
using ProcessorEmulator; // Add this if PartitionAnalyzer is in the root namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
// YAFFS handled by ExoticFilesystemManager
using DiscUtils.Setup;
using static ProcessorEmulator.Tools.ArchitectureDetector;
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

        // Store selected firmware path and platform
        private string firmwarePath;
        private string selectedPlatform;

        // Add default constructor for XAML
        public MainWindow()
        {
            InitializeComponent();
            // Load XAML UI components
            // Initialize drag-and-drop for file support
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            // Initialize real-time emulation log panel
            InitializeLogPanel();
        }

        public MainWindow(IEmulator currentEmulator)
        {
            InitializeComponent();
            this.currentEmulator = currentEmulator;
            InitializeLogPanel();
        }

        /// <summary>
        /// Initialize the real-time emulation log panel
        /// </summary>
        private void InitializeLogPanel()
        {
            try
            {
                logPanel = new EmulationLogPanel();

                // TODO: Find the log panel container in XAML and add our log panel
                // if (LogPanelContainer != null)
                // {
                //     LogPanelContainer.Child = logPanel;
                // }

                Debug.WriteLine("[MainWindow] Log panel initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to initialize log panel: {ex.Message}");
            }
        }

        // All Tools classes are static - no need to instantiate
        private ExoticFilesystemManager fsManager = new();
        private InstructionDispatcher dispatcher = new();

        // Real-time emulation logging
        private EmulationLogPanel logPanel;

        public TextBlock StatusBar { get; set; } = new TextBlock();
        public PartitionAnalyzer PartitionAnalyzer { get; set; } = null; // Static class, no instantiation needed
        public InstructionDispatcher Dispatcher1 { get => dispatcher; set => dispatcher = value; }
        PartitionAnalyzer IMainWindow.PartitionAnalyzer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                "PowerPC Bootloader Demo",
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
                "Simulate SWM LNB",
                "Boot Firmware (Homebrew First)",
                "Boot Firmware in Homebrew Emulator",
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
                case "PowerPC Bootloader Demo":
                    await HandlePowerPCBootloaderDemo();
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
                case "Simulate SWM LNB":
                    await HandleSwmLnbSimulation();
                    break;
                case "Boot Firmware (Homebrew First)":
                    await HandleBootFirmwareHomebrewFirst();
                    break;
                case "Boot Firmware in Homebrew Emulator":
                    await HandleBootFirmwareInHomebrew();
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
        /// Emulates an RDK-V set-top box using built-in emulator with display.
        /// </summary>
        private async Task HandleRdkVEmulation()
        {
            if (string.IsNullOrEmpty(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string path = firmwarePath;
            StatusBarText($"Loading RDK-V firmware: {System.IO.Path.GetFileName(path)}...");

            try
            {
                var bin = System.IO.File.ReadAllBytes(path);
                Debug.WriteLine($"Loaded RDK-V firmware: {bin.Length} bytes from {path}");
                StatusBarText($"Booting RDK-V firmware ({bin.Length:N0} bytes)...");

                // Just load and boot the firmware - NO POPUPS!
                var emulator = new HomebrewEmulator();
                emulator.LoadBinary(bin);
                emulator.Run(); // This will boot the actual firmware

                StatusBarText("RDK-V firmware boot initiated.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RDK-V firmware boot error:\n\n{ex.Message}", "Boot Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("RDK-V firmware boot failed.");
            }
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

        /// <summary>
        /// Demonstrates PowerPC bootloader functionality and emulation.
        /// </summary>
        private async Task HandlePowerPCBootloaderDemo()
        {
            var choice = PromptUserForChoice("PowerPC Bootloader Demo",
                new List<string> { "Create Bootloader Only", "Load Firmware + Bootloader", "Show Bootloader Info" });

            if (string.IsNullOrEmpty(choice)) return;

            try
            {
                switch (choice)
                {
                    case "Create Bootloader Only":
                        StatusBarText("Creating PowerPC bootloader...");
                        PowerPCBootloaderManager.LaunchPowerPCWithBootloader(null);
                        StatusBarText("PowerPC bootloader demo started.");
                        break;

                    case "Load Firmware + Bootloader":
                        var dlg = new OpenFileDialog
                        {
                            Filter = "PowerPC Firmware (*.bin;*.img;*.elf)|*.bin;*.img;*.elf|All Files (*.*)|*.*"
                        };
                        if (dlg.ShowDialog() == true)
                        {
                            StatusBarText($"Loading PowerPC firmware: {Path.GetFileName(dlg.FileName)}...");
                            PowerPCBootloaderManager.LaunchPowerPCWithBootloader(dlg.FileName);
                            StatusBarText("PowerPC emulation with firmware started.");
                        }
                        break;

                    case "Show Bootloader Info":
                        PowerPCBootloaderManager.ShowBootloaderInfo();
                        StatusBarText("Displayed PowerPC bootloader information.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PowerPC bootloader error: {ex.Message}", "PowerPC Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("PowerPC bootloader demo failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Configure emulator settings based on platform detection results
        /// </summary>
        private void ConfigureEmulatorFromDetection(PlatformSignature platform)
        {
            try
            {
                // Set emulator type radio button based on detection (commented out due to XAML binding issues)
                // TODO: Fix XAML control binding issues
                /*
                switch (platform.EmulatorType)
                {
                    case EmulatorType.HomebrewEmulator:
                        if (HomebrewEmulatorRadio != null)
                            HomebrewEmulatorRadio.IsChecked = true;
                        break;
                    case EmulatorType.QEMU:
                        if (QemuEmulatorRadio != null)
                            QemuEmulatorRadio.IsChecked = true;
                        break;
                    // Note: RetDecTranslatorRadio may not be accessible from code-behind
                }
                */

                // Set platform-specific configurations
                switch (platform.Name)
                {
                    case "RDK-V":
                        // Auto-select ARM architecture for RDK-V
                        StatusBarText("Configured for RDK-V: ARM Cortex-A15, BCM7449 SoC");
                        break;
                    case "U-verse":
                        // Auto-select MIPS for U-verse
                        StatusBarText("Configured for U-verse: MIPS architecture, IPTV platform");
                        break;
                    case "DirecTV":
                        // Auto-select MIPS for DirecTV
                        StatusBarText("Configured for DirecTV: MIPS architecture, Satellite platform");
                        break;
                    case "Windows CE":
                        // Auto-select ARM for WinCE
                        StatusBarText("Configured for Windows CE: ARM architecture");
                        break;
                    case "VxWorks":
                        StatusBarText("Configured for VxWorks: RTOS environment");
                        break;
                    case "Embedded Linux":
                        StatusBarText("Configured for Embedded Linux: Generic ARM platform");
                        break;
                }

                Debug.WriteLine($"[MainWindow] Auto-configured for platform: {platform.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Configuration error: {ex.Message}");
                StatusBarText($"Auto-configuration failed: {ex.Message}");
            }
        }

        private void LoadFirmwareImage(string imagePath, string signaturePath)
        {
            // Copy firmware image to temp folder to avoid modifying originals
            string tempDir = Path.Combine(Path.GetTempPath(), "ProcessorEmulator", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string tempImagePath = Path.Combine(tempDir, Path.GetFileName(imagePath));
            File.Copy(imagePath, tempImagePath, overwrite: true);

            // 🧠 PLATFORM AUTODETECTION - Analyze firmware to suggest platform
            StatusBarText("Analyzing firmware for platform detection...");
            var detectionResult = PlatformDetector.DetectPlatform(imagePath);

            // 🗂 REGION AWARENESS - Analyze firmware regions for boot logic
            StatusBarText("Analyzing firmware regions...");
            var regionResult = FirmwareRegionAnalyzer.AnalyzeFirmware(imagePath);

            if (detectionResult.Success && detectionResult.DetectedPlatform != null)
            {
                var platform = detectionResult.DetectedPlatform;
                StatusBarText($"Platform detected: {platform.Name} (confidence: {detectionResult.Confidence:P1})");

                // Show detection results and recommendations
                var resultMessage = $"🎯 Platform Detection & Region Analysis Results:\n\n";
                resultMessage += $"Platform: {platform.Name}\n";
                resultMessage += $"Confidence: {detectionResult.Confidence:P1}\n";
                resultMessage += $"Architecture: {platform.Architecture}\n";
                resultMessage += $"SoC Family: {platform.SocFamily}\n";
                resultMessage += $"Recommended Emulator: {platform.EmulatorType}\n\n";

                // Add region analysis results
                if (regionResult.Success && regionResult.DetectedRegions.Any())
                {
                    resultMessage += "�️ Detected Firmware Regions:\n";
                    foreach (var region in regionResult.DetectedRegions.Take(4))
                    {
                        resultMessage += $"• {region.Name}: {region.Confidence:P1} confidence\n";
                        resultMessage += $"  Address: 0x{region.LoadAddress:X8}, Size: ~{region.EstimatedSize / 1024}KB\n";
                    }
                    resultMessage += "\n";
                }

                if (detectionResult.Recommendations.Any())
                {
                    resultMessage += "� Platform Recommendations:\n";
                    foreach (var rec in detectionResult.Recommendations.Take(3))
                        resultMessage += $"• {rec}\n";
                    resultMessage += "\n";
                }

                // Add boot sequence recommendations
                if (regionResult.Success && regionResult.BootSequence.Any())
                {
                    resultMessage += "🚀 Recommended Boot Sequence:\n";
                    foreach (var step in regionResult.BootSequence.Take(6))
                        resultMessage += $"{step}\n";
                }

                MessageBox.Show(resultMessage, "Platform & Region Analysis Results",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                // Auto-configure emulator type based on detection
                ConfigureEmulatorFromDetection(platform);

                // Log platform and region information to emulation log
                if (logPanel != null)
                {
                    logPanel.LogPeripheralTrap("ANALYZER", "Platform Detection",
                        $"Detected {platform.Name} with {detectionResult.Confidence:P1} confidence");

                    if (regionResult.Success)
                    {
                        logPanel.LogPeripheralTrap("ANALYZER", "Region Analysis",
                            $"Found {regionResult.DetectedRegions.Count} firmware regions");
                    }
                }
            }
            else
            {
                StatusBarText("Platform detection failed - proceeding with manual configuration");
                if (!string.IsNullOrEmpty(detectionResult.Error))
                {
                    MessageBox.Show($"Platform detection failed: {detectionResult.Error}\n\nProceeding with manual configuration.",
                                   "Platform Detection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

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
        /// Simulates a DirecTV SWM LNB with default band settings.
        /// </summary>
        private async Task HandleSwmLnbSimulation()
        {
            // Default 5-band frequencies in MHz
            var bands = new Dictionary<int, int>
            {
                {1, 1150}, {2, 1250}, {3, 1350}, {4, 1450}, {5, 1550}
            };
            var emulator = new ProcessorEmulator.Tools.SwmLnbEmulator();
            bool ok = emulator.Initialize(bands.Count, bands, null);
            if (!ok)
            {
                MessageBox.Show("Failed to initialize SWM LNB emulator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Show initial state
            var info = new List<string>
            {
                $"SWM LNB initialized with {bands.Count} bands",
                $"Current IF: {emulator.GetCurrentIf()} MHz"
            };
            ShowTextWindow("SWM LNB Emulator", info);
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
                // Create file records from folder
                var fileRecords = new List<ProcessorEmulator.FileRecord>();
                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var info = new FileInfo(file);
                        byte[] preview = new byte[Math.Min(16, info.Length)];
                        if (info.Length > 0)
                        {
                            using (var fs = File.OpenRead(file))
                                fs.Read(preview, 0, preview.Length);
                        }
                        fileRecords.Add(new ProcessorEmulator.FileRecord
                        {
                            FilePath = file,
                            Size = info.Length,
                            HexPreview = BitConverter.ToString(preview).Replace("-", " ")
                        });
                    }
                }

                // Launch the XAML folder analysis window 
                var window = new FolderAnalysisWindow(fileRecords)
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

        /// <summary>
        /// Boots a firmware image in QEMU for rapid hardware testing.
        /// </summary>
        /// <summary>
        /// Boot firmware with HomebrewEmulator first, fallback to QEMU if needed.
        /// </summary>
        private async Task HandleBootFirmwareHomebrewFirst()
        {
            var dlg = new OpenFileDialog { Filter = "Firmware Images (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;

            try
            {
                byte[] binary = File.ReadAllBytes(path);
                string detectedArch = Tools.ArchitectureDetector.Detect(binary);

                // Prompt for architecture if unknown or to override detection
                string arch = PromptUserForChoice($"Detected: {detectedArch}. Select CPU Architecture:",
                    new List<string> { "MIPS32", "MIPS64", "ARM", "ARM64", "PowerPC", "x86", "x86-64", "RISC-V" });
                if (string.IsNullOrEmpty(arch)) return;

                StatusBarText($"Launching emulation for {Path.GetFileName(path)} ({arch})...");

                try
                {
                    // First attempt: HomebrewEmulator
                    var emulator = new HomebrewEmulator();
                    emulator.LoadBinary(binary);
                    emulator.Run();
                    StatusBarText($"HomebrewEmulator started for {Path.GetFileName(path)} ({arch})");
                }
                catch (Exception homebrewEx)
                {
                    Debug.WriteLine($"HomebrewEmulator failed: {homebrewEx.Message}");
                    StatusBarText("HomebrewEmulator failed, falling back to QEMU...");

                    // Fallback: QEMU
                    try
                    {
                        // Special handling for PowerPC - use bootloader manager
                        if (arch.Equals("PowerPC", StringComparison.OrdinalIgnoreCase))
                        {
                            PowerPCBootloaderManager.LaunchPowerPCWithBootloader(path);
                            StatusBarText($"Launched PowerPC emulation with bootloader for {Path.GetFileName(path)}");
                        }
                        else
                        {
                            QemuManager.Launch(path, arch);
                            StatusBarText($"Launched QEMU for {Path.GetFileName(path)} ({arch})");
                        }
                    }
                    catch (Exception qemuEx)
                    {
                        throw new Exception($"Both emulators failed:\n\nHomebrewEmulator: {homebrewEx.Message}\n\nQEMU: {qemuEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Emulator launch error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Emulation failed.");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Boots a firmware image using the homegrown emulator loop.
        /// </summary>
        private async Task HandleBootFirmwareInHomebrew()
        {
            var dlg = new OpenFileDialog { Filter = "Firmware Images (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            try
            {
                byte[] data = File.ReadAllBytes(path);
                // Load into instruction dispatcher memory region starting at 0
                dispatcher.LoadBinary(data); // Use a method to load binary data
                dispatcher.PC = 0;
                dispatcher.Start(); // begins emulation loop
                StatusBarText($"Homebrew emulation started for {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Homebrew emulation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Homebrew emulation failed.");
            }
            await Task.CompletedTask;
        }

        private void ScanDvrData_Click(object sender, RoutedEventArgs e)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR");
            if (!Directory.Exists(baseDir))
            {
                ShowTextWindow("DVR Scan", new List<string> { "Data\\DVR directory not found." });
                return;
            }
            var summary = new List<string>();
            foreach (var dir in Directory.GetDirectories(baseDir))
            {
                var name = Path.GetFileName(dir);
                summary.Add($"Dataset: {name}");
                var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                int firmwareCount = files.Count(f => new[] { ".csw", ".bin", ".pkgstream", ".gz", ".tar.gz" }
                                            .Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
                int logCount = files.Count(f => new[] { ".log", ".log.*", ".dump" }
                                    .Any(ext => f.IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0));
                int rawCount = files.Count(f => f.EndsWith(".raw", StringComparison.OrdinalIgnoreCase));
                summary.Add($"  Firmware files: {firmwareCount}");
                summary.Add($"  Log files: {logCount}");
                summary.Add($"  Raw partitions: {rawCount}");
                summary.Add(string.Empty);
            }
            ShowTextWindow("DVR Data Scan", summary);
        }

        private void ListDvrFirmware_Click(object sender, RoutedEventArgs e)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR");
            if (!Directory.Exists(baseDir))
            {
                ShowTextWindow("DVR Firmware List", new List<string> { "Data\\DVR directory not found." });
                return;
            }
            var result = new List<string>();
            var firmwareExts = new[] { ".csw", ".bin", ".pkgstream", ".gz", ".tar.gz" };
            foreach (var dir in Directory.GetDirectories(baseDir))
            {
                string dataset = Path.GetFileName(dir);
                result.Add($"Dataset: {dataset}");
                var allFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                var fwFiles = allFiles.Where(f => firmwareExts.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
                if (fwFiles.Count == 0)
                {
                    result.Add("  (no firmware files found)");
                }
                else
                {
                    foreach (var file in fwFiles)
                        result.Add("  " + file.Substring(baseDir.Length + 1));
                }
                result.Add(string.Empty);
            }
            ShowTextWindow("DVR Firmware List", result);
        }

        private void ProbeDvrXfs_Click(object sender, RoutedEventArgs e)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR");
            if (!Directory.Exists(baseDir))
            {
                ShowTextWindow("DVR XFS Probe", new List<string> { "Data\\DVR directory not found." });
                return;
            }
            var lines = new List<string>();
            var xfsDirs = Directory.GetDirectories(baseDir, "XFS", SearchOption.AllDirectories);
            if (xfsDirs.Length == 0)
            {
                lines.Add("No XFS directories found in DVR datasets.");
            }
            foreach (var xfs in xfsDirs)
            {
                string parent = Path.GetDirectoryName(xfs);
                string dataset = Path.GetFileName(parent);
                lines.Add($"Dataset: {dataset} - XFS at {xfs}");
                var subDirs = Directory.GetDirectories(xfs);
                lines.Add("  Subdirectories:");
                foreach (var d in subDirs)
                    lines.Add("    " + Path.GetFileName(d));
                int fileCount = Directory.GetFiles(xfs, "*.*", SearchOption.AllDirectories).Length;
                lines.Add($"  Total files: {fileCount}");
                lines.Add(string.Empty);
            }
            ShowTextWindow("DVR XFS Probe", lines);
        }

        private void AnalyzeAllDvrData_Click(object sender, RoutedEventArgs e)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR");
            if (!Directory.Exists(baseDir))
            {
                ShowTextWindow("DVR Full Analysis", new List<string> { "Data\\DVR directory not found." });
                return;
            }
            var report = DvrDataAnalyzer.AnalyzeAll(baseDir);
            ShowTextWindow("DVR Full Analysis", report);
        }

        private void BrowseFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Firmware Images|*.bin;*.img;*.exe;*.fw;*.csw;*.pkgstream|All Files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                // Store firmware path for later use
                firmwarePath = dlg.FileName;
                
                // Update UI to show selected file
                try
                {
                    FirmwarePathTextBox.Text = dlg.FileName;
                }
                catch
                {
                    // Fallback if TextBox not accessible
                    StatusBarText($"Firmware selected: {System.IO.Path.GetFileName(dlg.FileName)}");
                }
                
                StatusBarText($"Firmware loaded: {System.IO.Path.GetFileName(dlg.FileName)}");
            }
        }

        private async void StartEmulationButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if firmware is selected
            if (string.IsNullOrEmpty(firmwarePath) || !File.Exists(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first using the Browse button.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Determine selected platform
            selectedPlatform = "Generic"; // Default
            try
            {
                if (RdkVPlatformRadio.IsChecked == true)
                    selectedPlatform = "RDK-V";
                else if (GenericPlatformRadio.IsChecked == true)
                    selectedPlatform = "Generic";
            }
            catch
            {
                // Fallback if radio buttons not accessible
                selectedPlatform = "RDK-V"; // Assume RDK-V since that's the main focus
            }

            // Determine selected emulator type
            string selectedEmulator = "HomebrewEmulator"; // Default
            try
            {
                if (HomebrewEmulatorRadio.IsChecked == true)
                    selectedEmulator = "HomebrewEmulator";
                else if (QemuEmulatorRadio.IsChecked == true)
                    selectedEmulator = "QEMU";
                else if (RetDecTranslatorRadio.IsChecked == true)
                    selectedEmulator = "RetDec";
            }
            catch
            {
                selectedEmulator = "HomebrewEmulator"; // Fallback
            }

            StatusBarText($"Starting {selectedPlatform} emulation using {selectedEmulator} for {System.IO.Path.GetFileName(firmwarePath)}...");

            // Route to appropriate emulation handler based on emulator choice
            if (selectedEmulator == "QEMU")
            {
                await HandleQemuEmulation();
            }
            else if (selectedEmulator == "RetDec")
            {
                await HandleRetDecEmulation();
            }
            else
            {
                // HomebrewEmulator - route based on platform
                switch (selectedPlatform)
                {
                    case "RDK-V":
                        await HandleRdkVEmulation();
                        break;
                    default:
                        await HandleGenericEmulation();
                        break;
                }
            }
        }

        private async Task HandleGenericEmulation()
        {
            try
            {
                StatusBarText("Starting generic firmware emulation...");
                
                var binary = File.ReadAllBytes(firmwarePath);
                var arch = Tools.ArchitectureDetector.Detect(binary);
                
                StatusBarText("Starting generic emulation...");
                
                // Use HomebrewEmulator for generic emulation too
                var emulator = new HomebrewEmulator();
                emulator.LoadBinary(binary);
                emulator.Run();
                
                StatusBarText("Generic emulation started successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Generic emulation error:\n\n{ex.Message}", "Emulation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Generic emulation failed.");
            }
            
            await Task.CompletedTask;
        }

        private async Task HandleQemuEmulation()
        {
            try
            {
                StatusBarText("Starting QEMU emulation...");
                
                var binary = File.ReadAllBytes(firmwarePath);
                var arch = Tools.ArchitectureDetector.Detect(binary);
                
                StatusBarText($"Detected architecture: {arch}, launching QEMU...");
                
                // Use EmulatorLauncher to start QEMU
                EmulatorLauncher.Launch(firmwarePath, arch);
                
                StatusBarText("QEMU emulation started successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QEMU emulation error:\n\n{ex.Message}", "QEMU Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("QEMU emulation failed.");
            }
            
            await Task.CompletedTask;
        }

        private async Task HandleRetDecEmulation()
        {
            try
            {
                StatusBarText("Starting RetDec binary translation...");
                
                MessageBox.Show("RetDec binary translator is not yet implemented.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusBarText("RetDec translation cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RetDec error:\n\n{ex.Message}", "RetDec Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("RetDec translation failed.");
            }
            
            await Task.CompletedTask;
        }

        // Helper methods for UI control access with fallbacks
        private string GetSelectedEmulatorType()
        {
            try
            {
                if (HomebrewEmulatorRadio?.IsChecked == true) return "HomebrewEmulator";
                if (QemuEmulatorRadio?.IsChecked == true) return "QEMU";
                if (RetDecTranslatorRadio?.IsChecked == true) return "RetDec";
            }
            catch
            {
                // Fallback if UI controls not accessible
            }
            return "HomebrewEmulator"; // Default
        }

        private string GetSelectedPlatformType()
        {
            try
            {
                if (RdkVPlatformRadio?.IsChecked == true) return "RDK-V";
                if (GenericPlatformRadio?.IsChecked == true) return "Generic";
            }
            catch
            {
                // Fallback if UI controls not accessible
            }
            return "Generic"; // Default
        }

        private void HandleRetDecTranslation(string imagePath, byte[] bytes, string arch)
        {
            var result = MessageBox.Show($"Use RetDec to translate {arch} firmware to x86?\n\nThis will decompile and translate the binary for analysis.",
                                       "RetDec Translation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var translated = Tools.BinaryTranslator.Translate(arch, "x86", bytes);
                    if (translated != null && translated.Length > 0)
                    {
                        string outputPath = Path.ChangeExtension(imagePath, ".translated.bin");
                        File.WriteAllBytes(outputPath, translated);
                        MessageBox.Show($"Translation completed!\nOutput saved to: {outputPath}",
                                      "RetDec Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("RetDec translation failed - no output generated.",
                                      "RetDec Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"RetDec translation error: {ex.Message}",
                                  "RetDec Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SummarizeDvrData_Click(object sender, RoutedEventArgs e)
        {
            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR");
            var subs = new[] { "ATT_Firmware", "Uverse_Stuff", "Dish_Stuff", "Directv_Stuff" };
            var output = new List<string>();
            foreach (var sub in subs)
            {
                var path = Path.Combine(baseDir, sub);
                output.Add($"===== {sub} =====");
                if (Directory.Exists(path))
                {
                    var groups = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                        .GroupBy(p => Path.GetExtension(p).ToLowerInvariant())
                        .OrderByDescending(g => g.Count());
                    foreach (var g in groups)
                        output.Add($"{g.Count()} {g.Key}");
                }
                else
                {
                    output.Add($"Folder not found: {path}");
                }
                output.Add(string.Empty);
            }
            ShowTextWindow("DVR Data Summary", output);
        }

        // Filesystem mounting event handlers
        private void MountFat_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FAT filesystem mounting not yet implemented.", "Mount FAT");
        }

        private void MountIso_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ISO filesystem mounting not yet implemented.", "Mount ISO");
        }

        private void MountExt_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("EXT filesystem mounting not yet implemented.", "Mount EXT");
        }

        private void MountSquashFs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SquashFS filesystem mounting not yet implemented.", "Mount SquashFS");
        }

        // Button click to select firmware once
        private void SelectFirmware_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Firmware Images (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                firmwarePath = dialog.FileName;
                StatusBarText($"Firmware selected: {System.IO.Path.GetFileName(firmwarePath)}");
            }
        }
    }
}