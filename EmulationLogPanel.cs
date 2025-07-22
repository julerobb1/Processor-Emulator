using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Threading;

namespace ProcessorEmulator
{
    /// <summary>
    /// Real-time emulation log panel for displaying MMIO calls, peripheral traps,
    /// firmware string dumps, and emulation progress with breakpoint support.
    /// </summary>
    public partial class EmulationLogPanel : UserControl, INotifyPropertyChanged
    {
        private readonly ObservableCollection<LogEntry> logEntries = new();
        private readonly object logLock = new object();
        private Timer logFlushTimer;
        private volatile bool isEnabled = true;
        private int maxLogEntries = 1000;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ObservableCollection<LogEntry> LogEntries => logEntries;
        
        public bool IsVerboseMode { get; set; } = false;
        public bool ShowMMIOCalls { get; set; } = true;
        public bool ShowPeripheralTraps { get; set; } = true;
        public bool ShowFirmwareStrings { get; set; } = true;
        public bool ShowInstructionTrace { get; set; } = false;
        
        public EmulationLogPanel()
        {
            InitializeComponent();
            DataContext = this;
            InitializeLogging();
        }
        
        private void InitializeComponent()
        {
            // Create the log panel UI programmatically
            var mainGrid = new Grid();
            
            // Define rows
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Toolbar
            var toolbar = CreateToolbar();
            Grid.SetRow(toolbar, 0);
            mainGrid.Children.Add(toolbar);
            
            // Log list view
            var logListView = CreateLogListView();
            Grid.SetRow(logListView, 1);
            mainGrid.Children.Add(logListView);
            
            // Status bar
            var statusBar = CreateStatusBar();
            Grid.SetRow(statusBar, 2);
            mainGrid.Children.Add(statusBar);
            
            Content = mainGrid;
        }
        
        private StackPanel CreateToolbar()
        {
            var toolbar = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
            };
            
            // Verbose mode toggle
            var verboseCheckBox = new CheckBox
            {
                Content = "Verbose Mode",
                IsChecked = IsVerboseMode,
                Margin = new Thickness(5, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            verboseCheckBox.Checked += (s, e) => { IsVerboseMode = true; OnPropertyChanged(nameof(IsVerboseMode)); };
            verboseCheckBox.Unchecked += (s, e) => { IsVerboseMode = false; OnPropertyChanged(nameof(IsVerboseMode)); };
            toolbar.Children.Add(verboseCheckBox);
            
            // Filter checkboxes
            var mmioCheckBox = new CheckBox
            {
                Content = "MMIO",
                IsChecked = ShowMMIOCalls,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            mmioCheckBox.Checked += (s, e) => ShowMMIOCalls = true;
            mmioCheckBox.Unchecked += (s, e) => ShowMMIOCalls = false;
            toolbar.Children.Add(mmioCheckBox);
            
            var peripheralCheckBox = new CheckBox
            {
                Content = "Peripherals",
                IsChecked = ShowPeripheralTraps,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            peripheralCheckBox.Checked += (s, e) => ShowPeripheralTraps = true;
            peripheralCheckBox.Unchecked += (s, e) => ShowPeripheralTraps = false;
            toolbar.Children.Add(peripheralCheckBox);
            
            var stringCheckBox = new CheckBox
            {
                Content = "Strings",
                IsChecked = ShowFirmwareStrings,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            stringCheckBox.Checked += (s, e) => ShowFirmwareStrings = true;
            stringCheckBox.Unchecked += (s, e) => ShowFirmwareStrings = false;
            toolbar.Children.Add(stringCheckBox);
            
            // Clear button
            var clearButton = new Button
            {
                Content = "Clear",
                Width = 60,
                Margin = new Thickness(10, 0, 0, 0)
            };
            clearButton.Click += (s, e) => ClearLog();
            toolbar.Children.Add(clearButton);
            
            // Export button
            var exportButton = new Button
            {
                Content = "Export",
                Width = 60,
                Margin = new Thickness(5, 0, 0, 0)
            };
            exportButton.Click += ExportLog_Click;
            toolbar.Children.Add(exportButton);
            
            return toolbar;
        }
        
        private ListView CreateLogListView()
        {
            var listView = new ListView
            {
                ItemsSource = LogEntries,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11
            };
            
            // Create custom item template
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Grid));
            
            // Define columns
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(80));
            factory.AppendChild(col1);
            
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(100));
            factory.AppendChild(col2);
            
            var col3 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col3.SetValue(ColumnDefinition.WidthProperty, new GridLength(80));
            factory.AppendChild(col3);
            
            var col4 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col4.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            factory.AppendChild(col4);
            
            // Timestamp
            var timeText = new FrameworkElementFactory(typeof(TextBlock));
            timeText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Timestamp") { StringFormat = "HH:mm:ss.fff" });
            timeText.SetValue(Grid.ColumnProperty, 0);
            timeText.SetValue(TextBlock.MarginProperty, new Thickness(2));
            factory.AppendChild(timeText);
            
            // Category
            var categoryText = new FrameworkElementFactory(typeof(TextBlock));
            categoryText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Category"));
            categoryText.SetBinding(TextBlock.ForegroundProperty, new System.Windows.Data.Binding("CategoryBrush"));
            categoryText.SetValue(Grid.ColumnProperty, 1);
            categoryText.SetValue(TextBlock.MarginProperty, new Thickness(2));
            categoryText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            factory.AppendChild(categoryText);
            
            // Level
            var levelText = new FrameworkElementFactory(typeof(TextBlock));
            levelText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Level"));
            levelText.SetValue(Grid.ColumnProperty, 2);
            levelText.SetValue(TextBlock.MarginProperty, new Thickness(2));
            factory.AppendChild(levelText);
            
            // Message
            var messageText = new FrameworkElementFactory(typeof(TextBlock));
            messageText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Message"));
            messageText.SetValue(Grid.ColumnProperty, 3);
            messageText.SetValue(TextBlock.MarginProperty, new Thickness(2));
            messageText.SetValue(TextBlock.TextWrappingProperty, TextWrapping.NoWrap);
            factory.AppendChild(messageText);
            
            template.VisualTree = factory;
            listView.ItemTemplate = template;
            
            return listView;
        }
        
