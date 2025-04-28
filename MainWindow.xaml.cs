
using System.Windows;
using TodoOverlayApp.ViewModels;
using TodoOverlayApp.Views;

namespace TodoOverlayApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.MainViewModel;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
        }

        private void ThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var themeWindow = new ThemeSettingsWindow
            {
                Owner = this
            };
            themeWindow.ShowDialog();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
