// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.FileStore.Query.Internal;

/// <summary>
/// Query context for file-based queries.
/// </summary>
public class FileStoreQueryContext : QueryContext
{
    /// <summary>
    /// Creates a new instance of <see cref="FileStoreQueryContext"/>.
    /// </summary>
    public FileStoreQueryContext(QueryContextDependencies dependencies)
        : base(dependencies)
    {
    }
}

