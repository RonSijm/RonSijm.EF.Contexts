// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Markdown.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Markdown specific extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class MarkdownDbContextOptionsExtensions
{
    /// <summary>
    /// Configures the context to use the Markdown database provider, storing entities as markdown tables.
    /// </summary>
    /// <typeparam name="TContext">The type of context being configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where markdown files will be stored.</param>
    /// <param name="markdownOptionsAction">An optional action to allow additional Markdown specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseMarkdown<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string directoryPath,
        Action<MarkdownDbContextOptionsBuilder>? markdownOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseMarkdown(
            (DbContextOptionsBuilder)optionsBuilder, directoryPath, markdownOptionsAction);

    /// <summary>
    /// Configures the context to use the Markdown database provider, storing entities as markdown tables.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where markdown files will be stored.</param>
    /// <param name="markdownOptionsAction">An optional action to allow additional Markdown specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseMarkdown(
        this DbContextOptionsBuilder optionsBuilder,
        string directoryPath,
        Action<MarkdownDbContextOptionsBuilder>? markdownOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>()
            ?? new MarkdownOptionsExtension();

        extension = extension.WithDirectoryPath(directoryPath);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        markdownOptionsAction?.Invoke(new MarkdownDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
}

