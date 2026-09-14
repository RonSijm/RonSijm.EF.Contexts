// Licensed under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Json.Diagnostics.Internal;
using RonSijm.EF.Json.Infrastructure.Internal;
using RonSijm.EF.Json.Metadata.Conventions;
using RonSijm.EF.Json.Query.Internal;
using RonSijm.EF.Json.Storage.Internal;
using RonSijm.EF.Json.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// JSON specific extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class JsonServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the JSON database provider for Entity Framework Core.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkJson(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, JsonLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<JsonOptionsExtension>>()
            .TryAdd<IValueGeneratorSelector, JsonValueGeneratorSelector>()
            .TryAdd<IDatabase>(p => p.GetRequiredService<IJsonDatabase>())
            .TryAdd<IDbContextTransactionManager, FileStoreTransactionManager>()
            .TryAdd<IDatabaseCreator, JsonDatabaseCreator>()
            .TryAdd<IQueryContextFactory, JsonQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, JsonConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileStoreTypeMappingSource>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, JsonShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, JsonQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<ISingletonOptions, IJsonSingletonOptions>(p => p.GetRequiredService<IJsonSingletonOptions>())
            .TryAddProviderSpecificServices(b => b
                .TryAddSingleton<IJsonSingletonOptions, JsonSingletonOptions>()
                .TryAddSingleton<IJsonStoreCache, JsonStoreCache>()
                .TryAddScoped<IJsonDatabase, JsonDatabase>());

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}

