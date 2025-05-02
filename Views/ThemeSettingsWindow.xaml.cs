using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TodoOverlayApp.Models;
using TodoOverlayApp.Utils;
using MessageBox = HandyControl.Controls.MessageBox;

namespace TodoOverlayApp.Views
{
    public partial class ThemeSettingsWindow : HandyControl.Controls.Window
    {
        private MainWindowModel model;
        private RadioButton? customColorRadioButton = null;
        private RadioButton? customThemeColorRadioButton = null;

        public ThemeSettingsWindow()
        {
            InitializeComponent();

            // 获取应用的模型
            model = App.MainViewModel?.Model ?? new MainWindowModel();

            // 根据当前模型中的主题设置初始化控件
            ThemeComboBox.SelectedIndex = model.ThemeType == "Dark" ? 1 : 0;

            // 初始化主题颜色选择
            bool isThemeColorPredefined = ColorPickerHelper.InitializeColorSelection(ColorsPanel, model.ThemeColor);
            if (!isThemeColorPredefined)
            {
                // 创建自定义主题颜色按钮
                CreateCustomThemeColorButton(model.ThemeColor);
            }

            // 初始化悬浮窗背景颜色选择
            bool isOverlayColorPredefined = ColorPickerHelper.InitializeColorSelection(OverlayColorsPanel, model.OverlayBackground);
            if (!isOverlayColorPredefined)
            {
                // 创建自定义悬浮窗背景颜色按钮
                CreateCustomOverlayColorButton(model.OverlayBackground);
            }

            // 初始化不透明度滑块
            OpacitySlider.Value = model.OverlayOpacity;

            // 初始化预览
            UpdatePreview();
        }

        /// <summary>
        /// 创建自定义主题颜色按钮
        /// </summary>
        private void CreateCustomThemeColorButton(string colorHex)
        {
            customThemeColorRadioButton = ColorPickerHelper.CreateOrUpdateColorButton(
                customThemeColorRadioButton, colorHex, "ThemeColors", ColorRadioButton_Checked);

            // 添加到面板中，确保添加在自定义按钮之前
            int customButtonIndex = ColorsPanel.Children.IndexOf(CustomThemeColorButton);
            if (customButtonIndex > 0)
            {
                ColorsPanel.Children.Insert(customButtonIndex, customThemeColorRadioButton);
            }
            else
            {
                ColorsPanel.Children.Add(customThemeColorRadioButton);
            }
        }

        /// <summary>
        /// 创建自定义悬浮窗背景颜色按钮
        /// </summary>
        private void CreateCustomOverlayColorButton(string colorHex)
        {
            customColorRadioButton = ColorPickerHelper.CreateOrUpdateColorButton(
                customColorRadioButton, colorHex, "OverlayColors", OverlayColorRadioButton_Checked);

            // 添加到面板中，确保添加在自定义按钮之前
            int customButtonIndex = OverlayColorsPanel.Children.IndexOf(CustomColorButton);
            if (customButtonIndex > 0)
            {
                OverlayColorsPanel.Children.Insert(customButtonIndex, customColorRadioButton);
            }
            else
            {
                OverlayColorsPanel.Children.Add(customColorRadioButton);
            }
        }

        /// <summary>
        /// 点击自定义主题颜色按钮时打开颜色选择器
        /// </summary>
        private void CustomThemeColorButton_Click(object sender, RoutedEventArgs e)
        {
            string initialColor = customThemeColorRadioButton?.Tag as string ?? model.ThemeColor;

            ColorPickerHelper.ShowColorPicker(initialColor, hexColor => {
                // 更新模型中的主题颜色
                model.ThemeColor = hexColor;

                // 创建或更新自定义颜色按钮
                CreateCustomThemeColorButton(hexColor);

                // 立即应用主题设置
                model.ApplyThemeSettings();
            });
        }

        /// <summary>
        /// 点击自定义悬浮窗背景颜色按钮时打开颜色选择器
        /// </summary>
        private void CustomColorButton_Click(object sender, RoutedEventArgs e)
        {
            string initialColor = customColorRadioButton?.Tag as string ?? model.OverlayBackground;

            ColorPickerHelper.ShowColorPicker(initialColor, hexColor => {
                // 更新模型中的悬浮窗背景颜色
                model.OverlayBackground = hexColor;

                // 创建或更新自定义颜色按钮
                CreateCustomOverlayColorButton(hexColor);

                // 更新预览
                UpdatePreview();
            });
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;

            try
            {
                // 更新模型中的主题类型
                string themeType = ThemeComboBox.SelectedIndex == 0 ? "Default" : "Dark";
                model.ThemeType = themeType;

                // 立即应用主题设置
                model.ApplyThemeSettings();
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
                    model.ThemeColor = colorHex;

                    // 立即应用主题设置
                    model.ApplyThemeSettings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"应用主题颜色时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OverlayColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            if (sender is RadioButton radioButton && radioButton.Tag is string colorHex)
            {
                try
                {
                    // 更新模型中的悬浮窗背景颜色
                    model.OverlayBackground = colorHex;

                    // 更新预览
                    UpdatePreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"应用悬浮窗背景颜色时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;

            try
            {
                // 更新模型中的悬浮窗不透明度
                model.OverlayOpacity = OpacitySlider.Value;

                // 更新预览
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置悬浮窗不透明度时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePreview()
        {
            try
            {
                Color backgroundColor = (Color)ColorConverter.ConvertFromString(model.OverlayBackground);
                PreviewBorder.Background = new SolidColorBrush(backgroundColor);
                PreviewBorder.Opacity = model.OverlayOpacity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"更新预览时出错: {ex.Message}");
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}

