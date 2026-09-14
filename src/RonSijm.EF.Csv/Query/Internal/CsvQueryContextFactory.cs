// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Factory for creating CSV query contexts.
/// </summary>
public class CsvQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="CsvQueryContextFactory"/>.
    /// </summary>
    public CsvQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryContext Create()
    {
        return new CsvQueryContext(_dependencies);
    }
}

