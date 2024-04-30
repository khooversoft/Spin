using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;


public class DirectoryStoreActorTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;
    public DirectoryStoreActorTests(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    private record TestRecord(string Name, DateTime CreatedDate, int Count);

    [Fact]
    public async Task SimpleFileWithEntity()
    {
        const string nodeKey = "contract/company2.com/contract1";
        const string searchQuery = "nodes/entity/contract/company2.com/**/*";

        var actor = _clusterFixture.Cluster.ServiceProvider.GetRequiredService<IGraphEntity>();
        var rec = new TestRecord("name1", DateTime.UtcNow, 10);

        var addNodeOption = await actor.Command.ExecuteScalar($"add node key={nodeKey};", NullScopeContext.Instance);
        addNodeOption.IsOk().Should().BeTrue();

        DataETag writeDataEtag = rec.ToJson().ToDataETag();
        var writeOption = await actor.Store.Add(nodeKey, "entity", writeDataEtag, NullScopeContext.Instance);
        writeOption.IsOk().Should().BeTrue(writeOption.ToString());

        var readOption = await actor.Store.Get(nodeKey, "entity", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue(readOption.ToString());
        readOption.Return().ETag.Should().NotBeNullOrEmpty();

        IFileStoreSearchActor searchActor = _clusterFixture.Cluster.Client.GetFileStoreSearchActor();
        var searchList = await searchActor.Search(searchQuery, NullScopeContext.Instance);
        searchList.Count.Should().Be(1);
        searchList[0].Should().Be(nodeKey);

        IFileStore fileStore = ClusterFixture.FileStore;
        var search = await fileStore.Search(searchQuery, NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(1);
        search[0].Should().Be(nodeKey);

        var deleteOption = await actor.Store.Delete(nodeKey, "entity", NullScopeContext.Instance);
        deleteOption.IsOk().Should().BeTrue();

        search = await fileStore.Search(searchQuery, NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(0);

        searchList = await searchActor.Search(searchQuery, NullScopeContext.Instance);
        searchList.Count.Should().Be(0);
    }
}

