using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;

namespace SpinCluster.sdk.Actors.User;

public class UserClient : ClientBase<UserModel>
{
    public UserClient(HttpClient client) : base(client, SpinConstants.Schema.User) { }
}
