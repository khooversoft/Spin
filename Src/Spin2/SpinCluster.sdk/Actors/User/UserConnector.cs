using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;

namespace SpinCluster.sdk.Actors.User;

public class UserConnector : ConnectorBase<UserModel, IUserActor>
{
    public UserConnector(IClusterClient client, ILogger<UserConnector> logger)
        : base(client, "user", logger)
    {
    }
}
