using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Test")]

namespace Toolbox.Data;

public class DataClient : IDataClient
{
    public DataClient(IDataProvider dataProvider) => Handler = dataProvider;
    internal IDataProvider Handler { get; }

    public async Task<Option> Append<T>(string key, T value, ScopeContext context)
    {
        var data = value.NotNull().ToDataETag();

        var request = new DataAppend(key, data);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        var request = new DataDelete(key);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        var request = new DataGet(key);
        var resultOption = await Handler.Execute(request, context);
        if( resultOption.IsError() ) return resultOption.ToOptionStatus<T>();

        T value = resultOption.Return().GetData.First().ToObject<T>();
        return value;
    }

    public async Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        var data = value.NotNull().ToDataETag();

        var request = new DataSet(key, data);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }
}
