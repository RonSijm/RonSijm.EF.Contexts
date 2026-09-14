// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Markdown.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// The Markdown database implementation.
/// Inherits generic functionality from FileDatabaseBase.
/// </summary>
public class MarkdownDatabase : FileDatabaseBase, IMarkdownDatabase
{
    private readonly IMarkdownStore _markdownStore;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownDatabase"/>.
    /// </summary>
    public MarkdownDatabase(
        DatabaseDependencies dependencies,
        IMarkdownStoreCache storeCache,
        IDbContextOptions options)
        : base(dependencies, GetStore(storeCache, options))
    {
        _markdownStore = (IMarkdownStore)base.Store;
    }

    private static IFileStore GetStore(IMarkdownStoreCache storeCache, IDbContextOptions options)
    {
        var markdownOptions = options.FindExtension<MarkdownOptionsExtension>()
            ?? throw new InvalidOperationException("Markdown options extension not found.");

        return storeCache.GetStore(
            markdownOptions.DirectoryPath,
            markdownOptions.FileExtension,
            markdownOptions.EnforceForeignKeys);
    }

    /// <inheritdoc />
    public new IMarkdownStore Store => _markdownStore;
}

