using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantClient : ClientBase<TenantModel>
{
    public TenantClient(HttpClient client) : base(client, "tenant") { }
}
