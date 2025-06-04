using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Store;

public static class HybridCacheCommonTests
{
    public static async Task NoCache(IHost host)
    {
        host.NotNull();
        const string key = nameof(NoCache);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();

        var model = new EntityModel { Name = "NoCache", Age = 25 };
        var result = await cache.Set(key, model, context);
        result.BeOk();

        var readOption = await cache.Get<EntityModel>(key, context);
        readOption.IsNotFound().BeTrue();
    }

    public static async Task ProviderCreatedCache(IHost host)
    {
        host.NotNull();
        const string key = nameof(ProviderCreatedCache);
        int command = -1;

        var custom = new CustomProvider(x => command = x);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();

        var readOption = await cache.Get<EntityModel>(key, context);
        readOption.BeOk();

        readOption.Return().Action(x =>
        {
            x.Name.Be("CustomerProviderCreated");
            x.Age.Be(25);
        });
    }

    public static async Task OnlyMemoryCache(IHost host)
    {
        host.NotNull();
        const string key = nameof(OnlyMemoryCache);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();
        IHybridCacheProvider memoryProvider = GetMemoryProvider(host);

        context.Location().LogInformation("#1 - Set value");
        var model = new EntityModel { Name = "OnlyMemoryCache", Age = 25 };
        var result = await cache.Set(key, model, context);
        result.BeOk();
        memoryProvider.Counters.Hits.Be(0);
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);        // Set count
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#2 - Get from memory");
        var readOption = await cache.Get<EntityModel>(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);            // Hit count
        memoryProvider.Counters.Misses.Be(0);
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#3 - Waiting for memory to expire");
        await WaitFor(async () =>
        {
            var existsOption = await memoryProvider.Exists(key, context);
            if (existsOption.IsNotFound()) return true;
            return false;
        });

