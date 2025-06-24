using System.Collections.Frozen;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Test.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Journal;

public class JournalLogBackgroundWriterTests
{
    private readonly IServiceProvider _services;
    private readonly FileStoreJournalLogStandardTests _test;

    public JournalLogBackgroundWriterTests(ITestOutputHelper testOutputHelper)
    {
        testOutputHelper.NotNull();

        _services = new ServiceCollection()
            .AddInMemoryFileStore()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddLambda(x => testOutputHelper.WriteLine(x.ToString()));
                config.AddFilter(x => true);
            })
            .AddJournalLog("test", new JournalFileOption { ConnectionString = "journal2=/journal2/data", UseBackgroundWriter = true })
            .BuildServiceProvider();

        IFileStore fileStore = _services.GetRequiredService<IFileStore>();
        _test = new FileStoreJournalLogStandardTests(fileStore, testOutputHelper);
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
