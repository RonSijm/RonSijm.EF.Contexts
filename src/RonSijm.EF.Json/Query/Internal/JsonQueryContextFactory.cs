﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Json.Query.Internal;

/// <summary>
/// Factory for creating JSON query contexts.
/// </summary>
public class JsonQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="JsonQueryContextFactory"/>.
    /// </summary>
    public JsonQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryContext Create()
    {
        return new JsonQueryContext(_dependencies);
    }
}

