using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphHost
{
    GraphMap Map { get; }
    IGraphStore FileStore { get; }
    ITransactionLog TransactionLog { get; }
    Task<Option> CheckpointMap(ScopeContext context);
    Task<Option> LoadMap(ScopeContext context);
    Task<Option> Run(ScopeContext context);
    void SetMap(GraphMap map);
}

public class GraphHost : IGraphHost
{
    private readonly Guid _instance = Guid.NewGuid();
    private readonly GraphMapStore _mapStore;
    private readonly ILogger<GraphHost> _logger;
    private GraphMap _map = new GraphMap();
    private int _runningState = Stopped;
    private const int Stopped = 0;
    private const int Running = 1;

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

    public Task<Option> CheckpointMap(ScopeContext context) => _mapStore.Set(context);
    public Task<Option> LoadMap(ScopeContext context) => _mapStore.Get(context);

    public async Task<Option> Run(ScopeContext context)
    {
        context = context.With(_logger);
        int current = Interlocked.CompareExchange(ref _runningState, Running, Stopped);
        if (current == Running) return StatusCode.OK;

        var result = await LoadMap(context);
        Interlocked.Exchange(ref _runningState, Running);
        result.LogStatus(context, "Host started and map loaded");
        return result;
    }

    public void SetMap(GraphMap map)
    {
        Interlocked.Exchange(ref _runningState, Running);
        Interlocked.Exchange(ref _map, map.NotNull());
    }
}
