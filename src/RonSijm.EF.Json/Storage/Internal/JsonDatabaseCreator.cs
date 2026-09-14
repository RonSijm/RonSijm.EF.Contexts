﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Database creator for the JSON provider.
/// </summary>
public class JsonDatabaseCreator : IDatabaseCreator
{
    private readonly IJsonDatabase _database;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly JsonOptionsExtension _options;

    /// <summary>
    /// Creates a new instance of <see cref="JsonDatabaseCreator"/>.
    /// </summary>
    public JsonDatabaseCreator(
        IJsonDatabase database,
        IDesignTimeModel designTimeModel,
        IDbContextOptions options)
    {
        _database = database;
        _designTimeModel = designTimeModel;
        _options = options.FindExtension<JsonOptionsExtension>()
            ?? throw new InvalidOperationException("JSON options extension not found.");
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

