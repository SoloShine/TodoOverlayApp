using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TodoOverlayApp.Views;

namespace TodoOverlayApp.Utils
{
    public static class TrayIconManager
    {
        private static NotifyIcon? _notifyIcon;

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        public static void Initialize()
        {
            if (_notifyIcon != null) return;

            _notifyIcon = new NotifyIcon
            {
                Icon = LoadIcon("ToDoOverlayApp.ico"),
                Text = "TodoOverlayApp",
                Visibility = Visibility.Visible,
                // 添加托盘菜单
                ContextMenu = new System.Windows.Controls.ContextMenu
                {
                    Items =
                        {
                            CreateMenuItem("打开主面板", OpenMainWindow),
                            CreateMenuItem("设置", OpenSettings),
                            CreateMenuItem("退出", ExitApplication)
                        }
                }
            };
            _notifyIcon.Click += OpenMainWindow;
            _notifyIcon.Init();
        }

        /// <summary>
        /// 加载图标
        /// </summary>
        /// <param name="iconPath">图标文件路径</param>
        /// <returns>BitmapImage</returns>
        private static BitmapImage LoadIcon(string iconPath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, iconPath));
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        /// <summary>
        /// 创建菜单项
        /// </summary>
        /// <param name="header">菜单标题</param>
        /// <param name="clickHandler">点击事件处理程序</param>
        /// <returns>菜单项</returns>
        private static System.Windows.Controls.MenuItem CreateMenuItem(string header, RoutedEventHandler clickHandler)
        {
            var menuItem = new System.Windows.Controls.MenuItem
            {
                Header = header
            };
            menuItem.Click += clickHandler;
            return menuItem;
        }

        /// <summary>
        /// 打开主窗口
        /// </summary>
        private static void OpenMainWindow(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = new MainWindow();
            }

            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.Activate();
        }

        /// <summary>
        /// 打开设置窗口
        /// </summary>
        private static void OpenSettings(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new ThemeSettingsWindow
            {
                Owner = Application.Current.MainWindow
            };
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// 退出应用程序
        /// </summary>
        private static void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 发送全局消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void SendMessage(string message)
        {
            _notifyIcon?.ShowBalloonTip("通知", message, NotifyIconInfoType.Info);
        }

        /// <summary>
        /// 清理托盘图标
        /// </summary>
        public static void Cleanup()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visibility = Visibility.Collapsed;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}
