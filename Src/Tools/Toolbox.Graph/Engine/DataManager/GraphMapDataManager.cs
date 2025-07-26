using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMapDataManager
{
    private readonly IDataClient<GraphSerialization> _graphDataClient;
    private readonly IDataClient<DataChangeRecord> _changeClient;
    private readonly ILogger<GraphMapDataManager> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly IDataClient<DataETag> _dataFileClient;
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly GraphMapCounter _graphMapCounter;
    private GraphMap? _map;

    public GraphMapDataManager(
        IDataClient<GraphSerialization> graphDataClient,
        IDataClient<DataChangeRecord> changeClient,
        IDataClient<DataETag> dataFileClient,
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
        _map.Assert(x => x == null, "Map already loaded");

        _graphMapCounter.Clear();
        _map = map.NotNull().Clone(_graphMapCounter);

        var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization(), context);
        setOption.LogStatus(context, "Failed to initialize graph map").ToOption();

        return setOption;
    }

    public async Task<Option> LoadDatabase(ScopeContext context)
    {
        context = context.With(_logger);

        await _semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            var getOption = await _graphDataClient.Get(GraphConstants.GraphMap.Key, context);

            Option<GraphMap> result = getOption switch
            {
                { StatusCode: StatusCode.OK } => getOption.Return().FromSerialization(_graphMapCounter).ToOption(),
                { StatusCode: StatusCode.NotFound } => await new GraphMap(_graphMapCounter).Func(async x =>
                {
                    var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, x.ToSerialization(), context);
                    setOption.LogStatus(context, "Failed to initialize graph map").ToOption();

                    return x.ToOption();
                }),
                _ => getOption.LogStatus(context, "Failed to load graph map").ToOptionStatus<GraphMap>(),
            };
            if (result.IsError()) return getOption.ToOptionStatus();

            _map = result.Return();
            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Option<GraphMap>> BuildFromJournals(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Building new GraphMap from journals");

        var journalsOption = await _changeClient.GetList(GraphConstants.Journal.Key, context);
        if (journalsOption.IsError()) return journalsOption.LogStatus(context, "Failed to read journals").ToOptionStatus<GraphMap>();

        var journals = journalsOption.Return()
            .ForEach(x => x.Validate().ThrowOnError())
            .SelectMany(x => x.Entries)
            .OrderBy(x => x.LogSequenceNumber)
            .ToArray();

        var stack = journals.Reverse().ToStack();
        var map = new GraphMap();

        Func<DataChangeEntry, Task<Option>>[] _exeStack = [
            x => NodeBuild.Build(map, x, context),
            x => EdgeBuild.Build(map, x, context),
            x => DataBuild.Build(map, x, _dataFileClient, context),
            ];

        try
        {
            while (stack.TryPop(out var entry))
            {
                foreach (var item in _exeStack)
                {
                    var resultOption = await item(entry);
                    if (resultOption.IsOk()) break;
                    if (resultOption.IsNotFound()) continue;

                    resultOption.LogStatus(context, "Failed to recover").ThrowOnError();
                }
            }

            context.LogDebug("Compiled build process, nodeCount={nodeCount}, edgeCount={edgeCount}", map.Nodes.Count, map.Edges.Count);
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
        context = _logger.ToScopeContext();

        await _semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            var journalOption = await _changeClient.AppendList(GraphConstants.Journal.Key, [dataChangeRecord], context);
            if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to load graph map");

            dataChangeRecord.GetLastLogSequenceNumber()?.Action(x => _map.SetLastLogSequenceNumber(x));

            var setOption = await _graphDataClient.Set(GraphConstants.GraphMap.Key, _map.ToSerialization(), context);
            if (setOption.IsError()) return setOption.LogStatus(context, "Failed to load graph map");

            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Option> Rollback(DataChangeRecord record, ScopeContext context)
    {
        _logger.LogWarning("Rolling back changes for transaction {TransactionId}", record.TransactionId);
        _map.NotNull("Database has not been loaded");

        var stack = record.Entries.Reverse().ToStack();

        Func<DataChangeEntry, Task<Option>>[] _exeStack = [
            x => NodeCompensate.Compensate(_map, x, context),
            x => EdgeCompensate.Compensate(_map, x, context),
            x => DataCompensate.Compensate(_map, x, _dataFileClient, context),
            ];

        context.LogDebug("Starting rollback of transaction transactionId={transactionId} with {Count} entries", record.TransactionId, record.Entries.Count);

        try
        {
            while (stack.TryPop(out var entry))
            {
                foreach (var item in _exeStack)
                {
                    var resultOption = await item(entry);
                    if (resultOption.IsOk()) break;
                    if (resultOption.IsNotFound()) continue;

                    resultOption.LogStatus(context, "Failed to recover").ThrowOnError();
                }
            }

            context.LogDebug("Rollback completed for transaction {TransactionId}", record.TransactionId);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Rollback failed for transaction {TransactionId}", record.TransactionId);
            throw new InvalidOperationException($"Rollback failed for transaction {record.TransactionId}", ex);
        }
    }
}
