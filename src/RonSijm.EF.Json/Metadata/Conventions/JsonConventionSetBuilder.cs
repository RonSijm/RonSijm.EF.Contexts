// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace RonSijm.EF.Json.Metadata.Conventions;

/// <summary>
/// Convention set builder for the JSON provider.
/// </summary>
public class JsonConventionSetBuilder : ProviderConventionSetBuilder
{
    /// <summary>
    /// Creates a new instance of <see cref="JsonConventionSetBuilder"/>.
    /// </summary>
    public JsonConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        // Add convention to configure value generation for integer keys
        conventionSet.ModelFinalizingConventions.Add(new JsonValueGenerationConvention(Dependencies));

        return conventionSet;
    }
}

/// <summary>
/// Convention that configures value generation for integer primary keys.
/// </summary>
public class JsonValueGenerationConvention : IModelFinalizingConvention
{
    /// <summary>
    /// Creates a new instance of <see cref="JsonValueGenerationConvention"/>.
    /// </summary>
    public JsonValueGenerationConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    /// Gets the dependencies.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                // Configure value generation for integer and long primary keys
                if (property.IsPrimaryKey() && (propertyType == typeof(int) || propertyType == typeof(long)))
                {
                    property.Builder.ValueGenerated(ValueGenerated.OnAdd);
                }

                // Configure value generation for Guid properties
                if (propertyType == typeof(Guid) && property.IsPrimaryKey())
                {
                    property.Builder.ValueGenerated(ValueGenerated.OnAdd);
                }
            }
        }
    }
}

