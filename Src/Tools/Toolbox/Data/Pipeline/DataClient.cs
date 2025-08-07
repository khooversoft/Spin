using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataClient<T>
{
    Task<Option> Append(string key, T value, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<T>> Get(string key, ScopeContext context);
    Task<Option> Set(string key, T value, ScopeContext context);

    Task<Option> Drain(ScopeContext context);
    Task<Option> ReleaseLock(string key, ScopeContext context);
}


public class DataClient<T> : IDataClient<T>
{
    private readonly DataPipelineConfig<T> _pipelineConfig;
    private readonly ILogger<DataClient<T>> _logger;

    public DataClient(IServiceProvider serviceProvider, DataPipelineConfig<T> pipelineConfig, ILogger<DataClient<T>> logger)
    {
        serviceProvider.NotNull();
        pipelineConfig.NotNull().Validate().ThrowOnError();

        Handler = pipelineConfig.Handlers.BuildHandlers(serviceProvider).ThrowOnError().Return();
        _pipelineConfig = pipelineConfig;
        _logger = logger.NotNull();
    }

    public IDataProvider Handler { get; }
    public IDataPipelineConfig Config => _pipelineConfig;

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        value.NotNull();
        context = context.With(_logger);

        var path = _pipelineConfig.CreatePath<T>(key);
        context.LogDebug("Append key={key}, path={path}", key, path);

        var data = value.NotNull().ToDataETag();

        var request = new DataPipelineContext(DataPipelineCommand.Append, path, [data], _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);

        var path = _pipelineConfig.CreatePath<T>(key);
        context.LogDebug("Delete key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.Delete, path, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        context = context.With(_logger);

        var path = _pipelineConfig.CreatePath<T>(key);
        context.LogDebug("Get key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.Get, path, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<T>();

        T value = resultOption.Return().GetData.First().ToObject<T>();
        return value;
    }

    public async Task<Option> Set(string key, T value, ScopeContext context)
    {
        value.NotNull();
        context = context.With(_logger);

        var path = _pipelineConfig.CreatePath<T>(key);
        context.LogDebug("Set key={key}, path={path}", key, path);

        var data = value.NotNull().ToDataETag();

        var request = new DataPipelineContext(DataPipelineCommand.Set, path, [data], _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<IStorePathDetail>>> Search(string pattern, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Search, pattern={pattern}", pattern);

        var path = _pipelineConfig.CreateSearch(null, pattern);
        var request = new DataPipelineContext(DataPipelineCommand.Search, path, _pipelineConfig);

        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<IStorePathDetail>>();

        var result = resultOption.Return().GetData
            .Select(x => x.ToObject<StorePathDetail>())
            .ToImmutableArray();

        return result;
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Operations
    ///

    public async Task<Option> Drain(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Drain");

        var request = new DataPipelineContext(DataPipelineCommand.Drain, "drain", _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> ReleaseLock(string key, ScopeContext context)
    {
        context = context.With(_logger);

        var path = _pipelineConfig.CreatePath<T>(key);
        context.LogDebug("ReleaseLock, key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.ReleaseLock, path, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }
}


public static class DataClientExtensions
{
    public static IEnumerable<IDataProvider> GetDataProviders<T>(this IDataClient<T> dataClient)
    {
        IDataProvider handler = dataClient switch
        {
            DataClient<T> client => client.Handler,
            _ => throw new InvalidOperationException("Unsupported data client type")
        };

        var list = new Sequence<IDataProvider>();
        list += handler;

        while (handler.InnerHandler is not null)
        {
            handler = handler.InnerHandler;
            list += handler;
        }

        return list;
    }
}