        context.Location().LogInformation("#4 - Waiting for memory to expire");
        readOption = await cache.Get<EntityModel>(key, context);
        readOption.IsNotFound().BeTrue();
        memoryProvider.Counters.Hits.Be(1);                //.Assert(x => x >= 1);  // At least one hit
        memoryProvider.Counters.Misses.Be(1);              // Miss count
        memoryProvider.Counters.SetCount.Be(1);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
    }

    public static async Task OnlyFileCache(IHost host)
    {
        const string key = nameof(OnlyFileCache);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();
        IHybridCacheProvider counter = GetFileProvider(host);

        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await cache.Set(key, model, context);
        result.BeOk();
        counter.Counters.Hits.Be(0);
        counter.Counters.Misses.Be(0);
        counter.Counters.SetCount.Be(1);            // Set count
        counter.Counters.SetFailCount.Be(0);
        counter.Counters.DeleteCount.Be(0);
        counter.Counters.RetireCount.Be(0);

        var readOption = await cache.Get<EntityModel>(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        counter.Counters.Hits.Be(1);                // Hit count
        counter.Counters.Misses.Be(0);
        counter.Counters.SetCount.Be(1);
        counter.Counters.SetFailCount.Be(0);
        counter.Counters.DeleteCount.Be(0);
        counter.Counters.RetireCount.Be(0);

        await WaitFor(async () =>
        {
            var existsOption = await cache.Exists(key, context);
            if (existsOption.IsNotFound()) return true;  // File expired
            return false;
        });

        readOption = await cache.Get<EntityModel>(key, context);
        readOption.IsNotFound().BeTrue();
        counter.Counters.Hits.Assert(x => x >= 1);  // At least one hit
        counter.Counters.Misses.Be(1);              // Memory retired
        counter.Counters.SetCount.Be(1);
        counter.Counters.SetFailCount.Be(0);
        counter.Counters.DeleteCount.Be(0);
        counter.Counters.RetireCount.Be(1);
    }


    public static async Task MemoryAndFileCache(IHost host, TimeSpan? wait1 = null, TimeSpan? wait2 = null)
    {
        const string key = nameof(MemoryAndFileCache);
        wait1 ??= TimeSpan.FromMilliseconds(100);
        wait2 ??= TimeSpan.FromMilliseconds(500);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();
        IHybridCacheProvider memoryProvider = GetMemoryProvider(host);
        IHybridCacheProvider fileProvider = GetFileProvider(host);

        context.Location().LogInformation("#1 - Create value");
        var model = new EntityModel { Name = "OnlyFileCache", Age = 25 };
        var result = await cache.Set(key, model, context);
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
        var readOption = await cache.Get<EntityModel>(key, context);
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
        await WaitFor(async () =>
        {
            var lookup = await memoryProvider.Exists(key, context);
            if (lookup.IsNotFound()) return true;  // Memory expired
            return false;
        });

        context.Location().LogInformation("#4 - Get from file, refresh cache");
        readOption = await cache.Get<EntityModel>(key, context);
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
        await WaitFor(async () =>
        {
            var lookup = await fileProvider.Exists(key, context);
            if (lookup.IsNotFound()) return true;  // File expired
            return false;
        });

        context.Location().LogInformation("#6 - Failed to return any value, all caches failed");
        readOption = await cache.Get<EntityModel>(key, context);
        readOption.IsNotFound().BeTrue();
        memoryProvider.Counters.Hits.Assert(x => x >= 1, x => $"Invalid value={x}");
        memoryProvider.Counters.Misses.Be(2);            // Missed memory
        memoryProvider.Counters.SetCount.Be(2);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);
        fileProvider.Counters.Misses.Be(1);              // Missed file
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(1);         // File was retried
    }

    public static async Task MemoryAndFileCacheWithProviderAsSource(IHost host, TimeSpan? wait1 = null, TimeSpan? wait2 = null)
    {
        const string key = nameof(MemoryAndFileCacheWithProviderAsSource);
        wait1 ??= TimeSpan.FromMilliseconds(100);
        wait2 ??= TimeSpan.FromMilliseconds(500);

        IHybridCache cache = host.Services.GetRequiredService<IHybridCache>();
        var context = host.Services.CreateContext<HybridCacheTests>();
        IHybridCacheProvider memoryProvider = GetMemoryProvider(host);
        IHybridCacheProvider fileProvider = GetFileProvider(host);

        // Make sure the cache is clear
        context.Location().LogInformation("#01 - clear cache for setup");
        await cache.Delete<EntityModel>(key, context);
        memoryProvider.Counters.Clear();
        fileProvider.Counters.Clear();

        var model = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };

        context.Location().LogInformation("#1 - value is provided by the customer provider");
        var readOption = await cache.Get<EntityModel>(key, context);   // Read from custom provider
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(0);              // Hit on memory
        memoryProvider.Counters.Misses.Be(1);
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

        context.Location().LogInformation("#2 - value is read from memory cache");
        readOption = await cache.Get<EntityModel>(key, context);  // Re-read, should be satisfied by cache
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);              // Hit on memory
        memoryProvider.Counters.Misses.Be(1);
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

        context.Location().LogInformation("#3 - Waiting for memory to expire, retrieved by file");
        await WaitFor(async () =>
        {
            var lookup = await memoryProvider.Exists(key, context);
            if (lookup.IsNotFound()) return true;  // Memory expired
            return false;
        });

        context.Location().LogInformation("#4 - Getting value from file");
        readOption = await cache.Get<EntityModel>(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();
        memoryProvider.Counters.Hits.Be(1);
        memoryProvider.Counters.Misses.Be(2);
        memoryProvider.Counters.SetCount.Be(2);          // Memory expired, file refreshes memory
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);                // File hit
        fileProvider.Counters.Misses.Be(0);
        fileProvider.Counters.SetCount.Be(1);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(0);

        context.Location().LogInformation("#5 - Waiting for memory to expire, retrieved by file");
        await WaitFor(async () =>
        {
            var lookup = await fileProvider.Exists(key, context);
            if (lookup.IsNotFound()) return true;       // File expired
            return false;
        });

        context.Location().LogInformation("#6 - Miss for memory and file, get from custom provider");
        readOption = await cache.Get<EntityModel>(key, context);
        readOption.BeOk();
        (readOption.Return() == model).BeTrue();

        memoryProvider.Counters.Hits.Be(1);
        memoryProvider.Counters.Misses.Be(3);            // Missed memory
        memoryProvider.Counters.SetCount.Be(3);
        memoryProvider.Counters.SetFailCount.Be(0);
        memoryProvider.Counters.DeleteCount.Be(0);
        memoryProvider.Counters.RetireCount.Be(0);
        fileProvider.Counters.Hits.Be(1);
        fileProvider.Counters.Misses.Be(1);              // Missed file
        fileProvider.Counters.SetCount.Be(2);
        fileProvider.Counters.SetFailCount.Be(0);
        fileProvider.Counters.DeleteCount.Be(0);
        fileProvider.Counters.RetireCount.Be(1);         // File was retried
    }


    private static IHybridCacheProvider GetMemoryProvider(IHost host) => host.Services
        .GetServices<IHybridCacheProvider>()
        .OfType<HybridCacheMemoryProvider>()
        .First();

    private static IHybridCacheProvider GetFileProvider(IHost host) => host.Services
        .GetServices<IHybridCacheProvider>()
        .OfType<HybridCacheFileStoreProvider>()
        .First();

    private static async Task WaitFor(Func<Task<bool>> action)
    {
        await new CancellationTokenSource(TimeSpan.FromMinutes(1)).Func(async x =>
        {
            while (true)
            {
                x.Token.ThrowIfCancellationRequested();
                var isReady = await action();
                if (isReady) return;  // Condition met

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        });
    }

    private record EntityModel
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }

    public class CustomProvider : IHybridCacheProvider
    {
        public const int DeleteCmd = 0;
        public const int GetCmd = 1;
        public const int SetCmd = 2;

        private readonly Action<int> _action;
        public CustomProvider(Action<int> action) => _action = action.NotNull();

        public string Name => throw new NotImplementedException();
        public HybridCacheCounters Counters => new();

        public Task<Option> Delete<T>(string key, ScopeContext context)
        {
            _action(DeleteCmd);
            return Task.FromResult<Option>(StatusCode.OK);
        }

        public Task<Option<string>> Exists(string key, ScopeContext context)
        {
            return new Option<string>(StatusCode.OK).ToTaskResult();
        }

        public Task<Option<T>> Get<T>(string key, ScopeContext context)
        {
            _action(GetCmd);
            var result = new EntityModel { Name = "CustomerProviderCreated", Age = 25 };
            return result.Cast<T>().ToOption<T>().ToTaskResult();
        }

        public Task<Option> Set<T>(string key, T value, ScopeContext context)
        {
            _action(SetCmd);
            return Task.FromResult<Option>(StatusCode.OK);
        }
    }

}
