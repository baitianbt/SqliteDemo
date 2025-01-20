using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SqliteADODemo;
using SqliteDapperDemo;
using SqliteSugarDemo;
using SqliteEFCoreDemo;
using System.Data;
using Microsoft.Data.Sqlite;

namespace SqliteBenchmark.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DatabaseBenchmark
    {
        private string _dbPath;
        private SqliteADODemo.DatabaseHelper _adoHelper;
        private SqliteDapperDemo.DatabaseHelper _dapperHelper;
        private SqliteSugarDemo.DatabaseHelper _sugarHelper;
        private SqliteEFCoreDemo.DatabaseHelper _efHelper;

        [GlobalSetup]
        public async Task Setup()
        {
            _dbPath = "benchmark.db";
            
            // 初始化数据库
            var initializer = new SqliteADODemo.DatabaseInitializer(_dbPath);

            await initializer.InitializeAsync();

            // 初始化各个Helper
            _adoHelper = new SqliteADODemo.DatabaseHelper(_dbPath);
            _dapperHelper = new SqliteDapperDemo.DatabaseHelper(_dbPath);
            _sugarHelper = new SqliteSugarDemo.DatabaseHelper(_dbPath);
            _efHelper = new SqliteEFCoreDemo.DatabaseHelper(_dbPath);
        }

        [Benchmark(Description = "ADO.NET - 单行查询")]
        public async Task AdoSingleQuery()
        {
            var result = await _adoHelper.ExecuteScalarAsync(
                "SELECT Name FROM Users WHERE Id = @Id",
                new SqliteParameter("@Id", 1)
            );
        }

        [Benchmark(Description = "Dapper - 单行查询")]
        public async Task DapperSingleQuery()
        {
            var result = await _dapperHelper.QueryFirstOrDefaultAsync<string>(
                "SELECT Name FROM Users WHERE Id = @Id",
                new { Id = 1 }
            );
        }

        [Benchmark(Description = "SqlSugar - 单行查询")]
        public async Task SugarSingleQuery()
        {
            var result = await _sugarHelper.Query<SqliteSugarDemo.Entities.User>()
                .Where(u => u.Id == 1)
                .Select(u => u.Name)
                .FirstAsync();
        }

        [Benchmark(Description = "EF Core - 单行查询")]
        public async Task EFCoreSingleQuery()
        {
            var result = await _efHelper.GetFirstOrDefaultAsync<SqliteEFCoreDemo.Entities.User>(
                u => u.Id == 1
            );
        }

        [Benchmark(Description = "ADO.NET - 批量插入")]
        public async Task AdoBatchInsert()
        {
            var users = GenerateTestUsers(100);
            await _adoHelper.ExecuteBatchAsync(users.Select(u => (
                "INSERT INTO Users (Name, Age, Email) VALUES (@Name, @Age, @Email)",
                new SqliteParameter[] {
                    new SqliteParameter("@Name", u.Name),
                    new SqliteParameter("@Age", u.Age),
                    new SqliteParameter("@Email", u.Email)
                }
            )));
        }

        [Benchmark(Description = "Dapper - 批量插入")]
        public async Task DapperBatchInsert()
        {
            var users = GenerateTestUsers(100);
            await _dapperHelper.ExecuteAsync(
                "INSERT INTO Users (Name, Age, Email) VALUES (@Name, @Age, @Email)",
                users
            );
        }

        [Benchmark(Description = "SqlSugar - 批量插入")]
        public async Task SugarBatchInsert()
        {
            var users = GenerateTestUsers(100)
                .Select(u => new SqliteSugarDemo.Entities.User
                {
                    Name = u.Name,
                    Age = u.Age,
                    Email = u.Email
                });
            await _sugarHelper.InsertRangeAsync(users.ToList());
        }

        [Benchmark(Description = "EF Core - 批量插入")]
        public async Task EFCoreBatchInsert()
        {
            var users = GenerateTestUsers(100)
                .Select(u => new SqliteEFCoreDemo.Entities.User
                {
                    Name = u.Name,
                    Age = u.Age,
                    Email = u.Email
                });
            await _efHelper.AddRangeAsync(users.ToList());
        }

        private IEnumerable<(string Name, int Age, string Email)> GenerateTestUsers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return (
                    $"User{i}",
                    20 + (i % 30),
                    $"user{i}@example.com"
                );
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
    }
} 