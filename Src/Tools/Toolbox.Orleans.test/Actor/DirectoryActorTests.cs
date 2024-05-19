using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test.Actor;

public class DirectoryActorTests : IClassFixture<ActorClusterFixture>
{
    private readonly ActorClusterFixture _actorCluster;

    public DirectoryActorTests(ActorClusterFixture actorCluster) => _actorCluster = actorCluster.NotNull();

    [Fact]
    public async Task CreateSimpleNode()
    {
        var actor = _actorCluster.Cluster.Client.GetDirectoryActor();
        const string nodeKey = "node_simple";

        await actor.Execute($"delete (key={nodeKey});", NullScopeContext.Instance);

        var result = await actor.Execute($"add node key={nodeKey};", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Length.Should().Be(0);

        result = await actor.Execute($"select (key={nodeKey});", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Length.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be(nodeKey);
        });

        (await actor.Execute($"delete (key={nodeKey});", NullScopeContext.Instance)).IsOk().Should().BeTrue();

        // Verify can read directory
        IFileStoreActor fileStoreActor = _actorCluster.Cluster.Client.GetFileStoreActor("system/directory.json");
        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue(readOption.ToString());
    }
}