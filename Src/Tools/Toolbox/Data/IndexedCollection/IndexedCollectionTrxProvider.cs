using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context)
    {
        throw new NotImplementedException();
    }
}

public static class IndexedCollectionTrxProviderExtensions
{
    public static TransactionManagerFactory EnlistTransaction<TKey, TValue>(this TransactionManagerFactory factory, string name, IndexedCollection<TKey, TValue> collection)
        where TKey : notnull
        where TValue : notnull
    {
        var provider = new IndexedCollectionTrxProvider<TKey, TValue>(name, collection);
        factory.AddProvider(provider);
        return factory;
    }
}
