// Licensed under the MIT license.

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.EF.Markdown.Infrastructure.Internal;

/// <summary>
/// Options extension for the Markdown database provider.
/// </summary>
public class MarkdownOptionsExtension : IDbContextOptionsExtension
{
    private string _directoryPath = string.Empty;
    private string _fileExtension = ".md";
    private bool _createDirectoryIfNotExists = true;
    private bool _enforceForeignKeys;
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownOptionsExtension"/>.
    /// </summary>
    public MarkdownOptionsExtension()
    {
    }

    /// <summary>
    /// Creates a new instance by copying from an existing instance.
    /// </summary>
    protected MarkdownOptionsExtension(MarkdownOptionsExtension copyFrom)
    {
        _directoryPath = copyFrom._directoryPath;
        _fileExtension = copyFrom._fileExtension;
        _createDirectoryIfNotExists = copyFrom._createDirectoryIfNotExists;
        _enforceForeignKeys = copyFrom._enforceForeignKeys;
    }

    /// <summary>
    /// Gets the directory path where markdown files are stored.
    /// </summary>
    public virtual string DirectoryPath => _directoryPath;

    /// <summary>
    /// Gets the file extension for markdown files.
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

    /// <inheritdoc />
    public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    /// <summary>
    /// Creates a new instance with the specified directory path.
    /// </summary>
    public virtual MarkdownOptionsExtension WithDirectoryPath(string directoryPath)
    {
        var clone = Clone();
        clone._directoryPath = directoryPath;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified file extension.
    /// </summary>
    public virtual MarkdownOptionsExtension WithFileExtension(string fileExtension)
    {
        var clone = Clone();
        clone._fileExtension = fileExtension;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified create directory setting.
    /// </summary>
    public virtual MarkdownOptionsExtension WithCreateDirectoryIfNotExists(bool create)
    {
        var clone = Clone();
        clone._createDirectoryIfNotExists = create;
        return clone;
    }

    /// <summary>
    /// Creates a new instance with the specified foreign key enforcement setting.
    /// </summary>
    public virtual MarkdownOptionsExtension WithEnforceForeignKeys(bool enforce)
    {
        var clone = Clone();
        clone._enforceForeignKeys = enforce;
        return clone;
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    protected virtual MarkdownOptionsExtension Clone() => new(this);

    /// <inheritdoc />
    public virtual void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkMarkdown();
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        if (string.IsNullOrWhiteSpace(_directoryPath))
        {
            throw new InvalidOperationException("The directory path must be specified for the Markdown provider.");
        }
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(MarkdownOptionsExtension extension)
            : base(extension)
        {
        }

        private new MarkdownOptionsExtension Extension => (MarkdownOptionsExtension)base.Extension;

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
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
               && Extension._directoryPath == otherInfo.Extension._directoryPath
               && Extension._fileExtension == otherInfo.Extension._fileExtension
               && Extension._enforceForeignKeys == otherInfo.Extension._enforceForeignKeys;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Markdown:DirectoryPath"] = Extension._directoryPath;
            debugInfo["Markdown:FileExtension"] = Extension._fileExtension;
            debugInfo["Markdown:EnforceForeignKeys"] = Extension._enforceForeignKeys.ToString();
        }
    }
}

