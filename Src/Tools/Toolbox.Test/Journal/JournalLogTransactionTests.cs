using System.Collections.Frozen;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Test.Journal;

public class JournalLogTransactionTests
{
    private readonly IServiceProvider _services;
    private readonly ILogger<JournalLogTransactionTests> _logger;
    private readonly ScopeContext _context;

    public JournalLogTransactionTests()
    {
        _services = new ServiceCollection()
            .AddInMemoryFileStore()
            .AddLogging(config => config.AddDebug())
            .AddJournalLog("test", "journal2=/journal2/data")
            .BuildServiceProvider();

        _logger = _services.GetRequiredService<ILogger<JournalLogTransactionTests>>();
        _context = new ScopeContext(_logger);
    }

    [Fact]
    public async Task AddSingleJournalTrx()
    {
        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search("*", _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        var trxId = Guid.NewGuid().ToString();

        var journalEntry = new JournalEntry
        {
            LogSequenceNumber = "lsn1",
            Type = JournalType.Action,
            Data = new Dictionary<string, string?>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            }.ToFrozenDictionary(),
        };

        await using (var trx = journal.CreateTransactionContext(trxId))
        {
            await trx.Write([journalEntry]);
        }

        var journals = await journal.ReadJournals(_context);

        journals.Action(x =>
        {
            x.Count.Should().Be(3);
            x[0].Type.Should().Be(JournalType.Start);

            var journalEntryUpdate = journalEntry with { TransactionId = trxId };
            var read = x[1];
            (read == journalEntryUpdate).Should().BeTrue();
            journalEntry.TransactionId.Should().NotBe(trxId);

            x[2].Type.Should().Be(JournalType.Commit);
        });
    }

    [Fact]
    public async Task AddMulitpleJournal()
    {
        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();
        const int batchSize = 100;

        var search = await fileStore.Search("**/*", _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        var createdJournals = new Sequence<JournalEntry>();

        foreach (var outter in Enumerable.Range(0, 100))
        {
            var trx = journal.CreateTransactionContext();
            var batch = new Sequence<JournalEntry>();

            foreach (var item in Enumerable.Range(0, batchSize))
            {
                string v1 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
                string v2 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

                var journalEntry = new JournalEntry
                {
                    Type = JournalType.Action,
                    Data = new Dictionary<string, string?>()
                    {
                        ["key1"] = v1,
                        ["key2"] = v2,
                    }.ToFrozenDictionary(),
                };

                batch += journalEntry;
                createdJournals += journalEntry;
            }

            await trx.Write(batch);
            await trx.Commit();
        }

        search = await fileStore.Search("**/*", _context);
        search.Count.Should().Be(1);
        search[0].Should().StartWith("/journal2/data");
        search[0].Should().EndWith(".journal2.json");

        var journals = await journal.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count + (batchSize * 2));

        int count = 0;
        int createdJournalIndex = 0;
        bool lookForStart = true;

        for (int i = 0; i < journals.Count; i++)
        {
            if (lookForStart)
            {
                lookForStart = false;
                journals[i].Type.Should().Be(JournalType.Start);
                continue;
            }

            if (count == batchSize)
            {
                journals[i].Type.Should().Be(JournalType.Commit);
                count = 0;
                lookForStart = true;
                continue;
            }

            count++;
            var createdJournalUpdated = createdJournals[createdJournalIndex++] with
            {
                TransactionId = journals[i].TransactionId,
                LogSequenceNumber = journals[i].LogSequenceNumber,
            };

            (journals[i] == createdJournalUpdated).Should().BeTrue($"index={i}");
        }
    }

}
