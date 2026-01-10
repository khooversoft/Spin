using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeTrxRollbackTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);
    public DatalakeTrxRollbackTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService(bool useHash, bool useCache, [CallerMemberName] string function = "")
    {
        string basePath = nameof(DatalakeTrxRollbackTests) + "/" + function;
        var option = TestApplication.ReadOption(basePath);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddDatalakeFileStore(option);
                services.AddMemoryCache();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = basePath,
                        SpaceFormat = useHash ? SpaceFormat.Hash : SpaceFormat.Key,
                        UseCache = useCache
                    });

                    cnfg.Add<KeyStoreProvider>("fileStore");

                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "journals",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0);
        return host;
    }

    [Theory]
    [InlineData(false, false)]
    public async Task AttachToDatalakeStore(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        int rollbackCount = 0;

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var result = await transaction.Commit();
        result.BeOk();
        rollbackCount.Be(0);

        var data = (await listStore.Get("TestJournal")).BeOk().Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task AddCommit_ShouldExist(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Add(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Commit()).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();
        var readData = (await keyStore.Get(data.Name)).BeOk().Return().ToObject<MapRecord>();
        readData.Equals(data).BeTrue("data does not match");

        // Verify journal
        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(1);
        journal[0].Entries.Action(x =>
        {
            x.Count.Be(1);
            x[0].Action(y =>
            {
                y.Action.Be(ChangeOperation.Add);
                y.Before.BeNull();
                y.After.NotNull();
                y.After.Value.ToObject<MapRecord>().Equals(data).BeTrue();
            });
        });

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task AddRollback_ShouldNotExist(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Add(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Rollback()).BeOk();
        (await keyStore.Exists(data.Name)).BeNotFound();

        // Verify journal
        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(0);

        // After rollback, item should be removed
        (await keyStore.Exists(data.Name)).BeNotFound();

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task SetCommit_ShouldExist(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        var data = new MapRecord("Item1", 25);

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();
        (await keyStore.Exists(data.Name)).BeNotFound();

        (await keyStore.Set(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Commit()).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();
        var readData = (await keyStore.Get(data.Name)).BeOk().Return().ToObject<MapRecord>();
        readData.Equals(data).BeTrue("data does not match");

        // Verify journal
        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(1);
        journal[0].Entries.Action(x =>
        {
            x.Count.Be(1);
            x[0].Action(y =>
            {
                y.Action.Be(ChangeOperation.Add);
                y.Before.BeNull();
                y.After.NotNull();
                y.After.Value.ToObject<MapRecord>().Equals(data).BeTrue();
            });
        });

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task SetRollback_ShouldNotExist(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Set(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Rollback()).BeOk();
        (await keyStore.Exists(data.Name)).BeNotFound();

        // Verify journal
        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(0);

        // After rollback, item should be removed
        (await keyStore.Exists(data.Name)).BeNotFound();

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackTryAdd_ShouldRemoveItem(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Add(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Rollback()).BeOk();

        // After rollback, item should be removed
        (await keyStore.Exists(data.Name)).BeNotFound();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackTryRemove_ShouldRestoreItem(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var originalItem = new MapRecord("Item1", 25);
        (await keyStore.Set(originalItem.Name, originalItem)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        (await keyStore.Delete(originalItem.Name)).BeOk();
        (await keyStore.Exists(originalItem.Name)).BeNotFound();

        (await transaction.Rollback()).BeOk();

        var restored = (await keyStore.Get(originalItem.Name)).BeOk().Return().ToObject<MapRecord>();
        restored.Equals(originalItem).BeTrue();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackTryUpdate_ShouldRestorePreviousValue(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var originalItem = new MapRecord("Item1", 25);
        (await keyStore.Set(originalItem.Name, originalItem)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var updatedItem = new MapRecord("Item1", 30);
        (await keyStore.Set(updatedItem.Name, updatedItem)).BeOk();
        (await keyStore.Get(updatedItem.Name)).BeOk().Return().ToObject<MapRecord>().Age.Be(30);

        (await transaction.Rollback()).BeOk();

        var restored = (await keyStore.Get(originalItem.Name)).BeOk().Return().ToObject<MapRecord>();
        restored.Age.Be(25);
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackSet_NewItem_ShouldRemoveItem(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Set(data.Name, data)).BeOk();
        (await keyStore.Exists(data.Name)).BeOk();

        (await transaction.Rollback()).BeOk();

        (await keyStore.Exists(data.Name)).BeNotFound();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackSet_ExistingItem_ShouldRestorePreviousValue(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var originalItem = new MapRecord("Item1", 25);
        (await keyStore.Set(originalItem.Name, originalItem)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var updatedItem = new MapRecord("Item1", 30);
        (await keyStore.Set(updatedItem.Name, updatedItem)).BeOk();
        (await keyStore.Get(updatedItem.Name)).BeOk().Return().ToObject<MapRecord>().Age.Be(30);

        (await transaction.Rollback()).BeOk();

        var restored = (await keyStore.Get(originalItem.Name)).BeOk().Return().ToObject<MapRecord>();
        restored.Age.Be(25);
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackClear_ShouldRestoreAllItems(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var items = new[]
        {
            new MapRecord("Item1", 25),
            new MapRecord("Item2", 30),
            new MapRecord("Item3", 35),
        };

        foreach (var item in items) (await keyStore.Set(item.Name, item)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        foreach (var item in items) (await keyStore.Delete(item.Name)).BeOk();
        foreach (var item in items) (await keyStore.Exists(item.Name)).BeNotFound();

        (await transaction.Rollback()).BeOk();

        foreach (var item in items)
        {
            var restored = (await keyStore.Get(item.Name)).BeOk().Return().ToObject<MapRecord>();
            restored.Equals(item).BeTrue();
        }

        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);
        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackGetOrAdd_NewItem_ShouldRemoveItem(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var item = new MapRecord("Item1", 25);
        (await keyStore.Add(item.Name, item)).BeOk();
        (await keyStore.Get(item.Name)).BeOk().Return().ToObject<MapRecord>().Age.Be(25);

        (await transaction.Rollback()).BeOk();

        (await keyStore.Exists(item.Name)).BeNotFound();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackMultipleOperations_ShouldUndoInReverseOrder(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var original = new MapRecord("Item1", 25);
        (await keyStore.Set(original.Name, original)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        // Operation 1: Add Item2
        var item2 = new MapRecord("Item2", 30);
        (await keyStore.Add(item2.Name, item2)).BeOk();

        // Operation 2: Update Item1
        var updatedItem1 = new MapRecord("Item1", 26);
        (await keyStore.Set(updatedItem1.Name, updatedItem1)).BeOk();

        // Operation 3: Remove Item2
        (await keyStore.Delete(item2.Name)).BeOk();

        // Operation 4: Add Item3
        var item3 = new MapRecord("Item3", 35);
        (await keyStore.Add(item3.Name, item3)).BeOk();

        (await transaction.Rollback()).BeOk();

        // After rollback: Item1 restored, Item2 and Item3 removed
        var restored = (await keyStore.Get(original.Name)).BeOk().Return().ToObject<MapRecord>();
        restored.Age.Be(25);
        (await keyStore.Exists(item2.Name)).BeNotFound();
        (await keyStore.Exists(item3.Name)).BeNotFound();

        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);
        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task RollbackEmptyTransaction_ShouldSucceed(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var result = await transaction.Rollback();
        result.BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(0);
        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task DeleteCommit_ShouldRemoveExistingItem(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var existing = new MapRecord("Item1", 25);
        (await keyStore.Set(existing.Name, existing)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        (await keyStore.Delete(existing.Name)).BeOk();
        (await keyStore.Exists(existing.Name)).BeNotFound();

        (await transaction.Commit()).BeOk();

        (await keyStore.Exists(existing.Name)).BeNotFound();

        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(1);
        journal[0].Entries.Action(entries =>
        {
            entries.Count.Be(1);
            entries[0].Action(entry =>
            {
                entry.Action.Be(ChangeOperation.Delete);
                entry.Before.NotNull();
                entry.After.HasValue.BeFalse();

                var before = entry.Before!.Value.ToObject<MapRecord>();
                before.Equals(existing).BeTrue();
            });
        });

        transaction.Providers.Delist(keyStore);
    }

    [Theory]
    [InlineData(false, false)]
    public async Task UpdateCommit_ShouldPersistNewValue(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        var original = new MapRecord("Item1", 25);
        (await keyStore.Set(original.Name, original)).BeOk();

        transaction.Providers.Enlist(keyStore);
        await transaction.Start();

        var updated = new MapRecord("Item1", 30);
        (await keyStore.Set(updated.Name, updated)).BeOk();

        (await transaction.Commit()).BeOk();

        var stored = (await keyStore.Get(updated.Name)).BeOk().Return().ToObject<MapRecord>();
        stored.Equals(updated).BeTrue();

        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(1);
        journal[0].Entries.Action(entries =>
        {
            entries.Count.Be(1);
            entries[0].Action(entry =>
            {
                entry.Action.Be(ChangeOperation.Update);
                entry.Before.NotNull();
                entry.After.NotNull();

                entry.Before!.Value.ToObject<MapRecord>().Equals(original).BeTrue();
                entry.After!.Value.ToObject<MapRecord>().Equals(updated).BeTrue();
            });
        });

        transaction.Providers.Delist(keyStore);
    }
}
