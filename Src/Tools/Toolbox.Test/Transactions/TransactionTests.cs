using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Transactions;

public class TransactionTests
{
    private ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age);

    public TransactionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                });
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task StartWithoutRollbackThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        await Verify.ThrowsAsync<ArgumentException>(() => transaction.Start());
    }

    [Fact]
    public async Task EmptyTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();

        var result = await transaction.Commit();
        result.BeOk();
        rollbackCount.Be(0);

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }

    [Fact]
    public async Task SingleTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        var je1 = new JournalEntry("Alice", 30);
        transaction.TrxRecorder.Add("source1", "id1", je1);

        var result = await transaction.Commit();
        result.BeOk();
        rollbackCount.Be(0);

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Action(x =>
        {
            x.Entries.Count.Be(1);
            x.Entries[0].Action(y =>
            {
                y.TypeName.Be(typeof(JournalEntry).Name);
                y.SourceName.Be("source1");
                y.ObjectId.Be("id1");
                y.Action.Be(ChangeOperation.Add);
                (y.Before == null).BeTrue();

                var jread = y.After?.ToObject<JournalEntry>() ?? throw new ArgumentException();
                jread.Name.Be("Alice");
                jread.Age.Be(30);
            });
        });
    }

    [Fact]
    public async Task SingleTransactionWithRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);

            entry.TypeName.Be(typeof(JournalEntry).Name);
            entry.SourceName.Be("source1");
            entry.ObjectId.Be("id1");
            entry.Action.Be(ChangeOperation.Add);
            (entry.Before == null).BeTrue();

            var jread = entry.After?.ToObject<JournalEntry>() ?? throw new ArgumentException();
            jread.Name.Be("Alice");
            jread.Age.Be(30);

            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        var je1 = new JournalEntry("Alice", 30);
        transaction.TrxRecorder.Add("source1", "id1", je1);

        var result = await transaction.Rollback();
        result.BeOk();
        rollbackCount.Be(1);
    }

    [Fact]
    public async Task MultipleEntriesTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        JournalEntry j1 = new("Alice", 30);
        JournalEntry j2 = new("Bob", 25);
        JournalEntry j3 = new("Charlie", 35);

        transaction.TrxRecorder.Add("source1", "id1", j1);
        transaction.TrxRecorder.Add("source2", "id2", j2);
        transaction.TrxRecorder.Add("source3", "id3", j3);

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        var list = data.SelectMany(x => x.Entries).ToList();
        list.Count.Be(3);
        list.Select(x => x.TypeName).SequenceEqual(["JournalEntry", "JournalEntry", "JournalEntry"]);
        list.Select(x => x.SourceName).SequenceEqual(["source1", "source2", "source3"]);
        list.Select(x => x.ObjectId).SequenceEqual(["id1", "id2", "id3"]);
        list.Select(x => x.Action).SequenceEqual([ChangeOperation.Add, ChangeOperation.Add, ChangeOperation.Add]);
        list.Count(x => x.Before == null).Be(3);
        list.Count(x => x.After != null).Be(3);
        list.Select(x => x.After!.Value.ToObject<JournalEntry>()).SequenceEqual([j1, j2, j3]).BeTrue();
    }

    [Fact]
    public async Task RollbackEntriesInReverseOrder()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        List<string> rollbackOrder = new();

        transaction.EnlistLambda("source", entry =>
        {
            rollbackOrder.Add(entry.ObjectId);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        transaction.TrxRecorder.Add("source", "first", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Add("source", "second", new JournalEntry("Bob", 25));
        transaction.TrxRecorder.Add("source", "third", new JournalEntry("Charlie", 35));

        var result = await transaction.Rollback();
        result.BeOk();

        rollbackOrder.Count.Be(3);
        rollbackOrder[0].Be("third");
        rollbackOrder[1].Be("second");
        rollbackOrder[2].Be("first");
    }

    [Fact]
    public async Task RollbackFailureReturnsConflict()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.Conflict, "Rollback failed").ToTaskResult());

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        var result = await transaction.Rollback();
        result.StatusCode.Be(StatusCode.Conflict);
    }

    [Fact]
    public async Task MultipleRollbackHandlersAllCalled()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int handler1Count = 0;
        int handler2Count = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref handler1Count);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        transaction.EnlistLambda("source2", entry =>
        {
            Interlocked.Increment(ref handler2Count);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Add("source2", "id1", new JournalEntry("Bob", 30));

        var result = await transaction.Rollback();
        result.BeOk();
        handler1Count.Be(1);
        handler2Count.Be(1);
    }

    [Fact]
    public async Task StartTwiceThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();

        await Verify.ThrowsAsync<ArgumentException>(() => transaction.Start());
    }

    [Fact]
    public async Task TransactionIdChangesOnEachStart()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        string firstTransactionId = transaction.TransactionId;

        // Complete first transaction
        await transaction.Commit();

        await transaction.Start();
        string secondTransactionId = transaction.TransactionId;

        firstTransactionId.NotBe(secondTransactionId);
    }

    [Fact]
    public async Task CommitWithoutStartThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await Verify.ThrowsAsync<ArgumentException>(async () => await transaction.Commit());
    }

    [Fact]
    public async Task RollbackWithoutStartThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await Verify.ThrowsAsync<ArgumentException>(async () => await transaction.Rollback());
    }

    [Fact]
    public async Task RunStateTransitionsThroughRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        transaction.RunState.Be(RunState.None);

        await transaction.Start();
        transaction.RunState.Be(RunState.Active);

        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        await transaction.Rollback();
        transaction.RunState.Be(RunState.None);
    }

    [Fact]
    public async Task PartialRollbackFailureStillProcessesAllHandlers()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int handler1Count = 0;
        int handler2Count = 0;
        int handler3Count = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref handler1Count);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        transaction.EnlistLambda("source2", entry =>
        {
            Interlocked.Increment(ref handler2Count);
            return new Option(StatusCode.InternalServerError, "Handler 2 failed").ToTaskResult();
        });

        transaction.EnlistLambda("source3", entry =>
        {
            Interlocked.Increment(ref handler3Count);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Add("source2", "id1", new JournalEntry("Alice-2", 30));
        transaction.TrxRecorder.Add("source3", "id1", new JournalEntry("Alice-3", 30));

        var result = await transaction.Rollback();
        result.StatusCode.Be(StatusCode.InternalServerError);
        transaction.RunState.Be(RunState.None);

        (handler1Count + handler2Count + handler3Count).Be(2);
    }

    [Fact]
    public async Task CommitClearsQueueAfterSuccess()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        var result = await transaction.Commit();
        result.BeOk();

        // Start a new transaction to verify queue was cleared
        await transaction.Start();
        var result2 = await transaction.Commit();
        result2.BeOk();

        // Verify only the second (empty) transaction was committed
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(2);
        data[1].Entries.Count.Be(0);
    }

    [Fact]
    public async Task MultipleCommitsWithSameTransactionInstance()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        // First transaction
        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));
        var result1 = await transaction.Commit();
        result1.BeOk();

        // Second transaction
        await transaction.Start();
        transaction.TrxRecorder.Add("source2", "id2", new JournalEntry("Bob", 25));
        var result2 = await transaction.Commit();
        result2.BeOk();

        // Verify both transactions were committed
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(2);
        data[0].Entries.Count.Be(1);
        data[1].Entries.Count.Be(1);
    }

    [Fact]
    public async Task EmptyRollbackSucceeds()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();

        var result = await transaction.Rollback();
        result.BeOk();
        rollbackCount.Be(0);
    }

    [Fact]
    public async Task TransactionIdIsConsistentDuringTransaction()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        string capturedTransactionId = string.Empty;

        transaction.EnlistLambda("source1", entry =>
        {
            capturedTransactionId = entry.TransactionId;
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        string startTransactionId = transaction.TransactionId;

        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        await transaction.Rollback();

        startTransactionId.Be(capturedTransactionId);
    }

    [Fact]
    public async Task RollbackWithMultipleEntriesCallsHandlersForEachEntry()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int callCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref callCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Add("source1", "id2", new JournalEntry("Bob", 25));
        transaction.TrxRecorder.Add("source1", "id3", new JournalEntry("Charlie", 35));

        await transaction.Rollback();

        callCount.Be(3);
    }

    [Fact]
    public async Task CommitPreservesTransactionIdInJournal()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        string expectedTransactionId = transaction.TransactionId;
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        await transaction.Commit();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data[0].TransactionId.Be(expectedTransactionId);
    }

    [Fact]
    public async Task DeleteOperationWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        var je1 = new JournalEntry("Alice", 30);
        transaction.TrxRecorder.Delete("source1", "id1", je1);

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Action(x =>
        {
            x.Entries.Count.Be(1);
            x.Entries[0].Action(y =>
            {
                y.TypeName.Be(typeof(JournalEntry).Name);
                y.SourceName.Be("source1");
                y.ObjectId.Be("id1");
                y.Action.Be(ChangeOperation.Delete);
                (y.After == null).BeTrue();

                var jread = y.Before?.ToObject<JournalEntry>() ?? throw new ArgumentException();
                jread.Name.Be("Alice");
                jread.Age.Be(30);
            });
        });
    }

    [Fact]
    public async Task DeleteOperationWithRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);

            entry.TypeName.Be(typeof(JournalEntry).Name);
            entry.SourceName.Be("source1");
            entry.ObjectId.Be("id1");
            entry.Action.Be(ChangeOperation.Delete);
            (entry.After == null).BeTrue();

            var jread = entry.Before?.ToObject<JournalEntry>() ?? throw new ArgumentException();
            jread.Name.Be("Alice");
            jread.Age.Be(30);

            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        var je1 = new JournalEntry("Alice", 30);
        transaction.TrxRecorder.Delete("source1", "id1", je1);

        var result = await transaction.Rollback();
        result.BeOk();
        rollbackCount.Be(1);
    }

    [Fact]
    public async Task UpdateOperationWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        var oldEntry = new JournalEntry("Alice", 30);
        var newEntry = new JournalEntry("Alice", 31);
        transaction.TrxRecorder.Update("source1", "id1", oldEntry, newEntry);

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Action(x =>
        {
            x.Entries.Count.Be(1);
            x.Entries[0].Action(y =>
            {
                y.TypeName.Be(typeof(JournalEntry).Name);
                y.SourceName.Be("source1");
                y.ObjectId.Be("id1");
                y.Action.Be(ChangeOperation.Update);

                var beforeRead = y.Before?.ToObject<JournalEntry>() ?? throw new ArgumentException();
                beforeRead.Name.Be("Alice");
                beforeRead.Age.Be(30);

                var afterRead = y.After?.ToObject<JournalEntry>() ?? throw new ArgumentException();
                afterRead.Name.Be("Alice");
                afterRead.Age.Be(31);
            });
        });
    }

    [Fact]
    public async Task UpdateOperationWithRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);

            entry.TypeName.Be(typeof(JournalEntry).Name);
            entry.SourceName.Be("source1");
            entry.ObjectId.Be("id1");
            entry.Action.Be(ChangeOperation.Update);

            var beforeRead = entry.Before?.ToObject<JournalEntry>() ?? throw new ArgumentException();
            beforeRead.Name.Be("Alice");
            beforeRead.Age.Be(30);

            var afterRead = entry.After?.ToObject<JournalEntry>() ?? throw new ArgumentException();
            afterRead.Name.Be("Alice");
            afterRead.Age.Be(31);

            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        var oldEntry = new JournalEntry("Alice", 30);
        var newEntry = new JournalEntry("Alice", 31);
        transaction.TrxRecorder.Update("source1", "id1", oldEntry, newEntry);

        var result = await transaction.Rollback();
        result.BeOk();
        rollbackCount.Be(1);
    }

    [Fact]
    public async Task MixedOperationsWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Delete("source2", "id2", new JournalEntry("Bob", 25));
        transaction.TrxRecorder.Update("source3", "id3", new JournalEntry("Charlie", 35), new JournalEntry("Charlie", 36));

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        var list = data.SelectMany(x => x.Entries).ToList();
        list.Count.Be(3);
        list.Select(x => x.Action).SequenceEqual([ChangeOperation.Add, ChangeOperation.Delete, ChangeOperation.Update]);
        list[0].Before.BeNull();
        list[0].After.NotNull();
        list[1].Before.NotNull();
        list[1].After.BeNull();
        list[2].Before.NotNull();
        list[2].After.NotNull();
    }

    [Fact]
    public async Task MultipleDeleteOperationsWithRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        List<string> rollbackOrder = new();

        transaction.EnlistLambda("source", entry =>
        {
            rollbackOrder.Add(entry.ObjectId);
            entry.Action.Be(ChangeOperation.Delete);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        await transaction.Start();
        transaction.TrxRecorder.Delete("source", "first", new JournalEntry("Alice", 30));
        transaction.TrxRecorder.Delete("source", "second", new JournalEntry("Bob", 25));
        transaction.TrxRecorder.Delete("source", "third", new JournalEntry("Charlie", 35));

        var result = await transaction.Rollback();
        result.BeOk();

        rollbackOrder.Count.Be(3);
        rollbackOrder[0].Be("third");
        rollbackOrder[1].Be("second");
        rollbackOrder[2].Be("first");
    }

    [Fact]
    public async Task MultipleUpdateOperationsWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        await transaction.Start();
        transaction.TrxRecorder.Update("source1", "id1", new JournalEntry("Alice", 30), new JournalEntry("Alice", 31));
        transaction.TrxRecorder.Update("source2", "id2", new JournalEntry("Bob", 25), new JournalEntry("Bob", 26));
        transaction.TrxRecorder.Update("source3", "id3", new JournalEntry("Charlie", 35), new JournalEntry("Charlie", 36));

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        var list = data.SelectMany(x => x.Entries).ToList();
        list.Count.Be(3);
        list.All(x => x.Action == ChangeOperation.Update).BeTrue();
        list.All(x => x.Before != null && x.After != null).BeTrue();
    }

    [Fact]
    public async Task RunStateTransitionsThroughLifecycle()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        transaction.EnlistLambda("source1", entry => new Option(StatusCode.OK).ToTaskResult());

        // Initial state
        transaction.RunState.Be(RunState.None);

        // After start
        await transaction.Start();
        transaction.RunState.Be(RunState.Active);

        // After commit
        await transaction.Commit();
        transaction.RunState.Be(RunState.None);

        // Start again for rollback test
        await transaction.Start();
        transaction.RunState.Be(RunState.Active);
        transaction.TrxRecorder.Add("source1", "id1", new JournalEntry("Alice", 30));

        // After rollback
        await transaction.Rollback();
        transaction.RunState.Be(RunState.None);
    }
}
