using Microsoft.Extensions.Caching.Memory;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphFileStoreCache : IGraphStore
{
    private readonly IFileStore _fileStore;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public GraphFileStoreCache(IFileStore fileStore, IMemoryCache memoryCache)
    {
        _fileStore = fileStore.NotNull();
        _memoryCache = memoryCache.NotNull();
    }

    public async Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        var result = await _fileStore.Add(path, data, context);
        if (result.IsError()) return result;

        if (IsNodeData(path))
        {
            _memoryCache.Set(path, data, _memoryOptions);
        }

        return result;
    }

    public Task<Option> Append(string path, DataETag data, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public async Task<Option> Delete(string path, ScopeContext context)
    {
        var result = await _fileStore.Delete(path, context);
        if (result.IsError()) return result;

        if (IsNodeData(path))
        {
            _memoryCache.Remove(path);
        }

        return result;
    }

    public Task<Option> Exist(string path, ScopeContext context) => _fileStore.Exist(path, context);

    public async Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        if (IsNodeData(path) && _memoryCache.TryGetValue(path, out DataETag data))
        {
            return data;
        }

        var result = await _fileStore.Get(path, context);
        return result;
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context) => _fileStore.Search(pattern, context);

    public async Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        var result = await _fileStore.Set(path, data, context);
        if (result.IsError()) return result;

        if (IsNodeData(path))
        {
            _memoryCache.Set(path, data, _memoryOptions);
        }

        return result;
    }

    private bool IsNodeData(string path) => path.NotEmpty().StartsWith(GraphConstants.NodesDataBasePath + '/');
}
