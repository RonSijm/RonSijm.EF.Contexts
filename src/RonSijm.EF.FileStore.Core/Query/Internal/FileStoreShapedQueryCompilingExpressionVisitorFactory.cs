// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.FileStore.Query.Internal;

/// <summary>
/// Factory for creating <see cref="FileStoreShapedQueryCompilingExpressionVisitor"/> instances.
/// </summary>
public class FileStoreShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="FileStoreShapedQueryCompilingExpressionVisitorFactory"/>.
    /// </summary>
    public FileStoreShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new FileStoreShapedQueryCompilingExpressionVisitor(_dependencies, queryCompilationContext);
}

