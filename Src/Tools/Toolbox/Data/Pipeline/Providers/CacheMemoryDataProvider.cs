using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class CacheMemoryDataProvider : IDataProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheMemoryDataProvider> _logger;
    private readonly IOptions<DataPipelineOption> _option;
    private const string _name = nameof(CacheMemoryDataProvider);

    public CacheMemoryDataProvider(IMemoryCache memoryCache, IOptions<DataPipelineOption> option, ILogger<CacheMemoryDataProvider> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("CacheMemoryDataProvider: Executing command={command}, name={name}", dataContext.Command, _name);

        dataContext.NotNull();
        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
                OnDelete(dataContext.Key, context);
                break;

            case DataPipelineCommand.Delete:
                OnDelete(dataContext.Key, context);
                break;

            case DataPipelineCommand.Get:
                var getOption = OnGet(dataContext, context);
                if (getOption.IsOk()) return getOption;
                break;

            case DataPipelineCommand.Set:
                OnSet(dataContext.Key, dataContext.SetData.First(), context);
                break;

            case DataPipelineCommand.AppendList:
                OnDelete(dataContext.Key, context);
                break;

            case DataPipelineCommand.GetList:
                var getListOption = OnGet(dataContext, context);
                if (getListOption.IsOk()) return getListOption;
                break;

            default:
                context.LogError("Unknown command={command}, name={name}", dataContext.Command, _name);
                throw new ArgumentOutOfRangeException($"Unknown command '{dataContext.Command}'");

        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);

        if (nextOption.IsOk())
        {
            var data = nextOption.Return().GetData;

            switch (dataContext.Command)
            {
                case DataPipelineCommand.Get:
                    var getData = data.First();
                    OnSet(dataContext.Key, getData, context);
                    break;

                case DataPipelineCommand.GetList:
                    OnSet(dataContext.Key, data, context);
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
        context.LogDebug("Getting key={key} from in-memory cache", dataContext.Key);

        if (_memoryCache.TryGetValue(dataContext.Key, out DataETag value))
        {
            Counters.AddHits();
            context.LogDebug("Found key={key} in in-memory cache", dataContext.Key);
            dataContext = dataContext with { GetData = [value] };
            return dataContext;
        }

        Counters.AddMisses();
        context.LogDebug("Not found key={key} in in-memory cache", dataContext.Key);

        return StatusCode.NotFound;
    }

    private void OnSet(string key, object data, ScopeContext context)
    {
        context.LogDebug("Setting key={key} from in-memory cache", key);

        var cacheOption = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _option.Value.MemoryCacheDuration,
        };

        _memoryCache.Set(key, data.NotNull(), cacheOption);
        Counters.AddSetCount();
    }
}
