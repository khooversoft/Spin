using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;


public class JournalStoreDataProvider : IDataProvider
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<JournalStoreDataProvider> _logger;
    private const string _name = nameof(FileStoreDataProvider);

    public JournalStoreDataProvider(IFileStore fileStore, ILogger<JournalStoreDataProvider> logger)
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
        context.LogDebug("JournalStoreDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.AppendList:
                var appendOption = await OnAppendList(dataContext.Path, dataContext.SetData, context).ConfigureAwait(false);
                if (appendOption.IsError()) return appendOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.Delete:
                await OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.GetList:
                var getOption = await OnGetList(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            default:
                context.LogError("Unknown command={command}, name={name}", dataContext.Command, _name);
                throw new ArgumentOutOfRangeException($"Unknown command '{dataContext.Command}'");
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

    public async Task<Option> OnDelete(string path, ScopeContext context)
    {
        context.LogDebug("Delete path={path}, name={name}", path, _name);

        var deleteOption = await _fileStore.File(path).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete path={path}, name={name}", path, _name);
            Counters.AddDeleteFailCount();
            return deleteOption;
        }

        Counters.AddDeleteCount();
        return StatusCode.OK;
    }

    private async Task<Option<DataPipelineContext>> OnGetList(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting path={path}, name={name}", dataContext.Path, _name);

        var readOption = await _fileStore.File(dataContext.Path).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read path={path}, name={name}", dataContext.Path, _name);
            return StatusCode.NotFound;
        }

        var stringData = readOption.Return().DataToString();

        var dataItems = stringData
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToDataETag())
            .ToImmutableList();

        Counters.AddHits();
        context.LogDebug("Found path={path}, name={name}", dataContext.Path, _name);

        dataContext = dataContext with { GetData = dataItems };
        return dataContext;
    }
}