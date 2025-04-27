
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TodoOverlayApp.Models;
using TodoOverlayApp.Views;

namespace TodoOverlayApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Dictionary<string, OverlayWindow> _overlayWindows = [];


        private MainWIndowModel model;
        /// <summary>
        /// Model 属性，用于绑定到主窗口的视图模型。
        /// </summary>
        public MainWIndowModel Model
        {
            get => model;
            set
            {
                model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        public ICommand AddAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand SelectAppCommand { get; }
        public ICommand SelectRunningAppCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand ForceLaunchCommand { get; }
        public ICommand AddTodoItemCommand { get; }
        public ICommand DeleteTodoItemCommand { get; }

        public ICommand AddSubTodoItemCommand { get; }
        public ICommand DeleteSubTodoItemCommand { get; }

        public MainWindowViewModel()
        {
            // 尝试从文件加载配置，如果加载失败则创建新的模型
            var loadedModel = MainWIndowModel.LoadFromFile();
            if (loadedModel != null)
            {
                model = loadedModel;
            }
            else
            {
                model = new MainWIndowModel();
            }

            AddAppCommand = new RelayCommand(AddApp);
            RemoveAppCommand = new RelayCommand(RemoveApp);
            SelectAppCommand = new RelayCommand(SelectApp);
            SelectRunningAppCommand = new RelayCommand(SelectRunningApp);
            SaveConfigCommand = new RelayCommand(SaveConfig);
            ForceLaunchCommand = new RelayCommand(ForceLaunch);
            AddTodoItemCommand = new RelayCommand(AddTodoItem);
            DeleteTodoItemCommand = new RelayCommand(DeleteTodoItem);
            AddSubTodoItemCommand = new RelayCommand(AddSubTodoItem);
            DeleteSubTodoItemCommand = new RelayCommand(DeleteTodoItem); // 复用现有的删除方法

        }

        /// <summary>
        /// 添加一个应用。将新应用的路径设置为选中状态的AppPath，并将其加入到集合中。
        /// </summary>
        /// <param name="parameter"></param>
        private void AddApp(object? parameter)
        {
            var newApp = new AppAssociation();
            Model.AppAssociations.Add(newApp);
            Model.SelectedApp = newApp;
        }

        /// <summary>
        /// 移除一个应用。如果选中的是当前要删除的应用，则将选中状态设置为null。
        /// </summary>
        /// <param name="parameter"></param>
        private void RemoveApp(object? parameter)
        {
            if (parameter is AppAssociation app)
            {
                Model.AppAssociations.Remove(app);
                if (Model.SelectedApp == app)
                {
                    Model.SelectedApp = Model.AppAssociations.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// 选择一个应用，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectApp(object? parameter)
        {
            if (parameter is not AppAssociation app) return;

            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                app.AppPath = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 选择当前运行软件，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectRunningApp(object? parameter)
        {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ToList();

            if (processes.Count == 0)
            {
                MessageBox.Show("当前没有可选择的运行中软件。");
                return;
            }

            var selectWindow = new Window
            {
                Title = "选择当前运行软件",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };

            var searchBox = new TextBox { Text = "搜索进程..." };
            var listBox = new ListBox();
            void ChooseFunc()
            {
                if (listBox.SelectedIndex != -1 && Model.SelectedApp != null)
                {
                    var process = processes[listBox.SelectedIndex];
                    try
                    {
                        Model.SelectedApp.AppPath = process.MainModule?.FileName;
                        Model.SelectedApp.AppName = process.MainModule?.ModuleName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法获取进程路径: {ex.Message}");
                    }
                }
                selectWindow.Close();
            };
            listBox.MouseDoubleClick += (s, e) => ChooseFunc();
            void UpdateListBox()
            {
                var searchText = searchBox.Text == "搜索进程..." ? "" : searchBox.Text.ToLower();
                listBox.Items.Clear();
                foreach (var process in processes.Where(p =>
                    p.ProcessName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    p.MainWindowTitle.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)))
                {
                    listBox.Items.Add($"{process.ProcessName} - {process.MainWindowTitle}");
                }
            }

            searchBox.TextChanged += (s, e) => UpdateListBox();
            UpdateListBox();

            var selectButton = new Button { Content = "选择" };
            selectButton.Click += (s, e) => ChooseFunc();

            selectWindow.Content = new StackPanel { Children = { searchBox, listBox, selectButton } };
            selectWindow.ShowDialog();
        }

        /// <summary>
        /// 保存配置并启动悬浮窗
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveConfig(object? parameter)
        {
            if (Model.SelectedApp == null || string.IsNullOrEmpty(Model.SelectedApp.AppPath)) return;
            if (!File.Exists(Model.SelectedApp.AppPath))
            {
                MessageBox.Show("关联软件未安装，请检查路径。");
                return;
            }

            var processName = Path.GetFileNameWithoutExtension(Model.SelectedApp.AppPath);
            var targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0) return;

            var targetProcess = targetProcesses[0];
            var targetWindowHandle = targetProcess.MainWindowHandle;
            
            if (!Utils.NativeMethods.IsWindow(targetWindowHandle))
            {
                MessageBox.Show("无法获取有效的窗口句柄。");
                return;
            }

            var appKey = Model.SelectedApp.AppPath;
            if (!_overlayWindows.ContainsKey(appKey))
            {
                var overlayWindow = new OverlayWindow(Model.SelectedApp.TodoItems)
                {
                    Topmost = true,
                    //DataContext = this
                };

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

        /// <summary>
        /// 更新悬浮窗的位置，使其始终位于目标窗口的下方。
        /// </summary>
        /// <param name="overlayWindow"></param>
        /// <param name="targetWindowHandle"></param>
        private static void UpdateOverlayPosition(OverlayWindow overlayWindow, IntPtr targetWindowHandle)
        {
            if (Utils.NativeMethods.GetWindowRect(targetWindowHandle, out var rect))
            {
                // 获取 DPI 缩放因子
                var source = PresentationSource.FromVisual(overlayWindow);
                if (source?.CompositionTarget != null)
                {
                    // 获取 DPI 转换矩阵
                    Matrix transformMatrix = source.CompositionTarget.TransformFromDevice;

                    // 使用 DPI 转换矩阵转换坐标
                    double dpiScaledLeft = rect.Left * transformMatrix.M11;
                    double dpiScaledTop = rect.Bottom * transformMatrix.M22;

                    // 调整悬浮窗位置，确保它不会超出屏幕边界
                    double adjustedLeft = Math.Max(0, dpiScaledLeft);
                    double adjustedTop = Math.Max(0, dpiScaledTop - overlayWindow.ActualHeight);

                    Debug.WriteLine($"原始位置: Left={rect.Left}, Bottom={rect.Bottom}");
                    Debug.WriteLine($"DPI缩放后: Left={dpiScaledLeft}, Top={dpiScaledTop}");
                    Debug.WriteLine($"最终位置: Left={adjustedLeft}, Top={adjustedTop}");

                    // 设置悬浮窗的位置
                    double offsetX = 0; // 水平偏移量
                    double offsetY = 0; // 垂直偏移量，给一个小间距
                    overlayWindow.Left = adjustedLeft + offsetX;
                    overlayWindow.Top = adjustedTop + offsetY;

                    // 可选：调整悬浮窗大小以匹配目标窗口宽度
                    //double targetWidth = (rect.Right - rect.Left) * transformMatrix.M11;
                    //overlayWindow.Width = targetWidth;
                }
                else
                {
                    // 回退方案，当无法获取 DPI 信息时
                    overlayWindow.Left = rect.Left;
                    overlayWindow.Top = rect.Bottom - overlayWindow.ActualHeight;
                }
            }
        }

        /// <summary>
        /// 强制启动关联软件（如果已安装）
        /// </summary>
        /// <param name="parameter"></param>
        private void ForceLaunch(object? parameter)
        {
            if (Model.SelectedApp != null && File.Exists(Model.SelectedApp.AppPath))
            {
                Process.Start(Model.SelectedApp.AppPath);
            }
            else
            {
                MessageBox.Show("关联软件未安装，请检查路径。");
            }
        }

        /// <summary>
        /// 添加待办项到当前选中软件关联的列表中
        /// </summary>
        /// <param name="parameter"></param>
        private void AddTodoItem(object? parameter)
        {
            if (parameter is AppAssociation app)
            {
                app.TodoItems.Add(new TodoItem { Content = "新待办项", IsCompleted = false });
                app.IsExpanded = true;
            }
            else if (Model.SelectedApp != null)
            {
                Model.SelectedApp.TodoItems.Add(new TodoItem { Content = "新待办项", IsCompleted = false });
                Model.SelectedApp.IsExpanded = true;
            }
        }

        // 添加新的方法来处理子待办项
        /// <summary>
        /// 为待办项添加子待办项
        /// </summary>
        /// <param name="parameter">父待办项</param>
        private void AddSubTodoItem(object? parameter)
        {
            if (parameter is TodoItem parentItem && parentItem!=null)
            {
                parentItem.SubItems?.Add(new TodoItem { Content = "新子待办项", IsCompleted = false });
                parentItem.IsExpanded = true;
            }
        }

        private void DeleteTodoItem(object? parameter)
        {
            if (parameter is TodoItem item)
            {
                // 添加更详细的日志输出
                Debug.WriteLine($"尝试删除待办项：ID={item.Id}, Content={item.Content}");
                Debug.WriteLine($"当前应用数量：{Model.AppAssociations.Count}");
                if (Model.SelectedApp != null)
                {
                    Debug.WriteLine($"当前选中应用：{Model.SelectedApp.AppName}, TodoItems数量：{Model.SelectedApp.TodoItems.Count}");
                }

                var result = MessageBox.Show("确定要删除此待办项吗？", "确认删除", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                bool deleted = false;

                // 1. 首先检查当前选中应用的一级待办项
                if (Model.SelectedApp != null)
                {
                    var directMatch = Model.SelectedApp.TodoItems.FirstOrDefault(t => t.Id == item.Id);
                    if (directMatch != null)
                    {
                        Debug.WriteLine("在选中应用的一级待办项中找到匹配，删除中...");
                        Model.SelectedApp.TodoItems.Remove(directMatch);
                        deleted = true;
                    }
                    else
                    {
                        // 2. 在当前选中应用的所有子项中查找
                        Debug.WriteLine("在选中应用的子待办项中查找...");
                        deleted = FindAndRemoveItemById(Model.SelectedApp.TodoItems, item.Id);
                    }
                }

                // 3. 如果仍未找到，在所有应用中查找
                if (!deleted && Model.AppAssociations.Count > 0)
                {
                    Debug.WriteLine("在所有应用中查找待办项...");
                    foreach (var app in Model.AppAssociations)
                    {
                        if (FindAndRemoveItemById(app.TodoItems, item.Id))
                        {
                            deleted = true;
                            break;
                        }
                    }
                }

                if (!deleted)
                {
                    Debug.WriteLine("未找到待办项，删除失败");
                    MessageBox.Show("未能找到要删除的待办项", "删除失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Debug.WriteLine("删除成功");
                }
            }
        }

        // 使用ID匹配的查找和删除方法
        private bool FindAndRemoveItemById(ObservableCollection<TodoItem> items, string itemId)
        {
            // 先检查当前级别
            var directMatch = items.FirstOrDefault(t => t.Id == itemId);
            if (directMatch != null)
            {
                items.Remove(directMatch);
                return true;
            }

            // 递归检查所有子项
            foreach (var item in items.ToList())
            {
                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    if (FindAndRemoveItemById(item.SubItems, itemId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 在待办项集合中搜索并删除指定待办项
        /// </summary>
        private bool SearchAndRemoveInTodoItems(ObservableCollection<TodoItem> items, TodoItem itemToRemove)
        {
            // 直接检查当前集合
            if (items.Contains(itemToRemove))
            {
                items.Remove(itemToRemove);
                return true;
            }

            // 递归检查每个子项集合
            foreach (var item in items.ToList()) // 使用 ToList() 避免集合修改异常
            {
                if (item.SubItems != null && RemoveItemRecursively(item.SubItems, itemToRemove))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 递归搜索并删除待办项
        /// </summary>
        private static bool RemoveItemRecursively(ObservableCollection<TodoItem> items, TodoItem itemToRemove)
        {
            // 检查参数
            if (items == null || itemToRemove == null)
            {
                Debug.WriteLine("items 或 itemToRemove 为 null");
                return false;
            }

            // 直接检查当前集合
            var isContains = items.Contains(itemToRemove);
            if (isContains)
            {
                items.Remove(itemToRemove);
                return true;
            }

            // 递归检查每个子项的集合
            foreach (var item in items.ToList()) // 使用 ToList() 避免集合修改异常
            {
                if (item.SubItems != null && RemoveItemRecursively(item.SubItems, itemToRemove))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清理所有关联的悬浮窗和定时器
        /// </summary>
        public void Cleanup()
        {
            foreach (var window in _overlayWindows.Values)
            {
                window.Close();
            }
            _overlayWindows.Clear();
        }
    }
}
