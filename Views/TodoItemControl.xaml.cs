using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TodoOverlayApp.Models;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp.Views
{
    /// <summary>
    /// TodoItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class TodoItemControl : UserControl
    {
        // 跟踪点击次数和上次点击时间
        private int clickCount = 0;
        private DateTime lastClickTime = DateTime.MinValue;
        private const double DoubleClickTimeThreshold = 300; // 毫秒

        public TodoItemControl()
        {
            InitializeComponent();
        }


        public bool IsOverlay
        {
            get { return (bool)GetValue(IsOverlayProperty); }
            set { SetValue(IsOverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOverlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOverlayProperty =
            DependencyProperty.Register("IsOverlay", typeof(bool), typeof(TodoItemControl), new PropertyMetadata(false));



        /// <summary>
        /// 模拟双击事件的处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;

            // 检查是否是双击（时间间隔在阈值内的两次点击）
            if ((now - lastClickTime).TotalMilliseconds <= DoubleClickTimeThreshold)
            {
                if (DataContext is TodoItemModel todoItem)
                {
                    //修正展开情况
                    todoItem.IsExpanded = !todoItem.IsExpanded;
                    App.MainViewModel?.EditTodoItemCommand.Execute(todoItem);
                }

                // 重置点击计数
                clickCount = 0;
            }
            else
            {
                // 第一次点击
                clickCount = 1;
            }

            // 更新最后点击时间
            lastClickTime = now;
        }
    }
}
