//using System.Collections.Frozen;
//using System.Security.Cryptography;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Journal;
//using Toolbox.Store;
//using Toolbox.Tools.Should;
//using Toolbox.Types;

//namespace Toolbox.Test.Journal;

//public class JournalLogBackgroundWriterTests
//{
//    private readonly IServiceProvider _services;
//    private readonly ILogger<JournalLogBackgroundWriterTests> _logger;
//    private readonly ScopeContext _context;

//    public JournalLogBackgroundWriterTests()
//    {
//        _services = new ServiceCollection()
//            .AddInMemoryFileStore()
//            .AddLogging(config =>
//            {
//                config.AddDebug();
//                config.AddFilter(x => true);
//            })
//            .AddJournalLog("test", new JournalFileOption { ConnectionString = "journal2=/journal2/data", UseBackgroundWriter = true })
//            .BuildServiceProvider();

//        _logger = _services.GetRequiredService<ILogger<JournalLogBackgroundWriterTests>>();
//        _context = new ScopeContext(_logger);
//    }

//    [Fact]
//    public async Task AddSingleJournal()
//    {
//        IFileStore fileStore = _services.GetRequiredService<IFileStore>();

//        IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test");

//        var search = await fileStore.Search("*", _context);
//        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

//        var journalEntry = new JournalEntry
//        {
//            LogSequenceNumber = "lsn1",
//            TransactionId = Guid.NewGuid().ToString(),
//            Type = JournalType.Action,
//            Data = new Dictionary<string, string?>()
//            {
//                ["key1"] = "value1",
//                ["key2"] = "value2",
//            }.ToFrozenDictionary(),
//        };

//        await journal.Write([journalEntry], _context);
//        await journal.Close();

//        var journals = await journal.ReadJournals(_context);

//        journals.Action(x =>
//        {
//            x.Count.Be(1);

//            var read = x[0];
//            (read == journalEntry).BeTrue();
//        });
//    }

//    [Fact]
//    public async Task AddMulitpleJournal()
//    {
//        IFileStore fileStore = _services.GetRequiredService<IFileStore>();
//        var queue = new Sequence<JournalEntry>();
//        var createdJournals = new Sequence<JournalEntry>();

//        var search = await fileStore.Search("**/*", _context);
//        await search.ForEachAsync(async x => await fileStore.Delete(x, _context));

//        await using (IJournalFile journal = _services.GetRequiredKeyedService<IJournalFile>("test"))
//        {
//            int writeCount = 1;
//            int currentCount = 0;

//            foreach (var item in Enumerable.Range(0, 100))
//            {
//                string v1 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
//                string v2 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

//                var journalEntry = new JournalEntry
//                {
//                    Type = JournalType.Action,
//                    Data = new Dictionary<string, string?>()
//                    {
//                        ["key1"] = v1,
//                        ["key2"] = v2,
//                    }.ToFrozenDictionary(),
//                };

//                queue += journalEntry;
//                createdJournals += journalEntry;

//                currentCount++;
//                if (currentCount >= writeCount)
//                {
//                    writeCount = Math.Min(writeCount + 5, 100);
//                    await journal.Write(queue, _context);
//                    currentCount = 0;
//                    queue.Clear();
//                }
//            }

//            if (queue.Count > 0)
//            {
//                await journal.Write(queue, _context);
//            }
//        }

//        IJournalFile journal2 = _services.GetRequiredKeyedService<IJournalFile>("test");

//        var search2 = await fileStore.Search("**/*", _context);
//        search2.Count.Be(1);
//        search2[0].StartsWith("/journal2/data").BeTrue();
//        search2[0].EndsWith(".journal2.json").BeTrue();

//        var journals = await journal2.ReadJournals(_context);
//        journals.Count.Be(createdJournals.Count);

//        for (int i = 0; i < createdJournals.Count; i++)
//        {
//            var createdJournalUpdated = createdJournals[i] with { LogSequenceNumber = journals[i].LogSequenceNumber };
//            (journals[i] == createdJournalUpdated).BeTrue($"index={i}");
//        }
//    }
//}
