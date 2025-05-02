using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Views
{
    public partial class OverlayWindow : Window
    {
        public ObservableCollection<TodoItemModel> TodoItems { get; set; }

        public OverlayWindow(ObservableCollection<TodoItemModel> todoItems)
        {
            InitializeComponent();
            TodoItems = todoItems;
            DataContext = this;

            // 应用悬浮窗设置
            ApplyOverlaySettings();

            TodoItems.CollectionChanged += (sender, e) =>
            {

            };
        }

        /// <summary>
        /// 应用从模型中读取的悬浮窗设置
        /// </summary>
        public void ApplyOverlaySettings()
        {
            try
            {
                var model = App.MainViewModel?.Model;
                if (model != null)
                {
                    // 设置背景颜色
                    Color backgroundColor = (Color)ColorConverter.ConvertFromString(model.OverlayBackground);
                    MainBorder.Background = new SolidColorBrush(backgroundColor);

                    // 设置不透明度
                    MainBorder.Opacity = model.OverlayOpacity;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用悬浮窗设置时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取窗口句柄的辅助方法
        /// </summary>
        public IntPtr GetHandle()
        {
            return new System.Windows.Interop.WindowInteropHelper(this).Handle;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
