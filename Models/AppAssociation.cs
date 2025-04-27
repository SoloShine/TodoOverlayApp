
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoOverlayApp.Models
{
    public class AppAssociation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string? appPath = string.Empty;
        /// <summary>
        /// 应用路径
        /// </summary>
        public string? AppPath
        {
            get => appPath;
            set
            {
                appPath = value;
                OnPropertyChanged(nameof(AppPath));
            }
        }

        private string? appName = string.Empty;
        /// <summary>
        /// 应用名称
        /// </summary>
        public string? AppName
        {
            get => appName;
            set
            {
                appName = value;
                OnPropertyChanged(nameof(AppName));
            }
        }


        private ObservableCollection<TodoItem> todoItems = [];
        /// <summary>
        /// 待办事项列表
        /// </summary>
        public ObservableCollection<TodoItem> TodoItems
        {
            get => todoItems;
            set
            {
                todoItems = value;
                OnPropertyChanged(nameof(TodoItems));
            }
        }

        private bool isExpanded;
        /// <summary>
        /// 是否展开节点
        /// </summary>
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public AppAssociation()
        {
            
        }
    }
}
