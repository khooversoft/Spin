using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListStore<T> : IListStore<T>
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<ListStore<T>> _logger;
    private readonly IListFileSystem<T> _fileSystem;

    public ListStore(IFileStore fileStore, IListFileSystem<T> fileSystem, ILogger<ListStore<T>> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
        _fileSystem = fileSystem;
    }

    public async Task<Option<string>> Append(string key, IEnumerable<T> data, ScopeContext context)
    {
        var dataItems = data.NotNull().Select(x => x.ToJson()).ToArray();
        if (dataItems.Length == 0) return (StatusCode.NoContent, "Empty list");

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Appending key={key}, listType={listType}, path={path}, dataCount={dataCount}", key, typeof(T).Name, path, dataItems.Length);

        // FIX: Use string.Join instead of O(n^2) aggregation
        string json = string.Join(Environment.NewLine, dataItems) + Environment.NewLine;
        DataETag dataEtag = json.ToDataETag();

        var detailsOption = await _fileStore.File(path).Append(dataEtag, context);
        detailsOption.LogStatus(context, "Append to path={path}", [path]);

        return detailsOption;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.LogDebug("Delete: Deleting list key={key}", key);

        // FIX: Include filesystem base path prefix for correct folder clear
        string folder = $"{_fileSystem.CreatePathPrefix()}{key}";
        var clearOption = await _fileStore.ClearFolder(folder, context);
        return clearOption;
    }

    public Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context) => Get(key, "**/*", context);

    public async Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context)
    {
        key.NotEmpty();
        pattern.NotEmpty();
        context.LogDebug("Get: Getting list items, pattern={pattern}", pattern);

        // FIX: honor pattern in search
        string searchPattern = _fileSystem.BuildSearch(key, pattern);
        IReadOnlyList<IStorePathDetail> searchList = (await _fileStore.Search(searchPattern, context)).OrderBy(x => x.Path).ToArray();
        return await ReadList(pattern, context, searchList);
    }

    public async Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex, ScopeContext context)
    {
        context.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        string searchPattern = _fileSystem.BuildSearch(key);
        IReadOnlyList<IStorePathDetail> searchList = (await _fileStore.Search(searchPattern, context)).OrderBy(x => x.Path).ToArray();

        var indexedList = searchList
            .Select((x, i) => (index: i, dir: x, active: _fileSystem.ExtractTimeIndex(x.Path) >= timeIndex))
            .ToArray();

        int minIndex = indexedList.Where(x => x.active).Func(x => x.Any() ? x.Min(x => x.index) - 1 : 0);

        var list = indexedList
            .Where(x => x.index >= minIndex)
            .Select(x => x.dir)
            .ToArray();

        return await ReadList(searchPattern, context, list);
    }

    public async Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context)
    {
        pattern.NotEmpty();
        context.LogDebug("Search: pattern={pattern}", pattern);

        string searchPattern = _fileSystem.BuildSearch(key, pattern);
        IReadOnlyList<IStorePathDetail> searchList = await _fileStore.Search(searchPattern, context);
        return searchList;
    }

    private async Task<Option<IReadOnlyList<T>>> ReadList(string pattern, ScopeContext context, IReadOnlyList<IStorePathDetail> searchList)
    {
        var taskList = new Sequence<Task<IReadOnlyList<T>>>();

        foreach (var pathDetail in searchList)
        {
            if (pathDetail.IsFolder) continue;

            taskList += reader(pathDetail.Path);
        }

        IReadOnlyList<T>[] list = await Task.WhenAll(taskList);
        var dataItems = list.SelectMany(x => x).ToImmutableArray();
        context.LogDebug("GetList: search={pattern}, count={count}", pattern, dataItems.Length);
        return dataItems;

        async Task<IReadOnlyList<T>> reader(string path)
        {
            context.LogDebug("Reading path={path}", path);
            Option<DataETag> readOption = await _fileStore.File(path).Get(context);
            if (readOption.IsError())
            {
                context.LogDebug("Fail to read path={path}", path);
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