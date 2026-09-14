// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RonSijm.EF.FileStore.Infrastructure.Internal;

/// <summary>
/// Singleton options interface for file-based database providers.
/// </summary>
public interface IFileStoreSingletonOptions : ISingletonOptions
{
    /// <summary>
    /// Gets the directory path where files are stored.
    /// </summary>
    string? DirectoryPath { get; }

    /// <summary>
    /// Gets whether to create the directory if it doesn't exist.
    /// </summary>
    bool CreateDirectoryIfNotExists { get; }

    /// <summary>
    /// Gets whether to enforce foreign key constraints.
    /// When enabled, insert/update/delete operations will validate referential integrity.
    /// Default is false for performance reasons.
    /// </summary>
    bool EnforceForeignKeys { get; }
}

