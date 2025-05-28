using System.Buffers;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketSearchClient
{
    public const string _prefixPath = "config/TicketData";
    public const string _cacheKey = nameof(TicketSearchClient);

    private readonly IFileStore _fileStore;
    private readonly ILogger<TicketSearchClient> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };
    private readonly TicketMasterClient _ticketMasterClient;
    private readonly TicketOption _ticketOption;

    public TicketSearchClient(IMemoryCache memoryCache, IFileStore fileStore, TicketMasterClient ticketMasterClient, TicketOption ticketOption, ILogger<TicketSearchClient> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _fileStore = fileStore.NotNull();
        _ticketMasterClient = ticketMasterClient.NotNull();
        _ticketOption = ticketOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<ClassificationRecord>> GetClassifications(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting classification data from TicketMasterClient");

        var search = new TicketMasterSearch(TicketSearchType.Classification, _ticketOption, "classification");
        var option = await InternalGet<ClassificationRecord>(search, context);
        return option;
    }

    private async Task<Option<T>> InternalGet<T>(TicketMasterSearch search, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting data for searchName={searchName}", search.SearchName);

        var cacheOption = _memoryCache.Get<T>(CreateCacheKey(search.SearchName), context);
        if (cacheOption.IsOk()) return cacheOption;

        context.LogDebug("Cache miss, reading from file store");
        var readOption = await ReadFromFile<T>(search.SearchName, context);
        if (readOption.IsOk())
        {
            context.LogDebug("Read from file store, writing to cache");
            _memoryCache.Set(CreateCacheKey(search.SearchName), readOption.Return(), context);
            return readOption;
        }

        context.LogDebug("File store read failed, getting from TicketClassificationClient");
        var subjectOption = await _ticketMasterClient.Get<T>(search, context);
        if (subjectOption.IsError())
        {
            context.LogError("Failed to get data from TicketMasterClient, searchName={searchName}, error={error}", search.SearchName, subjectOption.Error);
            return subjectOption;
        }

        context.LogDebug("Writing classification to file store and cache, searchName={searchName}", search.SearchName);
        var writeOption = await WriteToFile<T>(search.SearchName, subjectOption.Return(), context);
        if (writeOption.IsError())
        {
            context.LogError("Failed to write classification data to file store, searchName={searchName}, error={error}", search.SearchName, writeOption.Error);
            return writeOption.ToOptionStatus<T>();
        }

        var data = subjectOption.Return();
        _memoryCache.Set(CreateCacheKey(search.SearchName), data, context);
        return data;
    }

    public async Task<Option> CleatData(string searchName, ScopeContext context)
    {
        context = context.With(_logger);
        _memoryCache.Remove(_cacheKey);

        var result = await ClearFile(searchName, context);
        result.LogStatus(context, "Clear ticket data model, searchName={searchName}", [searchName]);

        return result;
    }

    private static string CreateCacheKey(string searchName) => $"{_cacheKey}/{searchName.NotEmpty()}";

    private static string CreateFilePath(string searchName) => $"{_prefixPath}/{searchName.NotEmpty()}.json";

    private async Task<Option<T>> ReadFromFile<T>(string searchName, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Reading from file store, search={search}", searchName);

        IFileAccess fileAccess = _fileStore.File(CreateFilePath(searchName));

        var fileDetails = await fileAccess.GetDetails(context);
        if (fileDetails.IsError())
        {
            context.LogWarning("Cannot get file details for searchName={searchName}, error={error}", searchName, fileDetails.Error);
            return fileDetails.ToOptionStatus<T>();
        }

        if (fileDetails.Return().LastModified < DateTimeOffset.UtcNow.AddMinutes(-30))
        {
            (await fileAccess.Delete(context)).LogStatus(context, "Delete expired file={file}", [fileAccess.Path]);
            context.LogDebug("File for searchName={searchName} is older than 30 minutes, skipping read", searchName);
            return StatusCode.NotFound;
        }

        var getOption = await fileAccess.Get(context);
        if (getOption.IsError())
        {
            context.LogWarning("Canot read file for searchName={searchName}, error={error}", searchName, getOption.Error);
            return getOption.ToOptionStatus<T>();
        }

        var result = getOption.Return().ToObject<T>();
        return result;
    }

    private async Task<Option> WriteToFile<T>(string searchName, T subject, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Writing to file store, search={search}", searchName);

        DataETag dataETag = subject.ToJson().ToDataETag();
        var setOption = await _fileStore.File(CreateFilePath(searchName)).Set(dataETag, context);
        if (setOption.IsError()) context.LogWarning("Cannot write file for searchName={searchName}, error={error}", searchName, setOption.Error);

        return setOption.ToOptionStatus();
    }

    private async Task<Option> ClearFile(string searchName, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Clearing file store for searchName={searchName}", searchName);

        var deleteOption = await _fileStore.File(CreateFilePath(searchName)).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogWarning("Cannot delete file for searchName={searchName}, error={error}", searchName, deleteOption.Error);
        }

        return deleteOption;
    }
}