        private StatusBar CreateStatusBar()
        {
            var statusBar = new StatusBar();
            var statusText = new TextBlock { Text = $"Log entries: {logEntries.Count}" };
            var statusItem = new StatusBarItem { Content = statusText };
            statusBar.Items.Add(statusItem);
            
            // Update count when log changes
            logEntries.CollectionChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(() => statusText.Text = $"Log entries: {logEntries.Count}");
            };
            
            return statusBar;
        }
        
        private void InitializeLogging()
        {
            // Flush log entries every 100ms for real-time updates
            logFlushTimer = new Timer(FlushPendingLogs, null, 100, 100);
        }
        
        private void FlushPendingLogs(object state)
        {
            // This would be called by the emulation system to flush pending log entries
            // For now, we'll keep the collection management here
        }
        
        public void LogMMIOAccess(uint address, uint value, bool isWrite, string peripheralName)
        {
            if (!ShowMMIOCalls && !IsVerboseMode) return;
            
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = "MMIO",
                Level = "DEBUG",
                Message = $"{peripheralName}: {(isWrite ? "WRITE" : "READ")} 0x{address:X8} = 0x{value:X8}",
                CategoryBrush = new SolidColorBrush(Colors.Blue)
            };
            
            AddLogEntry(entry);
        }
        
        public void LogPeripheralTrap(string peripheralName, string operation, string details)
        {
            if (!ShowPeripheralTraps && !IsVerboseMode) return;
            
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = "PERIPHERAL",
                Level = "INFO",
                Message = $"{peripheralName}: {operation} - {details}",
                CategoryBrush = new SolidColorBrush(Colors.Green)
            };
            
            AddLogEntry(entry);
        }
        
        public void LogFirmwareString(uint address, string text)
        {
            if (!ShowFirmwareStrings && !IsVerboseMode) return;
            
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = "FIRMWARE",
                Level = "INFO",
                Message = $"String @ 0x{address:X8}: \"{text}\"",
                CategoryBrush = new SolidColorBrush(Colors.Purple)
            };
            
            AddLogEntry(entry);
        }
        
        public void LogInstructionTrace(uint pc, uint instruction, string disassembly)
        {
            if (!ShowInstructionTrace && !IsVerboseMode) return;
            
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = "TRACE",
                Level = "TRACE",
                Message = $"PC:0x{pc:X8} | 0x{instruction:X8} | {disassembly}",
                CategoryBrush = new SolidColorBrush(Colors.Gray)
            };
            
            AddLogEntry(entry);
        }
        
        public void LogBreakpoint(uint address, string reason)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = "BREAKPOINT",
                Level = "WARN",
                Message = $"Breakpoint hit @ 0x{address:X8}: {reason}",
                CategoryBrush = new SolidColorBrush(Colors.Red)
            };
            
            AddLogEntry(entry);
        }
        
        private void AddLogEntry(LogEntry entry)
        {
            if (!isEnabled) return;
            
            lock (logLock)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    logEntries.Add(entry);
                    
                    // Limit log size
                    while (logEntries.Count > maxLogEntries)
                    {
                        logEntries.RemoveAt(0);
                    }
                    
                    // Auto-scroll to bottom for new entries
                    if (Content is Grid grid && grid.Children[1] is ListView listView)
                    {
                        if (listView.Items.Count > 0)
                        {
                            listView.ScrollIntoView(listView.Items[listView.Items.Count - 1]);
                        }
                    }
                });
            }
        }
        
        private void ClearLog()
        {
            lock (logLock)
            {
                Dispatcher.BeginInvoke(() => logEntries.Clear());
            }
        }
        
        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"emulation_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };
                
                if (saveDialog.ShowDialog() == true)
                {
                    using var writer = new System.IO.StreamWriter(saveDialog.FileName);
                    writer.WriteLine($"Emulation Log Export - {DateTime.Now}");
                    writer.WriteLine(new string('=', 80));
                    
                    foreach (var entry in logEntries)
                    {
                        writer.WriteLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Category}] {entry.Level}: {entry.Message}");
                    }
                    
                    MessageBox.Show($"Log exported to: {saveDialog.FileName}", "Export Complete", 
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void Dispose()
        {
            isEnabled = false;
            logFlushTimer?.Dispose();
        }
    }
    
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Category { get; set; } = "";
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";
        public Brush CategoryBrush { get; set; } = Brushes.Black;
    }
}
