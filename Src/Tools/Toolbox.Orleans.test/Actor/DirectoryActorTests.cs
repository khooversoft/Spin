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

        var result = await actor.Execute("add node key=node2;", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Length.Should().Be(0);

        result = await actor.Execute("select (*);", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Length.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
        });

        var deleteResult = await actor.Execute("delete (key=node2);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();
    }
}