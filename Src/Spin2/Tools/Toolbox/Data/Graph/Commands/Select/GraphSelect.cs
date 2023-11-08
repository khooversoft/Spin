namespace Toolbox.Data;

public class GraphSelect : IGraphQL
{
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
