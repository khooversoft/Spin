using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphEdgePrimaryKey
{
    public string FromKey { get; init; } = null!;
    public string ToKey { get; init; } = null!;
    public string EdgeType { get; init; } = null!;

    public override string ToString() => $"{{ FromKey={FromKey} -> ToKey={ToKey} ({EdgeType}) }}";

    public bool Equals(GraphEdge? obj) => obj is GraphEdge subject &&
        FromKey.EqualsIgnoreCase(subject.FromKey) &&
        ToKey.EqualsIgnoreCase(subject.ToKey) &&
        EdgeType.EqualsIgnoreCase(subject.EdgeType);

    public override int GetHashCode() => HashCode.Combine(FromKey, ToKey, EdgeType);

    public static implicit operator GraphEdgePrimaryKey((string fromKey, string toKey, string edgeType) subject) => new GraphEdgePrimaryKey
    {
        FromKey = subject.fromKey,
        ToKey = subject.toKey,
        EdgeType = subject.edgeType,
    };

    public static IValidator<GraphEdgePrimaryKey> Validator { get; } = new Validator<GraphEdgePrimaryKey>()
        .RuleFor(x => x.FromKey).NotNull()
        .RuleFor(x => x.ToKey).NotNull()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .RuleForObject(x => x).Must(x => !x.FromKey.EqualsIgnoreCase(x.ToKey), _ => "From and to keys cannot be the same")
        .Build();
}


public static class GraphEdgePrimaryKeyTool
{
    public static Option Validate(this GraphEdgePrimaryKey subject) => GraphEdgePrimaryKey.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphEdgePrimaryKey subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static GraphEdgePrimaryKey GetPrimaryKey(this GraphEdge subject) => new GraphEdgePrimaryKey
    {
        FromKey = subject.FromKey,
        ToKey = subject.ToKey,
        EdgeType = subject.EdgeType,
    };
}


public sealed class GraphEdgePrimaryKeyComparer : IEqualityComparer<GraphEdgePrimaryKey>
{
    public static GraphEdgePrimaryKeyComparer Default { get; } = new GraphEdgePrimaryKeyComparer();

    public bool Equals(GraphEdgePrimaryKey? x, GraphEdgePrimaryKey? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null || y is null) return false;

        return x.FromKey.EqualsIgnoreCase(y.FromKey) &&
            x.ToKey.EqualsIgnoreCase(y.ToKey) &&
            x.EdgeType.EqualsIgnoreCase(y.EdgeType);
    }

    public int GetHashCode([DisallowNull] GraphEdgePrimaryKey obj) => HashCode.Combine(obj.FromKey, obj.ToKey, obj.EdgeType);
}

