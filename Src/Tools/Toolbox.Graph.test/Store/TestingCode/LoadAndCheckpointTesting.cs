using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.test.Command;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Store.TestingCode;

internal static class LoadAndCheckpointTesting
{
    public static async Task EmptyDbSave(GraphHostService testClient, ScopeContext context)
    {
        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();

        await testClient.GraphEngine.CheckpointMap(context);

        (await fileStore.File(GraphConstants.MapDatabasePath).Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Should().Be(0);
            readRec.Edges.Count.Should().Be(0);
        });
    }

    public static async Task SimpleMapDbRoundTrip(GraphHostService testClient, ScopeContext context)
    {
        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        const int count = 5;

        var seq = new Sequence<string>();

        seq += Enumerable.Range(0, count).Select(x => new NodeCommandBuilder().SetNodeKey($"node-{x}").Build());
        seq += Enumerable.Range(0, count - 1).Select(x => new EdgeCommandBuilder($"node-{x}", $"node-{x + 1}", "et").Build());

        var cmd = seq.Join(Environment.NewLine);
        var eResult = await testClient.ExecuteBatch(cmd, context);
        eResult.IsOk().Should().BeTrue(eResult.ToString());

        (await fileStore.File(GraphConstants.MapDatabasePath).Get(context)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Should().Be(count);
            readRec.Edges.Count.Should().Be(count - 1);

            var expectedMap = new GraphMap();
            Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
            Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

            GraphMap readMap = readRec.FromSerialization();
            var compareMap = GraphCommandTools.CompareMap(expectedMap, readMap, true);

            compareMap.Count.Should().Be(0);
        });
    }

    public static async Task LoadInitialDatabase(GraphHostService testClient, ScopeContext context)
    {
        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        IGraphEngine host = testClient.Services.GetRequiredService<IGraphEngine>();
        const int count = 5;

        var expectedMap = new GraphMap();
        Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
        Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

        string dbJson = expectedMap.ToSerialization().ToJson();
        (await fileStore.File(GraphConstants.MapDatabasePath).Set(dbJson.ToDataETag(), context)).IsOk().Should().BeTrue();

        (await host.InitializeDatabase(context)).IsOk().Should().BeTrue();

        var compareMap = GraphCommandTools.CompareMap(expectedMap, testClient.Map, true);
        compareMap.Count.Should().Be(0);
    }
}
