using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public Tags Tags { get; init; } = new Tags();
    public bool Upsert { get; init; }
    public HashSet<string> Links { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
