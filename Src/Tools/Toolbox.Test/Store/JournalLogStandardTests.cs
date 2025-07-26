//using System.Collections.Frozen;
//using System.Security.Cryptography;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.Store;

//public static class JournalLogStandardTests
//{
//    //private const string _searchPath = "/journal2/data/**/*";

//    private record JournalLogStandardTestsLogger();

//    public static async Task AddSingleJournal(IHost host, string basePath)
//    {
//        var searchPath = basePath + "/**/*";
//        IJournalClient journalClient = host.Services.GetJournalClient("test");
//        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
//        ScopeContext context = host.Services.GetRequiredService<ILogger<JournalLogStandardTestsLogger>>().ToScopeContext();

//        context.LogDebug("Deleting store");
//        var search = await fileStore.Search(searchPath, context);
//        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

//        var journalEntry = new JournalEntry
//        {
//            LogSequenceNumber = "lsn1",
//            Type = JournalType.Action,
//            Data = new Dictionary<string, string?>()
//            {
//                ["key1"] = "value1",
//                ["key2"] = "value2",
//            }.ToFrozenDictionary(),
//        };

//        await using (var trxContext = journalClient.CreateTransaction())
//        {
//            context.LogDebug("Write journal");
//            await trxContext.Write([journalEntry], context);
//        }

//        context.LogDebug("Get journal list");
//        var journals = await journalClient.GetList(context);
//        journals.BeOk();
//        journals.Return().Action(x =>
//        {
//            x.Count.Be(3);

//            var readData = x[1];
//            journalEntry = journalEntry with { TransactionId = readData.TransactionId, LogSequenceNumber = readData.LogSequenceNumber };
//            (journalEntry == readData).Be(true);
//        });

//        search = await fileStore.Search(searchPath, context);
//        search.Count.Be(1);
//        search[0].Path.StartsWith($"{basePath}/test/journalentry/").BeTrue();
//        search[0].Path.EndsWith("-journalentry.json").BeTrue();

//        var deleteList = await journalClient.DeleteList(context);
//        deleteList.BeOk();
//    }

//    public static async Task AddMultipleJournal(IHost host, string basePath)
//    {
//        var searchPath = basePath + "/**/*";
//        IJournalClient journalClient = host.Services.GetJournalClient("test");
//        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
//        ScopeContext context = host.Services.GetRequiredService<ILogger<JournalLogStandardTestsLogger>>().ToScopeContext();

//        context.LogDebug("Deleting store");
//        var search = await fileStore.Search(searchPath, context);
//        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

//        var createdJournals = new Sequence<JournalEntry>();
//        var queue = new Sequence<JournalEntry>();
//        int writeCount = 1;
//        int currentCount = 0;

//        await using (var trxContext = journalClient.CreateTransaction())
//        {
//            foreach (var item in Enumerable.Range(0, 100))
//            {
//                var journalEntry = new JournalEntry
//                {
//                    Type = JournalType.Action,
//                    Data = new Dictionary<string, string?>()
//                    {
//                        ["key1"] = "value1",
//                        ["key2"] = "value2",
//                    }.ToFrozenDictionary(),
//                };

//                queue += journalEntry;
//                createdJournals += journalEntry;

//                currentCount++;
//                if (currentCount >= writeCount)
//                {
//                    context.LogDebug("Writing journal entries");
//                    writeCount = Math.Min(writeCount + 5, 100);
//                    await trxContext.Write(queue.ToArray(), context);
//                    queue.Clear();
//                }
//            }

//            if (queue.Count > 0)
//            {
//                context.LogDebug("Completed writing journal entries");
//                await trxContext.Write(queue, context);
//            }
//        }

//        context.LogDebug("Get journal list - drain");
//        var readJournals = (await journalClient.GetList(context)).BeOk().Return();
//        readJournals.Count.Be(createdJournals.Count + 2);

