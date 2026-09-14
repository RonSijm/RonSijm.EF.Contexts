// Licensed under the MIT license.

using System.Collections.Concurrent;
using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Cache for CSV stores.
/// </summary>
public class CsvStoreCache : ICsvStoreCache
{
    private static readonly ConcurrentDictionary<string, ICsvStore> _stores = new();

    /// <inheritdoc />
    public ICsvStore GetStore(string directoryPath, string fileExtension)
    {
        var key = $"{directoryPath}|{fileExtension}";
        return _stores.GetOrAdd(key, _ => new CsvStore(directoryPath, fileExtension));
    }

    /// <inheritdoc />
    public ICsvStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
    {
        var key = $"{directoryPath}|{fileExtension}|{enforceForeignKeys}";
        return _stores.GetOrAdd(key, _ => new CsvStore(directoryPath, fileExtension, enforceForeignKeys));
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

