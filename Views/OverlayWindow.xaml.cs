using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Views
{
    public partial class OverlayWindow : Window
    {
        public ObservableCollection<TodoItem> TodoItems { get; set; }

        public OverlayWindow(ObservableCollection<TodoItem> todoItems)
        {
            InitializeComponent();
            TodoItems = todoItems;
            DataContext = this;

            // Ӧ������������
            ApplyOverlaySettings();
        }

        /// <summary>
        /// Ӧ�ô�ģ���ж�ȡ������������
        /// </summary>
        public void ApplyOverlaySettings()
        {
            try
            {
                var model = App.MainViewModel?.Model;
                if (model != null)
                {
                    // ���ñ�����ɫ
                    Color backgroundColor = (Color)ColorConverter.ConvertFromString(model.OverlayBackground);
                    MainBorder.Background = new SolidColorBrush(backgroundColor);

                    // ���ò�͸����
                    MainBorder.Opacity = model.OverlayOpacity;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ӧ������������ʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ȡ���ھ���ĸ�������
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
