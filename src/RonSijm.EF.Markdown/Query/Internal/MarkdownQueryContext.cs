// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Query context for Markdown queries.
/// </summary>
public class MarkdownQueryContext : QueryContext
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownQueryContext"/>.
    /// </summary>
    public MarkdownQueryContext(QueryContextDependencies dependencies)
        : base(dependencies)
    {
    }
}

