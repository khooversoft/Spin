using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class MapAccessTool
{
    public static async Task<Option> LoadDatabase(this IGraphEngine graphEngine, IFileReadWriteAccess readWrite, IGraphMapFactory graphMapFactory, ScopeContext context)
    {
        graphEngine.NotNull();
        readWrite.NotNull();
        graphMapFactory.NotNull();
        readWrite.Assert(x => x.GetLeaseId().IsNotEmpty(), _ => "LeaseId is empty");
        using var metric = context.LogDuration("LoadDatabase");

        context.LogDebug("Loading graph data, leaseId={leaseId}", readWrite.GetLeaseId());
        var dataETagOption = await readWrite.Get(context).ConfigureAwait(false);
        if (dataETagOption.IsError()) return dataETagOption.ToOptionStatus();

        var dataETag = dataETagOption.Return();

        GraphMap newMap = dataETag.Data.Length switch
        {
            0 => graphMapFactory.Create(),
            _ => graphMapFactory.Create(dataETag)
        };

        graphEngine.SetGraphMapData(newMap, dataETag.ETag);

        if (dataETag.Data.Length == 0)
        {
            context.LogDebug("Saving empty graph map because the file was 0 bytes");
            var updateOption = await SaveDatabase(graphEngine, readWrite, context).ConfigureAwait(false);
            if (updateOption.IsError()) return updateOption;
        }

        graphEngine.GetMapData().NotNull().Map.UpdateCounters();

        context.LogDebug("Loaded graph data file={mapDatabasePath}", GraphConstants.MapDatabasePath);
        return StatusCode.OK;
    }

    public static async Task<Option> SaveDatabase(this IGraphEngine graphEngine, IFileReadWriteAccess readWrite, ScopeContext context)
    {
        graphEngine.NotNull();
        readWrite.NotNull();
        readWrite.Assert(x => x.GetLeaseId().IsNotEmpty(), _ => "LeaseId is empty");
        using var metric = context.LogDuration("SaveDatabase");

        context.LogDebug("Writing graph data, leaseId={leaseId}", readWrite.GetLeaseId());

        var mapData = graphEngine.GetMapData().NotNull("No map data");

        DataETag dataETag = mapData.Map.NotNull("Graph not loaded")
            .ToSerialization()
            .ToDataETag(mapData.ETag);

        var saveOption = await readWrite.Set(dataETag, context).ConfigureAwait(false);
        if (saveOption.IsError()) return saveOption.LogStatus(context, "Failed to save database").ToOptionStatus();

        string currentETag = saveOption.Return().NotEmpty("No etag returned");
        graphEngine.UpdateGraphMapETag(currentETag);
        graphEngine.GetMapData().NotNull().Map.UpdateCounters();

        context.LogDebug("Write graph data file={mapDatabasePath}, eTag={etag}", GraphConstants.MapDatabasePath, currentETag);
        return StatusCode.OK;
    }
}
