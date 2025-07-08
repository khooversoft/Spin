using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class CacheMemoryHandler : IDataProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheMemoryHandler> _logger;
    private const string _name = nameof(CacheMemoryHandler);

    public CacheMemoryHandler(IMemoryCache memoryCache, ILogger<CacheMemoryHandler> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        dataContext.NotNull().Validate().ThrowOnError();
        dataContext.PipelineConfig.MemoryCacheDuration.Assert(x => x != null && x > TimeSpan.Zero, "MemoryCacheDuration must be set to a valid value");
        context = context.With(_logger);
        context.LogDebug("CacheMemoryDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        dataContext.NotNull();
        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
                OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.Delete:
                OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.Get:
                var getOption = OnGet(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            case DataPipelineCommand.AppendList:
                OnDelete(dataContext.Path, context);
                break;

            case DataPipelineCommand.GetList:
                var getListOption = OnGet(dataContext, context);
                if (getListOption.IsOk()) return getListOption;
                break;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);

        if (nextOption.IsOk())
        {
            var dc = nextOption.Return();

            switch (dataContext.Command)
            {
                case DataPipelineCommand.Set:
                    OnSet(dataContext, dc.SetData.First(), context);
                    break;

                case DataPipelineCommand.Get:
                    var getData = dc.GetData.First();
                    OnSet(dataContext, getData, context);
                    break;

                case DataPipelineCommand.GetList:
                    OnSet(dataContext, dc.GetData, context);
                    break;
            }
        }

        return nextOption;
    }

    private void OnDelete(string key, ScopeContext context)
    {
        context.LogDebug("Deleting key={key} from in-memory cache", key);

        _memoryCache.Remove(key);
        Counters.AddDeleteCount();
    }

    private Option<DataPipelineContext> OnGet(DataPipelineContext dataContext, ScopeContext context)
    {
        context.LogDebug("Getting key={key} from in-memory cache", dataContext.Path);

        if (_memoryCache.TryGetValue(dataContext.Path, out DataETag value))
        {
            Counters.AddHits();
            context.LogDebug("Found key={key} in in-memory cache", dataContext.Path);
            dataContext = dataContext with { GetData = [value] };
            return dataContext;
        }

        Counters.AddMisses();
        context.LogDebug("Not found key={key} in in-memory cache", dataContext.Path);

        return StatusCode.NotFound;
    }

    private void OnSet(DataPipelineContext dataContext, object data, ScopeContext context)
    {
        context.LogDebug("Setting key={key} from in-memory cache", dataContext.Path);

        var cacheOption = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = dataContext.PipelineConfig.MemoryCacheDuration,
        };

        _memoryCache.Set(dataContext.Path, data.NotNull(), cacheOption);
        Counters.AddSetCount();
    }
}
