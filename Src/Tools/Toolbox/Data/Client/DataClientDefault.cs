using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataClientDefault : IDataClient
{
    private readonly ILogger<DataClientDefault> _logger;
    public DataClientDefault(ILogger<DataClientDefault> logger) => _logger = logger.NotNull();

    public virtual Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.NotFound).ToTaskResult();
    }
    public virtual Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option<string>(StatusCode.NotFound).ToTaskResult();
    }

    public virtual Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option<T>(StatusCode.NotFound).ToTaskResult();
    }

    public virtual Task<Option> Set<T>(string key, T value, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.Conflict).ToTaskResult();
    }
}

public class DataClientDefault<T> : IDataClient<T>
{
    private readonly ILogger<DataClientDefault> _logger;
    public DataClientDefault(ILogger<DataClientDefault> logger) => _logger = logger.NotNull();

    public virtual Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.NotFound).ToTaskResult();
    }
    public virtual Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option<string>(StatusCode.NotFound).ToTaskResult();
    }

    public virtual Task<Option<T>> Get(string key, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option<T>(StatusCode.NotFound).ToTaskResult();
    }

    public virtual Task<Option> Set(string key, T value, object? state, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("No handler found for key={key}", key);
        return new Option(StatusCode.Conflict).ToTaskResult();
    }
}
