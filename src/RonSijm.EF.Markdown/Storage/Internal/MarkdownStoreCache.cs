// Licensed under the MIT license.

using System.Collections.Concurrent;
using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// Cache for Markdown stores.
/// </summary>
public class MarkdownStoreCache : IMarkdownStoreCache
{
    private static readonly ConcurrentDictionary<string, IMarkdownStore> _stores = new();

    /// <inheritdoc />
    public IMarkdownStore GetStore(string directoryPath, string fileExtension)
    {
        var key = $"{directoryPath}|{fileExtension}";
        return _stores.GetOrAdd(key, _ => new MarkdownStore(directoryPath, fileExtension));
    }

    /// <inheritdoc />
    public IMarkdownStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
    {
        var key = $"{directoryPath}|{fileExtension}|{enforceForeignKeys}";
        return _stores.GetOrAdd(key, _ => new MarkdownStore(directoryPath, fileExtension, enforceForeignKeys));
    }

    /// <inheritdoc />
    IFileStore IFileStoreCache.GetStore(string directoryPath, string fileExtension)
    {
        return GetStore(directoryPath, fileExtension);
    }

    /// <inheritdoc />
    IFileStore IFileStoreCache.GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
    {
        return GetStore(directoryPath, fileExtension, enforceForeignKeys);
    }
}

