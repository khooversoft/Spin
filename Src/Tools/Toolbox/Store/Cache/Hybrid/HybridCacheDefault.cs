using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class HybridCacheDefault : IHybridCache
{
    private readonly ILogger<HybridCacheDefault> _logger;
    public HybridCacheDefault(ILogger<HybridCacheDefault> logger) => _logger = logger.NotNull();

    public Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.NotFound).ToTaskResult();
    }
    public Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option<string>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option<T>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.Conflict).ToTaskResult();
    }
}

public class HybridCacheDefault<T> : IHybridCache<T>
{
    private readonly ILogger<HybridCacheDefault> _logger;
    public HybridCacheDefault(ILogger<HybridCacheDefault> logger) => _logger = logger.NotNull();

    public Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.NotFound).ToTaskResult();
    }
    public Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option<string>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<T>> Get(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option<T>(StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option> Set(string key, T value, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.Conflict).ToTaskResult();
    }
}
