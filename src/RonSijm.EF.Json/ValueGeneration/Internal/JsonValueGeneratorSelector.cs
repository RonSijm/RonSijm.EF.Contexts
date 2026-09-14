// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace RonSijm.EF.Json.ValueGeneration.Internal;

/// <summary>
/// Value generator selector for the JSON provider.
/// </summary>
public class JsonValueGeneratorSelector : ValueGeneratorSelector
{
    /// <summary>
    /// Creates a new instance of <see cref="JsonValueGeneratorSelector"/>.
    /// </summary>
    public JsonValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override bool TrySelect(IProperty property, ITypeBase typeBase, out ValueGenerator? valueGenerator)
    {
        var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (propertyType == typeof(int))
        {
            valueGenerator = new JsonIntValueGenerator();
            return true;
        }

        if (propertyType == typeof(long))
        {
            valueGenerator = new JsonLongValueGenerator();
            return true;
        }

        if (propertyType == typeof(Guid))
        {
            valueGenerator = new GuidValueGenerator();
            return true;
        }

        return base.TrySelect(property, typeBase, out valueGenerator);
    }
}

/// <summary>
/// Value generator for int properties.
/// </summary>
public class JsonIntValueGenerator : ValueGenerator<int>
{
    private static int _current;

    /// <inheritdoc />
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc />
    public override int Next(EntityEntry entry)
    {
        return Interlocked.Increment(ref _current);
    }
}

/// <summary>
/// Value generator for long properties.
/// </summary>
public class JsonLongValueGenerator : ValueGenerator<long>
{
    private static long _current;

    /// <inheritdoc />
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc />
    public override long Next(EntityEntry entry)
    {
        return Interlocked.Increment(ref _current);
    }
}

