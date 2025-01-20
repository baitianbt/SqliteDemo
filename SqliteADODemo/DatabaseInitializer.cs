using Microsoft.Data.Sqlite;

using SqliteADODemo.Exceptions;

namespace SqliteADODemo
{
    public class DatabaseInitializer
    {
        private readonly string _dbPath;
        private readonly DatabaseHelper _dbHelper;
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
                throw new DatabaseException("Initialize", "数据库初始化失败", ex);
            }
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        private void CreateDatabaseFile()
        {
            if (!File.Exists(_dbPath))
            {
                File.Create(_dbPath).Dispose();
                Console.WriteLine($"数据库文件 {_dbPath} 创建成功");
            }
        }

        /// <summary>
        /// 创建表结构
        /// </summary>
        private async Task CreateTablesAsync()
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

            await _dbHelper.ExecuteNonQueryAsync(createVersionTableSql);
            await _dbHelper.ExecuteNonQueryAsync(createUserTableSql);
            await _dbHelper.ExecuteNonQueryAsync(createDepartmentTableSql);

            Console.WriteLine("数据库表创建成功");
        }

        /// <summary>
        /// 初始化数据库版本信息
        /// </summary>
        private async Task InitializeSchemaVersionAsync()
        {
            var versionExists = await _dbHelper.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM SchemaVersions WHERE Version = @Version",
                new SqliteParameter("@Version", SchemaVersion));

            if (Convert.ToInt32(versionExists) == 0)
            {
                await _dbHelper.ExecuteNonQueryAsync(
                    "INSERT INTO SchemaVersions (Version, AppliedOn) VALUES (@Version, @AppliedOn)",
                    new SqliteParameter("@Version", SchemaVersion),
                    new SqliteParameter("@AppliedOn", DateTime.UtcNow));
            }
        }

        /// <summary>
        /// 初始化基础数据
        /// </summary>
        private async Task InitializeBasicDataAsync()
        {
            // 检查是否已经有数据
            var userCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM Users"));
            if (Convert.ToInt32(userCount) > 0)
            {
                Console.WriteLine("数据库已经初始化过，跳过基础数据初始化");
                return;
            }

            await _dbHelper.ExecuteInTransactionAsync(async transaction =>
            {
                // 插入部门数据
                var insertDeptSql = @"
                    INSERT INTO Departments (Name, Description, CreateTime) 
                    VALUES (@Name, @Description, @CreateTime)";

                await _dbHelper.ExecuteNonQueryAsync(insertDeptSql,
                    new SqliteParameter("@Name", "技术部"),
                    new SqliteParameter("@Description", "负责技术研发"),
                    new SqliteParameter("@CreateTime", DateTime.UtcNow));

                await _dbHelper.ExecuteNonQueryAsync(insertDeptSql,
                    new SqliteParameter("@Name", "人事部"),
                    new SqliteParameter("@Description", "负责人力资源管理"),
                    new SqliteParameter("@CreateTime", DateTime.UtcNow));

                // 插入用户数据
                var insertUserSql = @"
                    INSERT INTO Users (Name, Age, Email, CreateTime) 
                    VALUES (@Name, @Age, @Email, @CreateTime)";

                await _dbHelper.ExecuteNonQueryAsync(insertUserSql,
                    new SqliteParameter("@Name", "张三"),
                    new SqliteParameter("@Age", 25),
                    new SqliteParameter("@Email", "zhangsan@example.com"),
                    new SqliteParameter("@CreateTime", DateTime.UtcNow));

                await _dbHelper.ExecuteNonQueryAsync(insertUserSql,
                    new SqliteParameter("@Name", "李四"),
                    new SqliteParameter("@Age", 30),
                    new SqliteParameter("@Email", "lisi@example.com"),
                    new SqliteParameter("@CreateTime", DateTime.UtcNow));
            });

            Console.WriteLine("基础数据初始化成功");
        }

        /// <summary>
        /// 验证数据库完整性
        /// </summary>
        private async Task ValidateDatabaseAsync()
        {
            // 验证表是否存在
            var tables = new[] { "Users", "Departments", "SchemaVersions" };
            foreach (var table in tables)
            {
                var tableExists = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = @TableName",
                    new SqliteParameter("@TableName", table))) > 0;

                if (!tableExists)
                {
                    throw new DatabaseException("Validate", $"数据库验证失败：表 {table} 不存在");
                }
            }

            // 验证基础数据
            var userCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync("SELECT COUNT(*) FROM Users"));
            var deptCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync("SELECT COUNT(*) FROM Departments"));

            if (userCount == 0 || deptCount == 0)
            {
                throw new DatabaseException("Validate", "数据库验证失败：基础数据不完整");
            }

            Console.WriteLine("数据库验证成功");
        }

        /// <summary>
        /// 重置数据库
        /// </summary>
        public void ResetDatabase()
        {
            try
            {
                // 删除数据库文件
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                    Console.WriteLine("数据库文件删除成功");
                }

                // 重新初始化数据库
                InitializeAsync().Wait();
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