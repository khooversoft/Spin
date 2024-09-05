using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class NodeDataTool
{
    public static async Task<Option<IReadOnlyList<GraphLink>>> AddData(GiNode giNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        var dataMap = giNode.GetLinkData();

        foreach (var item in dataMap)
        {
            var writeResult = await SetNodeData(pContext, item.FileId, item.Data);
            if (writeResult.IsError()) return writeResult.ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        return dataMap.Select(x => x.ConvertTo()).ToImmutableArray();
    }

    public static async Task<Option<IReadOnlyList<GraphLink>>> MergeData(GiNode giNode, GraphNode graphNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        graphNode.NotNull();
        pContext.NotNull();

        var dataMapOption = await AddData(giNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption;
        IReadOnlyList<GraphLink> dataMap = dataMapOption.Return();

        var removeDataNames = giNode.Tags.GetTagCommands();

        var validGraphDataLinks = graphNode.DataMap.Values
            .Select(x => (graphLink: x, isOk: !removeDataNames.Contains(x.Name)))
            .ToArray();

        foreach (var dataLink in validGraphDataLinks.Where(x => x.isOk))
        {
            var deleteOption = await DeleteNodeData(dataLink.graphLink.FileId, pContext.GraphContext);

            if (deleteOption.IsError()) return deleteOption
                    .LogStatus(pContext.GraphContext.Context, $"Cannot delete fileId={dataLink.graphLink.FileId}")
                    .ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        var result = dataMap
            .Concat(validGraphDataLinks.Where(x => x.isOk).Select(x => x.graphLink))
            .ToImmutableArray();

        return result;
    }

    private static async Task<Option> SetNodeData(QueryExecutionContext pContext, string fileId, DataETag dataETag)
    {
        pContext.GraphContext.Context.LogInformation("Writing node data fileId={fileId}", fileId);
        var readOption = await pContext.GraphContext.FileStore.Get(fileId, pContext.GraphContext.Context);

        var writeOption = await pContext.GraphContext.FileStore.Set(fileId, dataETag.StripETag(), pContext.GraphContext.Context);
        if (writeOption.IsError()) return writeOption.LogStatus(pContext.GraphContext.Context, $"Write node data fileId={fileId} failed").ToOptionStatus();

        pContext.GraphContext.ChangeLog.Push(new CmNodeDataSet(fileId, readOption.IsOk() ? readOption.Return() : (DataETag?)null));
        return StatusCode.OK;
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
