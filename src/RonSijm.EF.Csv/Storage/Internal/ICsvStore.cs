// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Interface for a CSV store.
/// Extends the generic IFileStore interface.
/// </summary>
public interface ICsvStore : IFileStore
{
    /// <summary>
    /// Gets the table for the specified entity type.
    /// </summary>
    new ICsvTable GetTable(IEntityType entityType);
}

