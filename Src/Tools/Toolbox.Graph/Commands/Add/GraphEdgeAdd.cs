using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GraphEdgeAdd : IGraphQL
{
    public string FromKey { get; init; } = null!;
    public string ToKey { get; init; } = null!;
    public string? EdgeType { get; init; }
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
    public bool Upsert { get; init; }
    public bool Unique { get; init; }
}
