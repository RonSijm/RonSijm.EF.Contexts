// Licensed under the MIT license.

namespace RonSijm.EF.FileStore.ForeignKeys;

/// <summary>
/// Exception thrown when a foreign key constraint is violated.
/// </summary>
public class ForeignKeyValidationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new instance of <see cref="ForeignKeyValidationException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForeignKeyValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ForeignKeyValidationException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ForeignKeyValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

