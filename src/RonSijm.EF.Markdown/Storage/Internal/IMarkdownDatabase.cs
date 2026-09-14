// Licensed under the MIT license.

using RonSijm.EF.FileStore.Storage.Internal;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// Interface for the Markdown database.
/// Extends the generic IFileDatabase interface.
/// </summary>
public interface IMarkdownDatabase : IFileDatabase
{
    /// <summary>
    /// Gets the Markdown store.
    /// </summary>
    new IMarkdownStore Store { get; }
}

