using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionProviders
{
    private readonly ConcurrentDictionary<string, ITrxProvider> _enlistedProviders = new(StringComparer.OrdinalIgnoreCase);
    private readonly Transaction _parent;
    private readonly Func<bool> _isOpen;
    private readonly ILogger _logger;

    public TransactionProviders(Transaction parent, Func<bool> isOpen, ILogger logger)
    {
        _parent = parent.NotNull();
        _isOpen = isOpen;
        _logger = logger;
    }

    public int Count => _enlistedProviders.Count;

    public void Enlist(ITrxProvider provider)
    {
        provider.NotNull();
        _isOpen().BeTrue("Transaction is in progress");
        _enlistedProviders.TryAdd(provider.SourceName, provider).BeTrue($"Provider with sourceName={provider.SourceName} is already enlisted");

        provider.AttachRecorder(_parent.TrxRecorder);
    }

    public void Delist(ITrxProvider provider)
    {
        _isOpen().BeTrue("Transaction is not in progress");
        provider.NotNull();
        _enlistedProviders.TryRemove(provider.SourceName, out var _).BeTrue($"Provider with sourceName={provider.SourceName} is not enlisted");

        provider.DetachRecorder();
    }

    public void DelistAll()
    {
        _isOpen().BeTrue("Transaction is not in progress");

        var list = _enlistedProviders.Values.ToList();
        foreach (var provider in list) Delist(provider);
    }

    internal async Task<Option> Start()
    {
        foreach (var action in _enlistedProviders.Values)
        {
            var result = await action.Start();
            if (result.IsError())
            {
                LogError("Failed start action for transaction", action.SourceName, result);
                return result;
            }
        }

        return StatusCode.OK;
    }

    internal async Task<Option> Commit()
    {
        foreach (var action in _enlistedProviders.Values)
        {
            var result = await action.Commit();
            if (result.IsError())
            {
                LogError("Failed commit action for transaction", action.SourceName, result);
                return result;
            }
        }

        return StatusCode.OK;
    }

    internal async Task<Option> Rollback(DataChangeEntry entry)
    {
        if (!_enlistedProviders.TryGetValue(entry.SourceName, out var provider)) return StatusCode.OK;

        var result = await provider.Rollback(entry);
        if (result.IsError())
        {
            LogError("Failed rollback source for transaction", entry.SourceName, result);
            return result;
        }

        return StatusCode.OK;
    }

    private void LogError(string message, string sourceName, Option result)
    {
        _logger.LogError(
            message + " for sourceName={sourceName}, statusCode={statusCode}, error={error}",
            sourceName,
            result.StatusCode,
            result.Error
            );
    }
}
