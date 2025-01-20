using Dapper;
using Microsoft.Data.Sqlite;
using SqliteDapperDemo.Entities;
using System.Data;

namespace SqliteDapperDemo
{
    public class DatabaseInitializer
    {
        private readonly string _dbPath;
        private readonly DatabaseHelper _dbHelper;
        private const int MaxRetryCount = 3;
        private const int RetryDelayMilliseconds = 1000;
        private const string SchemaVersion = "1.0";

        public DatabaseInitializer(string dbPath)
        {
            _dbPath = dbPath;
            _dbHelper = new DatabaseHelper(dbPath);
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                CreateDatabaseFile();
                await CreateTablesAsync();
                await InitializeSchemaVersionAsync();
                await InitializeBasicDataAsync();
                await ValidateDatabaseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        private void CreateDatabaseFile()
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (!File.Exists(_dbPath))
                    {
                        using var connection = new SqliteConnection($"Data Source={_dbPath}");
                        connection.Open();
                        Console.WriteLine($"数据库文件 {_dbPath} 创建成功");
                    }
                    break;
                }
                catch (IOException ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        throw new Exception($"创建数据库文件失败，已重试{MaxRetryCount}次", ex);
                    }

                    Console.WriteLine($"创建数据库文件失败，正在重试({retryCount}/{MaxRetryCount})...");
                    Thread.Sleep(RetryDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// 创建表结构
        /// </summary>
        private async Task CreateTablesAsync()
        {
            try
            {
                // 创建版本表
                var createVersionTableSql = @"
                    CREATE TABLE IF NOT EXISTS SchemaVersions (
                        Version TEXT PRIMARY KEY NOT NULL,
                        AppliedOn DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                // 用户表
                var createUserTableSql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Age INTEGER,
                        Email TEXT,
                        CreateTime DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                // 部门表
                var createDepartmentTableSql = @"
                    CREATE TABLE IF NOT EXISTS Departments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        CreateTime DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                await _dbHelper.ExecuteAsync(createVersionTableSql);
                await _dbHelper.ExecuteAsync(createUserTableSql);
                await _dbHelper.ExecuteAsync(createDepartmentTableSql);

                Console.WriteLine("数据库表创建成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建表时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化数据库版本信息
        /// </summary>
        private async Task InitializeSchemaVersionAsync()
        {
            try
            {
                var versionExists = await _dbHelper.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM SchemaVersions WHERE Version = @Version",
                    new { Version = SchemaVersion });

                if (versionExists == 0)
                {
                    await _dbHelper.ExecuteAsync(
                        "INSERT INTO SchemaVersions (Version, AppliedOn) VALUES (@Version, @AppliedOn)",
                        new { Version = SchemaVersion, AppliedOn = DateTime.UtcNow });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化版本信息失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化基础数据
        /// </summary>
        private async Task InitializeBasicDataAsync()
        {
            try
            {
                // 检查是否已经有数据
                var userCount = await _dbHelper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
                if (userCount > 0)
                {
                    Console.WriteLine("数据库已经初始化过，跳过基础数据初始化");
                    return;
                }

                // 使用事务插入数据
                await _dbHelper.ExecuteInTransactionAsync(async transaction =>
                {
                    // 插入部门数据
                    var departments = new[]
                    {
                        new Department
                        {
                            Name = "技术部",
                            Description = "负责技术研发",
                            CreateTime = DateTime.UtcNow
                        },
                        new Department
                        {
                            Name = "人事部",
                            Description = "负责人力资源管理",
                            CreateTime = DateTime.UtcNow
                        }
                    };

                    var insertDeptSql = @"
                        INSERT INTO Departments (Name, Description, CreateTime) 
                        VALUES (@Name, @Description, @CreateTime)";
                    await transaction.Connection.ExecuteAsync(insertDeptSql, departments, transaction);
                    

                    // 插入用户数据
                    var users = new[]
                    {
                        new User
                        {
                            Name = "张三",
                            Age = 25,
                            Email = "zhangsan@example.com",
                            CreateTime = DateTime.UtcNow
                        },
                        new User
                        {
                            Name = "李四",
                            Age = 30,
                            Email = "lisi@example.com",
                            CreateTime = DateTime.UtcNow
                        }
                    };

                    var insertUserSql = @"
                        INSERT INTO Users (Name, Age, Email, CreateTime) 
                        VALUES (@Name, @Age, @Email, @CreateTime)";
                    await transaction.Connection.ExecuteAsync(insertUserSql, users, transaction);
                });

                Console.WriteLine("基础数据初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化基础数据时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 验证数据库完整性
        /// </summary>
        private async Task ValidateDatabaseAsync()
        {
            try
            {
                // 验证表是否存在
                var tables = new[] { "Users", "Departments", "SchemaVersions" };
                foreach (var table in tables)
                {
                    var tableExists = await _dbHelper.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = @TableName",
                        new { TableName = table }) > 0;

                    if (!tableExists)
                    {
                        throw new Exception($"数据库验证失败：表 {table} 不存在");
                    }
                }

                // 验证基础数据
                var userCount = await _dbHelper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
                var deptCount = await _dbHelper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departments");

                if (userCount == 0 || deptCount == 0)
                {
                    throw new Exception("数据库验证失败：基础数据不完整");
                }

                Console.WriteLine("数据库验证成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库验证失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 重置数据库
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                    Console.WriteLine("数据库文件删除成功");
                }

                await InitializeAsync();
                Console.WriteLine("数据库重置成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置数据库时出错: {ex.Message}");
                throw;
            }
        }
    }
}