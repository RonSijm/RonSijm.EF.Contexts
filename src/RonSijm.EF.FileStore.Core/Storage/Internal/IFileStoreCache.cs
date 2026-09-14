// Licensed under the MIT license.

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Interface for caching file stores.
/// </summary>
public interface IFileStoreCache
{
    /// <summary>
    /// Gets or creates a store for the given context.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <returns>The file store.</returns>
    IFileStore GetStore(string directoryPath, string fileExtension);

    /// <summary>
    /// Gets or creates a store for the given context with foreign key enforcement option.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="enforceForeignKeys">Whether to enforce foreign key constraints.</param>
    /// <returns>The file store.</returns>
    IFileStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys);
}

