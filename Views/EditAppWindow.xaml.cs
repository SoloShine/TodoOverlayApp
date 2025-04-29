using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TodoOverlayApp.Models;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp.Views
{
    public partial class EditAppWindow : HandyControl.Controls.Window
    {
        public AppAssociation App { get; private set; }

        public EditAppWindow(AppAssociation app)
        {
            InitializeComponent();
            App = new AppAssociation
            {
                Id = app.Id,
                AppName = app.AppName,
                AppPath = app.AppPath,
                IsNonApp = app.IsNonApp,
                IsInjected = app.IsInjected,
                Description = app.Description
            };

            // 初始化控件值
            IsNonAppCheckBox.IsChecked = App.IsNonApp;
            AppNameTextBox.Text = App.AppName;
            AppPathTextBox.Text = App.AppPath;
            DescriptionTextBox.Text = App.Description;

            // 根据IsNonApp状态更新控件状态
            UpdateControls();

            // 事件绑定
            IsNonAppCheckBox.Checked += (s, e) => UpdateControls();
            IsNonAppCheckBox.Unchecked += (s, e) => UpdateControls();
            SelectAppButton.Click += SelectAppButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void UpdateControls()
        {
            bool isNonApp = IsNonAppCheckBox.IsChecked == true;
            AppPathTextBox.IsEnabled = !isNonApp;
            SelectAppButton.IsEnabled = !isNonApp;

            if (isNonApp)
            {
                AppPathTextBox.Text = string.Empty;
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
                AppAssociation.SelectRunningApp(App);
                AppPathTextBox.Text = App.AppPath;
                AppNameTextBox.Text = App.AppName;
            }
            else if (result == MessageBoxResult.No)
            {
                // 调用选择文件的方法
                var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
                AppAssociation.SelectApp(App);
                AppPathTextBox.Text = App.AppPath;
                AppNameTextBox.Text = App.AppName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(AppNameTextBox.Text))
            {
                MessageBox.Show("名称不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsNonAppCheckBox.IsChecked == true && string.IsNullOrWhiteSpace(AppPathTextBox.Text))
            {
                MessageBox.Show("应用路径不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 保存数据
            App.AppName = AppNameTextBox.Text;
            App.AppPath = AppPathTextBox.Text;
            App.IsNonApp = IsNonAppCheckBox.IsChecked == true;
            App.Description = DescriptionTextBox.Text;

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
