using Microsoft.EntityFrameworkCore;
using SqliteEFCoreDemo.Data;
using SqliteEFCoreDemo.Entities;
using System.Data.Common;

namespace SqliteEFCoreDemo
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
                await CreateDatabaseAsync();
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
        /// 创建数据库
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    using var context = new AppDbContext(_dbPath);
                    
                    // 检查数据库是否可以连接
                    await context.Database.CanConnectAsync();
                    
                    // 确保数据库创建
                    await context.Database.EnsureCreatedAsync();
                    
                    Console.WriteLine("数据库创建成功");
                    break;
                }
                catch (DbException ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        throw new Exception($"创建数据库失败，已重试{MaxRetryCount}次", ex);
                    }

                    Console.WriteLine($"创建数据库失败，正在重试({retryCount}/{MaxRetryCount})...");
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// 初始化数据库版本信息
        /// </summary>
        private async Task InitializeSchemaVersionAsync()
        {
            using var context = new AppDbContext(_dbPath);
            
            // 创建版本表
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS SchemaVersions (
                    Version TEXT NOT NULL,
                    AppliedOn DATETIME DEFAULT CURRENT_TIMESTAMP
                )");

            // 检查是否已存在版本记录
            var versionExists = await context.Database.ExecuteSqlRawAsync(
                "SELECT COUNT(*) FROM SchemaVersions WHERE Version = {0}", SchemaVersion) > 0;

            if (!versionExists)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO SchemaVersions (Version) VALUES ({0})", SchemaVersion);
            }
        }

        /// <summary>
        /// 初始化基础数据
        /// </summary>
        private async Task InitializeBasicDataAsync()
        {
            try
            {
                using var context = new AppDbContext(_dbPath);
                await using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 检查是否已经有数据
                    if (await context.Users.AnyAsync())
                    {
                        Console.WriteLine("数据库已经初始化过，跳过基础数据初始化");
                        return;
                    }

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

                    await context.Departments.AddRangeAsync(departments);
                    await context.SaveChangesAsync();

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

                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    Console.WriteLine("基础数据初始化成功");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
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
            using var context = new AppDbContext(_dbPath);
            
            // 验证表是否存在
            var tables = new[] { "Users", "Departments", "SchemaVersions" };
            foreach (var table in tables)
            {
                var tableExists = await context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name={0}", table) > 0;
                
                if (!tableExists)
                {
                    throw new Exception($"数据库验证失败：表 {table} 不存在");
                }
            }

            // 验证基础数据
            var userCount = await context.Users.CountAsync();
            var deptCount = await context.Departments.CountAsync();

            if (userCount == 0 || deptCount == 0)
            {
                throw new Exception("数据库验证失败：基础数据不完整");
            }

            Console.WriteLine("数据库验证成功");
        }

        /// <summary>
        /// 重置数据库
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            try
            {
                using var context = new AppDbContext(_dbPath);
                await context.Database.EnsureDeletedAsync();
                Console.WriteLine("数据库删除成功");

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