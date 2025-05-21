using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Command;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphEngineLoadAndCheckpointTests
{
    private readonly ITestOutputHelper _outputHelper;

    public GraphEngineLoadAndCheckpointTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task EmptyDbSave()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));

        var context = graphTestClient.CreateScopeContext<GraphEngineLoadAndCheckpointTests>();
        IFileStore fileStore = graphTestClient.Services.GetRequiredService<IFileStore>();

        (await fileStore.File(GraphConstants.MapDatabasePath).Get(context)).Action(x =>
        {
            x.IsOk().BeTrue();

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Be(0);
            readRec.Edges.Count.Be(0);
        });
    }

    [Fact]
    public async Task SimpleMapDbRoundTrip()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<GraphEngineLoadAndCheckpointTests>();
        IFileStore fileStore = graphTestClient.Services.GetRequiredService<IFileStore>();
        const int count = 5;

        var seq = new Sequence<string>();

        seq += Enumerable.Range(0, count).Select(x => new NodeCommandBuilder().SetNodeKey($"node-{x}").Build());
        seq += Enumerable.Range(0, count - 1).Select(x => new EdgeCommandBuilder($"node-{x}", $"node-{x + 1}", "et").Build());

        var cmd = seq.Join(Environment.NewLine);
        var eResult = await graphTestClient.ExecuteBatch(cmd, context);
        eResult.IsOk().BeTrue(eResult.ToString());

        (await fileStore.File(GraphConstants.MapDatabasePath).Get(context)).Action(x =>
        {
            x.IsOk().BeTrue(x.ToString());

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Be(count);
            readRec.Edges.Count.Be(count - 1);

            var expectedMap = new GraphMap();
            Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
            Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

            GraphMap readMap = readRec.FromSerialization();
            var compareMap = GraphCommandTools.CompareMap(expectedMap, readMap, true);

            compareMap.Count.Be(0);
        });
    }

    //[Fact]
    //public async Task LoadInitialDatabase()
    //{
    //    using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(sharedMode: true, logOutput: x => _outputHelper.WriteLine(x), disableCache: true);
    //    var context = graphTestClient.CreateScopeContext<GraphEngineLoadAndCheckpointTests>();
    //    IFileStore fileStore = graphTestClient.Services.GetRequiredService<IFileStore>();
    //    IGraphEngine host = graphTestClient.Services.GetRequiredService<IGraphEngine>();
    //    const int count = 5;

    //    var expectedMap = new GraphMap();
    //    Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
    //    Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

    //    string dbJson = expectedMap.ToSerialization().ToJson();
    //    (await fileStore.File(GraphConstants.MapDatabasePath).Set(dbJson.ToDataETag(), context)).IsOk().BeTrue();

    //    (await host.InitializeDatabase(context)).IsOk().BeTrue();

    //    var compareMap = GraphCommandTools.CompareMap(expectedMap, graphTestClient.Map, true);
    //    compareMap.Count.Be(0);
    //}
}
