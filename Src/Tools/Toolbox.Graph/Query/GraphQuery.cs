using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphQuery
{
    public static GraphQueryResult Process(GraphMap map, IReadOnlyList<IGraphQL> instructions)
    {
        instructions.NotNull();

        var stack = instructions.Reverse().ToStack();
        SearchContext search = map.Search();
        IReadOnlyList<IGraphCommon> current = null!;
        Dictionary<string, IReadOnlyList<IGraphCommon>> aliasDict = new(StringComparer.OrdinalIgnoreCase);

        bool first = true;
        while (stack.TryPop(out var graphQL))
        {
            switch (graphQL)
            {
                case GraphNodeSearch node:
                    search = first switch
                    {
                        true => search.Nodes(x => node.IsMatch(x)),
                        false => search.HasNode(x => node.IsMatch(x)),
                    };

                    update(search, node.Alias);
                    break;

                case GraphEdgeSearch edge:
                    search = first switch
                    {
                        true => search.Edges(x => edge.IsMatch(x)),
                        false => search.HasEdge(x => edge.IsMatch(x), edge.Direction),
                    };

                    update(search, edge.Alias);
                    break;

                case GsSelect select:
                    select.Search.Reverse().ForEach(x => stack.Push(x));
                    continue;

                default:
                    throw new ArgumentException($"Unknown instruction={graphQL.GetType().FullName}");
            }

            first = false;
        }

        return new GraphQueryResult
        {
            Status = StatusCode.OK,
            Items = current.ToImmutableArray(),
            Alias = aliasDict.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray(), StringComparer.OrdinalIgnoreCase),
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

