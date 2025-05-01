using System.Windows;
using System.Windows.Controls;
using TodoOverlayApp.Models;
using TodoOverlayApp.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;

namespace TodoOverlayApp.Views
{
    public partial class EditTodoItemWindow : HandyControl.Controls.Window
    {
        public TodoItemModel Todo { get; private set; }

        public EditTodoItemWindow(TodoItemModel todo)
        {
            InitializeComponent();
            Todo = new TodoItemModel
            {
                Id = todo.Id,
                Content = todo.Content,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                ParentId = todo.ParentId,
                IsExpanded = todo.IsExpanded,
                SubItems = todo.SubItems,
                TodoItemType = todo.TodoItemType
            };

            // 初始化控件值
            ContentTextBox.Text = Todo.Content;
            DescriptionTextBox.Text = Todo.Description;
            AppPathTextBox.Text = todo.AppPath;
            AppNameTextBox.Text = todo.Name;
            AppTypeComboBox.SelectedItem = todo.TodoItemType;

            // 初始化枚举ComboBox
            var converter = new Converters.EnumToComboBoxConverter();
            AppTypeComboBox.ItemsSource = converter.Convert(typeof(TodoItemType), null, null, null) as List<KeyValuePair<Enum, string>>;

            // 设置当前选中项
            foreach (var item in AppTypeComboBox.Items)
            {
                var pair = (KeyValuePair<Enum, string>)item;
                if ((TodoItemType)pair.Key == Todo.TodoItemType)
                {
                    AppTypeComboBox.SelectedItem = item;
                    break;
                }
            }
            TypeChange();

            // 事件绑定
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            AppTypeComboBox.SelectionChanged += AppTypeComboBox_SelectionChanged;
            SelectAppButton.Click += SelectAppButton_Click;
        }

        private void AppTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TypeChange();
        }

        private void TypeChange()
        {
            if (AppTypeComboBox.SelectedItem is KeyValuePair<Enum, string> selectedPair)
            {
                Todo.TodoItemType = (TodoItemType)selectedPair.Key;
                // 根据类型显示或隐藏控件
                if (Todo.TodoItemType == TodoItemType.App)
                {
                    AppPathTextBox.Visibility = Visibility.Visible;
                    AppNameTextBox.Visibility = Visibility.Visible;
                    AppNameTextBlock.Visibility = Visibility.Visible;
                    AppPathTextBlock.Visibility = Visibility.Visible;
                    SelectAppButton.Visibility = Visibility.Visible;
                }
                else
                {
                    AppPathTextBox.Visibility = Visibility.Collapsed;
                    AppNameTextBox.Visibility = Visibility.Collapsed;
                    AppNameTextBlock.Visibility = Visibility.Collapsed;
                    AppPathTextBlock.Visibility = Visibility.Collapsed;
                    SelectAppButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "是否为当前运行软件添加待办事项？选\"是\"将检测正在运行的软件，选\"否\"将打开文件选择器。",
                "选择操作",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // 调用选择运行中软件的方法
                var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
                TodoItemModel.SelectRunningApp(Todo);
                AppPathTextBox.Text = Todo.AppPath;
                AppNameTextBox.Text = Todo.Name;
            }
            else if (result == MessageBoxResult.No)
            {
                // 调用选择文件的方法
                var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
                TodoItemModel.SelectApp(Todo);
                AppPathTextBox.Text = Todo.AppPath;
                AppNameTextBox.Text = Todo.Name;
            }
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
            Todo.AppPath = AppPathTextBox.Text;
            Todo.Name = AppNameTextBox.Text;
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
