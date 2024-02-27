using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Data;

public record GraphNodeSearch : IGraphQL
{
    public string? Key { get; init; }
    public Tags Tags { get; init; } = new Tags();
    public string? Alias { get; init; }
}


public static class GraphNodeQueryExtensions
{
    public static bool IsMatch(this GraphNodeSearch subject, GraphNode node)
    {
        bool isKey = subject.Key == null || node.Key.IsMatch(subject.Key);
        bool isTag = subject.Tags.Count == 0 || node.Tags.Has(subject.Tags);

        return isKey && isTag;
    }
}