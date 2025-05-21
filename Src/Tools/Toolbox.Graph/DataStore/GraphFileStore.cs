using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphFileStore : IFileStore
{
}


public class GraphFileStore : IGraphFileStore
{
    private readonly MemoryCacheAccess _memoryCache;
    private readonly ILogger<GraphFileStore> _logger;
    private readonly IFileStore _fileStore;

    public GraphFileStore(IFileStore fileStore, GraphHostOption hostOption, ILogger<GraphFileStore> logger)
    {
        hostOption.NotNull();

        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        _memoryCache = new MemoryCacheAccess(new NullMemoryCache());
    }

    public GraphFileStore(IFileStore fileStore, MemoryCacheAccess memoryAccess, GraphHostOption hostOption, ILogger<GraphFileStore> logger)
    {
        hostOption.NotNull();

        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        _memoryCache = (hostOption.DisableCache || hostOption.ShareMode) switch
        {
            false => memoryAccess.NotNull(),
            true => new MemoryCacheAccess(new NullMemoryCache()),
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
