// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RonSijm.EF.Markdown.Infrastructure.Internal;

/// <summary>
/// Singleton options implementation for the Markdown database provider.
/// </summary>
public class MarkdownSingletonOptions : IMarkdownSingletonOptions
{
    /// <inheritdoc />
    public string? DirectoryPath { get; private set; }

    /// <inheritdoc />
    public bool CreateDirectoryIfNotExists { get; private set; }

    /// <inheritdoc />
    public bool EnforceForeignKeys { get; private set; }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var markdownOptions = options.FindExtension<MarkdownOptionsExtension>();
        if (markdownOptions != null)
        {
            DirectoryPath = markdownOptions.DirectoryPath;
            CreateDirectoryIfNotExists = markdownOptions.CreateDirectoryIfNotExists;
            EnforceForeignKeys = markdownOptions.EnforceForeignKeys;
        }
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var markdownOptions = options.FindExtension<MarkdownOptionsExtension>();
        if (markdownOptions != null)
        {
            // Validate options
        }
    }
}

