using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Themes;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Views
{
    public partial class ThemeSettingsWindow : Window
    {
        private MainWindowModel _model;

        public ThemeSettingsWindow()
        {
            InitializeComponent();

            // 获取应用的模型
            _model = App.MainViewModel?.Model ?? new MainWindowModel();

            // 根据当前模型中的主题设置初始化控件
            ThemeComboBox.SelectedIndex = _model.ThemeType == "Dark" ? 1 : 0;

            // 选择当前主题颜色对应的单选按钮
            foreach (var child in ColorsPanel.Children)
            {
                if (child is RadioButton radioButton && radioButton.Tag is string colorHex)
                {
                    if (colorHex.Equals(_model.ThemeColor, StringComparison.OrdinalIgnoreCase))
                    {
                        radioButton.IsChecked = true;
                        break;
                    }
                }
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;

            try
            {
                // 更新模型中的主题类型
                string themeType = ThemeComboBox.SelectedIndex == 0 ? "Default" : "Dark";
                _model.ThemeType = themeType;

                // 立即应用主题设置
                _model.ApplyThemeSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换主题时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            if (sender is RadioButton radioButton && radioButton.Tag is string colorHex)
            {
                try
                {
                    // 更新模型中的主题颜色
                    _model.ThemeColor = colorHex;

                    // 立即应用主题设置
                    _model.ApplyThemeSettings();
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
}
