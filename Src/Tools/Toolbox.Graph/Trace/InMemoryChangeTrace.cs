using System.Collections.Concurrent;
using Toolbox.Extensions;

namespace Toolbox.Graph;

public class InMemoryChangeTrace : IChangeTrace
{
    private readonly int _maxSize;
    private readonly ConcurrentQueue<string> _traces = new ConcurrentQueue<string>();
    private readonly object _lock = new object();

    public InMemoryChangeTrace(int maxSize = 100_000)
    {
        _maxSize = maxSize;
    }

    public void Log(ChangeTrx trx) => Enqueue(trx);

    public Task LogAsync(ChangeTrx trx)
    {
        Enqueue(trx);
        return Task.CompletedTask;
    }

    public int Count => _traces.Count;

    public IReadOnlyList<string> GetTraces()
    {
        lock (_lock)
        {
            return _traces.ToArray();
        }
    }

    private void Enqueue(ChangeTrx trx)
    {
        lock (_lock)
        {
            while (_traces.Count >= _maxSize) _traces.TryDequeue(out _);

            _traces.Enqueue(trx.ToJson());
        }
    }
}

