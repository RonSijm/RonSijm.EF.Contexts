// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Factory for creating Markdown query contexts.
/// </summary>
public class MarkdownQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownQueryContextFactory"/>.
    /// </summary>
    public MarkdownQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryContext Create()
    {
        return new MarkdownQueryContext(_dependencies);
    }
}

