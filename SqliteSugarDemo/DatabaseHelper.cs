using SqliteSugarDemo.Entities;
using SqlSugar;
using System.Linq.Expressions;

namespace SqliteSugarDemo
{
    public class DatabaseHelper
    {
        public readonly SqlSugarScope _db;

        public DatabaseHelper(string dbPath)
        {
            _db = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"Data Source={dbPath}",
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// 获取查询对象
        /// </summary>
        public ISugarQueryable<T> Query<T>() where T : class, new()
        {
            return _db.Queryable<T>();
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        public async Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> expression = null) where T : class, new()
        {
            var query = _db.Queryable<T>();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return await query.ToListAsync();
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        public async Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return await _db.Queryable<T>().FirstAsync(expression);
        }

        /// <summary>
        /// 插入实体
        /// </summary>
        public async Task<bool> InsertAsync<T>(T entity) where T : class, new()
        {
            return await _db.Insertable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        public async Task<bool> InsertRangeAsync<T>(List<T> entities) where T : class, new()
        {
            return await _db.Insertable(entities).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public async Task<bool> UpdateAsync<T>(T entity) where T : class, new()
        {
            return await _db.Updateable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public async Task<bool> DeleteAsync<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return await _db.Deleteable<T>().Where(expression).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        public async Task<bool> ExecuteTransactionAsync(Func<Task> action)
        {
            try
            {
                await _db.Ado.BeginTranAsync();
                await action();
                await _db.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await _db.Ado.RollbackTranAsync();
                throw;
            }
        }

        /// <summary>
        /// 创建数据库表
        /// </summary>
        public void CreateTables()
        {
            _db.CodeFirst.SetStringDefaultLength(200)
                .InitTables(typeof(User), typeof(Department));
        }
    }
} 