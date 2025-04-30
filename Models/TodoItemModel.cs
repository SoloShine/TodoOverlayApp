using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TodoOverlayApp.Models
{
    public class TodoItemModel : TodoItem
    {
        public TodoItemModel() { }
        public TodoItemModel(TodoItem item)
        {
            Id = item.Id;
            ParentId = item.ParentId;
            Content = item.Content;
            IsCompleted = item.IsCompleted;
            IsExpanded = item.IsExpanded;
            Description = item.Description;
            Name = item.Name;
            AppPath = item.AppPath;
            IsInjected = item.IsInjected;
            TodoItemType = item.TodoItemType;
        }
        private ObservableCollection<TodoItemModel> _subItems = [];
        /// <summary>
        /// 子待办项集合
        /// </summary>
        public ObservableCollection<TodoItemModel> SubItems
        {
            get => _subItems;
            set
            {
                _subItems = value;
                OnPropertyChanged(nameof(SubItems));
            }
        }

        /// <summary>
        /// 选择一个应用，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="item"></param>
        public static void SelectApp(TodoItemModel item)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                //从完整路径获取文件名
                var fileName = Path.GetFileName(openFileDialog.FileName);

                item.Name = fileName;
                item.AppPath = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 选择当前运行软件，并将其路径设置为选中应用的AppPath。
        /// </summary>
        /// <param name="item"></param>
        public static void SelectRunningApp(TodoItemModel item)
        {
            var selectWindow = new Views.SelectRunningAppWindow();
            if (selectWindow.ShowDialog() == true && selectWindow.SelectedProcess != null)
            {
                try
                {
                    item.AppPath = selectWindow.SelectedProcess.MainModule?.FileName;
                    item.Name = selectWindow.SelectedProcess.MainModule?.ModuleName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法获取进程路径: {ex.Message}");
                }
            }
        }
    }
}
