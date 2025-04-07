using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphHost
{
    Task<Option> Run(ScopeContext context);
    Task<Option> Run(GraphMap map, ScopeContext context);
}

public class GraphHost : IGraphHost
{
    private int _runningState = Stopped;
    private const int Stopped = 0;
    private const int Running = 1;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly IGraphEngine _graphHostEngine;
    private readonly ILogger<GraphHost> _logger;

    public GraphHost(IGraphEngine graphHostEngine, ILogger<GraphHost> logger)
    {
        _graphHostEngine = graphHostEngine.NotNull();
        _logger = logger;
    }

    public Task<Option> Run(ScopeContext context) => InternalRun(null, context);

    public Task<Option> Run(GraphMap map, ScopeContext context) => InternalRun(map, context);


    private async Task<Option> InternalRun(GraphMap? map, ScopeContext context)
    {
        context = context.With(_logger);

        await _semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            int current = Interlocked.CompareExchange(ref _runningState, Running, Stopped);
            if (current == Running) return (StatusCode.Conflict, "GraphHost is already running");

            using var metric = context.LogDuration("graphHost-loadMap");

            if (map != null) _graphHostEngine.SetMap(map.NotNull());
            var loadOption = await _graphHostEngine.InitializeDatabase(context);
            if (loadOption.IsError()) return loadOption;

            var exclusiveLease = await _graphHostEngine.AcquireExclusive(context);
            if (exclusiveLease.IsError()) return exclusiveLease.LogStatus(context, "Failed to acquire exclusive lease and load graph map");

            context.LogTrace("Acquired exclusive lease for map database");


            Interlocked.Exchange(ref _runningState, Running);
            context.LogInformation("GraphHost is running");
            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
