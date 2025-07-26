//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Test.Store;
//using Toolbox.Tools;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.Journal;

//public class JournalLogQueuedTests
//{
//    private const string _basePath = "journal4/queue-data";
//    private readonly ITestOutputHelper _outputHelper;

//    public JournalLogQueuedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    [Fact]
//    public async Task AddSingleJournal()
//    {
//        using var host = BuildService();
//        await JournalLogStandardTests.AddSingleJournal(host, _basePath);
//    }

//    [Fact]
//    public async Task AddMultipleJournal()
//    {
//        using var host = BuildService();
//        await JournalLogStandardTests.AddMultipleJournal(host, _basePath);
//    }

//    [Fact]
//    public async Task AddMultipleBatchJournal()
//    {
//        using var host = BuildService();
//        await JournalLogStandardTests.AddMultipleBatchJournal(host, _basePath);
//    }

//    private IHost BuildService()
//    {
//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(config => config.AddDebug().AddLambda(x => _outputHelper.WriteLine(x)).AddFilter(x => true));
//                services.AddInMemoryFileStore();
//                services.AddJournalPipeline("test", builder =>
//                {
//                    builder.BasePath = _basePath;
//                    builder.AddQueueStore();
//                    builder.AddListStore();
//                });
//            })
//            .Build();

//        return host;
//    }
//}
