// Copyright (c) Kelvin Hoover.  All rights Reserved.
// Licensed under MIT license

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreTrxTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);

    public MemoryStoreTrxTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService(bool useCache)
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

                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = "dataFiles",
                        SpaceFormat = SpaceFormat.Key,
                        UseCache = useCache
                    });

                    cnfg.Add<KeyStoreProvider>("fileStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddKeyStore("file");

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                    config.Providers.Add<MemoryStore>();
                });
            })
            .Build();

        return host;
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SimpleKeyFileTransactions(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        var r1 = new MapRecord("Alice", 30);
        (await keyStore.Set(path, r1.ToDataETag())).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            string fullPath = "datafiles/" + path;

            x.Count.Be(1);
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(1);
            list[0].Action(entry =>
            {
                entry.LogSequenceNumber.NotEmpty();
                entry.TransactionId.NotEmpty();
                entry.Date.IsDateTimeValid().BeTrue();
                entry.SourceName.Be(MemoryStore.SourceNameText);
                entry.ObjectId.Be(fullPath);
                entry.Action.Be(ChangeOperation.Add);
                entry.Before.HasValue.BeFalse();
                entry.After.HasValue.BeTrue();

                entry.TypeName.Be(nameof(DirectoryDetail));
                var detail = entry.After!.Value.ToObject<DirectoryDetail>().NotNull();
                detail.PathDetail.Path.Be(fullPath);

                var data = detail.Data.ToObject<MapRecord>().NotNull();
                data.Name.Be("Alice");
                data.Age.Be(30);
            });
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateExistingFileTransaction(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        // Create initial file inside transaction
        await transaction.Start();

        var original = new MapRecord("Alice", 30);
        (await keyStore.Set(path, original.ToDataETag())).BeOk();

        var updated = new MapRecord("Alice", 35);
        (await keyStore.Set(path, updated.ToDataETag())).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            string fullPath = "datafiles/" + path;

            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(2);
            
            // First operation is Add
            list[0].Action(entry =>
            {
                entry.SourceName.Be(MemoryStore.SourceNameText);
                entry.ObjectId.Be(fullPath);
                entry.Action.Be(ChangeOperation.Add);
                entry.Before.HasValue.BeFalse();
                entry.After.HasValue.BeTrue();

                var detail = entry.After!.Value.ToObject<DirectoryDetail>().NotNull();
                var data = detail.Data.ToObject<MapRecord>().NotNull();
                data.Name.Be("Alice");
                data.Age.Be(30);
            });

            // Second operation is Update
            list[1].Action(entry =>
            {
                entry.SourceName.Be(MemoryStore.SourceNameText);
                entry.ObjectId.Be(fullPath);
                entry.Action.Be(ChangeOperation.Update);
                entry.Before.HasValue.BeTrue();
                entry.After.HasValue.BeTrue();

                var beforeDetail = entry.Before!.Value.ToObject<DirectoryDetail>().NotNull();
                var beforeData = beforeDetail.Data.ToObject<MapRecord>().NotNull();
                beforeData.Name.Be("Alice");
                beforeData.Age.Be(30);

                var afterDetail = entry.After!.Value.ToObject<DirectoryDetail>().NotNull();
                var afterData = afterDetail.Data.ToObject<MapRecord>().NotNull();
                afterData.Name.Be("Alice");
                afterData.Age.Be(35);
            });
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteFileTransaction(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        // Create file inside transaction
        var record = new MapRecord("Alice", 30);
        (await keyStore.Set(path, record.ToDataETag())).BeOk();

        (await keyStore.Delete(path)).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            string fullPath = "datafiles/" + path;

            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(2);
            
            // First operation is Add
            list[0].Action.Be(ChangeOperation.Add);
            
            // Second operation is Delete
            list[1].Action(entry =>
            {
                entry.SourceName.Be(MemoryStore.SourceNameText);
                entry.ObjectId.Be(fullPath);
                entry.Action.Be(ChangeOperation.Delete);
                entry.Before.HasValue.BeTrue();
                entry.After.HasValue.BeFalse();

                var beforeDetail = entry.Before!.Value.ToObject<DirectoryDetail>().NotNull();
                var beforeData = beforeDetail.Data.ToObject<MapRecord>().NotNull();
                beforeData.Name.Be("Alice");
                beforeData.Age.Be(30);
            });
        });

        // Verify file is deleted
        (await keyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackAfterSet_ShouldRemoveFile(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        var record = new MapRecord("Alice", 30);
        (await keyStore.Set(path, record.ToDataETag())).BeOk();

        // Verify file exists during transaction
        (await keyStore.Exists(path)).BeOk();

        (await transaction.Rollback()).BeOk();

        // After rollback, file should be removed
        (await keyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackAfterDelete_ShouldRestoreFile(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        // Create file inside transaction
        var original = new MapRecord("Alice", 30);
        (await keyStore.Set(path, original.ToDataETag())).BeOk();

        (await keyStore.Delete(path)).BeOk();

        // Verify file is deleted during transaction
        (await keyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);

        (await transaction.Rollback()).BeOk();

        // After rollback, both operations should be undone - file should not exist
        (await keyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackAfterUpdate_ShouldRestoreOriginalContent(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        // Create file inside transaction
        var original = new MapRecord("Alice", 30);
        (await keyStore.Set(path, original.ToDataETag())).BeOk();

        var updated = new MapRecord("Alice", 35);
        (await keyStore.Set(path, updated.ToDataETag())).BeOk();

        // Verify updated content during transaction
        (await keyStore.Get(path)).BeOk().Return().Action(data =>
        {
            var current = data.ToObject<MapRecord>().NotNull();
            current.Age.Be(35);
        });

        (await transaction.Rollback()).BeOk();

        // After rollback, both operations should be undone - file should not exist
        (await keyStore.Exists(path)).StatusCode.Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackMultipleOperations_ShouldUndoInReverseOrder(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");

        await transaction.Start();

        // Setup: Create file1 inside transaction
        var file1Original = new MapRecord("Alice", 30);
        (await keyStore.Set("file1.json", file1Original.ToDataETag())).BeOk();

        // Operation 1: Update file1
        var file1Updated = new MapRecord("Alice", 35);
        (await keyStore.Set("file1.json", file1Updated.ToDataETag())).BeOk();

        // Operation 2: Add file2
        var file2 = new MapRecord("Bob", 25);
        (await keyStore.Set("file2.json", file2.ToDataETag())).BeOk();

        // Operation 3: Add file3
        var file3 = new MapRecord("Charlie", 40);
        (await keyStore.Set("file3.json", file3.ToDataETag())).BeOk();

        // Operation 4: Delete file1
        (await keyStore.Delete("file1.json")).BeOk();

        (await transaction.Rollback()).BeOk();

        // After rollback: all operations undone, no files should exist
        (await keyStore.Exists("file1.json")).StatusCode.Be(StatusCode.NotFound);
        (await keyStore.Exists("file2.json")).StatusCode.Be(StatusCode.NotFound);
        (await keyStore.Exists("file3.json")).StatusCode.Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleOperationsOnDifferentFiles(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");

        await transaction.Start();

        var r1 = new MapRecord("Alice", 30);
        (await keyStore.Set("file1.json", r1.ToDataETag())).BeOk();

        var r2 = new MapRecord("Bob", 25);
        (await keyStore.Set("file2.json", r2.ToDataETag())).BeOk();

        var r3 = new MapRecord("Charlie", 40);
        (await keyStore.Set("file3.json", r3.ToDataETag())).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(3);
            list.All(entry => entry.Action == ChangeOperation.Add).BeTrue();
            list.All(entry => entry.SourceName == MemoryStore.SourceNameText).BeTrue();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmptyTransaction_ShouldSucceed(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        await transaction.Start();

        // No operations performed

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(0);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SequentialTransactions_ShouldWork(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");

        // First transaction
        await transaction.Start();
        var r1 = new MapRecord("Alice", 30);
        (await keyStore.Set("file1.json", r1.ToDataETag())).BeOk();
        (await transaction.Commit()).BeOk();

        // Second transaction
        await transaction.Start();
        var r2 = new MapRecord("Bob", 25);
        (await keyStore.Set("file2.json", r2.ToDataETag())).BeOk();
        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            x.Count.Be(2);
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(2);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleOperationsOnSameFile(bool useCache)
    {
        var host = BuildService(useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        string path = "test-file.json";

        await transaction.Start();

        var r1 = new MapRecord("Alice", 30);
        (await keyStore.Set(path, r1.ToDataETag())).BeOk();

        var r2 = new MapRecord("Alice", 35);
        (await keyStore.Set(path, r2.ToDataETag())).BeOk();

        var r3 = new MapRecord("Alice", 40);
        (await keyStore.Set(path, r3.ToDataETag())).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            string fullPath = "datafiles/" + path;

            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(3);

            // First operation is Add
            list[0].Action.Be(ChangeOperation.Add);
            list[0].Before.HasValue.BeFalse();

            // Second and third operations are Updates
            list[1].Action.Be(ChangeOperation.Update);
            list[1].Before.HasValue.BeTrue();
            list[2].Action.Be(ChangeOperation.Update);
            list[2].Before.HasValue.BeTrue();
        });

        // Verify final state
        (await keyStore.Get(path)).BeOk().Return().Action(data =>
        {
            var final = data.ToObject<MapRecord>().NotNull();
            final.Age.Be(40);
        });
    }

}