using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphClientInMemory : IGraphClient
{
    private readonly IGraphEngine _graphEngine;
    public GraphClientInMemory(IGraphEngine graphContext) => _graphEngine = graphContext.NotNull();

    public Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context) => _graphEngine.ExecuteBatch(command, context);

    public Task<Option<QueryResult>> Execute(string command, ScopeContext context) => _graphEngine.Execute(command, context);
}
