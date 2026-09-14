// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Interface for the file-based database.
/// </summary>
public interface IFileDatabase : IDatabase
{
    /// <summary>
    /// Gets the file store.
    /// </summary>
    IFileStore Store { get; }
}

