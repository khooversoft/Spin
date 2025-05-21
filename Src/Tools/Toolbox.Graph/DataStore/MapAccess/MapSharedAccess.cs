using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class MapSharedAccess : IGraphMapAccess
{
    private readonly ILogger<MapSharedAccess> _logger;
    private readonly IGraphFileStore _graphFileStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly GraphMapCounter _mapCounters;

    public MapSharedAccess(IGraphFileStore graphFileStore, GraphMapCounter mapCounters, IServiceProvider serviceProvider, ILogger<MapSharedAccess> logger)
    {
        _graphFileStore = graphFileStore.NotNull();
        _mapCounters = mapCounters.NotNull();
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public bool IsShareMode => true;

    public async Task<IGraphMapAccessScope> CreateScope(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Acquiring scope lease and creating scope");

        var leaseOption = await _graphFileStore.File(GraphConstants.MapDatabasePath).Acquire(TimeSpan.FromSeconds(60), context);
        if (leaseOption.IsError())
        {
            context.LogCritical("Failed to acquire shared lease, error={error}", leaseOption.Error);
            throw new InvalidOperationException($"Failed to acquire shared lease, error={leaseOption.Error}");
        }

        _mapCounters.Leases.ActiveAcquire.Record(1);
        _mapCounters.Leases.Acquire.Add();
        IFileLeasedAccess fileAccess = leaseOption.Return();
        var scope = ActivatorUtilities.CreateInstance<MapScopeAccess>(_serviceProvider, fileAccess, () => ReleaseInternal(fileAccess, context));

        context.LogWarning("Shared lease acquired, Loading database file for leaseId={leaseId}", scope.LeaseId);
        var loadDatabaseOption = await scope.LoadDatabase(context);
        if (loadDatabaseOption.IsError())
        {
            context.LogCritical("Failed to load database, error={error}, leaseId={leaseId}", loadDatabaseOption.Error, scope.LeaseId);
            throw new InvalidOperationException($"Failed to load database, error={loadDatabaseOption.Error}");
        }

        return scope;

    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task<Option> Start(ScopeContext context)
    {
        context.LogWarning("Starting shared access - nothing to do");
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Stop(ScopeContext context)
    {
        context.LogWarning("Stopping shared access - nothing to do");
        return new Option(StatusCode.OK).ToTaskResult();
    }

    private async Task ReleaseInternal(IFileLeasedAccess fileAccess, ScopeContext context)
    {
        fileAccess.GetLeaseId().NotEmpty("LeaseId is empty");
        context.LogWarning("Releasing shared lease, leaseId={leaseId}", fileAccess.LeaseId);

        var releaseOption = await fileAccess.Release(context);
        if (releaseOption.IsError())
        {
            context.LogWarning("Failed to release shared lease, error={error}", releaseOption.Error);
            throw new InvalidOperationException($"Failed to release shared lease, error={releaseOption.Error}");
        }

        _mapCounters.Leases.ActiveAcquire.Record(0);
        _mapCounters.Leases.Release.Add();
    }
}
