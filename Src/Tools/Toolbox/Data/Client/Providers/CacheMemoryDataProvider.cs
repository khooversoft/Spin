using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;


public class CacheMemoryDataProvider : IDataProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheMemoryDataProvider> _logger;
    private readonly IOptions<DataClientOption> _option;

    public CacheMemoryDataProvider(IMemoryCache memoryCache, IOptions<DataClientOption> option, ILogger<CacheMemoryDataProvider> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public string Name => nameof(CacheMemoryDataProvider);
    public DataClientCounters Counters { get; } = new();

    public Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        if (_memoryCache.TryGetValue(key, out _))
        {
            context.LogDebug("Found key={key} in in-memory cache", key);
            return Name.ToOption().ToTaskResult();
        }

        return new Option<string>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Deleting key={key} from in-memory cache", key);

        _memoryCache.Remove(key);
        Counters.AddDeleteCount();

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting key={key} from in-memory cache", key);

        if (_memoryCache.TryGetValue(key, out T? value))
        {
            Counters.AddHits();
            context.LogDebug("Found key={key} in in-memory cache", key);
            return value.ToOption().ToTaskResult();
        }

        Counters.AddMisses();
        context.LogDebug("Not found key={key} in in-memory cache", key);
        return new Option<T>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option> Set<T>(string key, T value, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Setting key={key} from in-memory cache", key);

        var cacheOption = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _option.Value.MemoryCacheDuration,
        };

        _memoryCache.Set(key, value, cacheOption);
        Counters.AddSetCount();
        return new Option(StatusCode.OK).ToTaskResult();
    }
}
