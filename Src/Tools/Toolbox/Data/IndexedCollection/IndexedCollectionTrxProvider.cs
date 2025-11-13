using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        throw new NotImplementedException();
    }

    public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context)
    {
        throw new NotImplementedException();
    }
}

public static class IndexedCollectionTrxProviderExtensions
{
    public static TransactionManager Register<TKey, TValue>(this TransactionManager manager, string sourceName, IndexedCollection<TKey, TValue> collection)
        where TKey : notnull
        where TValue : notnull
    {
        var provider = new IndexedCollectionTrxProvider<TKey, TValue>(sourceName, collection);
        var reader = manager.Register(sourceName, provider);
        return manager;
    }
}
