using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class FileStoreDataProvider : IDataProvider
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<FileStoreDataProvider> _logger;
    private readonly IOptions<DataPipelineOption> _option;
    private const string _name = nameof(FileStoreDataProvider);

    public FileStoreDataProvider(IFileStore fileStore, IOptions<DataPipelineOption> option, ILogger<FileStoreDataProvider> logger)
    {
        _fileStore = fileStore.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull();
        context = context.With(_logger);
        context.LogDebug("FileStoreDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
                var appendOption = await OnAppend(dataContext.Key, dataContext.SetData.First(), context).ConfigureAwait(false);
                if (appendOption.IsError()) return appendOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.Delete:
                await OnDelete(dataContext.Key, context);
                break;

            case DataPipelineCommand.Get:
                var getOption = await OnGet(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            case DataPipelineCommand.Set:
                var setOption = await OnSet(dataContext.Key, dataContext.SetData.First(), context);
                if (setOption.IsError()) return setOption.ToOptionStatus<DataPipelineContext>();
                break;

            default:
                context.LogError("Unknown command={command}, name={name}", dataContext.Command, _name);
                throw new ArgumentOutOfRangeException($"Unknown command '{dataContext.Command}'");
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);

        if (nextOption.IsOk())
        {
            switch (dataContext.Command)
            {
                case DataPipelineCommand.Get:
                    (await OnSet(dataContext.Key, nextOption.Return().GetData.First(), context))
                        .LogStatus(context, "Setting key={key} to file store cache, name={name}", [dataContext.Key, _name]);
                    break;
            }
        }

        return nextOption;
    }

    public async Task<Option> OnAppend(string key, DataETag data, ScopeContext context)
    {
        var isValidOption = await IsCacheIsValid(key, context);
        if (isValidOption.IsError()) return isValidOption;

        var detailsOption = await _fileStore.File(key).Append(data, context);
        if (detailsOption.IsError()) return detailsOption.ToOptionStatus<string>();

        Counters.AddAppendCount();
        return StatusCode.OK;
    }

    public async Task<Option> OnDelete(string key, ScopeContext context)
    {
        context.LogDebug("Deleting key={key} provider cache, name={name}", key, _name);

        var deleteOption = await _fileStore.File(key).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete key={key} from file store, name={name}", key, _name);
            Counters.AddDeleteFailCount();
            return deleteOption;
        }

        Counters.AddDeleteCount();
        return StatusCode.OK;
    }

    private async Task<Option<DataPipelineContext>> OnGet(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting key={key} from file store, name={name}", dataContext.Key, _name);

        var isValidOption = await IsCacheIsValid(dataContext.Key, context);
        if (isValidOption.IsError()) return isValidOption.ToOptionStatus<DataPipelineContext>();

        var readOption = await _fileStore.File(dataContext.Key).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read key={key} from file store, name={name}", dataContext.Key, _name);
            return StatusCode.NotFound;
        }

        Counters.AddHits();
        context.LogDebug("Found key={key} in file store cache, name={name}", dataContext.Key, _name);

        dataContext = dataContext with { GetData = [readOption.Return()] };
        return dataContext;
    }

    private async Task<Option> OnSet(string key, DataETag data, ScopeContext context)
    {
        context.LogDebug("Setting key={key} to file store cache, name={name}", key, _name);

        var setOption = await _fileStore.File(key).Set(data, context);
        if (setOption.IsOk())
        {
            Counters.AddSetCount();
        }
        else
        {
            Counters.AddSetFailCount();
            context.LogDebug("Fail to write key={key} from file store, name={name}", key, _name);
        }

        return setOption.ToOptionStatus();
    }

    internal async Task<Option> IsCacheIsValid(string key, ScopeContext context)
    {
        if (!_option.Value.FileCacheDuration.HasValue) return StatusCode.OK;

        context.LogDebug("Check to see if key={key} to file store cache, name={name}", key, _name);
        var file = _fileStore.File(key);

        var detailsOption = await file.GetDetails(context);
        if (detailsOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read key={key} from file store, name={name}", key, _name);
            return StatusCode.NotFound;
        }

        TimeSpan age = DateTime.UtcNow - (detailsOption.Return().CreatedOn ?? DateTime.UtcNow);
        if (age > _option.Value.FileCacheDuration.Value)
        {
            context.LogDebug("File store cache is too old, key={key}, name={name}, timeLimit={timeLimit}, age={age}", key, _name, _option.Value.FileCacheDuration, age);
            Counters.AddRetireCount();
            Counters.AddMisses();
            (await OnDelete(key, context)).LogStatus(context, "Deleting expired file={file} from name={name}", [key, _name]);
            return StatusCode.NotFound;
        }

        return StatusCode.OK;
    }
}
