﻿// Licensed under the MIT license.

namespace RonSijm.EF.Json.Serialization;

/// <summary>
/// Specifies how foreign key relationships are serialized in JSON.
/// </summary>
public enum JsonReferenceMode
{
    /// <summary>
    /// Foreign keys are stored as simple values (e.g., "AuthorId": 5).
    /// This is the default mode and matches the behavior of Markdown/CSV providers.
    /// </summary>
    Flat,

    /// <summary>
    /// Foreign keys are stored using JSON Reference specification ($ref).
    /// Example: "Author": { "$ref": "#/Authors/5" }
    /// Follows draft-pbryan-zyp-json-ref-03 with JSON Pointer (RFC 6901).
    /// </summary>
    JsonReference,

    /// <summary>
    /// Related entities are embedded inline as nested objects.
    /// Example: "Author": { "Id": 5, "Name": "John Doe", ... }
    /// Note: This may cause data duplication and larger file sizes.
    /// </summary>
    Inline
}

