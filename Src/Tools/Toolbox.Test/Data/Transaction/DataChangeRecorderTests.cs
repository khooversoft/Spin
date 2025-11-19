using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Transaction;

public class DataChangeRecorderTests
{
    [Fact]
    public void NoRecorderSet()
    {
        var r = new DataChangeRecorder();
        r.GetRecorder().BeNull();
        r.Resume(); // idempotent no-op
        r.Pause();  // idempotent no-op
    }

    [Fact]
    public void SetRecorder_Success()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();

        r.Set(mockRecorder);
        r.GetRecorder().NotNull();
        r.GetRecorder().Assert(x => x == mockRecorder, "Recorder should be the same instance");
    }

    //[Fact]
    //public void SetRecorder_TwiceShouldThrow()
    //{
    //    var r = new DataChangeRecorder();
    //    var mockRecorder1 = new MockTrxRecorder();
    //    var mockRecorder2 = new MockTrxRecorder();

    //    r.Set(mockRecorder1);
    //    Verify.Throws<ArgumentException>(() => r.Set(mockRecorder2));
    //}

    [Fact]
    public void ClearRecorder_Success()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();

        r.Set(mockRecorder);
        r.GetRecorder().NotNull();

        r.Clear();
        r.GetRecorder().BeNull();
    }

    [Fact]
    public void PauseRecorder_Success()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();

        r.Set(mockRecorder);
        r.GetRecorder().NotNull();

        r.Pause();
        r.GetRecorder().BeNull();
        r.IsPaused.Assert(x => x, "Should be paused");
    }

    [Fact]
    public void ResumeRecorder_Success()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();

        r.Set(mockRecorder);
        r.Pause();
        r.GetRecorder().BeNull();

        r.Resume();
        r.GetRecorder().NotNull();
        r.GetRecorder().Assert(x => x == mockRecorder, "Recorder should be restored after resume");
        r.IsOn.Assert(x => x, "Should be on");
    }

    [Fact]
    public void ClearNullRecorder_Success()
    {
        var r = new DataChangeRecorder();

        r.Clear();
        r.GetRecorder().BeNull();
    }

    [Fact]
    public void PauseAndResumeMultipleTimes_Idempotent()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();

        r.Set(mockRecorder);

        r.Pause();
        r.Pause(); // idempotent second call
        r.GetRecorder().BeNull();

        r.Resume();
        r.Resume(); // idempotent second call
        r.GetRecorder().NotNull();

        r.Pause();
        r.Resume();
        r.GetRecorder().NotNull();
    }

    // -------------------------
    // Stress / Concurrency Tests
    // -------------------------

    [Fact]
    public async Task ConcurrentPauseResume_GetRecorderConsistency()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();
        r.Set(mockRecorder);

        int nonNullCount = 0, nullCount = 0;
        int iterations = 1000;

        var block = new ActionBlock<Func<Task>>(async func => await func(),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                BoundedCapacity = Environment.ProcessorCount * 4
            });

        for (int iter = 0; iter < iterations; iter++)
        {
            await block.SendAsync(async () =>
            {
                r.Pause();
                r.Resume();
                Thread.SpinWait(64);
                await Task.Delay(RandomNumberGenerator.GetInt32(1, 5));
            });

            await block.SendAsync(async () =>
            {
                var current = r.GetRecorder();
                if (current is null)
                {
                    Interlocked.Increment(ref nullCount);
                }
                else
                {
                    // If present, it must be the same instance we Set
                    current.Assert(x => ReferenceEquals(x, mockRecorder), "Unexpected recorder instance observed");
                    Interlocked.Increment(ref nonNullCount);
                }
                Thread.SpinWait(32);
            });
        }

        block.Complete();
        await block.Completion;

        // We should have observed both null and non-null states
        nonNullCount.Assert(x => x > 0, "Should observe recorder available while running");
        nullCount.Assert(x => x > 0, "Should observe recorder null while paused");
    }

    [Fact]
    public async Task ConcurrentPauseResumeThenClear_RecorderStaysNullAfterClear()
    {
        var r = new DataChangeRecorder();
        var mockRecorder = new MockTrxRecorder();
        r.Set(mockRecorder);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var start = new ManualResetEventSlim(false);

        int afterClearNonNull = 0;
        bool cleared = false;

        var toggler = Task.Run(() =>
        {
            start.Wait();
            while (!cts.IsCancellationRequested)
            {
                r.Pause();
                r.Resume();

                // Small yield prevents monopolizing the pool under debugger
                Thread.SpinWait(64);
                Thread.Sleep(0); // yield to ready threads
            }
        }, cts.Token);

        var reader = Task.Run(() =>
        {
            start.Wait();
            while (!cts.IsCancellationRequested)
            {
                var current = r.GetRecorder();

                if (cleared)
                {
                    if (current != null) Interlocked.Increment(ref afterClearNonNull);
                }

                Thread.SpinWait(32);
                Thread.Sleep(0);
            }
        }, cts.Token);

        start.Set();

        // Let it run a bit with toggling
        await Task.Delay(150, cts.Token);

        // Now clear and mark the phase
        r.Clear();
        cleared = true;

        // Continue running a bit to sample after-clear
        await Task.Delay(150, cts.Token);

        cts.Cancel();
        try { await Task.WhenAll(toggler, reader); } catch { /* ignore cancellation exceptions */ }

        Assert.Equal(0, afterClearNonNull);
        r.GetRecorder().BeNull();
    }

    [Fact]
    public async Task Stress_RaceToSet_ExactlyOneWins()
    {
        var r = new DataChangeRecorder();

        int contenders = Math.Max(4, Environment.ProcessorCount);
        var recorders = Enumerable.Range(0, contenders).Select(_ => new MockTrxRecorder()).ToArray();

        var start = new ManualResetEventSlim(false);
        int success = 0;
        var tasks = new List<Task>();

        for (int i = 0; i < contenders; i++)
        {
            int idx = i;
            tasks.Add(Task.Run(() =>
            {
                start.Wait();
                try
                {
                    r.Set(recorders[idx]);
                    Interlocked.Increment(ref success);
                }
                catch (ArgumentException)
                {
                    // Expected for all but one
                }
            }));
        }

        start.Set();
        await Task.WhenAll(tasks);

        Assert.Equal(1, success);
        var final = r.GetRecorder();
        final.NotNull();
        // Must be one of the contenders
        Assert.Contains(recorders, x => ReferenceEquals(x, final));
    }

    private class MockTrxRecorder : ITrxRecorder
    {
        public void Add<K, T>(K objectId, T newValue) where K : notnull where T : notnull { }
        public void Delete<K, T>(K objectId, T currentValue) where K : notnull where T : notnull { }
        public void Update<K, T>(K objectId, T currentValue, T newValue) where K : notnull where T : notnull { }
    }
}
