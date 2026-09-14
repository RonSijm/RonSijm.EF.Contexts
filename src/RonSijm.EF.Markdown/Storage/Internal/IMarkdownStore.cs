// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// Interface for the Markdown store that manages entity data.
/// Extends the generic IFileStore interface.
/// </summary>
public interface IMarkdownStore : IFileStore
{
    /// <summary>
    /// Gets the table for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The markdown table.</returns>
    new IMarkdownTable GetTable(IEntityType entityType);
}

