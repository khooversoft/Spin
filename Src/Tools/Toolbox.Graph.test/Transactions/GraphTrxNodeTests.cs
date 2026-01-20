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

//public class GraphTrxNodeTests
//{
//    private readonly ITestOutputHelper _logOutput;
//    public GraphTrxNodeTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

//    private async Task<IHost> CreateService(bool useDatalake)
//    {
//        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphTrxNodeTests");

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
//    public async Task AddNodeFailure(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

//        string q = """
//            add node key=node1;
//            add node key=node2;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(1);

//        string q2 = """
//            add node key=node3;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.IsError().BeTrue();
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task UpdateNodeFailure(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>();
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

//        string q = """
//            add node key=node1;
//            add node key=node2;
//            set node key=node1 set t1;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(1);

//        string q2 = """
//            set node key=node2 set t2;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task DeleteNodeFailure(bool useDataLake)
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
//            """;

//        (await graphClient.ExecuteBatch(q)).IsOk().BeTrue();

//        graphEngine.DataManager.GetMap().Nodes.Count.Be(3);
//        graphEngine.DataManager.GetMap().Edges.Count.Be(1);

//        string q2 = """
//            delete node key=node3;
//            add edge from=node1, to=node2, type=default;
//            """;

//        (await graphClient.ExecuteBatch(q2)).Action(x =>
//        {
//            x.Value.Items.Count.Be(2);
//            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
//            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
//        });
//    }

//    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
//    {
//        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
//    }
//}
