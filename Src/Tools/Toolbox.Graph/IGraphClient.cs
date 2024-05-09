using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<GraphQueryResults>> Execute(string command, ScopeContext context);
    Task<Option<GraphQueryResult>> ExecuteScalar(string command, ScopeContext context);
}
