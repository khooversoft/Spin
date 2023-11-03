using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Graph;

public record QueryResult
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }

    public GraphMap<string>? Map { get; init; }
    public List<IGraphCommon>? Current { get; init; }
    public Dictionary<string, IGraphCommon>? Alias { get; init; }
}


public class GraphQuery
{
    private readonly GraphMap<string> _map;
    private readonly Dictionary<string, List<IGraphCommon>> _alias = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IGraphCommon> _current = new List<IGraphCommon>();

    public GraphQuery(GraphMap<string> map) => _map = map.NotNull();

    public QueryResult Search(string graphQuery)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return new QueryResult { StatusCode = result.StatusCode, Error = result.Error };

        var stack = result.Return().Reverse().ToStack();
        var query = _map.Query();
        bool first = true;

        while (stack.TryPop(out var graphQL))
        {
            switch (graphQL)
            {
                //case GraphNodeQuery<string> node:
                //    query = first switch
                //    {
                //        true => query.Nodes(x => node.IsMatch(x)),
                //        false => query.HasNode(x => node.IsMatch(x)),
                //    };
                //    break;

                //case GraphEdgeQuery<string> edge:
                //    query = first switch
                //    {
                //        true => query.Edges(x => node.IsMatch(x)),
                //        false => query.HasNode(x => node.IsMatch(x)),
                //    };
                //    break;
            }

            first = false;
            //if( )
        }

        return null!;
    }
}

//public static class GraphQueryExtension
//{
//    public static QueryContext1<T> Query<T>(this GraphMap<T> subject, string rawData) where T : notnull => new QueryContext1<T> { Map = subject.NotNull() };

//    public static QueryContext1<T> Nodes<T>(this QueryContext1<T> subject, Func<GraphNode<T>, bool>? predicate = null) where T : notnull
//    {
//        subject.NotNull();

//        var result = subject with
//        {
//            Nodes = subject.Map.Nodes.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
//            Edges = Array.Empty<GraphEdge<T>>(),
//        };

//        return result;
//    }
//}
