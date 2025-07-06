using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class FileStoreDataProvider : IDataProvider
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<FileStoreDataProvider> _logger;
    private const string _name = nameof(FileStoreDataProvider);

    public FileStoreDataProvider(IFileStore fileStore, ILogger<FileStoreDataProvider> logger)
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
            case DataPipelineCommand.Append:
                var appendOption = await OnAppend(dataContext, dataContext.SetData.First(), context).ConfigureAwait(false);
                if (appendOption.IsError()) return appendOption.ToOptionStatus<DataPipelineContext>();
                break;

            //case DataPipelineCommand.AppendList:
            //    var appendListOption = await OnAppendList(dataContext.Path, dataContext.SetData, context).ConfigureAwait(false);
            //    if (appendListOption.IsError()) return appendListOption.ToOptionStatus<DataPipelineContext>();
            //    break;

            case DataPipelineCommand.Delete:
                await OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.Get:
                var getOption = await OnGet(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            //case DataPipelineCommand.GetList:
            //    var getListOption = await OnGetList(dataContext, context);
            //    if (getListOption.IsOk()) return getListOption;
            //    break;

            case DataPipelineCommand.Set:
                var setOption = await OnSet(dataContext.Path, dataContext.SetData.First(), context);
                if (setOption.IsError()) return setOption.ToOptionStatus<DataPipelineContext>();
                break;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);

        if (nextOption.IsOk())
        {
            switch (dataContext.Command)
            {
                case DataPipelineCommand.Get:
                    (await OnSet(dataContext.Path, nextOption.Return().GetData.First(), context))
                        .LogStatus(context, "Setting path={path} to file store cache, name={name}", [dataContext.Path, _name]);
                    break;
            }
        }

        return nextOption;
    }

    public async Task<Option> OnAppend(DataPipelineContext dataContext, DataETag data, ScopeContext context)
    {
        var isValidOption = await IsCacheIsValid(dataContext, context);
        if (isValidOption.IsError()) return isValidOption;

        var detailsOption = await _fileStore.File(dataContext.Path).Append(data, context);
        if (detailsOption.IsError())
        {
            context.LogDebug("Fail to append to path={path} from file store, name={name}", dataContext.Path, _name);
            Counters.AddAppendFailCount();
            return detailsOption.ToOptionStatus();
        }

        Counters.AddAppendCount();
        return StatusCode.OK;
    }

    //public async Task<Option> OnAppendList(string path, IReadOnlyList<DataETag> dataItems, ScopeContext context)
    //{
    //    dataItems.NotNull().Assert(x => x.Count > 0, _ => "Empty list");
    //    context.LogDebug("Appending path={path}, name={name}", path, _name);

    //    string json = dataItems.Aggregate(string.Empty, (a, x) => a += x.DataToString() + Environment.NewLine);
    //    DataETag data = json.ToDataETag();

    //    var detailsOption = await _fileStore.File(path).Append(data, context);
    //    if (detailsOption.IsError())
    //    {
    //        Counters.AddAppendFailCount();
    //        return detailsOption.ToOptionStatus<string>();
    //    }

    //    Counters.AddAppendCount();
    //    return StatusCode.OK;
    //}

    public async Task<Option> OnDelete(string path, ScopeContext context)
    {
        context.LogDebug("Deleting path={path} provider cache, name={name}", path, _name);

        var deleteOption = await _fileStore.File(path).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete path={path} from file store, name={name}", path, _name);
            Counters.AddDeleteFailCount();
            return deleteOption;
        }

        Counters.AddDeleteCount();
        return StatusCode.OK;
    }

    private async Task<Option<DataPipelineContext>> OnGet(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting path={path} from file store, name={name}", dataContext.Path, _name);

        var isValidOption = await IsCacheIsValid(dataContext, context);
        if (isValidOption.IsError()) return isValidOption.ToOptionStatus<DataPipelineContext>();

        var readOption = await _fileStore.File(dataContext.Path).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read path={path} from file store, name={name}", dataContext.Path, _name);
            return StatusCode.NotFound;
        }

        Counters.AddHits();
        context.LogDebug("Found path={path} in file store cache, name={name}", dataContext.Path, _name);

        dataContext = dataContext with { GetData = [readOption.Return()] };
        return dataContext;
    }

    //private async Task<Option<DataPipelineContext>> OnGetList(DataPipelineContext dataContext, ScopeContext context)
    //{
    //    context.LogDebug("Getting path={path}, name={name}", dataContext.Path, _name);

    //    var readOption = await _fileStore.File(dataContext.Path).Get(context);
    //    if (readOption.IsError())
    //    {
    //        Counters.AddMisses();
    //        context.LogDebug("Fail to read path={path}, name={name}", dataContext.Path, _name);
    //        return StatusCode.NotFound;
    //    }

    //    var stringData = readOption.Return().DataToString();

    //    var dataItems = stringData
    //        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
    //        .Select(x => x.ToDataETag())
    //        .ToImmutableList();

    //    Counters.AddHits();
    //    context.LogDebug("Found path={path}, name={name}", dataContext.Path, _name);

    //    dataContext = dataContext with { GetData = dataItems };
    //    return dataContext;
    //}

    private async Task<Option> OnSet(string path, DataETag data, ScopeContext context)
    {
        context.LogDebug("Setting path={path} to file store cache, name={name}", path, _name);

        var setOption = await _fileStore.File(path).Set(data, context);
        if (setOption.IsOk())
        {
            Counters.AddSetCount();
        }
        else
        {
            Counters.AddSetFailCount();
            context.LogDebug("Fail to write path={path} from file store, name={name}", path, _name);
        }

        return setOption.ToOptionStatus();
    }

    internal async Task<Option> IsCacheIsValid(DataPipelineContext dataContext, ScopeContext context)
    {
        if (dataContext.PipelineConfig.FileCacheDuration == null) return StatusCode.OK;

        context.LogDebug("Check to see if path={path} to file store cache, name={name}", dataContext.Path, _name);
        var file = _fileStore.File(dataContext.Path);

        var detailsOption = await file.GetDetails(context);
        if (detailsOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read path={path} from file store, name={name}", dataContext.Path, _name);
            return StatusCode.NotFound;
        }

        TimeSpan age = DateTime.UtcNow - (detailsOption.Return().CreatedOn ?? DateTime.UtcNow);
        if (age > dataContext.PipelineConfig.FileCacheDuration)
        {
            context.LogDebug("File store cache is too old, path={path}, name={name}, timeLimit={timeLimit}, age={age}",
                dataContext.Path, _name, dataContext.PipelineConfig.FileCacheDuration, age);

            Counters.AddRetireCount();
            Counters.AddMisses();
            (await OnDelete(dataContext.Path, context)).LogStatus(context, "Deleting expired path={path} from name={name}", [dataContext.Path, _name]);
            return StatusCode.NotFound;
        }

        return StatusCode.OK;
    }
}
