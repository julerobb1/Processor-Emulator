using System;
using System.Collections.Generic;
using System.Windows;

namespace ProcessorEmulator
{
    /// <summary>
    /// Interaction logic for FolderAnalysisWindow.xaml
    /// </summary>
    public partial class FolderAnalysisWindow : Window
    {
        // Default constructor for XAML code-behind
        public FolderAnalysisWindow()
        {
            InitializeComponent();
        }

        // Constructor to supply items
        public FolderAnalysisWindow(List<FileRecord> items) : this()
        {
            DataGridFiles.ItemsSource = items;
        }
    }


    public class FileRecord
    {
        public string FilePath { get; set; }
        public long Size { get; set; }
        public string HexPreview { get; set; }
    }
}
