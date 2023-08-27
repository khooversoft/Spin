using SoftBank.sdk.Trx;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public static class SoftBankActivators
{
    public static ISoftBankActor GetSoftBankActor(this IClusterClient clusterClient, ResourceId accountId) => clusterClient.NotNull()
        .GetResourceGrain<ISoftBankActor>(accountId);

    public static ISoftBankTrxActor GetSoftBankTrxActor(this IClusterClient clusterClient, ResourceId accountId) => clusterClient.NotNull()
        .GetResourceGrain<ISoftBankTrxActor>(accountId);
}
