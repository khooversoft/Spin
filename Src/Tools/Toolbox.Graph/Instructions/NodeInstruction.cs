using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class NodeInstruction
{
    public static async Task<Option> Process(GiNode giNode, GraphTrxContext pContext)
    {
        pContext.Logger.LogDebug("Process giNode={giNode}, changeType={changeType}", giNode, giNode.ChangeType);

        var result = giNode.ChangeType switch
        {
            GiChangeType.Add => await Add(giNode, pContext),
            GiChangeType.Set => await Set(giNode, pContext),
            GiChangeType.Delete => await Delete(giNode, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        pContext.Logger.LogStatus(result, "Completed processing of giNode={giNode}", [giNode]);
        return result;
    }

    private static async Task<Option> Add(GiNode giNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.Logger.LogDebug("Adding giNode={giNode}", giNode);

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

        var graphResult = pContext.GetMap().Nodes.Add(graphNode, pContext);
        if (graphResult.IsError()) return graphResult;

        var fk = AddForeignKeys(giNode.Key, foreignKeys, tags, pContext);
        if (fk.IsError()) return fk;

        pContext.Logger.LogDebug("Added giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Delete(GiNode giNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.Logger.LogDebug("Deleting giNode={giNode}", giNode);

        if (pContext.GetMap().Nodes.TryGetValue(giNode.Key, out var readGraphNode))
        {
            var dataMapOption = await NodeDataTool.DeleteData([readGraphNode], pContext);
            if (dataMapOption.IsError()) return dataMapOption;
        }

        var graphResult = pContext.GetMap().Nodes.Remove(giNode.Key, pContext);
        if (!giNode.IfExist && graphResult.IsError()) return graphResult;

        pContext.Logger.LogDebug("Deleting giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Set(GiNode giNode, GraphTrxContext pContext)
    {
        if (!pContext.GetMap().Nodes.TryGetValue(giNode.Key, out var currentGraphNode))
        {
            pContext.Logger.LogDebug("Node key={key} not found, adding node for upsert", giNode.Key);
            return await Add(giNode, pContext);
        }

        pContext.Logger.LogDebug("Updating giNode={giNode}", giNode);

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

        var updateOption = pContext.GetMap().Nodes.Set(graphNode, pContext);
        if (updateOption.IsError()) return pContext.Logger.LogStatus(updateOption, "Failed to set node nodeKey={nodeKey}", [giNode.Key]);

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
        GraphTrxContext pContext
        )
    {
        fromKey.NotEmpty();
        tags.NotNull();
        foreignKeys.NotNull();
        pContext.NotNull();

        if (foreignKeys.Count == 0) return StatusCode.OK;

        pContext.Logger.LogDebug(
            "Add foreign keys fromKey={fromKey}, foreignKeys={foreignKeys}, tags={tags}",
            fromKey,
            foreignKeys.ToTagsString(),
            tags.ToTagsString()
            );

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

        pContext.Logger.LogDebug("Foreign keys fromKey={fromKey}, foreignKeys={foreignKeys}", fromKey, fkTags);

        var missingEdges = fkTags
            .Where(x => x.Value.IsNotEmpty())
            .Select(x => new GraphEdgePrimaryKey(fromKey, x.Value.NotEmpty(), x.Key))
            .Where(x => !pContext.GetMap().Edges.ContainsKey(x))
            .Select(x => new GraphEdge(x.FromKey, x.ToKey, x.EdgeType))
            .ToArray();

        pContext.Logger.LogDebug(
            "Missing edges fromKey={fromKey}, missingEdges={missingEdges}",
            fromKey,
            missingEdges.Select(x => x.ToString()).Join(',')
            );

        var allStatus = missingEdges
            .Select(x => pContext.GetMap().Edges.Add(x, pContext))
            .ToArray();

        var status = ScanOptions(allStatus);
        return status;
    }

    private static Option RemoveForeignKeys(
        string fromKey,
        IReadOnlyDictionary<string, string?> foreignKeys,
        IReadOnlyList<string> removedForeignKeys,
        IReadOnlyDictionary<string, string?> tags,
        IReadOnlyList<string> removedTags,
        GraphTrxContext pContext
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

        var currentEdgesNotValid = pContext.GetMap().Edges
            .LookupByFromKey([fromKey])
            .Where(x => removedEdgeTypes.Contains(x.EdgeType) || (isEdgeTypeValid(x.EdgeType) && isInTagList(x.ToKey, x.EdgeType)))
            .ToArray();

        var allStatus = currentEdgesNotValid
            .Select(x => pContext.GetMap().Edges.Remove(x, pContext))
            .ToArray();

        var status = ScanOptions(allStatus);
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
