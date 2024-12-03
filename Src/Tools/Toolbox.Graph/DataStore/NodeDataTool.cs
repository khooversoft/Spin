using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Logging;
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
            var writeResult = await SetNodeData(pContext, item.FileId, item.Data).ConfigureAwait(false);
            if (writeResult.IsError()) return writeResult.ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        return dataMap.Select(x => x.ConvertTo()).ToImmutableArray();
    }

    public static async Task<Option> DeleteData(IReadOnlyList<GraphNode> nodes, IGraphTrxContext graphContext)
    {
        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            var result = await NodeDataTool.DeleteNodeData(fileId, graphContext).ConfigureAwait(false);
            if (result.IsError()) return result;
        }

        return StatusCode.OK;
    }

    public static async Task<Option<GraphLinkData>> GetData(GraphLink graphLink, QueryExecutionContext pContext)
    {
        var readOption = await GetData(graphLink.FileId, pContext);
        if (readOption.IsError()) return readOption.ToOptionStatus<GraphLinkData>();

        var result = graphLink.ConvertTo(readOption.Return());
        return result;
    }

    public static async Task<Option<DataETag>> GetData(string fileId, QueryExecutionContext pContext)
    {
        var readOption = await pContext.TrxContext.FileStore.Get(fileId, pContext.TrxContext.Context).ConfigureAwait(false);
        readOption.LogStatus(pContext.TrxContext.Context, $" Get node data fileId={fileId}");
        return readOption;
    }

    public static async Task<Option<IReadOnlyList<GraphLink>>> MergeData(GiNode giNode, GraphNode graphNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        graphNode.NotNull();
        pContext.NotNull();

        var dataMapOption = await AddData(giNode, pContext).ConfigureAwait(false);
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
            var deleteOption = await DeleteNodeData(dataLink.graphLink.FileId, pContext.TrxContext);

            if (deleteOption.IsError()) return deleteOption
                    .LogStatus(pContext.TrxContext.Context, $"Cannot delete fileId={dataLink.graphLink.FileId}")
                    .ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        var result = dataLinks.Where(x => !x.remove)
            .Select(x => x.graphLink)
            .ToImmutableArray();

        return result;
    }

    public static async Task<Option> DeleteNodeData(string fileId, IGraphTrxContext graphContext)
    {
        var readOption = await graphContext.FileStore.Get(fileId, graphContext.Context).ConfigureAwait(false);
        if (readOption.IsNotFound()) return StatusCode.OK;
        if (readOption.IsError()) return readOption.ToOptionStatus();

        graphContext.ChangeLog.Push(new CmNodeDataDelete(fileId, readOption.Return()));

        graphContext.Context.LogInformation("Deleting data map={fileId}", fileId);
        var deleteOption = await graphContext.FileStore.Delete(fileId, graphContext.Context).ConfigureAwait(false);
        deleteOption.LogStatus(graphContext.Context, "Deleted data map");

        return deleteOption;
    }

    private static async Task<Option> SetNodeData(QueryExecutionContext pContext, string fileId, DataETag dataETag)
    {
        pContext.TrxContext.Context.LogInformation("Writing node data fileId={fileId}", fileId);
        var readOption = await pContext.TrxContext.FileStore.Get(fileId, pContext.TrxContext.Context).ConfigureAwait(false);

        var writeOption = await pContext.TrxContext.FileStore.Set(fileId, dataETag.StripETag(), pContext.TrxContext.Context).ConfigureAwait(false);

        if (writeOption.IsError()) return writeOption
                .LogStatus(pContext.TrxContext.Context, "Write node data fileId={fileId} failed", [fileId])
                .ToOptionStatus();

        pContext.TrxContext.ChangeLog.Push(new CmNodeDataSet(fileId, readOption.IsOk() ? readOption.Return() : (DataETag?)null));
        return StatusCode.OK;
    }
}
