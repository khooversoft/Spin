using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class FileStoreTransactionTests
{
    private ITestOutputHelper _outputHelper;

    public FileStoreTransactionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    private async Task<IHost> BuildService()
    {
        var option = new TransactionManagerOption
        {
            JournalKey = "transaction_journal"
        };

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddTransactionServices(option).AddInMemoryFileStore();
                services.AddListStore<DataChangeRecord>();
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        //await host.ClearStore<FileStoreTransactionTests>();
        return host;
    }

    [Fact]
    public async Task Startup()
    {
        using var host = await BuildService();
        var transactionManager = host.Services.GetRequiredKeyedService<TransactionManager>("default");
        transactionManager.NotNull();

        var fileStore = host.Services.GetRequiredService<IFileStore>();
        (fileStore as InMemoryFileStore).NotNull();
    }
}
