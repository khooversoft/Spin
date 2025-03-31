using System.Collections.Frozen;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class TransactionLogStandardTests
{
    private readonly IServiceProvider _services;
    private readonly ScopeContext _context;
    private const string _searchPath = "journal4/data/**/*";
    private const string _connectionString = "trxJournal=journal4/data";
    private const string _keyedName = "j1";

    public TransactionLogStandardTests(IFileStore fileStore, ITestOutputHelper outputHelper)
    {
        _services = new ServiceCollection()
            .AddSingleton<IFileStore>(fileStore)
            .AddLogging(config => config.AddDebug().AddLambda(x => outputHelper.WriteLine(x)))
            .AddJournalLog(_keyedName, new JournalFileOption { ConnectionString = _connectionString })
            .BuildServiceProvider();

        var logger = _services.GetRequiredService<ILogger<TransactionLogStandardTests>>();
        _context = new ScopeContext(logger);
    }

    public async Task AddSingleJournal()
    {
        IJournalFile journalFile = _services.GetRequiredKeyedService<IJournalFile>("j1");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

        IJournalTrx trx = journalFile.CreateTransactionContext();

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
        await trx.Commit();

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Should().Be(1);
        search[0].Path.Contains("journal4/data").Should().BeTrue();
        search[0].Path.EndsWith(".trxJournal.json").Should().BeTrue();

        var journals = await journalFile.ReadJournals(_context);

        journals.Action(x =>
        {
            x.Count.Should().Be(3);

            x[0].Type.Should().Be(JournalType.Start);

            var readData = x[1];
            var sourceData = journalEntry with { LogSequenceNumber = readData.LogSequenceNumber };
            (sourceData == readData).Should().Be(true);

            x[2].Type.Should().Be(JournalType.Commit);
        });
    }

    public async Task AddMulitpleJournal()
    {
        const int batchSize = 100;
        IJournalFile journalFile = _services.GetRequiredKeyedService<IJournalFile>("j1");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

        var createdJournals = new Sequence<JournalEntry>();

        await using (IJournalTrx trx = journalFile.CreateTransactionContext())
        {
            foreach (var item in Enumerable.Range(0, batchSize))
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

                createdJournals += journalEntry;
                await trx.Write([journalEntry]);
            }
        }

        var journals = await journalFile.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count + 2);

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

    public async Task AddMulitpleBatchJournal()
    {
        IJournalFile journalFile = _services.GetRequiredKeyedService<IJournalFile>("j1");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

        var createdJournals = new Sequence<JournalEntry>();

        int batchSize = 100;
        int batchCount = 100;

        foreach (var batch in Enumerable.Range(0, batchCount))
        {
            var batchJournals = new Sequence<JournalEntry>();

            await using (IJournalTrx trx = journalFile.CreateTransactionContext())
            {
                foreach (var item in Enumerable.Range(0, batchSize))
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

                    batchJournals += journalEntry;
                }

                await trx.Write(batchJournals);
                createdJournals += batchJournals;
            }
        }

        var journals = await journalFile.ReadJournals(_context);
        journals.Count.Should().Be(createdJournals.Count + (batchCount * 2));

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
