using Toolbox.Extensions;

namespace Toolbox.Data;

public record GraphNodeSearch : IGraphQL
{
    public string? Key { get; init; }
    public string? Tags { get; init; }
    public string? Alias { get; init; }
}


public static class GraphNodeQueryExtensions
{
    public static bool IsMatch(this GraphNodeSearch subject, GraphNode node)
    {
        bool isKey = subject.Key == null || node.Key.IsMatch(subject.Key);
        bool isTag = subject.Tags == null || node.Tags.Has(subject.Tags);

        return isKey && isTag;
    }
}