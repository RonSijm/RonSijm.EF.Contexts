// Licensed under the MIT license.

using RonSijm.EF.Markdown.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Allows Markdown specific configuration to be performed on <see cref="DbContextOptions"/>.
/// </summary>
public class MarkdownDbContextOptionsBuilder
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownDbContextOptionsBuilder"/>.
    /// </summary>
    public MarkdownDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    /// <summary>
    /// Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    /// <summary>
    /// Sets the file extension for markdown files (default is ".md").
    /// </summary>
    /// <param name="extension">The file extension to use.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual MarkdownDbContextOptionsBuilder UseFileExtension(string extension)
    {
        var markdownExtension = OptionsBuilder.Options.FindExtension<MarkdownOptionsExtension>()
            ?? new MarkdownOptionsExtension();

        markdownExtension = markdownExtension.WithFileExtension(extension);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(markdownExtension);

        return this;
    }

    /// <summary>
    /// Configures whether to create the directory if it doesn't exist.
    /// </summary>
    /// <param name="create">Whether to create the directory.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual MarkdownDbContextOptionsBuilder CreateDirectoryIfNotExists(bool create = true)
    {
        var markdownExtension = OptionsBuilder.Options.FindExtension<MarkdownOptionsExtension>()
            ?? new MarkdownOptionsExtension();

        markdownExtension = markdownExtension.WithCreateDirectoryIfNotExists(create);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(markdownExtension);

        return this;
    }

    /// <summary>
    /// Configures whether to enforce foreign key constraints.
    /// When enabled, insert/update/delete operations will validate referential integrity.
    /// </summary>
    /// <param name="enforce">Whether to enforce foreign key constraints. Default is true when this method is called.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual MarkdownDbContextOptionsBuilder EnforceForeignKeys(bool enforce = true)
    {
        var markdownExtension = OptionsBuilder.Options.FindExtension<MarkdownOptionsExtension>()
            ?? new MarkdownOptionsExtension();

        markdownExtension = markdownExtension.WithEnforceForeignKeys(enforce);

        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(markdownExtension);

        return this;
    }
}

