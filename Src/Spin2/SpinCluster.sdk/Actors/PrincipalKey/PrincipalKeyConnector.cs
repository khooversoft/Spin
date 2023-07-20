using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalKeyConnector : ConnectorBase<PrincipalKeyModel, IPrincipalKeyActor>
{
    public PrincipalKeyConnector(IClusterClient client, ILogger<PrincipalKeyConnector> logger)
        : base(client, SpinConstants.Schema.PrincipalKey, logger)
    {
    }
}