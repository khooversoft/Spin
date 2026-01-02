using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListSpace<T> : IListStore<T>
{
    private readonly IKeyStore _keyStore;
    private readonly ListKeySystem<T> _fileSystem;
    private readonly ILogger<ListSpace<T>> _logger;

    public ListSpace(IKeyStore keyStore, ListKeySystem<T> listKeySystem, ILogger<ListSpace<T>> logger)
    {
        _keyStore = keyStore.NotNull();
        _fileSystem = listKeySystem.NotNull();
        _logger = logger.NotNull();
    }

    public ListKeySystem<T> ListKeySystem => _fileSystem;

    public async Task<Option<string>> Append(string key, IEnumerable<T> data)
    {
        var dataItems = data.NotNull().Select(x => x.ToJson()).ToArray();
        if (dataItems.Length == 0) return StatusCode.OK;

        string path = _fileSystem.PathBuilder(key);
        _logger.LogDebug("Appending key={key}, listType={listType}, path={path}, dataCount={dataCount}", key, typeof(T).Name, path, dataItems.Length);

        // FIX: Use string.Join instead of O(n^2) aggregation
        string json = string.Join(Environment.NewLine, dataItems) + Environment.NewLine;
        DataETag dataEtag = json.ToDataETag();

        var append = await _keyStore.Append(path, dataEtag);
        if (append.IsError())
        {
            _logger.LogError("Append failed key={key}, path={path}, error={error}", key, path, append.Error);
        }

        return append;
    }

    public async Task<Option> Delete(string key)
    {
        key.NotEmpty();
        _logger.LogDebug("Delete: Deleting list key={key}", key);

        // FIX: Include filesystem base path prefix for correct folder clear
        string folder = $"{_fileSystem.GetPathPrefix()}/{key}";
        var clearOption = await _keyStore.ClearFolder(folder);
        return clearOption;
    }

    public Task<Option<IReadOnlyList<T>>> Get(string key) => Get(key, "**/*");

    public async Task<Option<IReadOnlyList<T>>> Get(string key, string pattern)
    {
        key.NotEmpty();
        pattern.NotEmpty();
        _logger.LogDebug("Get: Getting list items, pattern={pattern}", pattern);

        IReadOnlyList<StorePathDetail> searchList = await InternalSearch(key, pattern);
        return await ReadList(searchList);
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex)
    {
        timeIndex = new DateTime(timeIndex.Year, timeIndex.Month, timeIndex.Day, timeIndex.Hour, 0, 0, DateTimeKind.Utc);
        _logger.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        IReadOnlyList<StorePathDetail> searchList = await InternalSearch(key, null);

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

        return await ReadList(list);
    }

    public Task<IReadOnlyList<StorePathDetail>> Search(string key, string pattern) => InternalSearch(key, pattern);

    private async Task<IReadOnlyList<StorePathDetail>> InternalSearch(string key, string? pattern)
    {
        key.NotEmpty();
        _logger.LogDebug("Search: key={key}, pattern={pattern}", key, pattern);

        string searchPattern = pattern switch
        {
            null => _fileSystem.BuildSearch(key),
            _ => _fileSystem.BuildSearch(key, pattern),
        };

        IReadOnlyList<StorePathDetail> searchList = await _keyStore.Search(searchPattern);

        IReadOnlyList<StorePathDetail> result = searchList
            .Select(x => x with { Path = _fileSystem.RemovePathPrefix(x.Path) })
            .ToImmutableArray();

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
                .Select(x => x.ToObject<T>().NotNull())
                .ToArray();

            return result;
        }
    }
}
