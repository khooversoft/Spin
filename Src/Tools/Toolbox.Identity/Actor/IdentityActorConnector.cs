using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityActorConnector : IIdentityClient
{
    private readonly IClusterClient _clusterClient;
    private readonly string _resourceId;

    public IdentityActorConnector(string resourceId, IClusterClient clusterClient)
    {
        _resourceId = resourceId.NotEmpty().ToLower();
        _clusterClient = clusterClient.NotNull();
    }

    //public Task<Option> Clear(string traceId)
    //{
    //    var actor = _clusterClient.GetGrain<IGraphActor>(_resourceId);
    //    return actor.Clear(traceId);
    //}

    public Task<Option<GraphQueryResults>> Execute(string command, string traceId)
    {
        var actor = _clusterClient.GetGrain<IDirectoryActor>(_resourceId);
        return actor.Execute(command, new ScopeContext(traceId, NullLogger.Instance));
    }

    public IPrincipalIdentityActor GetPrincipalIdentityActor(string principalId) => _clusterClient.GetGrain<IPrincipalIdentityActor>(principalId.ToLower());
}
