//using System.Threading.Tasks.Dataflow;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Orleans.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans.test.InMemory;

//public class FileStoreActorTests : IClassFixture<InMemoryClusterFixture>
//{
//    private readonly InMemoryClusterFixture _inMemoryFixture;
//    public FileStoreActorTests(InMemoryClusterFixture inMemoryFixture) => _inMemoryFixture = inMemoryFixture.NotNull();

//    private record TestRecord(string Name, DateTime CreatedDate, int Count);

//    [Fact]
//    public async Task SimpleFile()
//    {
//        const string path = "contract/company1.com/contract1.json";

//        IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);
//        var rec = new TestRecord("name1", DateTime.UtcNow, 10);
//        await fileStoreActor.Delete(NullScopeContext.Instance);

//        DataETag writeDataEtag = rec.ToJson().ToDataETag();
//        var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
//        writeOption.IsOk().Should().BeTrue();

//        var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
//        readOption.IsOk().Should().BeTrue();
//        readOption.Return().ETag.Should().NotBeNullOrEmpty();

//        IFileStoreSearchActor searchActor = _inMemoryFixture.Cluster.Client.GetFileStoreSearchActor();
//        var searchList = await searchActor.Search("contract/company1.com/**.*", NullScopeContext.Instance);
//        searchList.Count.Should().Be(1);
//        searchList[0].Should().Be(path);

//        IFileStore fileStore = InMemoryClusterFixture.FileStore;
//        var search = await fileStore.Search("contract/company1.com/**/*", NullScopeContext.Instance);
//        search.Should().NotBeNull();
//        search.Count.Should().Be(1);
//        search[0].Should().Be(path);

//        var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
//        deleteOption.IsOk().Should().BeTrue();

//        search = await fileStore.Search("contract/company1.com/**/*", NullScopeContext.Instance);
//        search.Should().NotBeNull();
//        search.Count.Should().Be(0);

//        searchList = await searchActor.Search("contract/company1.com/**.*", NullScopeContext.Instance);
//        searchList.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task ScaleTest()
//    {
//        const int fileCount = 10000;
//        const int maxParallelCount = 1;
//        int addPathCount = 0;
//        int getPathCount = 0;
//        int deletePathCount = 0;
//        var dataflowBlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount };

//        var deleteBlock = new ActionBlock<string>(async path =>
//        {
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);
//            var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
//            deleteOption.IsOk().Should().BeTrue();

//            Interlocked.Increment(ref deletePathCount);
//        }, dataflowBlockOptions);

//        var getBlock = new ActionBlock<string>(async path =>
//        {
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);
//            var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
//            readOption.IsOk().Should().BeTrue();

//            var readRecord = readOption.Return().ToObject<TestRecord>();
//            readRecord.Should().NotBeNull();
//            readRecord.Name.Should().NotBeNullOrEmpty();
//            readRecord.Count.Should().Be(10);

//            Interlocked.Increment(ref getPathCount);
//            deleteBlock.Post(path);
//        }, dataflowBlockOptions);

//        var addBlock = new ActionBlock<string>(async path =>
//        {
//            string name = path.Split('/').Last();
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);

//            var rec = new TestRecord(name, DateTime.UtcNow, 10);
//            DataETag writeDataEtag = rec.ToDataETag();
//            var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
//            writeOption.IsOk().Should().BeTrue();

//            Interlocked.Increment(ref addPathCount);
//            getBlock.Post(path);
//        }, dataflowBlockOptions);

//        var files = await InMemoryClusterFixture.FileStore.Search("contract/scale_company/**/*", NullScopeContext.Instance);
//        files.Count.Should().Be(0);

//        Enumerable.Range(0, fileCount).ForEach(x => addBlock.Post($"contract/scale_company.com/contract{x}.json"));
//        addBlock.Complete();
//        await addBlock.Completion;
//        addPathCount.Should().Be(fileCount);

//        getBlock.Complete();
//        await getBlock.Completion;
//        getPathCount.Should().Be(fileCount);

//        deleteBlock.Complete();
//        await deleteBlock.Completion;
//        deletePathCount.Should().Be(fileCount);

//        files = await InMemoryClusterFixture.FileStore.Search("contract/scale_company/**/*", NullScopeContext.Instance);
//        files.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task StressTest()
//    {
//        const int fileCount = 1000;
//        const int maxParallelCount = 5;
//        int addPathCount = 0;
//        int getPathCount = 0;
//        int deletePathCount = 0;

//        var deleteBlock = new ActionBlock<string>(async path =>
//        {
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);
//            var deleteOption = await fileStoreActor.Delete(NullScopeContext.Instance);
//            deleteOption.IsOk().Should().BeTrue();

//            Interlocked.Increment(ref deletePathCount);
//        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

//        var getBlock = new ActionBlock<string>(async path =>
//        {
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);
//            var readOption = await fileStoreActor.Get(NullScopeContext.Instance);
//            readOption.IsOk().Should().BeTrue();

//            var readRecord = readOption.Return().ToObject<TestRecord>();
//            readRecord.Should().NotBeNull();
//            readRecord.Name.Should().NotBeNullOrEmpty();
//            readRecord.Count.Should().Be(10);

//            Interlocked.Increment(ref getPathCount);
//            await deleteBlock.SendAsync(path);
//        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

//        var addBlock = new ActionBlock<string>(async path =>
//        {
//            string name = path.Split('/').Last();
//            IFileStoreActor fileStoreActor = _inMemoryFixture.Cluster.Client.GetFileStoreActor(path);

//            var rec = new TestRecord(name, DateTime.UtcNow, 10);
//            DataETag writeDataEtag = rec.ToDataETag();
//            var writeOption = await fileStoreActor.Add(writeDataEtag, NullScopeContext.Instance);
//            writeOption.IsOk().Should().BeTrue();

//            Interlocked.Increment(ref addPathCount);
//            await getBlock.SendAsync(path);
//        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelCount });

//        var files = await InMemoryClusterFixture.FileStore.Search("contract/stress_company/**/*", NullScopeContext.Instance);
//        files.Count.Should().Be(0);

//        Enumerable.Range(0, fileCount).ForEach(x => addBlock.Post($"contract/stress_company.com/contract{x}.json"));
//        addBlock.Complete();
//        await addBlock.Completion;

//        getBlock.Complete();
//        await getBlock.Completion;

//        deleteBlock.Complete();
//        await deleteBlock.Completion;

//        addPathCount.Should().Be(fileCount);
//        getPathCount.Should().Be(fileCount);
//        deletePathCount.Should().Be(fileCount);

//        files = await InMemoryClusterFixture.FileStore.Search("contract/stress_company/**/*", NullScopeContext.Instance);
//        files.Count.Should().Be(0);
//    }
//}
