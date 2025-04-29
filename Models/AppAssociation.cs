
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

        public string Id { get; set; } = Guid.NewGuid().ToString();

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

        private bool isInjected = false;
        /// <summary>
        /// 是否注入软件
        /// </summary>
        public bool IsInjected
        {
            get => isInjected;
            set
            {
                isInjected = value;
                OnPropertyChanged(nameof(IsInjected));
            }
        }

        private bool isNonApp = false;
        /// <summary>
        /// 是否非软件待办，为true时，当作普通待办的集合特别处理，不需要注入也不需要其他处理
        /// </summary>
        public bool IsNonApp
        {
            get => isNonApp;
            set
            {
                isNonApp = value;
                OnPropertyChanged(nameof(IsNonApp));
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

        private string? description = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public AppAssociation()
        {
            
        }
    }
}
