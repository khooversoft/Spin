using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListSpace<T> : IListStore<T>
{
    private readonly IKeyStore _keyStore;
    private readonly ListKeySystem<T> _fileSystem;
    private readonly ILogger<ListSpace<T>> _logger;
    private readonly ITelemetryCounter<long>? _appendCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private readonly ITelemetryCounter<long>? _getCounter;
    private readonly ITelemetryCounter<long>? _getHistoryCounter;
    private readonly ITelemetryCounter<long>? _searchCounter;
    private readonly SpaceOption<T> _options;

    public ListSpace(IKeyStore keyStore, ListKeySystem<T> listKeySystem, SpaceOption<T> options, ILogger<ListSpace<T>> logger, ITelemetry? telemetry = null)
    {
        _keyStore = keyStore.NotNull();
        _fileSystem = listKeySystem.NotNull();
        _options = options.NotNull();
        _logger = logger.NotNull();

        _appendCounter = telemetry?.CreateCounter<long>("listspace.append", "Number of Append operations", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("listspace.delete", "Number of Delete operations", unit: "count");
        _getCounter = telemetry?.CreateCounter<long>("listspace.get", "Number of Get operations", unit: "count");
        _getHistoryCounter = telemetry?.CreateCounter<long>("listspace.gethistory", "Number of GetHistory operations", unit: "count");
        _searchCounter = telemetry?.CreateCounter<long>("listspace.search", "Number of Search operations", unit: "count");
    }

    public ListKeySystem<T> ListKeySystem => _fileSystem;

    public async Task<Option<string>> Append(string key, params IEnumerable<T> data)
    {
        var dataItems = data.NotNull().Select(x => _options.Serializer(x)).ToArray();
        if (dataItems.Length == 0) return StatusCode.OK;

        string path = _fileSystem.PathBuilder(key);
        _logger.LogDebug("Appending key={key}, listType={listType}, path={path}, dataCount={dataCount}", key, typeof(T).Name, path, dataItems.Length);

        string json = string.Join(Environment.NewLine, dataItems) + Environment.NewLine;
        DataETag dataEtag = json.ToDataETag();

        var append = await _keyStore.Append(path, dataEtag);
        if (append.IsError())
        {
            _logger.LogError("Append failed key={key}, path={path}, error={error}", key, path, append.Error);
        }

        _appendCounter?.Increment();
        return append;
    }

    public async Task<Option> Delete(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Delete: Deleting list key={key}", key);

        string folder = $"{_fileSystem.GetPathPrefix()}/{key}";
        var clearOption = await _keyStore.DeleteFolder(folder);

        _deleteCounter?.Increment();
        return clearOption;
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Get: Getting list items, key={key}", key);

        IReadOnlyList<StorePathDetail> searchList = await Search(key);
        var result = await ReadList(searchList);

        if (result.IsOk()) _getCounter?.Increment();
        return result;
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex)
    {
        timeIndex = new DateTime(timeIndex.Year, timeIndex.Month, timeIndex.Day, timeIndex.Hour, 0, 0, DateTimeKind.Utc);
        _logger.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        IReadOnlyList<StorePathDetail> searchList = await Search(key);

        var indexedList = searchList
            .Select((x, i) => (index: i, dir: x, extractTime: _fileSystem.ExtractTimeIndex(x.Path)))
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

        if (result.IsOk()) _getHistoryCounter?.Increment();
        return result;
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Search: key={key}", key);

        string searchPattern = _fileSystem.BuildKeySearch(key);

        IReadOnlyList<StorePathDetail> searchList = await _keyStore.Search(searchPattern);

        IReadOnlyList<StorePathDetail> result = searchList
            .Select(x => x with { Path = _fileSystem.RemovePathPrefix(x.Path) })
            .ToImmutableArray();

        _searchCounter?.Increment();
        return result;
    }

    private async Task<Option<IReadOnlyList<T>>> ReadList(IReadOnlyList<StorePathDetail> searchList)
    {
        var journalQueue = new ConcurrentQueue<IReadOnlyList<T>>();

        var scale = new ActionQueue<string>(async path =>
        {
            var result = await reader(path);
            journalQueue.Enqueue(result);
        }, maxWorkers: 5);

        await scale.SendAsync(searchList.Select(x => x.Path));
        await scale.CloseAsync();

        var dataItems = journalQueue.SelectMany(x => x).ToImmutableArray();
        _logger.LogDebug("ReadList: count={count}", dataItems.Length);
        return dataItems;

        async Task<IReadOnlyList<T>> reader(string path)
        {
            _logger.LogDebug("Reading path={path}", path);

            var fullPath = _fileSystem.AddPathPrefix(path);
            Option<DataETag> readOption = await _keyStore.Get(fullPath);
            if (readOption.IsError())
            {
                _logger.LogDebug("Fail to read path={path}", path);
                return Array.Empty<T>();
            }

            var result = readOption.Return()
                .DataToString()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => _options.Deserializer(x).NotNull())
                .ToArray();

            return result;
        }
    }
}
