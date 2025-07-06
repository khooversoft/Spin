using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Types;

public class BatchProcessingTests
{
    private readonly ITestOutputHelper _outputHelper;
    private ScopeContext _context;
    private ILogger<BatchProcessing<int>> _logger;

    public BatchProcessingTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper.NotNull();

        var host = TestApplication.CreateServiceProvider(_outputHelper);
        _context = host.CreateScopeContext<BatchProcessingTests>();
        _logger = host.CreateLogger<BatchProcessing<int>>();
    }

    [Fact]
    public async Task SimpleNoFlush()
    {
        int count = 100;
        var data = new ConcurrentQueue<int>();

        var queue = new BatchProcessing<int>(count * 2, 1000, TimeSpan.FromHours(1), x =>
        {
            x.ForEach(x => data.Enqueue(x));
            return Task.CompletedTask;
        }, _logger);

        await Enumerable.Range(0, count).ForEachAsync(async x => await queue.Enqueue(x, _context));
        await queue.Complete(_context);

        data.Count.Be(count);
    }

    [Fact]
    public async Task SteadyFlowNoFlush()
    {
        int count = 100;
        int batchCount = 0;
        var data = new ConcurrentQueue<(int Value, int BatchIndex)>();

        var queue = new BatchProcessing<int>(count * 2, 1000, TimeSpan.FromMilliseconds(100), x =>
        {
            int currentCount = Interlocked.Increment(ref batchCount);
            x.ForEach(x => data.Enqueue((x, currentCount)));
            return Task.CompletedTask;
        }, _logger);

        foreach (var n in Enumerable.Range(0, count))
        {
            await queue.Enqueue(n, _context);
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        await queue.Complete(_context);

        data.Count.Be(count);
        data.GroupBy(x => x.BatchIndex).Count().Assert(x => x == 1, "Flush grouping should be 1");
        batchCount.Assert(x => x == 1, "Flush index should be 1");
    }

    [Fact]
    public async Task SingleFlush()
    {
        int count = 100;
        int batchCount = 0;
        var data = new ConcurrentQueue<(int Value, int BatchIndex)>();
        var dataEvent = new AutoResetEvent(false);

        var queue = new BatchProcessing<int>(count * 2, 1000, TimeSpan.FromMilliseconds(100), x =>
        {
            int currentCount = Interlocked.Increment(ref batchCount);
            x.ForEach(x => data.Enqueue((x, currentCount)));
            dataEvent.Set(); // Signal that data has been processed
            return Task.CompletedTask;
        }, _logger);

        foreach (var n in Enumerable.Range(0, count))
        {
            await queue.Enqueue(n, _context);
        }

        dataEvent.WaitOne(TimeSpan.FromSeconds(100)).BeTrue();  // Wait for the data to be processed
        data.Count.Be(count);

        foreach (var n in Enumerable.Range(0, count))
        {
            await queue.Enqueue(n + (count * 2), _context);
        }

        dataEvent.WaitOne(TimeSpan.FromSeconds(100)).BeTrue();  // Wait for the data to be processed
        data.Count.Be(count * 2);

        await queue.Complete(_context);

        data.Count.Be(count * 2);
        data.GroupBy(x => x.BatchIndex).Count().Assert(x => x > 1, "Flush grouping should be greater than 1");
        batchCount.Assert(x => x > 1, "Flush index should be greater than 1");
    }

    [Fact]
    public async Task StressTest()
    {
        ConcurrentQueue<int> _queue = new();
        int batchCount = 0;
        var data = new ConcurrentQueue<(int Value, int BatchIndex)>();

        var queue = new BatchProcessing<int>(100, 1000, TimeSpan.FromMilliseconds(100), x =>
        {
            int currentCount = Interlocked.Increment(ref batchCount);
            x.ForEach(x => data.Enqueue((x, currentCount)));
            return Task.CompletedTask;
        }, _logger);

        int index = 0;
        var token = new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token;
        while (!token.IsCancellationRequested)
        {
            await queue.Enqueue(index++, _context);
        }

        await queue.Complete(_context);

        data.Count.Be(index);
        data.GroupBy(x => x.BatchIndex).Count().Assert(x => x > 1, "Flush grouping should be greater than 1");
        batchCount.Assert(x => x > 1, "Flush index should be greater than 1");
    }
}
