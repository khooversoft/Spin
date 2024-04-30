using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GraphNodeUpdate : IGraphQL
{
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableDictionary<string, ImmutableDictionary<string, string?>> DataMap { get; init; } = ImmutableDictionary<string, ImmutableDictionary<string, string?>>.Empty;
}
