using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;

public class FileStoreActorTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;

    public FileStoreActorTests(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    private record TestRecord(string name, DateTime CreatedDate, int count);

    [Fact]
    public async Task SimpleFile()
    {
        const string path = "contract/company1.com/contract1.json";

        IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
        var rec = new TestRecord("name1", DateTime.UtcNow, 10);

        DataETag writeDataEtag = rec.ToJson().ToDataETag();
        var writeOption = await fileStoreActor.Add(writeDataEtag, "trace");
        writeOption.IsOk().Should().BeTrue();

        var readOption = await fileStoreActor.Get("trace");
        readOption.IsOk().Should().BeTrue();
        readOption.Return().ETag.Should().NotBeNullOrEmpty();

        IFileStoreSearchActor searchActor = _clusterFixture.Cluster.Client.GetFileStoreSearchActor();
        var searchList = await searchActor.Search("contract/**.*", "trace");
        searchList.Count.Should().Be(1);
        searchList[0].Should().Be(path);

        IFileStore fileStore = ClusterFixture.FileStore;
        var search = await fileStore.Search("*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(1);
        search[0].Should().Be(path);

        var deleteOption = await fileStoreActor.Delete("trace");
        deleteOption.IsOk().Should().BeTrue();

        search = await fileStore.Search("*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(0);

        searchList = await searchActor.Search("contract/**.*", "trace");
        searchList.Count.Should().Be(0);
    }
}
