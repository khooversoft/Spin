using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace TicketShare.sdk.Actors;

public static class ActorExtensions
{
    public static IAccountActor GetUserActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<IAccountActor>("*");
}
