using System.Configuration;
using System.Data;
using System.Windows;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : Application
{
    // 全局单例 ViewModel  
    private static MainWindowViewModel? _mainViewModel;

    public static MainWindowViewModel? MainViewModel
    {
        get
        {
            _mainViewModel ??= Current.Resources["MainViewModel"] as MainWindowViewModel;
            return _mainViewModel;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 确保资源中的ViewModel是唯一的  
        _mainViewModel = Current.Resources["MainViewModel"] as MainWindowViewModel;
    }
}

