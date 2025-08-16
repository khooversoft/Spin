//using System.Collections.Immutable;
//using System.Security.Cryptography;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Models;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.DataListClient;

//public class JournalIndexingTests
//{
//    private readonly ITestOutputHelper _outputHelper;
//    private readonly LogSequenceNumber _lsn = new();
//    public JournalIndexingTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

//    private async Task<IHost> BuildService(bool addMemory, bool addQueueStore)
//    {
//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug());
//                AddStore(services);

//                services.AddDataListPipeline<DataChangeRecord>(builder =>
//                {
//                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(5);
//                    builder.BasePath = nameof(JournalIndexingTests);

//                    if (addMemory) builder.AddCacheMemory();
//                    if (addQueueStore) builder.AddQueueStore();
//                    builder.AddListStore();
//                });
//            })
//            .Build();

//        await host.ClearStore<JournalIndexingTests>();
//        return host;
//    }

//    [Theory]
//    [InlineData(false, false)]
//    //[InlineData(true, false)]
//    //[InlineData(false, true)]
//    //[InlineData(true, true)]
//    public async Task IndexJournal(bool addMemory, bool addQueueStore)
//    {
//        const string key = nameof(IndexJournal);
//        var host = await BuildService(addMemory, addQueueStore);
//        var context = host.Services.CreateContext<JournalIndexingTests>();
//        var listClient = host.Services.GetDataListClient<DataChangeRecord>();

//        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
//        const int batchSize = 10;
//        var sourceList = new Sequence<DataChangeRecord>();
//        int batchCount = 0;

//        while (!tokenSource.IsCancellationRequested)
//        {
//            context.LogDebug("Adding batch {count} with {BatchCount} items", batchCount++, batchSize);
//            var changeRecord = CreateChangeRecord();
//            sourceList += changeRecord;

//            (await listClient.Append(key, [changeRecord], context)).BeOk();
//            if (addQueueStore) (await listClient.Drain(context)).BeOk();

//            await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(100, 500)));
//        }

//        var selectedChangeRecord = sourceList.Shuffle().First();

//        DateTime timeIndex = LogSequenceNumber.ConvertToDateTime(selectedChangeRecord.GetLastLogSequenceNumber().NotEmpty());

//        //var timeRecord = await listClient.GetP.GetPartition(key, nameof(EntityModel), timeIndex, context);
//    }

//    private DataChangeRecord CreateChangeRecord()
//    {
//        var trxId = Guid.NewGuid().ToString();

//        var entries = Enumerable.Range(0, RandomNumberGenerator.GetInt32(1, 100))
//            .Select(x => new DataChangeEntry
//            {
//                LogSequenceNumber = _lsn.Next(),
//                TransactionId = trxId,
//                TypeName = nameof(EntityModel),
//                SourceName = "test-source",
//                ObjectId = $"Test-{x}",
//                Action = ChangeOperation.Add,
//                Before = new EntityModel($"test-{x}", x).ToDataETag(),
//            });

//        return new DataChangeRecord
//        {
//            TransactionId = trxId,
//            Entries = entries.ToImmutableArray(),
//        };
//    }

//    private record EntityModel
//    {
//        public EntityModel(string name, int age)
//        {
//            Name = name;
//            Age = age;
//        }

//        public string Name { get; }
//        public int Age { get; }
//    }
//}
