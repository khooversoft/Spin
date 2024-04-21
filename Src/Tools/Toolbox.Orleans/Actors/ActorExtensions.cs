using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public static class ActorExtensions
{
    public static IDirectoryActor GetDirectory(this IClusterClient clusterClient, string resourceId = "directory")
    {
        clusterClient.NotNull();
        resourceId = resourceId.NotEmpty().ToLower().Assert(x => IdPatterns.IsName(x), "Invalid resourceId");

        return clusterClient.GetGrain<IDirectoryActor>(resourceId);
    }

    public static IFileStoreSearchActor GetFileStoreSearchActor(this IClusterClient clusterClient) =>
        clusterClient.NotNull().GetGrain<IFileStoreSearchActor>("*");

    public static IFileStoreActor GetFileStoreActor(this IClusterClient clusterClient, string path) =>
        clusterClient.NotNull().GetGrain<IFileStoreActor>(path.NotEmpty().ToLower());
}
