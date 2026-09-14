// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// Represents the Markdown store that manages entity data.
/// Inherits generic functionality from FileStoreBase.
/// </summary>
public class MarkdownStore : FileStoreBase, IMarkdownStore
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownStore"/>.
    /// </summary>
    public MarkdownStore(string directoryPath, string fileExtension)
        : base(directoryPath, fileExtension)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownStore"/> with foreign key enforcement option.
    /// </summary>
    public MarkdownStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
        : base(directoryPath, fileExtension, enforceForeignKeys)
    {
    }

    /// <inheritdoc />
    public new IMarkdownTable GetTable(IEntityType entityType)
    {
        return (IMarkdownTable)base.GetTable(entityType);
    }

    /// <inheritdoc />
    protected override IFileTable CreateTable(IEntityType entityType, string directoryPath, string fileExtension)
    {
        return new MarkdownTable(entityType, directoryPath, fileExtension);
    }
}

