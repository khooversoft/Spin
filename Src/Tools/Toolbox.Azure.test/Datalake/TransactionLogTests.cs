using System.Collections.Frozen;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Test.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class TransactionLogTests
{
    private readonly FileStoreJournalLogStandardTests _test;

    public TransactionLogTests(ITestOutputHelper outputHelper)
    {
        var fileStore = TestApplication.GetDatalake("datastore-tests");
        _test = new FileStoreJournalLogStandardTests(fileStore, outputHelper);
    }

    [Fact]
    public Task AddSingleJournal()
    {
        return _test.AddSingleJournal();
    }

    [Fact]
    public Task AddMulitpleJournal()
    {
        return _test.AddMulitpleJournal();
    }

    [Fact]
    public Task AddMulitpleBatchJournal()
    {
        return _test.AddMulitpleBatchJournal();
    }
}
