using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// DbContext 工厂，用于创建数据库上下文实例
    /// </summary>
    public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
    {
        private readonly string _connectionString;

        /// <summary>
        /// 设计时工厂构造函数，用于 EF Core 工具（如迁移）
        /// </summary>
        public TodoDbContextFactory()
        {
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TodoOverlayApp");
                
            var dbPath = Path.Combine(dataDir, "todo.db");
            _connectionString = $"Data Source={dbPath}";
        }

        /// <summary>
        /// 带连接字符串的构造函数，用于运行时创建上下文
        /// </summary>
        public TodoDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 创建数据库上下文
        /// </summary>
        public TodoDbContext CreateDbContext(string[] args = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
            optionsBuilder.UseSqlite(_connectionString);
            return new TodoDbContext(optionsBuilder.Options);
        }
    }
}
