using System.Windows;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Views
{
    public partial class EditTodoItemWindow : HandyControl.Controls.Window
    {
        public TodoItem Todo { get; private set; }

        public EditTodoItemWindow(TodoItem todo)
        {
            InitializeComponent();
            Todo = new TodoItem
            {
                Id = todo.Id,
                Content = todo.Content,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                ParentId = todo.ParentId,
                IsExpanded = todo.IsExpanded,
                SubItems = todo.SubItems
            };

            // 初始化控件值
            ContentTextBox.Text = Todo.Content;
            DescriptionTextBox.Text = Todo.Description;

            // 事件绑定
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(ContentTextBox.Text))
            {
                MessageBox.Show("内容不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 保存数据
            Todo.Content = ContentTextBox.Text;
            Todo.Description = DescriptionTextBox.Text;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
