using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class ActorActivators
{
    public static ISignatureActor GetSignatureActor(this IClusterClient clusterClient) => clusterClient.NotNull()
        .GetGrain<ISignatureActor>(SpinConstants.SignValidation);

    public static IUserActor GetUserActor(this IClusterClient clusterClient, PrincipalId principal) => clusterClient.NotNull()
        .GetObjectGrain<IUserActor>(IdTool.CreateUserId(principal));

    public static IPrincipalKeyActor GetPublicKeyActor(this IClusterClient clusterClient, KeyId keyId) => clusterClient.NotNull()
        .GetObjectGrain<IPrincipalKeyActor>(IdTool.CreatePublicKeyObjectId(keyId));

    public static IPrincipalPrivateKeyActor GetPrivateKeyActor(this IClusterClient clusterClient, ObjectId objectId) => clusterClient.NotNull()
        .GetObjectGrain<IPrincipalPrivateKeyActor>(objectId);

    public static IPrincipalPrivateKeyActor GetPrivateKeyActor(this IClusterClient clusterClient, KeyId keyId)
    {
        clusterClient.NotNull();
        keyId.NotNull();

        ObjectId objectId = IdTool.CreatePrivateKeyObjectId(keyId);
        return clusterClient.GetObjectGrain<IPrincipalPrivateKeyActor>(objectId);
    }
}
