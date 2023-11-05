﻿using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphQuery
{
    private readonly GraphMap _map;
    public GraphQuery(GraphMap map) => _map = map.NotNull();

    public GraphQueryResult Execute(string graphQuery)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return new GraphQueryResult { StatusCode = result.StatusCode, Error = result.Error };

        bool first = true;
        var stack = result.Return().Reverse().ToStack();
        SearchContext search = _map.Search();
        IReadOnlyList<IGraphCommon> current = null!;
        Dictionary<string, IReadOnlyList<IGraphCommon>> aliasDict = new(StringComparer.OrdinalIgnoreCase);

        while (stack.TryPop(out var graphQL))
        {
            switch (graphQL)
            {
                case GraphNodeQuery node:
                    search = first switch
                    {
                        true => search.Nodes(x => node.IsMatch(x)),
                        false => search.HasNode(x => node.IsMatch(x)),
                    };

                    update(search, node.Alias);
                    break;

                case GraphEdgeQuery edge:
                    search = first switch
                    {
                        true => search.Edges(x => edge.IsMatch(x)),
                        false => search.HasEdge(x => edge.IsMatch(x)),
                    };

                    update(search, edge.Alias);
                    break;
            }

            first = false;
        }

        return new GraphQueryResult
        {
            StatusCode = StatusCode.OK,
            Items = current,
            Alias = aliasDict,
        };

        void update(SearchContext searchContext, string? alias)
        {
            current = search.LastSearch switch
            {
                SearchContext.LastSearchType.Node => searchContext.Nodes.ToArray(),
                SearchContext.LastSearchType.Edge => searchContext.Edges.ToArray(),
                _ => throw new UnreachableException(),
            };

            if (alias.IsNotEmpty()) aliasDict[alias] = current;
        }
    }
}

public static class GraphQueryExtension
{
    public static GraphQuery Query(this GraphMap subject) => new GraphQuery(subject);
}
