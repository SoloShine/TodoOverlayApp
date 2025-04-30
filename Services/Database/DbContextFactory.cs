using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// DbContext ���������ڴ������ݿ�������ʵ��
    /// </summary>
    public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
    {
        private readonly string _connectionString;

        /// <summary>
        /// ���ʱ�������캯�������� EF Core ���ߣ���Ǩ�ƣ�
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
        /// �������ַ����Ĺ��캯������������ʱ����������
        /// </summary>
        public TodoDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// �������ݿ�������
        /// </summary>
        public TodoDbContext CreateDbContext(string[] args = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
            optionsBuilder.UseSqlite(_connectionString);
            return new TodoDbContext(optionsBuilder.Options);
        }
    }
}
