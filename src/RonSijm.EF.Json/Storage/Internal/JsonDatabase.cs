// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// The JSON database implementation.
/// Inherits generic functionality from FileDatabaseBase.
/// </summary>
public class JsonDatabase : FileDatabaseBase, IJsonDatabase
{
    private readonly IJsonStore _jsonStore;

    /// <summary>
    /// Creates a new instance of <see cref="JsonDatabase"/>.
    /// </summary>
    public JsonDatabase(
        DatabaseDependencies dependencies,
        IJsonStoreCache storeCache,
        IDbContextOptions options)
        : base(dependencies, GetStore(storeCache, options))
    {
        _jsonStore = (IJsonStore)base.Store;
    }

    private static IFileStore GetStore(IJsonStoreCache storeCache, IDbContextOptions options)
    {
        var jsonOptions = options.FindExtension<JsonOptionsExtension>()
            ?? throw new InvalidOperationException("JSON options extension not found.");

        return storeCache.GetStore(jsonOptions);
    }

    /// <inheritdoc />
    public new IJsonStore Store => _jsonStore;
}

