using System.Collections.Frozen;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Azure.test.Datalake;

public class JournalLogTests
{
    public readonly IDatalakeStore _dataLakeStore;
    private readonly IServiceProvider _services;
    private readonly ILogger<JournalLogTests> _logger;
    private readonly ScopeContext _context;
    private const string _searchPath = "journal2/data/**/*";

    public JournalLogTests()
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");

        _services = new ServiceCollection()
            .AddSingleton<IDatalakeStore>(_dataLakeStore)
            .AddSingleton<IFileStore, DatalakeFileStoreConnector>()
            .AddLogging(config => config.AddDebug())
            .AddJournalLog("test", "journal2Key=/journal2/data")
            .BuildServiceProvider();

        _logger = _services.GetRequiredService<ILogger<JournalLogTests>>();
        _context = new ScopeContext(_logger);
    }

    [Fact]
    public async Task AddSingleJournal()
    {
        IJournalWriter journal = _services.GetRequiredKeyedService<IJournalWriter>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        var journalEntry = new JournalEntry
        {
            LogSequenceNumber = "lsn1",
            TransactionId = Guid.NewGuid().ToString(),
            Type = JournalType.Action,
            Data = new Dictionary<string, string?>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            }.ToFrozenDictionary(),
        };

        await journal.Write([journalEntry], _context);

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(1);
        search[0].Should().Contain("journal2/data");
        search[0].Should().EndWith(".journal2Key.json");

        var journals = await journal.ReadJournals(_context);

        journals.Action(x =>
        {
            x.Count.Should().Be(1);

            var readData = x[0];
            (journalEntry == readData).Should().Be(true);
        });
    }

    [Fact]
    public async Task AddMulitpleJournal()
    {
        IJournalWriter journal = _services.GetRequiredKeyedService<IJournalWriter>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        var createdJournals = new Sequence<JournalEntry>();
        var queue = new Sequence<JournalEntry>();
        int writeCount = 1;
        int currentCount = 0;

        foreach (var item in Enumerable.Range(0, 100))
        {
            var journalEntry = new JournalEntry
            {
                Type = JournalType.Action,
                Data = new Dictionary<string, string?>()
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2",
                }.ToFrozenDictionary(),
            };

            queue += journalEntry;
            createdJournals += journalEntry;

            currentCount++;
            if (currentCount >= writeCount)
            {
                writeCount = Math.Min(writeCount + 5, 100);
                await journal.Write(queue, _context);
                queue.Clear();
            }
        }

        if (queue.Count > 0)
        {
            await journal.Write(queue, _context);
        }

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(1);
        search.ForEach(x =>
        {
            x.Should().StartWith("journal2/data");
            x.Should().EndWith(".journal2Key.json");
        });

        var journals = await journal.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count);

        for (int i = 0; i < createdJournals.Count; i++)
        {
            var createdJournalUpdated = createdJournals[i] with { LogSequenceNumber = journals[i].LogSequenceNumber };
            (journals[i] == createdJournalUpdated).Should().BeTrue($"index={i}");
        }
    }

    [Fact]
    public async Task AddMulitpleBatchJournal()
    {
        IJournalWriter journal = _services.GetRequiredKeyedService<IJournalWriter>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

        var createdJournals = new Sequence<JournalEntry>();

        int batchSize = 100;
        int batchCount = 100;

        foreach (var batch in Enumerable.Range(0, batchCount))
        {
            var batchJournals = new Sequence<JournalEntry>();

            var trxContext = journal.CreateTransactionContext();

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

                batchJournals += journalEntry;
            }

            await trxContext.Write(batchJournals, _context);
            createdJournals += batchJournals;
        }

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(1);
        search.ForEach(x =>
        {
            x.Should().StartWith("journal2/data");
            x.Should().EndWith(".journal2Key.json");
        });

        var journals = await journal.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count);

        for (int i = 0; i < createdJournals.Count; i++)
        {
            var createdJournalUpdated = createdJournals[i] with 
            {
                LogSequenceNumber = journals[i].LogSequenceNumber,
                TransactionId = journals[i].TransactionId,
            };

            (journals[i] == createdJournalUpdated).Should().BeTrue($"index={i}");
        }
    }
}
