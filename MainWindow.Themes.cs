using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace ProcessorEmulator
{
    public partial class MainWindow
    {
        private System.Windows.Controls.ComboBox runtimeThemeCombo;
        private TextBlock runtimeGlassText;

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox cb && cb.SelectedItem is ComboBoxItem item)
            {
                string name = item.Content?.ToString() ?? "Win95";
                SwitchTheme(name);
            }
        }

        public void SwitchTheme(string themeName)
        {
            try
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Uri uri = themeName switch
                    {
                        "iGuide (Legacy)" => new Uri("/Themes/ThemeiGuide.xaml", UriKind.Relative),
                        "Mediaroom (U-verse)" => new Uri("/Themes/ThemeMediaroom.xaml", UriKind.Relative),
                        "X1 (Modern)" => new Uri("/Themes/ThemeX1.xaml", UriKind.Relative),
                        _ => new Uri("/Themes/ThemeWin95.xaml", UriKind.Relative),
                    };

                    var dict = new ResourceDictionary() { Source = uri };
                    Application.Current.Resources.MergedDictionaries.Add(dict);

                    // Update runtime glass indicator if present
                    if (runtimeGlassText != null)
                    {
                        if (themeName == "Mediaroom (U-verse)")
                        {
                            runtimeGlassText.Text = "On";
                            runtimeGlassText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00AEEF"));
                        }
                        else
                        {
                            runtimeGlassText.Text = "Off";
                            runtimeGlassText.Foreground = new SolidColorBrush(Colors.Gray);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to switch theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            AttachRuntimeThemeControls();
        }

        private void AttachRuntimeThemeControls()
        {
            // Find first StatusBar in visual tree
            var statusBar = FindVisualChild<System.Windows.Controls.Primitives.StatusBar>(this);
            if (statusBar == null) return;

            // create glass text
            runtimeGlassText = new TextBlock { Text = "Off", Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(4,0,0,0) };

            // create combobox
            runtimeThemeCombo = new ComboBox { Width = 220 };
            runtimeThemeCombo.Items.Add(new ComboBoxItem { Content = "Win95" });
            runtimeThemeCombo.Items.Add(new ComboBoxItem { Content = "iGuide (Legacy)" });
            runtimeThemeCombo.Items.Add(new ComboBoxItem { Content = "Mediaroom (U-verse)" });
            runtimeThemeCombo.Items.Add(new ComboBoxItem { Content = "X1 (Modern)" });
            runtimeThemeCombo.SelectedIndex = 0;
            runtimeThemeCombo.SelectionChanged += ThemeCombo_SelectionChanged;

            // insert into statusbar
            var label = new TextBlock { Text = "Glass:", Margin = new Thickness(0,0,4,0) };
            var container = new StackPanel { Orientation = Orientation.Horizontal };
            container.Children.Add(label);
            container.Children.Add(runtimeGlassText);

            var item = new System.Windows.Controls.Primitives.StatusBarItem { Content = container };
            var comboItem = new System.Windows.Controls.Primitives.StatusBarItem { HorizontalAlignment = HorizontalAlignment.Right, Content = runtimeThemeCombo };

            statusBar.Items.Add(item);
            statusBar.Items.Add(comboItem);
        }

        private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
