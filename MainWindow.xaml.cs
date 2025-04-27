using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp
{
    public class TodoItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _content;
        private bool _isCompleted;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        private string _targetAppPath;
        // 新增字典用于存储每个关联软件对应的待办事项窗口
        private Dictionary<string, OverlayWindow> _overlayWindows = new Dictionary<string, OverlayWindow>();

        private ObservableCollection<TodoItem> _todoItems = new ObservableCollection<TodoItem>();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this; // 设置DataContext
            todoListView.ItemsSource = _todoItems; // 直接绑定ItemsSource
            this.Closed += MainWindow_Closed;  // 添加关闭事件处理
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // 关闭所有子窗口
            foreach (var window in _overlayWindows.Values)
            {
                window.Close();
            }
            _overlayWindows.Clear();
        }

        private void SelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _targetAppPath = openFileDialog.FileName;
                txtAppPath.Text = _targetAppPath;
            }
        }

        private void SelectRunningAppButton_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();
            if (processes.Count == 0)
            {
                MessageBox.Show("当前没有可选择的运行中软件。");
                return;
            }

            var selectWindow = new Window
            {
                Title = "选择当前运行软件",
                Width = 300,
                Height = 400
            };

            var searchBox = new TextBox
            {
                Text = "搜索进程...",
                Margin = new Thickness(10, 10, 10, 0),
                Foreground = System.Windows.Media.Brushes.Gray
            };

            searchBox.GotFocus += (s, args) =>
            {
                if (searchBox.Text == "搜索进程...")
                {
                    searchBox.Text = "";
                    searchBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            };

            searchBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrEmpty(searchBox.Text))
                {
                    searchBox.Text = "搜索进程...";
                    searchBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };

            var listBox = new ListBox();
            void UpdateListBox()
            {
                listBox.Items.Clear();
                var searchText = searchBox.Text.ToLower();
                if (searchText == "搜索进程...")
                {
                    searchText = "";
                }
                var filteredProcesses = processes.Where(p =>
                    p.ProcessName.ToLower().Contains(searchText) ||
                    p.MainWindowTitle.ToLower().Contains(searchText)).ToList();

                foreach (var process in filteredProcesses)
                {
                    listBox.Items.Add($"{process.ProcessName} - {process.MainWindowTitle}");
                }
            }

            searchBox.TextChanged += (s, args) => UpdateListBox();
            UpdateListBox();

            var selectButton = new Button
            {
                Content = "选择",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 10)
            };

            selectButton.Click += (s, args) =>
            {
                if (listBox.SelectedIndex != -1)
                {
                    var selectedProcess = processes[listBox.Items.IndexOf(listBox.SelectedItem)];
                    try
                    {
                        _targetAppPath = selectedProcess.MainModule.FileName;
                        txtAppPath.Text = _targetAppPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法获取进程路径: {ex.Message}");
                    }
                }
                selectWindow.Close();
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(searchBox);
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(selectButton);
            selectWindow.Content = stackPanel;
            selectWindow.ShowDialog();
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAppInstalled() && CheckAppRunning())
            {
                InjectTodoOverlay();
            }
        }

        private void ForceLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAppInstalled())
            {
                Process.Start(_targetAppPath);
            }
            else
            {
                MessageBox.Show("关联软件未安装，请检查路径。");
            }
        }

        private bool CheckAppInstalled()
        {
            return File.Exists(_targetAppPath);
        }

        private bool CheckAppRunning()
        {
            var processName = Path.GetFileNameWithoutExtension(_targetAppPath);
            return Process.GetProcessesByName(processName).Length > 0;
        }

        private void AddTodoItem_Click(object sender, RoutedEventArgs e)
        {
            _todoItems.Add(new TodoItem { Content = "新待办项", IsCompleted = false });
        }

        private void DeleteTodoItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TodoItem item)
            {
                _todoItems.Remove(item);
            }
        }

        private void InjectTodoOverlay()
        {
            var targetProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_targetAppPath));
            if (targetProcesses.Length == 0) return;

            var targetProcess = targetProcesses[0];
            var targetWindowHandle = targetProcess.MainWindowHandle;
            
            if (targetWindowHandle == IntPtr.Zero || !NativeMethods.IsWindow(targetWindowHandle))
            {
                MessageBox.Show("无法获取有效的窗口句柄。");
                return;
            }

            var appKey = _targetAppPath;
            if (!_overlayWindows.ContainsKey(appKey))
            {
                var overlayWindow = new OverlayWindow(_todoItems); // 传入TodoItems集合
                overlayWindow.Topmost = true;
                
                // 使用DispatcherTimer需要System.Windows.Threading命名空间
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                timer.Tick += (s, e) => UpdateOverlayPosition(overlayWindow, targetWindowHandle);
                timer.Start();

                overlayWindow.Closed += (s, e) => timer.Stop();
                overlayWindow.Show();
                _overlayWindows[appKey] = overlayWindow;
            }
        }

        private void UpdateOverlayPosition(OverlayWindow overlayWindow, IntPtr targetWindowHandle)
        {
            if (NativeMethods.GetWindowRect(targetWindowHandle, out NativeMethods.RECT rect))
            {
                overlayWindow.Left = rect.Left;
                overlayWindow.Top = rect.Bottom - overlayWindow.Height;
            }
        }
    }

    // 导入 Windows API 函数
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;
    
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}