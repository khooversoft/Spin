using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<GraphQueryResult>> Execute(string command, ScopeContext context);
    Task<Option<GraphQueryResults>> ExecuteBatch(string command, ScopeContext context);
}
