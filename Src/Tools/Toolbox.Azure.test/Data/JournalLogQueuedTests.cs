//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure.test.Application;
//using Toolbox.Data;
//using Toolbox.Test.Store;
//using Toolbox.Tools;
//using Xunit.Abstractions;

//namespace Toolbox.Azure.test.Data;

//public class JournalLogQueuedTests
//{
//    private readonly IHost _host;
//    private const string _basePath = "journal5/queue-data";

//    public JournalLogQueuedTests(ITestOutputHelper outputHelper)
//    {
//        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");

//        _host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(config => config.AddDebug().AddLambda(x => outputHelper.WriteLine(x)));
//                services.AddDatalakeFileStore(datalakeOption);
//                services.AddJournalPipeline("test", builder =>
//                {
//                    builder.BasePath = _basePath;
//                    builder.AddQueueStore();
//                    builder.AddListStore();
//                });
//            })
//            .Build();
//    }


//    [Fact]
//    public Task AddSingleJournal()
//    {
//        return JournalLogStandardTests.AddSingleJournal(_host, _basePath);
//    }

//    [Fact]
//    public Task AddMultipleJournal()
//    {
//        return JournalLogStandardTests.AddMultipleJournal(_host, _basePath);
//    }

//    [Fact]
//    public Task AddMultipleBatchJournal()
//    {
//        return JournalLogStandardTests.AddMultipleBatchJournal(_host, _basePath);
//    }
//}
