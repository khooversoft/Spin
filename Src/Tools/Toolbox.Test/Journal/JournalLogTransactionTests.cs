using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        IJournalWriter journal = _services.GetRequiredKeyedService<IJournalWriter>("test");
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
            await trx.Write([journalEntry], _context);
        }

        var journals = await journal.ReadJournals(_context);

        journals.Action(x =>
        {
            x.Count.Should().Be(2);

            var journalEntryUpdate = journalEntry with { TransactionId = trxId };
            var read = x[0];
            (read == journalEntryUpdate).Should().BeTrue();
            journalEntry.TransactionId.Should().NotBe(trxId);

            x[1].Type.Should().Be(JournalType.Commit);
        });
    }

    [Fact]
    public async Task AddMulitpleJournal()
    {
        IJournalWriter journal = _services.GetRequiredKeyedService<IJournalWriter>("test");
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

            await trx.Write(batch, _context);
            await trx.Commit(_context);
        }

        search = await fileStore.Search("**/*", _context);
        search.Count.Should().Be(1);
        search[0].Should().StartWith("/journal2/data");
        search[0].Should().EndWith(".journal2.json");

        var journals = await journal.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count + batchSize);

        int count = 0;
        int createdJournalIndex = 0;

        for (int i = 0; i < journals.Count; i++)
        {
            if (count == batchSize)
            {
                journals[i].Type.Should().Be(JournalType.Commit);
                count = 0;
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
