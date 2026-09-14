// Licensed under the MIT license.

using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Data;

/// <summary>
/// DbContext for InMemory provider benchmarks.
/// </summary>
public class InMemoryBenchmarkDbContext : DbContext
{
    public InMemoryBenchmarkDbContext(DbContextOptions<InMemoryBenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkEntity> Entities { get; set; } = null!;
}

