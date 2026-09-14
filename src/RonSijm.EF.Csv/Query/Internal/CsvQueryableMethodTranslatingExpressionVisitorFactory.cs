// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Factory for creating CSV queryable method translating expression visitors.
/// </summary>
public class CsvQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="CsvQueryableMethodTranslatingExpressionVisitorFactory"/>.
    /// </summary>
    public CsvQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new CsvQueryableMethodTranslatingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

