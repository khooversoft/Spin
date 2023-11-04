using Toolbox.Extensions;

namespace Toolbox.Data;

public class GraphSerialization
{
    public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
}


public static class GraphSerializationExtensions
{
    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        Nodes = subject.Nodes.ToArray(),
        Edges = subject.Edges.ToArray(),
    };

    public static GraphSerialization ToSerialization<T>(this GraphMap subject) where T : notnull => new GraphSerialization
    {
        Nodes = subject.Nodes.ToArray(),
        Edges = subject.Edges.ToArray(),
    };

    public static string ToJson(this GraphMap subject)
    {
        var temp = new GraphSerialization
        {
            Nodes = subject.Nodes.ToArray(),
            Edges = subject.Edges.ToArray(),
        };

        return temp.ToJson();
    }

    public static GraphMap FromSerialization(this GraphSerialization subject) => new GraphMap(subject.Nodes, subject.Edges);
}
