using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Data;

public class CheckpointProviders
{
    private readonly ConcurrentDictionary<string, ICheckpoint> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly Transaction _parent;
    private readonly Action _testOpen;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public CheckpointProviders(Transaction parent, Action testOpen, ILogger logger)
    {
        _parent = parent.NotNull();
        _testOpen = testOpen;
        _logger = logger;
    }

    public int Count => _dict.Count;

    public void Attach(string name, ICheckpoint p)
    {
        p.NotNull();
        _testOpen();
        _dict.TryAdd(name, p).BeTrue($"Checkpoint with name={name} is already enlisted");
    }

    public void Detach(string name)
    {
        _testOpen();
        name.NotEmpty();
        _dict.TryRemove(name, out var _).BeTrue($"Checkpoint with name={name} is not enlisted");
    }

    public void DetachAll()
    {
        _testOpen();

        lock (_dict)
        {
            var checkpointNames = _dict.Keys.ToList();
            foreach (var checkpointName in checkpointNames) Detach(checkpointName);
        }
    }

    public async Task Checkpoint()
    {
        _testOpen();
        await _gate.WaitAsync();

        try
        {
            foreach(var item in _dict)
            {
                _logger.LogDebug("Checkpointing provider name={name}", item.Key);
                await item.Value.Checkpoint();
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}
