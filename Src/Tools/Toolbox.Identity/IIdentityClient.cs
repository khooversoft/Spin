using Toolbox.Data;
using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Identity;

public interface IIdentityClient
{
    Task<Option> Clear(string principalId, string traceId);
    Task<Option<GraphQueryResults>> Execute(string command, string traceId);
}
