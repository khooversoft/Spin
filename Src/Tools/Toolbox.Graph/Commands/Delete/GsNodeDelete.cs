using System.Collections.Immutable;

namespace Toolbox.Graph;

public record GsNodeDelete : IGraphQL
{
    public ImmutableArray<IGraphQL> Search { get; init; } = ImmutableArray<IGraphQL>.Empty;
    public bool Force { get; init; }
}
