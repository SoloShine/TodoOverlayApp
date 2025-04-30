using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database.Repositories
{
    /// <summary>
    /// 待办事项仓储实现，使用 EF Core
    /// </summary>
    public class TodoItemRepository
    {
        private readonly TodoDbContext _dbContext;
        
        public TodoItemRepository(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        /// <summary>
        /// 获取所有待办事项
        /// </summary>
        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await _dbContext.TodoItems.ToListAsync();
        }
        
        /// <summary>
        /// 根据ID获取待办事项
        /// </summary>
        public async Task<TodoItem?> GetByIdAsync(string id)
        {
            return await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// 根据应用路径获取待办事项
        /// </summary>
        /// <param name="appPath"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TodoItem>> GetByAppPathAsync(string appPath)
        {
            return await _dbContext.TodoItems
                .Where(t => t.AppPath == appPath)
                .ToListAsync();
        }
        
        /// <summary>
        /// 根据父ID获取子待办事项
        /// </summary>
        public async Task<IEnumerable<TodoItem>> GetByParentIdAsync(string parentId)
        {
            return await _dbContext.TodoItems
                .Where(t => t.ParentId == parentId)
                .ToListAsync();
        }
        
        /// <summary>
        /// 添加待办事项
        /// </summary>
        public async Task<int> AddAsync(TodoItem todoItem)
        {
            _dbContext.TodoItems.Add(todoItem);
            return await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 更新待办事项
        /// </summary>
        public async Task<int> UpdateAsync(TodoItem todoItem)
        {
            _dbContext.Entry(todoItem).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 删除待办事项
        /// </summary>
        public async Task<int> DeleteAsync(string id)
        {
            var todoItem = await _dbContext.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                _dbContext.TodoItems.Remove(todoItem);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// 更新待办事项的完成状态
        /// </summary>
        public async Task<int> UpdateCompletionStatusAsync(string id, bool isCompleted)
        {
            var todoItem = await _dbContext.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                todoItem.IsCompleted = isCompleted;
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
