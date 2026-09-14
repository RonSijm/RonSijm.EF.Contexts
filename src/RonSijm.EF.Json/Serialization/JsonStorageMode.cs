﻿// Licensed under the MIT license.

namespace RonSijm.EF.Json.Serialization;

/// <summary>
/// Specifies how JSON data is stored on disk.
/// </summary>
public enum JsonStorageMode
{
    /// <summary>
    /// Each entity type is stored in a separate JSON file (e.g., Users.json, Orders.json).
    /// This is the default mode and matches the behavior of Markdown/CSV providers.
    /// </summary>
    MultipleFiles,

    /// <summary>
    /// All entity types are stored in a single JSON file with lazy loading support.
    /// The file structure groups entities by type.
    /// </summary>
    SingleFile
}

