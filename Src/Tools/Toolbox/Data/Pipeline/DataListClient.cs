using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;


public interface IDataListClient<T>
{
    Task<Option> Append(string key, IEnumerable<T> values, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context);
    Task<Option<IReadOnlyList<IStorePathDetail>>> Search(string key, string pattern, ScopeContext context);
    Task<Option> Drain(ScopeContext context);
}


public class DataListClient<T> : IDataListClient<T>
{
    private readonly DataPipelineConfig<T> _pipelineConfig;
    private readonly ILogger<DataListClient<T>> _logger;

    public DataListClient(IServiceProvider serviceProvider, DataPipelineConfig<T> pipelineConfig, ILogger<DataListClient<T>> logger)
    {
        serviceProvider.NotNull();
        pipelineConfig.NotNull().Validate().ThrowOnError();

        Handler = pipelineConfig.Handlers.BuildHandlers(serviceProvider).ThrowOnError().Return();
        _pipelineConfig = pipelineConfig;
        _logger = logger.NotNull();
    }

    public IDataProvider Handler { get; }
    public IDataPipelineConfig Config => _pipelineConfig;

    public async Task<Option> Append(string key, IEnumerable<T> values, ScopeContext context)
    {
        key.NotEmpty();
        values.NotNull();
        context = context.With(_logger);

        string path = _pipelineConfig.CreatePath(key);
        context.LogDebug("AppendList key={key}, path={path}", key, path);

        var dataItems = values
            .Select(x => x.ToDataETag())
            .ToImmutableArray()
            .Assert(x => x.Length > 0, "Empty list");

        var request = new DataPipelineContext(DataPipelineCommand.AppendList, path, dataItems, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        key.NotEmpty();
        context = context.With(_logger);

        string path = _pipelineConfig.CreateSearch(key, "**/*");
        context.LogDebug("DeleteList key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.DeleteList, path, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context)
    {
        key.NotEmpty();
        context = context.With(_logger);

        string path = _pipelineConfig.CreateSearch(key, pattern);
        context.LogDebug("GetList key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.GetList, path, _pipelineConfig) { Pattern = pattern };
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<T>>();

        IReadOnlyList<T> values = resultOption.Return().GetData
            .Select(x => x.ToObject<T>())
            .ToImmutableArray();

        return values.ToOption();
    }

    public async Task<Option<IReadOnlyList<IStorePathDetail>>> Search(string key, string pattern, ScopeContext context)
    {
        string path = _pipelineConfig.CreateSearch(key, pattern);
        context.LogDebug("SearchList key={key}, path={path}", key, path);

        var request = new DataPipelineContext(DataPipelineCommand.SearchList, path, _pipelineConfig);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<IStorePathDetail>>();

        IReadOnlyList<IStorePathDetail> values = resultOption.Return().GetData
            .First()
            .ToObject<IReadOnlyList<StorePathDetail>>();

        return values.ToOption();
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
}


public static class DataListTool
{
    public static IEnumerable<IDataProvider> GetDataProviders<T>(this IDataListClient<T> dataClient)
    {
        IDataProvider handler = dataClient switch
        {
            DataListClient<T> client => client.Handler,
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
