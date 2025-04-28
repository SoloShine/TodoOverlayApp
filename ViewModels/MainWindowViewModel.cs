
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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


        private MainWindowModel model;
        /// <summary>
        /// Model 属性，用于绑定到主窗口的视图模型。
        /// </summary>
        public MainWindowModel Model
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
        public ICommand ForceLaunchCommand { get; }
        public ICommand AddTodoItemCommand { get; }
        public ICommand DeleteTodoItemCommand { get; }

        public ICommand AddSubTodoItemCommand { get; }
        public ICommand DeleteSubTodoItemCommand { get; }

        public ICommand ToggleIsInjectedCommand { get; }

        public MainWindowViewModel()
        {
            // 尝试从文件加载配置，如果加载失败则创建新的模型
            var loadedModel = MainWindowModel.LoadFromFile();
            if (loadedModel != null)
            {
                model = loadedModel;
            }
            else
            {
                model = new MainWindowModel();
            }

            AddAppCommand = new RelayCommand(AddApp);
            RemoveAppCommand = new RelayCommand(RemoveApp);
            SelectAppCommand = new RelayCommand(SelectApp);
            SelectRunningAppCommand = new RelayCommand(SelectRunningApp);
            ForceLaunchCommand = new RelayCommand(ForceLaunch);
            AddTodoItemCommand = new RelayCommand(AddTodoItem);
            DeleteTodoItemCommand = new RelayCommand(DeleteTodoItem);
            AddSubTodoItemCommand = new RelayCommand(AddSubTodoItem);
            DeleteSubTodoItemCommand = new RelayCommand(DeleteTodoItem); // 复用现有的删除方法
            ToggleIsInjectedCommand = new RelayCommand(ToggleIsInjected);

        }



        /// <summary>
        /// 添加一个应用。将新应用的路径设置为选中状态的AppPath，并将其加入到集合中。
        /// </summary>
        /// <param name="parameter"></param>
        private void AddApp(object? parameter)
        {
            var newApp = new AppAssociation();
            Model.AppAssociations.Add(newApp);
            //弹出一个二选一选项
            var result = MessageBox.Show("是否为当前运行软件添加待办事项？", "选择操作", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // 执行与当前运行软件相关的逻辑
                SelectRunningApp(null);
            }
            else if (result == MessageBoxResult.No)
            {
                // 选择文件路径的逻辑
                SelectApp(null);
            }
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
                Title = "选择当前运行软件"+"("+processes.Count+")",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.Height, // 根据内容调整高度
                MaxHeight = 600 // 设置最大高度，避免窗口过大
            };

            // 主布局容器
            var mainPanel = new DockPanel { LastChildFill = true };

            // 搜索框放在顶部
            var searchBox = new TextBox
            {
                Text = "搜索进程...",
                Margin = new Thickness(5),
                Padding = new Thickness(3)
            };
            DockPanel.SetDock(searchBox, Dock.Top);
            mainPanel.Children.Add(searchBox);

            // 选择按钮放在底部
            var selectButton = new Button
            {
                Content = "选择",
                Margin = new Thickness(5),
                Padding = new Thickness(5, 3, 5, 3),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            DockPanel.SetDock(selectButton, Dock.Bottom);
            mainPanel.Children.Add(selectButton);

            // 列表框包装在ScrollViewer中以提供滚动功能
            var listBox = new ListBox
            {
                Margin = new Thickness(5),
                MinHeight = 200 // 确保有足够的高度显示多个条目
            };
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto, // 自动显示滚动条
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = listBox
            };
            mainPanel.Children.Add(scrollViewer); // 添加到主面板

            void ChooseFunc()
            {
                if (listBox.SelectedIndex != -1)
                {
                    var process = processes[listBox.SelectedIndex];
                    try
                    {
                        //默认最后一个节点为操作节点
                        Model.AppAssociations.Last().AppPath = process.MainModule?.FileName;
                        Model.AppAssociations.Last().AppName = process.MainModule?.ModuleName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法获取进程路径: {ex.Message}");
                    }
                }
                selectWindow.Close();
            }

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

            searchBox.GotFocus += (s, e) => {
                if (searchBox.Text == "搜索进程...")
                    searchBox.Text = "";
            };

            searchBox.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                    searchBox.Text = "搜索进程...";
            };

            searchBox.TextChanged += (s, e) => UpdateListBox();
            selectButton.Click += (s, e) => ChooseFunc();

            // 初始化列表
            UpdateListBox();

            selectWindow.Content = mainPanel;
            selectWindow.ShowDialog();
        }

        /// <summary>
        /// 切换软件待办项的注入状态
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ToggleIsInjected(object? parameter)
        {
            if (parameter is AppAssociation app)
            {
                if (!File.Exists(app.AppPath))
                {
                    MessageBox.Show("关联软件未安装，请检查路径。");
                    return;
                }

                var processName = Path.GetFileNameWithoutExtension(app.AppPath);
                var targetProcesses = Process.GetProcessesByName(processName);
                if (targetProcesses.Length == 0) return;

                var targetProcess = targetProcesses[0];
                var targetWindowHandle = targetProcess.MainWindowHandle;

                if (!Utils.NativeMethods.IsWindow(targetWindowHandle))
                {
                    MessageBox.Show("无法获取有效的窗口句柄。");
                    return;
                }

                var appKey = app.AppPath;
                //由于绑定了属性，这里已经发生了变化，所以需要反着判定
                if (app.IsInjected)
                {
                    // 启动悬浮窗
                    if (!_overlayWindows.ContainsKey(appKey))
                    {
                        var overlayWindow = new OverlayWindow(app.TodoItems)
                        {
                            Topmost = true
                        };
                        ((OverlayWindow)overlayWindow).ApplyOverlaySettings();

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
                else
                {
                    // 关闭悬浮窗
                    if (_overlayWindows.ContainsKey(app.AppPath))
                    {
                        _overlayWindows[app.AppPath].Close();
                        _overlayWindows.Remove(app.AppPath);
                    }
                }
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
        /// 强制启动关联软件（如果已安装）或将其从最小化/隐藏状态恢复到前台
        /// </summary>
        /// <param name="parameter"></param>
        private void ForceLaunch(object? parameter)
        {
            if (parameter is AppAssociation app)
            {
                if (File.Exists(app.AppPath))
                {
                    // 获取目标进程名
                    string processName = Path.GetFileNameWithoutExtension(app.AppPath);
                    Process[] processes = Process.GetProcessesByName(processName);

                    if (processes.Length > 0)
                    {
                        // 进程已在运行
                        Process targetProcess = processes[0];
                        int targetProcessId = targetProcess.Id;

                        Debug.WriteLine($"找到正在运行的进程: {processName}, ID: {targetProcessId}");

                        // 尝试使用主窗口句柄
                        IntPtr mainWindowHandle = targetProcess.MainWindowHandle;
                        if (mainWindowHandle != IntPtr.Zero && Utils.NativeMethods.IsWindow(mainWindowHandle))
                        {
                            Debug.WriteLine("找到主窗口句柄，尝试将其激活");
                            ActivateWindow(mainWindowHandle, app.AppName);
                        }
                        else
                        {
                            // 主窗口不存在或不可用，查找进程的所有窗口
                            Debug.WriteLine("主窗口不可用，尝试查找进程的所有窗口");
                            List<IntPtr> windowHandles = FindWindowsForProcess(targetProcessId);

                            if (windowHandles.Count > 0)
                            {
                                // 优先选择可见窗口
                                IntPtr visibleWindow = windowHandles.FirstOrDefault(
                                    hwnd => Utils.NativeMethods.IsWindowVisible(hwnd));

                                if (visibleWindow != IntPtr.Zero)
                                {
                                    Debug.WriteLine("找到可见窗口，尝试激活");
                                    ActivateWindow(visibleWindow, app.AppName);
                                }
                                else
                                {
                                    // 如果没有可见窗口，尝试激活第一个找到的窗口
                                    Debug.WriteLine("没有找到可见窗口，尝试激活第一个窗口");
                                    ActivateWindow(windowHandles[0], app.AppName);
                                }
                            }
                            else
                            {
                                // 没有找到窗口，提示用户
                                Debug.WriteLine("没有找到任何窗口");

                                MessageBoxResult result = MessageBox.Show(
                                    $"应用 {app.AppName ?? Path.GetFileName(app.AppPath)} 正在运行，但未找到可以激活的窗口。\n\n" +
                                    "这可能是因为:\n" +
                                    "- 程序以系统服务或后台进程方式运行\n" +
                                    "- 程序窗口被特殊隐藏\n" +
                                    "- 程序窗口属于不同的用户会话\n\n" +
                                    "您想要尝试启动一个新的程序实例吗？",
                                    "无法激活窗口",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        // 尝试启动新实例，但不关闭现有进程
                                        Process.Start(new ProcessStartInfo(app.AppPath) { UseShellExecute = true });
                                        Debug.WriteLine("尝试启动新实例");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"启动新实例失败: {ex.Message}");
                                        MessageBox.Show($"启动应用失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // 应用未运行，启动它
                        try
                        {
                            Process.Start(new ProcessStartInfo(app.AppPath) { UseShellExecute = true });
                            Debug.WriteLine($"启动应用: {app.AppName ?? Path.GetFileName(app.AppPath)}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"启动应用失败: {ex.Message}");
                            MessageBox.Show($"启动应用失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("关联软件未安装，请检查路径。");
                }
            }
        }

        /// <summary>
        /// 激活指定的窗口并将其带到前台
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="appName">应用名称，用于调试输出</param>
        private static void ActivateWindow(IntPtr windowHandle, string? appName)
        {
            try
            {
                // 首先检查窗口是否最小化
                bool isIconic = Utils.NativeMethods.IsIconic(windowHandle);
                if (isIconic)
                {
                    Debug.WriteLine($"窗口已最小化，先恢复它");
                    Utils.NativeMethods.ShowWindow(windowHandle, Utils.NativeMethods.SW_RESTORE);
                }

                // 获取窗口当前样式
                int style = Utils.NativeMethods.GetWindowLong(windowHandle, Utils.NativeMethods.GWL_STYLE);

                // 如果窗口不可见，尝试使其可见
                if ((style & Utils.NativeMethods.WS_VISIBLE) == 0)
                {
                    Debug.WriteLine("窗口不可见，尝试使其可见");
                    Utils.NativeMethods.SetWindowLong(
                        windowHandle,
                        Utils.NativeMethods.GWL_STYLE,
                        style | Utils.NativeMethods.WS_VISIBLE);

                    Utils.NativeMethods.ShowWindow(windowHandle, Utils.NativeMethods.SW_SHOW);
                }

                // 尝试多种方法将窗口带到前台
                bool foregroundResult = Utils.NativeMethods.SetForegroundWindow(windowHandle);
                Utils.NativeMethods.BringWindowToTop(windowHandle);

                // 如果SetForegroundWindow失败，尝试闪烁窗口提醒用户
                if (!foregroundResult)
                {
                    Debug.WriteLine("SetForegroundWindow失败，尝试闪烁窗口");
                    Utils.NativeMethods.FlashWindow(windowHandle, true);
                }

                Debug.WriteLine($"已尝试激活窗口: {appName}, 最小化状态: {isIconic}, SetForegroundWindow结果: {foregroundResult}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"激活窗口时发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找指定进程ID的所有窗口句柄
        /// </summary>
        /// <param name="processId">进程ID</param>
        /// <returns>窗口句柄列表</returns>
        private static List<IntPtr> FindWindowsForProcess(int processId)
        {
            List<IntPtr> result = new List<IntPtr>();

            Utils.NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                int windowProcessId;
                Utils.NativeMethods.GetWindowThreadProcessId(hWnd, out windowProcessId);

                if (windowProcessId == processId)
                {
                    // 获取窗口类名和标题，用于调试
                    StringBuilder className = new StringBuilder(256);
                    StringBuilder windowText = new StringBuilder(256);
                    Utils.NativeMethods.GetClassName(hWnd, className, className.Capacity);
                    Utils.NativeMethods.GetWindowText(hWnd, windowText, windowText.Capacity);

                    Debug.WriteLine($"找到进程 {processId} 的窗口: 句柄={hWnd}, 类名={className}, 标题={windowText}, 可见={Utils.NativeMethods.IsWindowVisible(hWnd)}");

                    result.Add(hWnd);
                }

                return true; // 继续枚举
            }, IntPtr.Zero);

            return result;
        }

        /// <summary>
        /// 添加待办项到当前选中软件关联的列表中
        /// </summary>
        /// <param name="parameter"></param>
        private void AddTodoItem(object? parameter)
        {
            if (parameter is AppAssociation app)
            {
                app.TodoItems.Add(new TodoItem { Content = "新待办项", IsCompleted = false, ParentId=app.Id });
                app.IsExpanded = true;
            }
            Model.SaveToFileAsync().ConfigureAwait(false);
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
                parentItem.SubItems?.Add(new TodoItem { Content = "新子待办项", IsCompleted = false, ParentId = parentItem.Id });
                parentItem.IsExpanded = true;
                Model.SaveToFileAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 删除待办项（包括子待办项）
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteTodoItem(object? parameter)
        {
            if (parameter is TodoItem item)
            {
                var result = MessageBox.Show("确定要删除此待办项吗？", "确认删除", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                bool deleted = false;

                var app = Model.AppAssociations.FirstOrDefault(a => a.TodoItems.Contains(item));

                // 1. 首先检查当前选中应用的一级待办项
                if (app != null)
                {
                    var directMatch = app.TodoItems.FirstOrDefault(t => t.Id == item.Id);
                    if (directMatch != null)
                    {
                        Debug.WriteLine("在选中应用的一级待办项中找到匹配，删除中...");
                        app.TodoItems.Remove(directMatch);
                        deleted = true;
                    }
                    else
                    {
                        // 2. 在当前选中应用的所有子项中查找
                        Debug.WriteLine("在选中应用的子待办项中查找...");
                        deleted = FindAndRemoveItemById(app.TodoItems, item.Id);
                    }
                }

                // 3. 如果仍未找到，在所有应用中查找
                if (!deleted && Model.AppAssociations.Count > 0)
                {
                    Debug.WriteLine("在所有应用中查找待办项...");
                    foreach (var app2 in Model.AppAssociations)
                    {
                        if (FindAndRemoveItemById(app2.TodoItems, item.Id))
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
