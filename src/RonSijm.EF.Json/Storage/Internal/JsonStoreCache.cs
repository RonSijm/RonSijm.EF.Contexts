// Licensed under the MIT license.

using System.Collections.Concurrent;
using RonSijm.EF.FileStore.Storage.Internal;
using RonSijm.EF.Json.Infrastructure.Internal;
using RonSijm.EF.Json.Serialization;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Cache for JSON stores.
/// </summary>
public class JsonStoreCache : IJsonStoreCache
{
    private static readonly ConcurrentDictionary<string, IJsonStore> _stores = new();

    /// <inheritdoc />
    public IJsonStore GetStore(string directoryPath, string fileExtension)
    {
        var key = $"{directoryPath}|{fileExtension}";
        return _stores.GetOrAdd(key, _ => new JsonStore(directoryPath, fileExtension));
    }

    /// <inheritdoc />
    public IJsonStore GetStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
    {
        var key = $"{directoryPath}|{fileExtension}|{enforceForeignKeys}";
        return _stores.GetOrAdd(key, _ => new JsonStore(directoryPath, fileExtension, enforceForeignKeys));
    }

    /// <inheritdoc />
    public IJsonStore GetStore(JsonOptionsExtension options)
    {
        var key = BuildCacheKey(options);
        return _stores.GetOrAdd(key, _ => CreateStore(options));
    }

    private static IJsonStore CreateStore(JsonOptionsExtension options)
    {
        return options.StorageMode switch
        {
            JsonStorageMode.SingleFile => new JsonSingleFileStore(options),
            _ => new JsonStore(options)
        };
    }

    private static string BuildCacheKey(JsonOptionsExtension options)
    {
        return string.Join("|",
            options.DirectoryPath,
            options.FileExtension,
            options.EnforceForeignKeys,
            options.StorageMode,
            options.ReferenceMode,
            options.SingleFileName ?? "",
            options.UseIndentation,
            options.PropertyNamingPolicy?.GetType().FullName ?? "");
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

