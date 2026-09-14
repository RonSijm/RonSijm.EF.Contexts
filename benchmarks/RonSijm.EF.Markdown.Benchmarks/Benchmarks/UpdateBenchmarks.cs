// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using RonSijm.EF.Markdown.Benchmarks.Data;
using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks for update operations across different EF Core providers.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class UpdateBenchmarks
{
    private string _markdownDir = null!;
    private string _sqliteDbPath = null!;
    private string _inMemoryDbName = null!;
    private int _iterationCount;

    [Params(10, 100)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _markdownDir = Path.Combine(Path.GetTempPath(), $"MarkdownBenchmark_{Guid.NewGuid()}");
        _sqliteDbPath = Path.Combine(Path.GetTempPath(), $"SqliteBenchmark_{Guid.NewGuid()}.db");
        _inMemoryDbName = $"InMemoryBenchmark_{Guid.NewGuid()}";
        _iterationCount = 0;
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

    [IterationSetup]
    public void IterationSetup()
    {
        _iterationCount++;
        
        // Re-seed data before each iteration
        if (Directory.Exists(_markdownDir))
        {
            try { Directory.Delete(_markdownDir, true); } catch { }
        }
        if (File.Exists(_sqliteDbPath))
        {
            try { File.Delete(_sqliteDbPath); } catch { }
        }

        SeedInMemoryData();
        SeedMarkdownData();
        SeedSqliteData();
    }

    private static BenchmarkEntity CreateEntity(int index) => new()
    {
        Name = $"Entity {index}",
        Description = $"Description for entity {index}",
        Price = 10.99m + index,
        Quantity = index * 10,
        CreatedAt = DateTime.UtcNow,
        IsActive = index % 2 == 0,
        ExternalId = Guid.NewGuid()
    };

    private void SeedInMemoryData()
    {
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(_inMemoryDbName + _iterationCount)
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

    [Benchmark(Baseline = true)]
    public void InMemory_UpdateAll()
    {
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(_inMemoryDbName + _iterationCount)
            .Options;

        using var context = new InMemoryBenchmarkDbContext(options);
        var entities = context.Entities.ToList();
        foreach (var entity in entities)
        {
            entity.Name = $"Updated {entity.Name}";
            entity.Price += 1;
        }
        context.SaveChanges();
    }

    [Benchmark]
    public void Markdown_UpdateAll()
    {
        var options = new DbContextOptionsBuilder<MarkdownBenchmarkDbContext>()
            .UseMarkdown(_markdownDir)
            .Options;

        using var context = new MarkdownBenchmarkDbContext(options);
        var entities = context.Entities.ToList();
        foreach (var entity in entities)
        {
            entity.Name = $"Updated {entity.Name}";
            entity.Price += 1;
        }
        context.SaveChanges();
    }

    [Benchmark]
    public void Sqlite_UpdateAll()
    {
        var options = new DbContextOptionsBuilder<SqliteBenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqliteDbPath}")
            .Options;

        using var context = new SqliteBenchmarkDbContext(options);
        var entities = context.Entities.ToList();
        foreach (var entity in entities)
        {
            entity.Name = $"Updated {entity.Name}";
            entity.Price += 1;
        }
        context.SaveChanges();
    }
}

