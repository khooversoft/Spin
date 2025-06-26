using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Test.Store;
using Xunit.Abstractions;

namespace Toolbox.Test.Journal;

public class JournalLogTests
{
    private readonly IServiceProvider _services;
    private readonly FileStoreJournalLogStandardTests _test;

    public JournalLogTests(ITestOutputHelper outputHelper)
    {
        _services = new ServiceCollection()
            .AddInMemoryFileStore()
            .AddLogging(config => config.AddDebug())
            .AddJournalLog("test", new JournalFileOption { ConnectionString = "journal2=/journal2/data" })
            .BuildServiceProvider();

        IFileStore fileStore = _services.GetRequiredService<IFileStore>();
        _test = new FileStoreJournalLogStandardTests(fileStore, outputHelper);
    }


    [Fact]
    public Task AddSingleJournal()
    {
        return _test.AddSingleJournal();
    }

    [Fact]
    public Task AddMultipleJournal()
    {
        return _test.AddMultipleJournal();
    }

    [Fact]
    public Task AddMultipleBatchJournal()
    {
        return _test.AddMultipleBatchJournal();
    }
}
