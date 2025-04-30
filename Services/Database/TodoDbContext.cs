using Microsoft.EntityFrameworkCore;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Database
{
    /// <summary>
    /// EF Core ���ݿ�������
    /// </summary>
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// ���������
        /// </summary>
        public DbSet<TodoItem> TodoItems { get; set; }

        /// <summary>
        /// ����ʵ���ϵ��Լ��
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ���� TodoItem ʵ��
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
