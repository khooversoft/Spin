using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantConnector : ConnectorBase<TenantModel, ITenantActor>
{
    public TenantConnector(IClusterClient client, ILogger<TenantConnector> logger)
        : base(client, "tenant", logger)
    {
    }
}
