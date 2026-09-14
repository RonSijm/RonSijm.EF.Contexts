// Licensed under the MIT license.

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Represents a composite primary key for dictionary-based row indexing.
/// </summary>
public readonly struct CompositeKey : IEquatable<CompositeKey>
{
    private readonly object[] _values;
    private readonly int _hashCode;

    public CompositeKey(object[] values)
    {
        _values = values;
        _hashCode = ComputeHashCode(values);
    }

    private static int ComputeHashCode(object[] values)
    {
        var hash = new HashCode();
        foreach (var value in values)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    public bool Equals(CompositeKey other)
    {
        if (_values.Length != other._values.Length)
            return false;

        for (var i = 0; i < _values.Length; i++)
        {
            if (!Equals(_values[i], other._values[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is CompositeKey other && Equals(other);

    public override int GetHashCode() => _hashCode;
}

