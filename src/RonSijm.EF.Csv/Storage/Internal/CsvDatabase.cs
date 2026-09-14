// Licensed under the MIT license.

using RonSijm.EF.Csv.Infrastructure.Internal;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// The CSV database implementation.
/// Inherits generic functionality from FileDatabaseBase.
/// </summary>
public class CsvDatabase : FileDatabaseBase, ICsvDatabase
{
    private readonly ICsvStore _csvStore;

    /// <summary>
    /// Creates a new instance of <see cref="CsvDatabase"/>.
    /// </summary>
    public CsvDatabase(
        DatabaseDependencies dependencies,
        ICsvStoreCache storeCache,
        IDbContextOptions options)
        : base(dependencies, GetStore(storeCache, options))
    {
        _csvStore = (ICsvStore)base.Store;
    }

    private static IFileStore GetStore(ICsvStoreCache storeCache, IDbContextOptions options)
    {
        var csvOptions = options.FindExtension<CsvOptionsExtension>()
            ?? throw new InvalidOperationException("CSV options extension not found.");

        return storeCache.GetStore(
            csvOptions.DirectoryPath,
            csvOptions.FileExtension,
            csvOptions.EnforceForeignKeys);
    }

    /// <inheritdoc />
    public new ICsvStore Store => _csvStore;
}

