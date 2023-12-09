using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class ClusterClientExtensions
{
    public static T GetResourceGrain<T>(this IClusterClient actor, ResourceId resourceId) where T : IGrainWithStringKey
    {
        actor.NotNull();
        resourceId.Schema.NotEmpty("required");

        string actorKey = resourceId.NotNull().ToString().ToLower();
        return actor.GetGrain<T>(actorKey);
    }
}
