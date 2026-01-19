using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class SelectInstruction
{
    public static async Task<Option> Process(GiSelect giSelect, GraphTrxContext pContext)
    {
        pContext.Logger.LogDebug("Process giSelect={giSelect}", giSelect);
        var result = await Select(giSelect.Instructions, pContext);
        pContext.Logger.LogStatus(result, "Select return status");

        pContext.Logger.LogDebug("Completed processing of giSelect={giSelect}", giSelect);
        return result;
    }

    public static async Task<Option> Select(IReadOnlyList<ISelectInstruction> instructions, GraphTrxContext pContext)
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
                GiRightJoin rightJoin => StatusCode.OK.Action(_ => pContext.LastJoin.Set(rightJoin)),
                GiReturnNames returnNames => await ReturnNames(returnNames, pContext),

                _ => (StatusCode.BadRequest, $"Unknown instruction type={selectInstruction.GetType()}"),
            };

            if (result.IsError())
            {
                pContext.Logger.LogStatus(result, "Error in processing of selectInstruction={selectInstruction}", [selectInstruction]);
                break;
            }
        }

        pContext.Logger.LogDebug("Completed processing select, count={count}", instructions.Count);
        return result;
    }

    private static Option SelectNode(GiNodeSelect giNodeSelect, GraphTrxContext pContext)
    {
        IEnumerable<GraphNode> nodes = pContext.LastJoin.GetAndClear() switch
        {
            null => findRootNodes(),
            GiLeftJoin left => pContext.GetLastQueryResult().NotNull().Edges.Select(x => pContext.GetMap().Nodes[x.ToKey]),
            GiRightJoin right => pContext.GetLastQueryResult().NotNull().Edges.Select(x => pContext.GetMap().Nodes[x.FromKey]),
            GiFullJoin full => pContext.GetLastQueryResult().NotNull().Edges.SelectMany(x => (IEnumerable<GraphNode>)[
                pContext.GetMap().Nodes[x.FromKey],
                pContext.GetMap().Nodes[x.ToKey]
                ]),

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
            var v when v.Tags.Any(x => x.Key == "*") => pContext.GetMap().Nodes.Action(x => x.AddIndexScan()),
            { Key: string v } when !HasWildCard(v) => pContext.GetMap().Nodes.TryGetValue(v, out var gn) ? [gn] : [],
            { Tags: { Count: > 0 } v } => v.SelectMany(x => pContext.GetMap().Nodes.LookupTaggedNodes(x.Key)),
            _ => pContext.GetMap().Nodes.Action(x => x.AddIndexScan()),
        };
    }

    private static Option SelectEdge(GiEdgeSelect giEdgeSelect, GraphTrxContext pContext)
    {
        IEnumerable<GraphEdge> edges = pContext.LastJoin.GetAndClear() switch
        {
            null => findRootEdges(),
            GiLeftJoin left => lookupFromKeys(),
            GiRightJoin right => lookupToKeys(),
            GiFullJoin full => lookupNodeKeys(),

            _ => throw new UnreachableException()
        };

        edges = edges.Where(x => IsMatch(giEdgeSelect, x));

        var queryResult = new QueryResult
        {
            Option = StatusCode.OK,
            Alias = giEdgeSelect.Alias,
            Edges = edges
                .GroupBy(x => x.GetPrimaryKey(), GraphEdgePrimaryKeyComparer.Default)
                .Select(x => x.First())
                .ToImmutableArray(),
        };

        pContext.AddQueryResult(queryResult);
        return StatusCode.OK;

        IEnumerable<GraphEdge> findRootEdges() => giEdgeSelect switch
        {
            var v when v.Tags.Any(x => x.Key == "*") => pContext.GetMap().Edges.Action(x => pContext.GetMap().Edges.AddIndexScan()),
            { From: string v1, To: string v2, Type: string v3 } when !HasWildCard(v1) && !HasWildCard(v2) && !HasWildCard(v3) => lookupEdge(v1, v2, v3),
            { From: string v1 } when !HasWildCard(v1) => pContext.GetMap().Edges.LookupByFromKeyExpand([v1]),
            { To: string v2 } when !HasWildCard(v2) => pContext.GetMap().Edges.LookupByToKeyExpand([v2]),
            { Type: string v3 } when !HasWildCard(v3) => pContext.GetMap().Edges.LookupByEdgeTypeExpand([v3]),
            { Tags: { Count: > 0 } v } => v.SelectMany(x => pContext.GetMap().Edges.LookupTagExpand(x.Key)),
            _ => pContext.GetMap().Edges,
        };

        IEnumerable<GraphEdge> lookupEdge(string from, string to, string edgeType) =>
            pContext.GetMap().Edges.TryGetValue((from, to, edgeType), out GraphEdge? ge) ? [ge.NotNull()] : [];

        IEnumerable<string> getLastNodeResultKeys() => pContext.GetLastQueryResult().NotNull().Nodes.Select(x => x.Key);
        IEnumerable<GraphEdge> lookupNodeKeys() => pContext.GetMap().Edges.LookupByNodeKeyExpand(getLastNodeResultKeys());
        IEnumerable<GraphEdge> lookupFromKeys() => pContext.GetMap().Edges.LookupByFromKeyExpand(getLastNodeResultKeys());
        IEnumerable<GraphEdge> lookupToKeys() => pContext.GetMap().Edges.LookupByToKeyExpand(getLastNodeResultKeys());
    }

    private static async Task<Option> ReturnNames(GiReturnNames giReturnNames, GraphTrxContext pContext)
    {
        var latest = pContext.GetLastQueryResult();
        if (latest == null)
        {
            pContext.Logger.LogError("No data set found for giReturnNames");
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
                pContext.Logger.LogError("Cannot get data for fileId={item.FileId} for nodeKey={nodeKey}", item.FileId, item.NodeKey);
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
