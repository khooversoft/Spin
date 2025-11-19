using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class BatchStreamTests
{
    [Fact]
    public async Task SingleStreamScopeTerminated()
    {
        var queue = new ConcurrentQueue<(IReadOnlyList<int> list, string date)>();

        await using (var block = new BatchStream<int>(TimeSpan.FromMilliseconds(100), 1000, x =>
        {
            queue.Enqueue((x, DateTime.Now.ToString("mm:ss:fff")));
            return Task.CompletedTask;
        }, NullLogger<BatchStream<int>>.Instance))
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            int count = 0;
            while (!tokenSource.IsCancellationRequested)
            {
                await block.Send(count++);
            }
        }

        queue.Count.Assert(x => x > 1, x => $"{x} less then 100");

        var compare = queue
            .SelectMany(x => x.list)
            .Select((x, i) => (index: i, value: x))
            .Select(x => (x.index, x.value, test: x.index == x.value))
            .SkipWhile(x => x.test)
            .ToArray()
            .Length.Be(0);
    }

    [Fact]
    public async Task SingleBatch()
    {
        var queue = new ConcurrentQueue<(IReadOnlyList<int> list, string date)>();

        await using var block = new BatchStream<int>(TimeSpan.FromMilliseconds(100), 1000, x =>
        {
            queue.Enqueue((x, DateTime.Now.ToString("mm:ss:fff")));
            return Task.CompletedTask;
        }, NullLogger<BatchStream<int>>.Instance);

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        int count = 0;
        while (!tokenSource.IsCancellationRequested)
        {
            await block.Send(count++);
        }

        await block.Stop();

        queue.Count.Assert(x => x > 1, x => $"{x} less then 100");

        var compare = queue
            .SelectMany(x => x.list).Select((x, i) => (index: i, value: x))
            .Select(x => (x.index, x.value, test: x.index == x.value))
            .SkipWhile(x => x.test)
            .ToArray()
            .Length.Be(0);
    }

    [Fact]
    public async Task TwoSenders()
    {
        // Collect forwarded batches (no time data needed).
        var queue = new ConcurrentQueue<IReadOnlyList<(int group, int count)>>();

        const int itemsPerGroup = 500;
        const int maxBatchSize = 64;

        await using var block = new BatchStream<(int, int)>(TimeSpan.FromMilliseconds(25), maxBatchSize, batch =>
        {
            queue.Enqueue(batch);
            return Task.CompletedTask;
        }, NullLogger<BatchStream<(int, int)>>.Instance);

        // Two concurrent producers sending deterministic sequences.
        Task Producer(int group) => Task.Run(async () =>
        {
            for (int i = 0; i < itemsPerGroup; i++)
            {
                await block.Send((group, i));
            }
        });

        var t1 = Producer(0);
        var t2 = Producer(1);

        await Task.WhenAll(t1, t2);

        // Ensure all remaining items flushed.
        await block.Drain();
        await block.Stop();

        queue.Count.Assert(x => x > 0, "No batches captured");

        var all = queue.SelectMany(x => x).ToList();

        // Validate each group has exact count and contiguous ascending sequence.
        foreach (var grp in all.GroupBy(x => x.group))
        {
            grp.Count().Be(itemsPerGroup);

            grp.OrderBy(x => x.count)
               .Select((x, i) => (expected: i, actual: x.count))
               .SkipWhile(p => p.expected == p.actual)
               .ToArray()
               .Length.Be(0);
        }
    }

    [Fact]
    public async Task DrainAndContinue()
    {
        var queue = new ConcurrentQueue<IReadOnlyList<int>>();

        await using var block = new BatchStream<int>(TimeSpan.FromMilliseconds(100), 1000, x =>
        {
            queue.Enqueue(x);
            return Task.CompletedTask;
        }, NullLogger<BatchStream<int>>.Instance);

        int count = -1;
        await sendWorker();
        await block.Drain();

        int count1 = queue.Count.Assert(x => x > 0);

        var r1 = queue
            .SelectMany(x => x)
            .Select((x, i) => (index: i, value: x, test: x == i))
            .SkipWhile(x => x.test)
            .ToArray()
            .Length.Be(0);

        await sendWorker();
        await block.Drain();
        await block.Stop();

        (queue.Count - count1).Assert(x => x > 1, x => $"{x} range");

        var r2 = queue
            .SelectMany(x => x)
            .Select((x, i) => (index: i, value: x, test: i == x))
            .SkipWhile(x => x.test)
            .ToArray()
            .Length.Be(0);

        async Task sendWorker()
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            while (!tokenSource.IsCancellationRequested)
            {
                await block.Send(Interlocked.Increment(ref count));
            }
        }
    }

    [Fact]
    public async Task ForwardDelegate_ThrowsException_LogsAndContinues()
    {
        var callCount = 0;
        var exception = new InvalidOperationException("Test error");

        await using var block = new BatchStream<int>(
            TimeSpan.FromMilliseconds(50),
            10,
            batch =>
            {
                callCount++;
                throw exception;
            },
            NullLogger<BatchStream<int>>.Instance);

        for (int i = 0; i < 15; i++)
        {
            await block.Send(i);
        }

        await block.Drain();
        callCount.Assert(x => x >= 1, "Should attempt forwarding despite errors");
    }

    [Fact]
    public async Task MaxBatchSize_EnforcedCorrectly()
    {
        const int maxBatchSize = 10;
        var batches = new ConcurrentQueue<int>();

        await using var block = new BatchStream<int>(
            TimeSpan.FromSeconds(10), // Long interval to force size-based batching
            maxBatchSize,
            batch =>
            {
                batches.Enqueue(batch.Count);
                return Task.CompletedTask;
            },
            NullLogger<BatchStream<int>>.Instance);

        // Send exactly 2.5x maxBatchSize
        for (int i = 0; i < 25; i++)
        {
            await block.Send(i);
        }

        await block.Drain();

        batches.All(count => count <= maxBatchSize).Be(true);
    }

    [Fact]
    public async Task TimerBasedBatching_LowVolumeScenario()
    {
        var batches = new ConcurrentQueue<IReadOnlyList<int>>();

        await using var block = new BatchStream<int>(
            TimeSpan.FromMilliseconds(100),
            1000,
            batch =>
            {
                batches.Enqueue(batch);
                return Task.CompletedTask;
            },
            NullLogger<BatchStream<int>>.Instance);

        await block.Send(1);
        await Task.Delay(150); // Wait for timer tick
        await block.Send(2);
        await Task.Delay(150);

        await block.Drain();

        batches.Count.Assert(x => x >= 2, "Should have time-based batches");
    }

    [Fact]
    public async Task DrainOnEmptyQueue_Completes()
    {
        await using var block = new BatchStream<int>(
            TimeSpan.FromMilliseconds(100),
            10,
            _ => Task.CompletedTask,
            NullLogger<BatchStream<int>>.Instance);

        await block.Drain(); // Should complete without error
    }
}
