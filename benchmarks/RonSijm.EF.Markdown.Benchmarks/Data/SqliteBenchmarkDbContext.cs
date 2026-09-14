// Licensed under the MIT license.

using RonSijm.EF.Markdown.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Benchmarks.Data;

/// <summary>
/// DbContext for SQLite provider benchmarks.
/// </summary>
public class SqliteBenchmarkDbContext : DbContext
{
    public SqliteBenchmarkDbContext(DbContextOptions<SqliteBenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkEntity> Entities { get; set; } = null!;
}

