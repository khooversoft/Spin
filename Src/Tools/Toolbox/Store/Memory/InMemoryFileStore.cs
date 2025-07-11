using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public sealed class InMemoryFileStore : IFileStore
{
    private readonly ILogger<InMemoryFileStore> _logger;
    private readonly MemoryStore _memoryStore;

    public InMemoryFileStore(MemoryStore memoryStore, ILogger<InMemoryFileStore> logger)
    {
        _logger = logger.NotNull();
        _memoryStore = memoryStore.NotNull();
    }

    public Task<Option> DeleteFolder(string path, ScopeContext context) => _memoryStore.DeleteFolder(path, context).ToTaskResult();

    public IFileAccess File(string path) => new InMemoryFileAccess(path, _memoryStore, _logger);

    public Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context) => _memoryStore.Search(pattern).ToTaskResult();
}
