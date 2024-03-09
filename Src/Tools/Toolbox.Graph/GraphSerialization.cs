using Toolbox.Extensions;

namespace Toolbox.Graph;

public class GraphSerialization
{
    public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
}


public static class GraphSerializationExtensions
{
    public static string ToJson(this GraphMap subject) => subject.ToSerialization().ToJson();

    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        Nodes = subject.Nodes.ToArray(),
        Edges = subject.Edges.ToArray(),
    };

    public static GraphMap FromSerialization(this GraphSerialization subject) => new GraphMap(subject.Nodes, subject.Edges);
}
