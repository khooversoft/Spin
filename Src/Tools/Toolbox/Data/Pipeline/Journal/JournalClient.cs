using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class JournalClient<T> : IJournalClient<T>
{
    public JournalClient(IDataProvider dataProvider) => Handler = dataProvider.NotNull();

    internal IDataProvider Handler { get; }

    public async Task<Option> Append(IEnumerable<T> values, ScopeContext context)
    {
        var dataItems = values.NotNull().Select(x => x.ToDataETag()).ToImmutableArray();
        dataItems.Assert(x => x.Length > 0, "Empty list");

        var request = new DataAppendList(typeof(T).Name, dataItems);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        var request = new DataDelete(typeof(T).Name);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<T>>> Get(ScopeContext context)
    {
        var request = new DataGetList(typeof(T).Name);
        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<T>>();

        IReadOnlyList<T> values = resultOption.Return().GetData.Select(x => x.ToObject<T>()).ToImmutableArray();
        return values.ToOption();
    }
}
