using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;

public class DirectoryActorTest : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;

    public DirectoryActorTest(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    [Fact]
    public async Task CreateSimpleNode()
    {
        var actor = _clusterFixture.Cluster.Client.GetDirectory();

        var result = await actor.ExecuteScalar("add node key=node1;", "trace");
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(0);

        result = await actor.ExecuteScalar("select (*);", "trace");
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
        });

    }
}