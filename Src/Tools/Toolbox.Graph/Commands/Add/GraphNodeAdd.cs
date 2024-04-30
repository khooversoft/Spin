using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public bool Upsert { get; init; }
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableDictionary<string, ImmutableDictionary<string, string?>> DataMap { get; init; } = ImmutableDictionary<string, ImmutableDictionary<string, string?>>.Empty;
}
