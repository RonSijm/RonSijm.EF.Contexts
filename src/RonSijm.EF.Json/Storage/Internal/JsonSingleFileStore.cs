// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RonSijm.EF.FileStore.ForeignKeys;
using RonSijm.EF.FileStore.Storage.Internal;
using RonSijm.EF.Json.Infrastructure.Internal;
using RonSijm.EF.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// A JSON store that stores all entity types in a single JSON file with lazy loading.
/// </summary>
public class JsonSingleFileStore : IJsonStore
{
    private readonly string _directoryPath;
    private readonly string _filePath;
    private readonly bool _useIndentation;
    private readonly JsonNamingPolicy? _propertyNamingPolicy;
    private readonly JsonReferenceMode _referenceMode;
    private readonly bool _enforceForeignKeys;
    private readonly ForeignKeyValidator? _foreignKeyValidator;
    private readonly ConcurrentDictionary<string, JsonSingleFileTable> _tables = new();
    private readonly object _lock = new();
    private readonly HashSet<string> _loadedEntityTypes = new();
    private JsonDocument? _cachedDocument;
    private bool _isDirty;

    /// <summary>
    /// Creates a new instance of <see cref="JsonSingleFileStore"/>.
    /// </summary>
    public JsonSingleFileStore(JsonOptionsExtension options)
    {
        _directoryPath = options.DirectoryPath;
        _filePath = Path.Combine(options.DirectoryPath, options.SingleFileName ?? "database.json");
        _useIndentation = options.UseIndentation;
        _propertyNamingPolicy = options.PropertyNamingPolicy;
        _referenceMode = options.ReferenceMode;
        _enforceForeignKeys = options.EnforceForeignKeys;
        _foreignKeyValidator = _enforceForeignKeys ? new ForeignKeyValidator() : null;
    }

    /// <inheritdoc />
    public string DirectoryPath => _directoryPath;

    /// <inheritdoc />
    public bool EnsureCreated(IModel model)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Initialize tables for all entity types
        foreach (var entityType in model.GetEntityTypes())
        {
            GetTable(entityType);
        }

        // If file doesn't exist, create an empty one
        if (!File.Exists(_filePath))
        {
            SaveAll();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool EnsureDeleted()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
            _tables.Clear();
            _loadedEntityTypes.Clear();
            _cachedDocument?.Dispose();
            _cachedDocument = null;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public IJsonTable GetTable(IEntityType entityType)
    {
        var tableName = entityType.ClrType.Name;
        
        return _tables.GetOrAdd(tableName, _ =>
        {
            var table = new JsonSingleFileTable(
                entityType,
                tableName,
                _useIndentation,
                _propertyNamingPolicy,
                _referenceMode);
            
            // Lazy load data for this entity type
            LazyLoadEntityType(tableName, table);
            
            return table;
        });
    }

    IFileTable IFileStore.GetTable(IEntityType entityType) => GetTable(entityType);

    private void LazyLoadEntityType(string tableName, JsonSingleFileTable table)
    {
        lock (_lock)
        {
            if (_loadedEntityTypes.Contains(tableName))
            {
                return;
            }

            if (!File.Exists(_filePath))
            {
                _loadedEntityTypes.Add(tableName);
                return;
            }

            // Load or use cached document
            if (_cachedDocument == null)
            {
                var json = File.ReadAllText(_filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedDocument = JsonDocument.Parse(json);
                }
            }

            if (_cachedDocument != null)
            {
                var root = _cachedDocument.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(tableName, out var entityArray))
                {
                    table.LoadFromJsonElement(entityArray);
                }
            }

            _loadedEntityTypes.Add(tableName);
        }
    }

    /// <inheritdoc />
    public IEnumerable<object[]> GetRows(IEntityType entityType)
    {
        var table = GetTable(entityType);
        return table.Rows;
    }

    /// <inheritdoc />
    public int ExecuteTransaction(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;

        foreach (var entry in entries)
        {
            var entityType = entry.EntityType;
            var table = (JsonSingleFileTable)GetTable(entityType);

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    table.Create(entry);
                    rowsAffected++;
                    _isDirty = true;
                    break;

                case EntityState.Modified:
                    table.Update(entry);
                    rowsAffected++;
                    _isDirty = true;
                    break;

                case EntityState.Deleted:
                    table.Delete(entry);
                    rowsAffected++;
                    _isDirty = true;
                    break;
            }
        }

        // Validate foreign keys if enabled
        if (_enforceForeignKeys)
        {
            ValidateForeignKeys(entries);
        }

        // Save all changes to the single file
        if (_isDirty)
        {
            SaveAll();
            _isDirty = false;
        }

        return rowsAffected;
    }

    /// <summary>
    /// Validates foreign key constraints for all entries in the transaction.
    /// </summary>
    private void ValidateForeignKeys(IList<IUpdateEntry> entries)
    {
        foreach (var entry in entries)
        {
            switch (entry.EntityState)
            {
                case EntityState.Added:
                    _foreignKeyValidator!.ValidateInsert(entry, GetTableForValidation);
                    break;
                case EntityState.Modified:
                    _foreignKeyValidator!.ValidateUpdate(entry, GetTableForValidation);
                    break;
                case EntityState.Deleted:
                    _foreignKeyValidator!.ValidateDelete(entry, GetTableForValidation);
                    break;
            }
        }
    }

    private IFileTable GetTableForValidation(IEntityType entityType)
    {
        return GetTable(entityType);
    }

    private void SaveAll()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Dispose the cached document before writing
        _cachedDocument?.Dispose();
        _cachedDocument = null;

        var options = new JsonWriterOptions
        {
            Indented = _useIndentation
        };

        using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536);
        using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartObject();

        foreach (var (tableName, table) in _tables)
        {
            writer.WritePropertyName(tableName);
            table.WriteToJsonWriter(writer);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Disposes the cached JSON document.
    /// </summary>
    public void Dispose()
    {
        _cachedDocument?.Dispose();
        _cachedDocument = null;
    }
}
