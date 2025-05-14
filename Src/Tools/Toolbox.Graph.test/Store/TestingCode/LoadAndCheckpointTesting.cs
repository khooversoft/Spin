using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Command;
using Toolbox.Store;
using Toolbox.Tools;
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
            x.IsOk().BeTrue();

            GraphSerialization readRec = x.Return().ToObject<GraphSerialization>();
            readRec.NotNull();
            readRec.Nodes.Count.Be(0);
            readRec.Edges.Count.Be(0);
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
        eResult.BeOk();

        (await fileStore.File(GraphConstants.MapDatabasePath).Get(context)).Action(x =>
        {
            x.BeOk();

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
}
