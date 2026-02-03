using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class SequenceSpace<T> : ISequenceStore<T>
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<SequenceSpace<T>> _logger;
    private readonly SequenceKeySystem<T> _keySystem;
    private readonly ITelemetryCounter<long>? _addCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private readonly ITelemetryCounter<long>? _getCounter;
    private SequenceSizeLimit<T>? _limiter;

    public SequenceSpace(IKeyStore keyStore, SequenceKeySystem<T> keySystem, ILogger<SequenceSpace<T>> logger, IServiceProvider serviceProvider, ITelemetry? telemetry = null)
    {
        _keyStore = keyStore.NotNull();
        _keySystem = keySystem.NotNull();
        _logger = logger.NotNull();

        _addCounter = telemetry?.CreateCounter<long>("sequenceSpace.add", "Number of Add operations", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("sequenceSpace.delete", "Number of Delete operations", unit: "count");
        _getCounter = telemetry?.CreateCounter<long>("sequenceSpace.get", "Number of Get operations", unit: "count");
    }

    public SequenceKeySystem<T> SequenceKeySystem => _keySystem;

    internal void SetLimiter(SequenceSizeLimit<T> limiter) => _limiter = limiter.NotNull();

    public async Task<Option<string>> Add(string key, T data)
    {
        data.NotNull();

        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Add path={path}", path);

        DataETag dataEtag = data.ToJson().ToDataETag();
        var result = await _keyStore.Add(path, dataEtag);
        if (result.IsOk()) _addCounter?.Increment();

        if (_limiter != null) await _limiter.SignalChange();
        return result;
    }

    public async Task<Option> Delete(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Delete: Deleting list key={key}", key);

        string folder = $"{_keySystem.GetPathPrefix()}/{key}";
        var result = await _keyStore.DeleteFolder(folder);
        if (result.IsOk()) _deleteCounter?.Increment();

        return result;
    }

    public async Task<Option> DeleteItem(string path)
    {
        path.NotEmpty();
        var rootPath = _keySystem.AddPathPrefix(path);
        _logger.LogDebug("DeleteItem: Deleting item path={path}, rootPath={rootPath}", path, rootPath);

        var result = await _keyStore.Delete(rootPath);
        return result;
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Get: Getting list items, key={key}", key);

        IReadOnlyList<StorePathDetail> searchList = await GetDetails(key);
        var result = await ReadList(searchList);

        if (result.IsOk()) _getCounter?.Increment();
        return result;
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex)
    {
        timeIndex = new DateTime(timeIndex.Year, timeIndex.Month, timeIndex.Day, timeIndex.Hour, 0, 0, DateTimeKind.Utc);
        _logger.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        IReadOnlyList<StorePathDetail> searchList = await GetDetails(key);

        var indexedList = searchList
            .Select((x, i) => (index: i, dir: x, extractTime: _keySystem.ExtractTimeIndex(x.Path)))
            .OrderBy(x => x.dir.Path)
            .ToArray();

        int minIndex = indexedList
            .Where(x => x.extractTime >= timeIndex)
            .Func(x => x.Any() ? x.Min(x => x.index) - 1 : int.MaxValue);

        var list = indexedList
            .Where(x => x.index >= minIndex)
            .Select(x => x.dir)
            .ToArray();

        var result = await ReadList(list);
        return result;
    }

    public async Task<IReadOnlyList<StorePathDetail>> GetDetails(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Search: key={key}", key);

        string searchPattern = _keySystem.BuildKeySearch(key);

        IReadOnlyList<StorePathDetail> searchList = await _keyStore.Search(searchPattern);

        IReadOnlyList<StorePathDetail> result = searchList
            .Select(x => x with { Path = _keySystem.RemovePathPrefix(x.Path) })
            .ToImmutableArray();

        return result;
    }

    private async Task<Option<IReadOnlyList<T>>> ReadList(IReadOnlyList<StorePathDetail> searchList)
    {
        var journalQueue = new ConcurrentQueue<(DateTime logTime, long counter, T value)>();

        var scale = new ActionQueue<string>(async path =>
        {
            var result = await reader(path);
            (DateTime logTime, long counter) = PartitionSchemas.ExtractSequenceNumberIndex(path);
            if (result.IsError())
            {
                _logger.LogError("ReadList: Failed to read path={path}, error={error}", path, result.Error);
                return;
            }

            journalQueue.Enqueue((logTime, counter, result.Return()));
        }, maxWorkers: 5);

        await scale.SendAsync(searchList.Select(x => x.Path));
        await scale.CloseAsync();

        var dataItems = journalQueue
            .OrderBy(x => x.logTime)
            .ThenBy(x => x.counter)
            .Select(x => x.value)
            .ToImmutableArray();

        _logger.LogDebug("ReadList: count={count}", dataItems.Length);
        return dataItems;

        async Task<Option<T>> reader(string path)
        {
            _logger.LogDebug("Reading path={path}", path);

            var fullPath = _keySystem.AddPathPrefix(path);
            Option<DataETag> readOption = await _keyStore.Get(fullPath);
            if (readOption.IsError())
            {
                _logger.LogDebug("Fail to read path={path}", path);
                return readOption.ToOptionStatus<T>();
            }

            var json = readOption.Return().DataToString();
            var result = json.ToObject<T>().NotNull();

            return result;
        }
    }
}
