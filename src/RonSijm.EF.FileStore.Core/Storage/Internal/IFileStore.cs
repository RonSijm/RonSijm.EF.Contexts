// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Interface for the file store that manages entity data.
/// </summary>
public interface IFileStore
{
    /// <summary>
    /// Gets the directory path where files are stored.
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    /// Ensures the database (directory) is created.
    /// </summary>
    /// <param name="model">The model to create tables for.</param>
    /// <returns>True if the database was created, false if it already existed.</returns>
    bool EnsureCreated(IModel model);

    /// <summary>
    /// Ensures the database (directory) is deleted.
    /// </summary>
    /// <returns>True if the database was deleted, false if it didn't exist.</returns>
    bool EnsureDeleted();

    /// <summary>
    /// Executes a transaction with the given update entries.
    /// </summary>
    /// <param name="entries">The entries to process.</param>
    /// <returns>The number of state entries written to the database.</returns>
    int ExecuteTransaction(IList<IUpdateEntry> entries);

    /// <summary>
    /// Gets all rows for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>An enumerable of entity data.</returns>
    IEnumerable<object[]> GetRows(IEntityType entityType);

    /// <summary>
    /// Gets the table for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The file table.</returns>
    IFileTable GetTable(IEntityType entityType);
}

