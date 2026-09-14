﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Serialization;

namespace RonSijm.EF.Json.Infrastructure.Internal;

/// <summary>
/// Singleton options implementation for the JSON database provider.
/// </summary>
public class JsonSingletonOptions : IJsonSingletonOptions
{
    /// <summary>
    /// Gets the directory path where JSON files are stored.
    /// </summary>
    public string? DirectoryPath { get; private set; }

    /// <summary>
    /// Gets whether to create the directory if it doesn't exist.
    /// </summary>
    public bool CreateDirectoryIfNotExists { get; private set; }

    /// <summary>
    /// Gets whether to enforce foreign key constraints.
    /// </summary>
    public bool EnforceForeignKeys { get; private set; }

    /// <summary>
    /// Gets the storage mode (multiple files or single file).
    /// </summary>
    public JsonStorageMode StorageMode { get; private set; }

    /// <summary>
    /// Gets the reference mode for foreign key serialization.
    /// </summary>
    public JsonReferenceMode ReferenceMode { get; private set; }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var jsonOptions = options.FindExtension<JsonOptionsExtension>();
        if (jsonOptions != null)
        {
            DirectoryPath = jsonOptions.DirectoryPath;
            CreateDirectoryIfNotExists = jsonOptions.CreateDirectoryIfNotExists;
            EnforceForeignKeys = jsonOptions.EnforceForeignKeys;
            StorageMode = jsonOptions.StorageMode;
            ReferenceMode = jsonOptions.ReferenceMode;
        }
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var jsonOptions = options.FindExtension<JsonOptionsExtension>();
        if (jsonOptions != null)
        {
            // Validate options
        }
    }
}

