using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;


public record HybridCache : IHybridCache
{
    private readonly IReadOnlyList<IHybridCacheProvider> _caches;
    private readonly ILogger<HybridCache> _logger;

    public HybridCache(IEnumerable<IHybridCacheProvider> caches, ILogger<HybridCache> logger)
    {
        _caches = caches.NotNull().ToImmutableArray();
        _logger = logger.NotNull();
    }

    public string Name => nameof(HybridCache);

    public async Task<Option<string>> Exists(string key, ScopeContext context)
    {
        foreach (var item in _caches)
        {
            var existsOption = await item.Exists(key, context).ConfigureAwait(false);
            if (existsOption.IsOk())
            {
                context.LogDebug("Found key={key} in hybrid cache, name={name}", key, Name);
                return item.Name.ToOption();
            }
        }

        return StatusCode.NotFound;
    }

    public async Task<Option> Delete<T>(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Deleting key={key} from hybrid cache, name={name}", key, Name);

        Option status = StatusCode.OK;

        foreach (var item in _caches)
        {
            var deleteOption = await item.Delete<T>(key, context).ConfigureAwait(false);
            if (deleteOption.IsError() && status.IsOk()) status = status = deleteOption;
        }

        return status;
    }

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting key={key} from hybrid cache, name={name}", key, Name);

        var list = new Sequence<IHybridCacheProvider>();

        foreach (var item in _caches)
        {
            var getOption = await item.Get<T>(key, context).ConfigureAwait(false);
            if (getOption.IsOk())
            {
                context.LogDebug("Found key={key} from hybrid cache", key);

                T value = getOption.Return();
                await list.ForEachAsync(async x => await x.Set<T>(key, value, context));
                return value;
            }

            list += item;
        }

        context.LogDebug("Did not find key={key} in hybrid cache, name={name}", key, Name);
        return StatusCode.NotFound;
    }

    public async Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting key={key} from hybrid cache, name={name}", key, Name);

        foreach (var item in _caches)
        {
            var setOption = await item.Set<T>(key, value, context).ConfigureAwait(false);
            if (setOption.IsError())
            {
                context.LogDebug("Fail to write key={key} to hybrid cache, name={name}", key, Name);
                return setOption;
            }
        }

        context.LogDebug("Set key={key} in hybrid cache, name={name}", key, Name);
        return StatusCode.OK;
    }
}
