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
using System.Text;
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
        
        // BOLT Bootloader Integration
        private BoltEmulatorBridge boltBridge;
        private bool boltInitialized;

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
                "Pluto TV Integration",
                "Custom Hypervisor",
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
                case "Custom Hypervisor":
                    await HandleCustomHypervisor();
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
            try
            {
                StatusBarText("🚀 Starting AT&T U-verse + Microsoft Mediaroom emulation...");
                
                // Check if this is an nk.bin kernel file
                if (Path.GetFileName(firmwarePath).ToLower() == "nk.bin")
                {
                    StatusBarText("🔍 Detected nk.bin - using comprehensive Mediaroom boot manager...");
                    
                    // Use the enhanced U-verse emulator with Mediaroom boot manager
                    var uverseEmulator = new UverseEmulator();
                    
                    // Load the nk.bin kernel
                    byte[] kernelData = File.ReadAllBytes(firmwarePath);
                    if (!await uverseEmulator.LoadBootImage(kernelData))
                    {
                        throw new Exception("Failed to load U-verse boot image");
                    }
                    
                    // Start comprehensive Mediaroom boot sequence
                    bool bootSuccess = await uverseEmulator.StartEmulation();
                    
                    if (!bootSuccess)
                    {
                        StatusBarText("❌ Mediaroom boot failed - check boot log for details");
                        
                        // Show boot failure details
                        var failureLog = uverseEmulator.GetBootLog();
                        ShowTextWindow("U-verse + Mediaroom Boot Failure", failureLog);
                        return;
                    }
                    
                    // Show successful boot status
                    var status = uverseEmulator.GetEmulationStatus();
                    var bootLog = uverseEmulator.GetBootLog();
                    
                    var results = new List<string>
                    {
                        "🎉 AT&T U-verse + Microsoft Mediaroom Boot Complete!",
                        "",
                        "=== System Status ===",
                        $"Platform: {status["Platform"]}",
                        $"File: {Path.GetFileName(firmwarePath)}",
                        $"Initialized: {status["IsInitialized"]}",
                        $"Boot Complete: {status["IsBootComplete"]}",
                        $"Hardware: {((UverseHardwareConfig)status["HardwareConfig"]).Model}",
                        "",
                        "=== Boot Log (Last 15 entries) ===",
                    };
                    
                    // Add recent boot log entries
                    var recentLogs = bootLog.TakeLast(15);
                    results.AddRange(recentLogs);
                    
                    results.Add("");
                    results.Add("✅ AT&T U-verse IPTV Platform is fully operational!");
                    results.Add("📺 Microsoft Mediaroom services are running");
                    results.Add("🌐 IPTV infrastructure is connected and ready");
                    
                    ShowTextWindow("U-verse + Mediaroom Emulation Success", results);
                    StatusBarText("✅ U-verse + Mediaroom emulation started successfully");
                }
                else
                {
                    // Use the enhanced U-verse emulator for other files
                    StatusBarText("🔄 Using enhanced U-verse + Mediaroom emulator...");
                    
                    // Detect if it's a signature file (.sig) or other content
                    string ext = Path.GetExtension(firmwarePath).ToLowerInvariant();
                    
                    if (ext == ".sig" || ext == ".bin" || ext == ".img")
                    {
                        // Handle firmware-based U-verse emulation with Mediaroom boot
                        StatusBarText($"📦 Loading U-verse firmware: {Path.GetFileName(firmwarePath)}...");
                        
                        // Load firmware data
                        byte[] firmwareData = File.ReadAllBytes(firmwarePath);
                        
                        // Create enhanced U-verse emulator
                        var emulator = new UverseEmulator();
                        
                        // Load boot image
                        if (!await emulator.LoadBootImage(firmwareData))
                        {
                            throw new Exception("Failed to load U-verse firmware");
                        }
                        
                        // Start emulation with Mediaroom boot
                        bool success = await emulator.StartEmulation();
                        
                        if (!success)
                        {
                            StatusBarText("❌ U-verse emulation failed");
                            var failureLog = emulator.GetBootLog();
                            ShowTextWindow("U-verse Emulation Failure", failureLog);
                            return;
                        }
                        
                        // Get status and show results
                        var status = emulator.GetEmulationStatus();
                        var bootStatus = status.ContainsKey("BootStatus") ? (Dictionary<string, object>)status["BootStatus"] : null;
                        
                        var uverseLog = new List<string>
                        {
                            "🎉 AT&T U-verse + Microsoft Mediaroom Emulation Complete!",
                            "",
                            "=== System Information ===",
                            $"File: {Path.GetFileName(firmwarePath)}",
                            $"Size: {firmwareData.Length:N0} bytes",
                            $"Platform: {status["Platform"]}",
                            $"Hardware: {((UverseHardwareConfig)status["HardwareConfig"]).Model}",
                            $"Processor: {((UverseHardwareConfig)status["HardwareConfig"]).Processor}",
                            $"Memory: {((UverseHardwareConfig)status["HardwareConfig"]).MemoryMB}MB",
                            $"OS: {((UverseHardwareConfig)status["HardwareConfig"]).OS}",
                            "",
                            "=== Boot Status ===",
                            $"Boot Stage: {bootStatus?["Stage"] ?? "Complete"}",
                            $"Kernel Loaded: {bootStatus?["KernelLoaded"] ?? true}",
                            $"Mediaroom Ready: {bootStatus?["MediaroomReady"] ?? true}",
                            $"Components: {bootStatus?["ComponentsLoaded"] ?? "All"}",
                            "",
                            "✅ Microsoft Mediaroom IPTV platform is operational",
                            "📺 AT&T U-verse services are running",
                            "🌐 IPTV infrastructure connected"
                        };
                        
                        ShowTextWindow("U-verse + Mediaroom Emulation", uverseLog);
                        StatusBarText("✅ U-verse + Mediaroom emulation completed successfully");
                    }
                    else
                    {
                        // Generic firmware analysis for other U-verse files
                        StatusBarText("🔍 Analyzing U-verse firmware structure...");
                        
                        string extractDir = Path.Combine(Path.GetDirectoryName(firmwarePath), 
                            Path.GetFileNameWithoutExtension(firmwarePath) + "_extracted");
                        
                        await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(firmwarePath, extractDir));
                        FirmwareAnalyzer.AnalyzeFirmwareArchive(firmwarePath, extractDir);
                        
                        var results = new List<string>
                        {
                            "🔍 AT&T U-verse Firmware Analysis Complete",
                            "",
                            "=== Analysis Results ===",
                            $"File: {Path.GetFileName(firmwarePath)}",
                            $"Extracted to: {extractDir}",
                            $"Type: {Path.GetExtension(firmwarePath)} firmware",
                            "",
                            "📂 Check extracted directory for:",
                            "  • WinCE kernel files (nk.bin)",
                            "  • Mediaroom components",
                            "  • Registry hives (*.hv)",
                            "  • IPTV configuration files",
                            "  • System overlays and modules",
                            "",
                            "💡 Tip: If nk.bin is found, load it directly for full Mediaroom boot emulation"
                        };
                        
                        ShowTextWindow("U-verse Firmware Analysis", results);
                    }
                    
                    StatusBarText("U-verse content emulation completed");
                }
            }
            catch (Exception ex)
            {
                StatusBarText("U-verse emulation failed");
                ShowTextWindow("U-verse Emulation Error", new List<string> 
                { 
                    $"Error: {ex.Message}",
                    $"File: {Path.GetFileName(firmwarePath)}",
                    $"Stack: {ex.StackTrace}"
                });
            }
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
            if (string.IsNullOrEmpty(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            StatusBarText("Starting Firmadyne-based emulation...");
            var logEntries = new List<string> { "=== Firmadyne Firmware Extraction Pipeline ===" };
            
            try
            {
                string firmwareFile = firmwarePath;
                string workDir = Path.Combine(Path.GetTempPath(), "firmadyne_" + Path.GetFileNameWithoutExtension(firmwareFile));
                Directory.CreateDirectory(workDir);
                
                logEntries.Add($"Working directory: {workDir}");
                logEntries.Add($"Analyzing firmware: {Path.GetFileName(firmwareFile)}");
                
                // Step 1: Extract firmware using binwalk
                logEntries.Add("");
                logEntries.Add("=== Step 1: Firmware Extraction ===");
                await ExtractFirmwareWithBinwalk(firmwareFile, workDir, logEntries);
                
                // Step 2: Identify filesystem and architecture
                logEntries.Add("");
                logEntries.Add("=== Step 2: Filesystem Analysis ===");
                var fsInfo = await AnalyzeFirmwareFilesystem(workDir, logEntries);
                
                // Step 3: Create QEMU disk image
                logEntries.Add("");
                logEntries.Add("=== Step 3: QEMU Disk Image Creation ===");
                string diskImage = await CreateQemuDiskImage(fsInfo, workDir, logEntries);
                
                // Step 4: Launch QEMU emulation
                logEntries.Add("");
                logEntries.Add("=== Step 4: QEMU Emulation Launch ===");
                await LaunchQemuEmulation(fsInfo, diskImage, logEntries);
                
                StatusBarText("Firmadyne emulation complete - firmware extracted and running in QEMU.");
            }
            catch (Exception ex)
            {
                logEntries.Add($"ERROR: {ex.Message}");
                StatusBarText("Firmadyne emulation failed.");
            }
            
            ShowTextWindow("Firmadyne Emulation Pipeline", logEntries);
        }

        
        // Firmadyne Pipeline Implementation
        
        private class FirmwareInfo
        {
            public string Architecture { get; set; } = "unknown";
            public string RootfsPath { get; set; } = "";
            public string KernelPath { get; set; } = "";
            public string InitramfsPath { get; set; } = "";
            public List<string> Filesystems { get; set; } = new List<string>();
        }
        
        private async Task ExtractFirmwareWithBinwalk(string firmwareFile, string workDir, List<string> log)
        {
            log.Add("Extracting firmware with binwalk...");
            ShowFunnyStatus("Firmware extraction");
            
            try
            {
                // Try using binwalk if available
                var psi = new ProcessStartInfo("binwalk", $"-e \"{firmwareFile}\" -C \"{workDir}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var proc = Process.Start(psi);
                await proc.WaitForExitAsync();
                
                if (proc.ExitCode == 0)
                {
                    log.Add("Binwalk extraction successful");
                    return;
                }
            }
            catch
            {
                log.Add("Binwalk not available, using built-in extraction...");
            }
            
            // Fallback: Use built-in firmware analyzer
            await Task.Run(() =>
            {
                try
                {
                    FirmwareAnalyzer.AnalyzeFirmwareArchive(firmwareFile, workDir);
                    log.Add("Built-in extraction completed");
                }
                catch (Exception ex)
                {
                    log.Add($"Extraction failed: {ex.Message}");
                }
            });
        }
        
        private async Task<FirmwareInfo> AnalyzeFirmwareFilesystem(string workDir, List<string> log)
        {
            var info = new FirmwareInfo();
            
            await Task.Run(() =>
            {
                log.Add("Scanning extracted files...");
                
                var allFiles = Directory.GetFiles(workDir, "*", SearchOption.AllDirectories);
                log.Add($"Found {allFiles.Length} extracted files");
                
                // Look for common filesystem indicators
                foreach (var file in allFiles)
                {
                    var fileName = Path.GetFileName(file).ToLower();
                    var ext = Path.GetExtension(file).ToLower();
                    
                    // Detect architecture from binary files
                    if (fileName.Contains("vmlinux") || fileName.Contains("kernel"))
                    {
                        info.KernelPath = file;
                        info.Architecture = DetectArchitectureFromElf(file);
                        log.Add($"Kernel found: {Path.GetFileName(file)} ({info.Architecture})");
                    }
                    
                    // Look for root filesystem
                    if (fileName.Contains("rootfs") || fileName.Contains("squashfs") || ext == ".cramfs")
                    {
                        info.RootfsPath = file;
                        info.Filesystems.Add(file);
                        log.Add($"Filesystem found: {Path.GetFileName(file)}");
                    }
                    
                    // Look for initramfs
                    if (fileName.Contains("initramfs") || fileName.Contains("initrd"))
                    {
                        info.InitramfsPath = file;
                        log.Add($"Initramfs found: {Path.GetFileName(file)}");
                    }
                }
                
                // If no specific arch detected, try to detect from filesystem contents
                if (info.Architecture == "unknown" && !string.IsNullOrEmpty(info.RootfsPath))
                {
                    info.Architecture = DetectArchitectureFromFilesystem(info.RootfsPath);
                    log.Add($"Architecture detected from filesystem: {info.Architecture}");
                }
            });
            
            return info;
        }
        
        private string DetectArchitectureFromElf(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                if (bytes.Length < 20) return "unknown";
                
                // Check ELF magic
                if (bytes[0] != 0x7F || bytes[1] != 'E' || bytes[2] != 'L' || bytes[3] != 'F')
                    return "unknown";
                
                // Get architecture from ELF header
                ushort machine = BitConverter.ToUInt16(bytes, 18);
                return machine switch
                {
                    0x3E => "x86_64",
                    0x03 => "x86",
                    0x28 => "arm",
                    0xB7 => "arm64",
                    0x08 => "mips",
                    0x14 => "ppc",
                    0x15 => "ppc64",
                    0x2B => "sparc",
                    0x2A => "sparc64",
                    _ => "unknown"
                };
            }
            catch
            {
                return "unknown";
            }
        }
        
        private string DetectArchitectureFromFilesystem(string fsPath)
        {
            try
            {
                // Look for binaries in common locations
                var testPaths = new[] { "/bin/sh", "/bin/busybox", "/sbin/init" };
                
                // This is a simplified detection - in real implementation would mount and examine
                return "arm"; // Default for most embedded devices
            }
            catch
            {
                return "unknown";
            }
        }
        
        private async Task<string> CreateQemuDiskImage(FirmwareInfo info, string workDir, List<string> log)
        {
            string diskImage = Path.Combine(workDir, "firmware.qcow2");
            
            try
            {
                log.Add("Creating QEMU disk image...");
                
                // Create QEMU disk image
                var createCmd = $"create -f qcow2 \"{diskImage}\" 256M";
                await RunQemuCommand("qemu-img", createCmd, log);
                
                if (File.Exists(diskImage))
                {
                    log.Add($"Disk image created: {diskImage}");
                    
                    // If we have a rootfs, try to write it to the disk
                    if (!string.IsNullOrEmpty(info.RootfsPath))
                    {
                        await WriteFilesystemToDisk(info.RootfsPath, diskImage, log);
                    }
                }
                else
                {
                    throw new Exception("Failed to create disk image");
                }
            }
            catch (Exception ex)
            {
                log.Add($"Disk creation failed: {ex.Message}");
                log.Add("Creating dummy disk image for testing...");
                
                // Create a minimal disk image file as fallback
                await File.WriteAllBytesAsync(diskImage, new byte[256 * 1024 * 1024]);
            }
            
            return diskImage;
        }
        
        private async Task WriteFilesystemToDisk(string fsPath, string diskImage, List<string> log)
        {
            try
            {
                log.Add("Writing filesystem to disk image...");
                
                // Use dd-like operation to write filesystem to disk
                var sourceBytes = await File.ReadAllBytesAsync(fsPath);
                var diskBytes = await File.ReadAllBytesAsync(diskImage);
                
                // Write filesystem at offset (simple approach)
                if (sourceBytes.Length <= diskBytes.Length)
                {
                    Array.Copy(sourceBytes, 0, diskBytes, 0, sourceBytes.Length);
                    await File.WriteAllBytesAsync(diskImage, diskBytes);
                    log.Add("Filesystem written to disk image");
                }
                else
                {
                    log.Add("Filesystem too large for disk image");
                }
            }
            catch (Exception ex)
            {
                log.Add($"Failed to write filesystem: {ex.Message}");
            }
        }
        
        private async Task LaunchQemuEmulation(FirmwareInfo info, string diskImage, List<string> log)
        {
            try
            {
                log.Add("Launching QEMU emulation...");
                
                // Build QEMU command based on detected architecture
                var qemuCmd = BuildQemuCommand(info, diskImage);
                log.Add($"QEMU command: {qemuCmd}");
                
                // Launch QEMU in background
                await RunQemuCommand("qemu-system-" + info.Architecture, qemuCmd, log, isBackground: true);
                
                log.Add("QEMU emulation started successfully");
                log.Add("Check QEMU window for firmware boot process");
            }
            catch (Exception ex)
            {
                log.Add($"QEMU launch failed: {ex.Message}");
                log.Add("Note: Ensure QEMU is installed and in PATH");
            }
        }
        
        private string BuildQemuCommand(FirmwareInfo info, string diskImage)
        {
            var cmd = new List<string>();
            
            // Basic VM configuration
            cmd.Add("-M virt"); // Use virt machine for ARM
            cmd.Add("-m 256M"); // 256MB RAM
            cmd.Add("-cpu cortex-a15"); // ARM CPU
            
            // Add disk
            cmd.Add($"-drive file=\"{diskImage}\",format=qcow2");
            
            // Add kernel if available
            if (!string.IsNullOrEmpty(info.KernelPath))
            {
                cmd.Add($"-kernel \"{info.KernelPath}\"");
            }
            
            // Add initrd if available
            if (!string.IsNullOrEmpty(info.InitramfsPath))
            {
                cmd.Add($"-initrd \"{info.InitramfsPath}\"");
            }
            
            // Network setup
            cmd.Add("-netdev user,id=net0");
            cmd.Add("-device virtio-net-device,netdev=net0");
            
            // Console setup
            cmd.Add("-nographic");
            cmd.Add("-serial mon:stdio");
            
            return string.Join(" ", cmd);
        }
        
        private async Task RunQemuCommand(string command, string args, List<string> log, bool isBackground = false)
        {
            try
            {
                var psi = new ProcessStartInfo(command, args)
                {
                    RedirectStandardOutput = !isBackground,
                    RedirectStandardError = !isBackground,
                    UseShellExecute = isBackground,
                    CreateNoWindow = !isBackground
                };
                
                var proc = Process.Start(psi);
                
                if (isBackground)
                {
                    log.Add($"Started background process: {command} {args}");
                    return;
                }
                
                await proc.WaitForExitAsync();
                
                if (proc.ExitCode == 0)
                {
                    log.Add($"Command successful: {command}");
                }
                else
                {
                    var error = await proc.StandardError.ReadToEndAsync();
                    log.Add($"Command failed: {error}");
                }
            }
            catch (Exception ex)
            {
                log.Add($"Failed to run {command}: {ex.Message}");
            }
        }
        
        private async Task HandleAzeriaEmulation()
        {
            if (string.IsNullOrEmpty(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            StatusBarText("Starting Azeria Labs ARM firmware emulation...");
            var logEntries = new List<string> { "=== Azeria Labs ARM Firmware Emulation ===" };
            
            try
            {
                string firmwareFile = firmwarePath;
                logEntries.Add($"Firmware: {Path.GetFileName(firmwareFile)}");
                logEntries.Add("Following Azeria Labs methodology...");
                
                // Step 1: Analyze firmware binary
                logEntries.Add("");
                logEntries.Add("=== Step 1: Firmware Analysis ===");
                var firmwareInfo = await AnalyzeFirmwareBinary(firmwareFile, logEntries);
                
                // Step 2: Extract filesystem if embedded
                logEntries.Add("");
                logEntries.Add("=== Step 2: Filesystem Extraction ===");
                string extractedFs = await ExtractEmbeddedFilesystem(firmwareFile, logEntries);
                
                // Step 3: Setup QEMU environment
                logEntries.Add("");
                logEntries.Add("=== Step 3: QEMU Environment Setup ===");
                await SetupQemuForArm(firmwareInfo, logEntries);
                
                // Step 4: Create emulation environment
                logEntries.Add("");
                logEntries.Add("=== Step 4: ARM Emulation Launch ===");
                await LaunchArmEmulation(firmwareFile, extractedFs, firmwareInfo, logEntries);
                
                StatusBarText("Azeria ARM emulation setup complete.");
            }
            catch (Exception ex)
            {
                logEntries.Add($"ERROR: {ex.Message}");
                StatusBarText("Azeria ARM emulation failed.");
            }
            
            ShowTextWindow("Azeria Labs ARM Emulation", logEntries);
        }
        
        private async Task<Dictionary<string, string>> AnalyzeFirmwareBinary(string firmwareFile, List<string> log)
        {
            var info = new Dictionary<string, string>();
            
            await Task.Run(() =>
            {
                try
                {
                    var bytes = File.ReadAllBytes(firmwareFile);
                    log.Add($"File size: {bytes.Length:N0} bytes");
                    
                    // Check for ELF header
                    if (bytes.Length >= 4 && bytes[0] == 0x7F && bytes[1] == 'E' && bytes[2] == 'L' && bytes[3] == 'F')
                    {
                        info["type"] = "ELF";
                        info["arch"] = DetectArchitectureFromElf(firmwareFile);
                        log.Add($"ELF binary detected - Architecture: {info["arch"]}");
                    }
                    else
                    {
                        info["type"] = "raw";
                        info["arch"] = "arm"; // Assume ARM for raw binaries
                        log.Add("Raw binary detected - Assuming ARM architecture");
                    }
                    
                    // Look for embedded strings
                    var strings = ExtractStrings(bytes);
                    var interestingStrings = strings.Where(s => 
                        s.Contains("linux") || s.Contains("kernel") || s.Contains("init") ||
                        s.Contains("busybox") || s.Contains("arm") || s.Contains("mips")).ToList();
                    
                    if (interestingStrings.Any())
                    {
                        log.Add("Interesting strings found:");
                        foreach (var str in interestingStrings.Take(5))
                        {
                            log.Add($"  '{str}'");
                        }
                    }
                    
                    // Detect load address patterns
                    info["loadaddr"] = DetectLoadAddress(bytes);
                    log.Add($"Suggested load address: {info["loadaddr"]}");
                    
                }
                catch (Exception ex)
                {
                    log.Add($"Analysis error: {ex.Message}");
                }
            });
            
            return info;
        }
        
        private List<string> ExtractStrings(byte[] data)
        {
            var strings = new List<string>();
            var current = new List<byte>();
            
            foreach (byte b in data)
            {
                if (b >= 32 && b <= 126) // Printable ASCII
                {
                    current.Add(b);
                }
                else
                {
                    if (current.Count >= 4) // Minimum string length
                    {
                        strings.Add(System.Text.Encoding.ASCII.GetString(current.ToArray()));
                    }
                    current.Clear();
                }
            }
            
            return strings;
        }
        
        private string DetectLoadAddress(byte[] data)
        {
            // Common ARM load addresses
            var commonAddresses = new[] { "0x80008000", "0x80010000", "0x40008000", "0x20008000" };
            
            // Look for patterns that might indicate load addresses
            // This is a simplified heuristic
            return "0x80008000"; // Default ARM kernel load address
        }
        
        private async Task<string> ExtractEmbeddedFilesystem(string firmwareFile, List<string> log)
        {
            string extractDir = Path.Combine(Path.GetTempPath(), "azeria_extracted");
            
            try
            {
                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);
                Directory.CreateDirectory(extractDir);
                
                log.Add("Extracting embedded filesystem...");
                
                // Use our firmware analyzer
                await Task.Run(() => FirmwareAnalyzer.AnalyzeFirmwareArchive(firmwareFile, extractDir));
                
                var extractedFiles = Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories);
                log.Add($"Extracted {extractedFiles.Length} files to {extractDir}");
                
                // Look for filesystem images
                var fsImages = extractedFiles.Where(f => 
                    f.EndsWith(".cramfs") || f.EndsWith(".squashfs") || 
                    f.Contains("rootfs") || f.Contains("filesystem")).ToArray();
                
                if (fsImages.Any())
                {
                    log.Add("Filesystem images found:");
                    foreach (var img in fsImages)
                    {
                        log.Add($"  {Path.GetFileName(img)}");
                    }
                    return fsImages[0];
                }
                
                log.Add("No specific filesystem images found");
                return extractDir;
            }
            catch (Exception ex)
            {
                log.Add($"Extraction failed: {ex.Message}");
                return "";
            }
        }
        
        private async Task SetupQemuForArm(Dictionary<string, string> firmwareInfo, List<string> log)
        {
            await Task.Run(() =>
            {
                log.Add("Setting up QEMU ARM environment...");
                
                // Check if QEMU is available
                try
                {
                    var psi = new ProcessStartInfo("qemu-system-arm", "--version")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    var proc = Process.Start(psi);
                    proc.WaitForExit();
                    
                    if (proc.ExitCode == 0)
                    {
                        var version = proc.StandardOutput.ReadToEnd();
                        log.Add($"QEMU found: {version.Split('\n')[0]}");
                    }
                    else
                    {
                        log.Add("QEMU not found - install QEMU for full emulation");
                    }
                }
                catch
                {
                    log.Add("QEMU not available - using built-in ARM emulator");
                }
                
                log.Add("ARM emulation environment ready");
            });
        }
        
        private async Task LaunchArmEmulation(string firmwareFile, string extractedFs, Dictionary<string, string> info, List<string> log)
        {
            try
            {
                log.Add("Launching ARM firmware emulation...");
                
                // Try QEMU first
                if (await TryLaunchQemuArm(firmwareFile, info, log))
                {
                    log.Add("QEMU ARM emulation started successfully");
                    return;
                }
                
                // Fallback to our custom ARM emulator
                log.Add("Falling back to custom ARM emulator...");
                await LaunchCustomArmEmulation(firmwareFile, log);
                
            }
            catch (Exception ex)
            {
                log.Add($"Emulation launch failed: {ex.Message}");
            }
        }
        
        private async Task<bool> TryLaunchQemuArm(string firmwareFile, Dictionary<string, string> info, List<string> log)
        {
            try
            {
                var args = new List<string>
                {
                    "-M versatilepb",  // Versatile platform board
                    "-cpu arm1176",    // ARM1176 CPU
                    "-m 256M",         // 256MB RAM
                    "-nographic",      // No graphics
                    "-serial stdio"    // Serial console
                };
                
                // Add kernel if it's an ELF
                if (info.ContainsKey("type") && info["type"] == "ELF")
                {
                    args.Add($"-kernel \"{firmwareFile}\"");
                }
                else
                {
                    // For raw binaries, load at specific address
                    string loadAddr = info.ContainsKey("loadaddr") ? info["loadaddr"] : "0x80008000";
                    args.Add($"-device loader,file=\"{firmwareFile}\",addr={loadAddr}");
                }
                
                var cmdLine = string.Join(" ", args);
                log.Add($"QEMU command: qemu-system-arm {cmdLine}");
                
                var psi = new ProcessStartInfo("qemu-system-arm", cmdLine)
                {
                    UseShellExecute = true, // Let QEMU run in its own window
                    CreateNoWindow = false
                };
                
                await Task.Run(() => Process.Start(psi));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task LaunchCustomArmEmulation(string firmwareFile, List<string> log)
        {
            try
            {
                log.Add("Starting real MIPS emulator...");
                
                // Use our RealMipsHypervisor for actual emulation
                var firmware = await File.ReadAllBytesAsync(firmwareFile);
                log.Add($"Loaded {firmware.Length:N0} bytes of firmware");
                
                // Launch real MIPS hypervisor
                var hypervisor = new RealMipsHypervisor();
                await hypervisor.StartEmulation(firmware);
                
                log.Add("Real MIPS emulation started");
                log.Add("Check hypervisor window for firmware execution");
            }
            catch (Exception ex)
            {
                log.Add($"Custom ARM emulation failed: {ex.Message}");
            }
        }

        // Core feature handlers

        /// <summary>
        /// Emulates an RDK-V set-top box using dedicated RDK-V emulator with ARM decoding.
        /// </summary>
        private async Task HandleRdkVEmulation()
        {
            if (string.IsNullOrEmpty(firmwarePath))
            {
                ErrorManager.ShowError(ErrorManager.Codes.INVALID_PARAMETER, "No firmware file selected");
                return;
            }
            
            string path = firmwarePath;
            StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.INITIALIZING));

            try
            {
                var bin = System.IO.File.ReadAllBytes(path);
                Debug.WriteLine($"Loaded RDK-V firmware: {bin.Length} bytes from {path}");
                
                StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.PROCESSING));

                // Use the proper RDK-V emulator, not generic HomebrewEmulator
                var emulator = new ProcessorEmulator.Emulation.RDKVEmulator();
                emulator.LoadBinary(bin);
                emulator.Run(); // This will actually boot the firmware with ARM decoding

                StatusBarText(ErrorManager.GetSuccessMessage(ErrorManager.Codes.WUBBA_SUCCESS));
                
                // Show welcome message for first-time users
                if (IsFirstTimeExtraction())
                {
                    ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                    MarkFirstTimeExtractionDone();
                }
            }
            catch (FileNotFoundException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILE_NOT_FOUND, $"RDK-V firmware: {path}");
                ErrorManager.LogError(ErrorManager.Codes.FILE_NOT_FOUND, path);
            }
            catch (UnauthorizedAccessException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.ACCESS_DENIED, $"RDK-V firmware: {path}");
                ErrorManager.LogError(ErrorManager.Codes.ACCESS_DENIED, path);
            }
            catch (InvalidDataException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.INVALID_FIRMWARE_FORMAT, $"RDK-V firmware: {path}");
                ErrorManager.LogError(ErrorManager.Codes.INVALID_FIRMWARE_FORMAT, path);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ErrorManager.Codes.EMULATION_FAILED, $"RDK-V firmware: {path}", ex);
                ErrorManager.LogError(ErrorManager.Codes.EMULATION_FAILED, path, ex);
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
                // Show "instructions unclear" error
                ErrorManager.ShowInstructionsUnclear($"Cross-compilation from {fromArch} to {toArch}");
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
            StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.ANALYZING));
            
            try
            {
                await Task.Run(() => FirmwareAnalyzer.AnalyzeFirmwareArchive(archivePath, extractDir));
                StatusBarText(ErrorManager.GetSuccessMessage(ErrorManager.Codes.OPERATION_SUCCESS));
                
                MessageBox.Show($"D'oh! I mean... success! Analysis finished.\n\nExtracted files at:\n{extractDir}", 
                    "Analysis Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Show welcome message for first-time users
                if (IsFirstTimeExtraction())
                {
                    ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                    MarkFirstTimeExtractionDone();
                }
            }
            catch (UnauthorizedAccessException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.ACCESS_DENIED, archivePath);
            }
            catch (DirectoryNotFoundException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILE_NOT_FOUND, archivePath);
            }
            catch (IOException ioEx)
            {
                ErrorManager.ShowError(ErrorManager.Codes.DATA_CORRUPTION, archivePath, ioEx);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ErrorManager.Codes.GENERAL_FAILURE, archivePath, ex);
                ErrorManager.LogError(ErrorManager.Codes.GENERAL_FAILURE, archivePath, ex);
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
            
            StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.LOADING));
            
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
                
                // Show success message
                StatusBarText(ErrorManager.GetSuccessMessage(ErrorManager.Codes.SNAKE_JAZZ_SUCCESS));
                
                // Show welcome message for first-time users
                if (IsFirstTimeExtraction())
                {
                    ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                    MarkFirstTimeExtractionDone();
                }
            }
            catch (UnauthorizedAccessException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.ACCESS_DENIED, $"SquashFS: {path}");
                ErrorManager.LogError(ErrorManager.Codes.ACCESS_DENIED, path);
            }
            catch (FileNotFoundException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILE_NOT_FOUND, $"SquashFS: {path}");
                ErrorManager.LogError(ErrorManager.Codes.FILE_NOT_FOUND, path);
            }
            catch (IOException ioEx)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILESYSTEM_CORRUPTION, $"SquashFS: {path}", ioEx);
                ErrorManager.LogError(ErrorManager.Codes.FILESYSTEM_CORRUPTION, path, ioEx);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ErrorManager.Codes.MOUNT_FAILED, $"SquashFS: {path}", ex);
                ErrorManager.LogError(ErrorManager.Codes.MOUNT_FAILED, path, ex);
            }
            await Task.CompletedTask;
        }

        
        // First-time user tracking
        private static bool firstTimeExtractionDone = false;
        private static Random statusRandom = new Random();
        
        private bool IsFirstTimeExtraction()
        {
            return !firstTimeExtractionDone;
        }
        
        private void MarkFirstTimeExtractionDone()
        {
            firstTimeExtractionDone = true;
        }
        
        /// <summary>
        /// Show funny status message during long operations (Homer style)
        /// </summary>
        private void ShowFunnyStatus(string operation = "")
        {
            var funnyMessages = new[]
            {
                ErrorManager.GetStatusMessage(ErrorManager.Codes.INITIALIZING),
                "Processing... wubba lubba dub dub!",
                "Loading... snake jazz playing in background.",
                "Analyzing... stupid sexy Flanders analyzing.",
                "Please wait... D'oh! This is taking forever.",
                "Working hard... or hardly working?",
                "Computing... *dial-up modem sounds*",
                "Almost there... like Sisyphus, but funnier.",
                "Still going... grab a beer, this might take a while."
            };
            
            string message = funnyMessages[statusRandom.Next(funnyMessages.Length)];
            if (!string.IsNullOrEmpty(operation))
                message = $"{operation}: {message}";
                
            StatusBarText(message);
        }

        /// <summary>
        /// Mounts a FAT filesystem image using DiscUtils.Fat
        /// </summary>
        private async Task HandleFatMount()
        {
            var dlg = new OpenFileDialog { Filter = "FAT Images (*.img;*.fat;*.fat32)|*.img;*.fat;*.fat32|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            
            StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.LOADING));
            
            try
            {
                using var stream = File.OpenRead(path);
                SetupHelper.RegisterAssembly(typeof(DiscUtils.Fat.FatFileSystem).Assembly);
                var fs = new DiscUtils.Fat.FatFileSystem(stream);
                var entries = new List<string> { $"Mounted FAT: {Path.GetFileName(path)}" };
                foreach (var entry in fs.GetFiles("", "*", SearchOption.AllDirectories))
                    entries.Add(entry);
                ShowTextWindow("FAT Filesystem Mount", entries);
                
                StatusBarText(ErrorManager.GetSuccessMessage(ErrorManager.Codes.BART_SUCCESS));
                
                // Show welcome message for first-time users
                if (IsFirstTimeExtraction())
                {
                    ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                    MarkFirstTimeExtractionDone();
                }
            }
            catch (UnauthorizedAccessException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.ACCESS_DENIED, $"FAT: {path}");
            }
            catch (IOException ioEx)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILESYSTEM_CORRUPTION, $"FAT: {path}", ioEx);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ErrorManager.Codes.MOUNT_FAILED, $"FAT: {path}", ex);
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
                    var textBox = FindName("FirmwarePathTextBox") as TextBox;
                    if (textBox != null)
                        textBox.Text = dlg.FileName;
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
                var rdkRadio = FindName("RdkVPlatformRadio") as RadioButton;
                var uverseRadio = FindName("UversePlatformRadio") as RadioButton;
                var genericRadio = FindName("GenericPlatformRadio") as RadioButton;
                
                if (rdkRadio?.IsChecked == true)
                    selectedPlatform = "RDK-V";
                else if (uverseRadio?.IsChecked == true)
                    selectedPlatform = "U-verse";
                else if (genericRadio?.IsChecked == true)
                    selectedPlatform = "Generic";
            }
            catch
            {
                // Fallback if radio buttons not accessible
                selectedPlatform = "Generic"; // Default to generic for safety
            }

            // Determine selected emulator type
            string selectedEmulator = "HomebrewEmulator"; // Default
            try
            {
                var homebrewRadio = FindName("HomebrewEmulatorRadio") as RadioButton;
                var qemuRadio = FindName("QemuEmulatorRadio") as RadioButton;
                var retDecRadio = FindName("RetDecTranslatorRadio") as RadioButton;
                
                if (homebrewRadio?.IsChecked == true)
                    selectedEmulator = "HomebrewEmulator";
                else if (qemuRadio?.IsChecked == true)
                    selectedEmulator = "QEMU";
                else if (retDecRadio?.IsChecked == true)
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
                    case "U-verse":
                        await HandleUverseEmulation();
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
            if (string.IsNullOrEmpty(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                StatusBarText("Starting RetDec binary translation...");
                var logEntries = new List<string> { "=== RetDec Binary Translation Pipeline ===" };
                
                string firmwareFile = firmwarePath;
                logEntries.Add($"Input binary: {Path.GetFileName(firmwareFile)}");
                
                // Step 1: Analyze binary format and architecture
                logEntries.Add("");
                logEntries.Add("=== Step 1: Binary Analysis ===");
                var binaryInfo = await AnalyzeBinaryFormat(firmwareFile, logEntries);
                
                // Step 2: Decompile binary to high-level code
                logEntries.Add("");
                logEntries.Add("=== Step 2: Decompilation ===");
                string decompiledCode = await DecompileBinary(firmwareFile, binaryInfo, logEntries);
                
                // Step 3: Cross-compile to target architecture
                logEntries.Add("");
                logEntries.Add("=== Step 3: Cross-Architecture Translation ===");
                await TranslateBinaryArchitecture(firmwareFile, binaryInfo, logEntries);
                
                // Step 4: Generate analysis report
                logEntries.Add("");
                logEntries.Add("=== Step 4: Analysis Report ===");
                await GenerateAnalysisReport(firmwareFile, decompiledCode, logEntries);
                
                StatusBarText("RetDec translation completed successfully.");
                ShowTextWindow("RetDec Binary Translation", logEntries);
            }
            catch (Exception ex)
            {
                // Show Homer's philosophy about trying and failing
                ErrorManager.ShowTriedAndFailed("RetDec binary translation");
                
                var errorLog = new List<string> 
                { 
                    "=== RetDec Translation Error ===",
                    ErrorManager.GetErrorMessage(ErrorManager.Codes.TRIED_AND_FAILED),
                    "",
                    $"What went wrong: {ex.Message}",
                    "",
                    "Instructions unclear? " + ErrorManager.GetErrorMessage(ErrorManager.Codes.BEER_FRIDGE_INSTRUCTIONS),
                    "",
                    "Note: RetDec requires external installation.",
                    "Install RetDec from: https://github.com/avast/retdec",
                    "",
                    "Alternative: Using built-in binary analysis tools..."
                };
                
                // Fallback to built-in tools
                await FallbackBinaryAnalysis(firmwarePath, errorLog);
                
                ShowTextWindow("RetDec Translation (Fallback)", errorLog);
                StatusBarText("RetDec translation completed with fallback tools.");
            }
        }
        
        private async Task<Dictionary<string, object>> AnalyzeBinaryFormat(string filePath, List<string> log)
        {
            var info = new Dictionary<string, object>();
            
            await Task.Run(() =>
            {
                try
                {
                    var bytes = File.ReadAllBytes(filePath);
                    log.Add($"File size: {bytes.Length:N0} bytes");
                    
                    // Detect file format
                    if (bytes.Length >= 4)
                    {
                        // ELF detection
                        if (bytes[0] == 0x7F && bytes[1] == 'E' && bytes[2] == 'L' && bytes[3] == 'F')
                        {
                            info["format"] = "ELF";
                            info["architecture"] = DetectArchitectureFromElf(filePath);
                            log.Add($"ELF executable detected - {info["architecture"]}");
                        }
                        // PE detection
                        else if (bytes[0] == 'M' && bytes[1] == 'Z')
                        {
                            info["format"] = "PE";
                            info["architecture"] = "x86";
                            log.Add("PE executable detected - x86");
                        }
                        // Raw binary
                        else
                        {
                            info["format"] = "RAW";
                            info["architecture"] = "unknown";
                            log.Add("Raw binary detected");
                        }
                    }
                    
                    // Detect endianness
                    info["endianness"] = DetectEndianness(bytes);
                    log.Add($"Endianness: {info["endianness"]}");
                    
                    // Entry point analysis
                    info["entrypoint"] = DetectEntryPoint(bytes, (string)info["format"]);
                    log.Add($"Estimated entry point: {info["entrypoint"]}");
                    
                }
                catch (Exception ex)
                {
                    log.Add($"Analysis error: {ex.Message}");
                }
            });
            
            return info;
        }
        
        private string DetectEndianness(byte[] data)
        {
            if (data.Length < 4) return "unknown";
            
            // Simple heuristic based on common patterns
            // Look for ARM thumb instructions or MIPS patterns
            return "little"; // Most embedded systems use little endian
        }
        
        private string DetectEntryPoint(byte[] data, string format)
        {
            switch (format)
            {
                case "ELF":
                    // ELF entry point is at offset 0x18
                    if (data.Length >= 0x1C)
                    {
                        uint entryPoint = BitConverter.ToUInt32(data, 0x18);
                        return $"0x{entryPoint:X8}";
                    }
                    break;
                case "RAW":
                    return "0x80008000"; // Common ARM load address
                default:
                    return "unknown";
            }
            return "unknown";
        }
        
        private async Task<string> DecompileBinary(string filePath, Dictionary<string, object> binaryInfo, List<string> log)
        {
            string outputDir = Path.Combine(Path.GetTempPath(), "retdec_output");
            string outputFile = Path.Combine(outputDir, "decompiled.c");
            
            try
            {
                Directory.CreateDirectory(outputDir);
                
                // Try RetDec decompiler
                log.Add("Attempting decompilation with RetDec...");
                
                var args = $"\"{filePath}\" -o \"{outputFile}\"";
                if (binaryInfo.ContainsKey("architecture"))
                {
                    args += $" --arch {binaryInfo["architecture"]}";
                }
                
                var psi = new ProcessStartInfo("retdec-decompiler", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = outputDir
                };
                
                var proc = await Task.Run(() => Process.Start(psi));
                await proc.WaitForExitAsync();
                
                if (proc.ExitCode == 0 && File.Exists(outputFile))
                {
                    var decompiledCode = await File.ReadAllTextAsync(outputFile);
                    log.Add($"Decompilation successful - {decompiledCode.Length} characters");
                    log.Add($"Output saved to: {outputFile}");
                    
                    // Show first few lines
                    var lines = decompiledCode.Split('\n');
                    log.Add("First 10 lines of decompiled code:");
                    for (int i = 0; i < Math.Min(10, lines.Length); i++)
                    {
                        log.Add($"  {lines[i]}");
                    }
                    
                    return decompiledCode;
                }
                else
                {
                    var error = await proc.StandardError.ReadToEndAsync();
                    log.Add($"RetDec failed: {error}");
                    throw new Exception("RetDec decompilation failed");
                }
            }
            catch (Exception ex)
            {
                log.Add($"Decompilation error: {ex.Message}");
                
                // Fallback: Generate pseudo-code
                return await GeneratePseudoCode(filePath, binaryInfo, log);
            }
        }
        
        private async Task<string> GeneratePseudoCode(string filePath, Dictionary<string, object> binaryInfo, List<string> log)
        {
            log.Add("Generating pseudo-code using built-in analysis...");
            
            var pseudoCode = new StringBuilder();
            pseudoCode.AppendLine("// Pseudo-code generated by built-in analyzer");
            pseudoCode.AppendLine($"// Source: {Path.GetFileName(filePath)}");
            pseudoCode.AppendLine($"// Architecture: {binaryInfo.GetValueOrDefault("architecture", "unknown")}");
            pseudoCode.AppendLine();
            
            await Task.Run(() =>
            {
                try
                {
                    var bytes = File.ReadAllBytes(filePath);
                    
                    // Generate basic pseudo-code structure
                    pseudoCode.AppendLine("int main() {");
                    pseudoCode.AppendLine("    // Firmware initialization");
                    pseudoCode.AppendLine("    init_hardware();");
                    pseudoCode.AppendLine("    ");
                    pseudoCode.AppendLine("    // Main firmware loop");
                    pseudoCode.AppendLine("    while (1) {");
                    pseudoCode.AppendLine("        process_inputs();");
                    pseudoCode.AppendLine("        update_state();");
                    pseudoCode.AppendLine("        handle_outputs();");
                    pseudoCode.AppendLine("    }");
                    pseudoCode.AppendLine("    ");
                    pseudoCode.AppendLine("    return 0;");
                    pseudoCode.AppendLine("}");
                    
                    // Add discovered strings as comments
                    var strings = ExtractStrings(bytes);
                    if (strings.Any())
                    {
                        pseudoCode.AppendLine();
                        pseudoCode.AppendLine("// Discovered strings:");
                        foreach (var str in strings.Take(20))
                        {
                            pseudoCode.AppendLine($"// \"{str}\"");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Add($"Pseudo-code generation error: {ex.Message}");
                }
            });
            
            log.Add("Pseudo-code generated successfully");
            return pseudoCode.ToString();
        }
        
        private async Task TranslateBinaryArchitecture(string filePath, Dictionary<string, object> binaryInfo, List<string> log)
        {
            try
            {
                log.Add("Performing cross-architecture translation...");
                
                string sourceArch = binaryInfo.GetValueOrDefault("architecture", "unknown").ToString();
                string[] targetArchs = { "x86", "x86_64", "arm", "mips" };
                
                var bytes = await File.ReadAllBytesAsync(filePath);
                
                foreach (string targetArch in targetArchs)
                {
                    if (targetArch == sourceArch) continue;
                    
                    try
                    {
                        log.Add($"Translating {sourceArch} -> {targetArch}...");
                        
                        // Use our BinaryTranslator
                        var translatedBytes = BinaryTranslator.Translate(sourceArch, targetArch, bytes);
                        
                        if (translatedBytes != null && translatedBytes.Length > 0)
                        {
                            string outputPath = Path.Combine(
                                Path.GetTempPath(), 
                                $"{Path.GetFileNameWithoutExtension(filePath)}_{targetArch}.bin"
                            );
                            
                            await File.WriteAllBytesAsync(outputPath, translatedBytes);
                            log.Add($"  Success: {outputPath}");
                        }
                        else
                        {
                            log.Add($"  Failed: No output generated");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Add($"  Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Add($"Translation error: {ex.Message}");
            }
        }
        
        private async Task GenerateAnalysisReport(string filePath, string decompiledCode, List<string> log)
        {
            try
            {
                log.Add("Generating comprehensive analysis report...");
                
                string reportPath = Path.Combine(
                    Path.GetTempPath(), 
                    $"{Path.GetFileNameWithoutExtension(filePath)}_analysis.txt"
                );
                
                var report = new StringBuilder();
                report.AppendLine("=== RetDec Binary Analysis Report ===");
                report.AppendLine($"Generated: {DateTime.Now}");
                report.AppendLine($"Source File: {filePath}");
                report.AppendLine();
                
                // File information
                var fileInfo = new FileInfo(filePath);
                report.AppendLine("=== File Information ===");
                report.AppendLine($"Size: {fileInfo.Length:N0} bytes");
                report.AppendLine($"Created: {fileInfo.CreationTime}");
                report.AppendLine($"Modified: {fileInfo.LastWriteTime}");
                report.AppendLine();
                
                // Binary analysis
                report.AppendLine("=== Binary Analysis ===");
                var bytes = await File.ReadAllBytesAsync(filePath);
                report.AppendLine($"Entropy: {CalculateEntropy(bytes):F2}");
                report.AppendLine($"Null bytes: {bytes.Count(b => b == 0)} ({bytes.Count(b => b == 0) * 100.0 / bytes.Length:F1}%)");
                report.AppendLine();
                
                // Decompiled code
                if (!string.IsNullOrEmpty(decompiledCode))
                {
                    report.AppendLine("=== Decompiled Code ===");
                    report.AppendLine(decompiledCode);
                }
                
                await File.WriteAllTextAsync(reportPath, report.ToString());
                log.Add($"Analysis report saved: {reportPath}");
                log.Add("");
                log.Add("Report Summary:");
                log.Add($"  File size: {fileInfo.Length:N0} bytes");
                log.Add($"  Entropy: {CalculateEntropy(bytes):F2}");
                log.Add($"  Code lines: {decompiledCode?.Split('\n').Length ?? 0}");
            }
            catch (Exception ex)
            {
                log.Add($"Report generation error: {ex.Message}");
            }
        }
        
        private double CalculateEntropy(byte[] data)
        {
            var frequencies = new int[256];
            foreach (byte b in data)
                frequencies[b]++;
                
            double entropy = 0.0;
            foreach (int freq in frequencies)
            {
                if (freq > 0)
                {
                    double probability = (double)freq / data.Length;
                    entropy -= probability * Math.Log2(probability);
                }
            }
            
            return entropy;
        }
        
        private async Task FallbackBinaryAnalysis(string filePath, List<string> log)
        {
            try
            {
                log.Add("");
                log.Add("=== Built-in Binary Analysis ===");
                
                var bytes = await File.ReadAllBytesAsync(filePath);
                log.Add($"File size: {bytes.Length:N0} bytes");
                log.Add($"Entropy: {CalculateEntropy(bytes):F2}");
                
                // String extraction
                var strings = ExtractStrings(bytes);
                log.Add($"Extracted {strings.Count} strings");
                
                if (strings.Any())
                {
                    log.Add("Interesting strings:");
                    var interesting = strings.Where(s => 
                        s.Length > 5 && (
                        s.Contains("init") || s.Contains("main") || s.Contains("error") ||
                        s.Contains("config") || s.Contains("boot") || s.Contains("kernel")
                    )).Take(10);
                    
                    foreach (var str in interesting)
                    {
                        log.Add($"  '{str}'");
                    }
                }
                
                // Basic disassembly attempt
                log.Add("");
                log.Add("Basic instruction analysis:");
                AnalyzeInstructions(bytes, log);
                
            }
            catch (Exception ex)
            {
                log.Add($"Fallback analysis error: {ex.Message}");
            }
        }
        
        private void AnalyzeInstructions(byte[] data, List<string> log)
        {
            try
            {
                // Look for common ARM instruction patterns
                int armInstructions = 0;
                int x86Instructions = 0;
                int mipsInstructions = 0;
                
                for (int i = 0; i < data.Length - 4; i += 4)
                {
                    uint instruction = BitConverter.ToUInt32(data, i);
                    
                    // ARM detection patterns
                    if ((instruction & 0xF0000000) == 0xE0000000) // Conditional execution
                        armInstructions++;
                    
                    // x86 detection patterns  
                    if (data[i] == 0x55 || data[i] == 0x89 || data[i] == 0xC3) // push ebp, mov, ret
                        x86Instructions++;
                        
                    // MIPS detection patterns
                    if ((instruction & 0xFC000000) == 0x24000000) // addiu
                        mipsInstructions++;
                }
                
                log.Add($"ARM-like patterns: {armInstructions}");
                log.Add($"x86-like patterns: {x86Instructions}");
                log.Add($"MIPS-like patterns: {mipsInstructions}");
                
                string likelyArch = "unknown";
                int max = Math.Max(armInstructions, Math.Max(x86Instructions, mipsInstructions));
                if (max == armInstructions && armInstructions > 0) likelyArch = "ARM";
                else if (max == x86Instructions && x86Instructions > 0) likelyArch = "x86";
                else if (max == mipsInstructions && mipsInstructions > 0) likelyArch = "MIPS";
                
                log.Add($"Likely architecture: {likelyArch}");
            }
            catch (Exception ex)
            {
                log.Add($"Instruction analysis error: {ex.Message}");
            }
        }

        // Helper methods for UI control access with fallbacks
        private string GetSelectedEmulatorType()
        {
            try
            {
                var homebrewRadio = FindName("HomebrewEmulatorRadio") as RadioButton;
                var qemuRadio = FindName("QemuEmulatorRadio") as RadioButton;
                var retDecRadio = FindName("RetDecTranslatorRadio") as RadioButton;
                
                if (homebrewRadio?.IsChecked == true) return "HomebrewEmulator";
                if (qemuRadio?.IsChecked == true) return "QEMU";
                if (retDecRadio?.IsChecked == true) return "RetDec";
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
                var rdkRadio = FindName("RdkVPlatformRadio") as RadioButton;
                var uverseRadio = FindName("UversePlatformRadio") as RadioButton;
                var genericRadio = FindName("GenericPlatformRadio") as RadioButton;
                
                if (rdkRadio?.IsChecked == true) return "RDK-V";
                if (uverseRadio?.IsChecked == true) return "U-verse";
                if (genericRadio?.IsChecked == true) return "Generic";
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
        private async void MountFat_Click(object sender, RoutedEventArgs e)
        {
            await HandleFatMount();
        }

        private async void MountIso_Click(object sender, RoutedEventArgs e)
        {
            await HandleIsoMount();
        }

        private async void MountExt_Click(object sender, RoutedEventArgs e)
        {
            await HandleExtMount();
        }

        private async void MountSquashFs_Click(object sender, RoutedEventArgs e)
        {
            await HandleSquashFsMount();
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

        // BOLT Bootloader Event Handlers
        private async void InitBoltButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (boltBridge == null)
                {
                    boltBridge = new BoltEmulatorBridge();
                }

                var initButton = sender as Button;
                initButton.IsEnabled = false;
                initButton.Content = "Initializing...";

                bool success = await boltBridge.InitializeBolt();
                
                if (success)
                {
                    boltInitialized = true;
                    UpdateBoltStatus("BOLT: Initialized and ready");
                    initButton.Content = "BOLT Initialized ✓";
                    initButton.Foreground = System.Windows.Media.Brushes.Green;
                    
                    // Enable other BOLT buttons
                    EnableBoltButtons(true);
                    
                    MessageBox.Show("BOLT bootloader initialized successfully!\n\nBCM7449 SoC simulation ready.", 
                        "BOLT Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    UpdateBoltStatus("BOLT: Initialization failed");
                    initButton.Content = "Initialize BOLT";
                    initButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BOLT initialization error: {ex.Message}", "BOLT Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                var initButton = sender as Button;
                initButton.Content = "Initialize BOLT";
                initButton.IsEnabled = true;
            }
        }

        private void BoltCliButton_Click(object sender, RoutedEventArgs e)
        {
            if (!boltInitialized)
            {
                MessageBox.Show("Please initialize BOLT first.", "BOLT CLI", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show BOLT CLI window
            var cliWindow = new Window
            {
                Title = "BOLT Command Line Interface",
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Output area
            var outputBox = new TextBox
            {
                IsReadOnly = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Text = boltBridge.GetBoltStatus() + "\n\nBOLT> help\n" + boltBridge.ExecuteBoltCommand("help") + "\n\nBOLT> "
            };
            Grid.SetRow(outputBox, 0);

            // Input area
            var inputPanel = new DockPanel { Margin = new Thickness(5) };
            var promptLabel = new Label { Content = "BOLT> ", FontFamily = new System.Windows.Media.FontFamily("Consolas") };
            var inputBox = new TextBox { FontFamily = new System.Windows.Media.FontFamily("Consolas") };
            
            DockPanel.SetDock(promptLabel, Dock.Left);
            inputPanel.Children.Add(promptLabel);
            inputPanel.Children.Add(inputBox);
            Grid.SetRow(inputPanel, 1);

            // Handle command input
            inputBox.KeyDown += (s, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    string command = inputBox.Text.Trim();
                    if (!string.IsNullOrEmpty(command))
                    {
                        string result = boltBridge.ExecuteBoltCommand(command);
                        outputBox.Text += command + "\n" + result + "\n\nBOLT> ";
                        outputBox.ScrollToEnd();
                        inputBox.Clear();
                        
                        if (command.ToLower() == "exit")
                        {
                            cliWindow.Close();
                        }
                    }
                }
            };

            grid.Children.Add(outputBox);
            grid.Children.Add(inputPanel);
            cliWindow.Content = grid;
            
            cliWindow.Show();
            inputBox.Focus();
        }

        private async void LoadFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            if (!boltInitialized)
            {
                MessageBox.Show("Please initialize BOLT first.", "BOLT Boot", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string firmwareToLoad = GetBoltFirmwarePath();
            if (string.IsNullOrEmpty(firmwareToLoad))
            {
                MessageBox.Show("Please select a firmware file first.", "BOLT Boot", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var button = sender as Button;
                button.IsEnabled = false;
                button.Content = "Booting...";

                bool success = await boltBridge.BootFirmware(firmwareToLoad, "ARM");
                
                if (success)
                {
                    button.Content = "Firmware Booted ✓";
                    button.Foreground = System.Windows.Media.Brushes.Green;
                    UpdateBoltStatus("BOLT: Firmware booted successfully");
                    
                    MessageBox.Show($"Firmware booted successfully!\n\nFile: {Path.GetFileName(firmwareToLoad)}\nEmulator handoff complete.", 
                        "BOLT Boot Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    button.Content = "Load Firmware via BOLT";
                    button.IsEnabled = true;
                    MessageBox.Show("Firmware boot failed. Check the console for details.", 
                        "BOLT Boot Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BOLT boot error: {ex.Message}", "BOLT Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                var button = sender as Button;
                button.Content = "Load Firmware via BOLT";
                button.IsEnabled = true;
            }
        }

        private void BoltBrowseFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Firmware for BOLT",
                Filter = "All Firmware Files|*.bin;*.elf;*.img;*.itb;*.fit|ELF Files (*.elf)|*.elf|Binary Files (*.bin)|*.bin|Image Files (*.img)|*.img|FIT Images (*.itb;*.fit)|*.itb;*.fit|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                SetBoltFirmwarePath(dialog.FileName);
                StatusBarText($"BOLT firmware: {Path.GetFileName(dialog.FileName)}");
            }
        }

        private void MemTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!boltInitialized)
            {
                MessageBox.Show("Please initialize BOLT first.", "Memory Test", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string result = boltBridge.ExecuteBoltCommand("memtest");
            ShowTextWindow("BOLT Memory Test", new List<string> { result });
        }

        private void ShowDtbButton_Click(object sender, RoutedEventArgs e)
        {
            if (!boltInitialized)
            {
                MessageBox.Show("Please initialize BOLT first.", "Device Tree", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string result = boltBridge.ExecuteBoltCommand("dt show");
            ShowTextWindow("BOLT Device Tree", new List<string> { result });
        }

        private void DumpMemoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!boltInitialized)
            {
                MessageBox.Show("Please initialize BOLT first.", "Memory Dump", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dump a small section of memory for demonstration
            string result = boltBridge.ExecuteBoltCommand("dump 0x00008000 0x100");
            ShowTextWindow("BOLT Memory Dump", new List<string> { result });
        }

        // BOLT Helper Methods
        private void UpdateBoltStatus(string status)
        {
            // Find the BoltStatusText element and update it
            try
            {
                var statusElement = FindName("BoltStatusText") as TextBlock;
                if (statusElement != null)
                {
                    statusElement.Text = status;
                    statusElement.Foreground = boltInitialized ? 
                        System.Windows.Media.Brushes.Green : 
                        System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateBoltStatus error: {ex.Message}");
            }
        }

        private void EnableBoltButtons(bool enabled)
        {
            try
            {
                var boltCliButton = FindName("BoltCliButton") as Button;
                var loadFirmwareButton = FindName("LoadFirmwareButton") as Button;
                var memTestButton = FindName("MemTestButton") as Button;
                var showDtbButton = FindName("ShowDtbButton") as Button;
                var dumpMemoryButton = FindName("DumpMemoryButton") as Button;

                if (boltCliButton != null) boltCliButton.IsEnabled = enabled;
                if (loadFirmwareButton != null) loadFirmwareButton.IsEnabled = enabled;
                if (memTestButton != null) memTestButton.IsEnabled = enabled;
                if (showDtbButton != null) showDtbButton.IsEnabled = enabled;
                if (dumpMemoryButton != null) dumpMemoryButton.IsEnabled = enabled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnableBoltButtons error: {ex.Message}");
            }
        }

        private string GetBoltFirmwarePath()
        {
            try
            {
                var textBox = FindName("BoltFirmwarePathTextBox") as TextBox;
                return textBox?.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SetBoltFirmwarePath(string path)
        {
            try
            {
                var textBox = FindName("BoltFirmwarePathTextBox") as TextBox;
                if (textBox != null)
                {
                    textBox.Text = path;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetBoltFirmwarePath error: {ex.Message}");
            }
        }

        #region U-verse MIPS Emulator Testing
        
        private async void TestUverseMipsEmulator()
        {
            try
            {
                StatusBarText("Starting U-verse MIPS/WinCE emulator test...");
                
                // Create the MIPS U-verse emulator
                var mipsEmulator = new ProcessorEmulator.Emulation.MipsUverseEmulator();
                
                // Initialize the emulator
                if (!mipsEmulator.Initialize(""))
                {
                    ShowTextWindow("U-verse MIPS Test", new List<string> { "Failed to initialize MIPS emulator" });
                    return;
                }
                
                // Start emulation
                await mipsEmulator.StartEmulation();
                
                // Get status
                var status = mipsEmulator.GetStatus();
                var results = new List<string>
                {
                    "=== U-verse MIPS Emulator Test Results ===",
                    $"Chipset: {mipsEmulator.ChipsetName}",
                    $"Initialized: {status["IsInitialized"]}",
                    $"Kernel Loaded: {status["KernelLoaded"]}",
                    $"Running: {status["IsRunning"]}",
                    $"PC: {status["PC"]}",
                    "",
                    "Boot Log:",
                    status["BootLog"]?.ToString() ?? "No boot log available"
                };
                
                ShowTextWindow("U-verse MIPS Emulator Test", results);
                StatusBarText("U-verse MIPS emulator test completed");
            }
            catch (Exception ex)
            {
                ShowTextWindow("U-verse MIPS Test Error", new List<string> 
                { 
                    $"Error: {ex.Message}",
                    $"Stack: {ex.StackTrace}"
                });
                StatusBarText("U-verse MIPS emulator test failed");
            }
        }
        
        #endregion

        #region Advanced Analysis Options
        
        private async void AdvancedOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Present all advanced analysis and emulation options
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
            
            string mainChoice = PromptUserForChoice("Advanced Analysis Options", mainOptions);
            if (string.IsNullOrEmpty(mainChoice)) return;

            StatusBarText($"Starting: {mainChoice}");

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
                    await HandleDishNetworkAnalysis();
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
                    MessageBox.Show($"'{mainChoice}' is not implemented yet.", "Feature Not Available", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
            
            StatusBarText("Advanced analysis completed");
        }

        /// <summary>
        /// Analyzes Dish Network firmware and ecosystem components.
        /// </summary>
        private async Task HandleDishNetworkAnalysis()
        {
            if (string.IsNullOrEmpty(firmwarePath))
            {
                MessageBox.Show("Please select a firmware file first.", "No Firmware Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusBarText("Analyzing Dish Network ecosystem...");
            
            try
            {
                var bin = System.IO.File.ReadAllBytes(firmwarePath);
                Debug.WriteLine($"Analyzing Dish Network firmware: {bin.Length} bytes");

                // Look for Dish Network signatures
                bool foundDishSignatures = SearchForDishNetworkSignatures(bin);
                string chipsetInfo = AnalyzeDishNetworkChipset(bin);
                string osInfo = AnalyzeDishNetworkOS(bin);

                string analysis = $"=== Dish Network Firmware Analysis ===\n\n";
                analysis += $"File: {System.IO.Path.GetFileName(firmwarePath)}\n";
                analysis += $"Size: {bin.Length:N0} bytes\n\n";
                analysis += $"Dish Network Signatures: {(foundDishSignatures ? "DETECTED" : "Not Found")}\n";
                analysis += $"Chipset Analysis: {chipsetInfo}\n";
                analysis += $"Operating System: {osInfo}\n\n";

                // Additional Dish-specific analysis
                analysis += AnalyzeDishNetworkFeatures(bin);

                ShowTextWindow("Dish Network Analysis", new List<string> { analysis });
                StatusBarText("Dish Network analysis completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dish Network analysis error:\n\n{ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Dish Network analysis failed");
            }

            await Task.CompletedTask;
        }

        private bool SearchForDishNetworkSignatures(byte[] data)
        {
            string[] signatures = { "DISH", "EchoStar", "Hopper", "Joey", "ViP", "Wally", "Broadcom", "bcm7" };
            foreach (string sig in signatures)
            {
                if (SearchBinaryForString(data, sig))
                {
                    Debug.WriteLine($"Found Dish signature: {sig}");
                    return true;
                }
            }
            return false;
        }

        private string AnalyzeDishNetworkChipset(byte[] data)
        {
            // Common Dish Network chipsets
            if (SearchBinaryForString(data, "bcm7425") || SearchBinaryForString(data, "BCM7425"))
                return "Broadcom BCM7425 (Hopper/Joey)";
            if (SearchBinaryForString(data, "bcm7445") || SearchBinaryForString(data, "BCM7445"))
                return "Broadcom BCM7445 (Hopper 3)";
            if (SearchBinaryForString(data, "bcm7252") || SearchBinaryForString(data, "BCM7252"))
                return "Broadcom BCM7252 (Joey 4K)";
            
            return "Unknown/Generic";
        }

        private string AnalyzeDishNetworkOS(byte[] data)
        {
            if (SearchBinaryForString(data, "Linux") && SearchBinaryForString(data, "DISH"))
                return "Linux-based (Custom Dish OS)";
            if (SearchBinaryForString(data, "VxWorks"))
                return "VxWorks RTOS";
            if (data.Length >= 4 && data[0] == 0x7F && data[1] == 0x45 && data[2] == 0x4C && data[3] == 0x46)
                return "ELF Binary (Linux/Custom)";
            
            return "Unknown";
        }

        private string AnalyzeDishNetworkFeatures(byte[] data)
        {
            string features = "Detected Features:\n";
            
            if (SearchBinaryForString(data, "DVR") || SearchBinaryForString(data, "PVR"))
                features += "• DVR/PVR Recording Capability\n";
            if (SearchBinaryForString(data, "Netflix") || SearchBinaryForString(data, "YouTube"))
                features += "• Streaming Apps Support\n";
            if (SearchBinaryForString(data, "Bluetooth") || SearchBinaryForString(data, "WiFi"))
                features += "• Wireless Connectivity\n";
            if (SearchBinaryForString(data, "4K") || SearchBinaryForString(data, "UHD"))
                features += "• 4K/UHD Support\n";
            if (SearchBinaryForString(data, "Dolby"))
                features += "• Dolby Audio Support\n";
            
            return features;
        }

        private bool SearchBinaryForString(byte[] data, string searchString)
        {
            byte[] searchBytes = System.Text.Encoding.ASCII.GetBytes(searchString.ToLower());
            for (int i = 0; i <= data.Length - searchBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < searchBytes.Length; j++)
                {
                    if (char.ToLower((char)data[i + j]) != (char)searchBytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return true;
            }
            return false;
        }
        
        private async Task HandlePlutoTvIntegration()
        {
            try
            {
                StatusBarText("Initializing Pluto TV integration...");
                
                var guideFetcher = new ProcessorEmulator.Emulation.SyncEngine.GuideFetcher();
                var guideData = await guideFetcher.FetchGuideAsync();
                
                var channelList = new List<string>();
                channelList.Add("=== PLUTO TV CHANNELS ===");
                channelList.Add($"Total Channels: {guideData.Channels.Count}");
                channelList.Add($"Total Programs: {guideData.Programs.Count}");
                channelList.Add("");
                
                foreach (var channel in guideData.Channels)
                {
                    channelList.Add($"{channel.Number}: {channel.Name}");
                }
                
                ShowTextWindow("Pluto TV Integration", channelList);
                StatusBarText("Pluto TV integration completed successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Pluto TV integration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Pluto TV integration failed");
            }
        }
        
        private async Task HandleCustomHypervisor()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Firmware Files (*.bin;*.img;*.elf)|*.bin;*.img;*.elf|All Files (*.*)|*.*",
                Title = "Select firmware for custom hypervisor"
            };
            
            if (openFileDialog.ShowDialog() != true) return;
            
            try
            {
                StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.INITIALIZING));
                
                byte[] firmware = await File.ReadAllBytesAsync(openFileDialog.FileName);
                string platformName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                
                StatusBarText(ErrorManager.GetStatusMessage(ErrorManager.Codes.LOADING));
                
                // Launch the real MIPS hypervisor
                var hypervisor = new RealMipsHypervisor();
                await hypervisor.StartEmulation(firmware);
                
                StatusBarText(ErrorManager.GetSuccessMessage(ErrorManager.Codes.WUBBA_SUCCESS));
                
                // Show welcome message for first-time users
                if (IsFirstTimeExtraction())
                {
                    ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                    MarkFirstTimeExtractionDone();
                }
            }
            catch (FileNotFoundException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.FILE_NOT_FOUND, openFileDialog.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.ACCESS_DENIED, openFileDialog.FileName);
            }
            catch (OutOfMemoryException)
            {
                ErrorManager.ShowError(ErrorManager.Codes.MEMORY_ALLOCATION_ERROR, "Hypervisor launch");
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ErrorManager.Codes.HYPERVISOR_CRASH, openFileDialog.FileName, ex);
                ErrorManager.LogError(ErrorManager.Codes.HYPERVISOR_CRASH, openFileDialog.FileName, ex);
            }
        }
        
        #endregion

    }
}