using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class DirectoryActorConnector : IGraphCommand
{
    private readonly IClusterClient _clusterClient;
    public DirectoryActorConnector(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option<GraphQueryResults>> Execute(string command, ScopeContext context) => _clusterClient
        .GetDirectoryActor()
        .Execute(command, context);

    public Task<Option<GraphQueryResult>> ExecuteScalar(string command, ScopeContext context) => _clusterClient
        .GetDirectoryActor()
        .ExecuteScalar(command, context);
}
