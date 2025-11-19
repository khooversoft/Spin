using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class IndexedCollectionTrxProvider<TKey, TValue> : ITransactionProvider
    where TKey : notnull
    where TValue : notnull
{
    private readonly IndexedCollection<TKey, TValue> _index;

    public IndexedCollectionTrxProvider(string name, IndexedCollection<TKey, TValue> index)
    {
        _index = index.NotNull();
        Name = name.NotEmpty();
    }

    public string Name { get; }

    public Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context)
    {
        context.LogTrace("IndexedCollectionTrxProvider: Committing change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context)
    {
        context.LogTrace("IndexedCollectionTrxProvider: Preparing change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context)
    {
        if (dataChangeEntry.SourceName != Name) return new Option(StatusCode.OK).ToTaskResult();
        context.LogTrace("IndexedCollectionTrxProvider: Rolling back change entry dataChangeEntry={dataChangeEntry}", dataChangeEntry);

        _index.DataChangeLog.Pause();
        try
        {
            switch (dataChangeEntry.Action)
            {
                case ChangeOperation.Add:
                    {
                        if (dataChangeEntry.After == null) return new Option(StatusCode.OK).ToTaskResult();
                        var value = dataChangeEntry.After.Value.ToObject<TValue>();
                        _index.Remove(value).Assert(x => x == true, $"Failed to delete {value}");
                        return new Option(StatusCode.OK).ToTaskResult();
                    }
                case ChangeOperation.Update:
                case ChangeOperation.Delete:
                    {
                        if (dataChangeEntry.Before == null) return new Option(StatusCode.OK).ToTaskResult();
                        var value = dataChangeEntry.Before.Value.ToObject<TValue>();
                        _index.Set(value);
                        return new Option(StatusCode.OK).ToTaskResult();
                    }
                default:
                    return new Option(StatusCode.OK).ToTaskResult();
            }
        }
        finally
        {
            _index.DataChangeLog.Resume();
        }
    }
}

public static class IndexedCollectionTrxProviderExtensions
{
    public static TransactionManager Register<TKey, TValue>(this TransactionManager manager, string sourceName, IndexedCollection<TKey, TValue> collection)
        where TKey : notnull
        where TValue : notnull
    {
        var provider = new IndexedCollectionTrxProvider<TKey, TValue>(sourceName, collection);
        var reader = manager.Register(sourceName, provider).NotNull();
        collection.DataChangeLog.Set(reader);
        return manager;
    }
}
