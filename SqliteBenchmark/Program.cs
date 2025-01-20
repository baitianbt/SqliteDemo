using BenchmarkDotNet.Running;
using SqliteBenchmark.Benchmarks;

namespace SqliteBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<DatabaseBenchmark>();
        }
    }
} 