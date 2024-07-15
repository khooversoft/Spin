using Toolbox.Tools;

namespace TicketShare.sdk.Actors;

public static class ActorExtensions
{
    public static IAccountActor GetUserActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<IAccountActor>("*");
    public static IPartnershipActor GetPartnershipActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<IPartnershipActor>("*");
}
