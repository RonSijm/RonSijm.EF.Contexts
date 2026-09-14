// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Factory for creating Markdown queryable method translating expression visitors.
/// </summary>
public class MarkdownQueryableMethodTranslatingExpressionVisitorFactory 
    : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownQueryableMethodTranslatingExpressionVisitorFactory"/>.
    /// </summary>
    public MarkdownQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new MarkdownQueryableMethodTranslatingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

