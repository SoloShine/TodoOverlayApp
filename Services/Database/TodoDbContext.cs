using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// EF Core 数据库上下文
    /// </summary>
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 待办事项表
        /// </summary>
        public DbSet<TodoItem> TodoItems { get; set; }

        /// <summary>
        /// 配置实体关系和约束
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置 TodoItem 实体
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
