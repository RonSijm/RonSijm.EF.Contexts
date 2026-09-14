// Licensed under the MIT license.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using RonSijm.EF.Markdown.Benchmarks.Benchmarks;

Console.WriteLine("EF Core Provider Benchmarks");
Console.WriteLine("===========================");
Console.WriteLine();
Console.WriteLine("Comparing: Markdown vs InMemory vs SQLite");
Console.WriteLine();

// Check if running in debug mode
#if DEBUG
Console.WriteLine("WARNING: Running in DEBUG mode. For accurate benchmarks, run in RELEASE mode.");
Console.WriteLine("Use: dotnet run -c Release");
Console.WriteLine();
Console.WriteLine("Running quick test instead of full benchmarks...");
Console.WriteLine();

// Quick test in debug mode
RunQuickTest();
#else
// Run full benchmarks in release mode
var config = DefaultConfig.Instance;

// Check for command-line arguments
if (args.Length > 0)
{
    // Pass through to BenchmarkDotNet's command-line parser
    BenchmarkSwitcher.FromAssembly(typeof(InsertBenchmarks).Assembly).Run(args, config);
}
else
{
    Console.WriteLine("Select benchmark to run:");
    Console.WriteLine("1. Insert Benchmarks");
    Console.WriteLine("2. Read Benchmarks");
    Console.WriteLine("3. Update Benchmarks");
    Console.WriteLine("4. Mixed Operations Benchmarks");
    Console.WriteLine("5. All Benchmarks");
    Console.WriteLine("6. Memory Allocation Benchmarks");
    Console.WriteLine();
    Console.Write("Enter choice (1-6): ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            BenchmarkRunner.Run<InsertBenchmarks>(config);
            break;
        case "2":
            BenchmarkRunner.Run<ReadBenchmarks>(config);
            break;
        case "3":
            BenchmarkRunner.Run<UpdateBenchmarks>(config);
            break;
        case "4":
            BenchmarkRunner.Run<MixedOperationsBenchmarks>(config);
            break;
        case "5":
            BenchmarkRunner.Run<InsertBenchmarks>(config);
            BenchmarkRunner.Run<ReadBenchmarks>(config);
            BenchmarkRunner.Run<UpdateBenchmarks>(config);
            BenchmarkRunner.Run<MixedOperationsBenchmarks>(config);
            break;
        case "6":
            BenchmarkRunner.Run<MemoryAllocationBenchmarks>(config);
            break;
        default:
            BenchmarkRunner.Run<InsertBenchmarks>(config);
            BenchmarkRunner.Run<ReadBenchmarks>(config);
            BenchmarkRunner.Run<UpdateBenchmarks>(config);
            BenchmarkRunner.Run<MixedOperationsBenchmarks>(config);
            break;
    }
}
#endif

static void RunQuickTest()
{
    Console.WriteLine("Running quick insert test with 100 entities...");

    var insertBenchmark = new InsertBenchmarks { EntityCount = 100 };
    insertBenchmark.GlobalSetup();

    var sw = System.Diagnostics.Stopwatch.StartNew();
    insertBenchmark.IterationSetup();
    insertBenchmark.InMemory_Insert();
    sw.Stop();
    Console.WriteLine($"InMemory Insert: {sw.ElapsedMilliseconds}ms");

    sw.Restart();
    insertBenchmark.IterationSetup();
    insertBenchmark.Markdown_Insert();
    sw.Stop();
    Console.WriteLine($"Markdown Insert: {sw.ElapsedMilliseconds}ms");

    sw.Restart();
    insertBenchmark.IterationSetup();
    insertBenchmark.Sqlite_Insert();
    sw.Stop();
    Console.WriteLine($"SQLite Insert: {sw.ElapsedMilliseconds}ms");

    insertBenchmark.GlobalCleanup();

    Console.WriteLine();
    Console.WriteLine("Quick test complete. Run in Release mode for accurate benchmarks.");
}
