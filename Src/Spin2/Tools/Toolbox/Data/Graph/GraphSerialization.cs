using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public class GraphSerialization : GraphSerialization<string>
{
}

public class GraphSerialization<T>
    where T : notnull
{
    public IReadOnlyList<GraphNode<T>> Nodes { get; init; } = Array.Empty<GraphNode<T>>();
    public IReadOnlyList<GraphEdge<T>> Edges { get; init; } = Array.Empty<GraphEdge<T>>();
}


public static class GraphSerializationExtensions
{
    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        Nodes = subject.Nodes.ToArray(),
        Edges = subject.Edges.ToArray(),
    };

    public static GraphSerialization<T> ToSerialization<T>(this GraphMap<T> subject) where T : notnull => new GraphSerialization<T>
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

    public static string ToJson<T>(this GraphMap<T> subject) where T : notnull
    {
        var temp = new GraphSerialization<T>
        {
            Nodes = subject.Nodes.ToArray(),
            Edges = subject.Edges.ToArray(),
        };

        return temp.ToJson();
    }

    public static GraphMap FromSerialization(this GraphSerialization subject) => new GraphMap(subject.Nodes, subject.Edges);
    public static GraphMap<T> FromSerialization<T>(this GraphSerialization<T> subject) where T : notnull => new GraphMap<T>(subject.Nodes, subject.Edges);
}
