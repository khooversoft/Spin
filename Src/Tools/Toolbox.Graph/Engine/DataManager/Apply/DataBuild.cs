using Toolbox.Models;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class DataBuild
{
    public static async Task<Option> Build(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        map.NotNull();
        entry.NotNull();
        dataFileClient.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Data, ChangeOperation.Add):
            case (ChangeSource.Data, ChangeOperation.Update):
                return await Add(map, entry, dataFileClient, context).ThrowOnError();

            case (ChangeSource.Data, ChangeOperation.Delete):
                return await Delete(map, entry, dataFileClient, context).ThrowOnError();
        }

        return StatusCode.NotFound;
    }


    private static async Task<Option> Add(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        if (entry.After == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        var setOption = await dataFileClient.Set(entry.ObjectId, entry.After.Value, context);
        if (setOption.IsError())
        {
            context.LogError("Cannot set fileId={fileId}", entry.ObjectId);
            return setOption.ToOptionStatus();
        }

        context.LogDebug("Build: added fileId={fileId} to store, entry={entry}", entry.ObjectId, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static async Task<Option> Delete(GraphMap map, DataChangeEntry entry, IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        var deleteOption = await dataFileClient.Delete(entry.ObjectId, context);
        if (deleteOption.IsError())
        {
            context.LogError("Cannot delete fileId={fileId}", entry.ObjectId);
            return deleteOption;
        }

        context.LogDebug("Build: removed fileId={fileId} from store, entry={entry}", entry.ObjectId, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }
}
