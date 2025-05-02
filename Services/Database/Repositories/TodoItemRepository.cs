using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database.Repositories
{
    /// <summary>
    /// ��������ִ�ʵ�֣�ʹ�� EF Core
    /// </summary>
    public class TodoItemRepository
    {
        private readonly TodoDbContext dbContext;
        
        public TodoItemRepository(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        /// <summary>
        /// ��ȡ���д�������
        /// </summary>
        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await dbContext.TodoItems.ToListAsync();
        }
        
        /// <summary>
        /// ����ID��ȡ��������
        /// </summary>
        public async Task<TodoItem?> GetByIdAsync(string id)
        {
            return await dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// ����Ӧ��·����ȡ��������
        /// </summary>
        /// <param name="appPath"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TodoItem>> GetByAppPathAsync(string appPath)
        {
            return await dbContext.TodoItems
                .Where(t => t.AppPath == appPath)
                .ToListAsync();
        }
        
        /// <summary>
        /// ���ݸ�ID��ȡ�Ӵ�������
        /// </summary>
        public async Task<IEnumerable<TodoItem>> GetByParentIdAsync(string parentId)
        {
            return await dbContext.TodoItems
                .Where(t => t.ParentId == parentId)
                .ToListAsync();
        }
        
        /// <summary>
        /// ��Ӵ�������
        /// </summary>
        public async Task<int> AddAsync(TodoItem todoItem)
        {
            dbContext.TodoItems.Add(todoItem);
            return await dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// ���´�������
        /// </summary>
        public async Task<int> UpdateAsync(TodoItem todoItem)
        {
            var existingEntity = await dbContext.TodoItems.FindAsync(todoItem.Id);
            if (existingEntity == null) return 0;

            // ������������
            dbContext.Entry(existingEntity).CurrentValues.SetValues(todoItem);

            return await dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// ɾ����������
        /// </summary>
        public async Task<int> DeleteAsync(string id)
        {
            var todoItem = await dbContext.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                dbContext.TodoItems.Remove(todoItem);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// ���´�����������״̬
        /// </summary>
        public async Task<int> UpdateCompletionStatusAsync(string id, bool isCompleted)
        {
            var todoItem = await dbContext.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                todoItem.IsCompleted = isCompleted;
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
