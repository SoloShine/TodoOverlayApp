using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly TodoDbContext _dbContext;
        
        public DatabaseInitializer(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeAsync()
        {
            // 确保数据库已创建
            await _dbContext.Database.EnsureCreatedAsync();
            
            // 检查是否需要填充初始数据
            if (!await _dbContext.TodoItems.AnyAsync())
            {
                await SeedTestDataAsync();
            }
        }

        /// <summary>
        /// 重置数据库 - 清除所有数据并重新填充测试数据
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            try
            {
                // 清除所有待办事项数据
                await ClearAllDataAsync();

                // 重新填充测试数据
                await SeedTestDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置数据库时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 清除数据库中的所有数据
        /// </summary>
        private async Task ClearAllDataAsync()
        {
            // 删除所有待办事项
            var allItems = await _dbContext.TodoItems.ToListAsync();
            _dbContext.TodoItems.RemoveRange(allItems);

            // 如果后续添加了其他表，也在这里清除

            // 保存更改
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化测试数据
        /// </summary>
        private async Task SeedTestDataAsync()
        { 
            // 记事本应用
            var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            //获取notepad名称
            var notepadName = Path.GetFileName(notepadPath);
            // 添加待办事项
            // 普通待办事项
            var todoItem1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "普通待办事项示例1"
            };

            var todoItem1_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1_1",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "普通待办事项示例1_1"
            };
            var todoItem1_2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1_2",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "普通待办事项示例1_2"
            };

            var todoItem2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项2",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "普通待办事项示例2"
            };
            
            // 记事本应用待办事项
            var todoItem3 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "记事本待办项1",
                ParentId = string.Empty,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "记事本应用待办事项示例1"
            };

            var todoItem3_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "记事本待办项1",
                ParentId = todoItem3.Id,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "记事本应用待办事项示例1"
            };
            
            _dbContext.TodoItems.AddRange(todoItem1, todoItem1_1, todoItem1_2, todoItem2, todoItem3, todoItem3_1);
            
            // 保存所有更改
            await _dbContext.SaveChangesAsync();
        }
    }
}
