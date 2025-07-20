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
        public FolderAnalysisWindow(List<FileRecord> items)
        {
            InitializeComponent();
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
