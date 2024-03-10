using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityActorConnector : IIdentityClient
{
    private readonly IClusterClient _clusterClient;
    public IdentityActorConnector(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option> Clear(string principalId, string traceId)
    {
        var actor = _clusterClient.GetGrain<IIdentityActor>(ToolboxIdentityConstants.DirectoryActorKey);
        return actor.Clear(principalId, traceId);
    }

    public Task<Option<GraphQueryResults>> Execute(string command, string traceId)
    {
        var actor = _clusterClient.GetGrain<IIdentityActor>(ToolboxIdentityConstants.DirectoryActorKey);
        return actor.Execute(command, traceId);
    }
}
