//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class DataClientDefault : IDataClient
//{
//    private const string message = "No handler found for key={key}";
//    private readonly ILogger<DataClientDefault> _logger;

//    public DataClientDefault(ILogger<DataClientDefault> logger) => _logger = logger.NotNull();

//    public Task<Option> Append<T>(string key, T value, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option(StatusCode.Conflict).ToTaskResult();
//    }

//    public virtual Task<Option> Delete(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option(StatusCode.NotFound).ToTaskResult();
//    }
//    public virtual Task<Option<string>> Exists(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option<string>(StatusCode.NotFound).ToTaskResult();
//    }

//    public virtual Task<Option<T>> Get<T>(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option<T>(StatusCode.NotFound).ToTaskResult();
//    }

//    public virtual Task<Option> Set<T>(string key, T value, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option(StatusCode.Conflict).ToTaskResult();
//    }
//}

//public class DataClientDefault<T> : IDataClient<T>
//{
//    private const string message = "No handler found for key={key}";
//    private readonly ILogger<DataClientDefault> _logger;

//    public DataClientDefault(ILogger<DataClientDefault> logger) => _logger = logger.NotNull();

//    public Task<Option> Append(string key, T value, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option(StatusCode.Conflict).ToTaskResult();
//    }

//    public virtual Task<Option> Delete(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option(StatusCode.NotFound).ToTaskResult();
//    }
//    public virtual Task<Option<string>> Exists(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option<string>(StatusCode.NotFound).ToTaskResult();
//    }

//    public virtual Task<Option<T>> Get(string key, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug(message, key);
//        return new Option<T>(StatusCode.NotFound).ToTaskResult();
//    }

//    public virtual Task<Option> Set(string key, T value, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.LogDebug("No handler found for key={key}", key);
//        return new Option(StatusCode.Conflict).ToTaskResult();
//    }
//}
