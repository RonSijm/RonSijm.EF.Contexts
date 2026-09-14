// Licensed under the MIT license.

using RonSijm.EF.Csv.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// CSV specific extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class CsvDbContextOptionsExtensions
{
    /// <summary>
    /// Configures the context to use the CSV database provider, storing entities as CSV files.
    /// </summary>
    /// <typeparam name="TContext">The type of context being configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where CSV files will be stored.</param>
    /// <param name="csvOptionsAction">An optional action to allow additional CSV specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseCsv<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string directoryPath,
        Action<CsvDbContextOptionsBuilder>? csvOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseCsv(
            (DbContextOptionsBuilder)optionsBuilder, directoryPath, csvOptionsAction);

    /// <summary>
    /// Configures the context to use the CSV database provider, storing entities as CSV files.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where CSV files will be stored.</param>
    /// <param name="csvOptionsAction">An optional action to allow additional CSV specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseCsv(
        this DbContextOptionsBuilder optionsBuilder,
        string directoryPath,
        Action<CsvDbContextOptionsBuilder>? csvOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var extension = optionsBuilder.Options.FindExtension<CsvOptionsExtension>()
            ?? new CsvOptionsExtension();

        extension = extension.WithDirectoryPath(directoryPath);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        csvOptionsAction?.Invoke(new CsvDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
}

