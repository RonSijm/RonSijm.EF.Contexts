// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace RonSijm.EF.Markdown.ValueGeneration.Internal;

/// <summary>
/// Value generator selector for the Markdown provider.
/// </summary>
public class MarkdownValueGeneratorSelector : ValueGeneratorSelector
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownValueGeneratorSelector"/>.
    /// </summary>
    public MarkdownValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override bool TrySelect(IProperty property, ITypeBase typeBase, out ValueGenerator? valueGenerator)
    {
        var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (propertyType == typeof(int))
        {
            valueGenerator = new MarkdownIntValueGenerator();
            return true;
        }

        if (propertyType == typeof(long))
        {
            valueGenerator = new MarkdownLongValueGenerator();
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
public class MarkdownIntValueGenerator : ValueGenerator<int>
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
public class MarkdownLongValueGenerator : ValueGenerator<long>
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

