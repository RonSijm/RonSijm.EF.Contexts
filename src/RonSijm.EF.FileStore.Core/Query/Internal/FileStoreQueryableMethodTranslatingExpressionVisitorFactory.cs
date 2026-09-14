// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.FileStore.Query.Internal;

/// <summary>
/// Factory for creating <see cref="FileStoreQueryableMethodTranslatingExpressionVisitor"/> instances.
/// </summary>
public class FileStoreQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="FileStoreQueryableMethodTranslatingExpressionVisitorFactory"/>.
    /// </summary>
    public FileStoreQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new FileStoreQueryableMethodTranslatingExpressionVisitor(_dependencies, queryCompilationContext);
}

