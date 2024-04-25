using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class FileStoreActorConnector : IFileStore
{
    private readonly IClusterClient _clusterClient;
    public FileStoreActorConnector(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Add(data, context);
    }

    public Task<Option> Delete(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Delete(context);
    }

    public Task<Option> Exist(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Exist(context);
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Get(context);
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        return _clusterClient.GetFileStoreSearchActor().Search(pattern, context);
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Set(data, context);
    }
}
