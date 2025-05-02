
using System.Windows;
using TodoOverlayApp.Models;
using TodoOverlayApp.Utils;
using TodoOverlayApp.ViewModels;
using TodoOverlayApp.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace TodoOverlayApp
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.MainViewModel;
            Closed += MainWindow_Closed;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //弹出提示是关闭应用还是隐藏到托盘，选择关闭则正常退出，选择隐藏到托盘则隐藏窗口，取消则不关闭窗口
            var result = MessageBox.Show("关闭应用还是最小化到托盘？是直接退出，否最小化到托盘", "确认退出", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                return;
            }
            else if (result == MessageBoxResult.No)
            {
                // 阻止窗口关闭，改为隐藏窗口
                e.Cancel = true;
                this.Hide();

                // 发送通知气泡提示用户
                TrayIconManager.SendMessage("应用已最小化到托盘。");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
        }

        private void TodoInputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // 获取文本内容
                string todoContent = TodoInputTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(todoContent))
                {
                    // 创建新的TodoItemModel
                    var newTodo = new TodoItemModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Content = todoContent,
                        IsCompleted = false
                    };

                    // 将新待办添加到ViewModel的TodoItems集合中
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.Model.TodoItems.Add(newTodo);
                        //调用编辑
                        App.MainViewModel?.EditTodoItemCommand.Execute(newTodo);
                        // 保存到数据库
                        App.TodoItemRepository.AddAsync(newTodo).ConfigureAwait(false);
                        // 清空文本框
                        TodoInputTextBox.Clear();
                    }
                }
            }
        }

    }
}
