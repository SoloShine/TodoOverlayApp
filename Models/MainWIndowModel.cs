using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TodoOverlayApp.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            AppAssociations.CollectionChanged += OnTodoItemsChanged;
            // 默认添加一个普通的待办项集合、一个软件集合
            //获取notepad.exe的路径
            var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            AppAssociations.Add(new AppAssociation()
            {
                IsNonApp = true,
                AppName = "普通待办",
                AppPath = string.Empty,
                TodoItems = [
                    new() { Content = "普通待办项1", IsCompleted = false, ParentId = string.Empty }, 
                    new() { Content = "普通待办项2", IsCompleted = false, ParentId = string.Empty },
                ]
            });
            AppAssociations.Add(new AppAssociation()
            {
                IsNonApp = false,
                AppName = "记事本",
                AppPath = notepadPath,
                IsInjected = true,
                TodoItems =  [
                    new() { Content = "软件待办项1", IsCompleted = false, ParentId = string.Empty },
                    new() { Content = "软件待办项2", IsCompleted = false, ParentId = string.Empty },
                ]
            });

        }

        #region INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //保存配置到文件
            //SaveToFileAsync().ConfigureAwait(false);
        }
        #endregion

        #region 属性
        /// <summary>
        /// 待办项集合所在的程序集合，用于在程序中切换不同的待办项集合
        /// </summary>
        public ObservableCollection<AppAssociation> AppAssociations { get; set; } = [];

        // <summary>
        /// 当前主题类型：Default或Dark
        /// </summary>
        private string _themeType = "Default";
        public string ThemeType
        {
            get => _themeType;
            set
            {
                if (_themeType != value)
                {
                    _themeType = value;
                    OnPropertyChanged(nameof(ThemeType));
                    SaveToFileAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 主题颜色（十六进制颜色代码）
        /// </summary>
        private string _themeColor = "#2196F3";
        public string ThemeColor
        {
            get => _themeColor;
            set
            {
                if (_themeColor != value)
                {
                    _themeColor = value;
                    OnPropertyChanged(nameof(ThemeColor));
                    SaveToFileAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 悬浮窗背景颜色（十六进制颜色代码）
        /// </summary>
        private string _overlayBackground = "#D3D3D3"; // LightGray 的十六进制值
        public string OverlayBackground
        {
            get => _overlayBackground;
            set
            {
                if (_overlayBackground != value)
                {
                    _overlayBackground = value;
                    OnPropertyChanged(nameof(OverlayBackground));
                    SaveToFileAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 悬浮窗不透明度 (0.0-1.0)
        /// </summary>
        private double _overlayOpacity = 0.3;
        public double OverlayOpacity
        {
            get => _overlayOpacity;
            set
            {
                if (_overlayOpacity != value)
                {
                    _overlayOpacity = Math.Clamp(value, 0.1, 1.0); // 确保在有效范围内
                    OnPropertyChanged(nameof(OverlayOpacity));
                    SaveToFileAsync().ConfigureAwait(false);
                }
            }
        }
        #endregion

        #region 方法

        private static readonly string ConfigPath = "app_associations.json";
        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new() {
            //PropertyNameCaseInsensitive = true,
            //ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true };
        /// <summary>
        /// 当待办项集合发生变化时，触发保存配置到文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTodoItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveToFileAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 异步保存配置到文件（延迟执行）
        /// </summary>
        /// <returns></returns>
        public async Task SaveToFileAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, CachedJsonSerializerOptions);
                await File.WriteAllTextAsync(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用主题设置
        /// </summary>
        public void ApplyThemeSettings()
        {
            try
            {
                // 应用主题类型（浅色/深色）
                var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

                // 清除现有的主题资源
                var themeDicts = new List<System.Windows.ResourceDictionary>();
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

                // 添加主题类型资源
                string skinPath = ThemeType == "Dark"
                    ? "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml"
                    : "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml";

                mergedDicts.Add(new System.Windows.ResourceDictionary
                {
                    Source = new Uri(skinPath)
                });

                // 重新加载主题
                mergedDicts.Add(new System.Windows.ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                });

                // 应用主题颜色
                if (!string.IsNullOrEmpty(ThemeColor))
                {
                    var themeColor = (Color)ColorConverter.ConvertFromString(ThemeColor);
                    System.Windows.Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(themeColor);
                    System.Windows.Application.Current.Resources["DarkPrimaryBrush"] = new SolidColorBrush(
                        Color.FromRgb(
                            (byte)Math.Max(0, themeColor.R - 40),
                            (byte)Math.Max(0, themeColor.G - 40),
                            (byte)Math.Max(0, themeColor.B - 40)
                        ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"应用主题设置时出错: {ex.Message}");
            }
        }

        public static MainWindowModel? LoadFromFile()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);

                    var result = JsonSerializer.Deserialize<MainWindowModel>(json, CachedJsonSerializerOptions);
                    if (result != null)
                    {
                        Debug.WriteLine($"从文件加载成功：{result.AppAssociations.Count} 个应用关联");

                        // 特殊处理 - 确保所有子集合都被初始化
                        foreach (var app in result.AppAssociations)
                        {
                            app.TodoItems ??= [];

                            foreach (var item in app.TodoItems)
                            {
                                if (item.SubItems == null)
                                    item.SubItems = new ObservableCollection<TodoItem>();

                                // 递归确保所有子项的SubItems不为空
                                EnsureSubItemsInitialized(item);
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载配置失败: {ex.Message}");
                Debug.WriteLine($"异常详情: {ex}");
            }

            return new MainWindowModel();
        }

        private static void EnsureSubItemsInitialized(TodoItem item)
        {
            item.SubItems ??= [];

            foreach (var subItem in item.SubItems)
            {
                EnsureSubItemsInitialized(subItem);
            }
        }
        #endregion

    }
}
