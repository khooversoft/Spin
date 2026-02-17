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
    private readonly KeyListStrategy<T> _keyListStrategy;
    private readonly ILogger<ListSpace<T>> _logger;
    private readonly ITelemetryCounter<long>? _appendCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private readonly ITelemetryCounter<long>? _getCounter;
    private readonly ITelemetryCounter<long>? _getHistoryCounter;
    private readonly ITelemetryCounter<long>? _searchCounter;

    public ListSpace(IKeyStore keyStore, KeyListStrategy<T> keyListStrategy, ILogger<ListSpace<T>> logger, ITelemetry? telemetry = null)
    {
        _keyStore = keyStore.NotNull();
        _keyListStrategy = keyListStrategy.NotNull();
        _logger = logger.NotNull();

        _appendCounter = telemetry?.CreateCounter<long>("listspace.append", "Number of Append operations", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("listspace.delete", "Number of Delete operations", unit: "count");
        _getCounter = telemetry?.CreateCounter<long>("listspace.get", "Number of Get operations", unit: "count");
        _getHistoryCounter = telemetry?.CreateCounter<long>("listspace.gethistory", "Number of GetHistory operations", unit: "count");
        _searchCounter = telemetry?.CreateCounter<long>("listspace.search", "Number of Search operations", unit: "count");
    }

    public KeyListStrategy<T> KeyListStrategy => _keyListStrategy;

    public async Task<Option<string>> Append(string key, params IEnumerable<T> data)
    {
        var dataItems = data.NotNull().Select(x => x.ToJson()).ToArray();
        if (dataItems.Length == 0) return StatusCode.OK;

        string json = string.Join(Environment.NewLine, dataItems) + Environment.NewLine;
        DataETag dataEtag = json.ToDataETag();

        string path = await GetCurrentListPath(key, dataEtag.Data.Length);
        _logger.LogDebug(
            "[Appending] dataCount={dataCount}, size={size}, key={key}, listType={listType}, path={path}",
            dataItems.Length,
            json.Length,
            key,
            typeof(T).Name,
            path
            );

        var append = await _keyStore.Append(path, dataEtag);
        if (append.IsError())
        {
            _logger.LogError("Append failed key={key}, path={path}, error={error}", key, path, append.Error);
        }

        var testResult = await Get(key);
        var result = testResult.ThrowOnError().Return();
        _logger.LogDebug("[Append:TempGet] count={count}, path={path}", result.Count, path);

        _appendCounter?.Increment();
        return append;
    }

    public async Task<Option> Delete(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Delete: Deleting list key={key}", key);

        string folder = _keyListStrategy.BuildDeleteFolder(key);
        var clearOption = await _keyStore.DeleteFolder(folder);

        _deleteCounter?.Increment();
        return clearOption;
    }

    public async Task<Option<IReadOnlyList<T>>> Get(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("[Get] Getting list items, key={key}", key);

        IReadOnlyList<StorePathDetail> searchList = await Search(key);
        var result = await ReadList(searchList);
        if (result.IsOk())
        {
            _logger.LogDebug("[Get] Retrived list items, key={key}, count={count}", key, result.Return().Count);
        }

        if (result.IsOk()) _getCounter?.Increment();
        return result;
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex)
    {
        timeIndex = new DateTime(timeIndex.Year, timeIndex.Month, timeIndex.Day, timeIndex.Hour, 0, 0, DateTimeKind.Utc);
        Lsn timeIndexLsn = new Lsn(timeIndex, 0);
        _logger.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        IReadOnlyList<StorePathDetail> searchList = await Search(key);

        var indexedList = searchList
            .Select((x, i) => (index: i, dir: x, extractTimeLsn: getLsn(x.Path)))
            .OrderBy(x => x.dir.Path)
            .ToArray();

        int minIndex = indexedList
            .Where(x => x.extractTimeLsn >= timeIndexLsn)
            .Func(x => x.Any() ? x.Min(x => x.index) - 1 : int.MaxValue);

        var list = indexedList
            .Where(x => x.index >= minIndex)
            .Select(x => x.dir)
            .ToArray();

        var result = await ReadList(list);

        if (result.IsOk()) _getHistoryCounter?.Increment();
        return result;


        Lsn getLsn(string path)
        {
            var seqNumber = _keyListStrategy.GetPathParts(path).SeqNumber;
            var lsn = Lsn.Parse(seqNumber);
            return lsn;
        }
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Search: key={key}", key);

        string searchPattern = _keyListStrategy.BuildKeySearch(key);

        IReadOnlyList<StorePathDetail> searchList = await _keyStore.Search(searchPattern);

        IReadOnlyList<StorePathDetail> result = searchList
            .Select(x => x with { Path = _keyListStrategy.RemoveBasePath(x.Path) })
            .Where(x => _keyListStrategy.IsValidPath(x.Path))
            .ToImmutableArray();

        _searchCounter?.Increment();
        return result;
    }

    public async Task<IReadOnlyList<ListFileDetail<T>>> ReadListFiles(IReadOnlyList<StorePathDetail> readList)
    {
        var journalQueue = new ConcurrentQueue<ListFileDetail<T>>();

        var scale = new ActionBlock2<StorePathDetail>(async pathDetail =>
        {
            var result = await reader(pathDetail.Path);

            var listFileDetail = new ListFileDetail<T>
            {
                StorePathDetail = pathDetail,
                Data = result.ToJson().ToDataETag(),
                Items = result
            };

            journalQueue.Enqueue(listFileDetail);
        }, maxWorkers: 5);

        await scale.SendAsync(readList.Select(x => x));
        await scale.CloseAsync();

        var dataItems = journalQueue.ToImmutableArray();

        _logger.LogDebug("ReadList: count={count}", dataItems.Length);
        return dataItems;

        async Task<IReadOnlyList<T>> reader(string path)
        {
            _logger.LogDebug("Reading path={path}", path);

            var fullPath = _keyListStrategy.AddBasePath(path);
            Option<DataETag> readOption = await _keyStore.Get(fullPath);
            if (readOption.IsError())
            {
                _logger.LogDebug("Fail to read path={path}", path);
                throw new InvalidOperationException($"Failed to read path={path}, statusCode={readOption.StatusCode}, error={readOption.Error}");
            }

            var result = readOption.Return()
                .DataToString()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToObject<T>().NotNull())
                .ToArray();

            return result;
        }
    }

    private async Task<string> GetCurrentListPath(string key, int addSize)
    {
        var find = await FindCurrentListFile(key, addSize);
        if (find.IsOk()) return _keyListStrategy.AddBasePath(find.Return().Path);

        return _keyListStrategy.BuildPath(key);
    }

    private async Task<Option<StorePathDetail>> FindCurrentListFile(string key, int addSize)
    {
        var dir = await Search(key);
        if (dir.Count == 0) return StatusCode.NotFound;

        var last = dir.OrderBy(x => x.Path).Last();
        var totalSize = last.ContentLength + addSize;
        var maxSize = _keyListStrategy.MaxBlockSizeBytes - 2000;
        _logger.LogTrace("[FindCurrentListFile] key={key}, for file={path}, contentLength={contentLength}", key, last.Path, last.ContentLength);

        if (totalSize >= maxSize)
        {
            _logger.LogTrace("[FindCurrentListFile:PartitionTooBig] Partition is too big, key={key}, for file={path}, contentLength={contentLength}", key, last.Path, last.ContentLength);
            return StatusCode.NotFound;
        }

        _logger.LogTrace("[FindCurrentListFile] Found current partition, key={key}, for file={path}, contentLength={contentLength}", key, last.Path, last.ContentLength);
        return last;
    }

    private async Task<Option<IReadOnlyList<T>>> ReadList(IReadOnlyList<StorePathDetail> searchList)
    {
        var list = await ReadListFiles(searchList);

        var dataItems = list
            .OrderBy(x => x.StorePathDetail.Path)
            .SelectMany(x => x.Items)
            .ToImmutableArray();

        return dataItems;
    }
}
