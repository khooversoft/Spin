using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphHost
{
    GraphMap Map { get; }
    IGraphStore FileStore { get; }
    ITransactionLog TransactionLog { get; }
    void SetMap(GraphMap map);
    Task<Option> LoadMap(ScopeContext context);
    Task<Option> CheckpointMap(ScopeContext context);
}

public class GraphHost : IGraphHost
{
    private readonly GraphMapStore _mapStore;
    private readonly ILogger<GraphHost> _logger;
    private GraphMap _map = new GraphMap();
    public GraphHost(IGraphStore fileStore, ITransactionLog transactionLog, ILogger<GraphHost> logger)
    {
        FileStore = fileStore.NotNull();
        TransactionLog = transactionLog.NotNull();
        _logger = logger.NotNull();

        _mapStore = new GraphMapStore(this, _logger);
    }

    public GraphMap Map => _map;
    public IGraphStore FileStore { get; }
    public ITransactionLog TransactionLog { get; }

    public void SetMap(GraphMap map) => Interlocked.Exchange(ref _map, map.NotNull());
    public Task<Option> LoadMap(ScopeContext context) => _mapStore.Get(context);
    public Task<Option> CheckpointMap(ScopeContext context) => _mapStore.Set(context);
}
