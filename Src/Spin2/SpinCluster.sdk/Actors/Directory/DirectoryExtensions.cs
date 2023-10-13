using SpinCluster.sdk.Application;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

public static class DirectoryExtensions
{
    public static IDirectoryActor GetDirectoryActor(this IClusterClient client) => client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey);

    public static Task<Option> AddEdge(this IClusterClient client, DirectoryEdge edge, string traceId) => client.GetDirectoryActor().AddEdge(edge, traceId);

    public static Task<Option> AddNode(this IClusterClient client, DirectoryNode node, string traceId) => client.GetDirectoryActor().AddNode(node, traceId);
    public static Task<Option<DirectoryResponse>> Lookup(this IClusterClient client, DirectorySearch search, string traceId) => client.GetDirectoryActor().Lookup(search, traceId);
    public static Task<Option> RemoveEdge(this IClusterClient client, string nodeKey, string traceId) => client.GetDirectoryActor().RemoveEdge(nodeKey, traceId);
    public static Task<Option> RemoveEdge(this IClusterClient client, DirectoryEdge edge, string traceId) => client.GetDirectoryActor().RemoveEdge(edge, traceId);
    public static Task<Option> RemoveNode(this IClusterClient client, string nodeKey, string traceId) => client.GetDirectoryActor().RemoveNode(nodeKey, traceId);
}
