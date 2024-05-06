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
            var map = updateNode.DataMap.Select(x => x.Value.ExpandGraphDataSource(node.Key)).ToArray();
            var updatedMap = map.Select(x => x.DataLink).ToImmutableDictionary(x => x.Name, x => x);

            graphContext.Map.Nodes.Update(nodes, x => x.With(updateNode.Tags, updatedMap), graphContext);

            foreach (var item in map)
            {
                var writeResult = await SetNodeData(graphContext, item.DataLink.FileId, item.DataETag);
                if (writeResult.IsError()) return new GraphQueryResult(CommandType.UpdateNode, writeResult);
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

        var writeOption = await graphContext.FileStore.Set(fileId, dataETag, graphContext.Context);
        if (writeOption.IsError()) return writeOption.ToOptionStatus();

        graphContext.ChangeLog.Push(new CmNodeDataSet(fileId, readOption.IsOk() ? readOption.Return() : (DataETag?)null));
        return StatusCode.OK;
    }

    public static async Task<Option> DeleteNodeData(IGraphTrxContext graphContext, string fileId)
    {
        var readOption = await graphContext.FileStore.Get(fileId, graphContext.Context);
        if (readOption.IsNotFound()) return StatusCode.OK;
        if (readOption.IsError()) return readOption.ToOptionStatus();

        graphContext.ChangeLog.Push(new CmNodeDataDelete(fileId, readOption.Return()));

        var deleteOption = await graphContext.FileStore.Delete(fileId, graphContext.Context);
        return deleteOption;
    }

    private static async Task DeleteData(IReadOnlyList<GraphNode> nodes, IGraphTrxContext graphContext)
    {
        if (graphContext.FileStore == null) return;

        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            var existOption = await graphContext.FileStore.Delete(fileId, graphContext.Context);
            existOption.LogStatus(graphContext.Context.Location(), "Deleted data map={fileId}", fileId);
        }
    }
}
