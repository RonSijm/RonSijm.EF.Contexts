﻿// Licensed under the MIT license.

using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.EF.Json.Infrastructure.Internal;

/// <summary>
/// Options extension for the JSON database provider.
/// </summary>
public class JsonOptionsExtension : IDbContextOptionsExtension
{
    private string _directoryPath = string.Empty;
    private string _fileExtension = ".json";
    private bool _createDirectoryIfNotExists = true;
    private bool _enforceForeignKeys;
    private JsonStorageMode _storageMode = JsonStorageMode.MultipleFiles;
    private JsonReferenceMode _referenceMode = JsonReferenceMode.Flat;
    private string? _singleFileName;
    private bool _useIndentation = true;
    private JsonNamingPolicy? _propertyNamingPolicy;
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    /// Creates a new instance of <see cref="JsonOptionsExtension"/>.
    /// </summary>
    public JsonOptionsExtension()
    {
    }

    /// <summary>
    /// Creates a new instance by copying from an existing instance.
    /// </summary>
    protected JsonOptionsExtension(JsonOptionsExtension copyFrom)
    {
        _directoryPath = copyFrom._directoryPath;
        _fileExtension = copyFrom._fileExtension;
        _createDirectoryIfNotExists = copyFrom._createDirectoryIfNotExists;
        _enforceForeignKeys = copyFrom._enforceForeignKeys;
        _storageMode = copyFrom._storageMode;
        _referenceMode = copyFrom._referenceMode;
        _singleFileName = copyFrom._singleFileName;
        _useIndentation = copyFrom._useIndentation;
        _propertyNamingPolicy = copyFrom._propertyNamingPolicy;
    }

    /// <summary>
    /// Gets the directory path where JSON files are stored.
    /// </summary>
    public virtual string DirectoryPath => _directoryPath;

    /// <summary>
    /// Gets the file extension for JSON files.
    /// </summary>
    public virtual string FileExtension => _fileExtension;

    /// <summary>
    /// Gets whether to create the directory if it doesn't exist.
    /// </summary>
    public virtual bool CreateDirectoryIfNotExists => _createDirectoryIfNotExists;

    /// <summary>
    /// Gets whether to enforce foreign key constraints.
    /// </summary>
    public virtual bool EnforceForeignKeys => _enforceForeignKeys;

    /// <summary>
    /// Gets the storage mode.
    /// </summary>
    public virtual JsonStorageMode StorageMode => _storageMode;

    /// <summary>
    /// Gets the reference mode.
    /// </summary>
    public virtual JsonReferenceMode ReferenceMode => _referenceMode;

    /// <summary>
    /// Gets the single file name (when using SingleFile storage mode).
    /// </summary>
    public virtual string? SingleFileName => _singleFileName;

    /// <summary>
    /// Gets whether to use indentation (pretty print).
    /// </summary>
    public virtual bool UseIndentation => _useIndentation;

    /// <summary>
    /// Gets the property naming policy.
    /// </summary>
    public virtual JsonNamingPolicy? PropertyNamingPolicy => _propertyNamingPolicy;

    /// <inheritdoc />
    public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    /// <summary>
    /// Creates a new instance with the specified directory path.
    /// </summary>
    public virtual JsonOptionsExtension WithDirectoryPath(string directoryPath)
    {
        var clone = Clone();
        clone._directoryPath = directoryPath;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified file extension.
    /// </summary>
    public virtual JsonOptionsExtension WithFileExtension(string fileExtension)
    {
        var clone = Clone();
        clone._fileExtension = fileExtension;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified create directory setting.
    /// </summary>
    public virtual JsonOptionsExtension WithCreateDirectoryIfNotExists(bool create)
    {
        var clone = Clone();
        clone._createDirectoryIfNotExists = create;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified foreign key enforcement setting.
    /// </summary>
    public virtual JsonOptionsExtension WithEnforceForeignKeys(bool enforce)
    {
        var clone = Clone();
        clone._enforceForeignKeys = enforce;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified storage mode.
    /// </summary>
    public virtual JsonOptionsExtension WithStorageMode(JsonStorageMode storageMode)
    {
        var clone = Clone();
        clone._storageMode = storageMode;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified reference mode.
    /// </summary>
    public virtual JsonOptionsExtension WithReferenceMode(JsonReferenceMode referenceMode)
    {
        var clone = Clone();
        clone._referenceMode = referenceMode;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified single file name.
    /// </summary>
    public virtual JsonOptionsExtension WithSingleFileName(string? fileName)
    {
        var clone = Clone();
        clone._singleFileName = fileName;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified indentation setting.
    /// </summary>
    public virtual JsonOptionsExtension WithIndentation(bool useIndentation)
    {
        var clone = Clone();
        clone._useIndentation = useIndentation;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified property naming policy.
    /// </summary>
    public virtual JsonOptionsExtension WithPropertyNamingPolicy(JsonNamingPolicy? namingPolicy)
    {
        var clone = Clone();
        clone._propertyNamingPolicy = namingPolicy;
        return clone;
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    protected virtual JsonOptionsExtension Clone() => new(this);

    /// <inheritdoc />
    public virtual void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkJson();
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        if (string.IsNullOrWhiteSpace(_directoryPath))
        {
            throw new InvalidOperationException("The directory path must be specified for the JSON provider.");
        }

        if (_storageMode == JsonStorageMode.SingleFile && string.IsNullOrWhiteSpace(_singleFileName))
        {
            throw new InvalidOperationException("A file name must be specified when using SingleFile storage mode.");
        }
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(JsonOptionsExtension extension)
            : base(extension)
        {
        }

        private new JsonOptionsExtension Extension => (JsonOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => true;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();
                    builder.Append("DirectoryPath=").Append(Extension._directoryPath);
                    _logFragment = builder.ToString();
                }
                return _logFragment;
            }
        }

        public override int GetServiceProviderHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Extension._directoryPath);
            hashCode.Add(Extension._fileExtension);
            hashCode.Add(Extension._enforceForeignKeys);
            hashCode.Add(Extension._storageMode);
            hashCode.Add(Extension._referenceMode);
            hashCode.Add(Extension._singleFileName);
            hashCode.Add(Extension._useIndentation);
            hashCode.Add(Extension._propertyNamingPolicy?.GetType().FullName);
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
               && Extension._directoryPath == otherInfo.Extension._directoryPath
               && Extension._fileExtension == otherInfo.Extension._fileExtension
               && Extension._enforceForeignKeys == otherInfo.Extension._enforceForeignKeys
               && Extension._storageMode == otherInfo.Extension._storageMode
               && Extension._referenceMode == otherInfo.Extension._referenceMode
               && Extension._singleFileName == otherInfo.Extension._singleFileName
               && Extension._useIndentation == otherInfo.Extension._useIndentation
               && Extension._propertyNamingPolicy?.GetType() == otherInfo.Extension._propertyNamingPolicy?.GetType();

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Json:DirectoryPath"] = Extension._directoryPath;
            debugInfo["Json:FileExtension"] = Extension._fileExtension;
            debugInfo["Json:EnforceForeignKeys"] = Extension._enforceForeignKeys.ToString();
            debugInfo["Json:StorageMode"] = Extension._storageMode.ToString();
            debugInfo["Json:ReferenceMode"] = Extension._referenceMode.ToString();
            debugInfo["Json:SingleFileName"] = Extension._singleFileName ?? "(none)";
            debugInfo["Json:UseIndentation"] = Extension._useIndentation.ToString();
            debugInfo["Json:PropertyNamingPolicy"] = Extension._propertyNamingPolicy?.GetType().Name ?? "(none)";
        }
    }
}

