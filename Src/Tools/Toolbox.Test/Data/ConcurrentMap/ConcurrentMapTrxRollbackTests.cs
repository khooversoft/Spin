// Copyright (c) Kelvin Hoover.  All rights Reserved.
// Licensed under MIT license

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.ConcurrentMap;

public class ConcurrentMapTrxRollbackTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);

    public ConcurrentMapTrxRollbackTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryKeyStore();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "listBase",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddTransaction(new TransactionOption { ListSpaceName = "list", JournalKey = "TestJournal" });
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task AttachToConcurrentMap()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        int rollbackCount = 0;

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        var result = await transaction.Commit(context);
        result.BeOk();
        rollbackCount.Be(0);

        var records = await listStore.Get("TestJournal", context);
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackTryAdd_ShouldRemoveItem()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        var item = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(item).BeTrue();
        concurrentMap.Count.Be(1);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, item should be removed
        concurrentMap.Count.Be(0);
        concurrentMap.ContainsKey("Item1").BeFalse();

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackTryRemove_ShouldRestoreItem()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        var originalItem = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(originalItem).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        concurrentMap.TryRemove("Item1", out _).BeTrue();
        concurrentMap.Count.Be(0);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, item should be restored
        concurrentMap.Count.Be(1);
        concurrentMap.TryGetValue("Item1", out var restored).BeTrue();
        restored.NotNull().Age.Be(25);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackTryUpdate_ShouldRestorePreviousValue()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        var originalItem = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(originalItem).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        var updatedItem = new MapRecord("Item1", 30);
        concurrentMap.TryUpdate(updatedItem, originalItem).BeTrue();
        concurrentMap["Item1"].Age.Be(30);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, original value should be restored
        concurrentMap.TryGetValue("Item1", out var restored).BeTrue();
        restored.NotNull().Age.Be(25);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackSet_NewItem_ShouldRemoveItem()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        concurrentMap.Set(new MapRecord("Item1", 25));
        concurrentMap.Count.Be(1);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, item should be removed
        concurrentMap.Count.Be(0);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackSet_ExistingItem_ShouldRestorePreviousValue()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        concurrentMap.Set(new MapRecord("Item1", 30));
        concurrentMap["Item1"].Age.Be(30);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, original value should be restored
        concurrentMap["Item1"].Age.Be(25);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackClear_ShouldRestoreAllItems()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();
        concurrentMap.TryAdd(new MapRecord("Item2", 30)).BeTrue();
        concurrentMap.TryAdd(new MapRecord("Item3", 35)).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        concurrentMap.Clear();
        concurrentMap.Count.Be(0);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, all items should be restored
        concurrentMap.Count.Be(3);
        concurrentMap.ContainsKey("Item1").BeTrue();
        concurrentMap.ContainsKey("Item2").BeTrue();
        concurrentMap.ContainsKey("Item3").BeTrue();

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackGetOrAdd_NewItem_ShouldRemoveItem()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        var item = concurrentMap.GetOrAdd(new MapRecord("Item1", 25));
        item.Age.Be(25);
        concurrentMap.Count.Be(1);

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback, item should be removed
        concurrentMap.Count.Be(0);

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackMultipleOperations_ShouldUndoInReverseOrder()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        // Operation 1: Add Item2
        concurrentMap.TryAdd(new MapRecord("Item2", 30)).BeTrue();

        // Operation 2: Update Item1
        var original = new MapRecord("Item1", 25);
        concurrentMap.TryUpdate(new MapRecord("Item1", 26), original).BeTrue();

        // Operation 3: Remove Item2
        concurrentMap.TryRemove("Item2", out _).BeTrue();

        // Operation 4: Add Item3
        concurrentMap.TryAdd(new MapRecord("Item3", 35)).BeTrue();

        var result = await transaction.Rollback(context);
        result.BeOk();

        // After rollback: Item1 restored to 25, Item2 gone, Item3 gone
        concurrentMap.Count.Be(1);
        concurrentMap["Item1"].Age.Be(25);
        concurrentMap.ContainsKey("Item2").BeFalse();
        concurrentMap.ContainsKey("Item3").BeFalse();

        transaction.Enlistments.Delist(concurrentMap);
    }

    [Fact]
    public async Task RollbackEmptyTransaction_ShouldSucceed()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredService<Transaction>();
        var context = host.Services.CreateContext<ConcurrentMapTrxRollbackTests>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();

        transaction.Enlistments.Enlist(concurrentMap);
        transaction.Start();

        // No operations performed

        var result = await transaction.Rollback(context);
        result.BeOk();

        // Map should remain unchanged
        concurrentMap.Count.Be(1);
        concurrentMap["Item1"].Age.Be(25);

        transaction.Enlistments.Delist(concurrentMap);
    }
}