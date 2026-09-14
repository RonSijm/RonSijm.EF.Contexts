// Licensed under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Json.Serialization;

/// <summary>
/// Helper class for building and parsing JSON References according to draft-pbryan-zyp-json-ref-03.
/// </summary>
public static class JsonReferenceHelper
{
    /// <summary>
    /// The JSON Reference property name.
    /// </summary>
    public const string RefPropertyName = "$ref";

    /// <summary>
    /// Builds a JSON Reference URI for a related entity.
    /// For single-file mode: #/EntityTypeName/PrimaryKeyValue
    /// For multi-file mode: EntityTypeName.json#/PrimaryKeyValue
    /// </summary>
    /// <param name="principalEntityType">The principal (parent) entity type.</param>
    /// <param name="primaryKeyValue">The primary key value of the referenced entity.</param>
    /// <param name="singleFileMode">Whether using single-file storage mode.</param>
    /// <param name="fileExtension">The file extension (for multi-file mode).</param>
    /// <returns>The JSON Reference URI string.</returns>
    public static string BuildReference(
        IEntityType principalEntityType,
        object primaryKeyValue,
        bool singleFileMode,
        string fileExtension = ".json")
    {
        var entityTypeName = principalEntityType.ClrType.Name;
        var keyString = FormatKeyValue(primaryKeyValue);

        if (singleFileMode)
        {
            // Fragment-only reference for same document
            return $"#/{entityTypeName}/{keyString}";
        }
        else
        {
            // Relative file reference
            return $"{entityTypeName}{fileExtension}#/{keyString}";
        }
    }

    /// <summary>
    /// Builds a JSON Reference URI for a composite key.
    /// </summary>
    public static string BuildReference(
        IEntityType principalEntityType,
        object[] primaryKeyValues,
        bool singleFileMode,
        string fileExtension = ".json")
    {
        if (primaryKeyValues.Length == 1)
        {
            return BuildReference(principalEntityType, primaryKeyValues[0], singleFileMode, fileExtension);
        }

        var entityTypeName = principalEntityType.ClrType.Name;
        var keyString = string.Join(",", primaryKeyValues.Select(FormatKeyValue));

        if (singleFileMode)
        {
            return $"#/{entityTypeName}/{keyString}";
        }
        else
        {
            return $"{entityTypeName}{fileExtension}#/{keyString}";
        }
    }

    /// <summary>
    /// Parses a JSON Reference URI and extracts the entity type name and key value.
    /// </summary>
    /// <param name="refUri">The JSON Reference URI.</param>
    /// <param name="entityTypeName">The extracted entity type name.</param>
    /// <param name="keyValue">The extracted key value string.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParseReference(string refUri, out string? entityTypeName, out string? keyValue)
    {
        entityTypeName = null;
        keyValue = null;

        if (string.IsNullOrEmpty(refUri))
        {
            return false;
        }

        // Handle fragment-only references: #/EntityType/Key
        if (refUri.StartsWith("#/"))
        {
            var path = refUri.Substring(2);
            var slashIndex = path.IndexOf('/');
            if (slashIndex > 0)
            {
                entityTypeName = path.Substring(0, slashIndex);
                keyValue = path.Substring(slashIndex + 1);
                return true;
            }
        }
        // Handle relative file references: EntityType.json#/Key
        else
        {
            var hashIndex = refUri.IndexOf('#');
            if (hashIndex > 0)
            {
                var filePart = refUri.Substring(0, hashIndex);
                var fragmentPart = refUri.Substring(hashIndex + 1);

                // Remove file extension
                var dotIndex = filePart.LastIndexOf('.');
                entityTypeName = dotIndex > 0 ? filePart.Substring(0, dotIndex) : filePart;

                // Remove leading slash from fragment
                keyValue = fragmentPart.StartsWith("/") ? fragmentPart.Substring(1) : fragmentPart;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Writes a JSON Reference object to the writer.
    /// </summary>
    public static void WriteReference(Utf8JsonWriter writer, string propertyName, string refUri)
    {
        writer.WriteStartObject(propertyName);
        writer.WriteString(RefPropertyName, refUri);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Checks if a JSON element is a JSON Reference object.
    /// </summary>
    public static bool IsReference(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(RefPropertyName, out _);
    }

    /// <summary>
    /// Gets the reference URI from a JSON Reference object.
    /// </summary>
    public static string? GetReferenceUri(JsonElement element)
    {
        if (element.TryGetProperty(RefPropertyName, out var refProperty))
        {
            return refProperty.GetString();
        }
        return null;
    }

    private static string FormatKeyValue(object value)
    {
        // Escape special characters in JSON Pointer (RFC 6901)
        // ~ must be escaped as ~0
        // / must be escaped as ~1
        var str = value?.ToString() ?? "";
        return str.Replace("~", "~0").Replace("/", "~1");
    }

    /// <summary>
    /// Unescapes a JSON Pointer value.
    /// </summary>
    public static string UnescapeJsonPointer(string value)
    {
        // Unescape in reverse order: ~1 -> /, ~0 -> ~
        return value.Replace("~1", "/").Replace("~0", "~");
    }
}

