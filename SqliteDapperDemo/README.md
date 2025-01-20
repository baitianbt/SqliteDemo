# SQLite with Dapper 示例项目

这是一个使用Dapper操作SQLite数据库的.NET Core示例项目。

## 项目特点

- 🚀 使用Dapper微型ORM
- 📦 支持异步操作
- 💾 支持事务处理
- 🔄 支持批量操作
- 📊 强类型查询结果

## 快速开始

### 1. 安装必要的包

```bash
dotnet add package Microsoft.Data.Sqlite
dotnet add package Dapper
```

### 2. 初始化数据库

```csharp
var dbPath = "mydb.db";
var dbInitializer = new DatabaseInitializer(dbPath);
await dbInitializer.InitializeAsync();
```

### 3. 基本使用示例

```csharp
var dbHelper = new DatabaseHelper("mydb.db");

// 查询数据
var users = await dbHelper.QueryAsync<User>(
    "SELECT * FROM Users WHERE Age > @Age",
    new { Age = 20 }
);

// 插入数据
await dbHelper.ExecuteAsync(
    "INSERT INTO Users (Name, Age, Email) VALUES (@Name, @Age, @Email)",
    new { Name = "张三", Age = 25, Email = "zhangsan@example.com" }
);
```

## Dapper vs ADO.NET的优势

1. 更简洁的代码
2. 自动对象映射
3. 更好的性能
4. 参数化查询更简单
5. 支持强类型结果

## 主要功能

### 1. 查询操作

```csharp
// 查询列表
var users = await dbHelper.QueryAsync<User>("SELECT * FROM Users");

// 查询单个对象
var user = await dbHelper.QueryFirstOrDefaultAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = 1 }
);
```

### 2. 事务处理

```csharp
await dbHelper.ExecuteInTransactionAsync(async transaction =>
{
    await dbHelper.ExecuteAsync(
        "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
        new { Name = "张三", Age = 25 },
        transaction
    );

    await dbHelper.ExecuteAsync(
        "UPDATE Users SET Age = Age + 1 WHERE Name = @Name",
        new { Name = "李四" },
        transaction
    );
});
```

### 3. 批量操作

```csharp
var users = new[]
{
    new User { Name = "张三", Age = 25 },
    new User { Name = "李四", Age = 30 }
};

await dbHelper.BulkInsertAsync("Users", users);
```

## 项目结构

- `DatabaseHelper.cs`: Dapper数据库访问封装
- `DatabaseInitializer.cs`: 数据库初始化
- `Entities/`: 实体类
  - `User.cs`: 用户实体
  - `Department.cs`: 部门实体
- `Program.cs`: 示例程序

## 最佳实践

1. 实体映射
   - 属性名称要与数据库字段名匹配
   - 使用适当的数据类型

2. 性能优化
   - 使用参数化查询
   - 适当使用事务
   - 批量操作时使用BulkInsert

3. 异常处理
   - 正确处理数据库异常
   - 使用try-catch保护事务

## 注意事项

1. 连接字符串管理
2. 正确释放资源
3. 参数化查询防注入
4. 事务范围控制

## 许可证

MIT License 