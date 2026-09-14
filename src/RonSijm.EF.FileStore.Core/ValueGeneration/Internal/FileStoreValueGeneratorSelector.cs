// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace RonSijm.EF.FileStore.ValueGeneration.Internal;

/// <summary>
/// Value generator selector for file-based providers.
/// </summary>
public class FileStoreValueGeneratorSelector : ValueGeneratorSelector
{
    /// <summary>
    /// Creates a new instance of <see cref="FileStoreValueGeneratorSelector"/>.
    /// </summary>
    public FileStoreValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override bool TrySelect(IProperty property, ITypeBase typeBase, out ValueGenerator? valueGenerator)
    {
        var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (propertyType == typeof(int))
        {
            valueGenerator = new FileStoreIntValueGenerator();
            return true;
        }

        if (propertyType == typeof(long))
        {
            valueGenerator = new FileStoreLongValueGenerator();
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
public class FileStoreIntValueGenerator : ValueGenerator<int>
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
public class FileStoreLongValueGenerator : ValueGenerator<long>
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

