using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphClientInMemory : IGraphClient
{
    private readonly IGraphHost _graphHost;
    public GraphClientInMemory(IGraphHost graphContext) => _graphHost = graphContext.NotNull();

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        var result = await QueryExecution.Execute(_graphHost, command, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var result = await QueryExecution.Execute(_graphHost, command, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }
}
