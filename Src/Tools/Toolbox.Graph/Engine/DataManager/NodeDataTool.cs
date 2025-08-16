using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class NodeDataTool
{
    public static async Task<Option<IReadOnlyList<GraphLink>>> AddData(GiNode giNode, GraphTrxContext pContext)
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

    public static async Task<Option> DeleteData(IReadOnlyList<GraphNode> nodes, GraphTrxContext graphContext)
    {
        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            var result = await NodeDataTool.DeleteNodeData(fileId, graphContext).ConfigureAwait(false);
            if (result.IsError()) return result;
        }

        return StatusCode.OK;
    }

    public static async Task<Option<GraphLinkData>> GetData(GraphLink graphLink, GraphTrxContext pContext)
    {
        var readOption = await GetData(graphLink.FileId, pContext);
        if (readOption.IsError()) return readOption.ToOptionStatus<GraphLinkData>();

        var result = graphLink.ConvertTo(readOption.Return());
        return result;
    }

    public static async Task<Option<DataETag>> GetData(string fileId, GraphTrxContext pContext)
    {
        var readOption = await pContext.DataClient.Get(fileId, pContext.Context).ConfigureAwait(false);
        readOption.LogStatus(pContext.Context, $" Get node data fileId={fileId}");
        return readOption;
    }

    public static async Task<Option<IReadOnlyList<GraphLink>>> MergeData(GiNode giNode, GraphNode graphNode, GraphTrxContext pContext)
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
            var deleteOption = await DeleteNodeData(dataLink.graphLink.FileId, pContext);

            if (deleteOption.IsError()) return deleteOption
                    .LogStatus(pContext.Context, $"Cannot delete fileId={dataLink.graphLink.FileId}")
                    .ToOptionStatus<IReadOnlyList<GraphLink>>();
        }

        var result = dataLinks.Where(x => !x.remove)
            .Select(x => x.graphLink)
            .ToImmutableArray();

        return result;
    }

    public static async Task<Option> DeleteNodeData(string fileId, GraphTrxContext pContext)
    {
        var readOption = await pContext.DataClient.Get(fileId, pContext.Context).ConfigureAwait(false);
        if (readOption.IsNotFound()) return StatusCode.OK;
        if (readOption.IsError()) return readOption.ToOptionStatus();

        pContext.TransactionScope.DataDelete(fileId, readOption.Return());

        pContext.Context.LogTrace("Deleting data map={fileId}", fileId);
        var deleteOption = await pContext.DataClient.Delete(fileId, pContext.Context).ConfigureAwait(false);
        deleteOption.LogStatus(pContext.Context, "Deleted data map");

        return deleteOption;
    }

    private static async Task<Option> SetNodeData(GraphTrxContext pContext, string fileId, DataETag dataETag)
    {
        pContext.Context.LogTrace("Writing node data fileId={fileId}", fileId);
        var currentOption = await pContext.DataClient.Get(fileId, pContext.Context).ConfigureAwait(false);

        var writeOption = await pContext.DataClient.Set(fileId, dataETag, pContext.Context).ConfigureAwait(false);

        if (writeOption.IsError()) return writeOption.LogStatus(pContext.Context, "Write node data fileId={fileId} failed", [fileId]).ToOptionStatus();

        if (currentOption.IsNotFound())
            pContext.TransactionScope.DataAdd(fileId, dataETag);
        else
            pContext.TransactionScope.DataChange(fileId, currentOption.Return(), dataETag);

        return StatusCode.OK;
    }
}
