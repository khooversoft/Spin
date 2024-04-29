using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphNodeUpdate : IGraphQL
{
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
}
