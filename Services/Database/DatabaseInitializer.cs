using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// ���ݿ��ʼ����
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly TodoDbContext dbContext;
        
        public DatabaseInitializer(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        /// <summary>
        /// ��ʼ�����ݿ�
        /// </summary>
        public async Task InitializeAsync()
        {
            // ȷ�����ݿ��Ѵ���
            await dbContext.Database.EnsureCreatedAsync();
            
            // ����Ƿ���Ҫ����ʼ����
            if (!await dbContext.TodoItems.AnyAsync())
            {
                await SeedTestDataAsync();
            }
        }

        /// <summary>
        /// �������ݿ� - ����������ݲ���������������
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            try
            {
                // ������д�����������
                await ClearAllDataAsync();

                // ��������������
                await SeedTestDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�������ݿ�ʱ����: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ������ݿ��е���������
        /// </summary>
        private async Task ClearAllDataAsync()
        {
            // ɾ�����д�������
            var allItems = await dbContext.TodoItems.ToListAsync();
            dbContext.TodoItems.RemoveRange(allItems);

            // ������������������Ҳ���������

            // �������
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        private async Task SeedTestDataAsync()
        { 
            // ���±�Ӧ��
            var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            //��ȡnotepad����
            var notepadName = Path.GetFileName(notepadPath);
            // ��Ӵ�������
            // ��ͨ��������
            var todoItem1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "��ͨ������1",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "��ͨ��������ʾ��1"
            };

            var todoItem1_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "��ͨ������1_1",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "��ͨ��������ʾ��1_1"
            };
            var todoItem1_2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "��ͨ������1_2",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "��ͨ��������ʾ��1_2"
            };

            var todoItem2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "��ͨ������2",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "��ͨ��������ʾ��2"
            };
            
            // ���±�Ӧ�ô�������
            var todoItem3 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "���±�������1",
                ParentId = string.Empty,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "���±�Ӧ�ô�������ʾ��1"
            };

            var todoItem3_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "���±�������1",
                ParentId = todoItem3.Id,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "���±�Ӧ�ô�������ʾ��1"
            };
            
            dbContext.TodoItems.AddRange(todoItem1, todoItem1_1, todoItem1_2, todoItem2, todoItem3, todoItem3_1);
            
            // �������и���
            await dbContext.SaveChangesAsync();
        }
    }
}
