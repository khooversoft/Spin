using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class ListStoreDataProvider : IDataProvider
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<ListStoreDataProvider> _logger;
    private const string _name = nameof(FileStoreDataProvider);

    public ListStoreDataProvider(IFileStore fileStore, ILogger<ListStoreDataProvider> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);
        context.LogDebug("FileStoreDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.AppendList:
                var appendListOption = await OnAppendList(dataContext.Path, dataContext.SetData, context).ConfigureAwait(false);
                if (appendListOption.IsError()) return appendListOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.DeleteList:
                var deleteListOption = await OnDeleteList(dataContext, context).ConfigureAwait(false);
                if (deleteListOption.IsError()) return deleteListOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.GetList:
                var getListOption = await OnGetList(dataContext, context);
                if (getListOption.IsOk()) return getListOption;
                break;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);
        return nextOption;
    }

    public async Task<Option> OnAppendList(string path, IReadOnlyList<DataETag> dataItems, ScopeContext context)
    {
        dataItems.NotNull().Assert(x => x.Count > 0, _ => "Empty list");
        context.LogDebug("Appending path={path}, name={name}", path, _name);

        string json = dataItems.Aggregate(string.Empty, (a, x) => a += x.DataToString() + Environment.NewLine);
        DataETag data = json.ToDataETag();

        var detailsOption = await _fileStore.File(path).Append(data, context);
        if (detailsOption.IsError())
        {
            Counters.AddAppendFailCount();
            return detailsOption.ToOptionStatus<string>();
        }

        Counters.AddAppendCount();
        return StatusCode.OK;
    }

    private async Task<Option> OnDeleteList(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("OnDeleteList: Deleting list path={path}, name={name}", dataContext.Path, _name);

        IReadOnlyList<IStorePathDetail> searchList = await _fileStore.Search(dataContext.Path, context);

        foreach (var pathDetail in searchList)
        {
            if (pathDetail.IsFolder) continue;
            context.LogDebug("Reading path={path}, name={name}", pathDetail.Path, _name);

            Option readOption = await _fileStore.File(pathDetail.Path).Delete(context);
            if (readOption.IsError())
            {
                context.LogDebug("Fail to delete path={path}, name={name}", dataContext.Path, _name);
                continue;
            }
        }

        return StatusCode.OK;
    }

    private async Task<Option<DataPipelineContext>> OnGetList(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("OnGetList: Getting path={path}, name={name}", dataContext.Path, _name);

        IReadOnlyList<IStorePathDetail> searchList = await _fileStore.Search(dataContext.Path, context);
        var list = new Sequence<DataETag>();

        foreach (var pathDetail in searchList)
        {
            if (pathDetail.IsFolder) continue;
            context.LogDebug("Reading path={path}, name={name}", pathDetail.Path, _name);

            Option<DataETag> readOption = await _fileStore.File(pathDetail.Path).Get(context);
            if (readOption.IsError())
            {
                context.LogDebug("Fail to read path={path}, name={name}", dataContext.Path, _name);
                continue;
            }

            list += readOption.Return();
        }

        var dataItems = list
            .Select(x => x.DataToString())
            .SelectMany(x => x.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            .Select(y => y.ToDataETag())
            .ToImmutableArray();

        if (dataItems.Length == 0) Counters.AddMisses(); else Counters.AddHits();
        context.LogDebug("OnGetList: search={path}, name={name}, count={count}", dataContext.Path, _name, dataItems.Length);

        dataContext = dataContext with { GetData = dataItems };
        return dataContext;
    }
}
