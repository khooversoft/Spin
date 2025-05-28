//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketApi.sdk;

//public class TicketDataClient
//{
//    public const string Path = "config/TicketData.json";
//    public const string _cacheKey = nameof(TicketDataClient);

//    private readonly IFileStore _fileStore;
//    private readonly ILogger<TicketDataClient> _logger;
//    private readonly IMemoryCache _memoryCache;
//    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };

//    public TicketDataClient(IMemoryCache memoryCache, IFileStore fileStore, ILogger<TicketDataClient> logger)
//    {
//        _memoryCache = memoryCache.NotNull();
//        _fileStore = fileStore.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<Option<string>> Get(string queryId, ScopeContext context)
//    {
//        context = context.With(_logger);

//        var cacheOption = GetFromCache(context);
//        if (cacheOption.IsOk()) return cacheOption;

//        Option<DataETag> dataETag = await _fileStore.File(Path).Get(context);
//        dataETag.LogStatus(context, "Get ticket data model, path={path}", [Path]);
//        if (dataETag.IsError()) return dataETag.ToOptionStatus<TicketDataRecord>();

//        TicketDataRecord result = dataETag.Return().ToObject<TicketDataRecord>();
//        WriteToCache(result, context);
//        return result;
//    }

//    public async Task<Option> CleatData(ScopeContext context)
//    {
//        context = context.With(_logger);
//        _memoryCache.Remove(_cacheKey);

//        var result = await _fileStore.File(Path).Delete(context);
//        result.LogStatus(context, "Clear ticket data model, path={path}", [Path]);
//        if (result.IsError()) return result;

//        return result;
//    }

//    public async Task<Option<string>> Set(TicketDataRecord ticketDataModel, ScopeContext context)
//    {
//        context = context.With(_logger);

//        var result = await _fileStore.File(Path).Set(ticketDataModel.ToJson().ToDataETag(), context);
//        result.LogStatus(context, "Set ticket data model, path={path}", [Path]);
//        if (result.IsError()) return result;

//        WriteToCache(ticketDataModel, context);
//        return result;
//    }

//    private Option<TicketDataRecord> GetFromCache(ScopeContext context)
//    {
//        if (!_memoryCache.TryGetValue<TicketDataRecord>(_cacheKey, out var cachedValue)) return StatusCode.NotFound;

//        context.LogTrace("Read from cache");
//        return cachedValue.NotNull();
//    }

//    private void WriteToCache(TicketDataRecord subject, ScopeContext context)
//    {
//        _memoryCache.Set(_cacheKey, subject, _memoryOptions);
//        context.LogTrace("Write to cache");
//    }
//}
