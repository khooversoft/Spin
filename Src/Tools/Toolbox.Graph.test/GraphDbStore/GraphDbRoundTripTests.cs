using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphDbRoundTripTests
{
    private readonly ITestOutputHelper _logOutput;
    public GraphDbRoundTripTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphDbRoundTripTests");

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

        var context = host.Services.GetRequiredService<ILogger<GraphDbRoundTripTests>>().ToScopeContext();

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
    public async Task SimpleMapDbRoundTrip(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphDbRoundTripTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var fileSystem = host.Services.GetRequiredService<IFileSystem<GraphSerialization>>();
        const int count = 5;

        var seq = new Sequence<string>();

        seq += Enumerable.Range(0, count).Select(x => new NodeCommandBuilder().SetNodeKey($"node-{x}").Build());
        seq += Enumerable.Range(0, count - 1).Select(x => new EdgeCommandBuilder($"node-{x}", $"node-{x + 1}", "et").Build());

        var cmd = seq.Join(Environment.NewLine);
        var eResult = await graphClient.ExecuteBatch(cmd, context);
        eResult.IsOk().BeTrue(eResult.ToString());

        string path = fileSystem.PathBuilder(GraphConstants.GraphMap.Key);

        (await fileStore.File(path).Get(context)).Action(x =>
        {
            x.BeOk();

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count().Be(count);
            readRec.Edges.Count().Be(count - 1);

            var expectedMap = new GraphMap();
            Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
            Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

            GraphMap readMap = readRec.FromSerialization();
            var compareMap = GraphCommandTools.CompareMap(expectedMap, readMap, true);

            compareMap.Count.Be(0);
        });
    }
}
