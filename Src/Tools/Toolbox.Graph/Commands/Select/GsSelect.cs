using System.Collections.Immutable;

namespace Toolbox.Graph;

public class GsSelect : IGraphQL
{
    public ImmutableArray<IGraphQL> Search { get; init; } = ImmutableArray<IGraphQL>.Empty;
    public ImmutableHashSet<string> ReturnNames { get; init; } = ImmutableHashSet<string>.Empty;
}
