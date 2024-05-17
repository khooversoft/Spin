//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans;

//public interface IDirectoryActorClient : IGraphClient { }

//public class DirectoryActorClient : IDirectoryActorClient
//{
//    private readonly IClusterClient _clusterClient;
//    public DirectoryActorClient(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

//    public Task<Option<GraphQueryResult>> Execute(string command, ScopeContext context)
//    {
//        return _clusterClient.GetDirectoryActor().Execute(command, context);
//    }

//    public Task<Option<GraphQueryResults>> ExecuteBatch(string command, ScopeContext context)
//    {
//        return _clusterClient.GetDirectoryActor().ExecuteBatch(command, context);
//    }
//}
