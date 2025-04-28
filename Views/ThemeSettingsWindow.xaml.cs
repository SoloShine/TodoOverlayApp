using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Themes;

namespace TodoOverlayApp.Views
{
    public partial class ThemeSettingsWindow : Window
    {
        public ThemeSettingsWindow()
        {
            InitializeComponent();

            // 初始化主题选择
            bool isDarkTheme = false;
            // 尝试从应用资源中确定当前主题
            if (Application.Current.Resources.MergedDictionaries.Count > 0)
            {
                // 检查资源字典中是否包含特定的暗色主题标记（这是一种简单的检测方法）
                isDarkTheme = Application.Current.Resources["DarkDefault"] != null;
            }

            ThemeComboBox.SelectedIndex = isDarkTheme ? 1 : 0;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;

            try
            {
                if (ThemeComboBox.SelectedIndex == 0)
                {
                    // 切换到浅色主题
                    SwitchTheme(SkinType.Default);
                }
                else if(ThemeComboBox.SelectedIndex == 1)
                {
                    // 切换到深色主题
                    SwitchTheme(SkinType.Dark);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换主题时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchTheme(SkinType skinType)
        {
            // 获取应用的资源字典
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // 清除现有的主题资源
            var themeDicts = new System.Collections.Generic.List<ResourceDictionary>();
            foreach (var dict in mergedDicts)
            {
                if (dict.Source != null &&
                    (dict.Source.ToString().Contains("Theme") ||
                     dict.Source.ToString().Contains("Skin")))
                {
                    themeDicts.Add(dict);
                }
            }

            foreach (var dict in themeDicts)
            {
                mergedDicts.Remove(dict);
            }

            // 添加新的主题资源
            switch (skinType)
            {
                case SkinType.Default:
                    mergedDicts.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml")
                    });
                    break;
                case SkinType.Dark:
                    mergedDicts.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml")
                    });
                    break;
            }

            // 重新加载主题
            mergedDicts.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }

        private void ColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is string colorHex)
            {
                try
                {
                    Color themeColor = (Color)ColorConverter.ConvertFromString(colorHex);
                    // 在这里应用主题颜色的逻辑
                    // 由于没有直接的 AccentColor 设置，我们可以通过修改资源来实现
                    Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(themeColor);
                    Application.Current.Resources["DarkPrimaryBrush"] = new SolidColorBrush(
                        Color.FromRgb(
                            (byte)Math.Max(0, themeColor.R - 40),
                            (byte)Math.Max(0, themeColor.G - 40),
                            (byte)Math.Max(0, themeColor.B - 40)
                        ));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"应用主题颜色时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }

    // 主题类型枚举
    public enum SkinType
    {
        Default,
        Dark
    }
}
