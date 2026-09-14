// Licensed under the MIT license.

using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Data;

/// <summary>
/// DbContext for Markdown provider benchmarks.
/// </summary>
public class MarkdownBenchmarkDbContext : DbContext
{
    public MarkdownBenchmarkDbContext(DbContextOptions<MarkdownBenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkEntity> Entities { get; set; } = null!;
}

