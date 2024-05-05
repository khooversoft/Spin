using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GsNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public bool Upsert { get; init; }
    public ImmutableDictionary<string, GraphDataSource> DataMap { get; init; } = ImmutableDictionary<string, GraphDataSource>.Empty;
}
