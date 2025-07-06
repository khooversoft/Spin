using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Test.Store;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Journal;

public class JournalLogTests
{
    private const string _basePath = "journal4/data";
    private readonly IHost _host;

    public JournalLogTests(ITestOutputHelper outputHelper)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddDebug().AddLambda(x => outputHelper.WriteLine(x)).AddFilter(x => true));
                services.AddInMemoryFileStore();
                services.AddJournalPipeline("test", builder =>
                {
                    builder.BasePath = _basePath;
                    builder.AddListStore();
                });
            })
            .Build();
    }


    [Fact]
    public Task AddSingleJournal()
    {
        return JournalLogStandardTests.AddSingleJournal(_host, _basePath);
    }

    [Fact]
    public Task AddMultipleJournal()
    {
        return JournalLogStandardTests.AddMultipleJournal(_host, _basePath);
    }

    [Fact]
    public Task AddMultipleBatchJournal()
    {
        return JournalLogStandardTests.AddMultipleBatchJournal(_host, _basePath);
    }
}
