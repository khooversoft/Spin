using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class MapPartition
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyStore<GraphSerialization> _keyStore;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
    private GraphMap? _map;
    private string? _logSequenceNumber;

    public MapPartition(IKeyStore<GraphSerialization> keyStore, IServiceProvider serviceProvider, ILogger logger, GraphMap? map = null)
    {
        _keyStore = keyStore.NotNull();
        _logger = logger.NotNull();
        _serviceProvider = serviceProvider.NotNull();

        _map = map;
    }

    public string StoreName => "graphMapPartition";
    public GraphMap Map => _map ?? throw new InvalidOperationException("MapPartition not loaded");
    public string? GetLogSequenceNumber() => _logSequenceNumber;
    public void SetLogSequenceNumber(string lsn) => Interlocked.Exchange(ref _logSequenceNumber, lsn.NotEmpty());

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

    public async Task<Option<string>> Save()
    {
        _map.NotNull("MapPartition not loaded");

        await _gate.WaitAsync();

        try
        {
            GraphSerialization data = _map.ToSerialization();
            var setOption = await _keyStore.Set(GraphConstants.GraphMap.Key, data);
            if (setOption.IsError()) _logger.LogError("Failed to set map partition data from key={key}", GraphConstants.GraphMap.Key);

            return setOption;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task<string> GetSnapshot()
    {
        _map.NotNull();

        var json = _map.ToSerialization().ToJson();
        return json.ToTaskResult();
    }

    public Task<Option> Checkpoint() => new Option(StatusCode.OK).ToTaskResult();

    public async Task<Option> Recovery(IEnumerable<DataChangeRecord> records)
    {
        _map.NotNull();
        throw new NotImplementedException();

        //await _gate.WaitAsync();

        //try
        //{
        //    var gs = json.ToObject<GraphSerialization>().NotNull();
        //    gs.Validate().ThrowOnError();

        //    Interlocked.Exchange(ref _map, gs.FromSerialization(_serviceProvider));

        //    return StatusCode.OK;
        //}
        //finally
        //{
        //    _gate.Release();
        //}
    }

    public Task<Option> Restore(string json)
    {
        throw new NotImplementedException();
    }
}
