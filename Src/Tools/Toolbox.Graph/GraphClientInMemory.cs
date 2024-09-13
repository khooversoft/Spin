using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphClientInMemory : IGraphClient
{
    private readonly IGraphContext _graphContext;
    public GraphClientInMemory(IGraphContext graphContext) => _graphContext = graphContext.NotNull();

    public Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = QueryExecution.Execute(trxContext, command);
        return result;
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = await QueryExecution.Execute(trxContext, command);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items[0];
    }
}
