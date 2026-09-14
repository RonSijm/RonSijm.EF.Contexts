// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.FileStore.Query.Internal;

/// <summary>
/// Factory for creating file store query contexts.
/// </summary>
public class FileStoreQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="FileStoreQueryContextFactory"/>.
    /// </summary>
    public FileStoreQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryContext Create()
    {
        return new FileStoreQueryContext(_dependencies);
    }
}

