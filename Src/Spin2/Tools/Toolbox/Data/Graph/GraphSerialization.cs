using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

internal class GraphSerialization<T>
    where T : notnull
{
    public IReadOnlyList<GraphNode<T>> Nodes { get; init; } = Array.Empty<GraphNode<T>>();
    public IReadOnlyList<GraphEdge<T>> Edges { get; init; } = Array.Empty<GraphEdge<T>>();
}

public static class GraphMap
{
    public static string ToJson<T>(this GraphMap<T> subject)
    where T : notnull
    {
        var temp = new GraphSerialization<T>
        {
            Nodes = subject.Nodes.ToArray(),
            Edges = subject.Edges.ToArray(),
        };

        return temp.ToJson();
    }

    public static GraphMap<T> FromJson<T>(string json) where T : notnull
    {
        var temp = json.ToObject<GraphSerialization<T>>().NotNull();
        var map = new GraphMap<T>(temp.Nodes, temp.Edges);
        return map;
    }
}
