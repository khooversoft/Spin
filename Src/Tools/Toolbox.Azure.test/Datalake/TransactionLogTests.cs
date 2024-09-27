using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Azure.test.Datalake;

public class TransactionLogTests
{
    public readonly IDatalakeStore _dataLakeStore;
    private readonly IServiceProvider _services;
    private readonly ILogger<TransactionLogTests> _logger;
    private readonly ScopeContext _context;
    private const string _searchPath = "journal1/data/**/*";

    public TransactionLogTests()
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");

        _services = new ServiceCollection()
            .AddSingleton<IDatalakeStore>(_dataLakeStore)
            .AddSingleton<IFileStore, DatalakeFileStoreConnector>()
            .AddLogging(config => config.AddDebug())
            .AddTransactionLogProvider((service, provider) =>
            {
                var option = new TransactionLogFileOption
                {
                    ConnectionString = "journal1=journal1/data",
                    MaxCount = 10,
                };

                TransactionLogFile writer = ActivatorUtilities.CreateInstance<TransactionLogFile>(service, option);
                provider.Add(writer);
            })
            .BuildServiceProvider();

        _logger = _services.GetRequiredService<ILogger<TransactionLogTests>>();
        _context = new ScopeContext(_logger);
    }

    [Fact]
    public async Task AddSingleJournal()
    {
        ITransactionLog transactionLog = _services.GetRequiredService<ITransactionLog>();
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        using ILogicalTrx trx = (await transactionLog.StartTransaction(_context)).ThrowOnError().Return();

        var journalEntry = new JournalEntry
        {
            TransactionId = trx.TransactionId,
            Type = JournalType.Action,
            Data = new Dictionary<string, string?>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            }.ToImmutableDictionary(),
        };

        await trx.Write(journalEntry);

        await trx.CommitTransaction();

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(1);
        search[0].Should().Contain("journal1/data");
        search[0].Should().EndWith(".tranLog.json");

        var readOption = await fileStore.Get(search[0], _context);
        readOption.IsOk().Should().BeTrue();

        DataETag read = readOption.Return();
        read.Data.Length.Should().BeGreaterThan(10);
        string data = read.Data.BytesToString();
        IReadOnlyList<JournalEntry> journals = TransactionLogTool.ParseJournals(data);
        journals.Action(x =>
        {
            x.Count.Should().Be(3);
            x[0].Type.Should().Be(JournalType.StartTran);

            var readData = x[1];
            var sourceData = journalEntry with { LogSequenceNumber = readData.LogSequenceNumber };
            (sourceData == readData).Should().Be(true);

            x[2].Type.Should().Be(JournalType.CommitTran);
        });
    }

    [Fact]
    public async Task AddMulitpleJournal()
    {
        ITransactionLog transactionLog = _services.GetRequiredService<ITransactionLog>();
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        using ILogicalTrx trx = (await transactionLog.StartTransaction(_context)).ThrowOnError().Return();
        var createdJournals = new Sequence<JournalEntry>();

        foreach (var item in Enumerable.Range(0, 100))
        {
            var journalEntry = new JournalEntry
            {
                TransactionId = trx.TransactionId,
                Type = JournalType.Action,
                Data = new Dictionary<string, string?>()
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2",
                }.ToImmutableDictionary(),
            };

            createdJournals += journalEntry;
            await trx.Write(journalEntry);
        }

        await trx.CommitTransaction();

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(10);
        search.ForEach(x =>
        {
            x.Should().StartWith("journal1/data");
            x.Should().EndWith(".tranLog.json");
        });

        var journalEntryListData = await search.SelectAsync(async x => (await fileStore.Get(x, _context)).ThrowOnError().Return());

        var journals = journalEntryListData
            .Select(x => x.Data.BytesToString())
            .SelectMany(x => TransactionLogTool.ParseJournals(x))
            .OrderBy(x => x.LogSequenceNumber)
            .ToArray();

        journals.Length.Should().Be(createdJournals.Count + 2);

        journals[0].Type.Should().Be(JournalType.StartTran);

        createdJournals
            .Select((source, i) => (source, jCreated: source, jRead: journals[i + 1]))
            .Select(x => (x.source, jCreated: x.jCreated with { LogSequenceNumber = x.jRead.LogSequenceNumber }, x.jRead))
            .Where(x => x.source != x.jRead)
            .Any().Should().BeTrue();

        journals[^1].Type.Should().Be(JournalType.CommitTran);
    }
}
