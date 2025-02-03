using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Journal;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphHost
{
    GraphMap Map { get; }
    IGraphStore FileStore { get; }
    IJournalFile TransactionLog { get; }
    IJournalFile TraceLog { get; }
    ILogger Logger { get; }
    bool ReadOnly { get; }
    Task<Option> CheckpointMap(ScopeContext context);
    Task<Option> LoadMap(ScopeContext context);
    Task<Option> Run(ScopeContext context);
    void SetMap(GraphMap map);
}

public class GraphHost : IGraphHost
{
    private readonly GraphMapStore _mapStore;
    private GraphMap _map = new GraphMap();
    private readonly GraphHostOption _hostOption;
    private int _runningState = Stopped;
    private const int Stopped = 0;
    private const int Running = 1;

    public GraphHost(
        IGraphStore fileStore,
        [FromKeyedServices(GraphConstants.TrxJournal.DiKeyed)] IJournalFile transactionLog,
        [FromKeyedServices(GraphConstants.Trace.DiKeyed)] IJournalFile traceLog,
        ILogger<GraphHost> logger
        )
    {
        FileStore = fileStore.NotNull();
        TransactionLog = transactionLog.NotNull();
        TraceLog = traceLog.NotNull();
        Logger = logger.NotNull();

        _mapStore = new GraphMapStore(this, Logger);
        _hostOption = new GraphHostOption();
    }

    public GraphHost(
        IGraphStore fileStore,
        GraphHostOption hostOption,
        [FromKeyedServices(GraphConstants.TrxJournal.DiKeyed)] IJournalFile transactionLog,
        [FromKeyedServices(GraphConstants.Trace.DiKeyed)] IJournalFile traceLog,
        ILogger<GraphHost> logger
        )
    {
        FileStore = fileStore.NotNull();
        TransactionLog = transactionLog.NotNull();
        TraceLog = traceLog.NotNull();
        Logger = logger.NotNull();

        _mapStore = new GraphMapStore(this, Logger);
        _hostOption = hostOption;
    }

    public GraphMap Map => _map;
    public IGraphStore FileStore { get; }
    public IJournalFile TransactionLog { get; }
    public IJournalFile TraceLog { get; }
    public bool ReadOnly => _hostOption.ReadOnly;
    public ILogger Logger { get; }

    public Task<Option> CheckpointMap(ScopeContext context) => _mapStore.Set(context);
    public Task<Option> LoadMap(ScopeContext context) => _mapStore.Get(context);

    public async Task<Option> Run(ScopeContext context)
    {
        context = context.With(Logger);
        int current = Interlocked.CompareExchange(ref _runningState, Running, Stopped);
        if (current == Running) return StatusCode.OK;

        Option result;
        using (var metric = context.LogDuration("graphHost-loadMap"))
        {
            result = await LoadMap(context).ConfigureAwait(false);
        }

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
