﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Json.Query.Internal;

/// <summary>
/// Query context for JSON queries.
/// </summary>
public class JsonQueryContext : QueryContext
{
    /// <summary>
    /// Creates a new instance of <see cref="JsonQueryContext"/>.
    /// </summary>
    public JsonQueryContext(QueryContextDependencies dependencies)
        : base(dependencies)
    {
    }
}

