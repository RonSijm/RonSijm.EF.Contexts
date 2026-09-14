// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Json.Storage.Internal;

/// <summary>
/// Interface for the JSON database.
/// Extends the generic IFileDatabase interface.
/// </summary>
public interface IJsonDatabase : IFileDatabase
{
    /// <summary>
    /// Gets the JSON store.
    /// </summary>
    new IJsonStore Store { get; }
}

