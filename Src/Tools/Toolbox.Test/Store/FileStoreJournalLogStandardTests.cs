using System.Collections.Frozen;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class FileStoreJournalLogStandardTests
{
    private readonly IServiceProvider _services;
    private readonly ScopeContext _context;
    private const string _searchPath = "/journal3/data/**/*";

    public FileStoreJournalLogStandardTests(IFileStore fileStore, ITestOutputHelper outputHelper)
    {
        fileStore.NotNull();
        outputHelper.NotNull();

        _services = new ServiceCollection()
            .AddSingleton(fileStore)
            .AddLogging(config => config.AddDebug().AddLambda(x => outputHelper.WriteLine(x)))
            .AddJournalLog("test", new JournalFileOption { ConnectionString = "journal3Key=/journal3/data" })
            .BuildServiceProvider();

        var logger = _services.GetRequiredService<ILogger<FileStoreJournalLogStandardTests>>();
        _context = new ScopeContext(logger);
    }

    public async Task AddSingleJournal()
    {
        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

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
        search.Count.Be(1);
        search[0].Path.Contains("journal3/data").BeTrue();
        search[0].Path.EndsWith(".journal3Key.json").BeTrue();

        var journals = await journal.ReadJournals(_context);

        journals.Action(x =>
        {
            x.Count.Be(1);

            var readData = x[0];
            (journalEntry == readData).Be(true);
        });
    }

    public async Task AddMultipleJournal()
    {
        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

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
                await journal.Write(queue.ToArray(), _context);
                queue.Clear();
            }
        }

        if (queue.Count > 0)
        {
            await journal.Write(queue, _context);
        }

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Be(1);
        search.ForEach(x =>
        {
            x.Path.StartsWith("journal3/data").BeTrue();
            x.Path.EndsWith(".journal3Key.json").BeTrue();
        });

        var journals = await journal.ReadJournals(_context);
        journals.Count.Be(createdJournals.Count);

        for (int i = 0; i < createdJournals.Count; i++)
        {
            var createdJournalUpdated = createdJournals[i] with { LogSequenceNumber = journals[i].LogSequenceNumber };
            (journals[i] == createdJournalUpdated).BeTrue($"index={i}");
        }
    }

    public async Task AddMultipleBatchJournal()
    {
        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");
        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

        var search = await fileStore.Search(_searchPath, _context);
        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(_context));

        var createdJournals = new Sequence<JournalEntry>();

        int batchSize = 10;
        int batchCount = 30;

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

            createdJournals += batchJournals;

            await trxContext.Write(batchJournals.ToArray());
            await trxContext.Commit();
        }

        search = await fileStore.Search(_searchPath, _context);
        search.Count.Be(1);
        search.ForEach(x =>
        {
            x.Path.StartsWith("journal3/data").BeTrue();
            x.Path.EndsWith(".journal3Key.json").BeTrue();
        });

        var journals = await journal.ReadJournals(_context);
        journals.Count.Be(createdJournals.Count + batchCount * 2);

        int count = 0;
        int createdJournalIndex = 0;
        bool lookForStart = true;

        for (int i = 0; i < journals.Count; i++)
        {
            if (lookForStart)
            {
                lookForStart = false;
                journals[i].Type.Be(JournalType.Start);
                continue;
            }

            if (count == batchSize)
            {
                journals[i].Type.Be(JournalType.Commit);
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

            (journals[i] == createdJournalUpdated).BeTrue($"index={i}");
        }
    }
}
