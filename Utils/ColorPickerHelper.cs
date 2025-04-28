using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TodoOverlayApp.Utils
{
    /// <summary>
    /// 颜色选择相关的工具类
    /// </summary>
    public static class ColorPickerHelper
    {
        /// <summary>
        /// 显示颜色选择器对话框
        /// </summary>
        /// <param name="initialColor">初始颜色</param>
        /// <param name="onColorSelected">颜色选择回调</param>
        public static void ShowColorPicker(string initialColor, Action<string> onColorSelected)
        {
            try
            {
                // 创建并配置颜色选择器
                var colorPicker = new HandyControl.Controls.ColorPicker();

                // 如果有初始颜色，设置为默认值
                if (!string.IsNullOrEmpty(initialColor))
                {
                    try
                    {
                        Color defaultColor = (Color)ColorConverter.ConvertFromString(initialColor);
                        colorPicker.SelectedBrush = new SolidColorBrush(defaultColor);
                    }
                    catch { /* 忽略解析错误 */ }
                }

                // 显示颜色选择器对话框
                var dialog = HandyControl.Controls.Dialog.Show(colorPicker);

                // 当用户确认选择时
                colorPicker.Confirmed += (s, args) =>
                {
                    if (colorPicker.SelectedBrush is SolidColorBrush brush)
                    {
                        Color selectedColor = brush.Color;
                        string hexColor = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";

                        // 回调选中的颜色
                        onColorSelected(hexColor);

                        // 关闭对话框
                        dialog.Close();
                    }
                };

                // 当用户取消选择时
                colorPicker.Canceled += (s, args) =>
                {
                    dialog.Close();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开颜色选择器时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 创建或更新自定义颜色按钮
        /// </summary>
        public static RadioButton CreateOrUpdateColorButton(
            RadioButton existingButton,
            string colorHex,
            string groupName,
            RoutedEventHandler checkedHandler)
        {
            try
            {
                Color customColor = (Color)ColorConverter.ConvertFromString(colorHex);

                // 如果按钮已存在，更新它
                if (existingButton != null)
                {
                    existingButton.Background = new SolidColorBrush(customColor);
                    existingButton.BorderBrush = new SolidColorBrush(customColor);
                    existingButton.Foreground = new SolidColorBrush(customColor);
                    existingButton.Tag = colorHex;
                    existingButton.IsChecked = true;
                    return existingButton;
                }

                // 创建新按钮
                var newButton = new RadioButton
                {
                    Style = Application.Current.FindResource("RadioButtonIcon") as Style,
                    Background = new SolidColorBrush(customColor),
                    BorderBrush = new SolidColorBrush(customColor),
                    Foreground = new SolidColorBrush(customColor),
                    Margin = new Thickness(5),
                    GroupName = groupName,
                    Tag = colorHex,
                    IsChecked = true
                };

                // 添加事件处理
                newButton.Checked += checkedHandler;

                return newButton;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建颜色按钮时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return existingButton ?? new RadioButton(); // 返回原按钮或创建一个新的空按钮
            }
        }

        /// <summary>
        /// 在指定颜色面板中初始化颜色选择
        /// </summary>
        public static bool InitializeColorSelection(Panel panel, string colorValue)
        {
            foreach (var child in panel.Children)
            {
                if (child is RadioButton radioButton && radioButton.Tag is string colorHex)
                {
                    if (colorHex.Equals(colorValue, StringComparison.OrdinalIgnoreCase))
                    {
                        radioButton.IsChecked = true;
                        return true; // 找到匹配的预定义颜色
                    }
                }
            }
            return false; // 没有找到匹配的预定义颜色
        }
    }
}

