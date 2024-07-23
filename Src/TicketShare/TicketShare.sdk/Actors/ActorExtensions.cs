using Toolbox.Tools;

namespace TicketShare.sdk.Actors;

public static class ActorExtensions
{
    public static IAccountActor GetUserActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<IAccountActor>("*");
    public static ISeasonTicketsActor GetPartnershipActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<ISeasonTicketsActor>("*");
    public static ISeasonTicketsActor GetSeasonTicketsActor(this IClusterClient clusterClient) => clusterClient.NotNull().GetGrain<ISeasonTicketsActor>("*");
}
