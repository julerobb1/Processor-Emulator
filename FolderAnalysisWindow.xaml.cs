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
        public FolderAnalysisWindow(List<FileRecord> records)
        {
        }

        public FolderAnalysisWindow(List<FileRecord> items, object dataGridFiles)
        {
            InitializeComponent();
            DataGridFiles.ItemsSource = items;
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
    }

    internal class DataGridFiles
    {
        public static List<FileRecord> ItemsSource { get; internal set; }
    }

    public class FileRecord
    {
        public string FilePath { get; set; }
        public long Size { get; set; }
        public string HexPreview { get; set; }
    }
}
