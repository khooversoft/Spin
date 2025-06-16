using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataClient<T> : IDataClient<T>
{
    private readonly IDataClient _handler;
    private readonly ILogger<DataClient<T>> _logger;

    public DataClient(IDataClient handler, ILogger<DataClient<T>> logger)
    {
        _handler = handler.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var deleteOption = await _handler.Delete(key, context);
        if (deleteOption.IsError())
        {
            _logger.LogDebug("Cannot delete key={key} in hybrid cache", key);
            return deleteOption;
        }

        return deleteOption;
    }

    public Task<Option<string>> Exists(string key, ScopeContext context) => _handler.Exists(key, context);

    public async Task<Option<T>> Get(string key, object? state, ScopeContext context)
    {
        context = context.With(_logger);

        var getOption = await _handler.Get<T>(key, state, context).ConfigureAwait(false);
        if (getOption.IsError())
        {
            _logger.LogDebug("Not key={key} in hybrid cache", key);
            return getOption;
        }

        var value = getOption.Return().NotNull().Cast<T>();
        return value;
    }

    public async Task<Option> Set(string key, T value, object? state, ScopeContext context)
    {
        context = context.With(_logger);

        var setOption = await _handler.Set<T>(key, value, state, context).ConfigureAwait(false);
        if (setOption.IsError())
        {
            _logger.LogDebug("Cannot set key={key} in hybrid cache", key);
            return setOption;
        }

        return setOption;
    }
}
