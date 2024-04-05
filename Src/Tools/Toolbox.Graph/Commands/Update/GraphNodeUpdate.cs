using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphNodeUpdate : IGraphQL
{
    public Tags Tags { get; init; } = new Tags();
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
    public HashSet<string> Links { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
