// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Represents the CSV store that manages entity data.
/// Inherits generic functionality from FileStoreBase.
/// </summary>
public class CsvStore : FileStoreBase, ICsvStore
{
    /// <summary>
    /// Creates a new instance of <see cref="CsvStore"/>.
    /// </summary>
    public CsvStore(string directoryPath, string fileExtension)
        : base(directoryPath, fileExtension)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="CsvStore"/> with foreign key enforcement option.
    /// </summary>
    public CsvStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
        : base(directoryPath, fileExtension, enforceForeignKeys)
    {
    }

    /// <inheritdoc />
    public new ICsvTable GetTable(IEntityType entityType)
    {
        return (ICsvTable)base.GetTable(entityType);
    }

    /// <inheritdoc />
    protected override IFileTable CreateTable(IEntityType entityType, string directoryPath, string fileExtension)
    {
        return new CsvTable(entityType, directoryPath, fileExtension);
    }
}

