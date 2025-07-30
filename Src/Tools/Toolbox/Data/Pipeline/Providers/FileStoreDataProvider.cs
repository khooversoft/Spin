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
    private readonly LockManager _lockManager;

    public FileStoreDataProvider(IFileStore fileStore, LockManager lockManager, ILogger<FileStoreDataProvider> logger)
    {
        _fileStore = fileStore.NotNull();
        _lockManager = lockManager.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);
        context.LogDebug("FileStoreDataProvider: Executing command={command}", dataContext.Command);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
                var appendOption = await OnAppend(dataContext, dataContext.SetData.First(), context).ConfigureAwait(false);
                if (appendOption.IsError()) return appendOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.Delete:
                await OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.Get:
                var getOption = await OnGet(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            case DataPipelineCommand.Set:
                var setOption = await OnSet(dataContext, dataContext.SetData.First(), context);
                if (setOption.IsError()) return setOption.ToOptionStatus<DataPipelineContext>();
                break;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);

        if (nextOption.IsOk())
        {
            switch (dataContext.Command)
            {
                case DataPipelineCommand.Get:
                    (await OnSet(dataContext, nextOption.Return().GetData.First(), context))
                        .LogStatus(context, "Setting path={path} to file store cache", [dataContext.Path]);
                    break;
            }
        }

        return nextOption;
    }

    private async Task<Option> OnAppend(DataPipelineContext dataContext, DataETag data, ScopeContext context)
    {
        var isValidOption = await IsCacheIsValid(dataContext, context);
        if (isValidOption.IsError()) return isValidOption;

        var detailsOption = await _lockManager.GetReadWriteAccess(dataContext.Path, context).Append(data, context);
        if (detailsOption.IsError())
        {
            context.LogDebug("Fail to append to path={path} from file store", dataContext.Path);
            Counters.AddAppendFailCount();
            return detailsOption.ToOptionStatus();
        }

        Counters.AddAppendCount();
        return StatusCode.OK;
    }

    private async Task<Option> OnDelete(string path, ScopeContext context)
    {
        context.LogDebug("Deleting path={path} provider cache", path);

        var deleteOption = await _fileStore.File(path).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete path={path} from file store", path);
            Counters.AddDeleteFailCount();
            return deleteOption;
        }

        Counters.AddDeleteCount();
        return StatusCode.OK;
    }

    private async Task<Option<DataPipelineContext>> OnGet(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting path={path} from file store", dataContext.Path);

        var isValidOption = await IsCacheIsValid(dataContext, context);
        if (isValidOption.IsError()) return isValidOption.ToOptionStatus<DataPipelineContext>();

        var readOption = await _fileStore.File(dataContext.Path).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read path={path} from file store", dataContext.Path);
            return StatusCode.NotFound;
        }

        Counters.AddHits();
        context.LogDebug("Found path={path} in file store cache", dataContext.Path);

        dataContext = dataContext with { GetData = [readOption.Return()] };
        return dataContext;
    }

    private async Task<Option> OnSet(DataPipelineContext dataContext, DataETag data, ScopeContext context)
    {
        context.LogDebug("Setting path={path} to file store cache", dataContext.Path);

        var setOption = await _lockManager.GetReadWriteAccess(dataContext.Path, context).Set(data, context);
        if (setOption.IsOk())
        {
            Counters.AddSetCount();
        }
        else
        {
            Counters.AddSetFailCount();
            context.LogDebug(
                "Fail to write path={path} from file store, statusCode={statusCode}, error={error}",
                dataContext.Path, setOption.StatusCode, setOption.Error
                );
        }

        return setOption.ToOptionStatus();
    }

    internal async Task<Option> IsCacheIsValid(DataPipelineContext dataContext, ScopeContext context)
    {
        if (dataContext.PipelineConfig.FileCacheDuration == null) return StatusCode.OK;

        context.LogDebug("Check to see if path={path} to file store cache", dataContext.Path);
        var file = _fileStore.File(dataContext.Path);

        var detailsOption = await file.GetDetails(context);
        if (detailsOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read path={path} from file store", dataContext.Path);
            return StatusCode.NotFound;
        }

        TimeSpan age = DateTime.UtcNow - (detailsOption.Return().CreatedOn ?? DateTime.UtcNow);
        if (age > dataContext.PipelineConfig.FileCacheDuration)
        {
            context.LogDebug("File store cache is too old, path={path}, timeLimit={timeLimit}, age={age}",
                dataContext.Path, dataContext.PipelineConfig.FileCacheDuration, age);

            Counters.AddRetireCount();
            Counters.AddMisses();
            (await OnDelete(dataContext.Path, context)).LogStatus(context, "Deleting expired path={path}", [dataContext.Path]);
            return StatusCode.NotFound;
        }

        return StatusCode.OK;
    }
}
