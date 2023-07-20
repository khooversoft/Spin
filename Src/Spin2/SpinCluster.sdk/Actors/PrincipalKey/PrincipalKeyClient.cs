using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalKeyClient : ClientBase<UserModel>
{
    public PrincipalKeyClient(HttpClient client) : base(client, SpinConstants.Schema.PrincipalKey) { }
}
