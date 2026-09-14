// Licensed under the MIT license.

using System.Text.Json;
using RonSijm.EF.FileStore.Storage.Internal;
using RonSijm.EF.Json.Infrastructure.Internal;
using RonSijm.EF.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Represents the JSON store that manages entity data.
/// Inherits generic functionality from FileStoreBase.
/// </summary>
public class JsonStore : FileStoreBase, IJsonStore
{
    private readonly bool _useIndentation;
    private readonly JsonNamingPolicy? _propertyNamingPolicy;
    private readonly JsonReferenceMode _referenceMode;

    /// <summary>
    /// Creates a new instance of <see cref="JsonStore"/>.
    /// </summary>
    public JsonStore(string directoryPath, string fileExtension)
        : base(directoryPath, fileExtension)
    {
        _useIndentation = true;
        _propertyNamingPolicy = null;
        _referenceMode = JsonReferenceMode.Flat;
    }

    /// <summary>
    /// Creates a new instance of <see cref="JsonStore"/> with foreign key enforcement option.
    /// </summary>
    public JsonStore(string directoryPath, string fileExtension, bool enforceForeignKeys)
        : base(directoryPath, fileExtension, enforceForeignKeys)
    {
        _useIndentation = true;
        _propertyNamingPolicy = null;
        _referenceMode = JsonReferenceMode.Flat;
    }

    /// <summary>
    /// Creates a new instance of <see cref="JsonStore"/> with full options.
    /// </summary>
    public JsonStore(JsonOptionsExtension options)
        : base(options.DirectoryPath, options.FileExtension, options.EnforceForeignKeys)
    {
        _useIndentation = options.UseIndentation;
        _propertyNamingPolicy = options.PropertyNamingPolicy;
        _referenceMode = options.ReferenceMode;
    }

    /// <inheritdoc />
    public new IJsonTable GetTable(IEntityType entityType)
    {
        return (IJsonTable)base.GetTable(entityType);
    }

    /// <inheritdoc />
    protected override IFileTable CreateTable(IEntityType entityType, string directoryPath, string fileExtension)
    {
        return new JsonTable(
            entityType,
            directoryPath,
            fileExtension,
            _useIndentation,
            _propertyNamingPolicy,
            _referenceMode);
    }
}

