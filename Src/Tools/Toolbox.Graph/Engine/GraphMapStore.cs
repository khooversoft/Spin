using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public partial class GraphMapStore
{
    private readonly IKeyStore<GraphSerialization> _graphMapStore;
    private readonly IListStore<DataChangeRecord> _journalClient;
    private readonly IKeyStore _dataFileClient;
    private readonly Transaction _transaction;
    private readonly ILogger<GraphMapStore> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private GraphMap? _map;

    public GraphMapStore(
        IKeyStore<GraphSerialization> mapSerializationClient,
        IListStore<DataChangeRecord> journalClient,
        [FromKeyedServices(GraphConstants.File.Key)] IKeyStore dataFileClient,
        [FromKeyedServices(GraphConstants.Transaction.Name)] Transaction transaction,
        ILogger<GraphMapStore> logger,
        IServiceProvider serviceProvider
        )
    {
        _graphMapStore = mapSerializationClient.NotNull();
        _journalClient = journalClient.NotNull();
        _dataFileClient = dataFileClient.NotNull();
        _transaction = transaction.NotNull();
        _logger = logger.NotNull();
        _serviceProvider = serviceProvider.NotNull();
    }

    public GraphMap GetMap() => _map.NotNull("Database has not been loaded");
    public Transaction Transaction => _transaction;
    public IKeyStore DataFileClient => _dataFileClient;

    public async Task<Option> SetMap(GraphMap map)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_map != null) return (StatusCode.Conflict, "Map already loaded");
            _map = map.NotNull().Clone();

            Option<string> setOption = await _graphMapStore.Set(GraphConstants.GraphMap.Key, _map.ToSerialization());
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
            var getOption = await _graphMapStore.Get(GraphConstants.GraphMap.Key);

            Option<GraphMap> result = getOption switch
            {
                { StatusCode: StatusCode.OK } => getOption.Return().FromSerialization(_serviceProvider).ToOption(),

                { StatusCode: StatusCode.NotFound } => await ActivatorUtilities.CreateInstance<GraphMap>(_serviceProvider).Func(async x =>
                {
                    var setOption = await _graphMapStore.Set(GraphConstants.GraphMap.Key, x.ToSerialization());
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
}
