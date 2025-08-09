using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools;

public class BatchStream<T> : IAsyncDisposable
{
    private readonly TimeSpan _batchInterval;
    private readonly Func<IReadOnlyList<T>, Task> _forward;
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private readonly int _maxBatchSize;
    private readonly ILogger<BatchStream<T>> _logger;
    private DateTime _lastBatchTime = DateTime.Now;


    public BatchStream(TimeSpan batchInterval, int maxBatchSize, Func<IReadOnlyList<T>, Task> forward, ILogger<BatchStream<T>> logger)
    {
        _batchInterval = batchInterval.Assert(x => x.TotalMilliseconds > 0, "Minimum batch interval must be greater than zero");
        _forward = forward.NotNull();
        _maxBatchSize = maxBatchSize.Assert(x => x > 1, "Minimum batch size is 1");
        _logger = logger.NotNull();

        _timer = new PeriodicTimer(_batchInterval);
        _ = StartAsyncLoop();
    }

    public async Task Send(T item)
    {
        await _lock.WaitAsync();
        try
        {
            _queue.Enqueue(item);

            var dataTest = (DateTime.Now - _lastBatchTime) >= _batchInterval;
            var sizeTest = _queue.Count >= _maxBatchSize;

            if (dataTest || sizeTest)
            {
                _logger.LogDebug("Batch conditions met, draining immediately: dateTest={dateTest}, sizeTest={sizeTest}", dataTest, sizeTest);
                await InternalDrain();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task Drain()
    {
        await _lock.WaitAsync();
        try
        {
            await InternalDrain();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task Stop()
    {
        _logger.LogDebug("Stopping");
        _cts.Cancel();
        _timer.Dispose();

        await Drain();
    }

    public async ValueTask DisposeAsync() => await Stop();

    private async Task StartAsyncLoop()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                await Drain();
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    private async Task InternalDrain()
    {
        if (_queue.Count == 0) return;

        var forwardList = _queue.ToArray();
        _queue.Clear();
        _logger.LogDebug("Draining batch: count={count}, lastBatchTime={lastBatchTime}", forwardList.Length, _lastBatchTime);

        _lastBatchTime = DateTime.Now;
        await _forward(forwardList);
    }
}
