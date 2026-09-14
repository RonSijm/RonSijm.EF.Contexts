// Licensed under the MIT license.

using System.ComponentModel;
using RonSijm.EF.Csv.Diagnostics.Internal;
using RonSijm.EF.Csv.Infrastructure.Internal;
using RonSijm.EF.Csv.Query.Internal;
using RonSijm.EF.Csv.Storage.Internal;
using RonSijm.EF.Csv.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.FileStore.Metadata.Conventions;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// CSV specific extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class CsvServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the CSV database provider for Entity Framework Core.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkCsv(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, CsvLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<CsvOptionsExtension>>()
            .TryAdd<IValueGeneratorSelector, CsvValueGeneratorSelector>()
            .TryAdd<IDatabase>(p => p.GetRequiredService<ICsvDatabase>())
            .TryAdd<IDbContextTransactionManager, FileStoreTransactionManager>()
            .TryAdd<IDatabaseCreator, CsvDatabaseCreator>()
            .TryAdd<IQueryContextFactory, CsvQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, FileStoreConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileStoreTypeMappingSource>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, CsvShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, CsvQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<ISingletonOptions, ICsvSingletonOptions>(p => p.GetRequiredService<ICsvSingletonOptions>())
            .TryAddProviderSpecificServices(b => b
                .TryAddSingleton<ICsvSingletonOptions, CsvSingletonOptions>()
                .TryAddScoped<ICsvStoreCache, CsvStoreCache>()
                .TryAddScoped<ICsvDatabase, CsvDatabase>());

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}

