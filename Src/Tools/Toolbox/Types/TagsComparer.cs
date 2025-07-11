using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Types;

public class TagsComparer : IEqualityComparer<KeyValuePair<string, string>>
{
    public static TagsComparer Default { get; } = new();

    public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
    {
        if (!x.Key.Equals(y.Key, StringComparison.OrdinalIgnoreCase)) return false;

        var result = (x.Value, y.Value) switch
        {
            (null, null) => true,
            (string v1, string v2) => v1.Equals(v2, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

        return result;
    }

    public int GetHashCode([DisallowNull] KeyValuePair<string, string> obj) => HashCode.Combine(obj.Key, obj.Value);
}


public class TagsComparerOption : IEqualityComparer<KeyValuePair<string, string?>>
{
    public static TagsComparerOption Default { get; } = new();

    public bool Equals(KeyValuePair<string, string?> x, KeyValuePair<string, string?> y)
    {
        if (!x.Key.Equals(y.Key, StringComparison.OrdinalIgnoreCase)) return false;

        var result = (x.Value, y.Value) switch
        {
            (null, null) => true,
            (string v1, string v2) => v1.Equals(v2, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

        return result;
    }

    public int GetHashCode([DisallowNull] KeyValuePair<string, string?> obj) => HashCode.Combine(obj.Key, obj.Value);
}

