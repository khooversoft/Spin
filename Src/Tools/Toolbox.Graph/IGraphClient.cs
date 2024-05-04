using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<GraphQueryResults>> Execute(string command, ScopeContext context);
    Task<Option<GraphQueryResult>> ExecuteScalar(string command, ScopeContext context);
}

public class GraphClient : IGraphClient
{
    private readonly IGraphContext _graphContext;
    public GraphClient(IGraphContext graphContext) => _graphContext = graphContext.NotNull();

    public Task<Option<GraphQueryResults>> Execute(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = GraphCommand.Execute(trxContext, command);
        return result;
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string command, ScopeContext context)
    {
        var trxContext = _graphContext.CreateTrxContext(context);
        var result = await GraphCommand.Execute(trxContext, command);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items[0];
    }
}