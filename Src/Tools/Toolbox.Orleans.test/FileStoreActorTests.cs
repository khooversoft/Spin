using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Orleans.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;

//[Collection("ClusterFixture")]
public class FileStoreActorTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;

    public FileStoreActorTests(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    private record TestRecord(string Name, DateTime CreatedDate, int Count);

    [Fact]
    public async Task SimpleFile()
    {
        const string path = "contract/company1.com/contract1.json";

        IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
        var rec = new TestRecord("name1", DateTime.UtcNow, 10);

        DataETag writeDataEtag = rec.ToJson().ToDataETag();
        var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
        writeOption.IsOk().Should().BeTrue();

        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().ETag.Should().NotBeNullOrEmpty();

        IFileStoreSearchActor searchActor = _clusterFixture.Cluster.Client.GetFileStoreSearchActor();
        var searchList = await searchActor.Search("contract/company1.com/**.*", NullScopeContext.Instance);
        searchList.Count.Should().Be(1);
        searchList[0].Should().Be(path);

        IFileStore fileStore = ClusterFixture.FileStore;
        var search = await fileStore.Search("contract/company1.com/**/*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(1);
        search[0].Should().Be(path);

        var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
        deleteOption.IsOk().Should().BeTrue();

        search = await fileStore.Search("contract/company1.com/**/*", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Count.Should().Be(0);

        searchList = await searchActor.Search("contract/company1.com/**.*", NullScopeContext.Instance);
        searchList.Count.Should().Be(0);
    }

    [Fact]
    public async Task ScaleTest()
    {
        const int fileCount = 1000;
        const int maxParallelCount = 5;
        var addPaths = new ConcurrentQueue<string>();
        var getPaths = new ConcurrentQueue<string>();
        var deletePaths = new ConcurrentQueue<string>();

        var deleteBlock = new ActionBlock<string>(async path =>
        {
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
            var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
            deleteOption.IsOk().Should().BeTrue();

            deletePaths.Enqueue(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var getBlock = new ActionBlock<string>(async path =>
        {
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
            var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
            readOption.IsOk().Should().BeTrue();

            var readRecord = readOption.Return().ToObject<TestRecord>();
            readRecord.Should().NotBeNull();
            readRecord.Name.Should().NotBeNullOrEmpty();
            readRecord.Count.Should().Be(10);

            getPaths.Enqueue(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var addBlock = new ActionBlock<string>(async path =>
        {
            string name = path.Split('/').Last();
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);

            var rec = new TestRecord(name, DateTime.UtcNow, 10);
            DataETag writeDataEtag = rec.ToDataETag();
            var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
            writeOption.IsOk().Should().BeTrue();

            addPaths.Enqueue(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var files = await ClusterFixture.FileStore.Search("contract/scale_company/**/*", NullScopeContext.Instance);
        files.Count.Should().Be(0);

        Enumerable.Range(0, fileCount).ForEach(x => addBlock.Post($"contract/scale_company.com/contract{x}.json"));
        addBlock.Complete();
        await addBlock.Completion;
        addPaths.Count.Should().Be(fileCount);

        addPaths.ForEach(x => getBlock.Post(x));
        getBlock.Complete();
        await getBlock.Completion;
        getPaths.Count.Should().Be(fileCount);

        getPaths.ForEach(x => deleteBlock.Post(x));
        deleteBlock.Complete();
        await deleteBlock.Completion;
        deletePaths.Count.Should().Be(fileCount);

        files = await ClusterFixture.FileStore.Search("contract/scale_company/**/*", NullScopeContext.Instance);
        files.Count.Should().Be(0);
    }

    [Fact]
    public async Task StressTest()
    {
        const int fileCount = 1000;
        const int maxParallelCount = 5;
        var addPaths = new ConcurrentQueue<string>();
        var getPaths = new ConcurrentQueue<string>();
        var deletePaths = new ConcurrentQueue<string>();

        var deleteBlock = new ActionBlock<string>(async path =>
        {
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
            var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
            deleteOption.IsOk().Should().BeTrue();

            deletePaths.Enqueue(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var getBlock = new ActionBlock<string>(async path =>
        {
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);
            var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
            readOption.IsOk().Should().BeTrue();

            var readRecord = readOption.Return().ToObject<TestRecord>();
            readRecord.Should().NotBeNull();
            readRecord.Name.Should().NotBeNullOrEmpty();
            readRecord.Count.Should().Be(10);

            getPaths.Enqueue(path);
            await deleteBlock.SendAsync(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var addBlock = new ActionBlock<string>(async path =>
        {
            string name = path.Split('/').Last();
            IFileStoreActor fileStoreActor = _clusterFixture.Cluster.Client.GetFileStoreActor(path);

            var rec = new TestRecord(name, DateTime.UtcNow, 10);
            DataETag writeDataEtag = rec.ToDataETag();
            var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
            writeOption.IsOk().Should().BeTrue();

            addPaths.Enqueue(path);
            await getBlock.SendAsync(path);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

        var files = await ClusterFixture.FileStore.Search("contract/stress_company/**/*", NullScopeContext.Instance);
        files.Count.Should().Be(0);

        Enumerable.Range(0, fileCount).ForEach(x => addBlock.Post($"contract/stress_company.com/contract{x}.json"));
        addBlock.Complete();
        await addBlock.Completion;

        getBlock.Complete();
        await getBlock.Completion;

        deleteBlock.Complete();
        await deleteBlock.Completion;

        addPaths.Count.Should().Be(fileCount);
        getPaths.Count.Should().Be(fileCount);
        deletePaths.Count.Should().Be(fileCount);

        files = await ClusterFixture.FileStore.Search("contract/stress_company/**/*", NullScopeContext.Instance);
        files.Count.Should().Be(0);
    }
}
