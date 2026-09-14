// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Factory for creating Markdown shaped query compiling expression visitors.
/// </summary>
public class MarkdownShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownShapedQueryCompilingExpressionVisitorFactory"/>.
    /// </summary>
    public MarkdownShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new MarkdownShapedQueryCompilingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

