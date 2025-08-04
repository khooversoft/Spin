using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class ListStore : IListStore
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<ListStore> _logger;
    private readonly IListFileSystem _fileSystem;

    public ListStore(IFileStore fileStore, IListFileSystem fileSystem, ILogger<ListStore> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
        _fileSystem = fileSystem;
    }

    public async Task<Option<string>> Append(string key, string listType, IEnumerable<DataETag> data, ScopeContext context)
    {
        var dataItems = data.NotNull().ToArray();
        if (dataItems.Length == 0) return (StatusCode.NoContent, "Empty list");

        string path = _fileSystem.PathBuilder(key, listType);
        context.LogDebug("Appending key={key}, listType={listType}, path={path}", key, listType, path);

        string json = dataItems.Aggregate(string.Empty, (a, x) => a += x.DataToString() + Environment.NewLine);
        DataETag dataEtag = json.ToDataETag();

        var detailsOption = await _fileStore.File(path).Append(dataEtag, context);
        detailsOption.LogStatus(context, "Append to path={path}", [path]);

        return detailsOption;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        context.LogDebug("Delete: Deleting list key={key}", key);

        string pattern = _fileSystem.SearchBuilder(key, "**/*");
        var clearOption = await _fileStore.ClearFolder(pattern, context);

        return clearOption;
    }

    public Task<Option<IReadOnlyList<ListPathData>>> Get(string key, ScopeContext context) => Get(key, "**/*", context);

    public async Task<Option<IReadOnlyList<ListPathData>>> Get(string key, string pattern, ScopeContext context)
    {
        key.NotEmpty();
        pattern.NotEmpty();
        context.LogDebug("Get: Getting list items, pattern={pattern}", pattern);

        string searchPattern = _fileSystem.SearchBuilder(key, pattern);

        IReadOnlyList<IStorePathDetail> searchList = (await _fileStore.Search(searchPattern, context)).OrderBy(x => x.Path).ToArray();
        return await ReadList(pattern, context, searchList);
    }

    public async Task<Option<IReadOnlyList<ListPathData>>> GetHistory(string key, DateTime timeIndex, ScopeContext context)
    {
        context.LogDebug("Getting history, key={key}, timeIndex={timeIndex}", key, timeIndex);

        string pattern = _fileSystem.SearchBuilder(key, "**/*");
        IReadOnlyList<IStorePathDetail> searchList = (await _fileStore.Search(pattern, context)).OrderBy(x => x.Path).ToArray();

        var indexedList = searchList
            .Select((x, i) => (index: i, dir: x, active: _fileSystem.ExtractTimeIndex(x.Path) >= timeIndex))
            .ToArray();

        int minIndex = indexedList.Where(x => x.active).Func(x => x.Any() ? x.Min(x => x.index) - 1 : 0);

        var list = indexedList
            .Where(x => x.index >= minIndex)
            .Select(x => x.dir)
            .ToArray();

        return await ReadList(pattern, context, list);
    }

    public async Task<Option<IReadOnlyList<DataETag>>> GetPartition(string key, string listType, DateTime timeIndex, ScopeContext context)
    {
        var path = _fileSystem.PathBuilder(key, listType, timeIndex);
        context.LogDebug("GetPartition: key={key}, listType={listType}, timeIndex={timeIndex}, path={path}", key, listType, timeIndex, path);

        var getOption = await _fileStore.File(path).Get(context);

        if (getOption.IsError()) return getOption
                .LogStatus(context, "Failed to get file for path={path}", [path])
                .ToOptionStatus<IReadOnlyList<DataETag>>();

        DataETag dataETag = getOption.Return();

        var list = dataETag.DataToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToDataETag())
            .ToImmutableArray();

        return list;
    }

    public async Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context)
    {
        pattern.NotEmpty();
        context.LogDebug("Search: pattern={pattern}", pattern);

        IReadOnlyList<IStorePathDetail> searchList = await _fileStore.Search(pattern, context);
        return searchList;
    }

    private async Task<Option<IReadOnlyList<ListPathData>>> ReadList(string pattern, ScopeContext context, IReadOnlyList<IStorePathDetail> searchList)
    {
        var list = new Sequence<ListPathData>();

        foreach (var pathDetail in searchList)
        {
            if (pathDetail.IsFolder) continue;
            context.LogDebug("Reading path={path}", pathDetail.Path);

            Option<DataETag> readOption = await _fileStore.File(pathDetail.Path).Get(context);
            if (readOption.IsError())
            {
                context.LogDebug("Fail to read path={path}", pathDetail.Path);
                continue;
            }

            list += new ListPathData
            {
                PathDetail = pathDetail,
                Data = readOption.Return()
                    .DataToString()
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToDataETag())
                    .ToImmutableArray(),
            };
        }

        var dataItems = list.ToImmutableArray();
        context.LogDebug("GetList: search={pattern}, count={count}", pattern, dataItems.Length);
        return dataItems;
    }
}