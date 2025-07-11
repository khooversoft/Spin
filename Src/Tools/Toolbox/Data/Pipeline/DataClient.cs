using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

/// <summary>
/// Path are {pipelineName}/{typeName}/{key}/{jsonFile}
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataClient<T> : IDataClient<T>
{
    public DataClient(IDataProvider dataProvider, IDataPipelineConfig pipelineConfig)
    {
        Handler = dataProvider.NotNull();
        PipelineConfig = pipelineConfig.NotNull();
    }

    public IDataProvider Handler { get; }
    public IDataPipelineConfig PipelineConfig { get; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        context.LogDebug("DataClient: Append key={key}, type={type}", key, typeof(T).Name);
        var data = value.NotNull().ToDataETag();

        var request = PipelineConfig.CreateAppend<T>(key, data);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        context.LogDebug("DataClient: Delete key={key}", key);

        var request = PipelineConfig.CreateDelete<T>(key);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        context.LogDebug("DataClient: Get key={key}, type={type}", key, typeof(T).Name);

        var request = PipelineConfig.CreateGet<T>(key);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<T>();

        T value = resultOption.Return().GetData.First().ToObject<T>();
        return value;
    }

    public async Task<Option> Set(string key, T value, ScopeContext context)
    {
        context.LogDebug("DataClient: Set key={key}, type={type}", key, typeof(T).Name);
        var data = value.NotNull().ToDataETag();

        var request = PipelineConfig.CreateSet<T>(key, data);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// List Operations
    ///

    public async Task<Option> AppendList(string key, IEnumerable<T> values, ScopeContext context)
    {
        context.LogDebug("DataClient: AppendList key={key}, type={type}", key, typeof(T).Name);
        var dataItems = values.NotNull().Select(x => x.ToDataETag()).ToImmutableArray();
        dataItems.Assert(x => x.Length > 0, "Empty list");

        var request = PipelineConfig.CreateAppendList<T>(key, dataItems);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> DeleteList(string key, ScopeContext context)
    {
        context.LogDebug("DataClient: DeleteList key={key}", key);

        var request = PipelineConfig.CreateDeleteList<T>(key);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<T>>> GetList(string key, ScopeContext context)
    {
        context.LogDebug("DataClient: GetList key={key}, type={type}", key, typeof(T).Name);
        var request = PipelineConfig.CreateGetList<T>(key);

        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<T>>();

        IReadOnlyList<T> values = resultOption.Return().GetData.Select(x => x.ToObject<T>()).ToImmutableArray();
        return values.ToOption();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Operations
    ///

    public async Task<Option> Drain(ScopeContext context)
    {
        context.LogDebug("DataClient: Drain");

        var request = PipelineConfig.CreateDrain();
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> ReleaseLock(string key, ScopeContext context)
    {
        context.LogDebug("DataClient: ReleaseLock, key={key}", key);

        var request = PipelineConfig.CreateReleaseLock<T>(key);
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
