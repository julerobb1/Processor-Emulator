using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ProcessorEmulator
{

    public class FolderAnalysisWindow : Window
    {
        public FolderAnalysisWindow(string folderPath)
        {
            Title = $"Folder Analysis: {folderPath}";
            Width = 800;
            Height = 600;
            // Scan files
            var filePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int fileCount = filePaths.Length;
            long totalSize = filePaths.Select(f => new FileInfo(f).Length).Sum();
            // UI setup
            var dock = new DockPanel();
            // Summary panel
            var summary = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            summary.Children.Add(new TextBlock { Text = $"Files: {fileCount}", Margin = new Thickness(0,0,20,0) });
            summary.Children.Add(new TextBlock { Text = $"Total Size: {totalSize} bytes" });
            DockPanel.SetDock(summary, Dock.Top);
            dock.Children.Add(summary);
            // Hex preview box and Image viewer
            var previewBox = new TextBox { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var imageViewer = new Image { Visibility = Visibility.Collapsed, Stretch = Stretch.Uniform };
            // Container for preview controls
            var previewGrid = new Grid { Height = 150 };
            previewGrid.Children.Add(previewBox);
            previewGrid.Children.Add(imageViewer);
            DockPanel.SetDock(previewGrid, Dock.Bottom);
            dock.Children.Add(previewGrid);
            // Tree view
            var tree = new TreeView();
            DockPanel.SetDock(tree, Dock.Top);
            dock.Children.Add(tree);
            // Build tree nodes
            var root = new TreeViewItem { Header = folderPath, Tag = folderPath, IsExpanded = true };
            tree.Items.Add(root);
            foreach (var file in filePaths)
            {
                var relative = Path.GetRelativePath(folderPath, file);
                var parts = relative.Split(Path.DirectorySeparatorChar);
                var parent = root;
                for (int i = 0; i < parts.Length; i++)
                {
                    var name = parts[i];
                    TreeViewItem node = null;
                    foreach (TreeViewItem child in parent.Items)
                    {
                        if ((string)child.Header == name) { node = child; break; }
                    }
                    if (node == null)
                    {
                        node = new TreeViewItem { Header = name, Tag = (i == parts.Length - 1) ? file : null };
                        parent.Items.Add(node);
                    }
                    parent = node;
                }
            }
            // Select event
            tree.SelectedItemChanged += (s,e) =>
            {
                if (tree.SelectedItem is TreeViewItem item && item.Tag is string path)
                {
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    var imageExts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tga" };
                    if (imageExts.Contains(ext))
                    {
                        // Show image preview
                        imageViewer.Source = new BitmapImage(new Uri(path));
                        imageViewer.Visibility = Visibility.Visible;
                        previewBox.Visibility = Visibility.Collapsed;
                        return;
                    }
                    // Fallback to hex preview
                    previewBox.Visibility = Visibility.Visible;
                    imageViewer.Visibility = Visibility.Collapsed;
                    var info = new FileInfo(path);
                    int len = (int)Math.Min(256, info.Length);
                    byte[] buf = new byte[len];
                    using (var fs = File.OpenRead(path)) fs.Read(buf, 0, len);
                    previewBox.Text = BitConverter.ToString(buf).Replace("-", " ");
                }
            };
            Content = dock;
        }
    }
}
