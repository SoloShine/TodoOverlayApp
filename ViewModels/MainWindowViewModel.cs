
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
using System.Windows.Threading;
using HandyControl.Tools;
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

        //public ICommand AddAppCommand { get; }
        //public ICommand RemoveAppCommand { get; }
        //public ICommand SelectAppCommand { get; }
        //public ICommand SelectRunningAppCommand { get; }
        public ICommand ForceLaunchCommand { get; }
        //public ICommand AddTodoItemCommand { get; }
        public ICommand DeleteTodoItemCommand { get; }

        public ICommand AddSubTodoItemCommand { get; }
        public ICommand DeleteSubTodoItemCommand { get; }

        public ICommand ToggleIsInjectedCommand { get; }

        public ICommand ResetAppConfigCommand { get; }
        public ICommand ResetTodoCommand { get; }
        //public ICommand EditAppCommand { get; }
        public ICommand EditTodoItemCommand { get; }

        private readonly DispatcherTimer autoInjectTimer;
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

            //从数据库加载待办事项todo
            model.TodoItems = MainWindowModel.LoadFromDatabase();

            //AddAppCommand = new RelayCommand(AddApp);
            //RemoveAppCommand = new RelayCommand(RemoveApp);
            //SelectAppCommand = new RelayCommand(SelectApp);
            //SelectRunningAppCommand = new RelayCommand(SelectRunningApp);
            ForceLaunchCommand = new RelayCommand(ForceLaunch);
            //AddTodoItemCommand = new RelayCommand(AddTodoItem);
            DeleteTodoItemCommand = new RelayCommand(DeleteTodoItem);
            AddSubTodoItemCommand = new RelayCommand(AddSubTodoItem);
            DeleteSubTodoItemCommand = new RelayCommand(DeleteTodoItem); // 复用现有的删除方法
            ToggleIsInjectedCommand = new RelayCommand(ToggleIsInjected);
            ResetAppConfigCommand = new RelayCommand(ResetAppConfig);
            ResetTodoCommand = new RelayCommand(ResetTodo);
            //EditAppCommand = new RelayCommand(EditApp);
            EditTodoItemCommand = new RelayCommand(EditTodoItem);
            //周期遍历model中AppAssociations，当AppAssociation中IsInjected为true的时候，尝试自动注入OverlayWindow
            autoInjectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            autoInjectTimer.Tick += AutoInjectOverlays;
            autoInjectTimer.Start();
        }

        /// <summary>
        /// 重置应用数据，创建新的MainWindowModel实例并清理现有资源
        /// </summary>
        /// <param name="parameter"></param>
        private void ResetAppConfig(object? parameter)
        {
            var result = MessageBox.Show(
                "确定要重置应用吗？这将清除除开todo外的所有数据，且无法恢复。",
                "确认重置",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // 先清理现有资源
                Cleanup();

                // 创建新的MainWindowModel实例并赋值
                Model = new MainWindowModel();

                // 保存新模型到文件
                Model.SaveToFileAsync().ConfigureAwait(false);

                MessageBox.Show("应用已重置成功！", "重置完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetTodo(object? parameter)
        {
            var result = MessageBox.Show(
                "确定要重置所有todo数据吗，清除之前请做好备份，否则无法恢复。",
                "确认重置",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                //清除当前载入数据
                Model.TodoItems = new ObservableCollection<TodoItemModel>();
                MessageBox.Show("重置成功！", "重置完成", MessageBoxButton.OK, MessageBoxImage.Information);
                //清除数据库数据，并初始化
                App.DatabaseInitializer.ResetDatabaseAsync().ConfigureAwait(false);
                //重新加载数据
                Model.TodoItems = MainWindowModel.LoadFromDatabase();
            }
        }

        /// <summary>
        /// 编辑一个待办事项。弹出编辑窗口，如果用户点击确定则更新原始对象并保存配置到文件。
        /// </summary>
        /// <param name="parameter"></param>
        private void EditTodoItem(object? parameter)
        {
            if (parameter is not TodoItemModel todo) return;
            var editWindow = new EditTodoItemWindow(todo);
            if (editWindow.ShowDialog() == true)
            {
                // 更新原始对象
                todo.Content = editWindow.Todo.Content;
                todo.Description = editWindow.Todo.Description;
                todo.AppPath = editWindow.Todo.AppPath;
                todo.Name = editWindow.Todo.Name;

                // 保存配置到文件
                Model.SaveToFileAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 自动注入悬浮窗到当前前台窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoInjectOverlays(object? sender, EventArgs e)
        {
            // 获取当前前台窗口句柄
            IntPtr foregroundHandle = Utils.NativeMethods.GetForegroundWindow();

            foreach (var item in Model.TodoItems)
            {
                if (!item.IsInjected || item.TodoItemType != TodoItemType.App || string.IsNullOrEmpty(item.AppPath) || !File.Exists(item.AppPath))
                    continue;

                string processName = Path.GetFileNameWithoutExtension(item.AppPath);
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    // 如果进程未运行且之前已经创建了 overlay，则关闭它
                    if (_overlayWindows.TryGetValue(item.AppPath, out OverlayWindow? value))
                    {
                        value.Close();
                        _overlayWindows.Remove(item.AppPath);
                    }
                    continue;
                }

                Process targetProcess = processes[0];
                IntPtr targetWindowHandle = targetProcess.MainWindowHandle;
                if (!Utils.NativeMethods.IsWindow(targetWindowHandle))
                    continue;

                // 调用公共方法处理悬浮窗逻辑
                HandleOverlayWindow(item, targetWindowHandle, foregroundHandle);
            }
        }

        /// <summary>
        /// 切换软件待办项的注入状态
        /// </summary>
        /// <param name="parameter"></param>
        private void ToggleIsInjected(object? parameter)
        {
            if (parameter is TodoItemModel item)
            {
                if (item.TodoItemType != TodoItemType.App) return;
                if (!File.Exists(item.AppPath))
                {
                    MessageBox.Show("关联软件未安装，请检查路径。");
                    return;
                }

                var processName = Path.GetFileNameWithoutExtension(item.AppPath);
                var targetProcesses = Process.GetProcessesByName(processName);
                if (targetProcesses.Length == 0) return;

                var targetProcess = targetProcesses[0];
                var targetWindowHandle = targetProcess.MainWindowHandle;

                if (!Utils.NativeMethods.IsWindow(targetWindowHandle))
                {
                    MessageBox.Show("无法获取有效的窗口句柄。");
                    return;
                }

                // 调用公共方法处理悬浮窗逻辑
                HandleOverlayWindow(item, targetWindowHandle, Utils.NativeMethods.GetForegroundWindow());

                // 所有关联该软件的todo都要更新注入状态
                void UpdateIsInjected(TodoItemModel todo)
                {
                    todo.IsInjected = item.IsInjected;
                    if (todo.SubItems != null)
                        foreach (var subItem in todo.SubItems)
                            UpdateIsInjected(subItem);
                }

                UpdateIsInjected(item);

            }
        }

        /// <summary>
        /// 处理悬浮窗的创建、更新和关闭逻辑。
        /// </summary>
        /// <param name="item">目标应用</param>
        /// <param name="targetWindowHandle">目标窗口句柄</param>
        /// <param name="foregroundHandle">当前前台窗口句柄</param>
        private void HandleOverlayWindow(TodoItemModel item, IntPtr targetWindowHandle, IntPtr foregroundHandle)
        {
            var appKey = item.AppPath;
            if (string.IsNullOrEmpty(appKey)) return;
            // 若目标窗口为前台且尚未创建 overlay，则创建 overlay
            if (foregroundHandle == targetWindowHandle)
            {
                if (!_overlayWindows.ContainsKey(appKey))
                {
                    Debug.WriteLine("窗口需要创建");
                    //从所有todo中筛选出当前应用的todo项，并创建overlay窗口，注意这里是tree，不是list，所以不能用linq筛选
                    //尝试从数据库中获取数据后再重组tree结构
                    var apps = App.TodoItemRepository.GetByAppPathAsync(appKey).Result;
                    var appTrees = MainWindowModel.BuildTodoItemTree([.. apps]);
                    var overlayWindow = new OverlayWindow([.. appTrees])
                    {
                        Topmost = false
                    };
                    overlayWindow.ApplyOverlaySettings();
                    overlayWindow.Show();

                    // 建立父子关系（使 overlay 显示在目标窗口上方）
                    _ = Utils.NativeMethods.SetWindowLong(
                        overlayWindow.GetHandle(),
                        Utils.NativeMethods.GWL_HWNDPARENT,
                        targetWindowHandle.ToInt32());

                    // 定时器持续更新 overlay 的位置
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(100)
                    };
                    timer.Tick += (s, args) =>
                    {
                        UpdateOverlayPosition(overlayWindow, targetWindowHandle);
                        // 获取目标窗口上方的窗口句柄
                        IntPtr hAbove = Utils.NativeMethods.GetWindow(targetWindowHandle, Utils.NativeMethods.GW_HWNDPREV);
                        if (hAbove == IntPtr.Zero)
                            hAbove = targetWindowHandle;
                        Utils.NativeMethods.SetWindowPos(
                            overlayWindow.GetHandle(),
                            hAbove,
                            0, 0, 0, 0,
                            Utils.NativeMethods.SWP_NOMOVE | Utils.NativeMethods.SWP_NOSIZE | Utils.NativeMethods.SWP_NOACTIVATE);
                    };
                    timer.Start();
                    overlayWindow.Closed += (s, args) => timer.Stop();

                    _overlayWindows[appKey] = overlayWindow;
                }
            }
            else
            {
                // 如果目标最小化或关闭，则关闭 overlay，避免冗余窗口
                if (Utils.NativeMethods.IsIconic(targetWindowHandle) || !Utils.NativeMethods.IsWindowVisible(targetWindowHandle))
                {
                    if (_overlayWindows.TryGetValue(appKey, out OverlayWindow? value))
                    {
                        Debug.WriteLine("窗口需要删除");
                        value.Close();
                        _overlayWindows.Remove(appKey);
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
            if (parameter is TodoItemModel item)
            {
                if (item.TodoItemType != TodoItemType.App) return;
                if (File.Exists(item.AppPath))
                {
                    // 获取目标进程名
                    string processName = Path.GetFileNameWithoutExtension(item.AppPath);
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
                            ActivateWindow(mainWindowHandle, item.Name);
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
                                    ActivateWindow(visibleWindow, item.Name);
                                }
                                else
                                {
                                    // 如果没有可见窗口，尝试激活第一个找到的窗口
                                    Debug.WriteLine("没有找到可见窗口，尝试激活第一个窗口");
                                    ActivateWindow(windowHandles[0], item.Name);
                                }
                            }
                            else
                            {
                                // 没有找到窗口，提示用户
                                Debug.WriteLine("没有找到任何窗口");

                                MessageBoxResult result = MessageBox.Show(
                                    $"应用 {item.Name ?? Path.GetFileName(item.AppPath)} 正在运行，但未找到可以激活的窗口。\n\n" +
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
                                        Process.Start(new ProcessStartInfo(item.AppPath) { UseShellExecute = true });
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
                            Process.Start(new ProcessStartInfo(item.AppPath) { UseShellExecute = true });
                            Debug.WriteLine($"启动应用: {item.Name ?? Path.GetFileName(item.AppPath)}");
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
        /// 为待办项添加子待办项
        /// </summary>
        /// <param name="parameter">父待办项</param>
        private void AddSubTodoItem(object? parameter)
        {
            //没有父项的时候，直接添加到根节点
            if(parameter == null)
            {
                var item = new TodoItemModel() { Id = Guid.NewGuid().ToString(), Content = "新待办项", IsCompleted = false };
                Model.TodoItems.Add(item);
                App.TodoItemRepository.AddAsync(item).ConfigureAwait(false);
                return;
            }
            if (parameter is TodoItemModel parentItem && parentItem!=null)
            {
                var item = new TodoItemModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = "新子待办项",
                    IsCompleted = false,
                    ParentId = parentItem.Id,
                    IsExpanded = true,
                    Name = parentItem.Name,
                    AppPath = parentItem.AppPath,
                    IsInjected = parentItem.IsInjected,
                    TodoItemType = parentItem.TodoItemType
                };
                parentItem.IsExpanded = true;
                parentItem.SubItems?.Add(item);
                App.TodoItemRepository.AddAsync(item).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 删除待办项（包括子待办项）
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteTodoItem(object? parameter)
        {
            if (parameter is TodoItemModel item)
            {
                var result = MessageBox.Show("确定要删除此待办项吗？", "确认删除", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                FindAndRemoveItemById(Model.TodoItems, item.Id);
            }
        }

        /// <summary>
        /// 使用ID匹配的查找和删除方法
        /// </summary>
        /// <param name="items"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static bool FindAndRemoveItemById(ObservableCollection<TodoItemModel> items, string itemId)
        {
            // 先检查当前级别
            var directMatch = items.FirstOrDefault(t => t.Id == itemId);
            if (directMatch != null)
            {
                items.Remove(directMatch);
                //调用数据库删除操作
                App.TodoItemRepository.DeleteAsync(itemId).ConfigureAwait(false);
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
        /// 清理所有关联的悬浮窗和定时器
        /// </summary>
        public void Cleanup()
        {
            foreach (var window in _overlayWindows.Values)
            {
                window.Close();
            }
            Model.SaveToFileAsync().ConfigureAwait(false);
            _overlayWindows.Clear();
            autoInjectTimer.Stop();
        }
    }
}
