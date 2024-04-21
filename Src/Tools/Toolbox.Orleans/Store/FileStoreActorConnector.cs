using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.Store;

public class FileStoreActorConnector : IFileStore
{
    private readonly IClusterClient _clusterClient;
    public FileStoreActorConnector(IClusterClient clusterClient) => _clusterClient = clusterClient.NotNull();

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Add(data, context.TraceId);
    }

    public Task<Option> Delete(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Delete(context.TraceId);
    }

    public Task<Option> Exist(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Exist(context.TraceId);
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Get(context.TraceId);
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        return _clusterClient.GetFileStoreSearchActor().Search(pattern, context.TraceId);
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        return _clusterClient.GetFileStoreActor(path).Set(data, context.TraceId);
    }
}
