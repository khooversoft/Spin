using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class DirectoryStoreActorConnector : IGraphStore
{
    private readonly IClusterClient _clusterClient;
    public DirectoryStoreActorConnector(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option<string>> Add(string nodeKey, string name, DataETag data, ScopeContext context) => _clusterClient
        .GetDirectoryStoreActor(nodeKey)
        .Add(nodeKey, name, data, context);

    public Task<Option> Delete(string nodeKey, string name, ScopeContext context) => _clusterClient
        .GetDirectoryStoreActor(nodeKey)
        .Delete(nodeKey, name, context);

    public Task<Option> Exist(string nodeKey, string name, ScopeContext context) => _clusterClient
        .GetDirectoryStoreActor(nodeKey)
        .Exist(nodeKey, name, context);

    public Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context) => _clusterClient
        .GetDirectoryStoreActor(nodeKey)
        .Get(nodeKey, name, context);

    public Task<Option<string>> Set(string nodeKey, string name, DataETag data, ScopeContext context) => _clusterClient
        .GetDirectoryStoreActor(nodeKey)
        .Set(nodeKey, name, data, context);
}
