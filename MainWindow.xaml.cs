
using System.Windows;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
        }
    }
}
