﻿// Licensed under the MIT license.

using System.Text.Json;
using RonSijm.EF.Json.Infrastructure.Internal;
using RonSijm.EF.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Allows JSON specific configuration to be performed on <see cref="DbContextOptions"/>.
/// </summary>
public class JsonDbContextOptionsBuilder
{
    /// <summary>
    /// Creates a new instance of <see cref="JsonDbContextOptionsBuilder"/>.
    /// </summary>
    public JsonDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    /// <summary>
    /// Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    /// <summary>
    /// Sets the file extension for JSON files (default is ".json").
    /// </summary>
    /// <param name="extension">The file extension to use.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UseFileExtension(string extension)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithFileExtension(extension);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures whether to create the directory if it doesn't exist.
    /// </summary>
    /// <param name="create">Whether to create the directory.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder CreateDirectoryIfNotExists(bool create = true)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithCreateDirectoryIfNotExists(create);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures whether to enforce foreign key constraints.
    /// </summary>
    /// <param name="enforce">Whether to enforce foreign key constraints.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder EnforceForeignKeys(bool enforce = true)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithEnforceForeignKeys(enforce);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures the provider to use multiple files (one per entity type).
    /// This is the default storage mode.
    /// </summary>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UseMultipleFiles()
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithStorageMode(JsonStorageMode.MultipleFiles);
        jsonExtension = jsonExtension.WithSingleFileName(null);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures the provider to use a single file for all entities.
    /// </summary>
    /// <param name="fileName">The name of the single JSON file.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UseSingleFile(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithStorageMode(JsonStorageMode.SingleFile);
        jsonExtension = jsonExtension.WithSingleFileName(fileName);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures the reference mode for foreign key serialization.
    /// </summary>
    /// <param name="referenceMode">The reference mode to use.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UseReferenceMode(JsonReferenceMode referenceMode)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithReferenceMode(referenceMode);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures whether to use indentation (pretty print) in JSON output.
    /// </summary>
    /// <param name="useIndentation">Whether to use indentation.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UseIndentation(bool useIndentation = true)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithIndentation(useIndentation);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    /// <summary>
    /// Configures the property naming policy for JSON serialization.
    /// </summary>
    /// <param name="namingPolicy">The naming policy to use (e.g., JsonNamingPolicy.CamelCase).</param>
    /// <returns>The same builder instance for chaining.</returns>
    public virtual JsonDbContextOptionsBuilder UsePropertyNaming(JsonNamingPolicy? namingPolicy)
    {
        var jsonExtension = GetOrCreateExtension();
        jsonExtension = jsonExtension.WithPropertyNamingPolicy(namingPolicy);
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(jsonExtension);
        return this;
    }

    private JsonOptionsExtension GetOrCreateExtension()
        => OptionsBuilder.Options.FindExtension<JsonOptionsExtension>() ?? new JsonOptionsExtension();
}

