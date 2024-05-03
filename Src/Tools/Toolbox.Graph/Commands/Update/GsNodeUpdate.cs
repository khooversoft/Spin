using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GsNodeUpdate : IGraphQL
{
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableDictionary<string, GraphDataLink> DataMap { get; init; } = ImmutableDictionary<string, GraphDataLink>.Empty;
}
