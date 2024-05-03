using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GsNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public bool Upsert { get; init; }
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableDictionary<string, GraphDataLink> DataMap { get; init; } = ImmutableDictionary<string, GraphDataLink>.Empty;
}