//        context.LogDebug("Search file store for path entries entries");
//        search = await fileStore.Search(searchPath, context);
//        search.Count.Be(1);
//        search.ForEach(x =>
//        {
//            x.Path.StartsWith($"{basePath}/test/journalentry/").BeTrue();
//            x.Path.EndsWith("-journalentry.json").BeTrue();
//        });

//        var trimmedJournals = readJournals.Where(x => x.Type != JournalType.Start && x.Type != JournalType.Commit).ToArray();
//        trimmedJournals.Length.Be(createdJournals.Count);

//        for (int i = 0; i < createdJournals.Count; i++)
//        {
//            var createdJournalUpdated = createdJournals[i] with { TransactionId = trimmedJournals[i].TransactionId, LogSequenceNumber = trimmedJournals[i].LogSequenceNumber };
//            (trimmedJournals[i] == createdJournalUpdated).BeTrue($"index={i}");
//        }

//        var deleteList = await journalClient.DeleteList(context);
//        deleteList.BeOk();
//    }

//    public static async Task AddMultipleBatchJournal(IHost host, string basePath)
//    {
//        var searchPath = basePath + "/**/*";
//        IJournalClient journalClient = host.Services.GetJournalClient("test");
//        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
//        ScopeContext context = host.Services.GetRequiredService<ILogger<JournalLogStandardTestsLogger>>().ToScopeContext();

//        var search = await fileStore.Search(searchPath, context);
//        await search.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

//        var createdJournals = new Sequence<JournalEntry>();

//        int batchSize = 10;
//        int batchCount = 30;

//        foreach (var batch in Enumerable.Range(0, batchCount))
//        {
//            var batchJournals = new Sequence<JournalEntry>();

//            await using (var trxContext = journalClient.CreateTransaction())
//            {
//                foreach (var item in Enumerable.Range(0, batchSize))
//                {
//                    string v1 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
//                    string v2 = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

//                    var journalEntry = new JournalEntry
//                    {
//                        Type = JournalType.Action,
//                        Data = new Dictionary<string, string?>()
//                        {
//                            ["key1"] = v1,
//                            ["key2"] = v2,
//                        }.ToFrozenDictionary(),
//                    };

//                    batchJournals += journalEntry;
//                }

//                createdJournals += batchJournals;

//                await trxContext.Write(batchJournals.ToArray(), context);
//            }
//        }

//        await journalClient.Drain(context);

//        search = await fileStore.Search(searchPath, context);
//        search.Count.Be(1);
//        search.ForEach(x =>
//        {
//            x.Path.StartsWith($"{basePath}/test/journalentry/").BeTrue();
//            x.Path.EndsWith("-journalentry.json").BeTrue();
//        });

//        var journals = (await journalClient.GetList(context)).BeOk().Return();
//        journals.Count.Be(createdJournals.Count + batchCount * 2);

//        int count = 0;
//        int createdJournalIndex = 0;
//        bool lookForStart = true;

//        for (int i = 0; i < journals.Count; i++)
//        {
//            if (lookForStart)
//            {
//                lookForStart = false;
//                journals[i].Type.Be(JournalType.Start, $"Should be 'Start' but {journals[i].Type}, i={i}");
//                continue;
//            }

//            if (count == batchSize)
//            {
//                journals[i].Type.Be(JournalType.Commit, $"Should be 'Commit' but {journals[i].Type}, i={i}");
//                count = 0;
//                lookForStart = true;
//                continue;
//            }

//            count++;
//            var createdJournalUpdated = createdJournals[createdJournalIndex++] with
//            {
//                TransactionId = journals[i].TransactionId,
//                LogSequenceNumber = journals[i].LogSequenceNumber,
//            };

//            (journals[i] == createdJournalUpdated).BeTrue($"index={i}");
//        }

//        var deleteList = await journalClient.DeleteList(context);
//        deleteList.BeOk();
//    }
//}
