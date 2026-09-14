// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RonSijm.EF.Csv.Infrastructure.Internal;

/// <summary>
/// Singleton options implementation for the CSV provider.
/// </summary>
public class CsvSingletonOptions : ICsvSingletonOptions
{
    /// <inheritdoc />
    public string? DirectoryPath { get; private set; }

    /// <inheritdoc />
    public bool CreateDirectoryIfNotExists { get; private set; }

    /// <inheritdoc />
    public bool EnforceForeignKeys { get; private set; }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var csvExtension = options.FindExtension<CsvOptionsExtension>();
        if (csvExtension != null)
        {
            DirectoryPath = csvExtension.DirectoryPath;
            CreateDirectoryIfNotExists = csvExtension.CreateDirectoryIfNotExists;
            EnforceForeignKeys = csvExtension.EnforceForeignKeys;
        }
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        // Validation is done in CsvOptionsExtension
    }
}

