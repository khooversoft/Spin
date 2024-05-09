using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class GraphClientActor : IGraphClient
{
    private readonly IClusterClient _clusterClient;
    public GraphClientActor(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option<GraphQueryResults>> Execute(string command, ScopeContext context)
    {
        return _clusterClient.GetDirectoryActor().Execute(command, context);
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string command, ScopeContext context)
    {
        var result = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items[0];
    }
}
