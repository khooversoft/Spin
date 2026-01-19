using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class DataBuild
{
    public static async Task<Option> Build(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        map.NotNull();
        entry.NotNull();
        dataFileClient.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Data, ChangeOperation.Add):
            case (ChangeSource.Data, ChangeOperation.Update):
                return await Add(map, entry, dataFileClient, logger).ThrowOnError();

            case (ChangeSource.Data, ChangeOperation.Delete):
                return await Delete(map, entry, dataFileClient, logger).ThrowOnError();
        }

        return StatusCode.NotFound;
    }

    private static async Task<Option> Add(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        if (entry.After == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        var setOption = await dataFileClient.Set(entry.ObjectId, entry.After.Value);
        if (setOption.IsError())
        {
            logger.LogError("Cannot set fileId={fileId}", entry.ObjectId);
            return setOption.ToOptionStatus();
        }

        logger.LogDebug("Build: added fileId={fileId} to store, entry={entry}", entry.ObjectId, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static async Task<Option> Delete(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        var deleteOption = await dataFileClient.Delete(entry.ObjectId);
        if (deleteOption.IsError())
        {
            logger.LogError("Cannot delete fileId={fileId}", entry.ObjectId);
            return deleteOption;
        }

        logger.LogDebug("Build: removed fileId={fileId} from store, entry={entry}", entry.ObjectId, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }
}
