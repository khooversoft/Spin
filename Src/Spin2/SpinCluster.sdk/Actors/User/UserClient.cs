using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;

namespace SpinCluster.sdk.Actors.User;

public class UserClient : ClientBase<TenantModel>
{
    public UserClient(HttpClient client) : base(client, "user") { }
}
