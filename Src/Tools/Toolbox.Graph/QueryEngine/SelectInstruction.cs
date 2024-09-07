using System.Collections.Immutable;
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
                GiEdgeSelect edgeSelect => StatusCode.BadRequest,
                GiLeftJoin leftJoin => StatusCode.BadRequest,
                GiFullJoin fullJoin => StatusCode.BadRequest,
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
        var nodes = findNodes(giNodeSelect.Key).Where(x => IsMatch(giNodeSelect, x));

        var queryResult = new QueryResult
        {
            Option = StatusCode.OK,
            Alias = giNodeSelect.Alias,
            Nodes = nodes.ToImmutableArray(),
        };

        pContext.AddQueryResult(queryResult);
        return StatusCode.OK;


        IEnumerable<GraphNode> findNodes(string? key) => key switch
        {
            null => [],
            string v when v.IndexOfAny(['*', '?'], 0) < 0 => pContext.GraphContext.Map.Nodes.TryGetValue(v, out var gn) ? [gn] : [],
            string v => [.. pContext.GraphContext.Map.Nodes.Where(x => IsMatch(giNodeSelect, x))],
        };
    }

    private static async Task<Option> ReturnNames(GiReturnNames giReturnNames, QueryExecutionContext pContext)
    {
        var latest = pContext.GetLatestQueryResult();
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
