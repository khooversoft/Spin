using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Store;

public class InMemoryFileStoreTests
{
    [Fact]
    public async Task AddFileAndRemove()
    {
        const string dataText = "data";
        const string pathText = "path";
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);

        await AddFile(store, pathText, dataText);
        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });
        (await store.Search("**/*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });
        await DeleteFile(store, pathText, 0);
    }

    [Fact]
    public async Task AddFileFailOnDuplicate()
    {
        const string dataText = "data";
        const string pathText = "path";
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);

        await AddFile(store, pathText, dataText);

        DataETag data = new DataETag(dataText.ToBytes());
        Option<string> result = await store.Add(pathText, data, NullScopeContext.Default);
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
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);

        await AddFile(store, pathText, dataText);
        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });
        (await store.Search("**/*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });

        await SetFile(store, pathText, dataText2);
        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });
        (await store.Search("**/*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText);
        });

        await DeleteFile(store, pathText, 0);
    }

    [Fact]
    public async Task Add2FileAndRemove()
    {
        const string dataText1 = "data";
        const string pathText1 = "path";
        const string dataText2 = "data2";
        const string pathText2 = "path2";
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);

        await AddFile(store, pathText1, dataText1);
        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Should().Be(pathText1);
        });

        await AddFile(store, pathText2, dataText2);
        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(2);
            x.SequenceEqual([pathText1, pathText2]).Should().BeTrue();
        });

        await DeleteFile(store, pathText1, 1);
        await DeleteFile(store, pathText2, 0);
    }

    [Fact]
    public async Task AppendFile()
    {
        const string pathText1 = "path";
        const string dataText1 = "data";
        const string dataText2 = "data2";
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);

        DataETag data = new DataETag(dataText1.ToBytes());
        Option<string> result = await store.Add(pathText1, data, NullScopeContext.Default);
        result.IsOk().Should().BeTrue();
        (await store.Search("*", NullScopeContext.Default)).Count.Should().Be(1);

        DataETag data2 = new DataETag(dataText2.ToBytes());
        Option result2 = await store.Append(pathText1, data2, NullScopeContext.Default);
        result2.IsOk().Should().BeTrue();
        (await store.Search("*", NullScopeContext.Default)).Count.Should().Be(1);

        var readOption = await store.Get(pathText1, NullScopeContext.Default);
        readOption.IsOk().Should().BeTrue();

        var read = readOption.Return().Data;
        var expected = data.Data.Concat(data2.Data).ToArray();
        read.Length.Should().Be(expected.Length);

        Enumerable.SequenceEqual(expected, read).Should().BeTrue();
        ((InMemoryFileStore)store).Count.Should().Be(1);

        await DeleteFile(store, pathText1, 0);
    }

    [Fact]
    public async Task AddManyFile()
    {
        IFileStore store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var rnd = new Random();
        const int size = 1000;
        var queue = new ConcurrentQueue<(string fileId, DataETag data)>();

        var block = new ActionBlock<(int index, int size)>(async x =>
        {
            string fileId = $"File_{x.index}";
            string data = $"Data_{x.index}, " + new string('x', x.size);
            DataETag dataEtag = new DataETag(fileId.ToBytes());
            queue.Enqueue((fileId, dataEtag));

            var option = await store.Add(fileId, data, NullScopeContext.Default);
            option.IsOk().Should().BeTrue();

        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        Enumerable.Range(0, size).ForEach(x => block.Post((x, rnd.Next(0, 100))));
        block.Complete();
        await block.Completion;

        ((InMemoryFileStore)store).Count.Should().Be(size);
        queue.Count.Should().Be(size);

        (await store.Search("*", NullScopeContext.Default)).Action(x =>
        {
            x.Count.Should().Be(size);
            x.OrderBy(x => x).SequenceEqual(queue.Select(x => x.fileId).OrderBy(x => x)).Should().BeTrue();
        });

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
            var option = await store.Delete(x.fileId, NullScopeContext.Default);
            option.IsOk().Should().BeTrue();
            return option;
        });

        ((InMemoryFileStore)store).Count.Should().Be(0);
        (await store.Search("*", NullScopeContext.Default)).Count.Should().Be(0);
    }

    private Task AddFile(IFileStore store, string path, string dataText)
    {
        return AddOrSetFile(store, path, dataText, 1, (fileId, data) => store.Add(fileId, data, NullScopeContext.Default));
    }

    private Task SetFile(IFileStore store, string path, string dataText)
    {
        return AddOrSetFile(store, path, dataText, 0, (fileId, data) => store.Set(fileId, data, NullScopeContext.Default));
    }

    private async Task AddOrSetFile(IFileStore store, string path, string dataText, int increment, Func<string, DataETag, Task<Option<string>>> func)
    {
        DataETag data = new DataETag(dataText.ToBytes());

        int beginCount = ((InMemoryFileStore)store).Count;

        Option<string> result = await func(path, data);
        result.IsOk().Should().BeTrue();
        ((InMemoryFileStore)store).Count.Should().Be(beginCount + increment);
        (await store.Exist(path, NullScopeContext.Default)).Action(x => x.IsOk().Should().BeTrue());

        Option<DataETag> getResult = await store.Get(path, NullScopeContext.Default);
        result.IsOk().Should().BeTrue();
        ((InMemoryFileStore)store).Count.Should().Be(beginCount + increment);

        byte[] returnData = getResult.Return().Data.ToArray();
        Enumerable.SequenceEqual(data.Data, returnData).Should().BeTrue();
    }

    private async Task DeleteFile(IFileStore store, string path, int expectedCount)
    {
        (await store.Exist(path, NullScopeContext.Default)).Action(x => x.IsOk().Should().BeTrue());

        var deleteOption = await store.Delete(path, NullScopeContext.Default);
        deleteOption.IsOk().Should().BeTrue();

        ((InMemoryFileStore)store).Count.Should().Be(expectedCount);
        (await store.Exist(path, NullScopeContext.Default)).Action(x => x.IsOk().Should().BeFalse());

        var deleteOption2 = await store.Delete(path, NullScopeContext.Default);
        deleteOption2.IsError().Should().BeTrue();
    }
}
