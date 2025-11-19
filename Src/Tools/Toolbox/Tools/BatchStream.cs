using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools;

/// <summary>
/// Batches items pushed via <see cref="Send"/> and forwards them to a consumer delegate
/// at a maximum frequency (<see cref="_batchInterval"/>) or when the batch reaches
/// <see cref="_maxBatchSize"/>. A periodic timer also triggers draining to ensure
/// items are forwarded even under low traffic.
/// </summary>
/// <typeparam name="T">Item type to batch.</typeparam>
public class BatchStream<T> : IAsyncDisposable
{
    private readonly TimeSpan _batchInterval;
    private readonly Func<IReadOnlyList<T>, Task> _forward;
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private readonly int _maxBatchSize;
    private readonly ILogger<BatchStream<T>> _logger;
    private readonly Task _loopTask;

    // Track queue size without O(n) ConcurrentQueue.Count calls.
    private int _count;

    // Track last time we forwarded a batch (UTC to avoid clock skew issues).
    private DateTime _lastBatchTimeUtc = DateTime.UtcNow;

    /// <summary>
    /// Create a new <see cref="BatchStream{T}"/>.
    /// </summary>
    /// <param name="batchInterval">Minimum time between automatic batch flushes.</param>
    /// <param name="maxBatchSize">Maximum number of items per forwarded batch (minimum 1).</param>
    /// <param name="forward">Delegate invoked with each drained batch.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public BatchStream(TimeSpan batchInterval, int maxBatchSize, Func<IReadOnlyList<T>, Task> forward, ILogger<BatchStream<T>> logger)
    {
        _batchInterval = batchInterval.Assert(x => x.TotalMilliseconds > 0, "Minimum batch interval must be greater than zero");
        _forward = forward.NotNull();
        _maxBatchSize = maxBatchSize.Assert(x => x >= 1, "Minimum batch size is 1");
        _logger = logger.NotNull();

        _timer = new PeriodicTimer(_batchInterval);
        _loopTask = StartAsyncLoop();
    }

    /// <summary>
    /// Enqueue an item for batching. If either the time since the last batch
    /// exceeds <see cref="_batchInterval"/> or the queue size reaches
    /// <see cref="_maxBatchSize"/>, a drain is triggered.
    /// </summary>
    public async Task Send(T item)
    {
        bool shouldDrain = false;

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            _queue.Enqueue(item);
            _count++;

            var intervalElapsed = (DateTime.UtcNow - _lastBatchTimeUtc) >= _batchInterval;
            var sizeExceeded = _count >= _maxBatchSize;

            shouldDrain = intervalElapsed || sizeExceeded;

            if (shouldDrain)
            {
                _logger.LogDebug("Batch conditions met, draining: intervalElapsed={intervalElapsed}, sizeExceeded={sizeExceeded}, queued={queued}",
                    intervalElapsed, sizeExceeded, _count);
            }
        }
        finally
        {
            _lock.Release();
        }

        // Drain outside the lock to avoid blocking producers while forwarding.
        if (shouldDrain)
        {
            await Drain().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Drain the queue by forwarding batches of up to <see cref="_maxBatchSize"/>
    /// until the queue is empty.
    /// </summary>
    public async Task Drain()
    {
        while (true)
        {
            List<T> batch = [];

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_count == 0)
                {
                    return;
                }

                // Build a batch up to max size.
                while (batch.Count < _maxBatchSize && _queue.TryDequeue(out var item))
                {
                    batch.Add(item);
                    _count--;
                }

                if (batch.Count > 0)
                {
                    _logger.LogDebug("Draining batch: count={count}, queuedRemaining={remaining}, lastBatchTimeUtc={lastBatchTimeUtc}",
                        batch.Count, _count, _lastBatchTimeUtc);

                    _lastBatchTimeUtc = DateTime.UtcNow;
                }
            }
            finally
            {
                _lock.Release();
            }

            if (batch.Count == 0)
            {
                // Nothing to forward.
                return;
            }

            try
            {
                await _forward(batch).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                // Cancellation requested: stop draining.
                return;
            }
            catch (Exception ex)
            {
                // Log and continue to prevent the timer loop from dying.
                _logger.LogError(ex, "Error forwarding batch, dropping {count} items", batch.Count);
            }

            // Loop and attempt to drain any remaining items.
        }
    }

    /// <summary>
    /// Stop the batching loop and flush remaining items.
    /// </summary>
    public async Task Stop()
    {
        _logger.LogDebug("Stopping");
        _cts.Cancel();
        _timer.Dispose();

        // Flush any remaining items.
        await Drain().ConfigureAwait(false);

        // Observe loop task completion.
        try
        {
            await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected on stop.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in batching loop");
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Stop().ConfigureAwait(false);

    private async Task StartAsyncLoop()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false))
            {
                await Drain().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }
}
