using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        dataContext.NotNull();
        context = context.With(_logger);
        context.LogDebug("JournalStoreDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.AppendList:
                var appendOption = await OnAppendList(dataContext.Key, dataContext.SetData, context).ConfigureAwait(false);
                if (appendOption.IsError()) return appendOption.ToOptionStatus<DataPipelineContext>();
                break;

            case DataPipelineCommand.Delete:
                await OnDelete(dataContext.Key, context);
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

    public async Task<Option> OnAppendList(string key, IReadOnlyList<DataETag> dataItems, ScopeContext context)
    {
        dataItems.NotNull().Assert(x => x.Count > 0, _ => "Empty list");

        string json = dataItems.Aggregate(string.Empty, (a, x) => a += x.DataToString() + Environment.NewLine);
        DataETag data = json.ToDataETag();

        var detailsOption = await _fileStore.File(key).Append(data, context);
        if (detailsOption.IsError())
        {
            Counters.AddAppendFailCount();
            return detailsOption.ToOptionStatus<string>();
        }

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

    private async Task<Option<DataPipelineContext>> OnGetList(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting key={key} from file store, name={name}", dataContext.Key, _name);

        var readOption = await _fileStore.File(dataContext.Key).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read key={key} from file store, name={name}", dataContext.Key, _name);
            return StatusCode.NotFound;
        }

        var stringData = readOption.Return().DataToString();

        var dataItems = stringData
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToDataETag())
            .ToImmutableList();

        Counters.AddHits();
        context.LogDebug("Found key={key} in file store cache, name={name}", dataContext.Key, _name);

        dataContext = dataContext with { GetData = dataItems };
        return dataContext;
    }
}