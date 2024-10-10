using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Graph;

public record UniqueIndex
{
    public string IndexName { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string NodeKey { get; init; } = null!;
}

public class UniqueIndexEqualityComparer : IEqualityComparer<UniqueIndex>
{
    public bool Equals(UniqueIndex? x, UniqueIndex? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null || y is null) return false;
        return x == y;
    }

    public int GetHashCode([DisallowNull] UniqueIndex obj) => HashCode.Combine(obj.IndexName, obj.Value, obj.NodeKey);
}