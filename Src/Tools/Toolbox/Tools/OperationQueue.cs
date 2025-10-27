using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Tools;

public class OperationQueue : IAsyncDisposable
{
    private enum RunState { None, Run, Stopped }

    private readonly Channel<Func<Task>> _channel;
    private readonly ILogger<OperationQueue> _logger;
    private readonly Task _processingTask;
    private readonly CancellationTokenSource _cancelToken = new();
    private volatile RunState _runState;
    private long _getEnqueueCount;
    private long _getExecuteCount;
    private long _sendEnqueueCount;
    private long _processCount;

    public OperationQueue(int boundedCapacity, ILogger<OperationQueue> logger)
    {
        _logger = logger.NotNull();

        _channel = Channel.CreateBounded<Func<Task>>(new BoundedChannelOptions(boundedCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false, // multiple producers can call Send/Get concurrently
        });

        var processContext = _logger.ToScopeContext();
        _processingTask = Task.Run(() => ProcessQueueAsync(processContext));
        _runState = RunState.Run;
    }

    public async Task Complete(ScopeContext context)
    {
        var currentState = Interlocked.CompareExchange(ref _runState, RunState.Stopped, RunState.Run);
        if (currentState == RunState.Stopped) return;

        context.LogDebug("Completing operations");

        _cancelToken.Cancel();
        _channel.Writer.Complete();

        await _processingTask;
        _cancelToken.Dispose();
    }

    public async Task Drain(ScopeContext context)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        context = context.With(_logger);
        context.LogDebug("Enqueuing drain marker");

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<Task> wrapper = () =>
        {
            try
            {
                context.Location().LogDebug("Processing drain marker");
                tcs.SetResult();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                context.Location().LogError(ex, "Error processing drain marker");
                tcs.SetException(ex);
                return Task.CompletedTask;
            }
        };

        await _channel.Writer.WriteAsync(wrapper, _cancelToken.Token);
        await tcs.Task;
    }

    public async Task<T> Get<T>(Func<Task<T>> readOperation, ScopeContext context)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        context = context.With(_logger);
        context.LogDebug("Enqueuing reader (get)");

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<Task> wrapper = async () =>
        {
            try
            {
                context.LogDebug("Processing ReadOperation");
                using (var timeScope = context.LogDuration(nameof(Get), "ReadOperation - timing"))
                {
                    var result = await readOperation();
                    tcs.SetResult(result);
                    Interlocked.Increment(ref _getExecuteCount);
                }

                LogStats(context);
                context.LogDebug("Get readOperation completed");
            }
            catch (Exception ex)
            {
                context.Location().LogError(ex, "Error processing enqueued reader");
                tcs.SetException(ex);
            }
        };

        await _channel.Writer.WriteAsync(wrapper, _cancelToken.Token);
        Interlocked.Increment(ref _getEnqueueCount);
        LogStats(context);

        return await tcs.Task;
    }

    public async Task Send(Func<Task> sendOperation, ScopeContext context)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        context.With(_logger).LogDebug("Enqueue writer (send)");

        Interlocked.Increment(ref _sendEnqueueCount);
        await _channel.Writer.WriteAsync(sendOperation, _cancelToken.Token);
        LogStats(context);
    }

    private async Task ProcessQueueAsync(ScopeContext context)
    {
        context.With(_logger).LogDebug("Starting background processing of channel");

        try
        {
            while (await _channel.Reader.WaitToReadAsync(_cancelToken.Token))
            {
                while (_channel.Reader.TryRead(out var operation))
                {
                    try
                    {
                        context.Location().LogDebug("OperationQueue: Process operations");
                        using (var timeScope = context.LogDuration(nameof(ProcessQueueAsync), "OperationQueue: Process operations - timing"))
                        {
                            Interlocked.Increment(ref _processCount);
                            await operation();
                            LogStats(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        context.LogError(ex, "Process operations failed");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            context.LogDebug("Process operations canceled - shutting down");
        }
    }

    public async ValueTask DisposeAsync() => await Complete(_logger.ToScopeContext());

    private void LogStats(ScopeContext context) => context.LogDebug(
        "OperationQueue Stats: GetEnqueue={getEnqueue}, GetExecute={getExecute}, SendEnqueue={sendEnqueue}, ProcessCount={_processCount}",
        _getEnqueueCount,
        _getExecuteCount,
        _sendEnqueueCount,
        _processCount
        );
}
