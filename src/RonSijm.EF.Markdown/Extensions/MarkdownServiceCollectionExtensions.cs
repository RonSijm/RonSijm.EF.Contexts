// Licensed under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Markdown.Diagnostics.Internal;
using RonSijm.EF.Markdown.Infrastructure.Internal;
using RonSijm.EF.Markdown.Query.Internal;
using RonSijm.EF.Markdown.Storage.Internal;
using RonSijm.EF.Markdown.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Markdown specific extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class MarkdownServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the Markdown database provider for Entity Framework Core.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkMarkdown(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, MarkdownLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<MarkdownOptionsExtension>>()
            .TryAdd<IValueGeneratorSelector, MarkdownValueGeneratorSelector>()
            .TryAdd<IDatabase>(p => p.GetRequiredService<IMarkdownDatabase>())
            .TryAdd<IDbContextTransactionManager, FileStoreTransactionManager>()
            .TryAdd<IDatabaseCreator, MarkdownDatabaseCreator>()
            .TryAdd<IQueryContextFactory, MarkdownQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, MarkdownConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileStoreTypeMappingSource>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, MarkdownShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, MarkdownQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<ISingletonOptions, IMarkdownSingletonOptions>(p => p.GetRequiredService<IMarkdownSingletonOptions>())
            .TryAddProviderSpecificServices(b => b
                .TryAddSingleton<IMarkdownSingletonOptions, MarkdownSingletonOptions>()
                .TryAddSingleton<IMarkdownStoreCache, MarkdownStoreCache>()
                .TryAddScoped<IMarkdownDatabase, MarkdownDatabase>());

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}

