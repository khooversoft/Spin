using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;

//[Collection("ClusterFixture")]
public class DirectoryActorTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;

    public DirectoryActorTests(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    [Fact]
    public async Task CreateSimpleNode()
    {
        var actor = _clusterFixture.Cluster.Client.GetDirectoryActor();

        var result = await actor.ExecuteScalar("add node key=node1;", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(0);

        result = await actor.ExecuteScalar("select (*);", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
        });

        IFileStoreSearchActor fileStoreSearchActor = _clusterFixture.Cluster.Client.GetFileStoreSearchActor();

        var files = await fileStoreSearchActor.Search($"system/**/*", NullScopeContext.Instance);
        files.Should().NotBeNull();
        files.Count.Should().Be(1);
        files[0].Should().Be(OrleansConstants.DirectoryFilePath);

        IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(OrleansConstants.DirectoryFilePath);
        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();


        DataETag read = readOption.Return();
        var directoryObj = read.ToObject<GraphSerialization>();
        directoryObj.Should().NotBeNull();
        directoryObj.Nodes.Count.Should().Be(1);
        directoryObj.Edges.Count.Should().Be(0);

        IFileStore fileStore = ClusterFixture.FileStore;
        var search = await fileStore.Search("system/**/*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(1);
        search[0].Should().Be("system/directory.json");
    }

    [Fact]
    public async Task CreateSimpleNodeWithEntity()
    {
        var actor = _clusterFixture.Cluster.ServiceProvider.GetRequiredService<IGraphEntity>();

        var result = await actor.Command.ExecuteScalar("add node key=node1;", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(0);

        result = await actor.Command.ExecuteScalar("select (*);", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
        });

        //var files = await actor.Store.Search($"system/**/*", NullScopeContext.Instance);
        //files.Should().NotBeNull();
        //files.Count.Should().Be(1);
        //files[0].Should().Be(OrleansConstants.DirectoryFilePath);

        IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(OrleansConstants.DirectoryFilePath);
        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();

        DataETag read = readOption.Return();
        var directoryObj = read.ToObject<GraphSerialization>();
        directoryObj.Should().NotBeNull();
        directoryObj.Nodes.Count.Should().Be(1);
        directoryObj.Edges.Count.Should().Be(0);

        IFileStore fileStore = ClusterFixture.FileStore;
        var search = await fileStore.Search("system/**/*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(1);
        search[0].Should().Be("system/directory.json");
    }
}