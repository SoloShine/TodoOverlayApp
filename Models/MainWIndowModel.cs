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

namespace TodoOverlayApp.Models
{
    public class MainWIndowModel : INotifyPropertyChanged
    {
        public MainWIndowModel()
        {
            AppAssociations.CollectionChanged += OnTodoItemsChanged;

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
        public ObservableCollection<AppAssociation> AppAssociations { get; } = [];
        private AppAssociation? _selectedApp = null;
        /// <summary>
        /// 选择的待办项集合所在的程序
        /// </summary>
        public AppAssociation? SelectedApp
        {
            get => _selectedApp;
            set
            {
                _selectedApp = value;
                OnPropertyChanged(nameof(SelectedApp));
            }
        }
        #endregion

        #region 方法

        private static readonly string ConfigPath = "app_associations.json";
        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new() { WriteIndented = true };
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
        private async Task SaveToFileAsync()
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

        public static MainWIndowModel? LoadFromFile()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<MainWIndowModel>(json, CachedJsonSerializerOptions);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载配置失败: {ex.Message}");
            }
            return new();
        }
        #endregion

    }
}
