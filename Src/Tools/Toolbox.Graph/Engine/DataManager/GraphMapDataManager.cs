using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMapDataManager
{
    private readonly IKeyStore<GraphSerialization> _graphDataClient;
    private readonly IListStore<DataChangeRecord> _changeClient;
    private readonly ILogger<GraphMapDataManager> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly IKeyStore<DataETag> _dataFileClient;
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly GraphMapCounter _graphMapCounter;
    private GraphMap? _map;

    public GraphMapDataManager(
        IKeyStore<GraphSerialization> graphDataClient,
        IListStore<DataChangeRecord> changeClient,
        IKeyStore<DataETag> dataFileClient,
        GraphMapCounter graphMapCounter,
        ILogger<GraphMapDataManager> logger
        )
    {
        _graphDataClient = graphDataClient.NotNull();
        _changeClient = changeClient.NotNull();
        _dataFileClient = dataFileClient.NotNull();
        _graphMapCounter = graphMapCounter.NotNull();
        _logger = logger.NotNull();
    }

    public GraphMap GetMap() => _map.NotNull("Database has not been loaded");

    public async Task<Option> SetMap(GraphMap map, ScopeContext context)
    {
        context = context.With(_logger);

        await _semaphore.WaitAsync(context.CancellationToken);
        try
        {
            if (_map != null) return (StatusCode.Conflict, "Map already loaded");

            _graphMapCounter.Clear();
            _map = map.NotNull().Clone(_graphMapCounter);

            var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization(), context);
            return setOption.LogStatus(context, "Failed to initialize graph map").ToOptionStatus();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Option> LoadDatabase(ScopeContext context)
    {
        context = context.With(_logger);

        await _semaphore.WaitAsync(context.CancellationToken);
        try
        {
            var getOption = await _graphDataClient.Get(GraphConstants.GraphMap.Key, context);

            Option<GraphMap> result = getOption switch
            {
                { StatusCode: StatusCode.OK } => getOption.Return().FromSerialization(_graphMapCounter).ToOption(),
                { StatusCode: StatusCode.NotFound } => await new GraphMap(_graphMapCounter).Func(async x =>
                {
                    var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, x.ToSerialization(), context);
                    setOption.LogStatus(context, "Graph DB not found, failed to create a new one").ToOption();
                    return x.ToOption();
                }),
                _ => getOption.LogStatus(context, "Failed to load graph map").ToOptionStatus<GraphMap>(),
            };

            if (result.IsError()) return result.ToOptionStatus();

            _map = result.Return();
            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<Option<GraphMap>> BuildFromJournals(ScopeContext context) => BuildFromJournals(_dataFileClient, context);

    public async Task<Option<GraphMap>> BuildFromJournals(IKeyStore<DataETag> dataFileClient, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Building new GraphMap from journals");

        var journalsOption = await _changeClient.Get(GraphConstants.Journal.Key, "**/*", context);
        if (journalsOption.IsError()) return journalsOption.LogStatus(context, "Failed to read journals").ToOptionStatus<GraphMap>();

        var records = journalsOption.Return();

        // Validate all records first and compute capacity to reduce allocations
        int totalEntries = 0;
        foreach (var record in records)
        {
            record.Validate().ThrowOnError();
            totalEntries += record.Entries.Count;
        }

        var entries = new List<DataChangeEntry>(totalEntries);
        foreach (var record in records)
        {
            // Flatten all entries
            foreach (var entry in record.Entries)
            {
                entries.Add(entry);
            }
        }

        // Sort globally by LSN (ascending = oldest first)
        entries.Sort(static (a, b) => string.CompareOrdinal(a.LogSequenceNumber, b.LogSequenceNumber));

        var map = new GraphMap(_graphMapCounter);
        string? lastLsn = null;

        try
        {
            foreach (var entry in entries)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var resultOption = entry.SourceName switch
                {
                    ChangeSource.Node => await NodeBuild.Build(map, entry, context),
                    ChangeSource.Edge => await EdgeBuild.Build(map, entry, context),
                    ChangeSource.Data => await DataBuild.Build(map, entry, dataFileClient, context),
                    _ => throw new InvalidOperationException($"Unknown source name {entry.SourceName}"),
                };

                if (resultOption.IsOk())
                {
                    lastLsn = entry.LogSequenceNumber;
                    continue;
                }

                resultOption.LogStatus(context, "Failed to recover").ThrowOnError();
            }

            if (!string.IsNullOrEmpty(lastLsn))
            {
                map.SetLastLogSequenceNumber(lastLsn);
            }

            // Ensure counters reflect the built state
            map.UpdateCounters();

            context.LogDebug("Completed build process, nodeCount={nodeCount}, edgeCount={edgeCount}", map.Nodes.Count, map.Edges.Count);
            return map;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to build map from journals");
            return (StatusCode.BadRequest, "Failed to build map from journals");
        }
    }

    public TransactionScope StartTransaction() => new TransactionScope(Commit, Rollback, _logSequenceNumber, _logger);

    private async Task<Option> Commit(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        _map.NotNull("Database has not been loaded");
        dataChangeRecord.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);

        await _semaphore.WaitAsync(context.CancellationToken);
        try
        {
            var journalOption = await _changeClient.Append(GraphConstants.Journal.Key, [dataChangeRecord], context);
            if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to append to journal file").ToOptionStatus();

            dataChangeRecord.GetLastLogSequenceNumber()?.Action(x => _map.SetLastLogSequenceNumber(x));

            var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization(), context);
            if (setOption.IsError()) return setOption.LogStatus(context, "Failed to save graph map").ToOptionStatus();

            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Option> Rollback(DataChangeRecord record, ScopeContext context)
    {
        context = context.With(_logger);
        _map.NotNull("Database has not been loaded");
        record.NotNull().Validate().ThrowOnError();

        context.LogWarning("Starting rollback of transaction transactionId={transactionId} with {Count} entries", record.TransactionId, record.Entries.Count);

        await _semaphore.WaitAsync(context.CancellationToken);
        try
        {
            // Compensate in reverse order (LIFO)
            for (int i = record.Entries.Count - 1; i >= 0; i--)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var entry = record.Entries[i];
                var result = entry.SourceName switch
                {
                    ChangeSource.Node => await NodeCompensate.Compensate(_map, entry, context),
                    ChangeSource.Edge => await EdgeCompensate.Compensate(_map, entry, context),
                    ChangeSource.Data => await DataCompensate.Compensate(_map, entry, _dataFileClient, context),
                    _ => (StatusCode.BadRequest, $"Unknown source name {entry.SourceName}"),
                };

                // NotFound is considered idempotent; continue rollback
                if (result.IsOk() || result.IsNotFound()) continue;

                result.LogStatus(context, "Rollback failed to recover an entry").ThrowOnError();
            }

            context.LogDebug("Rollback completed for transaction {TransactionId}", record.TransactionId);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Rollback failed for transaction {TransactionId}", record.TransactionId);
            return (StatusCode.BadRequest, $"Rollback failed for transaction {record.TransactionId}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
