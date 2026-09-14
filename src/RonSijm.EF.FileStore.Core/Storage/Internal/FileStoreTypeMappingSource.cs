// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Type mapping source for file-based providers.
/// </summary>
public class FileStoreTypeMappingSource : TypeMappingSource
{
    /// <summary>
    /// Creates a new instance of <see cref="FileStoreTypeMappingSource"/>.
    /// </summary>
    public FileStoreTypeMappingSource(TypeMappingSourceDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        if (clrType == null)
        {
            return null;
        }

        // For file-based storage, we support all basic types as they can be serialized to strings
        if (clrType == typeof(string)
            || clrType == typeof(int) || clrType == typeof(int?)
            || clrType == typeof(long) || clrType == typeof(long?)
            || clrType == typeof(short) || clrType == typeof(short?)
            || clrType == typeof(byte) || clrType == typeof(byte?)
            || clrType == typeof(decimal) || clrType == typeof(decimal?)
            || clrType == typeof(double) || clrType == typeof(double?)
            || clrType == typeof(float) || clrType == typeof(float?)
            || clrType == typeof(bool) || clrType == typeof(bool?)
            || clrType == typeof(DateTime) || clrType == typeof(DateTime?)
            || clrType == typeof(DateTimeOffset) || clrType == typeof(DateTimeOffset?)
            || clrType == typeof(DateOnly) || clrType == typeof(DateOnly?)
            || clrType == typeof(TimeOnly) || clrType == typeof(TimeOnly?)
            || clrType == typeof(TimeSpan) || clrType == typeof(TimeSpan?)
            || clrType == typeof(Guid) || clrType == typeof(Guid?)
            || clrType.IsEnum
            || (Nullable.GetUnderlyingType(clrType)?.IsEnum ?? false))
        {
            return new FileStoreTypeMapping(clrType);
        }

        return null;
    }
}

/// <summary>
/// Type mapping for file-based providers.
/// This class contains EF Core infrastructure methods (WithComposedConverter, Clone)
/// that are called internally by EF Core and are not directly testable.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileStoreTypeMapping : CoreTypeMapping
{
    /// <summary>
    /// Creates a new instance of <see cref="FileStoreTypeMapping"/>.
    /// </summary>
    public FileStoreTypeMapping(Type clrType)
        : base(new CoreTypeMappingParameters(clrType))
    {
    }

    /// <summary>
    /// Creates a new instance with the specified parameters.
    /// </summary>
    protected FileStoreTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    public override CoreTypeMapping WithComposedConverter(
        ValueConverter? converter,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
    {
        return new FileStoreTypeMapping(
            Parameters.WithComposedConverter(converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter));
    }

    /// <inheritdoc />
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
    {
        return new FileStoreTypeMapping(parameters);
    }
}

