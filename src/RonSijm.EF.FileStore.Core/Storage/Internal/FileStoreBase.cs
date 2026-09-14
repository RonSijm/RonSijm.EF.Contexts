// Licensed under the MIT license.

using RonSijm.EF.FileStore.ForeignKeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Abstract base class for file stores that manage entity data.
/// </summary>
public abstract class FileStoreBase : IFileStore
{
    private readonly Dictionary<IEntityType, IFileTable> _tables = new();
    private readonly string _fileExtension;
    private readonly bool _enforceForeignKeys;
    private readonly IForeignKeyValidator? _foreignKeyValidator;
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new instance of <see cref="FileStoreBase"/>.
    /// </summary>
    protected FileStoreBase(string directoryPath, string fileExtension)
        : this(directoryPath, fileExtension, enforceForeignKeys: false)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileStoreBase"/> with foreign key enforcement option.
    /// </summary>
    protected FileStoreBase(string directoryPath, string fileExtension, bool enforceForeignKeys)
    {
        DirectoryPath = directoryPath;
        _fileExtension = fileExtension;
        _enforceForeignKeys = enforceForeignKeys;
        _foreignKeyValidator = enforceForeignKeys ? new ForeignKeyValidator() : null;
    }

    /// <inheritdoc />
    public string DirectoryPath { get; }

    /// <summary>
    /// Gets the file extension for this store.
    /// </summary>
    protected string FileExtension => _fileExtension;

    /// <inheritdoc />
    public bool EnsureCreated(IModel model)
    {
        var created = false;
        
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
            created = true;
        }

        foreach (var entityType in model.GetEntityTypes())
        {
            var table = GetTable(entityType);
            if (!File.Exists(GetFilePath(entityType)))
            {
                table.Save();
                created = true;
            }
        }

        return created;
    }

    /// <inheritdoc />
    public bool EnsureDeleted()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return false;
        }

        Directory.Delete(DirectoryPath, recursive: true);
        _tables.Clear();
        return true;
    }

    /// <inheritdoc />
    public int ExecuteTransaction(IList<IUpdateEntry> entries)
    {
        lock (_lock)
        {
            // Validate foreign keys before making any changes (if enabled)
            if (_enforceForeignKeys && _foreignKeyValidator != null)
            {
                ValidateForeignKeys(entries);
            }

            var affectedTables = new HashSet<IFileTable>();
            var count = 0;

            foreach (var entry in entries)
            {
                var entityType = entry.EntityType;
                var table = GetTable(entityType);
                affectedTables.Add(table);

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        table.Create(entry);
                        count++;
                        break;
                    case EntityState.Modified:
                        table.Update(entry);
                        count++;
                        break;
                    case EntityState.Deleted:
                        table.Delete(entry);
                        count++;
                        break;
                }
            }

            // Save all affected tables
            foreach (var table in affectedTables)
            {
                table.Save();
            }

            return count;
        }
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
                    _foreignKeyValidator!.ValidateInsert(entry, GetTable);
                    break;
                case EntityState.Modified:
                    _foreignKeyValidator!.ValidateUpdate(entry, GetTable);
                    break;
                case EntityState.Deleted:
                    _foreignKeyValidator!.ValidateDelete(entry, GetTable);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<object[]> GetRows(IEntityType entityType)
    {
        var table = GetTable(entityType);
        return table.Rows;
    }

    /// <inheritdoc />
    public IFileTable GetTable(IEntityType entityType)
    {
        lock (_lock)
        {
            if (!_tables.TryGetValue(entityType, out var table))
            {
                table = CreateTable(entityType, DirectoryPath, _fileExtension);
                table.Load();
                _tables[entityType] = table;
            }
            return table;
        }
    }

    /// <summary>
    /// Creates a new table for the specified entity type.
    /// Override in derived classes to create format-specific tables.
    /// </summary>
    protected abstract IFileTable CreateTable(IEntityType entityType, string directoryPath, string fileExtension);

    private string GetFilePath(IEntityType entityType)
    {
        return Path.Combine(DirectoryPath, $"{entityType.ClrType.Name}{_fileExtension}");
    }
}
