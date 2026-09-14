// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Interface for the CSV database.
/// Extends the generic IFileDatabase interface.
/// </summary>
public interface ICsvDatabase : IFileDatabase
{
    /// <summary>
    /// Gets the CSV store.
    /// </summary>
    new ICsvStore Store { get; }
}

