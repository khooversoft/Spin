using System.Buffers.Text;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class GraphCommandNode
{
    public static async Task<GraphQueryResult> Add(GsNodeAdd addNode, IGraphTrxContext graphContext)
    {
        var tags = addNode.Upsert ? addNode.Tags : addNode.Tags.RemoveCommands();

        if (!addNode.DataMap.All(x => Base64.IsValid(x.Value.Data64))) return new GraphQueryResult(CommandType.AddNode, StatusCode.BadRequest);
        var map = addNode.DataMap.Select(x => x.Value.ExpandGraphDataSource(addNode.Key)).ToArray();

        var updatedMap = map.Select(x => x.DataLink).ToImmutableDictionary(x => x.Name, x => x);
        var graphNode = new GraphNode(addNode.Key, tags, DateTime.UtcNow, updatedMap);

        Option result = addNode.Upsert switch
        {
            false => graphContext.Map.Nodes.Add(graphNode, graphContext),
            true => graphContext.Map.Nodes.Set(graphNode, graphContext),
        };

        if (result.IsError()) return new GraphQueryResult(CommandType.AddNode, result);

        foreach (var item in map)
        {
            var writeResult = await SetNodeData(graphContext, item.DataLink.FileId, item.DataETag);
            if (writeResult.IsError()) return new GraphQueryResult(CommandType.AddNode, writeResult);
        }

        return new GraphQueryResult(CommandType.AddNode, StatusCode.OK);
    }

    public static async Task<GraphQueryResult> Update(GsNodeUpdate updateNode, IGraphTrxContext graphContext)
    {
        if (!updateNode.DataMap.All(x => Base64.IsValid(x.Value.Data64))) return new GraphQueryResult(CommandType.AddNode, StatusCode.BadRequest);

        var searchResult = GraphQuery.Process(graphContext.Map, updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.NoContent);

        foreach (var node in nodes)
        {
            var dataToRemoveKeys = TagsTool.GetTagCommands(updateNode.Tags)
                .Intersect(node.DataMap.Select(x => x.Key))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var map = updateNode.DataMap
                .Where(x => !dataToRemoveKeys.Contains(x.Key))
                .Select(x => x.Value.ExpandGraphDataSource(node.Key))
                .ToArray();

            var updatedMap = map
                .Select(x => x.DataLink)
                .Concat(node.DataMap.Values.Where(x => !dataToRemoveKeys.Contains(x.Name)))
                .ToImmutableDictionary(x => x.Name, x => x);

            var updateOption = graphContext.Map.Nodes.Update(node, updateNode.Tags, updatedMap, graphContext);
            if (updateOption.IsError()) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.Conflict);

            foreach (var item in map)
            {
                var writeResult = await SetNodeData(graphContext, item.DataLink.FileId, item.DataETag);
                if (writeResult.IsError()) return new GraphQueryResult(CommandType.UpdateNode, writeResult);
            }

            foreach (var key in dataToRemoveKeys)
            {
                await DeleteNodeData(node.DataMap[key].FileId, graphContext);
            }
        }

        return searchResult with { CommandType = CommandType.UpdateNode };
    }

    public static async Task<GraphQueryResult> Delete(GsNodeDelete deleteNode, IGraphTrxContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.DeleteNode, StatusCode.NoContent);

        await DeleteData(nodes, graphContext);

        nodes.ForEach(x => graphContext.Map.Nodes.Remove(x.Key, graphContext));
        var result = searchResult with { CommandType = CommandType.DeleteNode };
        return result;
    }

    private static async Task<Option> SetNodeData(IGraphTrxContext graphContext, string fileId, DataETag dataETag)
    {
        var readOption = await graphContext.FileStore.Get(fileId, graphContext.Context);

        var writeOption = await graphContext.FileStore.Set(fileId, dataETag.StripETag(), graphContext.Context);
        if (writeOption.IsError()) return writeOption.ToOptionStatus();

        graphContext.ChangeLog.Push(new CmNodeDataSet(fileId, readOption.IsOk() ? readOption.Return() : (DataETag?)null));
        return StatusCode.OK;
    }

    private static async Task DeleteData(IReadOnlyList<GraphNode> nodes, IGraphTrxContext graphContext)
    {
        if (graphContext.FileStore == null) return;

        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            await DeleteNodeData(fileId, graphContext);
            var existOption = await graphContext.FileStore.Delete(fileId, graphContext.Context);
        }
    }

    private static async Task<Option> DeleteNodeData(string fileId, IGraphTrxContext graphContext)
    {
        var readOption = await graphContext.FileStore.Get(fileId, graphContext.Context);
        if (readOption.IsNotFound()) return StatusCode.OK;
        if (readOption.IsError()) return readOption.ToOptionStatus();

        graphContext.ChangeLog.Push(new CmNodeDataDelete(fileId, readOption.Return()));

        graphContext.Context.LogInformation("Deleting data map={fileId}", fileId);
        var deleteOption = await graphContext.FileStore.Delete(fileId, graphContext.Context);
        deleteOption.LogStatus(graphContext.Context, "Deleted data map");

        return deleteOption;
    }
}
