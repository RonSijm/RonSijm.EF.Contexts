// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Factory for creating CSV shaped query compiling expression visitors.
/// </summary>
public class CsvShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="CsvShapedQueryCompilingExpressionVisitorFactory"/>.
    /// </summary>
    public CsvShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new CsvShapedQueryCompilingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

