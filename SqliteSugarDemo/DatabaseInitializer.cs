using SqlSugar;
using SqliteSugarDemo.Entities;

namespace SqliteSugarDemo
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
                await InitializeSchemaVersionAsync();
                CreateTables();
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
                        File.Create(_dbPath).Close();
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
        /// 初始化数据库版本信息
        /// </summary>
        private async Task InitializeSchemaVersionAsync()
        {
            try
            {
                // 创建版本表
                _dbHelper._db.CodeFirst.SetStringDefaultLength(200)
                    .InitTables(typeof(SchemaVersion));

                // 检查是否已存在版本记录
                var versionExists = await _dbHelper._db.Queryable<SchemaVersion>()
                    .AnyAsync(v => v.Version == SchemaVersion);

                if (!versionExists)
                {
                    await _dbHelper._db.Insertable(new SchemaVersion
                    {
                        Version = SchemaVersion,
                        AppliedOn = DateTime.UtcNow
                    }).ExecuteCommandAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化版本信息失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建表结构
        /// </summary>
        private void CreateTables()
        {
            try
            {
                _dbHelper.CreateTables();
                Console.WriteLine("数据库表创建成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建表时出错: {ex.Message}");
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
                // 开启事务
                await _dbHelper.ExecuteTransactionAsync(async () =>
                {
                    // 检查是否已经有数据
                    var users = await _dbHelper.GetListAsync<User>();
                    if (users.Any())
                    {
                        Console.WriteLine("数据库已经初始化过，跳过基础数据初始化");
                        return;
                    }

                    // 插入部门数据
                    var departments = new List<Department>
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

                    await _dbHelper.InsertRangeAsync(departments);

                    // 插入用户数据
                    var newUsers = new List<User>
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

                    await _dbHelper.InsertRangeAsync(newUsers);
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
                    var tableExists = _dbHelper._db.DbMaintenance.IsAnyTable(table);

                    if (!tableExists)
                    {
                        throw new Exception($"数据库验证失败：表 {table} 不存在");
                    }
                }

                // 验证基础数据
                var userCount = await _dbHelper._db.Queryable<User>().CountAsync();
                var deptCount = await _dbHelper._db.Queryable<Department>().CountAsync();

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
                // 删除数据库文件
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                    Console.WriteLine("数据库文件删除成功");
                }

                // 重新初始化数据库
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

    /// <summary>
    /// 数据库版本信息实体
    /// </summary>
    [SugarTable("SchemaVersions")]
    public class SchemaVersion
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Version { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime AppliedOn { get; set; }
    }
} 