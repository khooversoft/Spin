using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

/// <summary>
/// Path are {pipelineName}/{typeName}/{key}/{jsonFile}
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataClient<T> : IDataClient<T>
{
    private readonly IDataPipelineConfig _pipelineConfig;

    public DataClient(IDataProvider dataProvider, IDataPipelineConfig pipelineConfig)
    {
        Handler = dataProvider.NotNull();
        _pipelineConfig = pipelineConfig.NotNull();
    }

    internal IDataProvider Handler { get; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        var data = value.NotNull().ToDataETag();

        var request = _pipelineConfig.CreateAppend<T>(key, data);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        var request = _pipelineConfig.CreateDelete<T>(key);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        var request = _pipelineConfig.CreateGet<T>(key);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<T>();

        T value = resultOption.Return().GetData.First().ToObject<T>();
        return value;
    }

    public async Task<Option> Set(string key, T value, ScopeContext context)
    {
        var data = value.NotNull().ToDataETag();

        var request = _pipelineConfig.CreateSet<T>(key, data);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }
}
