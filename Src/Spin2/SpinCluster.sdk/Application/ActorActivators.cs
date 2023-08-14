using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Tools;

namespace SpinCluster.sdk.Application;

public static class ActorActivators
{
    public static ISignatureActor GetSignatureActor(this IClusterClient clusterClient) => clusterClient
        .NotNull()
        .GetObjectGrain<ISignatureActor>(SpinConstants.SignValidation);
}
