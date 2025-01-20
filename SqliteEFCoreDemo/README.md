# SQLite with Entity Framework Core 示例项目

这是一个使用Entity Framework Core操作SQLite数据库的.NET Core示例项目。

## 项目特点

- 🚀 使用EF Core ORM框架
- 📦 支持Code First开发模式
- 💾 支持LINQ查询
- 🔄 支持事务和批量操作
- 📊 完整的实体关系映射

## 快速开始

### 1. 安装必要的包

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
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

// 添加数据
var user = new User { Name = "张三", Age = 25, Email = "zhangsan@example.com" };
await dbHelper.AddAsync(user);
```

## EF Core的主要特点

1. 完整的ORM功能
2. 强大的LINQ支持
3. 数据迁移支持
4. 关系映射
5. 变更跟踪

## 主要功能

### 1. 实体配置

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired();
        entity.Property(e => e.CreateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
    });
}
```

### 2. LINQ查询

```csharp
// 基本查询
var users = await context.Users
    .Where(u => u.Age > 20)
    .OrderBy(u => u.Name)
    .ToListAsync();

// 包含导航属性
var usersWithDepts = await context.Users
    .Include(u => u.Department)
    .ToListAsync();
```

### 3. 事务处理

```csharp
await dbHelper.ExecuteInTransactionAsync(async context =>
{
    var user = new User { Name = "张三", Age = 25 };
    await context.Users.AddAsync(user);

    var dept = await context.Departments.FindAsync(1);
    dept.Name = "新技术部";
    context.Departments.Update(dept);

    await context.SaveChangesAsync();
});
```

## 项目结构

- `Data/`: 数据访问层
  - `AppDbContext.cs`: EF Core数据库上下文
- `Entities/`: 实体类
  - `User.cs`: 用户实体
  - `Department.cs`: 部门实体
- `DatabaseHelper.cs`: 数据库访问帮助类
- `DatabaseInitializer.cs`: 数据库初始化
- `Program.cs`: 示例程序

## 最佳实践

1. 数据库上下文
   - 正确配置连接字符串
   - 合理设置实体关系
   - 使用适当的生命周期

2. 查询优化
   - 使用异步方法
   - 避免N+1查询问题
   - 合理使用Include

3. 性能考虑
   - 批量操作使用AddRange
   - 合理使用事务
   - 注意变更跟踪的影响

## 注意事项

1. DbContext生命周期管理
2. 正确处理并发情况
3. 合理使用延迟加载
4. 注意查询性能

## 常见问题

1. **Q: 如何处理并发更新？**
   ```csharp
   modelBuilder.Entity<User>()
       .Property(u => u.Version)
       .IsRowVersion();
   ```

2. **Q: 如何使用原生SQL？**
   ```csharp
   var users = await context.Users
       .FromSqlRaw("SELECT * FROM Users WHERE Age > {0}", age)
       .ToListAsync();
   ```

3. **Q: 如何实现软删除？**
   ```csharp
   modelBuilder.Entity<User>()
       .HasQueryFilter(u => u.DeleteTime == null);
   ```

## 扩展功能

1. 数据迁移
2. 全局查询筛选器
3. 值转换器
4. 并发处理

## 许可证

MIT License 