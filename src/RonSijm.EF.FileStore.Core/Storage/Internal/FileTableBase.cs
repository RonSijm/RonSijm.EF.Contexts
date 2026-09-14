// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Abstract base class for file-based tables that stores entity data.
/// Provides common functionality for row management, indexing, and type parsing.
/// </summary>
public abstract class FileTableBase : IFileTable
{
    private readonly string _filePath;
    private readonly List<object[]> _rows = new();
    private readonly Dictionary<CompositeKey, object[]> _rowIndex = new();
    
    /// <summary>
    /// The properties of the entity type.
    /// </summary>
    protected readonly IProperty[] Properties;
    
    /// <summary>
    /// The indexes of the key properties in the Properties array.
    /// </summary>
    protected readonly int[] KeyPropertyIndexes;
    
    /// <summary>
    /// Cached type parsers for each property.
    /// </summary>
    protected readonly Func<string, object>[] TypeParsers;

    // Cache for type parsers across all tables
    private static readonly ConcurrentDictionary<Type, Func<string, object>> TypeParserCache = new();

    // Threshold for using memory-mapped files (1 MB)
    private const long MemoryMappedFileThreshold = 1024 * 1024;

    /// <summary>
    /// Creates a new instance of <see cref="FileTableBase"/>.
    /// </summary>
    protected FileTableBase(IEntityType entityType, string directoryPath, string fileExtension)
    {
        EntityType = entityType;
        _filePath = Path.Combine(directoryPath, $"{GetTableName(entityType)}{fileExtension}");
        Properties = entityType.GetProperties().ToArray();

        var keyProperties = entityType.FindPrimaryKey()?.Properties ?? [];
        KeyPropertyIndexes = keyProperties.Select(p => Array.IndexOf(Properties, p)).ToArray();

        // Pre-create type parsers for each property
        TypeParsers = new Func<string, object>[Properties.Length];
        for (var i = 0; i < Properties.Length; i++)
        {
            TypeParsers[i] = GetOrCreateTypeParser(Properties[i]);
        }
    }

    /// <summary>
    /// Gets the file path for this table.
    /// </summary>
    protected string FilePath => _filePath;

    /// <inheritdoc />
    public IEntityType EntityType { get; }

    /// <inheritdoc />
    public IReadOnlyList<object[]> Rows => _rows;

    /// <summary>
    /// Gets the mutable rows list for derived classes.
    /// </summary>
    protected List<object[]> MutableRows => _rows;

    /// <summary>
    /// Gets the row index for derived classes.
    /// </summary>
    protected Dictionary<CompositeKey, object[]> RowIndex => _rowIndex;

    /// <inheritdoc />
    public void Create(IUpdateEntry entry)
    {
        var row = new object[Properties.Length];
        for (var i = 0; i < Properties.Length; i++)
        {
            row[i] = entry.GetCurrentValue(Properties[i])!;
        }
        _rows.Add(row);

        // Add to index for O(1) lookups
        if (KeyPropertyIndexes.Length > 0)
        {
            var key = GetKeyFromRow(row);
            _rowIndex[key] = row;
        }
    }

    /// <inheritdoc />
    public void Update(IUpdateEntry entry)
    {
        var keyValues = GetKeyValues(entry);
        var existingRow = FindRow(keyValues);
        if (existingRow != null)
        {
            for (var i = 0; i < Properties.Length; i++)
            {
                if (entry.IsModified(Properties[i]))
                {
                    existingRow[i] = entry.GetCurrentValue(Properties[i])!;
                }
            }
        }
    }

    /// <inheritdoc />
    public void Delete(IUpdateEntry entry)
    {
        var keyValues = GetKeyValues(entry);
        var existingRow = FindRow(keyValues);
        if (existingRow != null)
        {
            _rows.Remove(existingRow);

            // Remove from index
            if (KeyPropertyIndexes.Length > 0)
            {
                var key = new CompositeKey(keyValues);
                _rowIndex.Remove(key);
            }
        }
    }

    /// <inheritdoc />
    public void Load()
    {
        _rows.Clear();
        _rowIndex.Clear();
        if (!File.Exists(_filePath))
        {
            return;
        }

        var fileInfo = new FileInfo(_filePath);

        // Use memory-mapped files for large files, streaming for smaller ones
        if (fileInfo.Length >= MemoryMappedFileThreshold)
        {
            LoadWithMemoryMappedFile(fileInfo);
        }
        else
        {
            LoadWithStreaming(fileInfo);
        }
    }

    /// <inheritdoc />
    public abstract void Save();

    /// <inheritdoc />
    public object[]? FindRow(object[] keyValues)
    {
        // Use O(1) dictionary lookup if we have a primary key index
        if (KeyPropertyIndexes.Length > 0)
        {
            var key = new CompositeKey(keyValues);
            return _rowIndex.TryGetValue(key, out var row) ? row : null;
        }

        // Fallback to O(n) linear search if no primary key
        foreach (var row in _rows)
        {
            var match = true;
            for (var i = 0; i < KeyPropertyIndexes.Length; i++)
            {
                var rowKeyValue = row[KeyPropertyIndexes[i]];
                if (!Equals(rowKeyValue, keyValues[i]))
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                return row;
            }
        }
        return null;
    }

