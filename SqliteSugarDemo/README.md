# SQLite with SqlSugar 示例项目

这是一个使用SqlSugar操作SQLite数据库的.NET Core示例项目。SqlSugar是一个轻量级的ORM框架，支持多种数据库。

## 项目特点

- 🚀 使用SqlSugar ORM框架
- 📦 支持CodeFirst开发模式
- 💾 支持Lambda表达式查询
- 🔄 支持事务和批量操作
- 📊 强类型实体映射

## 快速开始

### 1. 安装必要的包

```bash
dotnet add package SqlSugarCore
dotnet add package Microsoft.Data.Sqlite
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
var users = await dbHelper.GetListAsync<User>(u => u.Age > 20);

// 插入数据
var user = new User { Name = "张三", Age = 25, Email = "zhangsan@example.com" };
await dbHelper.InsertAsync(user);
```

## SqlSugar vs 其他ORM的优势

1. 更简单的API设计
2. 支持代码优先和数据库优先
3. 更好的性能
4. 支持多种数据库
5. 丰富的CRUD操作支持

## 主要功能

### 1. 实体映射

```csharp
[SugarTable("Users")]
public class User
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    
    [SugarColumn(ColumnName = "Name", IsNullable = false)]
    public string Name { get; set; }
}
```

### 2. 查询操作

```csharp
// Lambda表达式查询
var users = await dbHelper.GetListAsync<User>(u => u.Age > 20);

// 获取单个实体
var user = await dbHelper.GetFirstAsync<User>(u => u.Name == "张三");

// 复杂查询
var query = dbHelper.Query<User>()
    .Where(u => u.Age > 20)
    .OrderBy(u => u.CreateTime)
    .Select(u => new { u.Name, u.Age });
```

### 3. 事务处理

```csharp
await dbHelper.ExecuteTransactionAsync(async () =>
{
    // 插入用户
    var user = new User { Name = "张三", Age = 25 };
    await dbHelper.InsertAsync(user);

    // 更新部门
    var dept = await dbHelper.GetFirstAsync<Department>(d => d.Name == "技术部");
    dept.Description = "更新后的描述";
    await dbHelper.UpdateAsync(dept);
});
```

### 4. 批量操作

```csharp
var users = new List<User>
{
    new User { Name = "张三", Age = 25 },
    new User { Name = "李四", Age = 30 }
};

await dbHelper.InsertRangeAsync(users);
```

## 项目结构

- `DatabaseHelper.cs`: SqlSugar数据库访问封装
- `DatabaseInitializer.cs`: 数据库初始化
- `Entities/`: 实体类
  - `User.cs`: 用户实体
  - `Department.cs`: 部门实体
- `Program.cs`: 示例程序

## 最佳实践

1. 实体定义
   - 使用特性标注表名和列名
   - 明确指定主键和自增列
   - 设置合适的可空性

2. 查询优化
   - 使用表达式目录树构建查询
   - 只查询需要的列
   - 合理使用索引

3. 性能优化
   - 批量操作使用事务
   - 合理使用延迟加载
   - 避免N+1查询问题

## 注意事项

1. 实体映射配置
2. 事务范围控制
3. 连接管理
4. 异常处理

## 常见问题

1. **Q: 如何处理复杂查询？**
   ```csharp
   var query = dbHelper.Query<User>()
       .LeftJoin<Department>((u, d) => u.DepartmentId == d.Id)
       .Where(u => u.Age > 20)
       .Select((u, d) => new { u.Name, d.DepartmentName });
   ```

2. **Q: 如何使用原生SQL？**
   ```csharp
   var result = await _db.Ado.SqlQueryAsync<User>("SELECT * FROM Users WHERE Age > @Age", 
       new { Age = 20 });
   ```

3. **Q: 如何处理软删除？**
   ```csharp
   [SugarColumn(IsNullable = true)]
   public DateTime? DeleteTime { get; set; }
   
   // 查询未删除的数据
   var users = await dbHelper.GetListAsync<User>(u => u.DeleteTime == null);
   ```

## 扩展功能

1. 分页查询
2. 软删除支持
3. 审计日志
4. 缓存支持

## 许可证

MIT License 