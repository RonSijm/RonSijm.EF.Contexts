// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using RonSijm.EF.Markdown.Benchmarks.Data;
using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks for read operations across different EF Core providers.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class ReadBenchmarks
{
    private string _markdownDir = null!;
    private string _csvDir = null!;
    private string _sqliteDbPath = null!;
    private string _inMemoryDbName = null!;

    [Params(10, 100, 1000, 10000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _markdownDir = Path.Combine(Path.GetTempPath(), $"MarkdownBenchmark_{Guid.NewGuid()}");
        _csvDir = Path.Combine(Path.GetTempPath(), $"CsvBenchmark_{Guid.NewGuid()}");
        _sqliteDbPath = Path.Combine(Path.GetTempPath(), $"SqliteBenchmark_{Guid.NewGuid()}.db");
        _inMemoryDbName = $"InMemoryBenchmark_{Guid.NewGuid()}";

        // Seed data for all providers
        SeedInMemoryData();
        SeedMarkdownData();
        SeedCsvData();
        SeedSqliteData();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (Directory.Exists(_markdownDir))
        {
            try { Directory.Delete(_markdownDir, true); } catch { }
        }
        if (Directory.Exists(_csvDir))
        {
            try { Directory.Delete(_csvDir, true); } catch { }
        }
        if (File.Exists(_sqliteDbPath))
        {
            try { File.Delete(_sqliteDbPath); } catch { }
        }
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

    private void SeedCsvData()
    {
        var options = new DbContextOptionsBuilder<CsvBenchmarkDbContext>()
            .UseCsv(_csvDir)
            .Options;

        using var context = new CsvBenchmarkDbContext(options);
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
    public List<BenchmarkEntity> InMemory_ReadAll()
    {
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(_inMemoryDbName)
            .Options;

        using var context = new InMemoryBenchmarkDbContext(options);
        return context.Entities.ToList();
    }

    [Benchmark]
    public List<BenchmarkEntity> Markdown_ReadAll()
    {
        var options = new DbContextOptionsBuilder<MarkdownBenchmarkDbContext>()
            .UseMarkdown(_markdownDir)
            .Options;

        using var context = new MarkdownBenchmarkDbContext(options);
        return context.Entities.ToList();
    }

    [Benchmark]
    public List<BenchmarkEntity> Csv_ReadAll()
    {
        var options = new DbContextOptionsBuilder<CsvBenchmarkDbContext>()
            .UseCsv(_csvDir)
            .Options;

        using var context = new CsvBenchmarkDbContext(options);
        return context.Entities.ToList();
    }

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

