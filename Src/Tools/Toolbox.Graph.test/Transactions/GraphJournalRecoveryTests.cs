using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Store;
using Toolbox.Types;
using Toolbox.Tools;
using Xunit.Abstractions;
using Toolbox.Models;
using Toolbox.Data;

namespace Toolbox.Graph.test.Transactions;

public class GraphJournalRecoveryTests
{
    private readonly ITestOutputHelper _logOutput;
    public GraphJournalRecoveryTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphTransactionTests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
            .ConfigureServices((context, services) =>
            {
                _ = useDatalake switch
                {
                    true => services.AddDatalakeFileStore(datalakeOption),
                    false => services.AddInMemoryFileStore(),
                };

                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();

        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var list = await fileStore.Search("**/*", context);
        await list.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SingleJournal(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        IDataClient<DataChangeRecord> changeClient = host.Services.GetRequiredService<IDataClient<DataChangeRecord>>();

        string q = """
            add node key=node1 set t1;
            add node key=node2 set t2,client;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.BeOk();
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
        });

        var recoveredMapOption = await graphEngine.DataManager.BuildFromJournals(context);
        GraphMap recoveredMap = recoveredMapOption.BeOk().Return();

        recoveredMap.LastLogSequenceNumber.NotEmpty().Be(graphEngine.DataManager.GetMap().LastLogSequenceNumber);
        var compareMap = GraphCommandTools.CompareMap(recoveredMap, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleJournals(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        IDataClient<DataChangeRecord> changeClient = host.Services.GetRequiredService<IDataClient<DataChangeRecord>>();

        string q = """
                add node key=node1 set t1;
                add node key=node2 set t2,client;
                """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.BeOk();
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
        });

        string q2 = """
                add node key=node3 set t1;
                add edge from=node1, to=node2, type=default;
                add node key=node3 set t2,client;
                add edge from=node3, to=node4, type=default;
                """;

        (await graphClient.ExecuteBatch(q2, context)).Action(x =>
        {
            x.BeError();
            x.Value.Items.Count.Be(3);

            int index = 0;
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        string q3 = """
                add node key=node3 set t1;
                add edge from=node1, to=node2, type=default;
                add node key=node4 set t2,client;
                add edge from=node3, to=node4, type=default;
                """;

        (await graphClient.ExecuteBatch(q3, context)).Action(x =>
        {
            x.BeOk();
            x.Value.Items.Count.Be(4);

            int index = 0;
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
        });

        var recoveredMapOption = await graphEngine.DataManager.BuildFromJournals(context);
        GraphMap recoveredMap = recoveredMapOption.BeOk().Return();

        recoveredMap.LastLogSequenceNumber.Be(graphEngine.DataManager.GetMap().LastLogSequenceNumber);
        var compareMap = GraphCommandTools.CompareMap(recoveredMap, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(0);
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
    }
}
