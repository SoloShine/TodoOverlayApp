using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace TodoOverlayApp.Views
{
    public partial class SelectRunningAppWindow : Window
    {
        public Process? SelectedProcess { get; private set; }
        private List<Process> allProcesses = new();
        private List<Process> filteredProcesses = new();

        public SelectRunningAppWindow()
        {
            InitializeComponent();

            // 加载所有运行中的进程
            allProcesses = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ToList();
            filteredProcesses = new List<Process>(allProcesses);

            // 初始化列表
            UpdateListBox();

            // 事件绑定
            SearchBox.GotFocus += (s, e) =>
            {
                if (SearchBox.Text == "搜索进程...")
                    SearchBox.Text = "";
            };

            SearchBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    SearchBox.Text = "搜索进程...";
            };

            SearchBox.TextChanged += (s, e) => UpdateListBox();
            ProcessListBox.MouseDoubleClick += (s, e) => SelectProcess();
            SelectButton.Click += (s, e) => SelectProcess();
            CancelButton.Click += (s, e) => Close();
        }

        private void UpdateListBox()
        {
            var searchText = SearchBox.Text == "搜索进程..." ? "" : SearchBox.Text.ToLower();
            ProcessListBox.Items.Clear();

            // 更新过滤后的进程列表
            filteredProcesses = allProcesses
                .Where(p =>
                    p.ProcessName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    p.MainWindowTitle.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            foreach (var process in filteredProcesses)
            {
                ProcessListBox.Items.Add($"{process.ProcessName} - {process.MainWindowTitle}");
            }
        }

        private void SelectProcess()
        {
            if (ProcessListBox.SelectedIndex != -1)
            {
                SelectedProcess = filteredProcesses[ProcessListBox.SelectedIndex];
                DialogResult = true;
                Close();
            }
        }
    }
}
