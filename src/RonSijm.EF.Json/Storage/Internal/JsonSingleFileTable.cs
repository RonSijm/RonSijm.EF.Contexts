// Licensed under the MIT license.

using System.Text.Json;
using RonSijm.EF.FileStore.Storage.Internal;
using RonSijm.EF.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// A JSON table for single-file mode. Does not manage its own file - the parent store does.
/// </summary>
public class JsonSingleFileTable : IJsonTable
{
    private readonly List<object[]> _rows = new();
    private readonly Dictionary<CompositeKey, object[]> _rowIndex = new();
    private readonly IProperty[] _properties;
    private readonly int[] _keyPropertyIndexes;
    private readonly bool _useIndentation;
    private readonly JsonNamingPolicy? _propertyNamingPolicy;
    private readonly JsonReferenceMode _referenceMode;

    // Maps property index to FK info (principal entity type) for $ref mode
    private readonly Dictionary<int, IEntityType>? _fkPropertyMapping;

    /// <summary>
    /// Creates a new instance of <see cref="JsonSingleFileTable"/>.
    /// </summary>
    public JsonSingleFileTable(
        IEntityType entityType,
        string tableName,
        bool useIndentation,
        JsonNamingPolicy? propertyNamingPolicy,
        JsonReferenceMode referenceMode)
    {
        EntityType = entityType;
        TableName = tableName;
        _useIndentation = useIndentation;
        _propertyNamingPolicy = propertyNamingPolicy;
        _referenceMode = referenceMode;
        _properties = entityType.GetProperties().ToArray();

        var keyProperties = entityType.FindPrimaryKey()?.Properties ?? [];
        _keyPropertyIndexes = keyProperties.Select(p => Array.IndexOf(_properties, p)).ToArray();

        // Build FK property mapping for $ref and Inline modes
        if (referenceMode == JsonReferenceMode.JsonReference || referenceMode == JsonReferenceMode.Inline)
        {
            _fkPropertyMapping = BuildForeignKeyMapping(entityType);
        }
    }

    private Dictionary<int, IEntityType>? BuildForeignKeyMapping(IEntityType entityType)
    {
        var mapping = new Dictionary<int, IEntityType>();

        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            // For each FK property, map it to the principal entity type
            foreach (var fkProperty in foreignKey.Properties)
            {
                var index = Array.IndexOf(_properties, fkProperty);
                if (index >= 0)
                {
                    mapping[index] = foreignKey.PrincipalEntityType;
                }
            }
        }

