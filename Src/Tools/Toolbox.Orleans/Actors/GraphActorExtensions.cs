using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Orleans;

public static class GraphActorExtensions
{
    public static IGraphActor GetDirectory(this IClusterClient clusterClient, string resourceId = "directory")
    {
        clusterClient.NotNull();
        resourceId = resourceId.NotEmpty().ToLower();

        return clusterClient.GetGrain<IGraphActor>(resourceId);
    }
}
