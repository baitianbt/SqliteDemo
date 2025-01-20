using Microsoft.Data.Sqlite;
using SqliteADODemo;
using System.Text;

namespace SqliteADODemo
{
    public class DatabaseInitializer
    {
        private readonly string _dbPath;
        private readonly DatabaseHelper _dbHelper;

        public DatabaseInitializer(string dbPath)
        {
            _dbPath = dbPath;
            _dbHelper = new DatabaseHelper(dbPath);
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void Initialize()
        {
            CreateDatabaseFile();
            CreateTables();
            InitializeBasicData();
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        private void CreateDatabaseFile()
        {
           if (!File.Exists(_dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                Console.WriteLine($"数据库文件 {_dbPath} 创建成功");
            }
        }

        /// <summary>
        /// 创建表结构
        /// </summary>
        private void CreateTables()
        {
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

            // 用户部门关系表
            var createUserDeptTableSql = @"
                CREATE TABLE IF NOT EXISTS UserDepartments (
                    UserId INTEGER,
                    DepartmentId INTEGER,
                    JoinTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (UserId, DepartmentId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id),
                    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
                );";

            try
            {
                _dbHelper.ExecuteNonQuery(createUserTableSql);
                _dbHelper.ExecuteNonQuery(createDepartmentTableSql);
                _dbHelper.ExecuteNonQuery(createUserDeptTableSql);
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
        private void InitializeBasicData()
        {
            try
            {
                // 检查是否已经有数据
                var count = (long)_dbHelper.ExecuteScalar("SELECT COUNT(*) FROM Users");
                if (count > 0)
                {
                    Console.WriteLine("数据库已经初始化过，跳过基础数据初始化");
                    return;
                }

                // 插入部门数据
                var insertDeptSql = "INSERT INTO Departments (Name, Description) VALUES (@Name, @Description)";
                _dbHelper.ExecuteNonQuery(insertDeptSql, new[]
                {
                    new SqliteParameter("@Name", "技术部"),
                    new SqliteParameter("@Description", "负责技术研发")
                });

                _dbHelper.ExecuteNonQuery(insertDeptSql, new[]
                {
                    new SqliteParameter("@Name", "人事部"),
                    new SqliteParameter("@Description", "负责人力资源管理")
                });

                // 插入用户数据
                var insertUserSql = "INSERT INTO Users (Name, Age, Email) VALUES (@Name, @Age, @Email)";
                _dbHelper.ExecuteNonQuery(insertUserSql, new[]
                {
                    new SqliteParameter("@Name", "张三"),
                    new SqliteParameter("@Age", 25),
                    new SqliteParameter("@Email", "zhangsan@example.com")
                });

                _dbHelper.ExecuteNonQuery(insertUserSql, new[]
                {
                    new SqliteParameter("@Name", "李四"),
                    new SqliteParameter("@Age", 30),
                    new SqliteParameter("@Email", "lisi@example.com")
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
                Initialize();
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