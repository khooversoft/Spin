using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class DataClientHandler : IDataClient
{
    private readonly IDataProvider _provider;
    private readonly ILogger<DataClientHandler> _logger;

    public DataClientHandler(IDataProvider provider, ILogger<DataClientHandler> logger)
    {
        _provider = provider.NotNull();
        _logger = logger.NotNull();
    }

    public DataClientHandler? InnerHandler { get; set; }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        await _provider.Delete(key, context).ConfigureAwait(false);

        if (InnerHandler == null) return StatusCode.OK;
        return await InnerHandler.Delete(key, context);
    }

    public async Task<Option<string>> Exists(string key, ScopeContext context)
    {
        var existOption = await _provider.Exists(key, context).ConfigureAwait(false);
        if (existOption.IsOk()) return existOption;

        if (InnerHandler == null) return StatusCode.NotFound;
        return await InnerHandler.Exists(key, context);
    }

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        var getOption = await _provider.Get<T>(key, context).ConfigureAwait(false);
        if (getOption.IsOk()) return getOption;

        if (InnerHandler == null) return StatusCode.NotFound;
        var innerGetOption = await InnerHandler.Get<T>(key, context);
        if (innerGetOption.IsError()) return innerGetOption;

        var value = innerGetOption.Return().NotNull();
        var setOption = await _provider.Set<T>(key, value, context).ConfigureAwait(false);
        if (setOption.IsError()) return setOption.ToOptionStatus<T>();

        return value;
    }

    public async Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        var setOption = await _provider.Set<T>(key, value, context).ConfigureAwait(false);
        if (setOption.IsError()) return setOption;

        if (InnerHandler == null) return StatusCode.OK;
        return await InnerHandler.Set<T>(key, value, context);
    }
}
