using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryQuery
{
    [Id(0)] public string? NodeKey { get; init; }
    [Id(1)] public string? MatchNodeTags { get; init; }
    [Id(2)] public string? FromKey { get; init; }
    [Id(3)] public string? ToKey { get; init; }
    [Id(4)] public string? MatchEdgeType { get; init; }
    [Id(5)] public string? MatchEdgeTags { get; init; }
}


public static class DirectoryQueryExtensions
{
    public static bool IsQueryEmpty(this DirectoryQuery subject)
    {
        if (subject == null) return false;

        return subject.NodeKey == null &&
            subject.MatchNodeTags == null &&
            subject.FromKey == null &&
            subject.ToKey == null &&
            subject.MatchEdgeType == null &&
            subject.MatchNodeTags == null;
    }

    public static bool IsMatch(this DirectoryQuery subject, IGraphNode<string> node)
    {
        subject.NotNull();

        if (subject.NodeKey != null && !GraphEdgeTool.IsKeysEqual(subject.NodeKey, node.Key)) return false;
        if (subject.MatchNodeTags != null && !node.Tags.Has(subject.MatchNodeTags)) return false;

        return true;
    }

    public static bool IsMatch(this DirectoryQuery subject, IGraphEdge<string> edge)
    {
        subject.NotNull();

        if (subject.FromKey != null && !GraphEdgeTool.IsKeysEqual(subject.FromKey, edge.FromKey)) return false;
        if (subject.ToKey != null && !GraphEdgeTool.IsKeysEqual(subject.ToKey, edge.ToKey)) return false;
        if (subject.MatchEdgeType != null && edge.EdgeType.Match(subject.MatchEdgeType)) return false;
        if (subject.MatchEdgeTags != null && !edge.Tags.Has(subject.MatchEdgeTags)) return false;

        return true;
    }
}