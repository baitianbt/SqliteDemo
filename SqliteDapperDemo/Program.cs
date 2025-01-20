using SqliteDapperDemo.Entities;

namespace SqliteDapperDemo
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
                var users = await dbHelper.QueryAsync<User>("SELECT * FROM Users");
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, 姓名: {user.Name}, 年龄: {user.Age}, 邮箱: {user.Email}");
                }

                // 查询部门数据
                Console.WriteLine("\n查询所有部门：");
                var departments = await dbHelper.QueryAsync<Department>("SELECT * FROM Departments");
                foreach (var dept in departments)
                {
                    Console.WriteLine($"ID: {dept.Id}, 名称: {dept.Name}, 描述: {dept.Description}");
                }

                // 演示事务操作
                await dbHelper.ExecuteInTransactionAsync(async transaction =>
                {
                    // 插入新用户
                    var user = new User
                    {
                        Name = "王五",
                        Age = 28,
                        Email = "wangwu@example.com",
                        CreateTime = DateTime.UtcNow
                    };

                    await dbHelper.ExecuteAsync(
                        "INSERT INTO Users (Name, Age, Email, CreateTime) VALUES (@Name, @Age, @Email, @CreateTime)",
                        user
                        
                    );

                    // 更新其他用户年龄
                    await dbHelper.ExecuteAsync(
                        "UPDATE Users SET Age = Age + 1 WHERE Name = @Name",
                        new { Name = "张三" }
                        
                    );
                });

                // 查询更新后的数据
                Console.WriteLine("\n更新后的用户数据：");
                users = await dbHelper.QueryAsync<User>("SELECT * FROM Users");
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, 姓名: {user.Name}, 年龄: {user.Age}, 邮箱: {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序执行出错: {ex.Message}");
            }
        }
    }
}
