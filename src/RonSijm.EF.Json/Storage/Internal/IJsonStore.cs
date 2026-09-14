// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Interface for the JSON store that manages entity data.
/// Extends the generic IFileStore interface.
/// </summary>
public interface IJsonStore : IFileStore
{
    /// <summary>
    /// Gets the table for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The JSON table.</returns>
    new IJsonTable GetTable(IEntityType entityType);
}

