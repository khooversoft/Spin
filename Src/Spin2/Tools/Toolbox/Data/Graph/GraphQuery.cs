using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Data.Graph;

public readonly record struct QueryContext
{
    [SetsRequiredMembers]
    public QueryContext() { }

    public GraphMap<string> Map { get; init; } = null!;
    public Stack<IGraphCommon> Stack { get; init; } = new Stack<IGraphCommon>();
    public Dictionary<string, IGraphCommon> Alias { get; init; } = new Dictionary<string, IGraphCommon>(StringComparer.OrdinalIgnoreCase);
}

public class GraphQuery
{
}

public static class GraphQueryExtension
{
    public static QueryContext1<T> Query<T>(this GraphMap<T> subject, string rawData) where T : notnull => new QueryContext1<T> { Map = subject.NotNull() };

    public static QueryContext1<T> Nodes<T>(this QueryContext1<T> subject, Func<GraphNode<T>, bool>? predicate = null) where T : notnull
    {
        subject.NotNull();

        var result = subject with
        {
            Nodes = subject.Map.Nodes.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
            Edges = Array.Empty<GraphEdge<T>>(),
        };

        return result;
    }
}
