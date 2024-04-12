using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphAccess
{
    private readonly GraphDbAccess _graphDbContext;
    internal GraphAccess(GraphDbAccess graphDbContext) => _graphDbContext = graphDbContext.NotNull();

    public async Task<Option<GraphQueryResults>> Execute(string graphQuery, ScopeContext context)
    {
        var graphContext = new GraphContext(_graphDbContext.Map, _graphDbContext.GraphStore, _graphDbContext.ChangeTrace, context);
        var result = await GraphCommand.Execute(graphContext, graphQuery);
        return result;
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string graphQuery, ScopeContext context)
    {
        var graphContext = new GraphContext(_graphDbContext.Map, _graphDbContext.GraphStore, _graphDbContext.ChangeTrace, context);
        var result = await GraphCommand.Execute(graphContext, graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items.First();
    }
}