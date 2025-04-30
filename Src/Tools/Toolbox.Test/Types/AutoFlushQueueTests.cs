using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class AutoFlushQueueTests
{
    [Fact]
    public async Task SimpleNoFlush()
    {
        int count = 100;
        var data = new ConcurrentQueue<int>();

        var queue = new AutoFlushQueue<int>(count * 2, TimeSpan.FromHours(1), x =>
        {
            x.ForEach(x => data.Enqueue(x));
            return Task.CompletedTask;
        });

        await Enumerable.Range(0, count).ForEachAsync(async x => await queue.Enqueue(x, default));
        await queue.FlushBuffer();
        await queue.Complete();

        data.Count.Be(count);
    }

    [Fact]
    public async Task SingleFlush()
    {
        int count = 100;
        int flushCount = 0;
        var data = new ConcurrentQueue<(int Value, int FlushIndex)>();

        var queue = new AutoFlushQueue<int>(count * 2, TimeSpan.FromMilliseconds(100), x =>
        {
            int currentCount = Interlocked.Increment(ref flushCount);
            x.ForEach(x => data.Enqueue((x, currentCount)));
            return Task.CompletedTask;
        });

        foreach (var n in Enumerable.Range(0, count))
        {
            await queue.Enqueue(n, default);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }

        await queue.FlushBuffer();
        await queue.Complete();

        data.Count.Be(count);
        data.GroupBy(x => x.FlushIndex).Count().Assert(x => x > 1, "Flush grouping should be greater than 1");
        flushCount.Assert(x => x > 1, "Flush index should be greater than 1");
    }
}
