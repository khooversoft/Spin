using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphAccess
{
    private readonly GraphDbContext _graphDbContext;
    internal GraphAccess(GraphDbContext graphDbContext) => _graphDbContext = graphDbContext.NotNull();

    public async Task<Option<GraphQueryResults>> Execute(string graphQuery, ScopeContext context)
    {
        var result = await GraphCommand.Execute(_graphDbContext.Map, graphQuery, _graphDbContext.GraphStore, context);
        return result;
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string graphQuery, ScopeContext context)
    {
        var result = await GraphCommand.Execute(_graphDbContext.Map, graphQuery, _graphDbContext.GraphStore, context);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items.First();
    }
}