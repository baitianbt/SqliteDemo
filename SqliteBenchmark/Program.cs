using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using SqliteBenchmark.Benchmarks;

namespace SqliteBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            BenchmarkRunner.Run<DatabaseBenchmark>(config);
        }
    }
}