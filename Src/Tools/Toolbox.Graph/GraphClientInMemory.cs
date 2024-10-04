using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphClientInMemory : IGraphClient
{
    private readonly IGraphHost _graphContext;
    public GraphClientInMemory(IGraphHost graphContext) => _graphContext = graphContext.NotNull();

    public Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        var result = QueryExecution.Execute(_graphContext, command, context);
        return result;
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var result = await QueryExecution.Execute(_graphContext, command, context);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }
}
