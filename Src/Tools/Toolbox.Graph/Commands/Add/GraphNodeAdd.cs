using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public bool Upsert { get; init; }
    public ImmutableHashSet<string> Links { get; init; } = ImmutableHashSet<string>.Empty;
}
