using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public class BatchProcessing<T> : IAsyncDisposable
{
    private readonly Channel<T> _channel;
    private readonly ScopeContext _context;
    private readonly ActionBlock<IReadOnlyList<T>> _writer;
    private readonly Task _timerTask;
    private CancellationTokenSource? _cancellationTokenSource = new();

    private enum RunState { None, Run, Stopped }
    private RunState _runState;

    public BatchProcessing(int batchSize, int boundedCapacity, TimeSpan flushInterval, Func<IReadOnlyList<T>, Task> writeAction, ILogger logger)
    {
        batchSize.Assert(x => x > 1, _ => "Batch size must be greater than 1");
        boundedCapacity.Assert(x => x > 1 && x >= batchSize, _ => "Batch size must be greater than 1");

        _context = logger.NotNull().ToScopeContext();

        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(boundedCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        _writer = new ActionBlock<IReadOnlyList<T>>(writeAction);

        var timerContext = new ScopeContext(logger, _cancellationTokenSource.Token);
        _timerTask = ProcessBatch(_channel.Reader, batchSize, flushInterval, timerContext);
        _runState = RunState.Run;
    }

    public async Task Enqueue(IEnumerable<T> items, ScopeContext context)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        await items.ForEachAsync(async x => await Enqueue(x, context));
    }

    public ValueTask Enqueue(T item, ScopeContext context)
    {
        if (_runState != RunState.Run) throw new InvalidOperationException("Not running");
        return _channel.Writer.WriteAsync(item, context.CancellationToken);
    }

    public async Task Complete(ScopeContext context)
    {
        var currentState = Interlocked.CompareExchange(ref _runState, RunState.Stopped, RunState.Run);
        if (currentState == RunState.Stopped) return;

        context.LogDebug("Completing batch processing");
        _channel.Writer.Complete();
        await _channel.Reader.Completion;

        _cancellationTokenSource?.Cancel();
        await _timerTask;

        _writer.Complete();
        await _writer.Completion;

        Interlocked.Exchange(ref _cancellationTokenSource!, null)?.Dispose();
    }

    public async ValueTask DisposeAsync() => await Complete(_context);


    private Task ProcessBatch(ChannelReader<T> reader, int maxBatchSize, TimeSpan flushInterval, ScopeContext context)
    {
        var tcs = new TaskCompletionSource();

        _ = Task.Run(() => ProcessChannel(reader, maxBatchSize, flushInterval, context).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                context.LogError(task.Exception, "Error processing channel");
            }
            tcs.SetResult();
        }, TaskScheduler.Default), context.CancellationToken);

        return tcs.Task;
    }

    private async Task ProcessChannel(ChannelReader<T> reader, int maxBatchSize, TimeSpan flushInterval, ScopeContext context)
    {
        var batch = new List<T>();
        var flushTimer = Task.Delay(flushInterval, context.CancellationToken);
        context.LogDebug("Starting batch processing with maxBatchSize={maxBatchSize}, flushInterval={flushInterval}", maxBatchSize, flushInterval);

        try
        {
            while (true)
            {
                var readTask = reader.WaitToReadAsync(context.CancellationToken).AsTask();
                var completedTask = await Task.WhenAny(readTask, flushTimer);

                if (completedTask == flushTimer)
                {
                    await _processChannel(reader, batch, true);

                    flushTimer = Task.Delay(flushInterval, context.CancellationToken); // Reset the timer
                    continue;
                }

                if (await readTask)
                {
                    // Process items from the channel
                    await _processChannel(reader, batch, false);
                    flushTimer = Task.Delay(flushInterval, context.CancellationToken); // Reset the timer
                }
            }
        }
        catch (OperationCanceledException)
        {
            context.LogDebug("Channel reader completed, processing remaining items");
            await _processChannel(reader, batch, true);
            return;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Error during batch processing");
            return;
        }

        async Task _processChannel(ChannelReader<T> reader, List<T> batch, bool force)
        {
            while (reader.TryRead(out var item))
            {
                batch.Add(item);
                if (batch.Count >= maxBatchSize) await _processBatch(batch);
            }

            if (force && batch.Count > 0) await _processBatch(batch);
        }

        async Task _processBatch(List<T> batch)
        {
            context.LogDebug("Sending batch to receiver");
            await _writer.SendAsync(batch.ToImmutableArray());
            batch.Clear();
        }
    }
}
