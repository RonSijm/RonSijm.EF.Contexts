// Licensed under the MIT license.

using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Data;

/// <summary>
/// DbContext for CSV provider benchmarks.
/// </summary>
public class CsvBenchmarkDbContext : DbContext
{
    public CsvBenchmarkDbContext(DbContextOptions<CsvBenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkEntity> Entities { get; set; } = null!;
}

