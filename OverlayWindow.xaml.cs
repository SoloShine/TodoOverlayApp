using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;

namespace TodoOverlayApp
{
    public partial class OverlayWindow : Window
    {
        public ObservableCollection<TodoItem> TodoItems { get; set; }

        public OverlayWindow(ObservableCollection<TodoItem> todoItems)
        {
            InitializeComponent();
            TodoItems = todoItems;
            this.DataContext = this;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            // 标记所有已完成项
            foreach(var item in TodoItems.Where(i => i.IsCompleted))
            {
                // 可以在这里添加完成后的处理逻辑
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // 编辑待办项的逻辑
            MessageBox.Show("编辑功能待实现");
        }
    }
}