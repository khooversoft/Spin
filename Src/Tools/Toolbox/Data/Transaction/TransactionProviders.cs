using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionProviders
{
    private readonly ConcurrentDictionary<string, ITrxProvider> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly Transaction _parent;
    private readonly Action _testOpen;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public TransactionProviders(Transaction parent, Action testOpen, ILogger logger)
    {
        _parent = parent.NotNull();
        _testOpen = testOpen.NotNull();
        _logger = logger;
    }

    public int Count => _dict.Count;

    public void Enlist(ITrxProvider provider)
    {
        provider.NotNull();
        _testOpen();
        _dict.TryAdd(provider.SourceName, provider).BeTrue($"Provider with sourceName={provider.SourceName} is already enlisted");

        provider.AttachRecorder(_parent.TrxRecorder);
    }

    public void Delist(ITrxProvider provider)
    {
        _testOpen();
        provider.NotNull();
        _dict.TryRemove(provider.SourceName, out var _).BeTrue($"Provider with sourceName={provider.SourceName} is not enlisted");

        lock (provider)
        {
            provider.DetachRecorder();
        }
    }

    public void DelistAll()
    {
        _testOpen();

        lock (_dict)
        {
            var providers = _dict.Values.ToList();
            foreach (var provider in providers) Delist(provider);
        }
    }

    internal async Task<Option> Start()
    {
        await _gate.WaitAsync();

        try
        {
            foreach (var action in _dict.Values)
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
        finally
        {
            _gate.Release();
        }
    }

    internal async Task<Option> Commit()
    {
        await _gate.WaitAsync();

        try
        {
            foreach (var action in _dict.Values)
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
        finally
        {
            _gate.Release();
        }
    }

    internal async Task<Option> Rollback(DataChangeEntry entry)
    {
        await _gate.WaitAsync();

        try
        {
            if (!_dict.TryGetValue(entry.SourceName, out var provider)) return StatusCode.OK;

            var result = await provider.Rollback(entry);
            if (result.IsError())
            {
                LogError("Failed rollback source for transaction", entry.SourceName, result);
                return result;
            }

            return StatusCode.OK;
        }
        finally
        {
            _gate.Release();
        }
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
