using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
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

        IFileStoreSearchActor fileStoreSearchActor = _clusterFixture.Cluster.Client.GetFileStoreSearch();

        var files = await fileStoreSearchActor.Search($"{OrleansConstants.DirectoryActorKey}/**/*", "trace");
        files.Should().NotBeNull();
        files.Count.Should().Be(1);
        files[0].Should().Be("directory.json");
    }
}