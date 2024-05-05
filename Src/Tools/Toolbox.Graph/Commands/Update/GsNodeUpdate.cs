using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GsNodeUpdate : IGraphQL
{
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
    public ImmutableDictionary<string, GraphDataSource> DataMap { get; init; } = ImmutableDictionary<string, GraphDataSource>.Empty;
}
