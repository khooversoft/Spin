using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools;

/// <summary>
/// A bounded, single-consumer queue that executes enqueued work items sequentially
/// on a background task. Supports concurrent producers. Use <see cref="Send"/> for
/// fire-and-forget work, <see cref="Get{T}"/> for work that returns a value, and
/// <see cref="Drain"/> / <see cref="Complete"/> to flush or stop processing.
/// </summary>
public class SequentialAsyncQueue : IAsyncDisposable
{
    private enum RunState { None, Run, Stopped }

    private readonly Channel<Func<Task>> _channel;
    private readonly ILogger<SequentialAsyncQueue> _logger;
    private readonly Task _processingTask;
    private volatile RunState _runState;
    private long _getEnqueueCount;
    private long _getExecuteCount;
    private long _sendEnqueueCount;
    private long _processCount;

    /// <summary>
    /// Create a sequential queue with a bounded capacity. When the channel is full,
    /// producers will asynchronously wait. The queue starts its background processor immediately.
    /// </summary>
    /// <param name="boundedCapacity">Maximum number of pending work items.</param>
    /// <param name="logger">Logger used for diagnostics and timing scopes.</param>
    public SequentialAsyncQueue(int boundedCapacity, ILogger<SequentialAsyncQueue> logger)
    {
        _logger = logger.NotNull();

        _channel = Channel.CreateBounded<Func<Task>>(new BoundedChannelOptions(boundedCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false, // multiple producers can call Send/Get concurrently
        });

        _runState = RunState.Run;
        _processingTask = Task.Run(() => ProcessQueueAsync());
    }

    /// <summary>
    /// Stop accepting new work, complete the channel, and wait for all queued work to finish.
    /// Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public async Task Complete()
    {
        var currentState = Interlocked.CompareExchange(ref _runState, RunState.Stopped, RunState.Run);
        if (currentState == RunState.Stopped) return;

        _logger.LogDebug("Completing operations");

        // Signal shutdown and drain remaining operations.
        _channel.Writer.Complete();

        await _processingTask;
    }

    /// <summary>
    /// Enqueue a barrier marker and wait until all work before it has executed.
    /// Throws if the queue is not running.
    /// </summary>
    public async Task Drain()
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        _logger.LogDebug("Enqueuing drain marker");

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<Task> wrapper = () =>
        {
            try
            {
                _logger.LogDebug("Processing drain marker");
                tcs.SetResult();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing drain marker");
                tcs.SetException(ex);
                return Task.CompletedTask;
            }
        };

        await _channel.Writer.WriteAsync(wrapper);
        await tcs.Task;
    }

    /// <summary>
    /// Enqueue a function that returns a value. The function executes in order relative
    /// to other enqueued work. Throws if the queue is stopped.
    /// </summary>
    public async Task<T> Get<T>(Func<Task<T>> readOperation)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        _logger.LogDebug("Enqueuing reader (get)");

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<Task> wrapper = async () =>
        {
            try
            {
                _logger.LogDebug("Processing ReadOperation");
                using (var timeScope = _logger.LogDuration(nameof(Get), "ReadOperation - timing"))
                {
                    var result = await readOperation();
                    tcs.SetResult(result);
                    Interlocked.Increment(ref _getExecuteCount);
                }

                LogStats();
                _logger.LogDebug("Get readOperation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing enqueued reader");
                tcs.SetException(ex);
            }
        };

        await _channel.Writer.WriteAsync(wrapper);
        Interlocked.Increment(ref _getEnqueueCount);
        LogStats();

        return await tcs.Task;
    }

    /// <summary>
    /// Enqueue a fire-and-forget task to be executed sequentially. Throws if the queue is stopped.
    /// </summary>
    public async Task Send(Func<Task> sendOperation)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        _logger.LogDebug("Enqueue writer (send)");

        await _channel.Writer.WriteAsync(sendOperation);
        Interlocked.Increment(ref _sendEnqueueCount);
        LogStats();
    }

    private async Task ProcessQueueAsync()
    {
        _logger.LogDebug("Starting background processing of channel");

        try
        {
            await foreach (var operation in _channel.Reader.ReadAllAsync())
            {
                try
                {
                    _logger.LogDebug("OperationQueue: Process operations");
                    using (var timeScope = _logger.LogDuration(nameof(ProcessQueueAsync), "OperationQueue: Process operations - timing"))
                    {
                        Interlocked.Increment(ref _processCount);
                        await operation();
                        LogStats();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Process operations failed");
                }
            }

            _logger.LogDebug("Process operations completed - channel closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ProcessQueueAsync");
        }
    }

    /// <summary>
    /// Dispose pattern entry point; waits for queued work to complete.
    /// </summary>
    public async ValueTask DisposeAsync() => await Complete();

    private void LogStats() => _logger.LogDebug(
        "OperationQueue Stats: GetEnqueue={getEnqueue}, GetExecute={getExecute}, SendEnqueue={sendEnqueue}, ProcessCount={processCount}",
        _getEnqueueCount,
        _getExecuteCount,
        _sendEnqueueCount,
        _processCount
        );
}