        return mapping.Count > 0 ? mapping : null;
    }

    /// <summary>
    /// Gets the table name.
    /// </summary>
    public string TableName { get; }

    /// <inheritdoc />
    public IEntityType EntityType { get; }

    /// <inheritdoc />
    public IReadOnlyList<object[]> Rows => _rows;

    /// <inheritdoc />
    public void Create(IUpdateEntry entry)
    {
        var row = new object[_properties.Length];
        for (var i = 0; i < _properties.Length; i++)
        {
            row[i] = entry.GetCurrentValue(_properties[i])!;
        }
        _rows.Add(row);

        if (_keyPropertyIndexes.Length > 0)
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
            for (var i = 0; i < _properties.Length; i++)
            {
                if (entry.IsModified(_properties[i]))
                {
                    existingRow[i] = entry.GetCurrentValue(_properties[i])!;
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
            if (_keyPropertyIndexes.Length > 0)
            {
                var key = new CompositeKey(keyValues);
                _rowIndex.Remove(key);
            }
        }
    }

    /// <inheritdoc />
    public void Load()
    {
        // In single-file mode, loading is handled by the parent store via LoadFromJsonElement
    }

    /// <inheritdoc />
    public void Save()
    {
        // In single-file mode, saving is handled by the parent store via WriteToJsonWriter
    }

    /// <inheritdoc />
    public object[]? FindRow(object[] keyValues)
    {
        if (_keyPropertyIndexes.Length > 0)
        {
            var key = new CompositeKey(keyValues);
            return _rowIndex.TryGetValue(key, out var row) ? row : null;
        }

        foreach (var row in _rows)
        {
            var match = true;
            for (var i = 0; i < _keyPropertyIndexes.Length; i++)
            {
                var rowKeyValue = row[_keyPropertyIndexes[i]];
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

    private object[] GetKeyValues(IUpdateEntry entry)
    {
        var keyValues = new object[_keyPropertyIndexes.Length];
        for (var i = 0; i < _keyPropertyIndexes.Length; i++)
        {
            keyValues[i] = entry.GetCurrentValue(_properties[_keyPropertyIndexes[i]])!;
        }
        return keyValues;
    }

    private CompositeKey GetKeyFromRow(object[] row)
    {
        var keyValues = new object[_keyPropertyIndexes.Length];
        for (var i = 0; i < _keyPropertyIndexes.Length; i++)
        {
            keyValues[i] = row[_keyPropertyIndexes[i]];
        }
        return new CompositeKey(keyValues);
    }

    /// <summary>
    /// Loads data from a JSON element (called by the parent store during lazy loading).
    /// </summary>
    public void LoadFromJsonElement(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var propertyMapping = BuildPropertyMapping();

        foreach (var entityElement in element.EnumerateArray())
        {
            if (entityElement.ValueKind == JsonValueKind.Object)
            {
                var row = ParseEntity(entityElement, propertyMapping);
                if (row != null)
                {
                    _rows.Add(row);
                    if (_keyPropertyIndexes.Length > 0)
                    {
                        var key = GetKeyFromRow(row);
                        _rowIndex[key] = row;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Writes this table's data to a JSON writer (called by the parent store during save).
    /// </summary>
    public void WriteToJsonWriter(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (var row in _rows)
        {
            WriteEntity(writer, row);
        }

        writer.WriteEndArray();
    }

    private Dictionary<string, int> BuildPropertyMapping()
    {
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < _properties.Length; i++)
        {
            var property = _properties[i];
            mapping[property.Name] = i;

            if (_propertyNamingPolicy != null)
            {
                var convertedName = _propertyNamingPolicy.ConvertName(property.Name);
                if (!mapping.ContainsKey(convertedName))
                {
                    mapping[convertedName] = i;
                }
            }
        }

        return mapping;
    }

    private object[]? ParseEntity(JsonElement element, Dictionary<string, int> propertyMapping)
    {
        var row = new object[_properties.Length];

        for (var i = 0; i < _properties.Length; i++)
        {
            row[i] = _properties[i].ClrType.IsValueType
                ? Activator.CreateInstance(_properties[i].ClrType)!
                : null!;
        }

        foreach (var jsonProperty in element.EnumerateObject())
        {
            if (propertyMapping.TryGetValue(jsonProperty.Name, out var index))
            {
                // Check if this is a $ref object
                if (JsonReferenceHelper.IsReference(jsonProperty.Value))
                {
                    row[index] = ParseReferenceValue(jsonProperty.Value, _properties[index].ClrType);
                }
                // Check if this is an inline object (FK property with embedded object)
                else if (_fkPropertyMapping != null &&
                         _fkPropertyMapping.TryGetValue(index, out var principalEntityType) &&
                         jsonProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    row[index] = ParseInlineReference(jsonProperty.Value, principalEntityType, _properties[index].ClrType);
                }
                else
                {
                    row[index] = ParsePropertyValue(jsonProperty.Value, _properties[index].ClrType);
                }
            }
        }

        return row;
    }

    private object ParseInlineReference(JsonElement element, IEntityType principalEntityType, Type propertyType)
    {
        // Get the primary key property name of the principal entity
        var primaryKey = principalEntityType.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count == 0)
        {
            return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
                ? Activator.CreateInstance(propertyType)!
                : null!;
        }

        // Try to find the primary key property in the inline object
        var pkProperty = primaryKey.Properties[0];
        var pkPropertyName = pkProperty.Name;
        var pkPropertyNameConverted = _propertyNamingPolicy?.ConvertName(pkPropertyName) ?? pkPropertyName;

        // Try both original and converted names
        if (element.TryGetProperty(pkPropertyNameConverted, out var pkElement) ||
            element.TryGetProperty(pkPropertyName, out pkElement))
        {
            return ParsePropertyValue(pkElement, propertyType);
        }

        return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
            ? Activator.CreateInstance(propertyType)!
            : null!;
    }

    private object ParseReferenceValue(JsonElement element, Type propertyType)
    {
        var refUri = JsonReferenceHelper.GetReferenceUri(element);
        if (string.IsNullOrEmpty(refUri))
        {
            return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
                ? Activator.CreateInstance(propertyType)!
                : null!;
        }

        // Parse the reference to get the key value
        if (JsonReferenceHelper.TryParseReference(refUri, out _, out var keyValue) && keyValue != null)
        {
            // Unescape JSON Pointer encoding
            keyValue = JsonReferenceHelper.UnescapeJsonPointer(keyValue);

            // Parse the key value to the property type
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlyingType == typeof(int) && int.TryParse(keyValue, out var intVal))
            {
                return intVal;
            }
            if (underlyingType == typeof(long) && long.TryParse(keyValue, out var longVal))
            {
                return longVal;
            }
            if (underlyingType == typeof(Guid) && Guid.TryParse(keyValue, out var guidVal))
            {
                return guidVal;
            }
            if (underlyingType == typeof(string))
            {
                return keyValue;
            }

            // Fallback: return the string value
            return keyValue;
        }

        return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
            ? Activator.CreateInstance(propertyType)!
            : null!;
    }

    private object ParsePropertyValue(JsonElement element, Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (element.ValueKind == JsonValueKind.Null)
        {
            return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
                ? Activator.CreateInstance(propertyType)!
                : null!;
        }

        if (underlyingType == typeof(string)) return element.GetString() ?? string.Empty;
        if (underlyingType == typeof(int)) return element.TryGetInt32(out var intVal) ? intVal : 0;
        if (underlyingType == typeof(long)) return element.TryGetInt64(out var longVal) ? longVal : 0L;
        if (underlyingType == typeof(decimal)) return element.TryGetDecimal(out var decVal) ? decVal : 0m;
        if (underlyingType == typeof(double)) return element.TryGetDouble(out var dblVal) ? dblVal : 0d;
        if (underlyingType == typeof(float)) return element.TryGetSingle(out var fltVal) ? fltVal : 0f;
        if (underlyingType == typeof(bool)) return element.ValueKind == JsonValueKind.True;
        if (underlyingType == typeof(DateTime)) return element.TryGetDateTime(out var dtVal) ? dtVal : default;
        if (underlyingType == typeof(DateTimeOffset)) return element.TryGetDateTimeOffset(out var dtoVal) ? dtoVal : default;
        if (underlyingType == typeof(Guid)) return element.TryGetGuid(out var guidVal) ? guidVal : Guid.Empty;
        if (underlyingType == typeof(byte[])) return element.TryGetBytesFromBase64(out var bytes) ? bytes : Array.Empty<byte>();

        if (underlyingType == typeof(DateOnly))
        {
            var str = element.GetString();
            return !string.IsNullOrEmpty(str) && DateOnly.TryParse(str, out var dateOnly) ? dateOnly : default;
        }
        if (underlyingType == typeof(TimeOnly))
        {
            var str = element.GetString();
            return !string.IsNullOrEmpty(str) && TimeOnly.TryParse(str, out var timeOnly) ? timeOnly : default;
        }
        if (underlyingType == typeof(TimeSpan))
        {
            var str = element.GetString();
            return !string.IsNullOrEmpty(str) && TimeSpan.TryParse(str, out var timeSpan) ? timeSpan : default;
        }
        if (underlyingType.IsEnum)
        {
            var str = element.GetString();
            return !string.IsNullOrEmpty(str) && Enum.TryParse(underlyingType, str, true, out var enumVal)
                ? enumVal!
                : Activator.CreateInstance(underlyingType)!;
        }

        return element.ToString();
    }

    private void WriteEntity(Utf8JsonWriter writer, object[] row)
    {
        writer.WriteStartObject();

        for (var i = 0; i < _properties.Length; i++)
        {
            var property = _properties[i];
            var value = row[i];
            var propertyName = _propertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

            // Check if this is an FK property and we're in $ref or Inline mode
            if (_fkPropertyMapping != null && _fkPropertyMapping.TryGetValue(i, out var principalEntityType))
            {
                if (value == null)
                {
                    writer.WriteNull(propertyName);
                }
                else if (_referenceMode == JsonReferenceMode.JsonReference)
                {
                    // Single-file mode uses fragment-only references: #/EntityType/Key
                    var refUri = JsonReferenceHelper.BuildReference(principalEntityType, value, singleFileMode: true);
                    JsonReferenceHelper.WriteReference(writer, propertyName, refUri);
                }
                else if (_referenceMode == JsonReferenceMode.Inline)
                {
                    // Write as inline object with primary key property
                    WriteInlineReference(writer, propertyName, principalEntityType, value);
                }
            }
            else
            {
                WritePropertyValue(writer, propertyName, value, property.ClrType);
            }
        }

        writer.WriteEndObject();
    }

    private void WriteInlineReference(Utf8JsonWriter writer, string propertyName, IEntityType principalEntityType, object keyValue)
    {
        // Get the primary key property name of the principal entity
        var primaryKey = principalEntityType.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count == 0)
        {
            // Fallback to flat value if no primary key
            WritePropertyValue(writer, propertyName, keyValue, keyValue.GetType());
            return;
        }

        writer.WriteStartObject(propertyName);

        // Write the primary key property(ies)
        var pkProperties = primaryKey.Properties;
        if (pkProperties.Count == 1)
        {
            var pkPropertyName = _propertyNamingPolicy?.ConvertName(pkProperties[0].Name) ?? pkProperties[0].Name;
            WritePropertyValue(writer, pkPropertyName, keyValue, pkProperties[0].ClrType);
        }
        else
        {
            // Composite key - keyValue should be an array
            var keyValues = keyValue as object[] ?? new[] { keyValue };
            for (var i = 0; i < pkProperties.Count && i < keyValues.Length; i++)
            {
                var pkPropertyName = _propertyNamingPolicy?.ConvertName(pkProperties[i].Name) ?? pkProperties[i].Name;
                WritePropertyValue(writer, pkPropertyName, keyValues[i], pkProperties[i].ClrType);
            }
        }

        writer.WriteEndObject();
    }

    private void WritePropertyValue(Utf8JsonWriter writer, string propertyName, object? value, Type propertyType)
    {
        if (value == null)
        {
            writer.WriteNull(propertyName);
            return;
        }

        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType == typeof(string)) writer.WriteString(propertyName, (string)value);
        else if (underlyingType == typeof(int)) writer.WriteNumber(propertyName, (int)value);
        else if (underlyingType == typeof(long)) writer.WriteNumber(propertyName, (long)value);
        else if (underlyingType == typeof(decimal)) writer.WriteNumber(propertyName, (decimal)value);
        else if (underlyingType == typeof(double)) writer.WriteNumber(propertyName, (double)value);
        else if (underlyingType == typeof(float)) writer.WriteNumber(propertyName, (float)value);
        else if (underlyingType == typeof(bool)) writer.WriteBoolean(propertyName, (bool)value);
        else if (underlyingType == typeof(DateTime)) writer.WriteString(propertyName, ((DateTime)value).ToString("O"));
        else if (underlyingType == typeof(DateTimeOffset)) writer.WriteString(propertyName, ((DateTimeOffset)value).ToString("O"));
        else if (underlyingType == typeof(DateOnly)) writer.WriteString(propertyName, ((DateOnly)value).ToString("O"));
        else if (underlyingType == typeof(TimeOnly)) writer.WriteString(propertyName, ((TimeOnly)value).ToString("O"));
        else if (underlyingType == typeof(TimeSpan)) writer.WriteString(propertyName, ((TimeSpan)value).ToString("c"));
        else if (underlyingType == typeof(Guid)) writer.WriteString(propertyName, ((Guid)value).ToString());
        else if (underlyingType.IsEnum) writer.WriteString(propertyName, value.ToString());
        else if (underlyingType == typeof(byte[])) writer.WriteBase64String(propertyName, (byte[])value);
        else writer.WriteString(propertyName, value.ToString());
    }
}

