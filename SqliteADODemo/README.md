# SQLite ADO.NET 示例项目

这是一个使用ADO.NET原生方式访问SQLite数据库的示例项目。

## 项目特点

- 🚀 使用原生ADO.NET
- 📦 完全控制SQL和参数
- 💾 支持同步和异步操作
- 🔄 支持事务和批量操作
- 📊 完整的异常处理

## 项目结构

```
SqliteADODemo/
├── Program.cs              # 程序入口
├── DatabaseHelper.cs       # 数据库访问帮助类
├── DatabaseInitializer.cs  # 数据库初始化
├── Exceptions/            
│   └── DatabaseException.cs # 自定义异常类
└── Entities/              
    ├── User.cs            # 用户实体
    └── Department.cs      # 部门实体
```

## 主要功能

1. 数据库操作
   - 执行非查询SQL (ExecuteNonQuery)
   - 执行标量查询 (ExecuteScalar)
   - 执行数据查询 (ExecuteQuery)
   - 事务支持
   - 批量操作

2. 异常处理
   - 自定义异常类
   - 详细的错误信息
   - 异常追踪

3. 资源管理
   - 正确的连接管理
   - IDisposable实现
   - 异步资源释放

## 使用示例

```csharp
// 初始化数据库
var dbPath = "test.db";
var dbInitializer = new DatabaseInitializer(dbPath);
await dbInitializer.InitializeAsync();

// 基本查询
using var dbHelper = new DatabaseHelper(dbPath);
var result = await dbHelper.ExecuteQueryAsync(
    "SELECT * FROM Users WHERE Age > @Age",
    new SqliteParameter("@Age", 20)
);

// 事务操作
await dbHelper.ExecuteInTransactionAsync(async transaction => {
    await dbHelper.ExecuteNonQueryAsync(
        "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
        new SqliteParameter("@Name", "张三"),
        new SqliteParameter("@Age", 25)
    );
});
```

## 最佳实践

1. 连接管理
   - 使用using语句
   - 及时释放连接
   - 避免连接泄露

2. 参数化查询
   - 始终使用参数
   - 防止SQL注入
   - 提高性能

3. 异常处理
   - 捕获具体异常
   - 提供详细信息
   - 正确的异常传播

4. 性能优化
   - 批量操作使用事务
   - 合理使用连接池
   - 避免过度查询

## 注意事项

1. 连接字符串管理
2. 参数化查询
3. 事务范围控制
4. 资源释放
5. 异常处理

## 许可证

MIT License 