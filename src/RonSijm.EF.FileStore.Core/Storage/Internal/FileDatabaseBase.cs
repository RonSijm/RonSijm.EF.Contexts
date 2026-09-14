// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Abstract base class for file-based database implementations.
/// </summary>
public abstract class FileDatabaseBase : Database, IFileDatabase
{
    private readonly IFileStore _store;

    /// <summary>
    /// Creates a new instance of <see cref="FileDatabaseBase"/>.
    /// </summary>
    protected FileDatabaseBase(
        DatabaseDependencies dependencies,
        IFileStore store)
        : base(dependencies)
    {
        _store = store;
    }

    /// <inheritdoc />
    public virtual IFileStore Store => _store;

    /// <inheritdoc />
    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        return _store.ExecuteTransaction(entries);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(SaveChanges(entries));
    }
}

