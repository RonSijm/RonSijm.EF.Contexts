// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using RonSijm.EF.Markdown.Benchmarks.Data;
using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks for mixed CRUD operations across different EF Core providers.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class MixedOperationsBenchmarks
{
    private string _markdownDir = null!;
    private string _sqliteDbPath = null!;
    private int _iterationCount;

    [Params(50)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _markdownDir = Path.Combine(Path.GetTempPath(), $"MarkdownBenchmark_{Guid.NewGuid()}");
        _sqliteDbPath = Path.Combine(Path.GetTempPath(), $"SqliteBenchmark_{Guid.NewGuid()}.db");
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
        Description = $"Description for entity {index}",
        Price = 10.99m + index,
        Quantity = index * 10,
        CreatedAt = DateTime.UtcNow,
        IsActive = index % 2 == 0,
        ExternalId = Guid.NewGuid()
    };

    [Benchmark(Baseline = true)]
    public void InMemory_MixedOperations()
    {
        var dbName = $"InMemoryBenchmark_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<InMemoryBenchmarkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        // Create
        using (var context = new InMemoryBenchmarkDbContext(options))
        {
            context.Database.EnsureCreated();
            for (int i = 0; i < EntityCount; i++)
            {
                context.Entities.Add(CreateEntity(i));
            }
            context.SaveChanges();
        }

        // Read
        using (var context = new InMemoryBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
        }

        // Update
        using (var context = new InMemoryBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
            foreach (var entity in entities.Take(EntityCount / 2))
            {
                entity.Name = $"Updated {entity.Name}";
            }
            context.SaveChanges();
        }

        // Delete
        using (var context = new InMemoryBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList().Take(EntityCount / 4).ToList();
            context.Entities.RemoveRange(entities);
            context.SaveChanges();
        }

        // Final Read
        using (var context = new InMemoryBenchmarkDbContext(options))
        {
            var finalCount = context.Entities.ToList().Count;
        }
    }

    [Benchmark]
    public void Markdown_MixedOperations()
    {
        var options = new DbContextOptionsBuilder<MarkdownBenchmarkDbContext>()
            .UseMarkdown(_markdownDir)
            .Options;

        // Create
        using (var context = new MarkdownBenchmarkDbContext(options))
        {
            context.Database.EnsureCreated();
            for (int i = 0; i < EntityCount; i++)
            {
                context.Entities.Add(CreateEntity(i));
            }
            context.SaveChanges();
        }

        // Read
        using (var context = new MarkdownBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
        }

        // Update
        using (var context = new MarkdownBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
            foreach (var entity in entities.Take(EntityCount / 2))
            {
                entity.Name = $"Updated {entity.Name}";
            }
            context.SaveChanges();
        }

        // Delete
        using (var context = new MarkdownBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList().Take(EntityCount / 4).ToList();
            context.Entities.RemoveRange(entities);
            context.SaveChanges();
        }

        // Final Read
        using (var context = new MarkdownBenchmarkDbContext(options))
        {
            var finalCount = context.Entities.ToList().Count;
        }
    }

    [Benchmark]
    public void Sqlite_MixedOperations()
    {
        var options = new DbContextOptionsBuilder<SqliteBenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqliteDbPath}")
            .Options;

        // Create
        using (var context = new SqliteBenchmarkDbContext(options))
        {
            context.Database.EnsureCreated();
            for (int i = 0; i < EntityCount; i++)
            {
                context.Entities.Add(CreateEntity(i));
            }
            context.SaveChanges();
        }

        // Read
        using (var context = new SqliteBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
        }

        // Update
        using (var context = new SqliteBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList();
            foreach (var entity in entities.Take(EntityCount / 2))
            {
                entity.Name = $"Updated {entity.Name}";
            }
            context.SaveChanges();
        }

        // Delete
        using (var context = new SqliteBenchmarkDbContext(options))
        {
            var entities = context.Entities.ToList().Take(EntityCount / 4).ToList();
            context.Entities.RemoveRange(entities);
            context.SaveChanges();
        }

        // Final Read
        using (var context = new SqliteBenchmarkDbContext(options))
        {
            var finalCount = context.Entities.ToList().Count;
        }
    }
}

