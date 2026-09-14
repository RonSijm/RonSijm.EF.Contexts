// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Query context for the CSV provider.
/// </summary>
public class CsvQueryContext : QueryContext
{
    /// <summary>
    /// Creates a new instance of <see cref="CsvQueryContext"/>.
    /// </summary>
    public CsvQueryContext(QueryContextDependencies dependencies)
        : base(dependencies)
    {
    }
}

