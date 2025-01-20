using Microsoft.Data.Sqlite;
using SqliteADODemo.Exceptions;
using System.Data;
using System.Threading.Tasks;

namespace SqliteADODemo
{
    public class DatabaseHelper : IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection _connection;
        private bool _disposed;

        public DatabaseHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        /// <summary>
        /// 执行非查询SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询，返回第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public object ExecuteScalar(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteScalar();
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果DataTable</returns>
        public DataTable ExecuteQuery(string sql, params SqliteParameter[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            using var reader = command.ExecuteReader();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>
        /// 执行查询，返回SqliteDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>SqliteDataReader对象</returns>
        public SqliteDataReader ExecuteReader(string sql, params SqliteParameter[] parameters)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = new SqliteCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private SqliteCommand CreateCommand(SqliteConnection connection, string sql, SqliteParameter[] parameters)
        {
            var command = new SqliteCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command;
        }

        /// <summary>
        /// 异步执行非查询SQL语句
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                using var command = CreateCommand(connection, sql, parameters);
                return await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("ExecuteNonQuery", $"执行SQL失败: {sql}", ex);
            }
        }

        /// <summary>
        /// 异步执行查询，返回第一行第一列
        /// </summary>
        public async Task<object> ExecuteScalarAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                using var command = CreateCommand(connection, sql, parameters);
                return await command.ExecuteScalarAsync();
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("ExecuteScalar", $"执行SQL失败: {sql}", ex);
            }
        }

        /// <summary>
        /// 异步执行查询，返回DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                using var command = CreateCommand(connection, sql, parameters);
                var dataTable = new DataTable();
                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
                return dataTable;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("ExecuteQuery", $"执行查询失败: {sql}", ex);
            }
        }

        /// <summary>
        /// 在事务中执行多个SQL命令
        /// </summary>
        public void ExecuteInTransaction(Action<SqliteTransaction> action)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                action(transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 异步在事务中执行多个SQL命令
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<SqliteTransaction, Task> action)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                await action(transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Transaction", "事务执行失败", ex);
            }
        }

        /// <summary>
        /// 批量执行SQL命令
        /// </summary>
        public void ExecuteBatch(IEnumerable<(string sql, SqliteParameter[] parameters)> commands)
        {
            ExecuteInTransaction(transaction =>
            {
                using var command = new SqliteCommand();
                command.Connection = transaction.Connection;
                command.Transaction = transaction;

                foreach (var (sql, parameters) in commands)
                {
                    command.CommandText = sql;
                    command.Parameters.Clear();
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            });
        }

        /// <summary>
        /// 异步批量执行SQL命令
        /// </summary>
        public async Task ExecuteBatchAsync(IEnumerable<(string sql, SqliteParameter[] parameters)> commands)
        {
            try
            {
                await ExecuteInTransactionAsync(async transaction =>
                {
                    using var command = new SqliteCommand();
                    command.Connection = transaction.Connection;
                    command.Transaction = transaction;

                    foreach (var (sql, parameters) in commands)
                    {
                        command.CommandText = sql;
                        command.Parameters.Clear();
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        await command.ExecuteNonQueryAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new DatabaseException("BatchExecution", "批量执行失败", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DatabaseHelper()
        {
            Dispose(false);
        }
    }
} 