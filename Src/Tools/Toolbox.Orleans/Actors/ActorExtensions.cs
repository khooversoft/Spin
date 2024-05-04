using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public static class ActorExtensions
{
    //public static IDirectoryActor GetDirectoryActor(this IClusterClient clusterClient, string resourceId = "system/directory")
    //{
    //    clusterClient.NotNull();
    //    resourceId = resourceId.NotEmpty().ToLower().Assert(x => IdPatterns.IsPath(x), "Invalid path");

    //    return clusterClient.GetGrain<IDirectoryActor>(resourceId);
    //}

    //public static IDirectoryStoreActor GetDirectoryStoreActor(this IClusterClient clusterClient, string resourceId = "system/directory") =>
    //    clusterClient.NotNull().GetGrain<IDirectoryStoreActor>(resourceId.NotEmpty().ToLower());

    public static IFileStoreSearchActor GetFileStoreSearchActor(this IClusterClient clusterClient) =>
        clusterClient.NotNull().GetGrain<IFileStoreSearchActor>("*");

    public static IFileStoreActor GetFileStoreActor(this IClusterClient clusterClient, string path) =>
        clusterClient.NotNull().GetGrain<IFileStoreActor>(path.NotEmpty().ToLower());
}
