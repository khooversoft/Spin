using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMapManager
{
    private GraphMap? _map;
    private readonly MapPartition _mapPartition;
    private readonly IKeyStore<GraphSerialization> _graphDataClient;
    private readonly IListStore<DataChangeRecord> _changeClient;
    private readonly ILogger<GraphMapManager> _logger;
    private readonly IKeyStore<DataETag> _dataFileClient;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly IServiceProvider _serviceProvider;

    public GraphMapManager(
        IKeyStore<GraphSerialization> graphDataClient,
        IListStore<DataChangeRecord> changeClient,
        IKeyStore<DataETag> dataFileClient,
        ILogger<GraphMapManager> logger,
        IServiceProvider serviceProvider
        )
    {
        _graphDataClient = graphDataClient.NotNull();
        _changeClient = changeClient.NotNull();
        _dataFileClient = dataFileClient.NotNull();
        _logger = logger.NotNull();
        _serviceProvider = serviceProvider.NotNull();

        _mapPartition = new MapPartition(graphDataClient, _serviceProvider, _logger);
    }

    public GraphMap GetMap() => _map.NotNull("Database has not been loaded");
    public Task<string> GetSnapshot() => _mapPartition.GetSnapshot();
    public Task<Option> Recovery(IEnumerable<DataChangeRecord> records) => _mapPartition.Recovery(records);
    public string? GetLogSequenceNumber() => _map?.LastLogSequenceNumber;
    public void SetLogSequenceNumber(string? lsn) => _map?.SetLastLogSequenceNumber(lsn);
    public string StoreName => _mapPartition.StoreName;
    public async Task<Option> Checkpoint() => await _mapPartition.Checkpoint();
    public Task<Option> Restore(string json) => throw new NotImplementedException();

    public async Task<Option> SetMap(GraphMap map)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_map != null) return (StatusCode.Conflict, "Map already loaded");
            _map = map.NotNull().Clone();

            Option<string> setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization());
            return _logger.LogStatus(setOption, "Failed to initialize graph map").ToOptionStatus();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Option> LoadDatabase()
    {
        await _semaphore.WaitAsync();

        try
        {
            var getOption = await _graphDataClient.Get(GraphConstants.GraphMap.Key);

            Option<GraphMap> result = getOption switch
            {
                { StatusCode: StatusCode.OK } => getOption.Return().FromSerialization(_serviceProvider).ToOption(),

                { StatusCode: StatusCode.NotFound } => await ActivatorUtilities.CreateInstance<GraphMap>(_serviceProvider).Func(async x =>
                {
                    var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, x.ToSerialization());
                    _logger.LogStatus(setOption, "Graph DB not found, failed to create a new one").ToOption();
                    return x.ToOption();
                }),

                _ => _logger.LogStatus(getOption, "Failed to load graph map").ToOptionStatus<GraphMap>(),
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

    public Task<Option<GraphMap>> BuildFromJournals(CancellationToken token = default) => BuildFromJournals(_dataFileClient, token);

    public async Task<Option<GraphMap>> BuildFromJournals(IKeyStore<DataETag> dataFileClient, CancellationToken token = default)
    {
        _logger.LogDebug("Building new GraphMap from journals");

        var journalsOption = await _changeClient.Get(GraphConstants.Journal.Key);
        if (journalsOption.IsError()) return _logger.LogStatus(journalsOption, "Failed to read journals").ToOptionStatus<GraphMap>();

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

        var map = ActivatorUtilities.CreateInstance<GraphMap>(_serviceProvider);
        string? lastLsn = null;

        try
        {
            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                //var resultOption = entry.SourceName switch
                //{
                //    ChangeSource.Node => await NodeBuild.Build(map, entry, context),
                //    ChangeSource.Edge => await EdgeBuild.Build(map, entry, context),
                //    ChangeSource.Data => await DataBuild.Build(map, entry, dataFileClient, context),
                //    _ => throw new InvalidOperationException($"Unknown source name {entry.SourceName}"),
                //};

                //if (resultOption.IsOk())
                //{
                //    lastLsn = entry.LogSequenceNumber;
                //    continue;
                //}

                //resultOption.LogStatus(context, "Failed to recover").ThrowOnError();
            }

            if (lastLsn.IsNotEmpty())
            {
                map.SetLastLogSequenceNumber(lastLsn);
            }

            _logger.LogDebug("Completed build process, nodeCount={nodeCount}, edgeCount={edgeCount}", map.Nodes.Count, map.Edges.Count);
            return map;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build map from journals");
            return (StatusCode.BadRequest, "Failed to build map from journals");
        }
    }

    public TransactionScope StartTransaction() => new TransactionScope(Commit, Rollback, _logSequenceNumber, _logger);

    private async Task<Option> Commit(DataChangeRecord dataChangeRecord)
    {
        _map.NotNull("Database has not been loaded");
        dataChangeRecord.NotNull().Validate().ThrowOnError();

        await _semaphore.WaitAsync();
        try
        {
            var journalOption = await _changeClient.Append(GraphConstants.Journal.Key, [dataChangeRecord]);
            if (journalOption.IsError()) return _logger.LogStatus(journalOption, "Failed to append to journal file").ToOptionStatus();

            dataChangeRecord.GetLastLogSequenceNumber()?.Action(x => _map.SetLastLogSequenceNumber(x));

            var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization());
            if (setOption.IsError()) return _logger.LogStatus(setOption, "Failed to save graph map").ToOptionStatus();

            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Option> Rollback(DataChangeRecord record)
    {
        _map.NotNull("Database has not been loaded");
        record.NotNull().Validate().ThrowOnError();

        _logger.LogWarning("Starting rollback, transactionId={transactionId}, count={Count}", record.TransactionId, record.Entries.Count);

        await _semaphore.WaitAsync();
        try
        {
            // Compensate in reverse order (LIFO)
            for (int i = record.Entries.Count - 1; i >= 0; i--)
            {
                //var entry = record.Entries[i];
                //var result = entry.SourceName switch
                //{
                //    ChangeSource.Node => await NodeCompensate.Compensate(_map, entry, context),
                //    ChangeSource.Edge => await EdgeCompensate.Compensate(_map, entry, context),
                //    ChangeSource.Data => await DataCompensate.Compensate(_map, entry, _dataFileClient, context),
                //    _ => (StatusCode.BadRequest, $"Unknown source name {entry.SourceName}"),
                //};

                //// NotFound is considered idempotent; continue rollback
                //if (result.IsOk() || result.IsNotFound()) continue;

                //result.LogStatus(context, "Rollback failed to recover an entry").ThrowOnError();
            }

            _logger.LogDebug("Rollback completed for transaction {TransactionId}", record.TransactionId);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed for transaction {TransactionId}", record.TransactionId);
            return (StatusCode.BadRequest, $"Rollback failed for transaction {record.TransactionId}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
