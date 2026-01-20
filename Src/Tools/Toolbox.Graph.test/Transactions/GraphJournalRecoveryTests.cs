//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Transactions;

//public class GraphJournalRecoveryTests
//{
//    private readonly ITestOutputHelper _logOutput;
//    public GraphJournalRecoveryTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

//    private async Task<IHost> CreateService(bool useDatalake)
//    {
//        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphJournalRecoveryTests");

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
//            .ConfigureServices((context, services) =>
//            {
//                _ = useDatalake switch
//                {
//                    true => services.AddDatalakeFileStore(datalakeOption),
//                    false => services.AddInMemoryKeyStore(),
//                };

//                services.AddGraphEngine(config => config.BasePath = "basePath");
//            })
//            .Build();

//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();

//        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
//        await fileStore.ClearStore(context);

//        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        await graphEngine.DataManager.LoadDatabase(context);

//        return host;
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task SingleJournal(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IListStore<DataChangeRecord> changeClient = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

//        string q = """
//            add node key=node1 set t1;
//            add node key=node2 set t2,client;
//            """;

//        (await graphClient.ExecuteBatch(q)).Action(x =>
//        {
//            x.BeOk();
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
//        });

//        var recoveredMapOption = await graphEngine.DataManager.BuildFromJournals(context);
//        GraphMap recoveredMap = recoveredMapOption.BeOk().Return();

//        recoveredMap.LastLogSequenceNumber.NotEmpty().Be(graphEngine.DataManager.GetMap().LastLogSequenceNumber);
//        var compareMap = GraphCommandTools.CompareMap(recoveredMap, graphEngine.DataManager.GetMap());
//        compareMap.Count.Be(0);
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task MultipleJournals(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        IListStore<DataChangeRecord> changeClient = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

//        string q = """
//                add node key=node1 set t1;
//                add node key=node2 set t2,client;
//                """;

//        (await graphClient.ExecuteBatch(q)).Action(x =>
//        {
//            x.BeOk();
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
//        });

//        string q2 = """
//                add node key=node3 set t1;
//                add edge from=node1, to=node2, type=default;
//                add node key=node3 set t2,client;
//                add edge from=node3, to=node4, type=default;
//                """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.BeError();
//            x.Value.Items.Count.Be(3);

//            int index = 0;
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.Conflict));
//        });

//        string q3 = """
//                add node key=node3 set t1;
//                add edge from=node1, to=node2, type=default;
//                add node key=node4 set t2,client;
//                add edge from=node3, to=node4, type=default;
//                """;

//        (await graphClient.ExecuteBatch(q3)).Action(x =>
//        {
//            x.BeOk();
//            x.Value.Items.Count.Be(4);

//            int index = 0;
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
//        });

//        var recoveredMapOption = await graphEngine.DataManager.BuildFromJournals(context);
//        GraphMap recoveredMap = recoveredMapOption.BeOk().Return();

//        recoveredMap.LastLogSequenceNumber.Be(graphEngine.DataManager.GetMap().LastLogSequenceNumber);
//        var compareMap = GraphCommandTools.CompareMap(recoveredMap, graphEngine.DataManager.GetMap());
//        compareMap.Count.Be(0);
//    }

//    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
//    {
//        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
//    }
//}
