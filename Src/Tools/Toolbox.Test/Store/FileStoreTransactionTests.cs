//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Store;

//public class FileStoreTransactionTests
//{
//    private ITestOutputHelper _outputHelper;

//    public FileStoreTransactionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

//    private async Task<IHost> BuildService()
//    {
//        var option = new TransactionManagerOption
//        {
//            JournalKey = "transaction_journal"
//        };

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
//                services.AddTransactionServices(option).AddInMemoryFileStore();
//                services.AddListStore<DataChangeRecord>();
//            })
//            .Build();

//        // Clear the store before running tests, this includes any locked files
//        //await host.ClearStore<FileStoreTransactionTests>();
//        return host;
//    }

//    [Fact]
//    public async Task Startup()
//    {
//        using var host = await BuildService();
//        var transactionManager = host.Services.GetRequiredKeyedService<TransactionManager>("default");
//        transactionManager.NotNull();

//        var fileStore = host.Services.GetRequiredService<IFileStore>();
//        (fileStore as InMemoryFileStore).NotNull();
//    }

//    //[Fact]
//    //public async Task GivenFileAdd_WhenCommit_ShouldPersistFile()
//    //{
//    //    using var host = await BuildService();
//    //    var memoryStore = host.Services.GetRequiredService<MemoryStore>();
//    //    var trxManager = host.Services.GetRequiredKeyedService<TransactionManager>("default");
//    //    var fileStore = host.Services.GetRequiredService<IFileStore>();
//    //    var context = host.Services.CreateContext<FileStoreTransactionTests>();

//    //    trxManager.InitializeProviders(context);

//    //    await trxManager.Start(context);

//    //    const string path = "test/transaction/file1.txt";
//    //    var data = new DataETag("test data".ToBytes());
//    //    var addResult = await fileStore.File(path).Add(data, context);
//    //    addResult.BeOk();

//    //    await trxManager.Commit(context);

//    //    var existResult = await fileStore.File(path).Exists(context);
//    //    existResult.BeOk();

//    //    var getResult = await fileStore.File(path).Get(context);
//    //    getResult.BeOk();
//    //    getResult.Return().Data.SequenceEqual(data.Data.ToArray()).BeTrue();
//    //}
//}
