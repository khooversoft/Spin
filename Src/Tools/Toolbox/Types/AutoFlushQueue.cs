using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;

namespace Toolbox.Types;

public class AutoFlushQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly int _bufferSize;
    private readonly TimeSpan _flushInterval;
    private readonly ActionBlock<IReadOnlyList<T>> _writer;
    private readonly Timer _timer;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public AutoFlushQueue(int bufferSize, TimeSpan flushInterval, Func<IReadOnlyList<T>, Task> writeAction)
    {
        _bufferSize = bufferSize;
        _flushInterval = flushInterval;

        _writer = new ActionBlock<IReadOnlyList<T>>(writeAction);
        _timer = new Timer(FlushBufferFromTimer, null, _flushInterval, _flushInterval);
    }

    public async Task Enqueue(IEnumerable<T> items, ScopeContext context) => await items.ForEachAsync(async x => await Enqueue(x, context));

    public async Task Enqueue(T item, ScopeContext context)
    {
        await _semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            _queue.Enqueue(item);
            if (_queue.Count >= _bufferSize) await FlushBuffer();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task FlushBuffer()
    {
        if (_queue.Count == 0) return;

        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            var list = _queue.ToImmutableArray();
            _queue.Clear();
            await _writer.SendAsync(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task Complete()
    {
        _writer.Complete();
        await _writer.Completion;
    }

    private async void FlushBufferFromTimer(object? _) => await FlushBuffer();
}
