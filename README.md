# SQLite数据访问示例项目

这个解决方案包含了使用不同技术访问SQLite数据库的示例项目。

## 项目结构

- **SqliteADODemo**: 使用ADO.NET原生方式访问SQLite
- **SqliteDapperDemo**: 使用Dapper微型ORM访问SQLite
- **SqliteSugarDemo**: 使用SqlSugar ORM访问SQLite
- **SqliteEFCoreDemo**: 使用Entity Framework Core访问SQLite

## 技术特点对比

### 1. ADO.NET
- 最基础的数据访问方式
- 完全控制SQL和参数
- 性能最好
- 代码量最多

### 2. Dapper
- 轻量级ORM
- 性能接近ADO.NET
- 支持对象映射
- SQL语句完全控制

### 3. SqlSugar
- 国产ORM框架
- 支持多种数据库
- API简单易用
- 性能优秀

### 4. Entity Framework Core
- 微软官方ORM
- 功能最完整
- 支持Code First
- LINQ查询支持

## 快速开始

1. 克隆仓库
```bash
git clone <repository-url>
cd SqliteDemo
```

2. 还原包
```bash
dotnet restore
```

3. 运行项目
```bash
# 运行ADO.NET示例
dotnet run --project SqliteADODemo

# 运行Dapper示例
dotnet run --project SqliteDapperDemo

# 运行SqlSugar示例
dotnet run --project SqliteSugarDemo

# 运行EF Core示例
dotnet run --project SqliteEFCoreDemo
```

## 性能对比

| 操作类型 | ADO.NET | Dapper | SqlSugar | EF Core |
|---------|---------|---------|-----------|---------|
| 单行查询 | 最快 | 接近原生 | 较快 | 较慢 |
| 批量插入 | 最快 | 较快 | 较快 | 较慢 |
| 复杂查询 | 较复杂 | 简单 | 简单 | 最简单 |
| 开发效率 | 最低 | 较高 | 高 | 最高 |

## 最佳实践

1. 简单项目，追求性能
   - 选择ADO.NET或Dapper

2. 中型项目，平衡性能和开发效率
   - 选择Dapper或SqlSugar

3. 大型项目，需要完整ORM支持
   - 选择Entity Framework Core

4. 需要跨数据库支持
   - 选择SqlSugar或Entity Framework Core

## 注意事项

1. 连接字符串管理
2. 事务处理
3. 异常处理
4. 资源释放
5. 性能优化

## 许可证

MIT License