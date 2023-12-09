using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record GraphCommandResults
{
    public IReadOnlyList<GraphQueryResult> Items { get; init; } = Array.Empty<GraphQueryResult>();
}



public static class GraphCommandResultsExtensions
{
    public static bool IsOk(this GraphCommandResults subject) => subject.NotNull().Items.All(x => x.StatusCode.IsOk());
    public static bool IsError(this GraphCommandResults subject) => subject.NotNull().Items.Any(x => x.StatusCode.IsError());
}
