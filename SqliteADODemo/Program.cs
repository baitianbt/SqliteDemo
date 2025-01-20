using Microsoft.Data.Sqlite;
using SqliteADODemo.Entities;

namespace SqliteADODemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string dbPath = "test.db";
            var dbInitializer = new DatabaseInitializer(dbPath);
            var dbHelper = new DatabaseHelper(dbPath);

            try
            {
                // 初始化数据库
                await dbInitializer.InitializeAsync();

                // 查询用户数据
                Console.WriteLine("\n查询所有用户：");
                var userDataTable = await dbHelper.ExecuteQueryAsync("SELECT * FROM Users");
                foreach (System.Data.DataRow row in userDataTable.Rows)
                {
                    Console.WriteLine($"ID: {row["Id"]}, 姓名: {row["Name"]}, 年龄: {row["Age"]}, 邮箱: {row["Email"]}");
                }

                // 查询部门数据
                Console.WriteLine("\n查询所有部门：");
                var deptDataTable = await dbHelper.ExecuteQueryAsync("SELECT * FROM Departments");
                foreach (System.Data.DataRow row in deptDataTable.Rows)
                {
                    Console.WriteLine($"ID: {row["Id"]}, 名称: {row["Name"]}, 描述: {row["Description"]}");
                }

                // 演示事务操作
                await dbHelper.ExecuteInTransactionAsync(async transaction =>
                {
                    // 插入新用户
                    var insertSql = @"
                        INSERT INTO Users (Name, Age, Email, CreateTime) 
                        VALUES (@Name, @Age, @Email, @CreateTime)";
                    
                    await dbHelper.ExecuteNonQueryAsync(insertSql,
                        new SqliteParameter("@Name", "王五"),
                        new SqliteParameter("@Age", 28),
                        new SqliteParameter("@Email", "wangwu@example.com"),
                        new SqliteParameter("@CreateTime", DateTime.UtcNow)
                    );

                    // 更新其他用户年龄
                    var updateSql = "UPDATE Users SET Age = Age + 1 WHERE Name = @Name";
                    await dbHelper.ExecuteNonQueryAsync(updateSql,
                        new SqliteParameter("@Name", "张三")
                    );
                });

                // 查询更新后的数据
                Console.WriteLine("\n更新后的用户数据：");
                userDataTable = await dbHelper.ExecuteQueryAsync("SELECT * FROM Users");
                foreach (System.Data.DataRow row in userDataTable.Rows)
                {
                    Console.WriteLine($"ID: {row["Id"]}, 姓名: {row["Name"]}, 年龄: {row["Age"]}, 邮箱: {row["Email"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序执行出错: {ex.Message}");
            }
        }
    }
} 