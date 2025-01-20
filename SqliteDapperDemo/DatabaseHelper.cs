using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace SqliteDapperDemo
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        private SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// 执行SQL返回实体列表
        /// </summary>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<T>(sql, param);
        }

        /// <summary>
        /// 执行SQL返回单个实体
        /// </summary>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        /// <summary>
        /// 执行SQL返回受影响的行数
        /// </summary>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync(sql, param);
        }

        /// <summary>
        /// 执行SQL返回单个值
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null)
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<T>(sql, param);
        }

        /// <summary>
        /// 在事务中执行多个操作
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                await action(transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        public async Task BulkInsertAsync<T>(string tableName, IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            var connection = transaction?.Connection ?? GetConnection();
            var props = typeof(T).GetProperties();
            var columns = string.Join(", ", props.Select(p => p.Name));
            var parameters = string.Join(", ", props.Select(p => "@" + p.Name));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            if (transaction == null)
            {
                await connection.ExecuteAsync(sql, entities);
            }
            else
            {
                await connection.ExecuteAsync(sql, entities, transaction);
            }
        }
    }
} 