using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Services.Database;
using TodoOverlayApp.Services.Database.Repositories;
using TodoOverlayApp.Utils;
using TodoOverlayApp.ViewModels;

namespace TodoOverlayApp;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : Application
{
    private static MainWindowViewModel? mainViewModel;
    /// <summary>
    /// 全局单例 ViewModel
    /// </summary>
    public static MainWindowViewModel? MainViewModel
    {
        get
        {
            mainViewModel ??= Current.Resources["MainViewModel"] as MainWindowViewModel;
            return mainViewModel;
        }
    }

    // EF Core 上下文
    public static TodoDbContext DbContext { get; private set; }
    
    // 仓储实例
    //public static GroupRepository GroupRepository { get; private set; }
    public static TodoItemRepository TodoItemRepository { get; private set; }
    public static AutoTaskRepository AutoTaskRepository { get; private set; }
    public static DatabaseInitializer DatabaseInitializer { get; private set; }


    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化数据库服务
        await InitializeDatabaseAsync();

        // 确保资源中的ViewModel是唯一的  
        mainViewModel = Current.Resources["MainViewModel"] as MainWindowViewModel;

        // 应用保存的主题设置
        mainViewModel?.Model.ApplyThemeSettings();

        // 初始化托盘图标
        TrayIconManager.Initialize();
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private static async Task InitializeDatabaseAsync()
    {
        // 创建数据目录
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TodoOverlayApp");
            
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        
        // 数据库路径
        var dbPath = Path.Combine(dataDir, "todo.db");
        
        // 初始化 DbContext
        var connectionString = $"Data Source={dbPath}";
        var factory = new TodoDbContextFactory(connectionString);
        DbContext = factory.CreateDbContext();
        
        // 初始化仓储
        //GroupRepository = new GroupRepository(DbContext);
        TodoItemRepository = new TodoItemRepository(DbContext);
        AutoTaskRepository = new AutoTaskRepository(DbContext);
        //AppAssociationRepository = new AppAssociationRepository(DbContext);

        // 初始化数据库
        DatabaseInitializer = new DatabaseInitializer(DbContext);
        await DatabaseInitializer.InitializeAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 释放 DbContext
        DbContext?.Dispose();
        
        // 清理托盘图标
        TrayIconManager.Cleanup();

        base.OnExit(e);
    }
}

