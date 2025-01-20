using Microsoft.EntityFrameworkCore;
using SqliteEFCoreDemo.Entities;

namespace SqliteEFCoreDemo.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        public AppDbContext(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 用户表配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CreateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 部门表配置
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CreateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
} 