using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class MapScopeAccess : IGraphMapAccessScope, IAsyncDisposable
{
    private readonly IFileReadWriteAccess _readWriteAccess;
    private readonly Func<Task> _releaseFunc;
    private readonly IGraphEngine _graphEngine;
    private readonly IGraphMapFactory _graphMapFactory;
    private readonly ILogger<MapScopeAccess> _logger;

    public MapScopeAccess(IGraphEngine graphEngine, IFileReadWriteAccess readWriteAccess, IGraphMapFactory graphMapFactory, Func<Task> releaseFunc, ILogger<MapScopeAccess> logger)
    {
        _graphEngine = graphEngine.NotNull();
        _readWriteAccess = readWriteAccess.NotNull();
        _graphMapFactory = graphMapFactory.NotNull();
        _releaseFunc = releaseFunc.NotNull();
        _logger = logger.NotNull();
    }

    public string LeaseId => _readWriteAccess.GetLeaseId().NotEmpty();

    public async ValueTask DisposeAsync() => await _releaseFunc().ConfigureAwait(false);

    public async Task<Option> LoadDatabase(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Loading database, leaseId={leaseId}", _readWriteAccess.GetLeaseId());

        return await _graphEngine.LoadDatabase(_readWriteAccess, _graphMapFactory, context).ConfigureAwait(false);
    }

    public async Task<Option> SaveDatabase(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Saving database, leaseId={leaseId}", _readWriteAccess.GetLeaseId());

        return await _graphEngine.SaveDatabase(_readWriteAccess, context).ConfigureAwait(false);
    }
}
