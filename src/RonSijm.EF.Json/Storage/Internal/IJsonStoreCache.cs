// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using RonSijm.EF.Json.Infrastructure.Internal;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Interface for caching JSON stores.
/// Extends the generic IFileStoreCache interface.
/// </summary>
public interface IJsonStoreCache : IFileStoreCache
{
    /// <summary>
    /// Gets or creates a store for the given context.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <returns>The JSON store.</returns>
    new IJsonStore GetStore(string directoryPath, string fileExtension);

    /// <summary>
    /// Gets or creates a store for the given context with foreign key enforcement option.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="enforceForeignKeys">Whether to enforce foreign key constraints.</param>
    /// <returns>The JSON store.</returns>
    new IJsonStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys);

    /// <summary>
    /// Gets or creates a store with full JSON options.
    /// </summary>
    /// <param name="options">The JSON options extension.</param>
    /// <returns>The JSON store.</returns>
    IJsonStore GetStore(JsonOptionsExtension options);
}

