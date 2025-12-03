using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreTests
{
    private readonly ITestOutputHelper _output;

    public MemoryStoreTests(ITestOutputHelper output) => _output = output.NotNull();

    private IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddSingleton<MemoryStore>();
            })
            .Build();

        return host;
    }

    [Fact]
    public void EmptyTest()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var list = ms.Search("**/*");
        list.Count.Be(0);
    }

    [Fact]
    public void GivenValidPath_WhenAdd_ShouldSucceed()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/file.txt";
        var data = new DataETag("test data".ToBytes());

        var result = ms.Add(path, data, context);
        result.BeOk();
        result.Return().NotEmpty();

        ms.Exist(path).BeTrue();
    }

    [Fact]
    public void GivenDuplicatePath_WhenAdd_ShouldReturnConflict()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/duplicate.txt";
        var data = new DataETag("data".ToBytes());

        ms.Add(path, data, context).BeOk();
        var result = ms.Add(path, data, context);
        result.IsConflict().BeTrue();
    }

    [Fact]
    public void GivenInvalidPath_WhenAdd_ShouldReturnBadRequest()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string invalidPath = "";
        var data = new DataETag("data".ToBytes());

        var result = ms.Add(invalidPath, data, context);
        result.StatusCode.Be(StatusCode.BadRequest);
    }

    [Fact]
    public void GivenExistingPath_WhenSet_ShouldUpdate()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/update.txt";
        var data1 = new DataETag("original".ToBytes());
        var data2 = new DataETag("updated".ToBytes());

        ms.Add(path, data1, context).BeOk();
        var result = ms.Set(path, data2, null, context);
        result.BeOk();

        var retrieved = ms.Get(path);
        retrieved.BeOk();
        retrieved.Return().Data.SequenceEqual(data2.Data).BeTrue();
    }

    [Fact]
    public void GivenNewPath_WhenSet_ShouldCreate()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/new.txt";
        var data = new DataETag("new data".ToBytes());

        var result = ms.Set(path, data, null, context);
        result.BeOk();
        ms.Exist(path).BeTrue();
    }

    [Fact]
    public void GivenExistingPath_WhenGet_ShouldReturnData()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/get.txt";
        var expected = new DataETag("get data".ToBytes());

        ms.Add(path, expected, context);
        var result = ms.Get(path);

        result.BeOk();
        result.Return().Data.SequenceEqual(expected.Data).BeTrue();
    }

    [Fact]
    public void GivenNonExistingPath_WhenGet_ShouldReturnNotFound()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();

        var result = ms.Get("nonexistent/path.txt");
        result.IsNotFound().BeTrue();
    }

    [Fact]
    public void GivenExistingPath_WhenDelete_ShouldRemove()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/delete.txt";
        var data = new DataETag("data".ToBytes());

        ms.Add(path, data, context);
        var result = ms.Delete(path, null, context);

        result.BeOk();
        ms.Exist(path).BeFalse();
    }

    [Fact]
    public void GivenNonExistingPath_WhenDelete_ShouldReturnNotFound()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        var result = ms.Delete("nonexistent.txt", null, context);
        result.IsNotFound().BeTrue();
    }

    [Fact]
    public void GivenNewFile_WhenAppend_ShouldCreate()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/append.txt";
        var data = new DataETag("append data".ToBytes());

        var result = ms.Append(path, data, null, context);
        result.BeOk();
        ms.Exist(path).BeTrue();
    }

    [Fact]
    public void GivenExistingFile_WhenAppend_ShouldConcatenate()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/append2.txt";
        var data1 = new DataETag("part1".ToBytes());
        var data2 = new DataETag("part2".ToBytes());

        ms.Add(path, data1, context);
        ms.Append(path, data2, null, context);

        var result = ms.Get(path);
        var expected = data1.Data.Concat(data2.Data).ToArray();
        result.Return().Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void GivenMultipleFiles_WhenSearchWithWildcard_ShouldReturnAll()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        ms.Add("file1.txt", new DataETag("data1".ToBytes()), context);
        ms.Add("file2.txt", new DataETag("data2".ToBytes()), context);
        ms.Add("dir/file3.txt", new DataETag("data3".ToBytes()), context);

        var result = ms.Search("*");
        result.Count.Be(3);
    }

    [Fact]
    public void GivenMultipleFiles_WhenSearchWithPattern_ShouldReturnMatching()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        ms.Add("dir/file1.txt", new DataETag("data1".ToBytes()), context);
        ms.Add("dir/file2.txt", new DataETag("data2".ToBytes()), context);
        ms.Add("other/file3.txt", new DataETag("data3".ToBytes()), context);

        var result = ms.Search("dir/*");
        result.Count.Be(2);
        result.All(x => x.Path.StartsWith("dir/")).BeTrue();
    }

    [Fact]
    public void GivenFiles_WhenDeleteFolder_ShouldRemoveMatching()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        ms.Add("folder/file1.txt", new DataETag("data1".ToBytes()), context);
        ms.Add("folder/file2.txt", new DataETag("data2".ToBytes()), context);
        ms.Add("other/file3.txt", new DataETag("data3".ToBytes()), context);

        ms.DeleteFolder("folder/*", context);

        ms.Search("*").Count.Be(1);
        ms.Exist("other/file3.txt").BeTrue();
    }

    [Fact]
    public void GivenPathWithLeadingSlash_WhenAdded_ShouldNormalize()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "/test/file.txt";
        var data = new DataETag("data".ToBytes());

        ms.Add(path, data, context).BeOk();
        ms.Exist("test/file.txt").BeTrue();
    }

    [Fact]
    public async Task GivenConcurrentAdds_WhenDifferentPaths_ShouldSucceed()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const int concurrency = 10;
        var tasks = Enumerable.Range(0, concurrency)
            .Select(i => Task.Run(() =>
            {
                var path = $"concurrent/file{i}.txt";
                var data = new DataETag($"data{i}".ToBytes());
                return ms.Add(path, data, context);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        results.All(r => r.IsOk()).BeTrue();
        ms.Search("concurrent/*").Count.Be(concurrency);
    }

    [Fact]
    public async Task GivenConcurrentAdds_WhenSamePath_OnlyOneShouldSucceed()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "concurrent/same.txt";
        const int concurrency = 10;

        var tasks = Enumerable.Range(0, concurrency)
            .Select(i => Task.Run(() =>
            {
                var data = new DataETag($"data{i}".ToBytes());
                return ms.Add(path, data, context);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        results.Count(r => r.IsOk()).Be(1);
        results.Count(r => r.IsConflict()).Be(concurrency - 1);
    }

    [Fact]
    public async Task GivenConcurrentSets_WhenSamePath_ShouldHandleCorrectly()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "concurrent/set.txt";
        const int concurrency = 100;

        var tasks = Enumerable.Range(0, concurrency)
            .Select(i => Task.Run(() =>
            {
                var data = new DataETag($"data{i}".ToBytes());
                return ms.Set(path, data, null, context);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        results.All(r => r.IsOk()).BeTrue();

        // Final value should be one of the concurrent updates
        var final = ms.Get(path);
        final.BeOk();
    }

    [Fact]
    public async Task GivenConcurrentGetSet_WhenRunning_ShouldMaintainConsistency()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "concurrent/getset.txt";
        ms.Add(path, new DataETag("initial".ToBytes()), context);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var setTask = Task.Run(async () =>
        {
            int count = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                var data = new DataETag($"data{count++}".ToBytes());
                ms.Set(path, data, null, context);
                await Task.Delay(1);
            }
            return count;
        });

        var getTask = Task.Run(async () =>
        {
            int count = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                var result = ms.Get(path);
                result.BeOk();
                count++;
                await Task.Delay(1);
            }
            return count;
        });

        var results = await Task.WhenAll(setTask, getTask);
        _output.WriteLine($"Sets: {results[0]}, Gets: {results[1]}");
    }

    [Fact]
    public async Task StressTest_LargeDataset_ShouldPerform()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const int size = 1000;
        var start = DateTime.Now;

        // Add phase
        var addTasks = Enumerable.Range(0, size)
            .Select(i => Task.Run(() =>
            {
                var path = $"stress/file{i}.txt";
                var data = new DataETag($"data{i}".ToBytes());
                return ms.Add(path, data, context);
            }))
            .ToArray();

        await Task.WhenAll(addTasks);
        var addTime = DateTime.Now - start;

        ms.Search("stress/*").Count.Be(size);

        // Get phase
        start = DateTime.Now;
        var getTasks = Enumerable.Range(0, size)
            .Select(i => Task.Run(() => ms.Get($"stress/file{i}.txt")))
            .ToArray();

        var getResults = await Task.WhenAll(getTasks);
        var getTime = DateTime.Now - start;
        getResults.All(r => r.IsOk()).BeTrue();

        // Performance metrics
        double addTps = size / addTime.TotalSeconds;
        double getTps = size / getTime.TotalSeconds;

        _output.WriteLine($"Add: {size} items in {addTime.TotalMilliseconds}ms, TPS: {addTps:F2}");
        _output.WriteLine($"Get: {size} items in {getTime.TotalMilliseconds}ms, TPS: {getTps:F2}");

        // Cleanup
        ms.DeleteFolder("stress/*", context);
        ms.Search("*").Count.Be(0);
    }

    [Fact]
    public async Task StressTest_ContinuousOperations_ShouldMaintainStability()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int addCount = 0, setCount = 0, getCount = 0, deleteCount = 0;

        var tasks = new[]
        {
            Task.Run(async () => {
                while (!cts.Token.IsCancellationRequested)
                {
                    var path = $"stress/add{Interlocked.Increment(ref addCount)}.txt";
                    ms.Add(path, new DataETag($"data{addCount}".ToBytes()), context);
                    await Task.Delay(1);
                }
            }),
            Task.Run(async () => {
                while (!cts.Token.IsCancellationRequested)
                {
                    var path = $"stress/set{Interlocked.Increment(ref setCount) % 100}.txt";
                    ms.Set(path, new DataETag($"data{setCount}".ToBytes()), null, context);
                    await Task.Delay(1);
                }
            }),
            Task.Run(async () => {
                while (!cts.Token.IsCancellationRequested)
                {
                    Interlocked.Increment(ref getCount);
                    ms.Get($"stress/set{getCount % 100}.txt");
                    await Task.Delay(1);
                }
            })
        };

        await Task.WhenAll(tasks);

        var totalOps = addCount + setCount + getCount + deleteCount;
        var tps = totalOps / 5.0; // 5 second test

        _output.WriteLine($"Total ops: {totalOps}, TPS: {tps:F2}");
        _output.WriteLine($"Add: {addCount}, Set: {setCount}, Get: {getCount}, Delete: {deleteCount}");

        // Cleanup
        ms.DeleteFolder("stress/*", context);
    }

    [Fact]
    public void GivenData_WhenAdded_ShouldGenerateETag()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/etag.txt";
        var data = new DataETag("test".ToBytes());

        var result = ms.Add(path, data, context);
        result.BeOk();
        result.Return().NotEmpty();

        var detail = ms.GetDetail(path);
        detail.BeOk();
        detail.Return().ETag.NotEmpty();
    }

    [Fact]
    public void GivenUpdate_WhenSet_ShouldChangeETag()
    {
        var host = BuildHost();
        var ms = host.Services.GetRequiredService<MemoryStore>();
        var context = host.Services.CreateContext<MemoryStoreTests>();

        const string path = "test/etag2.txt";
        var data1 = new DataETag("original".ToBytes());
        var data2 = new DataETag("updated".ToBytes());

        ms.Add(path, data1, context);
        var etag1 = ms.GetDetail(path).Return().ETag;

        ms.Set(path, data2, null, context);
        var etag2 = ms.GetDetail(path).Return().ETag;

        etag1.NotBe(etag2);
    }
}
