using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class DataCompensate
{
    public static async Task<Option> Compensate(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        map.NotNull();
        entry.NotNull();
        dataFileClient.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Data, ChangeOperation.Add):
                return await UndoAdd(map, entry, dataFileClient, context).ThrowOnError();

            case (ChangeSource.Data, ChangeOperation.Delete):
            case (ChangeSource.Data, ChangeOperation.Update):
                return await UndoUpdate(map, entry, dataFileClient, context).ThrowOnError();
        }

        return StatusCode.NotFound;

    }

    private static async Task<Option> UndoAdd(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        //var deleteOption = await dataFileClient.Delete(entry.ObjectId, context);
        //if (deleteOption.IsError())
        //{
        //    context.LogError("Cannot delete fileId={fileId}");
        //    return deleteOption;
        //}

        //context.LogDebug("Rollback: removed fileId={fileId} from store, entry={entry}", entry.ObjectId, entry);
        return StatusCode.OK;
    }

    private static async Task<Option> UndoUpdate(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        //if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        //var setOption = await dataFileClient.Set(entry.ObjectId, entry.Before.Value, context);
        //if (setOption.IsError())
        //{
        //    context.LogError("Cannot set fileId={fileId} to old data");
        //    return setOption.ToOptionStatus();
        //}

        //context.LogDebug("Rollback: removed fileId={fileId} from store, entry={entry}", entry.ObjectId, entry);
        return StatusCode.OK;
    }
}
