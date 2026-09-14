// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Interface for a file-based table that stores entity data.
/// </summary>
public interface IFileTable
{
    /// <summary>
    /// Gets the entity type for this table.
    /// </summary>
    IEntityType EntityType { get; }

    /// <summary>
    /// Gets all rows in the table.
    /// </summary>
    IReadOnlyList<object[]> Rows { get; }

    /// <summary>
    /// Creates a new row from the given update entry.
    /// </summary>
    /// <param name="entry">The update entry.</param>
    void Create(IUpdateEntry entry);

    /// <summary>
    /// Updates an existing row from the given update entry.
    /// </summary>
    /// <param name="entry">The update entry.</param>
    void Update(IUpdateEntry entry);

    /// <summary>
    /// Deletes a row based on the given update entry.
    /// </summary>
    /// <param name="entry">The update entry.</param>
    void Delete(IUpdateEntry entry);

    /// <summary>
    /// Loads data from the file.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves data to the file.
    /// </summary>
    void Save();

    /// <summary>
    /// Finds a row by its primary key values.
    /// </summary>
    /// <param name="keyValues">The primary key values.</param>
    /// <returns>The row data, or null if not found.</returns>
    object[]? FindRow(object[] keyValues);
}

