using Microsoft.Win32;
using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

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
            GreadtedAt = item.GreadtedAt;
            UpdatedAt = item.UpdatedAt;
            CompletedAt = item.CompletedAt;
            StartTime = item.StartTime;
            ReminderTime = item.ReminderTime;
            EndTime = item.EndTime;
            //todo更新任务
        }
        private ObservableCollection<TodoItemModel> subItems = [];
        /// <summary>
        /// 子待办项集合
        /// </summary>
        public ObservableCollection<TodoItemModel> SubItems
        {
            get => subItems;
            set
            {
                subItems = value;
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
                    item.Name = selectWindow.SelectedProcess.MainModule?.ModuleName ?? selectWindow.SelectedProcess.ProcessName;
                    item.Content = item.Name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法获取进程路径: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 查询节点并更新其属性
        /// </summary>
        /// <param name="items"></param>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        public static TodoItemModel? SearchChangeNode(ObservableCollection<TodoItemModel> items, TodoItemModel todoItem)
        {
            if (string.IsNullOrEmpty(todoItem.Id)) return null;
            foreach (var item in items)
            {
                if (item.Id == todoItem.Id)
                {
                    item.IsCompleted = todoItem.IsCompleted;
                    item.UpdatedAt = DateTime.Now;
                    item.Description = todoItem.Description;
                    item.Name = todoItem.Name;
                    item.AppPath = todoItem.AppPath;
                    item.Content = todoItem.Content;
                    //item.IsInjected = todoItem.IsInjected;
                    item.TodoItemType = todoItem.TodoItemType;

                    return item;
                }
                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    var result = SearchChangeNode(item.SubItems, todoItem);
                    if (result != null) return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据AppPath重组TodoItemModel集合，保持树的结构
        /// </summary>
        /// <param name="items">原始TodoItemModel集合</param>
        /// <param name="appPath">要筛选的应用路径</param>
        /// <returns>重组后的TodoItemModel集合</returns>
        public static ObservableCollection<TodoItemModel> RecTodoItems(ObservableCollection<TodoItemModel> items, string appPath)
        {
            // 创建结果集合
            var result = new ObservableCollection<TodoItemModel>();

            // 创建一个ID到节点的映射，方便快速查找
            Dictionary<string, TodoItemModel> idToModelMap = new Dictionary<string, TodoItemModel>();

            // 首先将所有节点(包括子节点)放入字典中，便于后续查找
            PopulateModelMap(items, idToModelMap);

            // 获取所有满足AppPath条件的项
            var matchingItems = new List<TodoItemModel>();
            FindItemsByAppPath(items, appPath, matchingItems);

            // 将每个匹配的项及其父项链添加到结果中
            foreach (var item in matchingItems)
            {
                // 如果项目已经在结果中，则跳过
                if (ContainsItem(result, item.Id))
                    continue;

                // 检查父节点
                if (!string.IsNullOrEmpty(item.ParentId) && idToModelMap.TryGetValue(item.ParentId, out var parent))
                {
                    // 如果父节点的AppPath为空，则将此节点视为根节点
                    if (string.IsNullOrEmpty(parent.AppPath))
                    {
                        if (!ContainsItem(result, item.Id))
                            result.Add(item);
                    }
                    // 如果父节点的AppPath与指定的不同，则将此节点视为根节点
                    else if (parent.AppPath != appPath)
                    {
                        if (!ContainsItem(result, item.Id))
                            result.Add(item);
                    }
                    // 否则，处理整个父节点链
                    else
                    {
                        AddItemWithParentChain(item, idToModelMap, result, appPath);
                    }
                }
                else
                {
                    // 没有父节点的情况，直接添加到结果中
                    if (!ContainsItem(result, item.Id))
                        result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 将指定项及其所有父项链添加到结果集合中
        /// </summary>
        public static void AddItemWithParentChain(TodoItemModel item, Dictionary<string, TodoItemModel> idToModelMap, ObservableCollection<TodoItemModel> result, string appPath)
        {
            var currentItem = item;
            var itemsToAdd = new List<TodoItemModel>();

            // 向上遍历父项链
            while (currentItem != null)
            {
                itemsToAdd.Insert(0, currentItem);

                if (string.IsNullOrEmpty(currentItem.ParentId) ||
                    !idToModelMap.TryGetValue(currentItem.ParentId, out var parent) ||
                    string.IsNullOrEmpty(parent.AppPath) ||
                    parent.AppPath != appPath)
                {
                    break; // 遇到根节点或AppPath不匹配的父节点时停止
                }

                currentItem = parent;
            }

            // 将顶层节点添加到结果中
            if (itemsToAdd.Count > 0 && !ContainsItem(result, itemsToAdd[0].Id))
            {
                result.Add(itemsToAdd[0]);
            }
        }

        /// <summary>
        /// 判断集合中是否已包含指定ID的项
        /// </summary>
        public static bool ContainsItem(ObservableCollection<TodoItemModel> items, string id)
        {
            foreach (var item in items)
            {
                if (item.Id == id)
                    return true;

                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    if (ContainsItem(item.SubItems, id))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 递归构建ID到TodoItemModel的映射
        /// </summary>
        public static void PopulateModelMap(ObservableCollection<TodoItemModel> items, Dictionary<string, TodoItemModel> idToModelMap)
        {
            foreach (var item in items)
            {
                idToModelMap[item.Id] = item;

                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    PopulateModelMap(item.SubItems, idToModelMap);
                }
            }
        }

        /// <summary>
        /// 查找集合中AppPath匹配的所有项
        /// </summary>
        public static void FindItemsByAppPath(ObservableCollection<TodoItemModel> items, string appPath, List<TodoItemModel> result)
        {
            foreach (var item in items)
            {
                if (item.AppPath == appPath)
                {
                    result.Add(item);
                }

                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    FindItemsByAppPath(item.SubItems, appPath, result);
                }
            }
        }
    }
}
