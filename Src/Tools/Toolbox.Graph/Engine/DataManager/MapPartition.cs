using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class MapPartition
{
    private readonly IServiceProvider _serviceProvider;
    private GraphMap? _map;
    private readonly IKeyStore<GraphSerialization> _keyStore;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    public MapPartition(IKeyStore<GraphSerialization> keyStore, IServiceProvider serviceProvider, ILogger logger, GraphMap? map = null)
    {
        _keyStore = keyStore.NotNull();
        _logger = logger.NotNull();
        _serviceProvider = serviceProvider.NotNull();

        _map = map;
    }

    public GraphMap Map => _map ?? throw new InvalidOperationException("MapPartition not loaded");

    public async Task<Option> Load()
    {
        await _gate.WaitAsync();

        try
        {
            var getOption = await _keyStore.Get(GraphConstants.GraphMap.Key);
            if (getOption.IsError()) _logger.LogError("Failed to get map partition data from key={key}", GraphConstants.GraphMap.Key);

            var loadData = getOption.Return();
            loadData.Validate().ThrowOnError();

            _map = loadData.FromSerialization(_serviceProvider);
            return StatusCode.OK;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Option<string>> Save(GraphSerialization data)
    {
        _map.NotNull("MapPartition not loaded");

        await _gate.WaitAsync();

        try
        {
            var setOption = await _keyStore.Set(GraphConstants.GraphMap.Key, data);
            if (setOption.IsError()) _logger.LogError("Failed to set map partition data from key={key}", GraphConstants.GraphMap.Key);

            return setOption;
        }
        finally
        {
            _gate.Release();
        }
    }
}
