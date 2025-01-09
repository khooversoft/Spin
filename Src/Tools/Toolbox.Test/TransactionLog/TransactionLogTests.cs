using System.Collections.Frozen;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Test.TransactionLog;

public class TransactionLogTests
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TransactionLogTests> _logger;
    private readonly ScopeContext _context;

    public TransactionLogTests()
    {
        _services = new ServiceCollection()
            .AddInMemoryFileStore()
            .AddLogging(config => config.AddDebug())
            .AddTransactionLogProvider("journal1=/journal1/data")
            .BuildServiceProvider();

        _logger = _services.GetRequiredService<ILogger<TransactionLogTests>>();
        _context = new ScopeContext(_logger);
    }

    [Fact]
    public async Task AddSingleJournal()
    {
        ITransactionLog transactionLog = _services.GetRequiredService<ITransactionLog>();
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search("*", _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        ILogicalTrx trx = (await transactionLog.StartTransaction(_context)).ThrowOnError().Return();

        var journalEntry = new JournalEntry
        {
            TransactionId = trx.TransactionId,
            Type = JournalType.Action,
            Data = new Dictionary<string, string?>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            }.ToFrozenDictionary(),
        };

        await trx.Write([journalEntry]);

        var journals = await transactionLog.ReadJournals("journal1", _context);

        journals.Action(x =>
        {
            x.Count.Should().Be(2);

            var read = x[0];
            var source = journalEntry with { LogSequenceNumber = read.LogSequenceNumber };
            (read == source).Should().BeTrue();
            x[1].Type.Should().Be(JournalType.CommitTran);
        });
    }

    [Fact]
    public async Task AddMulitpleJournal()
    {
        ITransactionLog transactionLog = _services.GetRequiredService<ITransactionLog>();
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search("**/*", _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        ILogicalTrx trx = (await transactionLog.StartTransaction(_context)).ThrowOnError().Return();
        var createdJournals = new Sequence<JournalEntry>();

        foreach (var item in Enumerable.Range(0, 100))
        {
            string v1 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
            string v2 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

            var journalEntry = new JournalEntry
            {
                TransactionId = trx.TransactionId,
                Type = JournalType.Action,
                Data = new Dictionary<string, string?>()
                {
                    ["key1"] = v1,
                    ["key2"] = v2,
                }.ToFrozenDictionary(),
            };

            createdJournals += journalEntry;
            await trx.Write([journalEntry]);
        }

        search = await fileStore.Search("**/*", _context);
        search.Count.Should().Be(1);
        search[0].Should().StartWith("/journal1/data");
        search[0].Should().EndWith(".tranLog.json");

        var readOption = await fileStore.Get(search[0], _context);
        readOption.IsOk().Should().BeTrue();

        DataETag read = readOption.Return();
        string data = read.Data.BytesToString();

        IReadOnlyList<JournalEntry> journals = TransactionLogTool.ParseJournals(data);
        journals.Count.Should().Be(createdJournals.Count * 2);

        createdJournals
            .Select((source, i) => (source, jCreated: source, jRead: journals[i]))
            .Select(x => (x.source, jCreated: x.jCreated with { LogSequenceNumber = x.jRead.LogSequenceNumber }, x.jRead))
            .Where(x => x.source != x.jRead)
            .Any().Should().BeTrue();

        journals[^1].Type.Should().Be(JournalType.CommitTran);
    }
}
