using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphClientInMemory : IGraphClient
{
    private readonly IGraphContext _graphContext;
    public GraphClientInMemory(IGraphContext graphContext) => _graphContext = graphContext.NotNull();

    public Task<Option<GraphQueryResults>> ExecuteBatch(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = GraphCommand.Execute(trxContext, command);
        return result;
    }

    public async Task<Option<GraphQueryResult>> Execute(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = await GraphCommand.Execute(trxContext, command);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items[0];
    }
}

public class GraphClientInMemory2 : IGraphClient2
{
    private readonly IGraphContext _graphContext;
    public GraphClientInMemory2(IGraphContext graphContext) => _graphContext = graphContext.NotNull();

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
