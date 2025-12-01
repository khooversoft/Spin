using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class MemoryStoreTrxProvider
{
    private readonly MemoryStore _memoryStore;

    public MemoryStoreTrxProvider(string name, MemoryStore memoryStore)
    {
        Name = name.NotEmpty();
        _memoryStore = memoryStore.NotNull();
    }

    public string Name { get; }

    public Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context)
    {
        context.LogTrace("MemoryStoreTrxProvider: Committing change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context)
    {
        context.LogTrace("MemoryStoreTrxProvider: Preparing change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context)
    {
        if (dataChangeEntry.SourceName != Name) return new Option(StatusCode.OK).ToTaskResult();
        context.LogTrace("MemoryStoreTrxProvider: Rolling back change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);

        switch (dataChangeEntry.Action)
        {
            case ChangeOperation.Add:
                {
                    if (dataChangeEntry.After == null) return new Option(StatusCode.OK).ToTaskResult();
                    var directoryDetail = dataChangeEntry.After.Value.ToObject<DirectoryDetail>();
                    _memoryStore.Delete(directoryDetail.PathDetail.Path, null, context).Assert(x => x.IsOk(), $"Failed to delete {directoryDetail.PathDetail.Path}");
                    return new Option(StatusCode.OK).ToTaskResult();
                }
            case ChangeOperation.Update:
            case ChangeOperation.Delete:
                {
                    if (dataChangeEntry.Before == null) return new Option(StatusCode.OK).ToTaskResult();
                    var directoryDetail = dataChangeEntry.Before.Value.ToObject<DirectoryDetail>();
                    _memoryStore.Set(directoryDetail.PathDetail.Path, directoryDetail.Data, null, context);
                    return new Option(StatusCode.OK).ToTaskResult();
                }
            default:
                return new Option(StatusCode.OK).ToTaskResult();
        }
    }
}

//public static class MemoryStoreTrxProviderExtensions
//{
//    public static TransactionManager Register(this TransactionManager manager, string sourceName, MemoryStore memoryStore)
//    {
//        manager.Register(memoryStore).NotNull();
//        return manager;
//    }
//}