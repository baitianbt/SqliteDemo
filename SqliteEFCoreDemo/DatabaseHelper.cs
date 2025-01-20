using Microsoft.EntityFrameworkCore;
using SqliteEFCoreDemo.Data;
using System.Linq.Expressions;

namespace SqliteEFCoreDemo
{
    public class DatabaseHelper
    {
        private readonly string _dbPath;

        public DatabaseHelper(string dbPath)
        {
            _dbPath = dbPath;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(_dbPath);
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        public async Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> predicate = null) where T : class
        {
            using var context = CreateContext();
            var query = context.Set<T>().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.ToListAsync();
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            using var context = CreateContext();
            return await context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            using var context = CreateContext();
            var entry = await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            using var context = CreateContext();
            await context.Set<T>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public async Task UpdateAsync<T>(T entity) where T : class
        {
            using var context = CreateContext();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public async Task DeleteAsync<T>(T entity) where T : class
        {
            using var context = CreateContext();
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<AppDbContext, Task> action)
        {
            using var context = CreateContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await action(context);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
} 