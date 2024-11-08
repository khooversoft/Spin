using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class NodeInstruction
{
    public static async Task<Option> Process(GiNode giNode, QueryExecutionContext pContext)
    {
        pContext.TrxContext.Context.Location().LogInformation("Process giNode={giNode}", giNode);

        var result = giNode.ChangeType switch
        {
            GiChangeType.Add => await Add(giNode, pContext),
            GiChangeType.Set => await Set(giNode, pContext),
            GiChangeType.Delete => await Delete(giNode, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        result.LogStatus(pContext.TrxContext.Context, "Completed processing of giNode={giNode}", [giNode]);
        return result;
    }

    private static async Task<Option> Add(GiNode giNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.TrxContext.Context.LogInformation("Adding giNode={giNode}", giNode);

        var dataMapOption = await NodeDataTool.AddData(giNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

        IReadOnlyDictionary<string, string?> tags = giNode.Tags.RemoveDeleteCommands();
        IReadOnlyDictionary<string, string?> foreignKeys = giNode.ForeignKeys.RemoveDeleteCommands();

        var graphNode = new GraphNode(
            giNode.Key,
            tags,
            DateTime.UtcNow,
            dataMap,
            giNode.Indexes.RemoveDeleteCommands(),
            foreignKeys
            );

        var graphResult = pContext.TrxContext.Map.Nodes.Add(graphNode, pContext.TrxContext);
        if (graphResult.IsError()) return graphResult;

        var fk = AddForeignKeys(giNode.Key, foreignKeys, tags, pContext);
        if (fk.IsError()) return fk;

        pContext.TrxContext.Context.LogInformation("Added giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Delete(GiNode giNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.TrxContext.Context.LogInformation("Deleting giNode={giNode}", giNode);

        if (pContext.TrxContext.Map.Nodes.TryGetValue(giNode.Key, out var readGraphNode))
        {
            var dataMapOption = await NodeDataTool.DeleteData([readGraphNode], pContext.TrxContext);
            if (dataMapOption.IsError()) return dataMapOption;
        }

        var graphResult = pContext.TrxContext.Map.Nodes.Remove(giNode.Key, pContext.TrxContext);
        if (!giNode.IfExist && graphResult.IsError()) return graphResult;

        pContext.TrxContext.Context.LogInformation("Deleting giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Set(GiNode giNode, QueryExecutionContext pContext)
    {
        if (!pContext.TrxContext.Map.Nodes.TryGetValue(giNode.Key, out var currentGraphNode))
        {
            pContext.TrxContext.Context.LogInformation("Node key={key} not found, adding node for upsert", giNode.Key);
            return await Add(giNode, pContext);
        }

        pContext.TrxContext.Context.LogInformation("Updating giNode={giNode}", giNode);

        var dataMapOption = await NodeDataTool.MergeData(giNode, currentGraphNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

        var tags = TagsTool.Merge(giNode.Tags, currentGraphNode.Tags);
        var foreignKeys = TagsTool.Merge(giNode.ForeignKeys, currentGraphNode.ForeignKeys);

        var graphNode = new GraphNode(
            giNode.Key,
            tags,
            currentGraphNode.CreatedDate,
            dataMap,
            giNode.Indexes.MergeCommands(currentGraphNode.Indexes),
            foreignKeys
            );

        var updateOption = pContext.TrxContext.Map.Nodes.Set(graphNode, pContext.TrxContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.TrxContext.Context, "Failed to upsert node nodeKey={nodeKey}", [giNode.Key]);

        var fkAdd = AddForeignKeys(giNode.Key, foreignKeys, tags, pContext);
        if (fkAdd.IsError()) return fkAdd;

        var fkRemove = RemoveForeignKeys(giNode.Key, foreignKeys, giNode.ForeignKeys.GetTagDeleteCommands(), tags, giNode.Tags.GetTagDeleteCommands(), pContext);
        if (fkRemove.IsError()) return fkRemove;

        return StatusCode.OK;
    }

    private static Option AddForeignKeys(
        string fromKey,
        IReadOnlyDictionary<string, string?> foreignKeys,
        IReadOnlyDictionary<string, string?> tags,
        QueryExecutionContext pContext
        )
    {
        fromKey.NotEmpty();
        tags.NotNull();
        foreignKeys.NotNull();
        pContext.NotNull();

        if (foreignKeys.Count == 0) return StatusCode.OK;

        // Valid tags based on foreignKey as edgeType
        var fkTags = tags
            .Where(x => x.Value.IsNotEmpty())
            .SelectMany(x => foreignKeys.Select(y => (y.Key, y.Value) switch
            {
                (string edgeType, null) when edgeType.EqualsIgnoreCase(x.Key) => (Key: edgeType, Value: x.Value),
                (string edgeType, string matchTo) when x.Key.Like(matchTo) => (Key: edgeType, Value: x.Value.NotEmpty()),
                _ => (Key: null!, Value: null!),
            }))
            .Where(x => x.Key.IsNotEmpty())
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.NotEmpty()))
            .ToArray();

        //var fkTags = foreignKeys
        //    .Join(tags, x => x.Key, y => y.Key, (fk, t) => t, StringComparer.OrdinalIgnoreCase)
        //    .Where(x => x.Value.IsNotEmpty())
        //    .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.NotEmpty()))
        //    .ToArray();

        var missingEdges = fkTags
            .Where(x => x.Value.IsNotEmpty())
            .Select(x => new GraphEdgePrimaryKey(fromKey, x.Value.NotEmpty(), x.Key))
            .Where(x => !pContext.TrxContext.Map.Edges.ContainsKey(x))
            .Select(x => new GraphEdge(x.FromKey, x.ToKey, x.EdgeType))
            .ToArray();

        var allStatus = missingEdges
            .Select(x => pContext.TrxContext.Map.Edges.Add(x, pContext.TrxContext))
            .ToArray();

        var status = ScanOptions(allStatus);
        pContext.TrxContext.Map.Meter.Node.ForeignKeyAdd(missingEdges.Length > 0 && status.IsOk());
        return status;
    }

    private static Option RemoveForeignKeys(
        string fromKey,
        IReadOnlyDictionary<string, string?> foreignKeys,
        IReadOnlyList<string> removedForeignKeys,
        IReadOnlyDictionary<string, string?> tags,
        IReadOnlyList<string> removedTags,
        QueryExecutionContext pContext
        )
    {
        fromKey.NotEmpty();
        foreignKeys.NotNull();
        removedForeignKeys.NotNull();
        pContext.NotNull();

        var fkTags = tags
            .Where(x => x.Value.IsNotEmpty())
            .SelectMany(x => foreignKeys.Select(y => (y.Key, y.Value) switch
            {
                (string edgeType, null) when edgeType.EqualsIgnoreCase(x.Key) => (Key: x.Key, ToKey: x.Value, EdgeType: edgeType),
                (string edgeType, string matchTo) when x.Key.Like(matchTo) => (Key: x.Key, ToKey: x.Value.NotEmpty(), EdgeType: edgeType),
                _ => (Key: null!, ToKey: null!, EdgeType: null!),
            }))
            .Where(x => x.Key.IsNotEmpty())
            .ToArray();

        var removedEdgeTypes = removedForeignKeys
            .Concat(removedTags)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        removedEdgeTypes = removedEdgeTypes
            .Concat(fkTags.Where(x => removedEdgeTypes.Contains(x.Key)).Select(x => x.EdgeType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (removedEdgeTypes.Count == 0 && removedForeignKeys.Count == 0) return StatusCode.OK;

        var currentEdgesNotValid = pContext.TrxContext.Map.Edges
            .LookupByFromKey([fromKey])
            .Where(x => removedEdgeTypes.Contains(x.EdgeType) || (isEdgeTypeValid(x.EdgeType) && isInTagList(x.ToKey, x.EdgeType)))
            .ToArray();

        var allStatus = currentEdgesNotValid
            .Select(x => pContext.TrxContext.Map.Edges.Remove(x, pContext.TrxContext))
            .ToArray();

        var status = ScanOptions(allStatus);
        pContext.TrxContext.Map.Meter.Node.ForeignKeyRemove(currentEdgesNotValid.Length > 0 && status.IsOk());
        return status;

        bool isEdgeTypeValid(string edgeType) => foreignKeys.ContainsKey(edgeType);
        bool isInTagList(string toKey, string edgeType) => tags.TryGetValue(edgeType, out var value) && !value.EqualsIgnoreCase(toKey);
    }

    private static Option ScanOptions(IEnumerable<Option> options) => options.Aggregate(
        new Option(StatusCode.OK),
        (a, x) => (a.IsOk(), x.IsOk()) switch
        {
            (true, true) => StatusCode.OK,
            (false, _) => a,
            (true, _) => x,
        });
}
