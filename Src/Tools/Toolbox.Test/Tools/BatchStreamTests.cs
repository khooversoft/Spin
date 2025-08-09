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
        var queue = new ConcurrentQueue<(IReadOnlyList<(int group, int count)> list, string date)>();

        await using var block = new BatchStream<(int, int)>(TimeSpan.FromMilliseconds(100), 1000, x =>
        {
            queue.Enqueue((x, DateTime.Now.ToString("mm:ss:fff")));
            return Task.CompletedTask;
        }, NullLogger<BatchStream<(int, int)>>.Instance);

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var t1 = Task.Run(async () => await sendWorker(0));
        var t2 = Task.Run(async () => await sendWorker(1));

        await block.Stop();
        await Task.WhenAll(t1, t2);

        queue.Count.Assert(x => x > 1, x => $"{x} less then 100");

        var baseSet = queue
            .SelectMany(x => x.list)
            .GroupBy(x => x.group)
            .Select(x => x.Select((y, i) => (index: i, value: y.count, test: i == y.count)).Action(z =>
            {
                z.SkipWhile(y => y.test).ToArray().Length.Be(0);
            }))
            .ToArray();

        async Task sendWorker(int group)
        {
            int count = 0;
            while (!tokenSource.IsCancellationRequested)
            {
                await block.Send((group, count++));
            }
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
}
