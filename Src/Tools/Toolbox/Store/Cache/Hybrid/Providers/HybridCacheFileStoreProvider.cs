using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class HybridCacheFileStoreProvider : IHybridCacheProvider
{
    private readonly IFileStore _fileStore;
    private readonly ILogger<HybridCacheFileStoreProvider> _logger;
    private readonly IOptions<HybridCacheOption> _option;

    public HybridCacheFileStoreProvider(IFileStore fileStore, IOptions<HybridCacheOption> option, ILogger<HybridCacheFileStoreProvider> logger)
    {
        _fileStore = fileStore.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public string Name => nameof(HybridCacheFileStoreProvider);
    public HybridCacheCounters Counters { get; } = new();

    public async Task<Option<string>> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);

        var detailsOption = await _fileStore.File(key).GetDetails(context);
        if (detailsOption.IsError()) return detailsOption.ToOptionStatus<string>();

        if (detailsOption.Return().CreatedOn < DateTime.UtcNow - _option.Value.FileCacheDuration)
        {
            return StatusCode.NotFound;
        }

        return Name;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Deleting key={key} from hybrid provider cache, name={name}", key, Name);

        var deleteOption = await _fileStore.File(key).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete key={key} from file store, name={name}", key, Name);
            return deleteOption;
        }

        Counters.AddDeleteCount();
        return StatusCode.OK;
    }

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting key={key} from file store cache, name={name}", key, Name);

        var file = _fileStore.File(key);

        var detailsOption = await file.GetDetails(context);
        if (detailsOption.IsError()) return detailsOption.ToOptionStatus<T>();

        if (detailsOption.Return().CreatedOn < DateTime.UtcNow - _option.Value.FileCacheDuration)
        {
            Counters.AddRetireCount();
            Counters.AddMisses();
            (await file.Delete(context)).LogStatus(context, "Deleting expired file={file} from name={name}", [key, Name]);
            context.LogDebug("File store cache is too old, key={key}, name={name}", key, Name);
            return new Option<T>(StatusCode.NotFound);
        }

        var readOption = await _fileStore.File(key).Get(context);
        if (readOption.IsError())
        {
            Counters.AddMisses();
            context.LogDebug("Fail to read key={key} from file store, name={name}", key, Name);
            return readOption.ToOptionStatus<T>();
        }

        Counters.AddHits();
        var data = readOption.Return();
        var subject = data.ToObject<T>();
        return subject;
    }

    public async Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Setting key={key} to file store cache, name={name}", key, Name);

        var data = value.ToJson().ToDataETag();
        var setOption = await _fileStore.File(key).Set(data, context);
        if (setOption.IsError())
        {
            Counters.AddSetFailCount();
            context.LogDebug("Fail to write key={key} from file store, name={name}", key, Name);
            return setOption.ToOptionStatus();
        }

        Counters.AddSetCount();
        return setOption.ToOptionStatus();
    }
}
