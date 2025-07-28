using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services, IDataPipelineBuilder builder)
    {
        services.AddInMemoryFileStore();
    }

    private async Task<IHost> BuildService(
        bool addMemory,
        bool addFileStore,
        IDataProvider? custom,
        TimeSpan? memoryCacheDuration = null,
        TimeSpan? fileCacheDuration = null
        )
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));

                services.AddDataPipeline<EntityModel>(builder =>
                {
                    builder.MemoryCacheDuration = memoryCacheDuration;
                    builder.FileCacheDuration = fileCacheDuration;
                    builder.BasePath = nameof(DataClientTests);

                    if (addFileStore) AddStore(services, builder);

                    if (addMemory) builder.AddCacheMemory();
                    if (addFileStore) builder.AddFileStore();
                    if (custom != null) builder.AddProvider(_ => custom);
                });
            })
            .Build();

        if (addFileStore) await host.ClearStore<DataClientTests>();
        return host;
    }


    [Fact]
    public async Task NoHandler()
    {
        using var host = await BuildService(false, false, null);

        Verify.Throw<ArgumentException>(() => host.Services.GetDataClient<EntityModel>());
    }

    [Fact]
    public async Task ProviderCreatedCache()
    {
        int command = -1;
        var custom = new CustomProvider(x => command = x);

        using var host = await BuildService(false, false, custom);
        const string key = nameof(ProviderCreatedCache);

        IDataClient<EntityModel> cache = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataClientTests>();

        var readOption = await cache.Get(key, context);
        readOption.BeOk();

        readOption.Return().Action(x =>
        {
            x.Name.Be("CustomerProviderCreated");
            x.Age.Be(25);
        });
    }

    [Fact]
    public async Task OnlyMemoryCache()
    {
        using var host = await BuildService(true, false, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100));
        const string key = nameof(OnlyMemoryCache);

        IDataClient<EntityModel> dataHandler = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataHandler.GetDataProviders().OfType<CacheMemoryHandler>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        string path = host.Services.GetRequiredService<DataPipelineConfig<EntityModel>>().CreatePath(key);

        context.Location().LogInformation("#1 - Set value");
        var model = new EntityModel { Name = "OnlyMemoryCache", Age = 25 };
        var result = await dataHandler.Set(key, model, context);
        result.BeOk();
        memoryProvider.Counters.Hits.Be(0);
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);        // Set count
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#2 - Get from memory");
        var readOption = await dataHandler.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);            // Hit count
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#3 - Waiting for memory to expire");
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(path, out _), TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#4 - Waiting for memory to expire");
        readOption = await dataHandler.Get(key, context);
        readOption.IsNotFound().BeTrue();
        memoryProvider.Counters.Hits.Be(1);                //.Assert(x => x >= 1);  // At least one hit
        memoryProvider.Counters.Misses.Be(1);              // Miss count
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
    }

    [Fact]
    public async Task OnlyFileCache()
    {
        const string key = nameof(OnlyFileCache);
        using var host = await BuildService(false, true, null, fileCacheDuration: TimeSpan.FromMilliseconds(100));

        IDataClient<EntityModel> dataClient = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataClientTests>();
        FileStoreDataProvider fileProvider = dataClient.GetDataProviders().OfType<FileStoreDataProvider>().First();
        string path = host.Services.GetRequiredService<DataPipelineConfig<EntityModel>>().CreatePath(key);

        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await dataClient.Set(key, model, context);
        result.BeOk();
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);            // Set count
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        var readOption = await dataClient.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        fileProvider.Counters.Hits.Be(1);                // Hit count
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        await WaitForTool.WaitFor(async () =>
        {
            var request = new DataPipelineContext(DataPipelineCommand.Get, path, ((DataClient<EntityModel>)dataClient).Config);
            var isValidOption = await fileProvider.IsCacheIsValid(request, context);
            if (isValidOption.IsNotFound()) return true;  // File expired
            return false;
        }, TimeSpan.FromMinutes(1));

        readOption = await dataClient.Get(key, context);
        readOption.IsNotFound().BeTrue();
        fileProvider.Counters.Hits.Assert(x => x >= 1);  // At least one hit
        fileProvider.Counters.Misses.Be(2);              // Memory retired
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(1);
        fileProvider.Counters.RetireCount.Be(1);
    }

    [Fact]
    public async Task MemoryAndFileCache()
    {
        using var host = await BuildService(true, true, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100), fileCacheDuration: TimeSpan.FromMilliseconds(500));
        const string key = nameof(MemoryAndFileCache);

        IDataClient<EntityModel> dataClient = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataClient.GetDataProviders().OfType<CacheMemoryHandler>().First();
        FileStoreDataProvider fileProvider = dataClient.GetDataProviders().OfType<FileStoreDataProvider>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        string path = host.Services.GetRequiredService<DataPipelineConfig<EntityModel>>().CreatePath(key);

        context.Location().LogInformation("#1 - Create value");
        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await dataClient.Set(key, model, context);
        result.BeOk();
        memoryProvider.Counters.Hits.Be(0);
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);          // Set memory
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);            // Set file
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#2 - get from memory");
        var readOption = await dataClient.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);              // Hit on memory
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#3 - Waiting for memory to expire");
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(path, out _), TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#4 - Get from file, refresh cache");
        readOption = await dataClient.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);
        memoryProvider.Counters.Misses.Be(1);
        memoryProvider.Counters.SetCount.Be(2);          // Memory expired, file refreshes memory
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);                // File hit, will refresh memory
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#5 - Waiting for file to expire");
        await WaitForTool.WaitFor(async () =>
        {
            var request = new DataPipelineContext(DataPipelineCommand.Get, path, ((DataClient<EntityModel>)dataClient).Config);
            var existsOption = await fileProvider.IsCacheIsValid(request, context);
            if (existsOption.IsNotFound()) return true;  // File expired
            return false;
        }, TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#6 - Failed to return any value, all caches failed");
        readOption = await dataClient.Get(key, context);
        readOption.IsNotFound().BeTrue();
        memoryProvider.Counters.Hits.Assert(x => x >= 1, x => $"Invalid value={x}");
        memoryProvider.Counters.Misses.Be(2);            // Missed memory
        memoryProvider.Counters.SetCount.Be(2);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);
        fileProvider.Counters.Misses.Be(2);              // Missed file
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(1);
        fileProvider.Counters.RetireCount.Be(1);         // File was retried
    }

    [Fact]
    public async Task MemoryAndFileCacheWithProviderAsSource()
    {
        int command = -1;
        var custom = new CustomProvider(x => command = x);

        using var host = await BuildService(true, true, custom, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
        const string key = nameof(MemoryAndFileCacheWithProviderAsSource);

        IDataClient<EntityModel> dataClient = host.Services.GetDataClient<EntityModel>();
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataClient.GetDataProviders().OfType<CacheMemoryHandler>().First();
        FileStoreDataProvider fileProvider = dataClient.GetDataProviders().OfType<FileStoreDataProvider>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        string path = host.Services.GetRequiredService<DataPipelineConfig<EntityModel>>().CreatePath(key);

        // Make sure the cache is clear
        context.Location().LogInformation("#01 - clear cache for setup");
        await dataClient.Delete(key, context);
        memoryProvider.Counters.Clear();
        fileProvider.Counters.Clear();

        var model = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };

        context.Location().LogInformation("#1 - value is provided by the custom provider");
        var readOption = await dataClient.Get(key, context);   // Read from custom provider
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(0);              // Hit on memory
        memoryProvider.Counters.Misses.Be(1);
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(1);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#2 - value is read from memory cache");
        readOption = await dataClient.Get(key, context);  // Re-read, should be satisfied by cache
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);              // Hit on memory
        memoryProvider.Counters.Misses.Be(1);
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(1);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#3 - Waiting for memory to expire, retrieved by file");
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(path, out _), TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#4 - Getting value from file");
        readOption = await dataClient.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);
        memoryProvider.Counters.Misses.Be(2);
        memoryProvider.Counters.SetCount.Be(2);          // Memory expired, file refreshes memory
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);                // File hit
        fileProvider.Counters.Misses.Be(1);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#5 - Waiting for memory to expire, retrieved by file");
        await waitForFile();

        context.Location().LogInformation("#6 - Miss for memory and file, get from custom provider");
        readOption = await dataClient.Get(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();

        memoryProvider.Counters.Hits.Be(1);
        memoryProvider.Counters.Misses.Be(3);            // Missed memory
        memoryProvider.Counters.SetCount.Be(3);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);
        fileProvider.Counters.Misses.Be(3);              // Missed file
        fileProvider.Counters.SetCount.Be(2);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(1);
        fileProvider.Counters.RetireCount.Be(1);         // File was retried

        async Task waitForFile()
        {
            await WaitForTool.WaitFor(async () =>
            {
                var request = new DataPipelineContext(DataPipelineCommand.Get, path, ((DataClient<EntityModel>)dataClient).Config);
                var existsOption = await fileProvider.IsCacheIsValid(request, context);
                if (existsOption.IsNotFound()) return true;  // File expired
                return false;
            }, TimeSpan.FromMinutes(1));
        }
    }

    internal record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }

    public class CustomProvider : IDataProvider
    {
        public const int DeleteCmd = 0;
        public const int GetCmd = 1;
        public const int SetCmd = 2;

        private readonly Action<int> _action;
        public CustomProvider(Action<int> action) => _action = action.NotNull();

        public IDataProvider? InnerHandler { get; set; }

        public Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
        {
            switch (dataContext.Command)
            {
                case DataPipelineCommand.Append: throw new NotImplementedException("Append not implemented in CustomProvider");

                case DataPipelineCommand.Delete:
                    _action(DeleteCmd);
                    break;

                case DataPipelineCommand.Get:
                    var result = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
                    var data = result.ToDataETag();
                    dataContext = dataContext with { GetData = [data] };
                    _action(GetCmd);
                    break;

                case DataPipelineCommand.Set:
                    _action(SetCmd);
                    break;
            }

            return dataContext.ToOption().ToTaskResult();
        }
    }
}
