using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using TodoOverlayApp.Services.Database.Repositories;

namespace TodoOverlayApp.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            //AppAssociations.CollectionChanged += OnTodoItemsChanged;
        }

        #region INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 属性
        /// <summary>
        /// 待办项集合所在的程序集合，用于在程序中切换不同的待办项集合
        /// </summary>
        //public ObservableCollection<AppAssociationModel> AppAssociations { get; set; } = [];
        private ObservableCollection<TodoItemModel> todoItems { get; set; } = [];
        /// <summary>
        /// 待办项集合，用于显示和编辑待办项
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<TodoItemModel> TodoItems
        {
            get => todoItems;
            set
            {
                if (todoItems != value)
                {
                    todoItems = value;
                    OnPropertyChanged(nameof(TodoItems));
                }
            }
        }

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

        /// <summary>
        /// 从文件加载配置（静态方法）
        /// </summary>
        /// <returns></returns>
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
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载配置失败: {ex.Message}");
                Debug.WriteLine($"异常详情: {ex}");
            }

            return new();
        }

        #region 从数据库加载待办事项

        /// <summary>
        /// 从数据库加载待办事项（静态方法）
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<TodoItemModel> LoadFromDatabase()
        {
            var items = App.TodoItemRepository.GetAllAsync().Result;
            //组装TodoItemModel
            List<TodoItemModel> todoItemModels = BuildTodoItemTree([.. items]);
            return [.. todoItemModels];
        }


        /// <summary>
        /// 将TodoItem列表转换为TodoItemModel树结构
        /// </summary>
        /// <param name="items">TodoItem列表</param>
        /// <returns>树形结构的TodoItemModel根节点列表</returns>
        public static List<TodoItemModel> BuildTodoItemTree(List<TodoItem> items)
        {
            return BuildTree<TodoItem, TodoItemModel>(
                items,
                item => item.Id,
                item => item.ParentId,
                item => new TodoItemModel(item),
                model => model.SubItems,
                (parent, child) => parent.SubItems.Add(child)
            );
        }

        /// <summary>
        /// 通用方法：将平面列表结构转换为树形结构
        /// </summary>
        /// <typeparam name="TItem">源项类型</typeparam>
        /// <typeparam name="TItemModel">目标树节点类型（必须能从TItem构造）</typeparam>
        /// <param name="items">源数据列表</param>
        /// <param name="getItemId">获取项ID的函数</param>
        /// <param name="getParentId">获取父项ID的函数</param>
        /// <param name="createModel">从源项创建模型的函数</param>
        /// <param name="getChildren">获取子项集合的函数</param>
        /// <param name="addChild">添加子项的函数</param>
        /// <returns>树形结构的根节点列表</returns>
        public static List<TItemModel> BuildTree<TItem, TItemModel>(
            List<TItem> items,
            Func<TItem, string> getItemId,
            Func<TItem, string?> getParentId,
            Func<TItem, TItemModel> createModel,
            Func<TItemModel, ICollection<TItemModel>> getChildren,
            Action<TItemModel, TItemModel> addChild)
        {
            // 创建一个字典，用于快速查找模型实例
            var modelLookup = items.ToDictionary(getItemId, createModel);

            // 定义一个列表，用于存储根节点
            var rootNodes = new List<TItemModel>();

            foreach (var item in items)
            {
                var parentId = getParentId(item);
                var itemId = getItemId(item);
                var model = modelLookup[itemId];

                // 如果 ParentId 为空或找不到父节点，则认为是根节点
                if (string.IsNullOrEmpty(parentId) || !modelLookup.TryGetValue(parentId, out TItemModel? parentModel))
                {
                    rootNodes.Add(model);
                }
                else
                {
                    addChild(parentModel, model);
                }
            }

            return rootNodes;
        }

        #endregion
        #endregion
    }
}
