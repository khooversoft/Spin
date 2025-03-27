using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public sealed class InMemoryFileStore : IFileStore
{
    private readonly ILogger<InMemoryFileStore> _logger;
    private readonly InMemoryStoreControl _storeControl;

    public InMemoryFileStore(ILogger<InMemoryFileStore> logger)
    {
        _logger = logger.NotNull();
        _storeControl = new InMemoryStoreControl(_logger);
    }

    public int Count => _storeControl.Count;
    public IFileAccess File(string path) => new InMemoryFileAccess(path, _storeControl, _logger);
    public IFileLeasedAccess Lease(string path) => new InMemoryStoreLeaseControl(path, _storeControl, _logger);

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context) => _storeControl.Search(pattern, context.With(_logger));
    public Task<IReadOnlyList<IStorePathDetail>> DetailSearch(string pattern, ScopeContext context) => _storeControl.DetailSearch(pattern, context.With(_logger));
    public Task<Option> DeleteFolder(string path, ScopeContext context) => _storeControl.DeleteFolder(path, context.With(_logger));
}
