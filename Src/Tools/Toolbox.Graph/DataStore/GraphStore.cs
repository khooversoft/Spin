using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphStore : IFileStore
{
}


public class GraphStore : IGraphStore
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GraphStore> _logger;
    private readonly IFileStore _fileStore;

    public GraphStore(IFileStore fileStore, IMemoryCache memoryCache, GraphHostOption hostOption, ILogger<GraphStore> logger)
    {
        hostOption.NotNull();

        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        _memoryCache = hostOption.DisableCache switch
        {
            false => memoryCache.NotNull(),
            true => new NullMemoryCache(),
        };
    }

    public Task<Option> DeleteFolder(string path, ScopeContext context)
    {
        _memoryCache.Remove(path);
        return _fileStore.DeleteFolder(path, context);
    }

    public IFileAccess File(string path) => new GraphStoreFileAccess(_fileStore.File(path), _memoryCache, _logger);
    public Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context) => _fileStore.Search(pattern, context);
}