    /// <summary>
    /// Loads data using memory-mapped files for very large files.
    /// </summary>
    private void LoadWithMemoryMappedFile(FileInfo fileInfo)
    {
        var estimatedRows = Math.Max(16, (int)(fileInfo.Length / 50));
        _rows.Capacity = estimatedRows;

        using var mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var accessor = mmf.CreateViewAccessor(0, fileInfo.Length, MemoryMappedFileAccess.Read);

        var bytes = new byte[fileInfo.Length];
        accessor.ReadArray(0, bytes, 0, bytes.Length);

        var content = Encoding.UTF8.GetString(bytes);
        ParseContent(content.AsSpan());
    }

    /// <summary>
    /// Loads data using streaming for smaller files.
    /// </summary>
    private void LoadWithStreaming(FileInfo fileInfo)
    {
        var estimatedRows = Math.Max(16, (int)(fileInfo.Length / 50));
        _rows.Capacity = estimatedRows;

        ParseFileStreaming(_filePath);
    }

    /// <summary>
    /// Parses file content from a span. Override in derived classes for format-specific parsing.
    /// </summary>
    protected abstract void ParseContent(ReadOnlySpan<char> content);

    /// <summary>
    /// Parses file using streaming reads. Override in derived classes for format-specific parsing.
    /// </summary>
    protected abstract void ParseFileStreaming(string filePath);

    /// <summary>
    /// Adds a row to the table and updates the index.
    /// </summary>
    protected void AddRow(object[] row)
    {
        _rows.Add(row);
        if (KeyPropertyIndexes.Length > 0)
        {
            var key = GetKeyFromRow(row);
            _rowIndex[key] = row;
        }
    }

    /// <summary>
    /// Extracts key values from a row for indexing.
    /// </summary>
    protected CompositeKey GetKeyFromRow(object[] row)
    {
        var keyValues = new object[KeyPropertyIndexes.Length];
        for (var i = 0; i < KeyPropertyIndexes.Length; i++)
        {
            keyValues[i] = row[KeyPropertyIndexes[i]];
        }
        return new CompositeKey(keyValues);
    }

    private object[] GetKeyValues(IUpdateEntry entry)
    {
        var keyProperties = EntityType.FindPrimaryKey()?.Properties ?? [];
        return keyProperties.Select(p => entry.GetCurrentValue(p)!).ToArray();
    }

    /// <summary>
    /// Parses a string value to the appropriate type for the given property index.
    /// </summary>
    protected object ParseValue(string value, int propertyIndex)
    {
        if (value == "(null)" || string.IsNullOrEmpty(value))
        {
            var property = Properties[propertyIndex];
            if (Nullable.GetUnderlyingType(property.ClrType) != null)
            {
                return null!;
            }
            if (property.ClrType.IsValueType)
            {
                return Activator.CreateInstance(property.ClrType)!;
            }
            return null!;
        }

        return TypeParsers[propertyIndex](value);
    }

    /// <summary>
    /// Gets or creates a cached type parser for the property.
    /// </summary>
    private static Func<string, object> GetOrCreateTypeParser(IProperty property)
    {
        var targetType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        return TypeParserCache.GetOrAdd(targetType, type =>
        {
            if (type == typeof(string))
                return static s => s;
            if (type == typeof(int))
                return static s => int.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(long))
                return static s => long.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(short))
                return static s => short.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(byte))
                return static s => byte.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(decimal))
                return static s => decimal.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(double))
                return static s => double.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(float))
                return static s => float.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(bool))
                return static s => bool.Parse(s);
            if (type == typeof(DateTime))
                return static s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (type == typeof(DateTimeOffset))
                return static s => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (type == typeof(DateOnly))
                return static s => DateOnly.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(TimeOnly))
                return static s => TimeOnly.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(TimeSpan))
                return static s => TimeSpan.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(Guid))
                return static s => Guid.Parse(s);
            if (type.IsEnum)
                return s => Enum.Parse(type, s);

            return s => Convert.ChangeType(s, type, CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// Formats a value to a string for writing to file.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void FormatValueToWriter(object? value, IProperty property, StreamWriter writer)
    {
        if (value == null)
        {
            writer.Write("(null)");
            return;
        }

        switch (value)
        {
            case DateTime dt:
                writer.Write(dt.ToString("O", CultureInfo.InvariantCulture));
                break;
            case DateTimeOffset dto:
                writer.Write(dto.ToString("O", CultureInfo.InvariantCulture));
                break;
            case bool b:
                writer.Write(b ? "true" : "false");
                break;
            case string s:
                writer.Write(s);
                break;
            case int i:
                writer.Write(i);
                break;
            case long l:
                writer.Write(l);
                break;
            case double d:
                writer.Write(d.ToString(CultureInfo.InvariantCulture));
                break;
            case decimal dec:
                writer.Write(dec.ToString(CultureInfo.InvariantCulture));
                break;
            case Guid g:
                writer.Write(g.ToString());
                break;
            default:
                writer.Write(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                break;
        }
    }

    /// <summary>
    /// Gets the table name from the entity type.
    /// </summary>
    protected static string GetTableName(IEntityType entityType)
    {
        return entityType.ClrType.Name;
    }
}

