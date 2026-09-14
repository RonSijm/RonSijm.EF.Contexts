// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.FileStore.ForeignKeys;

/// <summary>
/// Validates foreign key constraints for file-based stores.
/// </summary>
public class ForeignKeyValidator : IForeignKeyValidator
{
    /// <inheritdoc />
    public void ValidateInsert(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable)
    {
        var entityType = entry.EntityType;

        // Check all foreign keys where this entity is the dependent (child)
        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            // Skip if the FK is optional and all FK values are null
            if (!foreignKey.IsRequired && AreAllForeignKeyValuesNull(entry, foreignKey))
            {
                continue;
            }

            // Get the principal (parent) table
            var principalEntityType = foreignKey.PrincipalEntityType;
            var principalTable = getTable(principalEntityType);

            // Get the FK values from the entry
            var fkValues = GetForeignKeyValues(entry, foreignKey);

            // Check if the parent row exists using the principal key
            var principalKey = foreignKey.PrincipalKey;
            var principalKeyValues = MapForeignKeyToPrincipalKey(fkValues, foreignKey, principalKey);

            var parentRow = principalTable.FindRow(principalKeyValues);
            if (parentRow == null)
            {
                var fkValueString = string.Join(", ", fkValues.Select(v => v?.ToString() ?? "null"));
                throw new ForeignKeyValidationException(
                    $"Foreign key constraint violation: Cannot insert entity '{entityType.DisplayName()}' " +
                    $"because the referenced parent entity '{principalEntityType.DisplayName()}' " +
                    $"with key value(s) [{fkValueString}] does not exist.");
            }
        }
    }

    /// <inheritdoc />
    public void ValidateDelete(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable)
    {
        var entityType = entry.EntityType;

        // Check all foreign keys where this entity is the principal (parent)
        foreach (var foreignKey in entityType.GetReferencingForeignKeys())
        {
            // Get the dependent (child) table
            var dependentEntityType = foreignKey.DeclaringEntityType;
            var dependentTable = getTable(dependentEntityType);

            // Get the principal key values from the entry being deleted
            var principalKey = foreignKey.PrincipalKey;
            var principalKeyValues = GetPrincipalKeyValues(entry, principalKey);

            // Check if any child rows reference this parent
            if (HasReferencingRows(dependentTable, foreignKey, principalKeyValues))
            {
                var pkValueString = string.Join(", ", principalKeyValues.Select(v => v?.ToString() ?? "null"));
                throw new ForeignKeyValidationException(
                    $"Foreign key constraint violation: Cannot delete entity '{entityType.DisplayName()}' " +
                    $"with key value(s) [{pkValueString}] because it is referenced by " +
                    $"entity '{dependentEntityType.DisplayName()}'.");
            }
        }
    }

    /// <inheritdoc />
    public void ValidateUpdate(IUpdateEntry entry, Func<IEntityType, IFileTable> getTable)
    {
        var entityType = entry.EntityType;

        // Check if any FK properties are being modified
        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            var fkProperties = foreignKey.Properties;
            var anyFkModified = fkProperties.Any(p => entry.IsModified(p));

            if (!anyFkModified)
            {
                continue;
            }

            // Skip if the FK is optional and all new FK values are null
            if (!foreignKey.IsRequired && AreAllForeignKeyValuesNull(entry, foreignKey))
            {
                continue;
            }

            // Validate the new FK values point to an existing parent
            var principalEntityType = foreignKey.PrincipalEntityType;
            var principalTable = getTable(principalEntityType);

            var fkValues = GetForeignKeyValues(entry, foreignKey);
            var principalKey = foreignKey.PrincipalKey;
            var principalKeyValues = MapForeignKeyToPrincipalKey(fkValues, foreignKey, principalKey);

            var parentRow = principalTable.FindRow(principalKeyValues);
            if (parentRow == null)
            {
                var fkValueString = string.Join(", ", fkValues.Select(v => v?.ToString() ?? "null"));
                throw new ForeignKeyValidationException(
                    $"Foreign key constraint violation: Cannot update entity '{entityType.DisplayName()}' " +
                    $"because the referenced parent entity '{principalEntityType.DisplayName()}' " +
                    $"with key value(s) [{fkValueString}] does not exist.");
            }
        }

        // Check if any principal key properties are being modified
        foreach (var foreignKey in entityType.GetReferencingForeignKeys())
        {
            var principalKey = foreignKey.PrincipalKey;
            var anyPkModified = principalKey.Properties.Any(p => entry.IsModified(p));

            if (!anyPkModified)
            {
                continue;
            }

            // Check if any child rows reference the old key values
            var dependentEntityType = foreignKey.DeclaringEntityType;
            var dependentTable = getTable(dependentEntityType);

            var oldPrincipalKeyValues = GetOriginalPrincipalKeyValues(entry, principalKey);

            if (HasReferencingRows(dependentTable, foreignKey, oldPrincipalKeyValues))
            {
                var pkValueString = string.Join(", ", oldPrincipalKeyValues.Select(v => v?.ToString() ?? "null"));
                throw new ForeignKeyValidationException(
                    $"Foreign key constraint violation: Cannot update key of entity '{entityType.DisplayName()}' " +
                    $"with key value(s) [{pkValueString}] because it is referenced by " +
                    $"entity '{dependentEntityType.DisplayName()}'.");
            }
        }
    }

    private static bool AreAllForeignKeyValuesNull(IUpdateEntry entry, IForeignKey foreignKey)
    {
        foreach (var property in foreignKey.Properties)
        {
            var value = entry.GetCurrentValue(property);
            if (value != null)
            {
                return false;
            }
        }
        return true;
    }

    private static object[] GetForeignKeyValues(IUpdateEntry entry, IForeignKey foreignKey)
    {
        var properties = foreignKey.Properties;
        var values = new object[properties.Count];
        for (var i = 0; i < properties.Count; i++)
        {
            values[i] = entry.GetCurrentValue(properties[i])!;
        }
        return values;
    }

    private static object[] GetPrincipalKeyValues(IUpdateEntry entry, IKey principalKey)
    {
        var properties = principalKey.Properties;
        var values = new object[properties.Count];
        for (var i = 0; i < properties.Count; i++)
        {
            values[i] = entry.GetCurrentValue(properties[i])!;
        }
        return values;
    }

    private static object[] GetOriginalPrincipalKeyValues(IUpdateEntry entry, IKey principalKey)
    {
        var properties = principalKey.Properties;
        var values = new object[properties.Count];
        for (var i = 0; i < properties.Count; i++)
        {
            values[i] = entry.GetOriginalValue(properties[i])!;
        }
        return values;
    }

    private static object[] MapForeignKeyToPrincipalKey(object[] fkValues, IForeignKey foreignKey, IKey principalKey)
    {
        // In most cases, FK properties map directly to principal key properties in order
        // This handles composite keys correctly
        if (fkValues.Length == principalKey.Properties.Count)
        {
            return fkValues;
        }

        // If lengths don't match, we need to map by property
        var principalKeyValues = new object[principalKey.Properties.Count];
        for (var i = 0; i < foreignKey.Properties.Count && i < principalKey.Properties.Count; i++)
        {
            principalKeyValues[i] = fkValues[i];
        }
        return principalKeyValues;
    }

    private static bool HasReferencingRows(IFileTable dependentTable, IForeignKey foreignKey, object[] principalKeyValues)
    {
        var fkProperties = foreignKey.Properties;
        var dependentEntityType = foreignKey.DeclaringEntityType;
        var allProperties = dependentEntityType.GetProperties().ToArray();

        // Get the indexes of FK properties in the row
        var fkPropertyIndexes = new int[fkProperties.Count];
        for (var i = 0; i < fkProperties.Count; i++)
        {
            fkPropertyIndexes[i] = Array.IndexOf(allProperties, fkProperties[i]);
        }

        // Scan all rows in the dependent table
        foreach (var row in dependentTable.Rows)
        {
            var matches = true;
            for (var i = 0; i < fkPropertyIndexes.Length; i++)
            {
                var fkValue = row[fkPropertyIndexes[i]];
                var principalValue = principalKeyValues[i];

                // Handle null FK values (optional relationships)
                if (fkValue == null)
                {
                    matches = false;
                    break;
                }

                if (!Equals(fkValue, principalValue))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return true;
            }
        }

        return false;
    }
}

