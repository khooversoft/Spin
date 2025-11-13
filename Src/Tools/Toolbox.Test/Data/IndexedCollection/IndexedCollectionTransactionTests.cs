using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.IndexedCollection;

public class IndexedCollectionTransactionTests
{
    private record TestRec
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    private readonly ITestOutputHelper _outputHelper;

    public IndexedCollectionTransactionTests(ITestOutputHelper output) => _outputHelper = output.NotNull();
    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public async Task<IHost> BuildService()
    {
        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
            services.AddInMemoryFileStore();
            services.AddListStore<DataChangeRecord>();
            services.AddTransactionServices();
        })
        .Build();

        await host.ClearStore<IndexedCollectionTransactionTests>();
        return host;
    }

    [Fact]
    public async Task EmptyTransaction()
    {
        var host = await BuildService();
        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
            .Register("indexedCollection", c);

        await trxMgr.Commit(context);

        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var result = await fileList.Get("transaction_journal", context);
        result.BeOk();
        result.Return().Count.Be(0);
    }

    [Fact]
    public async Task SimpleTransaction()
    {
        var host = await BuildService();
        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
            .Register("indexedCollection", c);

        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue();

        await trxMgr.Commit(context);

        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var result = await fileList.Get("transaction_journal", context);
        result.BeOk();
        result.Return().Count.Be(2);
    }
}
