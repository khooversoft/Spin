using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Store;

public class InMemoryFileStoreTests
{
    [Fact]
    public async Task AddFileAndRemove()
    {
        const string dataText = "data";
        const string pathText = "path";
        IFileStore store = new InMemoryFileStore();

        await AddFile(store, pathText, dataText);
        await DeleteFile(store, pathText, 0);
    }

    [Fact]
    public async Task AddFileFailOnDuplicate()
    {
        const string dataText = "data";
        const string pathText = "path";
        IFileStore store = new InMemoryFileStore();

        await AddFile(store, pathText, dataText);

        DataETag data = new DataETag(dataText.ToBytes());
        Option result = await store.Add(pathText, data, NullScopeContext.Instance);
        result.IsOk().Should().BeFalse();
        result.IsConflict().Should().BeTrue();

        await DeleteFile(store, pathText, 0);
    }

    [Fact]
    public async Task AddFileUpdateWithSet()
    {
        const string dataText = "data";
        const string dataText2 = "data2";
        const string pathText = "path";
        IFileStore store = new InMemoryFileStore();

        await AddFile(store, pathText, dataText);
        await SetFile(store, pathText, dataText2);

        await DeleteFile(store, pathText, 0);
    }

    [Fact]
    public async Task Add2FileAndRemove()
    {
        const string dataText1 = "data";
        const string pathText1 = "path";
        const string dataText2 = "data2";
        const string pathText2 = "path2";
        IFileStore store = new InMemoryFileStore();

        await AddFile(store, pathText1, dataText1);
        await AddFile(store, pathText2, dataText2);
        await DeleteFile(store, pathText1, 1);
        await DeleteFile(store, pathText2, 0);
    }

    [Fact]
    public async Task AddManyFile()
    {
        IFileStore store = new InMemoryFileStore();
        var rnd = new Random();
        const int size = 1000;
        var queue = new ConcurrentQueue<(string fileId, DataETag data)>();

        var block = new ActionBlock<(int index, int size)>(async x =>
        {
            string fileId = $"File_{x.index}";
            string data = $"Data_{x.index}, " + new string('x', x.size);
            DataETag dataEtag = new DataETag(fileId.ToBytes());
            queue.Enqueue((fileId, dataEtag));

            var option = await store.Add(fileId, data, NullScopeContext.Instance);
            option.IsOk().Should().BeTrue();

        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        Enumerable.Range(0, size).ForEach(x => block.Post((x, rnd.Next(0, 100))));
        block.Complete();
        await block.Completion;

        ((InMemoryFileStore)store).Count.Should().Be(size);
        queue.Count.Should().Be(size);

        KeyValuePair<string, DataETag>[] storeList = ((InMemoryFileStore)store).OrderBy(x => x.Key).ToArray();
        (string fileId, DataETag data)[] queueList = queue.OrderBy(x => x.fileId).ToArray();
        storeList.Length.Should().Be(queueList.Length);

        var zip = storeList
            .Zip(queueList, (o, i) => (store: o, queue: i))
            .Select(x => (src: x, pass: x.store.Key == x.queue.fileId && x.store.Value == x.queue.data))
            .All(x => x.pass);

        new Random().Shuffle(queueList);
        var p = await ActionParallel.RunAsync(queueList, async x =>
        {
            var option = await store.Delete(x.fileId, NullScopeContext.Instance);
            option.IsOk().Should().BeTrue();
            return option;
        });

        ((InMemoryFileStore)store).Count.Should().Be(0);
    }

    private Task AddFile(IFileStore store, string path, string dataText)
    {
        return AddOrSetFile(store, path, dataText, 1, (fileId, data) => store.Add(fileId, data, NullScopeContext.Instance));
    }

    private Task SetFile(IFileStore store, string path, string dataText)
    {
        return AddOrSetFile(store, path, dataText, 0, (fileId, data) => store.Set(fileId, data, NullScopeContext.Instance));
    }

    private async Task AddOrSetFile(IFileStore store, string path, string dataText, int increment, Func<string, DataETag, Task<Option>> func)
    {
        DataETag data = new DataETag(dataText.ToBytes());

        int beginCount = ((InMemoryFileStore)store).Count;

        Option result = await func(path, data);
        result.IsOk().Should().BeTrue();
        ((InMemoryFileStore)store).Count.Should().Be(beginCount + increment);
        (await store.Exist(path, NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue());

        Option<DataETag> getResult = await store.Get(path, NullScopeContext.Instance);
        result.IsOk().Should().BeTrue();
        ((InMemoryFileStore)store).Count.Should().Be(beginCount + increment);

        byte[] returnData = getResult.Return().Data.ToArray();
        Enumerable.SequenceEqual(data.Data, returnData).Should().BeTrue();
    }

    private async Task DeleteFile(IFileStore store, string path, int expectedCount)
    {
        (await store.Exist(path, NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue());

        var deleteOption = await store.Delete(path, NullScopeContext.Instance);
        deleteOption.IsOk().Should().BeTrue();

        ((InMemoryFileStore)store).Count.Should().Be(expectedCount);
        (await store.Exist(path, NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeFalse());

        var deleteOption2 = await store.Delete(path, NullScopeContext.Instance);
        deleteOption2.IsError().Should().BeTrue();
    }
}
