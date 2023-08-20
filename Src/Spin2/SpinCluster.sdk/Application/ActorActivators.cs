using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class ActorActivators
{
    public static ISubscriptionActor GetSubscriptionActor(this IClusterClient clusterClient, ResourceId subscriptionId) => clusterClient.NotNull()
        .GetResourceGrain<ISubscriptionActor>(subscriptionId);

    public static ITenantActor GetTenantActor(this IClusterClient clusterClient, ResourceId resourceId) => clusterClient.NotNull()
        .GetResourceGrain<ITenantActor>(resourceId);

    public static ISignatureActor GetSignatureActor(this IClusterClient clusterClient) => clusterClient.NotNull()
        .GetGrain<ISignatureActor>(SpinConstants.SignValidation);

    public static IUserActor GetUserActor(this IClusterClient clusterClient, ResourceId principalId) => clusterClient.NotNull()
        .GetResourceGrain<IUserActor>(principalId);

    public static IPrincipalKeyActor GetPublicKeyActor(this IClusterClient clusterClient, ResourceId keyId) => clusterClient.NotNull()
        .GetResourceGrain<IPrincipalKeyActor>(keyId);

    public static IPrincipalPrivateKeyActor GetPrivateKeyActor(this IClusterClient clusterClient, ResourceId keyId) => clusterClient.NotNull()
        .GetResourceGrain<IPrincipalPrivateKeyActor>(keyId);
}
