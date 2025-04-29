using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TodoOverlayApp.Utils;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : Application
{
    private static MainWindowViewModel? _mainViewModel;
    /// <summary>
    /// 全局单例 ViewModel
    /// </summary>
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

        // 应用保存的主题设置
        _mainViewModel?.Model.ApplyThemeSettings();

        // 初始化托盘图标
        TrayIconManager.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 清理托盘图标
        TrayIconManager.Cleanup();

        base.OnExit(e);
    }
}

