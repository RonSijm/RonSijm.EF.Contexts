// Licensed under the MIT license.

using RonSijm.EF.Csv.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Allows CSV specific configuration to be performed on <see cref="DbContextOptions"/>.
/// </summary>
public class CsvDbContextOptionsBuilder
{
    /// <summary>
    /// Creates a new instance of <see cref="CsvDbContextOptionsBuilder"/>.
    /// </summary>
    public CsvDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    /// <summary>
    /// Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    /// <summary>
    /// Sets the file extension for CSV files (default is ".csv").
    /// </summary>
    /// <param name="extension">The file extension to use.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual CsvDbContextOptionsBuilder UseFileExtension(string extension)
    {
        var csvExtension = OptionsBuilder.Options.FindExtension<CsvOptionsExtension>()
            ?? new CsvOptionsExtension();

        csvExtension = csvExtension.WithFileExtension(extension);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(csvExtension);

        return this;
    }

    /// <summary>
    /// Configures whether to create the directory if it doesn't exist.
    /// </summary>
    /// <param name="create">Whether to create the directory.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual CsvDbContextOptionsBuilder CreateDirectoryIfNotExists(bool create = true)
    {
        var csvExtension = OptionsBuilder.Options.FindExtension<CsvOptionsExtension>()
            ?? new CsvOptionsExtension();

        csvExtension = csvExtension.WithCreateDirectoryIfNotExists(create);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(csvExtension);

        return this;
    }

    /// <summary>
    /// Configures whether to enforce foreign key constraints.
    /// When enabled, insert/update/delete operations will validate referential integrity.
    /// </summary>
    /// <param name="enforce">Whether to enforce foreign key constraints. Default is true when this method is called.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual CsvDbContextOptionsBuilder EnforceForeignKeys(bool enforce = true)
    {
        var csvExtension = OptionsBuilder.Options.FindExtension<CsvOptionsExtension>()
            ?? new CsvOptionsExtension();

        csvExtension = csvExtension.WithEnforceForeignKeys(enforce);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(csvExtension);

        return this;
    }
}

