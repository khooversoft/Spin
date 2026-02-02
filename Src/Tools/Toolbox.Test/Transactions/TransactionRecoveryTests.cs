using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Transactions;

public class TransactionRecoveryTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public TransactionRecoveryTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(TransactionBindTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryKeyStore();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = basePath + "/listBase",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");

                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "keyStore",
                        ProviderName = "fileStore",
                        BasePath = basePath + "/keyStore",
                        SpaceFormat = SpaceFormat.Key,
                        UseCache = false
                    });
                    cnfg.Add<KeyStoreProvider>("fileStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddKeyStore<TestRecord>("keyStore");

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                    config.TrxProviders.Add(x => x.GetRequiredService<MemoryStore>());
                    config.CheckpointProviders.Add(x => x.GetRequiredService<MemoryStore>());
                });
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task StartWithoutErrorsThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        await transaction.Start();
    }

    [Fact]
    public async Task EmptyTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        await transaction.Start();

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }
}
