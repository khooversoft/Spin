using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public partial class GraphQueryExecute
{
    private async Task<Option> ProcessNode(GiNode giNode, GraphTrxContext pContext)
    {
        _logger.LogDebug("Process giNode={giNode}, changeType={changeType}", giNode, giNode.ChangeType);

        var result = giNode.ChangeType switch
        {
            GiChangeType.Add => await Add(giNode, pContext),
            GiChangeType.Set => await Set(giNode, pContext),
            GiChangeType.Delete => await Delete(giNode, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        _logger.LogStatus(result, "Completed processing of giNode={giNode}", [giNode]);
        return result;
    }

    private async Task<Option> Add(GiNode giNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        _logger.LogDebug("Adding giNode={giNode}", giNode);

        var dataMapOption = await AddData(giNode, pContext);
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

        var graphResult = _graphMapStore.GetMap().Nodes.Add(graphNode, pContext);
        if (graphResult.IsError()) return graphResult;

        var fk = AddForeignKeys(giNode.Key, foreignKeys, tags, pContext);
        if (fk.IsError()) return fk;

        _logger.LogDebug("Added giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private async Task<Option> Delete(GiNode giNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        _logger.LogDebug("Deleting giNode={giNode}", giNode);

        if (_graphMapStore.GetMap().Nodes.TryGetValue(giNode.Key, out var readGraphNode))
        {
            var dataMapOption = await DeleteData([readGraphNode], pContext);
            if (dataMapOption.IsError()) return dataMapOption;
        }

        var graphResult = _graphMapStore.GetMap().Nodes.Remove(giNode.Key, pContext);
        if (!giNode.IfExist && graphResult.IsError()) return graphResult;

        _logger.LogDebug("Deleting giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private async Task<Option> Set(GiNode giNode, GraphTrxContext pContext)
    {
        if (!_graphMapStore.GetMap().Nodes.TryGetValue(giNode.Key, out var currentGraphNode))
        {
            _logger.LogDebug("Node key={key} not found, adding node for upsert", giNode.Key);
            return await Add(giNode, pContext);
        }

        _logger.LogDebug("Updating giNode={giNode}", giNode);
        var dataMapOption = await MergeData(giNode, currentGraphNode, pContext);
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

        var updateOption = _graphMapStore.GetMap().Nodes.Set(graphNode, pContext);
        if (updateOption.IsError()) return _logger.LogStatus(updateOption, "Failed to set node nodeKey={nodeKey}", [giNode.Key]);

        var fkAdd = AddForeignKeys(giNode.Key, foreignKeys, tags, pContext);
        if (fkAdd.IsError()) return fkAdd;

        var fkRemove = RemoveForeignKeys(giNode.Key, foreignKeys, giNode.ForeignKeys.GetTagDeleteCommands(), tags, giNode.Tags.GetTagDeleteCommands(), pContext);
        if (fkRemove.IsError()) return fkRemove;

        return StatusCode.OK;
    }

    private Option AddForeignKeys(string fromKey, IReadOnlyDictionary<string, string?> foreignKeys, IReadOnlyDictionary<string, string?> tags, GraphTrxContext pContext)
    {
        fromKey.NotEmpty();
        tags.NotNull();
        foreignKeys.NotNull();
        pContext.NotNull();

        if (foreignKeys.Count == 0) return StatusCode.OK;

        _logger.LogDebug(
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

        _logger.LogDebug("Foreign keys fromKey={fromKey}, foreignKeys={foreignKeys}", fromKey, fkTags);

        var missingEdges = fkTags
            .Where(x => x.Value.IsNotEmpty())
            .Select(x => new GraphEdgePrimaryKey(fromKey, x.Value.NotEmpty(), x.Key))
            .Where(x => !_graphMapStore.GetMap().Edges.ContainsKey(x))
            .Select(x => new GraphEdge(x.FromKey, x.ToKey, x.EdgeType))
            .ToArray();

        _logger.LogDebug(
            "Missing edges fromKey={fromKey}, missingEdges={missingEdges}",
            fromKey,
            missingEdges.Select(x => x.ToString()).Join(',')
            );

        var allStatus = missingEdges
            .Select(x => _graphMapStore.GetMap().Edges.Add(x, pContext))
            .ToArray();

        var status = ScanOptions(allStatus);
        return status;
    }

    private Option RemoveForeignKeys(
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

        var currentEdgesNotValid = _graphMapStore.GetMap().Edges
            .LookupByFromKey([fromKey])
            .Where(x => removedEdgeTypes.Contains(x.EdgeType) || (isEdgeTypeValid(x.EdgeType) && isInTagList(x.ToKey, x.EdgeType)))
            .ToArray();

        var allStatus = currentEdgesNotValid
            .Select(x => _graphMapStore.GetMap().Edges.Remove(x, pContext))
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


    private async Task<Option<IReadOnlyList<GraphLink>>> AddData(GiNode giNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        var dataMap = giNode.GetLinkData();

        foreach (var item in dataMap)
        {
            var writeResult = await _graphMapStore.DataFileClient.Set(item.FileId, item.Data);
            if (writeResult.IsError()) return writeResult.ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        return dataMap.Select(x => x.ConvertTo()).ToImmutableArray();
    }

    private async Task<Option> DeleteData(IReadOnlyList<GraphNode> nodes, GraphTrxContext graphContext)
    {
        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            var result = await _graphMapStore.DataFileClient.Delete(fileId);
            if (result.IsError()) return result;
        }

        return StatusCode.OK;
    }

    private async Task<Option<GraphLinkData>> GetData(GraphLink graphLink, GraphTrxContext pContext)
    {
        var readOption = await _graphMapStore.DataFileClient.Get(graphLink.FileId);
        if (readOption.IsError()) return readOption.ToOptionStatus<GraphLinkData>();

        var result = graphLink.ConvertTo(readOption.Return());
        return result;
    }

    private async Task<Option<IReadOnlyList<GraphLink>>> MergeData(GiNode giNode, GraphNode graphNode, GraphTrxContext pContext)
    {
        giNode.NotNull();
        graphNode.NotNull();
        pContext.NotNull();

        var dataMapOption = await AddData(giNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption;

        var removeDataNames = giNode.Tags.GetTagDeleteCommands();
        var addedData = dataMapOption.Return();
        var currentData = graphNode.DataMap.Values.Where(x => !addedData.Any(y => y.Name.EqualsIgnoreCase(x.Name)));

        var dataLinks = currentData
            .Concat(addedData)
            .Select(x => (graphLink: x, remove: removeDataNames.Contains(x.Name)))
            .ToArray();

        foreach (var dataLink in dataLinks.Where(x => x.remove))
        {
            var deleteOption = await _graphMapStore.DataFileClient.Delete(dataLink.graphLink.FileId);
            if (deleteOption.IsError())
            {
                return _logger.LogStatus(deleteOption, "Cannot delete fileId={fieldId}").ToOptionStatus<IReadOnlyList<GraphLink>>();
            }
        }

        var result = dataLinks.Where(x => !x.remove)
            .Select(x => x.graphLink)
            .ToImmutableArray();

        return result;
    }
}
