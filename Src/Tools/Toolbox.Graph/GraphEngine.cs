using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEngine : IAsyncDisposable
{
    IGraphStore FileStore { get; }
    bool ReadOnly { get; }
    bool ShareMode { get; }
    IJournalFile TransactionLog { get; }

    GraphMap Map { get; }
    Task SetMap(GraphMap map, ScopeContext context);
    Task<Option<IFileLeasedAccess>> AcquireLease(ScopeContext context);
    Task<Option> ReleaseLease(ScopeContext context);
    Task ReleaseExclusive(ScopeContext context);
    Task<Option> CheckpointMap(ScopeContext context);

}

public class GraphEngine : IGraphEngine, IAsyncDisposable
{
    private readonly GraphHostOption _hostOption;
    private readonly ILogger<GraphEngine> _logger;
    private readonly GraphMapStore _mapStore;

    public GraphEngine(
        IGraphStore fileStore,
        GraphHostOption hostOption,
        [FromKeyedServices(GraphConstants.TrxJournal.DiKeyed)] IJournalFile transactionLog,
        GraphMapStore graphMapStore,
        ILogger<GraphEngine> logger
        )
    {
        FileStore = fileStore.NotNull();
        _hostOption = hostOption.NotNull();
        TransactionLog = transactionLog.NotNull();
        _mapStore = graphMapStore.NotNull();
        _logger = logger.NotNull();
    }

    public IGraphStore FileStore { get; }
    public IJournalFile TransactionLog { get; }
    public bool ReadOnly => _hostOption.ReadOnly;
    public bool ShareMode => _hostOption.ShareMode;

    public GraphMap Map => _mapStore.GetMapReference();
    public Task SetMap(GraphMap map, ScopeContext context) => _mapStore.SetMap(map, context);
    public Task<Option> CheckpointMap(ScopeContext context) => _mapStore.CheckpointMap(context);

    public Task<Option<IFileLeasedAccess>> AcquireLease(ScopeContext context) => _mapStore.AcquireLease(context);
    public Task<Option> ReleaseLease(ScopeContext context) => _mapStore.ReleaseLease(context);
    public Task ReleaseExclusive(ScopeContext context) => _mapStore.ReleaseExclusive(context);

    public async ValueTask DisposeAsync() => await _mapStore.ReleaseExclusive(new ScopeContext(_logger));
}