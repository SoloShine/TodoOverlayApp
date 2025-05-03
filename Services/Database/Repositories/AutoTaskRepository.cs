using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database.Repositories
{
    public class AutoTaskRepository
    {
        private readonly TodoDbContext dbContext;

        public AutoTaskRepository(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// 获取所有自动任务
        /// </summary>
        public async Task<IEnumerable<AutoTask>> GetAllAsync()
        {
            return await dbContext.AutoTasks.ToListAsync();
        }

        /// <summary>
        /// 根据ID获取自动任务
        /// </summary>
        public async Task<AutoTask?> GetByIdAsync(string id)
        {
            return await dbContext.AutoTasks
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// 添加自动任务
        /// </summary>
        public async Task<int> AddAsync(AutoTask task)
        {
            dbContext.AutoTasks.Add(task);
            return await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 更新自动任务
        /// </summary>
        public async Task<int> UpdateAsync(AutoTask task)
        {
            var existingEntity = await dbContext.AutoTasks.FindAsync(task.Id);
            if (existingEntity == null) return 0;

            // 更新所有属性
            dbContext.Entry(existingEntity).CurrentValues.SetValues(task);

            return await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 删除自动任务
        /// </summary>
        public async Task<int> DeleteAsync(string id)
        {
            var task = await dbContext.AutoTasks.FindAsync(id);
            if (task != null)
            {
                dbContext.AutoTasks.Remove(task);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
