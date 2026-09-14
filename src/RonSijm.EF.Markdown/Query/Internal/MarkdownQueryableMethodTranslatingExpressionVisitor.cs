// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Expression visitor for translating queryable methods for Markdown provider.
/// This class intentionally returns null for most Translate* methods because
/// the Markdown provider does not support server-side LINQ operations.
/// All LINQ operations are performed client-side after ToList().
/// </summary>
[ExcludeFromCodeCoverage]
public class MarkdownQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownQueryableMethodTranslatingExpressionVisitor"/>.
    /// </summary>
    public MarkdownQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
    }

    /// <summary>
    /// Creates a new instance for subqueries.
    /// </summary>
    protected MarkdownQueryableMethodTranslatingExpressionVisitor(
        MarkdownQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
    {
    }

    /// <inheritdoc />
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new MarkdownQueryableMethodTranslatingExpressionVisitor(this);

    /// <inheritdoc />
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
    {
        var queryExpression = new MarkdownQueryExpression(entityType);

        return new ShapedQueryExpression(
            queryExpression,
            new StructuralTypeShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    queryExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCast(ShapedQueryExpression source, Type resultType)
    {
        return source.ShaperExpression.Type == resultType ? source : null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateConcat(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(
        ShapedQueryExpression source,
        Expression? defaultValue)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateExcept(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateIntersect(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(
        ShapedQueryExpression source,
        LambdaExpression? predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOrderBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member
    protected override ShapedQueryExpression? TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
#pragma warning restore CS8764
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression selector)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateThenBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateUnion(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2)
    {
        return null; // Fall back to client evaluation
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        return null; // Fall back to client evaluation
    }
}
