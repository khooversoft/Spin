//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure;
//using Toolbox.Extensions;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Transactions;

//public class GraphTrxEdgeTests
//{
//    private readonly ITestOutputHelper _logOutput;
//    public GraphTrxEdgeTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

//    private async Task<IHost> CreateService(bool useDatalake)
//    {
//        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphTrxEdgeTests");

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

//        var context = host.Services.GetRequiredService<ILogger<GraphTrxEdgeTests>>();

//        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
//        var list = await fileStore.Search("**/*");
//        await list.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

//        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        await graphEngine.DataManager.LoadDatabase(context);

//        return host;
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task AddEdgeFailure(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

//        string q = """
//            add node key=node1;
//            add node key=node2;
//            add node key=node3;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3"]).BeTrue();
//        graphEngine.DataManager.GetMap().Edges.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1:node2:default"]).BeTrue();

//        // should fail, adding node3 because it already exist
//        string q2 = """
//            add edge from=node2, to=node3, type=default;
//            add node key=node3;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });

//        graphEngine.DataManager.GetMap().Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3"]).BeTrue();
//        graphEngine.DataManager.GetMap().Edges.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1:node2:default"]).BeTrue();
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task UpdateEdgeFailure(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

//        string q = """
//            add node key=node1;
//            add node key=node2;
//            add node key=node3;
//            add edge from=node1, to=node2, type=default;
//            add edge from=node2, to=node3, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(3);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(2);

//        string q2 = """
//            set edge from=node2, to=node3, type=default set t1;
//            add node key=node2;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(3);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(2);
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task DeleteEdgeFailure(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

//        string q = """
//            add node key=node1;
//            add node key=node2;
//            add node key=node3;
//            set node key=node1 set t1;
//            add edge from=node1, to=node2, type=default;
//            add edge from=node2, to=node3, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(3);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(2);

//        string q2 = """
//            delete edge from=node2, to=node3, type=default;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });


//        graphEngine.DataManager.GetMap().Nodes.Count.Be(3);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(2);
//    }

//    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
//    {
//        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
//    }
//}
