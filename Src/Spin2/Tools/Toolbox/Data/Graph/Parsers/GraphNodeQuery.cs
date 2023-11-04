using Toolbox.Extensions;

namespace Toolbox.Data;

public record GraphNodeQuery : IGraphQL
{
    public string? Key { get; init; }
    public string? Tags { get; init; }
    public string? Alias { get; init; }
}


public static class GraphNodeQueryExtensions
{
    public static bool IsMatch(this GraphNodeQuery subject, GraphNode node)
    {
        bool isKey = subject.Key == null || subject.Key.IsMatch(node.Key);
        bool isTag = subject.Tags == null || node.Tags.Has(subject.Tags);

        return isKey && isTag;
    }
}