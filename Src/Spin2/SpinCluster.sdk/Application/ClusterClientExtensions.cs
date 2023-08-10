using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class ClusterClientExtensions
{
    public static T GetObjectGrain<T>(this IClusterClient actor, ObjectId objectId) where T : IGrainWithStringKey
    {
        string actorKey = objectId.NotNull().ToString().ToLower();
        return actor.GetGrain<T>(actorKey);
    }
}
