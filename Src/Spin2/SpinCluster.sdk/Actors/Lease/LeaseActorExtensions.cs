using SpinCluster.sdk.Application;

namespace SpinCluster.sdk.Actors.Lease;

public static class LeaseActorExtensions
{
    public static ILeaseActor GetLeaseActor(this IClusterClient clusterClient) => clusterClient
        .GetResourceGrain<ILeaseActor>(SpinConstants
        .LeaseActorKey);
}
