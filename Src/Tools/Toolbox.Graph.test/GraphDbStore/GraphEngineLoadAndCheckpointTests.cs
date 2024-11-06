using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Command;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphEngineLoadAndCheckpointTests
{
    [Fact]
    public async Task EmptyDbSave()
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        IGraphHost host = engine.ServiceProvider.GetRequiredService<IGraphHost>();
        IFileStore fileStore = engine.ServiceProvider.GetRequiredService<IFileStore>();
        var context = engine.GetScopeContext<GraphEngineLoadAndCheckpointTests>();

        await host.CheckpointMap(context);

        (await fileStore.Get(GraphConstants.MapDatabasePath, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Should().Be(0);
            readRec.Edges.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task SimpleMapDbRoundTrip()
    {
        const int count = 5;
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        IGraphHost host = engine.ServiceProvider.GetRequiredService<IGraphHost>();
        IFileStore fileStore = engine.ServiceProvider.GetRequiredService<IFileStore>();
        var context = engine.GetScopeContext<GraphEngineLoadAndCheckpointTests>();

        var seq = new Sequence<string>();

        seq += Enumerable.Range(0, count).Select(x => new NodeCommandBuilder().SetNodeKey($"node-{x}").Build());
        seq += Enumerable.Range(0, count - 1).Select(x => new EdgeCommandBuilder($"node-{x}", $"node-{x + 1}", "et").Build());

        var cmd = seq.Join(Environment.NewLine);
        var eResult = await engine.ExecuteBatch(cmd, context);
        eResult.IsOk().Should().BeTrue(eResult.ToString());

        (await fileStore.Get(GraphConstants.MapDatabasePath, context)).Action(x =>
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

    [Fact]
    public async Task LoadInitialDatabase()
    {
        const int count = 5;
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost();
        IGraphHost host = engine.ServiceProvider.GetRequiredService<IGraphHost>();
        IFileStore fileStore = engine.ServiceProvider.GetRequiredService<IFileStore>();
        var context = engine.GetScopeContext<GraphEngineLoadAndCheckpointTests>();

        var expectedMap = new GraphMap();
        Enumerable.Range(0, count).ForEach(x => expectedMap.Add(new GraphNode($"node-{x}")));
        Enumerable.Range(0, count - 1).ForEach(x => expectedMap.Add(new GraphEdge($"node-{x}", $"node-{x + 1}", "et")));

        var dbJson = expectedMap.ToSerialization();
        (await fileStore.Set(GraphConstants.MapDatabasePath, dbJson, context)).IsOk().Should().BeTrue();

        (await host.LoadMap(context)).IsOk().Should().BeTrue();

        var compareMap = GraphCommandTools.CompareMap(expectedMap, host.Map, true);
        compareMap.Count.Should().Be(0);
    }
}
