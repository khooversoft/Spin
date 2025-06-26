using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class JournalClient<T> : IJournalClient<T>
{
    private readonly IDataPipelineConfig _pipelineConfig;

    public JournalClient(IDataProvider dataProvider, IDataPipelineConfig pipelineConfig)
    {
        Handler = dataProvider.NotNull();
        _pipelineConfig = pipelineConfig.NotNull();
    }

    internal IDataProvider Handler { get; }

    public async Task<Option> Append(string key, IEnumerable<T> values, ScopeContext context)
    {
        var dataItems = values.NotNull().Select(x => x.ToDataETag()).ToImmutableArray();
        dataItems.Assert(x => x.Length > 0, "Empty list");

        var request = _pipelineConfig.CreateAppendList<T>(key, dataItems);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        var request = _pipelineConfig.CreateDelete<T>(key);
        var resultOption = await Handler.Execute(request, context);
        return resultOption.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context)
    {
        var request = _pipelineConfig.CreateGetList<T>(key);

        var resultOption = await Handler.Execute(request, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<T>>();

        IReadOnlyList<T> values = resultOption.Return().GetData.Select(x => x.ToObject<T>()).ToImmutableArray();
        return values.ToOption();
    }
}
