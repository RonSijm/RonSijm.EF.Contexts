﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Json.Query.Internal;

/// <summary>
/// Factory for creating JSON queryable method translating expression visitors.
/// </summary>
public class JsonQueryableMethodTranslatingExpressionVisitorFactory 
    : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="JsonQueryableMethodTranslatingExpressionVisitorFactory"/>.
    /// </summary>
    public JsonQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new JsonQueryableMethodTranslatingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

