// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Interface for caching CSV stores.
/// Extends the generic IFileStoreCache interface.
/// </summary>
public interface ICsvStoreCache : IFileStoreCache
{
    /// <summary>
    /// Gets or creates a store for the given context.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <returns>The CSV store.</returns>
    new ICsvStore GetStore(string directoryPath, string fileExtension);

    /// <summary>
    /// Gets or creates a store for the given context with foreign key enforcement option.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="enforceForeignKeys">Whether to enforce foreign key constraints.</param>
    /// <returns>The CSV store.</returns>
    new ICsvStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys);
}

