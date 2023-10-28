using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryQuery
{
    [Id(0)] public string? NodeKey { get; init; }
    [Id(1)] public string? NodeTags { get; init; }
    [Id(2)] public string? FromKey { get; init; }
    [Id(3)] public string? ToKey { get; init; }
    [Id(4)] public string? EdgeType { get; init; }
    [Id(5)] public string? EdgeTags { get; init; }
}


public static class DirectoryQueryExtensions
{
    public static bool IsQueryEmpty(this DirectoryQuery subject)
    {
        if (subject == null) return false;

        return subject.NodeKey == null &&
            subject.NodeTags == null &&
            subject.FromKey == null &&
            subject.ToKey == null &&
            subject.EdgeType == null &&
            subject.NodeTags == null;
    }

    public static bool IsMatch(this DirectoryQuery subject, IGraphNode<string> node)
    {
        subject.NotNull();

        if (subject.NodeKey != null && !node.Key.Match(subject.NodeKey)) return false;
        if (subject.NodeTags != null && !node.Tags.Has(subject.NodeTags)) return false;

        return true;
    }

    public static bool IsMatch(this DirectoryQuery subject, IGraphEdge<string> edge)
    {
        subject.NotNull();

        if (subject.FromKey != null && !edge.FromKey.Match(subject.FromKey)) return false;
        if (subject.ToKey != null && !edge.ToKey.Match(subject.ToKey)) return false;
        if (subject.EdgeType != null && !edge.EdgeType.Match(subject.EdgeType)) return false;
        if (subject.EdgeTags != null && !edge.Tags.Has(subject.EdgeTags)) return false;

        return true;
    }
}