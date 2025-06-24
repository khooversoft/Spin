using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataClient<T> : IDataClient<T>
{
    public DataClient(IDataProvider dataProvider) => Handler = dataProvider.NotNull();

    internal IDataProvider Handler { get; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
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

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        var request = new DataGet(key);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<T>();

        T value = resultOption.Return().GetData.First().ToObject<T>();
        return value;
    }

    public async Task<Option> Set(string key, T value, ScopeContext context)
    {
        var data = value.NotNull().ToDataETag();

        var request = new DataSet(key, data);
        var resultOption = await Handler.Execute(request, context);

        return resultOption.ToOptionStatus();
    }
}
