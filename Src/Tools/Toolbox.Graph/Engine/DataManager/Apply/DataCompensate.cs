using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class DataCompensate
{
    public static async Task<Option> Compensate(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        map.NotNull();
        entry.NotNull();
        dataFileClient.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Data, ChangeOperation.Add):
                return await UndoAdd(map, entry, dataFileClient, logger).ThrowOnError();

            case (ChangeSource.Data, ChangeOperation.Delete):
            case (ChangeSource.Data, ChangeOperation.Update):
                return await UndoUpdate(map, entry, dataFileClient, logger).ThrowOnError();
        }

        return StatusCode.NotFound;

    }

    private static async Task<Option> UndoAdd(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        var deleteOption = await dataFileClient.Delete(entry.ObjectId);
        if (deleteOption.IsError())
        {
            logger.LogError("Cannot delete objectId={objectId}", entry.ObjectId);
            return deleteOption;
        }

        logger.LogDebug("Rollback: removed fileId={fileId} from store, entry={entry}", entry.ObjectId, entry);
        return StatusCode.OK;
    }

    private static async Task<Option> UndoUpdate(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ILogger logger)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        var setOption = await dataFileClient.Set(entry.ObjectId, entry.Before);
        if (setOption.IsError())
        {
            logger.LogError("Cannot set objectId={objectId} to old data", entry.ObjectId);
            return setOption.ToOptionStatus();
        }

        logger.LogDebug("Rollback: set objectId={objectId} to old data, entry={entry}", entry.ObjectId, entry);
        return StatusCode.OK;
    }
}
