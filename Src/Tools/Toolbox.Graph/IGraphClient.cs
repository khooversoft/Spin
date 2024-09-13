using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<QueryResult>> Execute(string command, ScopeContext context);
    Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context);
}