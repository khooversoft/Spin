using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Client.Common;

public static class DataClientCommonTests
{
    public static void NoHandler(IHost host, string pipelineName)
    {
        host.NotNull();

        Verify.Throw<ArgumentException>(() => host.Services.GetDataClient<EntityModel>(pipelineName));
    }

    public static async Task ProviderCreatedCache(IHost host, string pipelineName)
    {
        host.NotNull();
        const string key = nameof(ProviderCreatedCache);

        IDataClient<EntityModel> cache = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataClientTests>();

        var readOption = await cache.Get(key, context);
        readOption.BeOk();

        readOption.Return().Action(x =>
        {
            x.Name.Be("CustomerProviderCreated");
            x.Age.Be(25);
        });
    }

    public static async Task OnlyMemoryCache(IHost host, string pipelineName)
    {
        host.NotNull();
        pipelineName.NotEmpty();
        const string key = nameof(OnlyMemoryCache);

        IDataClient<EntityModel> dataHandler = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataHandler.GetDataProviders().OfType<CacheMemoryHandler>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

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
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(dataContext.Path, out _), TimeSpan.FromMinutes(1));

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

    public static async Task OnlyFileCache(IHost host, string pipelineName)
    {
        const string key = nameof(OnlyFileCache);

        IDataClient<EntityModel> dataHandler = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataClientTests>();
        FileStoreDataProvider fileProvider = dataHandler.GetDataProviders().OfType<FileStoreDataProvider>().First();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await dataHandler.Set(key, model, context);
        result.BeOk();
        fileProvider.Counters.Hits.Be(0);
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);            // Set count
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        var readOption = await dataHandler.Get(key, context);
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
            var isValidOption = await fileProvider.IsCacheIsValid(dataContext, context);
            if (isValidOption.IsNotFound()) return true;  // File expired
            return false;
        }, TimeSpan.FromMinutes(1));

        readOption = await dataHandler.Get(key, context);
        readOption.IsNotFound().BeTrue();
        fileProvider.Counters.Hits.Assert(x => x >= 1);  // At least one hit
        fileProvider.Counters.Misses.Be(2);              // Memory retired
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(1);
        fileProvider.Counters.RetireCount.Be(1);
    }


    public static async Task MemoryAndFileCache(IHost host, string pipelineName)
    {
        const string key = nameof(MemoryAndFileCache);

        IDataClient<EntityModel> dataHandler = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataHandler.GetDataProviders().OfType<CacheMemoryHandler>().First();
        FileStoreDataProvider fileProvider = dataHandler.GetDataProviders().OfType<FileStoreDataProvider>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        context.Location().LogInformation("#1 - Create value");
        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await dataHandler.Set(key, model, context);
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
        var readOption = await dataHandler.Get(key, context);
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
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(dataContext.Path, out _), TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#4 - Get from file, refresh cache");
        readOption = await dataHandler.Get(key, context);
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
            var existsOption = await fileProvider.IsCacheIsValid(dataContext, context);
            if (existsOption.IsNotFound()) return true;  // File expired
            return false;
        }, TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#6 - Failed to return any value, all caches failed");
        readOption = await dataHandler.Get(key, context);
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

    public static async Task MemoryAndFileCacheWithProviderAsSource(IHost host, string pipelineName)
    {
        const string key = nameof(MemoryAndFileCacheWithProviderAsSource);

        IDataClient<EntityModel> dataHandler = host.Services.GetDataClient<EntityModel>(pipelineName);
        var context = host.Services.CreateContext<DataClientTests>();
        CacheMemoryHandler memoryProvider = dataHandler.GetDataProviders().OfType<CacheMemoryHandler>().First();
        FileStoreDataProvider fileProvider = dataHandler.GetDataProviders().OfType<FileStoreDataProvider>().First();
        IMemoryCache memoryCache = host.Services.GetRequiredService<IMemoryCache>();
        DataPipelineContext dataContext = host.Services.GetDataPipelineBuilder<EntityModel>(pipelineName).CreateGet<EntityModel>(key);

        // Make sure the cache is clear
        context.Location().LogInformation("#01 - clear cache for setup");
        await dataHandler.Delete(key, context);
        memoryProvider.Counters.Clear();
        fileProvider.Counters.Clear();

        var model = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };

        context.Location().LogInformation("#1 - value is provided by the custom provider");
        var readOption = await dataHandler.Get(key, context);   // Read from custom provider
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
        readOption = await dataHandler.Get(key, context);  // Re-read, should be satisfied by cache
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
        WaitForTool.WaitFor(() => !memoryCache.TryGetValue(dataContext.Path, out _), TimeSpan.FromMinutes(1));

        context.Location().LogInformation("#4 - Getting value from file");
        readOption = await dataHandler.Get(key, context);
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
        readOption = await dataHandler.Get(key, context);
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
                var existsOption = await fileProvider.IsCacheIsValid(dataContext, context);
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
