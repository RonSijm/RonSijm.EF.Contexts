// Licensed under the MIT license.

using RonSijm.EF.Csv.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Database creator for the CSV provider.
/// </summary>
public class CsvDatabaseCreator : IDatabaseCreator
{
    private readonly ICsvDatabase _database;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly CsvOptionsExtension _options;

    /// <summary>
    /// Creates a new instance of <see cref="CsvDatabaseCreator"/>.
    /// </summary>
    public CsvDatabaseCreator(
        ICsvDatabase database,
        IDesignTimeModel designTimeModel,
        IDbContextOptions options)
    {
        _database = database;
        _designTimeModel = designTimeModel;
        _options = options.FindExtension<CsvOptionsExtension>()
            ?? throw new InvalidOperationException("CSV options extension not found.");
    }

    /// <inheritdoc />
    public virtual bool EnsureCreated()
    {
        return _database.Store.EnsureCreated(_designTimeModel.Model);
    }

    /// <inheritdoc />
    public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnsureCreated());
    }

    /// <inheritdoc />
    public virtual bool EnsureDeleted()
    {
        return _database.Store.EnsureDeleted();
    }

    /// <inheritdoc />
    public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnsureDeleted());
    }

    /// <inheritdoc />
    public virtual bool CanConnect()
    {
        return Directory.Exists(_options.DirectoryPath) || _options.CreateDirectoryIfNotExists;
    }

    /// <inheritdoc />
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CanConnect());
    }
}

