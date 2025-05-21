using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEngine : IAsyncDisposable
{
    GraphHostOption Options { get; }
    IGraphFileStore FileStore { get; }
    IJournalFile TransactionLog { get; }
    bool IsShareMode { get; }


    GraphMapData? GetMapData();
    void SetGraphMapData(GraphMap map, string? eTag);
    void UpdateGraphMapETag(string eTag);

    Task<Option> Start(ScopeContext context);
    Task<Option> Start(GraphMap map, ScopeContext context);
    Task<Option> Stop(ScopeContext context);
}

public class GraphEngine : IGraphEngine, IAsyncDisposable
{
    private readonly GraphHostOption _hostOption;
    private readonly ILogger<GraphEngine> _logger;
    private readonly IGraphMapAccess _graphMapAccess;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private GraphMapData? _mapData;

    public GraphEngine(
        IGraphFileStore fileStore,
        GraphHostOption hostOption,
        [FromKeyedServices(GraphConstants.TrxJournal.DiKeyed)] IJournalFile transactionLog,
        IGraphMapAccess graphMapAccess,
        ILogger<GraphEngine> logger
        )
    {
        FileStore = fileStore.NotNull();
        _hostOption = hostOption.NotNull();
        TransactionLog = transactionLog.NotNull();
        _graphMapAccess = graphMapAccess.NotNull();
        _logger = logger.NotNull();
    }

    public GraphHostOption Options => _hostOption;
    public IGraphFileStore FileStore { get; }
    public IJournalFile TransactionLog { get; }
    public bool IsShareMode => _hostOption.ShareMode;

    public GraphMapData? GetMapData() => _mapData;
    public void SetGraphMapData(GraphMap map, string? eTag) => _mapData = new GraphMapData(map, eTag);
    public void UpdateGraphMapETag(string eTag) => _mapData = new GraphMapData(_mapData.NotNull().Map, eTag);

    public async ValueTask DisposeAsync() => await _graphMapAccess.Stop(_logger.ToScopeContext());

    public Task<Option> Start(ScopeContext context) => InternalStart(null, context);

    public Task<Option> Start(GraphMap map, ScopeContext context) => InternalStart(map, context);

    public async Task<Option> Stop(ScopeContext context) => await _graphMapAccess.Stop(context).ConfigureAwait(false);


    private async Task<Option> InternalStart(GraphMap? graphMap, ScopeContext context)
    {
        await _semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            if (_mapData != null)
            {
                context.LogCritical("Graph engine already started");
                throw new InvalidOperationException("Graph engine already started");
            }

            using var metric = context.LogDuration("graphEngine-Start");
            await _graphMapAccess.Start(context).ConfigureAwait(false);

            await using IGraphMapAccessScope scope = await _graphMapAccess.CreateScope(context);

            var resultOption = graphMap switch
            {
                GraphMap map => await InternalSetMap(scope, map, context).ConfigureAwait(false),
                null => await InternalLoadMap(scope, context).ConfigureAwait(false),
            };

            var readOption = await scope.LoadDatabase(context).ConfigureAwait(false);
            if (readOption.IsError())
            {
                context.LogError("Failed to load graph database");
                return readOption;
            }

            context.LogInformation("GraphHost is running");
            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Option> InternalSetMap(IGraphMapAccessScope scope, GraphMap map, ScopeContext context)
    {
        context.LogDebug("Graph database data has been manual set and checkpoint");

        SetGraphMapData(map, null);
        var readOption = await scope.SaveDatabase(context).ConfigureAwait(false);
        if (readOption.IsError()) readOption.LogStatus(context, "Failed to save graph database");

        return readOption;
    }

    private async Task<Option> InternalLoadMap(IGraphMapAccessScope scope, ScopeContext context)
    {
        context.LogDebug("Graph database data is being loaded");

        var readOption = await scope.LoadDatabase(context).ConfigureAwait(false);
        if (readOption.IsError()) readOption.LogStatus(context, "Failed to load graph database");

        return readOption;
    }
}

public record GraphMapData
{
    public GraphMapData(GraphMap map, string? eTag)
    {
        Map = map.NotNull();
        ETag = eTag;
    }

    public GraphMap Map { get; }
    public string? ETag { get; }
}
