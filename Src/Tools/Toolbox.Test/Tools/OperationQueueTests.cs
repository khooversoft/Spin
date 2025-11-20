using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Tools;

public class OperationQueueTests
{
    private ITestOutputHelper _testOutputHelper;
    private IHost _host;
    private ScopeContext _context;

    public OperationQueueTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_testOutputHelper.WriteLine).AddDebug().AddFilter(x => true));
            })
            .Build();

        _context = _host.Services.GetRequiredService<ILogger<OperationQueueTests>>().ToScopeContext();
    }

    [Fact]
    public async Task SimpleSendOperation()
    {
        int count = 100;
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 100);

        foreach (var x in Enumerable.Range(0, count))
        {
            await operationQueue.Send(() =>
            {
                queue.Enqueue(x);
                return Task.CompletedTask;
            }, _context);
        }

        await operationQueue.Complete(_context);
        queue.Count.Be(count);
        queue.All(x => x >= 0 && x < count).BeTrue();
    }

    [Fact]
    public async Task SimpleSendWithGetOperation()
    {
        int count = 100;
        int[] setIndexes = [0, 10, 55, 85, 99];
        int[][] lookFor = [[-100, 0], [9, -10, 10], [54, -55, 55], [84, -85, 85], [98, -99, 99]];
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 100);

        await Enumerable.Range(0, count).ForEachAsync(async x =>
        {
            if (setIndexes.Contains(x))
            {
                var result = await operationQueue.Get(() =>
                {
                    var v = x switch { 0 => -100, _ => -x };
                    queue.Enqueue(v);
                    return Task.FromResult(x);
                }, _context);
            }

            await operationQueue.Send(() =>
            {
                queue.Enqueue(x);
                return Task.CompletedTask;
            }, _context);
        });

        await operationQueue.Complete(_context);
        queue.Count.Be(count + setIndexes.Length);
        queue.All(x => x >= 0 && x < count || x == -100 || setIndexes.Contains(Math.Abs(x))).BeTrue();

        var searchData = queue.ToArray();
        foreach (var look in lookFor)
        {
            searchData.AsSpan().IndexOf(look).Assert(x => x >= 0, "Sub not found");
        }
    }

    [Fact]
    public async Task AsyncWriteOperation()
    {
        int count = 100;
        int[] setIndexes = [0, 10, 55, 85, 99];
        int[][] lookFor = [[-100, 0], [9, -10, 10], [54, -55, 55], [84, -85, 85], [98, -99, 99]];
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 100);

        await Enumerable.Range(0, count).ForEachAsync(async x =>
        {
            if (setIndexes.Contains(x))
            {
                var result = await operationQueue.Get(async () =>
                {
                    var v = x switch { 0 => -100, _ => -x };
                    int result = await deferred(v);
                    result.Be(v);
                    return v;
                }, _context);
            }

            await operationQueue.Send(async () =>
            {
                await deferred(x);
            }, _context);
        });

        await operationQueue.Complete(_context);
        queue.Count.Be(count + setIndexes.Length);
        queue.All(x => x >= 0 && x < count || x == -100 || setIndexes.Contains(Math.Abs(x))).BeTrue();

        var searchData = queue.ToArray();
        foreach (var look in lookFor)
        {
            searchData.AsSpan().IndexOf(look).Assert(x => x >= 0, "Sub not found");
        }

        async Task<int> deferred(int value)
        {
            await Task.Delay(10);
            queue.Enqueue(value);
            return value;
        }
    }

    // Drain should flush all work enqueued before it returns.
    [Fact]
    public async Task DrainFlushesPendingOperations()
    {
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 10);

        foreach (var x in Enumerable.Range(0, 50))
        {
            await operationQueue.Send(() =>
            {
                queue.Enqueue(x);
                return Task.CompletedTask;
            }, _context);
        }

        await operationQueue.Drain(_context);
        queue.Count.Be(50);

        foreach (var x in Enumerable.Range(50, 50))
        {
            await operationQueue.Send(() =>
            {
                queue.Enqueue(x);
                return Task.CompletedTask;
            }, _context);
        }

        await operationQueue.Drain(_context);
        queue.Count.Be(100);

        await operationQueue.Complete(_context);
    }

    // After Complete -> Send/Get/Drain throw; Complete can be called multiple times.
    [Fact]
    public async Task SendGetAndDrainThrowWhenNotRunningAndCompleteIsIdempotent()
    {
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 10);

        await operationQueue.Complete(_context);
        await operationQueue.Complete(_context); // idempotent

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            operationQueue.Send(() => Task.CompletedTask, _context));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            operationQueue.Get(() => Task.FromResult(42), _context));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            operationQueue.Drain(_context));
    }

    // Exception in Send is swallowed by the queue; subsequent work continues.
    [Fact]
    public async Task SendExceptionDoesNotStopProcessing()
    {
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 10);

        await operationQueue.Send(() =>
        {
            queue.Enqueue(1);
            return Task.CompletedTask;
        }, _context);

        await operationQueue.Send(() => throw new ApplicationException("boom"), _context);

        await operationQueue.Send(() =>
        {
            queue.Enqueue(2);
            return Task.CompletedTask;
        }, _context);

        await operationQueue.Complete(_context);

        queue.Count.Be(2);
        queue.Contains(1).BeTrue();
        queue.Contains(2).BeTrue();
    }

    // Exception in Get propagates, but queue keeps running for subsequent operations.
    [Fact]
    public async Task GetExceptionPropagatesAndDoesNotStopProcessing()
    {
        var queue = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 10);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            operationQueue.Get<int>(() => throw new InvalidOperationException("bad read"), _context));

        await operationQueue.Send(() =>
        {
            queue.Enqueue(999);
            return Task.CompletedTask;
        }, _context);

        await operationQueue.Complete(_context);

        queue.Contains(999).BeTrue();
        queue.Count.Be(1);
    }

    // Small capacity exercises back-pressure; all items should be processed.
    [Fact]
    public async Task BackpressureWithSmallCapacityProcessesAll()
    {
        int total = 10_000;
        int processed = 0;

        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 4);

        await Enumerable.Range(0, total).ForEachAsync(async _ =>
        {
            await operationQueue.Send(() =>
            {
                Interlocked.Increment(ref processed);
                return Task.CompletedTask;
            }, _context);
        });

        await operationQueue.Complete(_context);

        processed.Be(total);
    }

    [Fact]
    public async Task ReentrantSendFromWithinSendRunsLater()
    {
        // A Send invoked from inside another Send should enqueue and execute
        // only after the outer operation finishes. We enforce ordering:
        // 1 (outer start) < 3 (outer end) < 2 (nested send execution).
        // Fix: call Drain before Complete so the nested send is guaranteed to run
        // before the queue is shut down. Previously Complete could race with the
        // inner enqueue causing it to be rejected.

        var order = new List<int>();
        object gate = new();

        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 10);

        await operationQueue.Send(async () =>
        {
            lock (gate) order.Add(1);

            // Enqueue nested work (will run after outer finishes)
            await operationQueue.Send(() =>
            {
                lock (gate) order.Add(2);
                return Task.CompletedTask;
            }, _context);

            lock (gate) order.Add(3);
        }, _context);

        // Ensure all enqueued work (including the nested send) is processed before shutdown.
        await operationQueue.Drain(_context);
        await operationQueue.Complete(_context);

        order.Count.Be(3);
        order.Contains(1).BeTrue();
        order.Contains(2).BeTrue();
        order.Contains(3).BeTrue();

        int idx1 = order.IndexOf(1);
        int idx2 = order.IndexOf(2);
        int idx3 = order.IndexOf(3);

        (idx1 < idx3).BeTrue();
        (idx3 < idx2).BeTrue();
    }

    [Fact]
    public async Task MixedHighConcurrencyGetSendStress()
    {
        int total = 5_000;
        var bag = new ConcurrentBag<int>();

        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 32);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

        await Parallel.ForEachAsync(Enumerable.Range(0, total), parallelOptions, async (i, ct) =>
        {
            await operationQueue.Get(() =>
            {
                bag.Add(-i);
                return Task.FromResult(i);
            }, _context);

            await operationQueue.Send(() =>
            {
                bag.Add(i);
                return Task.CompletedTask;
            }, _context);
        });

        await operationQueue.Complete(_context);

        bag.Count.Be(total * 2);

        var arr = bag.ToArray();
        Enumerable.Range(0, total).All(i => arr.Contains(i) && arr.Contains(-i)).BeTrue();
    }

    [Fact]
    public async Task DrainOnlyFlushesWorkEnqueuedBeforeCallUnderLoad()
    {
        var processed = new ConcurrentQueue<int>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 8);

        int total = 2_000;

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= total; i++)
            {
                await operationQueue.Send(() =>
                {
                    processed.Enqueue(i);
                    return Task.CompletedTask;
                }, _context);
            }
        });

        await Task.Delay(10);

        await operationQueue.Drain(_context);
        int countAtDrain = processed.Count;

        await producer;
        await operationQueue.Complete(_context);

        int final = processed.Count;
        (countAtDrain > 0 && final >= countAtDrain && final == total).BeTrue();
    }

    [Fact]
    public async Task DisposeCallsCompleteAndDrains()
    {
        var q = new ConcurrentQueue<int>();

        await using (var operationQueue = ActivatorUtilities.CreateInstance<OperationQueue>(_host.Services, 16))
        {
            foreach (var i in Enumerable.Range(0, 500))
            {
                await operationQueue.Send(() =>
                {
                    q.Enqueue(i);
                    return Task.CompletedTask;
                }, _context);
            }
            // Leaving the scope calls DisposeAsync -> Complete
        }

        q.Count.Be(500);
    }
}
