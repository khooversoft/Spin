using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.KeyStore;

public class DataSpaceCacheTests
{
    private ITestOutputHelper _outputHelper;

    public DataSpaceCacheTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();

    private async Task<IHost> BuildService(bool useHash, [CallerMemberName] string function = "")
    {
        string basePath = nameof(DataSpaceCacheTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);
                services.AddMemoryCache();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = "dataFiles",
                        SpaceFormat = useHash ? SpaceFormat.Hash : SpaceFormat.Key,
                        UseCache = true
                    });

                    cnfg.Add<KeyStoreProvider>("fileStore");
                });

                services.AddSingleton<TelemetryAggregator>();
                services.AddTelemetry(config =>
                {
                    config.AddCollector<TelemetryAggregator>();
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0);

        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SimpleWriteAndRead(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        listener.GetAllEvents().Count.Be(0);

        var ls = keyStore as KeySpace ?? throw new ArgumentException();
        var fileSystem = ls.KeyPathStrategy;

        string path = "test/data.txt";
        string realPath = fileSystem.RemoveBasePath(fileSystem.BuildPath(path));

        var content = "Hello, World!".ToBytes();
        var setResult = await keyStore.Set(path, new DataETag(content));
        setResult.BeOk();
        listener.GetAllEvents().Count.Be(1); // Set
        listener.GetCounterValue("keyspace.set").Be(1);

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        var readData = readOption.Return().Data;
        content.SequenceEqual(readData).BeTrue();
        listener.GetAllEvents().Count.Be(2); // Set, Cache hit
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        var deleteOption = await keyStore.Delete(path);
        deleteOption.BeOk();
        listener.GetAllEvents().Count.Be(3); // Set, Cache hit, Delete
        listener.GetCounterValue("keyspace.delete").Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CacheMiss_WhenReadingNonExistentData(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        string path = "test/nonexistent.txt";

        var readOption = await keyStore.Get(path);
        readOption.IsError().BeTrue();
        readOption.StatusCode.Be(StatusCode.NotFound);

        listener.GetAllEvents().Count.Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleReads_ShouldIncrementCacheHitCounter(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        string path = "test/multiread.txt";
        var content = "Cache test content".ToBytes();

        var setResult = await keyStore.Set(path, new DataETag(content));
        setResult.BeOk();
        listener.GetCounterValue("keyspace.set").Be(1);

        // First read - should be cache hit from Set operation
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        content.SequenceEqual(readOption1.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        // Second read - should be cache hit
        var readOption2 = await keyStore.Get(path);
        readOption2.BeOk();
        content.SequenceEqual(readOption2.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(2);

        // Third read - should be cache hit
        var readOption3 = await keyStore.Get(path);
        readOption3.BeOk();
        content.SequenceEqual(readOption3.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(3);

        listener.GetCounterValue("keyspace.cache.miss").Be(-1);
        listener.GetAllEvents().Count.Be(4);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Add_ShouldCacheDataAndIncrementCounter(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        listener.GetAllEvents().Count.Be(0);

        string path = "test/adddata.txt";
        var content = "Added content".ToBytes();

        var addResult = await keyStore.Add(path, new DataETag(content));
        addResult.BeOk();
        listener.GetCounterValue("keyspace.add").Be(1);
        listener.GetAllEvents().Count.Be(1);

        // Read should be cache hit
        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        content.SequenceEqual(readOption.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);
        listener.GetCounterValue("keyspace.cache.miss").Be(-1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Append_ShouldInvalidateCacheAndIncrementCounter(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        string path = "test/appenddata.txt";
        var content1 = "Initial content".ToBytes();
        var content2 = "\nAppended content".ToBytes();

        var setResult = await keyStore.Set(path, new DataETag(content1));
        setResult.BeOk();
        listener.GetCounterValue("keyspace.set").Be(1);

        // First read should be cache hit
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        listener.GetCounterValue("keyspace.get").Be(-1);
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        // Append should invalidate cache
        var appendResult = await keyStore.Append(path, new DataETag(content2));
        appendResult.BeOk();
        listener.GetCounterValue("keyspace.append").Be(1);

        // Read after append should be cache miss (cache was invalidated)
        var readOption2 = await keyStore.Get(path);
        readOption2.BeOk();
        listener.GetCounterValue("keyspace.get").Be(1);
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        // Verify combined content
        var expectedContent = content1.Concat(content2).ToArray();
        expectedContent.SequenceEqual(readOption2.Return().Data).BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Delete_ShouldInvalidateCacheAndIncrementCounter(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        string path = "test/deletedata.txt";
        var content = "Content to delete".ToBytes();

        var setResult = await keyStore.Set(path, new DataETag(content));
        setResult.BeOk();
        listener.GetCounterValue("keyspace.set").Be(1);

        // First read should be cache hit
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);
        listener.GetCounterValue("keyspace.get").Be(-1);

        // Delete should invalidate cache
        var deleteResult = await keyStore.Delete(path);
        deleteResult.BeOk();
        listener.GetCounterValue("keyspace.delete").Be(1);

        // Read after delete should be cache miss and return NotFound
        var readOption2 = await keyStore.Get(path);
        readOption2.BeError();
        readOption2.StatusCode.Be(StatusCode.NotFound);

        listener.GetAllEvents().Count.Be(3);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateExistingKey_ShouldUpdateCacheAndIncrementCounter(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        string path = "test/updatedata.txt";
        var content1 = "Original content".ToBytes();
        var content2 = "Updated content".ToBytes();

        // Initial set
        var setResult1 = await keyStore.Set(path, new DataETag(content1));
        setResult1.BeOk();
        listener.GetCounterValue("keyspace.set").Be(1);

        // First read should be cache hit
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        content1.SequenceEqual(readOption1.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        // Update content
        var setResult2 = await keyStore.Set(path, new DataETag(content2));
        setResult2.BeOk();
        listener.GetCounterValue("keyspace.set").Be(2);

        // Read should return updated content from cache
        var readOption2 = await keyStore.Get(path);
        readOption2.BeOk();
        content2.SequenceEqual(readOption2.Return().Data).BeTrue();
        listener.GetCounterValue("keyspace.cache.hit").Be(2);
        listener.GetCounterValue("keyspace.cache.miss").Be(-1);

        listener.GetAllEvents().Count.Be(4);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CacheMissFollowedByCacheHit(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        string path = "test/cachemisstohit.txt";
        var content = "Test content".ToBytes();

        // Set data
        await keyStore.Set(path, new DataETag(content));

        // Delete to clear cache
        await keyStore.Delete(path);

        // Set again
        await keyStore.Set(path, new DataETag(content));
        listener.GetCounterValue("keyspace.set").Be(2);

        // Read should be cache hit
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);

        // Delete again
        await keyStore.Delete(path);

        // Read should be cache miss
        var readOption2 = await keyStore.Get(path);
        readOption2.BeError();

        listener.GetAllEvents().Count.Be(5);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CacheMiss(bool useHash)
    {
        using var host = await BuildService(useHash);
        var keyStore = host.Services.GetRequiredService<DataSpace>().GetFileStore("file");

        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        var ls = keyStore as KeySpace ?? throw new ArgumentException();
        var fileSystem = ls.KeyPathStrategy;

        string path = "test/cachemisstohit.txt";
        var content = "Test content".ToBytes();

        // Set data
        await keyStore.Set(path, new DataETag(content));

        (await keyStore.Get(path)).BeOk();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);
        listener.GetCounterValue("keyspace.get").Be(-1);

        // Clear cache
        string realPath = fileSystem.BuildPath(path);
        memoryCache.Remove(realPath);

        // Read should be cache hit
        var readOption1 = await keyStore.Get(path);
        readOption1.BeOk();
        listener.GetCounterValue("keyspace.cache.hit").Be(1);
        listener.GetCounterValue("keyspace.get").Be(1);

        // Delete again
        await keyStore.Delete(path);
        listener.GetCounterValue("keyspace.delete").Be(1);

        // Read should be cache miss
        var readOption2 = await keyStore.Get(path);
        readOption2.BeError();
        listener.GetCounterValue("keyspace.cache.miss").Be(-1);
        listener.GetCounterValue("keyspace.get").Be(1);

        listener.GetAllEvents().Count.Be(4);
    }
}
