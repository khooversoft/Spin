using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test.InMemory;

public class DirectoryActorTests : IClassFixture<InMemoryClusterFixture>
{
    private readonly InMemoryClusterFixture _inMemoryFixture;

    public DirectoryActorTests(InMemoryClusterFixture clusterFixture) => _inMemoryFixture = clusterFixture.NotNull();

    [Fact]
    public async Task VerifyDirectoryDbFile()
    {
        var actor = _inMemoryFixture.Cluster.Client.GetDirectoryActor();

        var result = await actor.Execute("add node key=node1;", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(0);

        IFileStoreSearchActor fileStoreSearchActor = _inMemoryFixture.Cluster.Client.GetFileStoreSearchActor();
        var files = await fileStoreSearchActor.Search($"system/**/*", NullScopeContext.Instance);
        files.Should().NotBeNull();
        files.Length.Should().Be(1);
        files[0].Should().Be(OrleansConstants.DirectoryFilePath);

        var deleteResult = await actor.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();

        IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(OrleansConstants.DirectoryFilePath);
        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();

        DataETag read = readOption.Return();
        var directoryObj = read.ToObject<GraphSerialization>();
        directoryObj.Should().NotBeNull();

        IFileStore fileStore = InMemoryClusterFixture.FileStore;
        var search = await fileStore.Search("system/**/*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Length.Should().Be(1);
        search[0].Should().Be("system/directory.json");
    }

    [Fact]
    public async Task CreateSimpleNode()
    {
        var actor = _inMemoryFixture.Cluster.Client.GetDirectoryActor();

        var result = await actor.Execute("add node key=node1;", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(0);

        result = await actor.Execute("select (*);", NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue();
        result.Return().Items.Count.Should().Be(1);
        result.Return().Items.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
        });

        var deleteResult = await actor.Execute("delete (key=node1);", NullScopeContext.Instance);
        deleteResult.IsOk().Should().BeTrue();
    }
}