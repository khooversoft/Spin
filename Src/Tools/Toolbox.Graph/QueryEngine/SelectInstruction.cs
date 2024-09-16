using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class SelectInstruction
{
    public static async Task<Option> Process(GiSelect giSelect, QueryExecutionContext pContext)
    {
        pContext.GraphContext.Context.Location().LogInformation("Process giSelect={giSelect}", giSelect);
        var result = await Select(giSelect.Instructions, pContext);
        result.LogStatus(pContext.GraphContext.Context, "Select return status");

        pContext.GraphContext.Context.LogInformation("Completed processing of giSelect={giSelect}", giSelect);
        return result;
    }

    public static async Task<Option> Select(IReadOnlyList<ISelectInstruction> instructions, QueryExecutionContext pContext)
    {
        var stack = new Stack<ISelectInstruction>(instructions.NotNull().Reverse());
        Option result = (StatusCode.BadRequest, "No instructions");

        while (stack.TryPop(out var selectInstruction))
        {
            result = selectInstruction switch
            {
                GiNodeSelect nodeSelect => SelectNode(nodeSelect, pContext),
                GiEdgeSelect edgeSelect => SelectEdge(edgeSelect, pContext),
                GiLeftJoin leftJoin => StatusCode.OK.Action(_ => pContext.LastJoin.Set(leftJoin)),
                GiFullJoin fullJoin => StatusCode.OK.Action(_ => pContext.LastJoin.Set(fullJoin)),
                GiReturnNames returnNames => await ReturnNames(returnNames, pContext),

                _ => (StatusCode.BadRequest, $"Unknown instuction type={selectInstruction.GetType()}"),
            };

            if (result.IsError())
            {
                result.LogStatus(pContext.GraphContext.Context, $"Error in processing of selectInstruction={selectInstruction}");
                break;
            }
        }

        pContext.GraphContext.Context.LogInformation("Completed processing select, count={count}", instructions.Count);
        return result;
    }

    private static Option SelectNode(GiNodeSelect giNodeSelect, QueryExecutionContext pContext)
    {
        IEnumerable<GraphNode> nodes = pContext.LastJoin.GetAndClear() switch
        {
            null => findRootNodes(),
            GiLeftJoin left => pContext.GetLastQueryResult().NotNull().Edges.Select(x => pContext.GraphContext.Map.Nodes[x.ToKey]),
            GiFullJoin full => pContext.GetLastQueryResult().NotNull().Edges.SelectMany(x => (IEnumerable<GraphNode>)[
                pContext.GraphContext.Map.Nodes[x.FromKey],
                pContext.GraphContext.Map.Nodes[x.ToKey] ]
                ),

            _ => throw new UnreachableException()
        };

        nodes = nodes.Where(x => IsMatch(giNodeSelect, x));

        var queryResult = new QueryResult
        {
            Option = StatusCode.OK,
            Alias = giNodeSelect.Alias,
            Nodes = nodes.GroupBy(x => x.Key).Select(x => x.First()).ToImmutableArray(),
        };

        pContext.AddQueryResult(queryResult);
        return StatusCode.OK;


        IEnumerable<GraphNode> findRootNodes() => giNodeSelect switch
        {
            var v when v.Tags.Any(x => x.Key == "*") => pContext.GraphContext.Map.Nodes,
            { Key: string v } when !HasWildCard(v) => pContext.GraphContext.Map.Nodes.TryGetValue(v, out var gn) ? [gn] : [],
            _ => pContext.GraphContext.Map.Nodes,
        };
    }

    private static Option SelectEdge(GiEdgeSelect giEdgeSelect, QueryExecutionContext pContext)
    {
        IEnumerable<GraphEdge> edges = pContext.LastJoin.GetAndClear() switch
        {
            null => findRootEdges(),
            GiLeftJoin left => lookupFromKeys(),
            GiFullJoin full => lookupNodeKeys(),

            _ => throw new UnreachableException()
        };

        edges = edges.Where(x => IsMatch(giEdgeSelect, x));

        var queryResult = new QueryResult
        {
            Option = StatusCode.OK,
            Alias = giEdgeSelect.Alias,
            Edges = edges.GroupBy(x => x.GetPrimaryKey(), GraphEdgePrimaryKeyComparer.Default).Select(x => x.First()).ToImmutableArray(),
        };

        pContext.AddQueryResult(queryResult);
        return StatusCode.OK;

        IEnumerable<GraphEdge> findRootEdges() => giEdgeSelect switch
        {
            var v when v.Tags.Any(x => x.Key == "*") => pContext.GraphContext.Map.Edges,
            { From: string v1, To: string v2, Type: string v3 } when !HasWildCard(v1) && !HasWildCard(v2) && !HasWildCard(v3) => lookupEdge(v1, v2, v3),
            _ => pContext.GraphContext.Map.Edges,
        };

        IEnumerable<GraphEdge> lookupEdge(string from, string to, string edgeType) =>
            pContext.GraphContext.Map.Edges.TryGetValue((from, to, edgeType), out GraphEdge? ge) ? [ge.NotNull()] : [];

        IEnumerable<string> getLastNodeResultKeys() => pContext.GetLastQueryResult().NotNull().Nodes.Select(x => x.Key);
        IEnumerable<GraphEdge> lookupNodeKeys() => pContext.GraphContext.Map.Edges.Lookup(pContext.GraphContext.Map.Edges.LookupByNodeKey(getLastNodeResultKeys()));
        IEnumerable<GraphEdge> lookupFromKeys() => pContext.GraphContext.Map.Edges.Lookup(pContext.GraphContext.Map.Edges.LookupByFromKey(getLastNodeResultKeys()));
    }

    private static async Task<Option> ReturnNames(GiReturnNames giReturnNames, QueryExecutionContext pContext)
    {
        var latest = pContext.GetLastQueryResult();
        if (latest == null)
        {
            pContext.GraphContext.Context.LogError("No data set found for giReturnNames={giReturnNames}");
            return (StatusCode.BadRequest, "No data set found");
        }

        IEnumerable<GraphLink> dataToReturnNames = latest.Nodes
            .SelectMany(x => x.DataMap, (o, i) => i.Value)
            .Where(x => giReturnNames.ReturnNames.Contains(x.Name));

        var seq = new Sequence<GraphLinkData>();
        foreach (var item in dataToReturnNames)
        {
            var readOption = await NodeDataTool.GetData(item, pContext);
            if (readOption.IsError())
            {
                pContext.GraphContext.Context.LogError("Cannot get data for fileId={item.FileId} for nodeKey={nodeKey}", item.FileId, item.NodeKey);
                continue;
            }

            seq += readOption.Return();
        }

        pContext.UpdateLastQueryResult(seq);
        return StatusCode.OK;
    }

    private static bool HasWildCard(string? value) => value switch
    {
        null => false,
        string v when v.IsEmpty() => false,
        string v when v.IndexOfAny(['*', '?'], 0) >= 0 => true,
        _ => false,
    };

    private static bool IsMatch(this GiNodeSelect subject, GraphNode node)
    {
        bool isKey = subject.Key == null || node.Key.Like(subject.Key);
        bool isTag = subject.Tags.Count == 0 || node.Tags.Has(subject.Tags);

        return isKey && isTag;
    }

    public static bool IsMatch(this GiEdgeSelect subject, GraphEdge edge)
    {
        bool isFromKey = subject.From == null || edge.FromKey.Like(subject.From);
        bool isToKey = subject.To == null || edge.ToKey.Like(subject.To);
        bool isEdgeType = subject.Type == null || edge.EdgeType.Like(subject.Type);
        bool isTag = subject.Tags.Count == 0 || edge.Tags.Has(subject.Tags);

        return isFromKey && isToKey && isEdgeType && isTag;
    }
}
