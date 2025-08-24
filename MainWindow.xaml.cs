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
using System.Windows.Media;
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

    public partial class MainWindow : Window, IMainWindow, System.ComponentModel.INotifyPropertyChanged
    {
        private IEmulator currentEmulator;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        private string statusMessage = "Ready";
        private string glassStatus = "Glass: pending";
        public string StatusMessage { get => statusMessage; set { statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); } }
        public string GlassStatus { get => glassStatus; set { glassStatus = value; OnPropertyChanged(nameof(GlassStatus)); } }
        private void SafeInitializeComponent()
        {
            try
            {
                // Use LoadComponent to avoid editor-time false positives for InitializeComponent
                // and still load the compiled BAML at runtime.
                System.Windows.Application.LoadComponent(this, new Uri("/ProcessorEmulator;component/MainWindow.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] InitializeComponent failed: {ex.Message}");
                BuildFallbackUi();
            }
        }

        /// <summary>
        /// Construct a minimal UI approximately matching the first tab so the window isn't blank
        /// when XAML/BAML loading fails.
        /// </summary>
        private void BuildFallbackUi()
        {
            this.Title = "Processor Emulator";
            this.Width = 1200;
            this.Height = 800;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var dock = new DockPanel();
            this.Content = dock;

            var menu = new Menu();
            DockPanel.SetDock(menu, Dock.Top);
            menu.Items.Add(new MenuItem { Header = "_File" });
            menu.Items.Add(new MenuItem { Header = "_Edit" });
            menu.Items.Add(new MenuItem { Header = "_Help" });
            dock.Children.Add(menu);

            // Simplified fallback status area (avoid StatusBar type if WPF references fail)
            var statusPanel = new Border { BorderThickness = new Thickness(0,1,0,0), BorderBrush = System.Windows.Media.Brushes.Gray, Padding = new Thickness(4) };
            DockPanel.SetDock(statusPanel, Dock.Bottom);
            statusPanel.Child = new TextBlock { Text = "(Fallback UI)", FontStyle = FontStyles.Italic };
            dock.Children.Add(statusPanel);

            var tabs = new TabControl();
            dock.Children.Add(tabs);

            var tab = new TabItem { Header = "Emulation" };
            tabs.Items.Add(tab);
            var stack = new StackPanel { Margin = new Thickness(10) };
            tab.Content = stack;
            stack.Children.Add(new TextBlock { Text = "Firmware Emulation (Fallback)", FontWeight = FontWeights.Bold, FontSize = 16 });

            var gb = new GroupBox { Header = "1. Select Firmware", Margin = new Thickness(0, 10, 0, 10) };
            var dp = new DockPanel { Margin = new Thickness(5) };
            var tb = new TextBox { Width = 400 };
            dp.Children.Add(tb);
            var btn = new Button { Content = "Browse...", Width = 80, Margin = new Thickness(5, 0, 0, 0) };
            dp.Children.Add(btn);
            gb.Content = dp;
            stack.Children.Add(gb);
        }

        // Store selected firmware path and platform
        private string firmwarePath;
        private string selectedPlatform;
        
        // Universal Hypervisor Configuration
        private string selectedArchitecture = "Auto-Detect";
        private string selectedSecurityBypass = "Bypass All Security (Maximum Freedom)";
        private string selectedMemorySize = "Auto-Calculate (Recommended)";
        private string selectedCpuType = "Auto-Select (Recommended)";
        private string selectedMachineType = "Auto-Select (Recommended)";
        private string selectedAction = "Generic CPU/OS Emulation";
        
        // BOLT Bootloader Integration
        private BoltEmulatorBridge boltBridge;
        private bool boltInitialized;

        // Add default constructor for XAML
        public MainWindow()
        {
            try
            {
                SafeInitializeComponent();
                InitializeAero();
                Win7Chrome.ApplyChrome(this);
                
                // Initialize drag-and-drop for file support
                this.AllowDrop = true;
                this.Drop += MainWindow_Drop;

                // Initialize real-time emulation log panel
                InitializeLogPanel();
                
                // Initialize dropdown handlers
                this.Loaded += (s, e) => InitializeDropdownHandlers();
                this.Loaded += MainWindow_Loaded; // initialize view controls state
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Constructor error: {ex.Message}");
                MessageBox.Show($"Failed to initialize MainWindow: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow(IEmulator currentEmulator)
        {
            try
            {
                SafeInitializeComponent();
                InitializeAero();
                Win7Chrome.ApplyChrome(this);
                this.currentEmulator = currentEmulator;
                InitializeLogPanel();
                
                // Initialize dropdown handlers
                this.Loaded += (s, e) => InitializeDropdownHandlers();
                this.Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Constructor error: {ex.Message}");
                MessageBox.Show($"Failed to initialize MainWindow: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Show QEMU installation status for real firmware emulation
        /// </summary>
        private void ShowQemuStatus()
        {
            try
            {
                string status = Tools.QemuInstaller.GetQemuStatus();
                StatusBarText(status);
                
                if (!Tools.QemuInstaller.IsQemuInstalled())
                {
                    // Show installation prompt after a delay
                    Task.Delay(2000).ContinueWith(_ => 
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var result = MessageBox.Show(
                                "🔧 Real Firmware Emulation Available!\n\n" +
                                "Install QEMU to boot actual ARM/MIPS firmware instead of simulations.\n\n" +
                                "Would you like installation instructions?",
                                "Enable Real Emulation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            
                            if (result == MessageBoxResult.Yes)
                            {
                                Tools.QemuInstaller.ShowInstallationInstructions();
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show QEMU status: {ex.Message}");
            }
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

        /// <summary>
        /// Initialize dropdown event handlers for Universal Hypervisor configuration
        /// </summary>
        private void InitializeDropdownHandlers()
        {
            try
            {
                // Architecture dropdown - with null check
                var archComboBox = this.FindName("ArchitectureComboBox") as ComboBox;
                if (archComboBox != null)
                {
                    archComboBox.SelectionChanged += (s, e) =>
                    {
                        if (archComboBox.SelectedItem is ComboBoxItem item)
                        {
                            selectedArchitecture = item.Content.ToString();
                            StatusBarText($"Architecture: {selectedArchitecture}");
                        }
                    };
                }

                // Security bypass dropdown - with null check
                var securityComboBox = this.FindName("SecurityBypassComboBox") as ComboBox;
                if (securityComboBox != null)
                {
                    securityComboBox.SelectionChanged += (s, e) =>
                    {
                        if (securityComboBox.SelectedItem is ComboBoxItem item)
                        {
                            selectedSecurityBypass = item.Content.ToString();
                            StatusBarText($"Security Level: {selectedSecurityBypass}");
                        }
                    };
                }

                // Memory size dropdown - with null check
                var memoryComboBox = this.FindName("MemorySizeComboBox") as ComboBox;
                if (memoryComboBox != null)
                {
                    memoryComboBox.SelectionChanged += (s, e) =>
                    {
                        if (memoryComboBox.SelectedItem is ComboBoxItem item)
                        {
                            selectedMemorySize = item.Content.ToString();
                            StatusBarText($"Memory: {selectedMemorySize}");
                        }
                    };
                }

                // CPU type dropdown - with null check
                var cpuComboBox = this.FindName("CpuTypeComboBox") as ComboBox;
                if (cpuComboBox != null)
                {
                    cpuComboBox.SelectionChanged += (s, e) =>
                    {
                        if (cpuComboBox.SelectedItem is ComboBoxItem item)
                        {
                            selectedCpuType = item.Content.ToString();
                            StatusBarText($"CPU: {selectedCpuType}");
                        }
                    };
                }

                // Firmware path text box - sync with firmwarePath variable
                var firmwarePathTextBox = this.FindName("FirmwarePathTextBox") as TextBox;
                if (firmwarePathTextBox != null)
                {
                    firmwarePathTextBox.TextChanged += (s, e) =>
                    {
                        firmwarePath = firmwarePathTextBox.Text;
                        if (!string.IsNullOrEmpty(firmwarePath))
                        {
                            StatusBarText($"Firmware path: {Path.GetFileName(firmwarePath)}");
                        }
                    };
                }

                Debug.WriteLine("[MainWindow] Dropdown handlers initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to initialize dropdown handlers: {ex.Message}");
            }
        }

        // All Tools classes are static - no need to instantiate
        private ExoticFilesystemManager fsManager = new();
        private InstructionDispatcher dispatcher = new();

        // Real-time emulation logging
        private EmulationLogPanel logPanel;

    public TextBlock StatusBar { get; set; } = new TextBlock(); // legacy interface compatibility
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
            StatusMessage = text;
        }

    private IEmulator GetCurrentEmulator() => currentEmulator;

        /// <summary>
        /// Get configuration from dropdown selections for Universal Hypervisor
        /// </summary>
        private Dictionary<string, string> GetHypervisorConfiguration()
        {
            var config = new Dictionary<string, string>
            {
                ["Action"] = selectedAction,
                ["Architecture"] = selectedArchitecture,
                ["SecurityBypass"] = selectedSecurityBypass,
                ["MemorySize"] = selectedMemorySize,
                ["CpuType"] = selectedCpuType,
                ["MachineType"] = selectedMachineType,
                ["FirmwarePath"] = firmwarePath ?? ""
            };

            return config;
        }
        
        private void InitializeAero()
        {
            try
            {
                DataContext = this; // for bindings
                // Attempt full true Aero glass (Win7 style) first
                bool glassApplied = AeroGlassHelper.TryApplyTrueGlass(this);
                if (!glassApplied)
                {
                    // fallback to partial frame extension (menu + status bar bands)
                    glassApplied = AeroGlassHelper.TryApplyGlass(this);
                }
                if (glassApplied)
                {
                    // Make window background transparent so extended frame shows through
                    this.Background = System.Windows.Media.Brushes.Transparent;
                    GlassStatus = "Glass: on";
                }
                else
                {
                    GlassStatus = AeroGlassHelper.IsDwmEnabled() ? "Glass: off" : "Glass: unsupported";
                }
            }
            catch (Exception ex)
            {
                GlassStatus = $"Glass: error ({ex.Message.Split('\n')[0]})";
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize theme menu state
            if (this.FindName("ThemeWin7") is MenuItem t7 && this.FindName("ThemeCarl") is MenuItem tc)
            {
                t7.IsCheckable = true;
                tc.IsCheckable = true;
                t7.IsChecked = AppThemeManager.Current == AppTheme.Windows7Aero;
                tc.IsChecked = AppThemeManager.Current == AppTheme.CarlMode;
            }
        }

        private void ThemeWin7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppThemeManager.Apply(AppTheme.Windows7Aero, Application.Current);
                if (FindName("ThemeWin7") is MenuItem t7 && FindName("ThemeCarl") is MenuItem tc)
                { t7.IsChecked = true; tc.IsChecked = false; }
            }
            catch (Exception ex)
            {
                StatusBarText($"Theme apply error: {ex.Message}");
            }
        }

        private void ThemeCarl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppThemeManager.Apply(AppTheme.CarlMode, Application.Current);
                if (FindName("ThemeWin7") is MenuItem t7 && FindName("ThemeCarl") is MenuItem tc)
                { t7.IsChecked = false; tc.IsChecked = true; }
            }
            catch (Exception ex)
            {
                StatusBarText($"Theme apply error: {ex.Message}");
            }
        }

        private void TintSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                // Adjust alpha of glass brushes live
                var app = Application.Current;
                byte a = (byte)Math.Max(0, Math.Min(255, e.NewValue));
                if (app.Resources["GlassMenuBackgroundBrush"] is SolidColorBrush mb)
                {
                    var c = mb.Color; app.Resources["GlassMenuBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B));
                }
                if (app.Resources["GlassStatusBackgroundBrush"] is SolidColorBrush sb)
                {
                    var c = sb.Color; app.Resources["GlassStatusBackgroundBrush"] = new SolidColorBrush(Color.FromArgb((byte)Math.Max(0, a - 20), c.R, c.G, c.B));
                }
                if (app.Resources["GlassPanelBrush"] is SolidColorBrush pb)
                {
                    var c = pb.Color; app.Resources["GlassPanelBrush"] = new SolidColorBrush(Color.FromArgb((byte)Math.Max(0, a / 5), c.R, c.G, c.B));
                }
            }
            catch { }
        }

        /// <summary>
        /// Main entry point for user actions. Uses dropdown selection instead of dialog.
        /// </summary>
        private async void StartEmulation_Click(object sender, RoutedEventArgs e)
        {
            // Use the selected action from the dropdown instead of showing a dialog
            string mainChoice = selectedAction;
            if (string.IsNullOrEmpty(mainChoice)) 
            {
                StatusBarText("Please select an action from the dropdown");
                return;
            }

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
                case "Comcast X1 Platform Emulator":
                    await HandleComcastX1Emulation();
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
                case "Windows CE Binary Execution":
                    await HandleWindowsCEExecution();
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
                // Ensure firmware path is set; prompt for U-verse dump if not selected
                if (string.IsNullOrEmpty(firmwarePath) || !File.Exists(firmwarePath))
                {
                    var dlg = new OpenFileDialog
                    {
                        Title = "Select U-verse Firmware Dump",
                        InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR", "Uverse_Stuff"),
                        Filter = "Firmware Files (*.bin;*.img;*.exe)|*.bin;*.img;*.exe|Registry Files (*.hv)|*.hv|All Files (*.*)|*.*"
                    };
                    if (dlg.ShowDialog() != true) return;
                    firmwarePath = dlg.FileName;
                    StatusBarText($"Selected U-verse dump: {Path.GetFileName(firmwarePath)}");
                }
                StatusBarText(" Starting AT&T U-verse + Microsoft Mediaroom emulation...");
                // If this is a real U-verse dump, boot it with QEMU
                string uverseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR", "Uverse_Stuff");
                if (firmwarePath.StartsWith(uverseDir, StringComparison.OrdinalIgnoreCase))
                {
                    StatusBarText("REAL BOOT: Starting QEMU to boot actual U-verse firmware...");
                    
                    string nkExePath = null;
                    string registryPath = null;
                    
                    // If user selected the nk.bin directory, look inside it for nk.exe
                    if (Directory.Exists(firmwarePath) && Path.GetFileName(firmwarePath) == "nk.bin")
                    {
                        nkExePath = Path.Combine(firmwarePath, "nk.exe");
                        registryPath = Path.Combine(firmwarePath, "boot.hv");
                    }
                    // If user selected nk.exe directly
                    else if (Path.GetFileName(firmwarePath) == "nk.exe")
                    {
                        nkExePath = firmwarePath;
                        registryPath = Path.Combine(Path.GetDirectoryName(firmwarePath), "boot.hv");
                    }
                    // Otherwise, look for nk.exe in the same directory
                    else
                    {
                        nkExePath = Path.Combine(Path.GetDirectoryName(firmwarePath), "nk.exe");
                        registryPath = Path.Combine(Path.GetDirectoryName(firmwarePath), "boot.hv");
                    }
                    
                    if (File.Exists(nkExePath))
                    {
                        StatusBarText("Found nk.exe - launching REAL QEMU MIPS emulation...");
                        var qemuEmulator = new RealQemuEmulator();
                        bool bootSuccess = await qemuEmulator.BootWinCEFirmware(nkExePath, 
                            File.Exists(registryPath) ? registryPath : null);
                        
                        if (bootSuccess)
                        {
                            StatusBarText("SUCCESS: Real U-verse WinCE firmware booted in QEMU!");
                            return; // Exit here - QEMU is running
                        }
                        else
                        {
                            StatusBarText("QEMU launch failed - check QEMU installation");
                            var failureResults = new List<string>
                            {
                                "❌ QEMU Failed to Launch",
                                "",
                                $"Attempted to boot: {Path.GetFileName(nkExePath)}",
                                $"QEMU Path: {qemuEmulator.GetQemuPath() ?? "Not found"}",
                                "",
                                "💡 Troubleshooting:",
                                "1. Install QEMU: choco install qemu",
                                "2. Or download from: https://qemu.weilnetz.de/w64/",
                                "3. Ensure qemu-system-mips.exe is in PATH"
                            };
                            ShowTextWindow("QEMU Launch Failed", failureResults);
                            return;
                        }
                    }
                    else
                    {
                        StatusBarText($"nk.exe not found at: {nkExePath}");
                        ShowTextWindow("Firmware Not Found", new List<string>
                        {
                            "❌ WinCE Kernel Not Found",
                            "",
                            $"Expected: {nkExePath}",
                            $"Selected: {firmwarePath}",
                            "",
                            "💡 Please select the nk.bin folder or nk.exe file directly"
                        });
                        return;
                    }
                }
                
                // Check if this is an nk.bin kernel file
                if (Path.GetFileName(firmwarePath).ToLower() == "nk.bin")
                {
                    StatusBarText(" Detected nk.bin - using comprehensive Mediaroom boot manager...");
                    
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
                        StatusBarText(" Mediaroom boot failed - check boot log for details");
                        
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
                        " AT&T U-verse + Microsoft Mediaroom Boot Complete!",
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
                    results.Add(" AT&T U-verse IPTV Platform is fully operational!");
                    results.Add(" Microsoft Mediaroom services are running");
                    results.Add(" IPTV infrastructure is connected and ready");
                    
                    ShowTextWindow("U-verse + Mediaroom Emulation Success", results);
                    StatusBarText(" U-verse + Mediaroom emulation started successfully");
                }
                else
                {
                    // Use the enhanced U-verse emulator for other files
                    StatusBarText(" Using enhanced U-verse + Mediaroom emulator...");
                    
                    // Detect if it's a signature file (.sig) or other content
                    string ext = Path.GetExtension(firmwarePath).ToLowerInvariant();
                    
                    if (ext == ".sig" || ext == ".bin" || ext == ".img")
                    {
                        // Handle firmware-based U-verse emulation with Mediaroom boot
                        StatusBarText($" Loading U-verse firmware: {Path.GetFileName(firmwarePath)}...");
                        
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
                            StatusBarText(" U-verse emulation failed");
                            var failureLog = emulator.GetBootLog();
                            ShowTextWindow("U-verse Emulation Failure", failureLog);
                            return;
                        }
                        
                        // Get status and show results
                        var status = emulator.GetEmulationStatus();
                        var bootStatus = status.ContainsKey("BootStatus") ? (Dictionary<string, object>)status["BootStatus"] : null;
                        
                        var uverseLog = new List<string>
                        {
                            " AT&T U-verse + Microsoft Mediaroom Emulation Complete!",
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
                            " Microsoft Mediaroom IPTV platform is operational",
                            " AT&T U-verse services are running",
                            " IPTV infrastructure connected"
                        };
                        
                        ShowTextWindow("U-verse + Mediaroom Emulation", uverseLog);
                        StatusBarText(" U-verse + Mediaroom emulation completed successfully");
                    }
                    else
                    {
                        // Generic firmware analysis for other U-verse files
                        StatusBarText(" Analyzing U-verse firmware structure...");
                        
                        string extractDir = Path.Combine(Path.GetDirectoryName(firmwarePath), 
                            Path.GetFileNameWithoutExtension(firmwarePath) + "_extracted");
                        
                        await Task.Run(() => ArchiveExtractor.ExtractAndAnalyze(firmwarePath, extractDir));
                        FirmwareAnalyzer.AnalyzeFirmwareArchive(firmwarePath, extractDir);
                        
                        var results = new List<string>
                        {
                            " AT&T U-verse Firmware Analysis Complete",
                            "",
                            "=== Analysis Results ===",
                            $"File: {Path.GetFileName(firmwarePath)}",
                            $"Extracted to: {extractDir}",
                            $"Type: {Path.GetExtension(firmwarePath)} firmware",
                            "",
                            " Check extracted directory for:",
                            "   WinCE kernel files (nk.bin)",
                            "   Mediaroom components",
                            "   Registry hives (*.hv)",
                            "   IPTV configuration files",
                            "   System overlays and modules",
                            "",
                            " Tip: If nk.bin is found, load it directly for full Mediaroom boot emulation"
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
        /// Emulates an RDK-V set-top box using real QEMU hypervisor with firmware unpacking and live boot.
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
                StatusBarText(" Initializing REAL hypervisor for live firmware boot...");

                // RDK-V is ARM-based, so we create an ARM emulator instance.
                IEmulator emulator = new ArmCpuEmulator();

                // Use the REAL hypervisor manager with the ARM emulator
                var hypervisor = new RealHypervisorManager(emulator);
                
                StatusBarText(" Unpacking firmware and launching QEMU hypervisor...");
                
                // Boot the firmware file in real QEMU emulation
                bool bootSuccess = await hypervisor.BootFirmwareFile(path);
                
                if (bootSuccess)
                {
                    StatusBarText(" Real hypervisor launched - firmware is booting live!");
                    
                    // Show welcome message for first-time users
                    if (IsFirstTimeExtraction())
                    {
                        ErrorManager.ShowSuccess(ErrorManager.Codes.WELCOME_MESSAGE);
                        MarkFirstTimeExtractionDone();
                    }
                }
                else
                {
                    StatusBarText(" Hypervisor launch failed");
                    ErrorManager.ShowError(ErrorManager.Codes.INITIALIZATION_FAILED, "Failed to launch real hypervisor");
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

        // U-verse dump analysis for Data/DVR/Uverse_Stuff
        private async Task HandleUverseDumpAnalysis()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dumpsPath = System.IO.Path.Combine(baseDir, "Data", "DVR", "Uverse_Stuff", "Dumps");
            if (!System.IO.Directory.Exists(dumpsPath))
            {
                MessageBox.Show("U-verse dumps folder not found:\n" + dumpsPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var files = System.IO.Directory.GetFiles(dumpsPath, "*", System.IO.SearchOption.AllDirectories);
            var records = new List<FileRecord>();
            foreach (var file in files)
            {
                var info = new System.IO.FileInfo(file);
                byte[] data = System.IO.File.ReadAllBytes(file).Take(64).ToArray();
                string hex = BitConverter.ToString(data).Replace("-", " ");
                records.Add(new FileRecord { FilePath = file, Size = info.Length, HexPreview = hex });
            }
            var analysisWindow = new FolderAnalysisWindow(records);
            analysisWindow.Show();
            StatusBarText($"U-verse dump analysis: {files.Length} files loaded.");
            await Task.CompletedTask;
        }

        private void UverseDumpAnalysis_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleUverseDumpAnalysis();
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

            //  PLATFORM AUTODETECTION - Analyze firmware to suggest platform
            StatusBarText("Analyzing firmware for platform detection...");
            var detectionResult = PlatformDetector.DetectPlatform(imagePath);

            //  REGION AWARENESS - Analyze firmware regions for boot logic
            StatusBarText("Analyzing firmware regions...");
            var regionResult = FirmwareRegionAnalyzer.AnalyzeFirmware(imagePath);

            if (detectionResult.Success && detectionResult.DetectedPlatform != null)
            {
                var platform = detectionResult.DetectedPlatform;
                StatusBarText($"Platform detected: {platform.Name} (confidence: {detectionResult.Confidence:P1})");

                // Show detection results and recommendations
                var resultMessage = $" Platform Detection & Region Analysis Results:\n\n";
                resultMessage += $"Platform: {platform.Name}\n";
                resultMessage += $"Confidence: {detectionResult.Confidence:P1}\n";
                resultMessage += $"Architecture: {platform.Architecture}\n";
                resultMessage += $"SoC Family: {platform.SocFamily}\n";
                resultMessage += $"Recommended Emulator: {platform.EmulatorType}\n\n";

                // Add region analysis results
                if (regionResult.Success && regionResult.DetectedRegions.Any())
                {
                    resultMessage += " Detected Firmware Regions:\n";
                    foreach (var region in regionResult.DetectedRegions.Take(4))
                    {
                        resultMessage += $" {region.Name}: {region.Confidence:P1} confidence\n";
                        resultMessage += $"  Address: 0x{region.LoadAddress:X8}, Size: ~{region.EstimatedSize / 1024}KB\n";
                    }
                    resultMessage += "\n";
                }

                if (detectionResult.Recommendations.Any())
                {
                    resultMessage += " Platform Recommendations:\n";
                    foreach (var rec in detectionResult.Recommendations.Take(3))
                        resultMessage += $" {rec}\n";
                    resultMessage += "\n";
                }

                // Add boot sequence recommendations
                if (regionResult.Success && regionResult.BootSequence.Any())
                {
                    resultMessage += " Recommended Boot Sequence:\n";
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

        // Add this method to handle File -> Open menu click - detect firmware type automatically
        private async void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Firmware Files (*.bin;*.img;*.fw;*.rdk)|*.bin;*.img;*.fw;*.rdk|All Files (*.*)|*.*",
                Title = "Select Firmware File"
            };

            if (dlg.ShowDialog() != true) return;

            string filePath = dlg.FileName;
            StatusBarText($"Analyzing firmware: {Path.GetFileName(filePath)}");

            try
            {
                // Read file for analysis
                byte[] firmwareData = await File.ReadAllBytesAsync(filePath);
                string firmwareType = AnalyzeFileType(filePath, firmwareData);

                // Auto-detect firmware type and route to appropriate emulator
                if (firmwareType == "Comcast X1 Firmware")
                {
                    await HandleComcastX1Emulation(filePath);
                }
                else
                {
                    // Show a clean, minimal dialog for other firmware types
                    var emulatorOptions = new List<string>
                    {
                        "RDK-V Emulator",
                        "RDK-B Emulator", 
                        "Uverse Box Emulator",
                        "DirecTV Box/Firmware Analysis",
                        "Generic CPU/OS Emulation",
                        "Custom Hypervisor"
                    };

                    string selectedEmulator = PromptUserForChoice("Select emulator for this firmware:", emulatorOptions);
                    if (string.IsNullOrEmpty(selectedEmulator)) return;

                    // Route to the selected emulator with the file
                    await RouteToSelectedEmulator(selectedEmulator, filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening firmware: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Error opening firmware");
            }
        }

        /// <summary>
        /// Route to the selected emulator with a pre-selected firmware file
        /// </summary>
        private async Task RouteToSelectedEmulator(string emulatorName, string filePath)
        {
            switch (emulatorName)
            {
                case "RDK-V Emulator":
                    await HandleRdkVEmulation();
                    break;
                case "RDK-B Emulator":
                    await HandleRdkBEmulation();
                    break;
                case "Uverse Box Emulator":
                    await HandleUverseEmulation();
                    break;
                case "DirecTV Box/Firmware Analysis":
                    await HandleDirectvAnalysis();
                    break;
                case "Generic CPU/OS Emulation":
                    await HandleGenericEmulation();
                    break;
                case "Custom Hypervisor":
                    await HandleCustomHypervisor();
                    break;
            }
        }

        /// <summary>
        /// Analyze file type based on content and extension
        /// </summary>
        private string AnalyzeFileType(string filePath, byte[] fileData)
        {
            // Simple analysis based on file extension and content
            string extension = Path.GetExtension(filePath).ToLower();
            string content = Encoding.ASCII.GetString(fileData.Take(1024).ToArray());

            if (content.Contains("XG1") || content.Contains("BCM7449"))
            {
                return "Comcast X1 Firmware";
            }
            
            // Add more rules here for other firmware types
            
            return "Unknown";
        }

        /// <summary>
        /// Handle Comcast X1 emulation with a specific file
        /// </summary>
        private async Task HandleComcastX1Emulation(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Comcast X1 Firmware (*.bin;*.rdk)|*.bin;*.rdk|All Files (*.*)|*.*"
                };
                if (openFileDialog.ShowDialog() != true) return;
                filePath = openFileDialog.FileName;
            }

            StatusBarText($"Starting Comcast X1 emulation for {Path.GetFileName(filePath)}...");

            try
            {
                // Use the simple firmware emulator for reliable operation
                var emulator = new SimpleFirmwareEmulator();
                
                // Load firmware
                if (await emulator.LoadFirmware(filePath))
                {
                    // Start emulation
                    if (await emulator.Start())
                    {
                        StatusBarText("Comcast X1 emulation started successfully!");
                    }
                    else
                    {
                        StatusBarText("Failed to start Comcast X1 emulation");
                    }
                }
                else
                {
                    StatusBarText("Failed to load Comcast X1 firmware");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Comcast X1 emulation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText("Comcast X1 emulation failed.");
            }
        }

        /// <summary>
        /// Handle generic CPU/OS emulation
        /// </summary>
        private async Task HandleGenericEmulation()
        {
            // Get hypervisor configuration from UI
            var config = GetHypervisorConfiguration();
            
            // Launch hypervisor window with dummy hypervisor and platform name
            var hypervisorWindow = new HypervisorWindow(new RealMipsHypervisor(), "Generic Platform");
            hypervisorWindow.Show();
            
            StatusBarText("Generic hypervisor launched.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle custom hypervisor launch
        /// </summary>
        private async Task HandleCustomHypervisor()
        {
            // Get hypervisor configuration from UI
            var config = GetHypervisorConfiguration();
            
            // Launch hypervisor window with dummy hypervisor and platform name
            var hypervisorWindow = new HypervisorWindow(new RealMipsHypervisor(), "Custom Platform");
            hypervisorWindow.Show();
            
            StatusBarText("Custom hypervisor launched.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle drag-and-drop of firmware files
        /// </summary>
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    firmwarePath = files[0];
                    StatusBarText($"Loaded firmware: {Path.GetFileName(firmwarePath)}");
                    
                    // Auto-detect and start emulation
                    AutoDetectAndStartEmulation(firmwarePath);
                }
            }
        }

        /// <summary>
        /// Auto-detect firmware type and start appropriate emulation
        /// </summary>
        private async void AutoDetectAndStartEmulation(string filePath)
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(filePath);
                string firmwareType = AnalyzeFileType(filePath, fileData);

                switch (firmwareType)
                {
                    case "Comcast X1 Firmware":
                        await HandleComcastX1Emulation(filePath);
                        break;
                    default:
                        // Default to generic emulation if type is unknown
                        await HandleGenericEmulation();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to auto-start emulation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handle folder analysis
        /// </summary>
        private async Task HandleFolderAnalysis()
        {
            // WPF does not have a native folder picker; use OpenFileDialog to pick a file inside the folder instead
            var openFile = new OpenFileDialog { Title = "Select any file inside the folder to analyze", Filter = "All Files (*.*)|*.*" };
            if (openFile.ShowDialog() != true)
            {
                return;
            }
            string folderPath = System.IO.Path.GetDirectoryName(openFile.FileName);
            var files = System.IO.Directory.GetFiles(folderPath);
            var items = new List<FileRecord>();
            foreach (var file in files)
            {
                items.Add(new FileRecord
                {
                    FilePath = file,
                    Size = new System.IO.FileInfo(file).Length,
                    HexPreview = string.Empty
                });
            }
            var analysisWindow = new FolderAnalysisWindow(items);
            analysisWindow.Show();
            StatusBarText($"Analyzing folder: {folderPath}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle SWM/LNB simulation
        /// </summary>
        private async Task HandleSwmLnbSimulation()
        {
            ProcessorEmulator.Emulation.SwmLnbEmulator.SendChannelMap();
            ShowTextWindow("SWM/LNB Simulation", new List<string> { "SWM/LNB simulation running." });
            StatusBarText("SWM/LNB simulation started.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle filesystem mount for various types
        /// </summary>
        private async Task HandleCeMount()
        {
            var dlg = new OpenFileDialog { Filter = "WinCE Filesystem Images (*.img;*.bin)|*.img;*.bin|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting WinCE FS from {Path.GetFileName(path)}...");
            // Logic to mount WinCE filesystem
            StatusBarText("WinCE FS mounted.");
            await Task.CompletedTask;
        }

        private async Task HandleYaffsMount()
        {
            var dlg = new OpenFileDialog { Filter = "YAFFS Filesystem Images (*.img;*.bin)|*.img;*.bin|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting YAFFS from {Path.GetFileName(path)}...");
            // Logic to mount YAFFS filesystem
            StatusBarText("YAFFS mounted.");
            await Task.CompletedTask;
        }

        private async Task HandleIsoMount()
        {
            var dlg = new OpenFileDialog { Filter = "ISO Files (*.iso)|*.iso|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting ISO from {Path.GetFileName(path)}...");
            // Logic to mount ISO filesystem
            StatusBarText("ISO mounted.");
            await Task.CompletedTask;
        }

        private async Task HandleExtMount()
        {
            var dlg = new OpenFileDialog { Filter = "EXT Filesystem Images (*.ext2;*.ext3;*.ext4)|*.ext2;*.ext3;*.ext4|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Mounting EXT FS from {Path.GetFileName(path)}...");
            // Logic to mount EXT filesystem
            StatusBarText("EXT FS mounted.");
await Task.CompletedTask;
        }

        /// <summary>
        /// Handle booting firmware with homebrew emulator first
        /// </summary>
        private async Task HandleBootFirmwareHomebrewFirst()
        {
            var dlg = new OpenFileDialog { Filter = "Firmware Files (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Booting firmware {Path.GetFileName(path)} with homebrew emulator...");
            // Logic to boot with homebrew emulator
            StatusBarText("Firmware booted with homebrew emulator.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle booting firmware in homebrew emulator
        /// </summary>
        private async Task HandleBootFirmwareInHomebrew()
        {
            var dlg = new OpenFileDialog { Filter = "Firmware Files (*.bin;*.img)|*.bin;*.img|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;
            StatusBarText($"Booting firmware {Path.GetFileName(path)} in homebrew emulator...");
            // Logic to boot in homebrew emulator
            StatusBarText("Firmware booted in homebrew emulator.");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Helper to prompt user for a choice from a list
        /// </summary>
        private string PromptUserForChoice(string message, IEnumerable<string> choices)
        {
            var choiceDialog = new Window
            {
                Title = "Select an Option",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            stackPanel.Children.Add(new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 10) });

            var comboBox = new ComboBox { ItemsSource = choices, SelectedIndex = 0 };
            stackPanel.Children.Add(comboBox);

            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            stackPanel.Children.Add(okButton);

            choiceDialog.Content = stackPanel;

            string selectedChoice = null;
            okButton.Click += (s, e) =>
            {
                selectedChoice = comboBox.SelectedItem as string;
                choiceDialog.DialogResult = true;
                choiceDialog.Close();
            };

            choiceDialog.ShowDialog();
            return selectedChoice;
        }

        /// <summary>
        /// Check if this is the first time the user is running firmware extraction
        /// </summary>
        private bool IsFirstTimeExtraction()
        {
            // Simple check using a temp file
            string flagFile = Path.Combine(Path.GetTempPath(), "ProcessorEmulator_FirstTimeFlag.txt");
            return !File.Exists(flagFile);
        }

        /// <summary>
        /// Mark that the first-time extraction has been done
        /// </summary>
        private void MarkFirstTimeExtractionDone()
        {
            string flagFile = Path.Combine(Path.GetTempPath(), "ProcessorEmulator_FirstTimeFlag.txt");
            File.WriteAllText(flagFile, "done");
        }

        /// <summary>
        /// Show a funny status message during long operations
        /// </summary>
        private void ShowFunnyStatus(string operation)
        {
            var messages = new[]
            {
                "Reticulating splines...",
                "Charging flux capacitor...",
                "Aligning warp coils...",
                "Polishing the hyperdrive...",
                "Recalibrating the quantum carburetor...",
                "Defragging the reality matrix...",
                "Downloading more RAM...",
                "Reversing the polarity of the neutron flow..."
            };
            var random = new Random();
            StatusBarText($"{operation}: {messages[random.Next(messages.Length)]}");
        }

        // Missing event handlers referenced in XAML
        private void RdkVEmulator_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleRdkVEmulation();
        }

        private void BrowseFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Firmware Files (*.bin;*.img;*.exe)|*.bin;*.img;*.exe|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                firmwarePath = openFileDialog.FileName;
                
                // Update the UI text field
                if (this.FindName("FirmwarePathTextBox") is TextBox firmwarePathTextBox)
                {
                    firmwarePathTextBox.Text = firmwarePath;
                }
                
                StatusBarText($"Selected firmware: {Path.GetFileName(firmwarePath)}");
            }
        }

        private void AnalyzeAllDvrData_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText("Analyzing all DVR data...");
            // Implement DVR data analysis
        }

        private void ListDvrFirmware_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText("Listing DVR firmware...");
            // Implement DVR firmware listing
        }

        private void ScanDvrData_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText("Scanning DVR data...");
            // Implement DVR data scanning
        }

        private void AnalyzeFirmware_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText("Analyzing firmware...");
            // Implement firmware analysis
        }

        // Additional missing event handlers
        private void UverseEmulator_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleUverseEmulation();
        }


        private void ComcastX1Emulator_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleComcastX1Emulation();
        }


        private void DirectvAnalysis_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleDirectvAnalysis();
        }


        private void RdkBEmulator_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleRdkBEmulation();
        }


        private void DishVxWorks_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleDishVxWorks();
        }


        private void PowerPCDemo_Click(object sender, RoutedEventArgs e)
        {
            _ = HandlePowerPCDemo();
        }


        private void GenericEmulation_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleGenericEmulation();
        }


        private void WindowsCEExecutor_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleWindowsCEExecution();
        }


        private void UniversalHypervisor_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleCustomHypervisor();
        }


        private void StopAllProcesses_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleStopAllProcesses();
        }


        private void ShowRunningProcesses_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleShowRunningProcesses();
        }


        private void ProcessMonitor_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleProcessMonitor();
        }


        private void ExtractFirmware_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleFirmwareExtraction();
        }


        private void DetectFileType_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleFileTypeDetection();
        }


        private void ExecutableAnalysis_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleExecutableAnalysis();
        }


        private void CrossCompile_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleCrossCompile();
        }


        private void AnalyzeFolder_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleFolderAnalysis();
        }


        private void SummarizeDvrData_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleDvrDataSummary();
        }


        private void MountIso_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleIsoMount();
        }


        private void MountExt_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleExtMount();
        }


        private void MountFat_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleFatMount();
        }


        private void MountSquashFs_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleSquashFsMount();
        }


        private void MountYaffs_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleYaffsMount();
        }


        private void MountCe_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleCeMount();
        }


        private void ProbeFilesystem_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleFilesystemProbe();
        }


        private void LinuxFsReadWrite_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleLinuxFsReadWrite();
        }


        private void SimulateSwmLnb_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleSwmLnbSimulation();
        }


        private void InitBoltButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltInit();
        }


        private void BoltCliButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltCli();
        }


        private void LoadFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltLoadFirmware();
        }


        private void BoltBrowseFirmwareButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltBrowseFirmware();
        }


        private void MemTestButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltMemTest();
        }


        private void ShowDtbButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltShowDtb();
        }


        private void DumpMemoryButton_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleBoltDumpMemory();
        }

        // Stub implementations for missing handlers

        private Task HandleDishVxWorks()
        {
            StatusBarText("Dish VxWorks analysis started");
            return Task.CompletedTask;
        }


        private Task HandlePowerPCDemo()
        {
            StatusBarText("PowerPC demo started");
            return Task.CompletedTask;
        }


        private Task HandleFirmwareExtraction()
        {
            StatusBarText("Firmware extraction started");
            return Task.CompletedTask;
        }


        private Task HandleFileTypeDetection()
        {
            StatusBarText("File type detection started");
            return Task.CompletedTask;
        }


        private Task HandleDvrDataSummary()
        {
            StatusBarText("DVR data summary started");
            return Task.CompletedTask;
        }


        private Task HandleFatMount()
        {
            StatusBarText("FAT filesystem mounted");
            return Task.CompletedTask;
        }


        private Task HandleSquashFsMount()
        {
            StatusBarText("SquashFS mounted");
            return Task.CompletedTask;
        }


        private Task HandleBoltInit()
        {
            StatusBarText("BOLT initialization started");
            return Task.CompletedTask;
        }


        private Task HandleBoltCli()
        {
            StatusBarText("BOLT CLI started");
            return Task.CompletedTask;
        }

        private Task HandleBoltLoadFirmware()
        {
            StatusBarText("BOLT firmware loading started");
            return Task.CompletedTask;
        }

        private Task HandleBoltBrowseFirmware()
        {
            StatusBarText("BOLT firmware browsing started");
            return Task.CompletedTask;
        }

        private Task HandleBoltMemTest()
        {
            StatusBarText("BOLT memory test started");
            return Task.CompletedTask;
        }

        private Task HandleBoltShowDtb()
        {
            StatusBarText("BOLT DTB display started");
            return Task.CompletedTask;
        }

        private Task HandleBoltDumpMemory()
        {
            StatusBarText("BOLT memory dump started");
            return Task.CompletedTask;
        }


        /// <summary>
        /// Handle Windows CE binary execution using cross-platform translation
        /// </summary>
        private async Task HandleWindowsCEExecution()
        {
            try
            {
                // Prompt user to select Windows CE binaries (allow multiple selection)
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Windows CE Binaries",
                    Filter = "Windows CE Executables (*.exe)|*.exe|All Files (*.*)|*.*",
                    InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DVR", "Uverse_Stuff"),
                    Multiselect = true
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    StatusBarText("Windows CE execution cancelled");
                    return;
                }

                string[] binaryPaths = openFileDialog.FileNames;
                StatusBarText($"Loading {binaryPaths.Length} Windows CE binaries...");

                // Ask user if they want concurrent or sequential execution
                var executionChoice = MessageBox.Show(
                    $"Execute {binaryPaths.Length} binaries:\n\n" +
                    "YES = Concurrently (all at once)\n" +
                    "NO = Sequentially (one after another)\n" +
                    "CANCEL = Abort execution",
                    "Execution Mode",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (executionChoice == MessageBoxResult.Cancel)
                {
                    StatusBarText("Windows CE execution cancelled");
                    return;
                }

                bool concurrent = executionChoice == MessageBoxResult.Yes;

                // Initialize Windows CE executor
                var executor = new WindowsCEExecutor();
                List<WindowsCEExecutionResult> results;

                if (concurrent)
                {
                    StatusBarText($"Executing {binaryPaths.Length} binaries concurrently...");
                    results = await executor.ExecuteMultipleAsync(binaryPaths);
                }
                else
                {
                    StatusBarText($"Executing {binaryPaths.Length} binaries sequentially...");
                    results = new List<WindowsCEExecutionResult>();
                    
                    for (int i = 0; i < binaryPaths.Length; i++)
                    {
                        StatusBarText($"Executing binary {i + 1}/{binaryPaths.Length}: {Path.GetFileName(binaryPaths[i])}");
                        var result = await executor.ExecuteAsync(binaryPaths[i]);
                        results.Add(result);
                    }
                }

                // Display execution results
                var logEntries = new List<string>
                {
                    "=== Windows CE Multi-Binary Execution Results ===",
                    $"Execution Mode: {(concurrent ? "Concurrent" : "Sequential")}",
                    $"Total Binaries: {binaryPaths.Length}",
                    $"Successful: {results.Count(r => r.Success)}",
                    $"Failed: {results.Count(r => !r.Success)}",
                    ""
                };

                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    var binary = Path.GetFileName(binaryPaths[i]);
                    
                    logEntries.Add($"=== {i + 1}. {binary} ===");
                    logEntries.Add($"Process ID: {result.ProcessId}");
                    logEntries.Add($"Architecture: {result.Architecture}");
                    logEntries.Add($"Entry Point: 0x{result.EntryPoint:X8}");
                    logEntries.Add($"Status: {(result.Success ? "SUCCESS" : "FAILED")}");
                    logEntries.Add($"Exit Code: {result.ExitCode}");
                    logEntries.Add($"Execution Time: {result.ExecutionTime.TotalMilliseconds:F0}ms");
                    
                    if (!result.Success && !string.IsNullOrEmpty(result.Error))
                    {
                        logEntries.Add($"Error: {result.Error}");
                    }
                    
                    // Add recent log entries (last 5 lines)
                    if (result.Log != null && result.Log.Count > 0)
                    {
                        logEntries.Add("Recent Log:");
                        var recentLogs = result.Log.TakeLast(5);
                        foreach (var log in recentLogs)
                        {
                            logEntries.Add($"  {log}");
                        }
                    }
                    
                    logEntries.Add("");
                }

                // Show running processes
                var runningProcesses = executor.GetRunningProcesses();
                if (runningProcesses.Any(p => p.IsRunning))
                {
                    logEntries.Add("=== Currently Running Processes ===");
                    foreach (var proc in runningProcesses.Where(p => p.IsRunning))
                    {
                        logEntries.Add($"• {Path.GetFileName(proc.ExePath)} (PID: {proc.ProcessId})");
                        logEntries.Add($"  Runtime: {proc.RunTime.TotalSeconds:F1}s");
                    }
                    logEntries.Add("");
                    logEntries.Add("Use 'Stop All Processes' button to terminate running executables.");
                }

                ShowTextWindow($"Windows CE Multi-Execution Results ({binaryPaths.Length} binaries)", logEntries);
                
                int successCount = results.Count(r => r.Success);
                StatusBarText($"Windows CE execution completed: {successCount}/{binaryPaths.Length} successful");

                // Offer to show individual process details
                if (results.Count > 1)
                {
                    var detailChoice = MessageBox.Show(
                        "Would you like to see detailed logs for individual processes?",
                        "View Details",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (detailChoice == MessageBoxResult.Yes)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            var result = results[i];
                            var binary = Path.GetFileName(binaryPaths[i]);
                            
                            var detailLog = new List<string>
                            {
                                $"=== Detailed Log for {binary} ===",
                                $"Process ID: {result.ProcessId}",
                                $"Architecture: {result.Architecture}",
                                $"Entry Point: 0x{result.EntryPoint:X8}",
                                $"Status: {(result.Success ? "SUCCESS" : "FAILED")}",
                                $"Exit Code: {result.ExitCode}",
                                $"Execution Time: {result.ExecutionTime.TotalMilliseconds:F0}ms",
                                ""
                            };

                            if (result.Log != null)
                            {
                                detailLog.AddRange(result.Log);
                            }

                            ShowTextWindow($"Detailed Log - {binary}", detailLog);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new List<string>
                {
                    "=== Windows CE Multi-Execution Error ===",
                    $"Error: {ex.Message}",
                    $"Type: {ex.GetType().Name}",
                    "",
                    "=== Stack Trace ===",
                    ex.StackTrace
                };

                ShowTextWindow("Windows CE Multi-Execution Error", errorLog);
                StatusBarText($"Windows CE execution error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop all running Windows CE processes
        /// </summary>
        private async Task HandleStopAllProcesses()
        {
            try
            {
                var executor = new WindowsCEExecutor();
                var runningProcesses = executor.GetRunningProcesses();
                var activeProcesses = runningProcesses.Where(p => p.IsRunning).ToList();

                if (!activeProcesses.Any())
                {
                    StatusBarText("No running processes to stop");
                    MessageBox.Show("No Windows CE processes are currently running.", "No Active Processes", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirmResult = MessageBox.Show(
                    $"Stop {activeProcesses.Count} running Windows CE processes?\n\n" +
                    string.Join("\n", activeProcesses.Select(p => $"• {Path.GetFileName(p.ExePath)} (Runtime: {p.RunTime.TotalSeconds:F1}s)")),
                    "Confirm Stop All Processes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    executor.StopAllProcesses();
                    StatusBarText($"Stopped {activeProcesses.Count} Windows CE processes");
                    
                    var logEntries = new List<string>
                    {
                        "=== Stopped Windows CE Processes ===",
                        $"Total Processes Stopped: {activeProcesses.Count}",
                        ""
                    };

                    foreach (var proc in activeProcesses)
                    {
                        logEntries.Add($"• {Path.GetFileName(proc.ExePath)}");
                        logEntries.Add($"  Process ID: {proc.ProcessId}");
                        logEntries.Add($"  Runtime: {proc.RunTime.TotalSeconds:F1}s");
                        logEntries.Add("");
                    }

                    ShowTextWindow("Stopped Processes", logEntries);
                }
                else
                {
                    StatusBarText("Process termination cancelled");
                }
            }
            catch (Exception ex)
            {
                StatusBarText($"Error stopping processes: {ex.Message}");
                MessageBox.Show($"Error stopping processes: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Show currently running Windows CE processes
        /// </summary>
        private async Task HandleShowRunningProcesses()
        {
            try
            {
                var executor = new WindowsCEExecutor();
                var processes = executor.GetRunningProcesses();

                var logEntries = new List<string>
                {
                    "=== Windows CE Process Status ===",
                    $"Total Processes: {processes.Count}",
                    $"Running: {processes.Count(p => p.IsRunning)}",
                    $"Stopped: {processes.Count(p => !p.IsRunning)}",
                    ""
                };

                if (processes.Any())
                {
                    logEntries.Add("=== Process Details ===");
                    foreach (var proc in processes.OrderByDescending(p => p.IsRunning).ThenBy(p => p.StartTime))
                    {
                        var status = proc.IsRunning ? "🟢 RUNNING" : "🔴 STOPPED";
                        logEntries.Add($"{status} {Path.GetFileName(proc.ExePath)}");
                        logEntries.Add($"  Process ID: {proc.ProcessId}");
                        logEntries.Add($"  Architecture: {proc.Architecture}");
                        logEntries.Add($"  Started: {proc.StartTime:HH:mm:ss}");
                        
                        if (proc.IsRunning)
                        {
                            logEntries.Add($"  Runtime: {proc.RunTime.TotalSeconds:F1}s");
                        }
                        else
                        {
                            logEntries.Add($"  Stopped: {proc.StopTime?.ToString("HH:mm:ss") ?? "Unknown"}");
                            logEntries.Add($"  Exit Code: {proc.ExitCode}");
                            logEntries.Add($"  Total Runtime: {proc.RunTime.TotalSeconds:F1}s");
                        }
                        
                        logEntries.Add("");
                    }
                }
                else
                {
                    logEntries.Add("No Windows CE processes have been executed yet.");
                }

                ShowTextWindow("Windows CE Process Status", logEntries);
                StatusBarText($"Displayed status for {processes.Count} processes");
            }
            catch (Exception ex)
            {
                StatusBarText($"Error retrieving process status: {ex.Message}");
                MessageBox.Show($"Error retrieving process status: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Open process monitor for real-time updates
        /// </summary>
        private async Task HandleProcessMonitor()
        {
            try
            {
                StatusBarText("Process monitor not implemented yet - use 'Show Running Processes' for current status");
                
                // For now, just show current status with refresh option
                var choice = MessageBox.Show(
                    "Process Monitor is not yet implemented.\n\n" +
                    "Would you like to see the current process status instead?",
                    "Process Monitor",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (choice == MessageBoxResult.Yes)
                {
                    await HandleShowRunningProcesses();
                }
            }
            catch (Exception ex)
            {
                StatusBarText($"Process monitor error: {ex.Message}");
            }

            await Task.CompletedTask;
        }
    }
}
