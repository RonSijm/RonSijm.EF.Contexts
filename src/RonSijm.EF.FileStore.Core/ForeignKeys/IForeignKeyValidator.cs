// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.ForeignKeys;

/// <summary>
/// Interface for validating foreign key constraints.
/// </summary>
public interface IForeignKeyValidator
{
    /// <summary>
    /// Validates that all foreign key constraints are satisfied for an insert operation.
    /// </summary>
    /// <param name="entry">The update entry being inserted.</param>
    /// <param name="getTable">Function to get a table for an entity type.</param>
    /// <exception cref="ForeignKeyValidationException">Thrown when a foreign key constraint is violated.</exception>
    void ValidateInsert(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable);

    /// <summary>
    /// Validates that no foreign key constraints are violated by a delete operation.
    /// </summary>
    /// <param name="entry">The update entry being deleted.</param>
    /// <param name="getTable">Function to get a table for an entity type.</param>
    /// <exception cref="ForeignKeyValidationException">Thrown when a foreign key constraint is violated.</exception>
    void ValidateDelete(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable);

    /// <summary>
    /// Validates that all foreign key constraints are satisfied for an update operation.
    /// </summary>
    /// <param name="entry">The update entry being updated.</param>
    /// <param name="getTable">Function to get a table for an entity type.</param>
    /// <exception cref="ForeignKeyValidationException">Thrown when a foreign key constraint is violated.</exception>
    void ValidateUpdate(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable);
}

