using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GraphEdgeUpdate : IGraphQL
{
    public string? EdgeType { get; init; }
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
