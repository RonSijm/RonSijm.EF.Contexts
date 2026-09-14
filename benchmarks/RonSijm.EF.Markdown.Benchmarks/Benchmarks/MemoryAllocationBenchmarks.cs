// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using RonSijm.EF.Markdown.Benchmarks.Data;
using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks specifically focused on memory allocation patterns.
/// This benchmark helps identify memory optimization opportunities.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[GcServer(true)]
public class MemoryAllocationBenchmarks
{
    private string _markdownDir = null!;
    private string _sqliteDbPath = null!;
    private string _inMemoryDbName = null!;

    [Params(100, 1000, 5000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _markdownDir = Path.Combine(Path.GetTempPath(), $"MarkdownMemBenchmark_{Guid.NewGuid()}");
        _sqliteDbPath = Path.Combine(Path.GetTempPath(), $"SqliteMemBenchmark_{Guid.NewGuid()}.db");
        _inMemoryDbName = $"InMemoryMemBenchmark_{Guid.NewGuid()}";

        // Seed data for all providers
        SeedInMemoryData();
        SeedMarkdownData();
        SeedSqliteData();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (Directory.Exists(_markdownDir))
        {
            try { Directory.Delete(_markdownDir, true); } catch { }
        }
        if (File.Exists(_sqliteDbPath))
        {
            try { File.Delete(_sqliteDbPath); } catch { }
        }
    }

    private static BenchmarkEntity CreateEntity(int index) => new()
    {
        Name = $"Entity {index}",
        Description = $"Description for entity {index} with some additional text to make it longer",
        Price = 10.99m + index,
        Quantity = index * 10,
        CreatedAt = DateTime.UtcNow,
        IsActive = index % 2 == 0,
        ExternalId = Guid.NewGuid()
    };

    private void SeedInMemoryData()
    {
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(_inMemoryDbName)
            .Options;

        using var context = new InMemoryBenchmarkDbContext(options);
        context.Database.EnsureCreated();

        for (int i = 0; i < EntityCount; i++)
        {
            context.Entities.Add(CreateEntity(i));
        }
        context.SaveChanges();
    }

    private void SeedMarkdownData()
    {
        var options = new DbContextOptionsBuilder<MarkdownBenchmarkDbContext>()
            .UseMarkdown(_markdownDir)
            .Options;

        using var context = new MarkdownBenchmarkDbContext(options);
        context.Database.EnsureCreated();

        for (int i = 0; i < EntityCount; i++)
        {
            context.Entities.Add(CreateEntity(i));
        }
        context.SaveChanges();
    }

    private void SeedSqliteData()
    {
        var options = new DbContextOptionsBuilder<SqliteBenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqliteDbPath}")
            .Options;

        using var context = new SqliteBenchmarkDbContext(options);
        context.Database.EnsureCreated();

        for (int i = 0; i < EntityCount; i++)
        {
            context.Entities.Add(CreateEntity(i));
        }
        context.SaveChanges();
    }

    /// <summary>
    /// Measures memory allocation for reading all entities from InMemory provider.
    /// </summary>
    [Benchmark(Baseline = true)]
    public List<BenchmarkEntity> InMemory_ReadAll()
    {
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(_inMemoryDbName)
            .Options;

        using var context = new InMemoryBenchmarkDbContext(options);
        return context.Entities.ToList();
    }

    /// <summary>
    /// Measures memory allocation for reading all entities from Markdown provider.
    /// This is the key benchmark for identifying string allocation overhead.
    /// </summary>
    [Benchmark]
    public List<BenchmarkEntity> Markdown_ReadAll()
    {
        var options = new DbContextOptionsBuilder<MarkdownBenchmarkDbContext>()
            .UseMarkdown(_markdownDir)
            .Options;

        using var context = new MarkdownBenchmarkDbContext(options);
        return context.Entities.ToList();
    }

    /// <summary>
    /// Measures memory allocation for reading all entities from SQLite provider.
    /// </summary>
    [Benchmark]
    public List<BenchmarkEntity> Sqlite_ReadAll()
    {
        var options = new DbContextOptionsBuilder<SqliteBenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqliteDbPath}")
            .Options;

        using var context = new SqliteBenchmarkDbContext(options);
        return context.Entities.ToList();
    }
}

