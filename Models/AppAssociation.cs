
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

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

        /// <summary>
        /// 选择一个应用，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="app"></param>
        public static void SelectApp(AppAssociation app)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                //从完整路径获取文件名
                var fileName = Path.GetFileName(openFileDialog.FileName);

                app.AppName = fileName;
                app.AppPath = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 选择当前运行软件，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="app"></param>
        public static void SelectRunningApp(AppAssociation app)
        {
            var selectWindow = new Views.SelectRunningAppWindow();
            if (selectWindow.ShowDialog() == true && selectWindow.SelectedProcess != null)
            {
                try
                {
                    app.AppPath = selectWindow.SelectedProcess.MainModule?.FileName;
                    app.AppName = selectWindow.SelectedProcess.MainModule?.ModuleName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法获取进程路径: {ex.Message}");
                }
            }
        }


        public AppAssociation()
        {
            
        }
    }
}
