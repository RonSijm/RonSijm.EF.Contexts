﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Json.Query.Internal;

/// <summary>
/// Factory for creating JSON shaped query compiling expression visitors.
/// </summary>
public class JsonShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="JsonShapedQueryCompilingExpressionVisitorFactory"/>.
    /// </summary>
    public JsonShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new JsonShapedQueryCompilingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}

