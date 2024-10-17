using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record UniqueIndex
{
    public UniqueIndex(string indexName, string value, string nodeKey)
    {
        IndexName = indexName.NotEmpty();
        Value = value.NotEmpty();
        NodeKey = nodeKey.NotEmpty();

        PrimaryKey = UniqueIndexComparer.CreatePrimaryKey(IndexName, Value);
    }

    public string PrimaryKey { get; } = null!;
    public string IndexName { get; } = null!;
    public string Value { get; } = null!;
    public string NodeKey { get; } = null!;
}

public class UniqueIndexComparer : IEqualityComparer<UniqueIndex>
{
    public static UniqueIndexComparer Default { get; } = new UniqueIndexComparer();

    public bool Equals(UniqueIndex? x, UniqueIndex? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null || y is null) return false;
        return x == y;
    }

    public int GetHashCode([DisallowNull] UniqueIndex obj) => HashCode.Combine(obj.IndexName, obj.Value, obj.NodeKey);

    public static string CreatePrimaryKey(string indexName, string value) => indexName + '=' + value;
}

public static class UniqueIndexTool
{
    public static IReadOnlyList<string> RemoveDeleteCommands(this IEnumerable<string> values) => values.NotNull()
        .Where(x => !TagsTool.HasRemoveFlag(x))
        .ToImmutableArray();

    public static IReadOnlyList<string> GetDeleteCommands(this IEnumerable<string> values) => values.NotNull()
        .Where(x => TagsTool.HasRemoveFlag(x))
        .Select(x => x[1..])
        .ToImmutableArray();

    public static IReadOnlyList<string> MergeIndexes(this IEnumerable<string> newIndexes, IEnumerable<string> currentIndexes)
    {
        newIndexes.NotNull();
        currentIndexes.NotNull();

        var deleteCommands = GetDeleteCommands(newIndexes);

        var list = newIndexes
            .Concat(currentIndexes)
            .Where(x => !TagsTool.HasRemoveFlag(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(x => !deleteCommands.Contains(x))
            .ToImmutableArray();

        return list;
    }
}