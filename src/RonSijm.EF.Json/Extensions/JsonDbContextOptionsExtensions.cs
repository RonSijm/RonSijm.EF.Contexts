﻿// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// JSON specific extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class JsonDbContextOptionsExtensions
{
    /// <summary>
    /// Configures the context to use the JSON database provider, storing entities as JSON files.
    /// </summary>
    /// <typeparam name="TContext">The type of context being configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where JSON files will be stored.</param>
    /// <param name="jsonOptionsAction">An optional action to allow additional JSON specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseJson<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string directoryPath,
        Action<JsonDbContextOptionsBuilder>? jsonOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseJson(
            (DbContextOptionsBuilder)optionsBuilder, directoryPath, jsonOptionsAction);

    /// <summary>
    /// Configures the context to use the JSON database provider, storing entities as JSON files.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="directoryPath">The directory path where JSON files will be stored.</param>
    /// <param name="jsonOptionsAction">An optional action to allow additional JSON specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseJson(
        this DbContextOptionsBuilder optionsBuilder,
        string directoryPath,
        Action<JsonDbContextOptionsBuilder>? jsonOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var extension = optionsBuilder.Options.FindExtension<JsonOptionsExtension>()
            ?? new JsonOptionsExtension();

        extension = extension.WithDirectoryPath(directoryPath);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        jsonOptionsAction?.Invoke(new JsonDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
}

