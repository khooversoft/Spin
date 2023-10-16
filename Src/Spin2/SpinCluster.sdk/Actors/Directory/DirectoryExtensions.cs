﻿using SpinCluster.sdk.Application;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

public static class DirectoryExtensions
{
    public static IDirectoryActor GetDirectoryActor(this IClusterClient client) =>
        client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey);
}
